local pairs = pairs
local require = require
local format = string.format
local match = string.match
local loaded = package.loaded


local Bit = Bit
local Time = Time
local Local = Local
local CfgMgr = CfgMgr
local ResMgr = ResMgr
local Vector3 = Vector3
local LogError = LogError
local LuaUtils = LuaUtils
local LuaHelper = LuaHelper
local GameObject = GameObject
local PrintTable = PrintTable
local PrintYellow = PrintYellow
local TernaryOperation = TernaryOperation
local ViewUtil = require "Common.ViewUtil"

local LOAD_ING = 1              --正在加载中
local LOAD_SUCC = 2             --加载成功
local LAYER_UI = 20             --UI Layer层
local MAX_HIDE_VIEW_NUM = 0
local UI_LOAD_TYPE = LuaFramework.ResourceLoadType.LoadBundleFromFile -- www方式加载

local NeedRefresh               --是否需要刷新
local OnShow                    --页面显示回调
local OnHide                    --页面隐藏回调
local ShowLoading               --显示加载中提示
local HideLoading               --隐藏加载中提示
local ShowLoadedView            --显示已经加载的页面 用于弹出堆栈 或者tab页切换
local HideLoadedView            --隐藏已经加载的页面 用于弹出堆栈 或者tab页切换
local CallBackDestroyAllDlgs   --销毁所有界面回调事件

--[[
    filename                -- 文件名[小写]
    status                  -- 加载状态[加载成功/加载中]
    loaded                  -- 是否已被加载
    gameObject              -- 界面游戏对象

    isDialog                -- 是否为窗口
    dialogViewName          -- 是带dlgdialog的窗口?

    tabs                    --
    initedTabs              -- tabName,true
    tabIndex                --
    tabGroupStates          -- tabIndex,map{tabName,isShow}

    hideTime                -- 最后隐藏时刻
    isShow                  -- 是否已显示
    needRefresh             -- 是否需要刷新
    refreshParams           -- 刷新界面时所带参数
--]]
local views = {}
local isLocked = false
local dialogStack = nil
local uiRoot = nil
local playingParticleSystems = {}

local UIManager = {}
local this = UIManager

UIShowType = {
    Default                 = 0, --默认策略显示
    ShowImmediate           = 1, --直接调用show
    Refresh                 = 2, --强制调用show
    RefreshAndShowImmediate = 3, -- bit.bor(UIShowType.ShowImmediate,UIShowType.Refresh)
    DestroyWhenHide         = 4, --Hide时释放资源
}


--显示已经加载的页面 用于弹出堆栈 或者tab页切换
ShowLoadedView = function(viewName)
    if Local.LogModuals.UIManager then
        PrintYellow("ShowLoadedView", viewName)
    end
    local data = this.GetViewData(viewName)
    local params = data.params
    if not data.isShow and this.HasScript(viewName) then
        if this.IsUIShowType(viewName, UIShowType.Refresh) then
            if (this.HasMethod(viewName, "ShowTab")) and not this.IsUIShowType(viewName, UIShowType.ShowImmediate) then
                this.Call(viewName, "ShowTab", params)
            else
                this.Show(viewName, params)
            end
        else
            data.gameObject:SetActive(true)
            data.isShow = true
        end
    end

    if data.isDialog then
        this.RefreshDlgdialog(viewName, data.tabIndex)
        data.dialogViewName = viewName
        local tabGroup = this.gettabgroup(viewName, data.tabIndex)
        if tabGroup then
            for tabName, isShow in pairs(data.tabGroupStates[data.tabIndex]) do
                if isShow then
                    local tabData = this.GetViewData(tabName)
                    tabData.dialogViewName = viewName
                    this.ShowLoadedView(tabName, tabData.params)
                end
            end
        end
    end
end
--隐藏已经加载的页面 用于弹出堆栈 或者tab页切换
HideLoadedView = function(viewName)
    if Local.LogModuals.UIManager then
        PrintYellow("HideLoadedView", viewName)
    end
    local data = this.GetViewData(viewName)
    if data.isShow and this.HasScript(viewName) then
        if this.IsUIShowType(viewName, UIShowType.Refresh) then
            if this.HasMethod(viewName, "HideTab") and not this.IsUIShowType(viewName, UIShowType.ShowImmediate) then
                this.Call(viewName, "HideTab")
            else
                this.Hide(viewName)
            end
        else
            data.hideTime = Time.time
            data.gameObject:SetActive(false)
            data.isShow = false
        end
    end

    if data.isDialog then
        local tabGroup = this.gettabgroup(viewName, data.tabIndex)
        if tabGroup then
            for tabName, isShow in pairs(data.tabGroupStates[data.tabIndex]) do
                if isShow then
                    this.HideLoadedView(tabName)
                end
            end
        end
    end
end



function UIManager.Init()
    uiRoot = FindObj("/Canvas/GuiCamera")
    if uiRoot and uiRoot.transform.parent then
        GameObject.DontDestroyOnLoad(uiRoot.transform.parent)
    end

    UpdateBeat:AddListener(this.Update)
    LateUpdateBeat:AddListener(this.LateUpdate)
    --status.AddStatusListener("UImgr", gameevent.evt_update, evtid_update)
    --status.AddStatusListener("UImgr", gameevent.evt_late_update, evtid_late_update)
    --status.AddStatusListener("UImgr", gameevent.evt_late_update2, evtid_late_update2)
    dialogStack = list:new()
end

function UIManager.Update()
    for viewName, info in pairs(views) do
        if info.isShow then
            if (this.HasMethod(viewName, "Update")) then
                this.Call(viewName, "Update")
            end
        end
    end
    for i = #playingParticleSystems, 1, -1 do
        local particle = playingParticleSystems[i].particle
        if not particle.isPlaying then
            local callback = playingParticleSystems[i].callback
            if callback then
                callback()
            end
            table.remove(playingParticleSystems, i)
        else
            playingParticleSystems[i].time = playingParticleSystems[i].time + Time.deltaTime
        end
    end
end

function UIManager.LateUpdate()
    for viewName, info in pairs(views) do
        if info.isShow then
            if (this.HasMethod(viewName, "LateUpdate")) then
                this.Call(viewName, "LateUpdate")
            end

        end

        if info.needRefresh and NeedRefresh(viewName) then
            if Local.LogModuals.UIManager then
                PrintYellow("LateUpdate Refresh", viewName)
                PrintTable(info.refreshparams)
            end
            info.needRefresh = false
            --PrintYellow("ViewName",viewName)
            this.Call(viewName, "Refresh", info.refreshparams)
        end

    end

    --late_updateCharacterInfoOnUI()
end

function UIManager.UnloadExpireView(now)
    local unshowViewNum = 0
    local toDestroyViewName
    local minHideTime = now
    for name, data in pairs(views) do
        if data ~= nil and data.status == LOAD_SUCC and not data.isShow and not this.IsPersistent(name) and not this.IsInStack(name) then
            unshowViewNum = unshowViewNum + 1
            if data.hideTime ~= nil and data.hideTime < minHideTime then
                toDestroyViewName = name
                minHideTime = data.hideTime
            end
        end
    end
    if toDestroyViewName and unshowViewNum > MAX_HIDE_VIEW_NUM then
        Destroy(toDestroyViewName)
    end
end
--未实现
function UIManager.SecondUpdate(now)
    this.UnloadExpireView(now)
    for viewName, info in pairs(views) do
        if info.isShow and this.HasMethod(viewName, "SecondUpdate") then
            this.Call(viewName, "SecondUpdate", now)
        end
    end
end

function UIManager.GetViewData(viewName)
    local data = views[viewName]
    if not data then
        data = {}
        local _, fileName = match(viewName, "^(.*)%.(.*)$")
        data.fileName = TernaryOperation(fileName, fileName, viewName)
        views[viewName] = data
    end
    return data
end

function UIManager.GetModuleName(viewName)
    return "UI." .. viewName
end

function UIManager.GetViewModule(viewName)
    local view = require(this.GetModuleName(viewName))
    if not view then
        LogError("View:%s script file not find!", viewName)
    end
    return view
end

function UIManager.HasScript(viewName)
    return LuaHelper.ScriptExists(format("UI.%s", viewName))
end

function UIManager.HasMethod(viewName, methodName)
    local view = this.GetViewModule(viewName)
    if not view then
        return false
    end
    local method = view[methodName]
    if not method then
        return false
    end
    return true
end

function UIManager.IsShow(viewName)
    local data = views[viewName]
    if data and data.isShow then
        return data.isShow
    end
    return false
end

function UIManager.HasLoaded(viewName)
    local viewData = this.GetViewData(viewName)
    return viewData.status == LOAD_SUCC
end

function UIManager.Call(viewName, methodName, params)
    local view = this.GetViewModule(viewName)
    if not view then
        return
    end
    local method = view[methodName]
    if not method then
        print(format("View. %s.%s not find.", viewName, methodName))
        return
    end
    local viewData = this.GetViewData(viewName)
    if viewData.status ~= LOAD_SUCC and methodName ~= "Show" and methodName ~= "ShowDialog" and methodName ~= "ShowTabByIndex" and methodName ~= "ShowTab" then
        LogError("uimanager. view:%s not loaded! can't call method:%s", viewName, methodName)
        return
    end
    local succ = LuaUtils.Myxpcall(method, params)
    if succ then
        return true
    else
        LogError("View.call  %s.%s fail.", viewName, methodName)
        return false
    end
end

function UIManager.CallWithReturn(viewName, methodName, params)
    local view = this.GetViewModule(viewName)
    if not view then
        return nil
    end
    local method = view[methodName]
    if not method then
        print(format("View. %s.%s not find.", viewName, methodName))
        return nil
    end
    return method(params)
end

function UIManager.GetUIShowType(viewName)
    local uishowtype = UIShowType.Default
    if (this.HasMethod(viewName, "UIShowType")) then
        uishowtype = this.CallWithReturn(viewName, "UIShowType")
    end
    return uishowtype
end

function UIManager.IsUIShowType(viewName, uiShowType)
    local viewUIShowType = this.GetUIShowType(viewName)
    return Bit.band(uiShowType, viewUIShowType) > 0
end

function UIManager.GetDlgsShow()
    local list = {}
    for name, data in pairs(views) do
        if data.isShow then
            table.insert(list, name)
        end
    end
    return list
end

function UIManager.IsPersistent(viewName)
    return Local.UIPersistentMap[viewName] == true
end

function UIManager.IsInStack(viewName)
    if not dialogStack:IsEmpty() then
        local it = dialogStack:CreateIterator()
        while not it:IsEnd() do
            if it:Cur().value == viewName then
                return true
            else
                local data = this.GetViewData(it:Cur().value)
                if data.initedTabs and data.initedTabs[viewName] then
                    return true
                end
            end
            it:MoveNext()
        end
    end
    return false
end

function UIManager.SetLock(lock)
    isLocked = lock
end

function UIManager.GetIsLock()
    return isLocked
end




------------------------------------------------------------------
---普通窗口操作方法
------------------------------------------------------------------
OnShow = function(viewName, params)
    if Local.LogModuals.UIManager then
        PrintYellow("OnShow", viewName)
    end
    local data = this.GetViewData(viewName)
    data.params = params
    if viewName ~= "dlgopenloading" then
        HideLoading()
    end
    if data.dialogViewName ~= nil and data.dialogViewName ~= this.CurrentDialogName() then
        PrintYellow("Dialog has been hide", "dialog name:", data.dialogViewName, "current dialog name:", this.CurrentDialogName())
        HideLoadedView(viewName)
    elseif this.HasScript(viewName) then
        this.Refresh(viewName, params)
        --if data.uifadein ~= nil then
        --    data.uifadein:Play(true)
        --end
    end

end
OnHide = function(viewName)
    if Local.LogModuals.UIManager then
        PrintYellow("OnHide", viewName)
    end
    local data = this.GetViewData(viewName)
    if this.Call(viewName, "Hide") then
        data.isShow = false
        data.gameObject:SetActive(false)
        data.dialogViewName = nil
        if this.IsUIShowType(viewName, UIShowType.DestroyWhenHide) then
            Destroy(viewName, "Destroy")
        end
    end
end
NeedRefresh = function(viewName)
    return this.IsShow(viewName) or this.IsInStack(viewName)
end
function UIManager.Show(viewName, params)
    if Local.LogModuals.UIManager then
        PrintYellow("Show", viewName)
        PrintTable(params)
    end
    local data = this.GetViewData(viewName)
    if data.isShow then
        return
    end
    if data.status ~= LOAD_SUCC then
        if not data.loaded then
            print(format("View:", viewName, "not loaded!"))
            data.loaded = LOAD_ING
            ShowLoading()--显示正在加载中提示界面
            ResMgr:AddTask(format("UI/%s.ui", data.fileName), function(assetObj)
                if assetObj == nil then
                    return
                end
                local viewObj = GameObject.Instantiate(assetObj)
                GameObject.DontDestroyOnLoad(viewObj)
                if not viewObj then
                    data.loaded = nil
                    LogError(format("View %s prefab load fail!", viewName))
                    --evt:trigger("load_end", {false, viewName})
                    return
                end
                data.status = LOAD_SUCC
                data.gameObject = viewObj
                data.fields = ViewUtil.ExportFields(viewObj)
                local trans = viewObj.transform
                trans.parent = uiRoot.transform
                trans.localPosition = Vector3.zero
                trans.localScale = Vector3.one
                viewObj.name = data.fileName
                viewObj.layer = LAYER_UI
                viewObj:SetActive(true)
                if this.Call(viewName, "Init", { viewName, viewObj, data.fields })
                        and this.Call(viewName, "Show", params) then
                    data.isShow = true
                    OnShow(viewName, params)
                else
                    viewObj:SetActive(false)
                end
            end, UI_LOAD_TYPE)
        end
        return
    end

    data.gameObject:SetActive(true)
    if this.Call(viewName, "Show", params) then
        data.isShow = true
        OnShow(viewName, params)
    else
        data.gameObject:SetActive(false)
    end
end

function UIManager.Refresh(viewName, params)
    if Local.LogModuals.UIManager then
        PrintYellow("Refresh", viewName)
    end
    if NeedRefresh(viewName) then
        local data = this.GetViewData(viewName)
        data.needRefresh = true
        data.refreshParams = params
    end
end

function UIManager.ShowOrRefresh(viewName, params)
    if this.IsShow(viewName) then
        this.Refresh(viewName, params)
    else
        this.Show(viewName, params)
    end
end

function UIManager.IfShowThenCall(viewName, methodName, params)
    if this.IsShow(viewName) then
        this.Call(viewName, methodName, params)
    end
end

function UIManager.Hide(viewName)
    if Local.LogModuals.UIManager then
        PrintYellow("Hide", viewName)
    end
    local data = this.GetViewData(viewName)
    data.hideTime = Time.time
    if not data.isShow then
        print(format("View:%s not show!", viewName))
        return
    end
    if data.uifadeout ~= nil then
        --local DlgHiding = require "UI.DlgHiding"
        --UIEventListenerHelper.SetPlayTweenFinish(data.uifadeout, function(uifadeout)
        --    DlgHiding.OnFadeOutEnd()
        --    OnHide(viewName)
        --end)
        --data.uifadeout:Play(true)
        --DlgHiding.OnFadeOutBegin()
    else
        OnHide(viewName)
    end
    --local NoviceGuideTrigger = require "noviceguide.noviceguide_trigger"
    --NoviceGuideTrigger.HideDialog(viewName)
end

function UIManager.HideImmediate(viewName)
    if Local.LogModuals.UIManager then
        PrintYellow("HideImmediate", viewName)
    end
    local data = this.GetViewData(viewName)
    data.hideTime = Time.time
    if not data.isShow then
        print(format("View:%s not show!", viewName))
        return
    end
    OnHide(viewName)
end

function UIManager.Destroy(viewName)
    local data = this.GetViewData(viewName)
    if data.isShow then
        this.HideImmediate(viewName)
    end
    this.Call(viewName, "Destroy")
    if data.fields then
        for k, _ in pairs(data.fields) do
            data.fields[k] = nil
        end
        data.fields = nil
    end
    views[viewName] = nil
    assert(loaded[this.GetModuleName(viewName)])
    loaded[this.GetModuleName(viewName)] = nil
    GameObject.Destroy(data.gameObject)
end

function UIManager.HideAll()
    for name, data in pairs(views) do
        this.Hide(name)
    end
end

function UIManager.RegistCallBackDestroyAllDlgs(callback)
    PrintTable(CallBackDestroyAllDlgs, "RegistCallBackDestroyAllDlgs")
    CallBackDestroyAllDlgs = callback
end

function UIManager.DestroyAllDlgs()
    local list = this.GetDlgsShow()
    local d = false
    for _, name in pairs(list) do
        d = false
        for _, persistentName in pairs(Local.UIPersistentList) do
            if name == persistentName then
                this.Hide(name)
                d = true
                break
            end
        end
    end
    dialogStack:Clear()
    if CallBackDestroyAllDlgs then
        CallBackDestroyAllDlgs()
        CallBackDestroyAllDlgs = nil
    end
end

function UIManager.OnLogout()
    this.HideAll()
end




------------------------------------------------------------------
---针对模块操作方法
------------------------------------------------------------------
function UIManager.ShowMaincityDlgs()
    if Local.LogModuals.UIManager then
        PrintYellow("ShowMaincityDlgs")
    end
    for _, dlg in pairs(Local.MaincityDlgList) do
        if not this.IsShow(dlg) then
            this.Show(dlg)
        end
    end
end

function UIManager.HideMainCityDlgs()
    if Local.LogModuals.UIManager then
        PrintYellow("HideMainCityDlgs")
    end
    for _, dlg in pairs(Local.MaincityDlgList) do
        if this.IsShow(dlg) then
            this.Hide(dlg)
        end
    end
end

function UIManager.BeforeShowDialog()
    if this.IsShow("dlgjoystick") then
        --Game.JoyStickManager.singleton:Reset()
        PrintYellow("---------------DlgJoystick 界面未完成")
    end
end

function UIManager.RefreshDlgdialog(dialogViewName, tabindex)
    local dialogData = this.GetViewData("dlgdialog")
    if dialogData ~= nil and dialogData.uifadeout ~= nil then
        dialogData.uifadeout:Stop()
    end
    this.ShowOrRefresh("dlgdialog", { viewName = dialogViewName, tabIndex = tabindex })
end

function UIManager.ShowDialog(viewName, params, tabIndex)
    if Local.LogModuals.UIManager then
        PrintYellow("ShowDialog", viewName)
        PrintTable(params)
    end
    -- 在showdialog之前隐藏其它窗口
    this.BeforeShowDialog()

    if tabIndex == nil then
        tabIndex = 1
    end
    local data = this.GetViewData(viewName)
    data.isDialog = true
    data.initedTabs = {} -- tab_name,true
    data.tabGroupStates = {}
    data.dialogViewName = viewName

    if not dialogStack:IsEmpty() then
        local lastViewName = dialogStack:Top()
        if lastViewName == viewName then
            return
        end
        HideLoadedView(lastViewName)
    else
        this.HideMainCityDlgs()
    end

    dialogStack:Push(viewName)
    if Local.LogModuals.UIManager then
        dialogStack:Print()
    end

    if this.HasScript(viewName) then
        ShowLoading()
        if (this.HasMethod(viewName, "ShowDialog")) and not this.IsUIShowType(viewName, UIShowType.ShowImmediate) then
            this.Call(viewName, "ShowDialog", params)
        else
            this.Show(viewName, params)
        end
    else
        OnShow(viewName, params)
    end

    this.ShowTabByIndex(viewName, tabIndex, params)
    this.RefreshDlgdialog(viewName, data.tabIndex)

end

function UIManager.Hidedialog(viewName, isImmediate)
    if Local.LogModuals.UIManager then
        PrintYellow("HideDialog", viewName)
    end
    if dialogStack:IsEmpty() then
        return
    else
        local currentViewName = dialogStack:Top()
        if currentViewName ~= viewName then
            return
        end
    end
    local data = this.GetViewData(viewName)
    local tabGroup = this.gettabgroup(viewName, data.tabIndex)
    if tabGroup then
        this.hidetabbyindex(viewName, data.tabIndex)
    end
    if this.HasScript(viewName) then
        if (this.HasMethod(viewName, "HideDialog")) then
            this.Call(viewName, "HideDialog")
        else
            this.Hide(viewName)
        end
    end

    dialogStack:Pop()

    if not dialogStack:IsEmpty() then
        local lastview_name = dialogStack:Top()
        ShowLoadedView(lastview_name)
    else
        this.ShowMaincityDlgs()
        if isImmediate and true == isImmediate then
            this.HideImmediate("dlgdialog")
        else
            this.Hide("dlgdialog")
        end
    end
    if Local.LogModuals.UIManager then
        dialogStack:Print()
    end
    --local NoviceGuideTrigger = require "noviceguide.noviceguide_trigger"
    --NoviceGuideTrigger.HideDialog(viewName)
end

function UIManager.CurrentDialogName()
    if dialogStack:IsEmpty() then
        return "-"
    else
        return dialogStack:Top()
    end
end

function UIManager.CloseAllDialog()

    if not dialogStack:IsEmpty() then
        local lastViewName = dialogStack:Top()
        if (this.HasMethod(lastViewName, "HideDialog")) then
            this.Call(lastViewName, "HideDialog")
        else
            this.Hide(lastViewName)
        end
        dialogStack:Clear()
        this.ShowMaincityDlgs()
        this.Hide("dlgdialog")
    end
end



------------------------------------------------------------------
---下面是公用弹窗的用法
------------------------------------------------------------------
ShowLoading = function(beginTime, endTime)
    if isLocked == true then
        return
    end
    if Local.LogModuals.UIManager then
        PrintYellow("ShowLoading")
    end
    local params = { beginTime = TernaryOperation(beginTime, beginTime, 0.5), endTime = TernaryOperation(endTime, endTime, 3) }
    if this.IsShow("dlgopenloading") then
        this.Refresh("dlgopenloading", params)
    else
        this.Show("dlgopenloading", params)
    end
end
HideLoading = function()
    if isLocked == true then
        return
    end
    if Local.LogModuals.UIManager then
        PrintYellow("hideloading")
    end
    this.Hide("dlgopenloading")
end
--local function ShowAlertDlg(params)
--    local InfoManager = require "assistant.infomanager"
--    if isLocked == true then
--        return InfoManager.AddInfo(params)
--    end
--    return InfoManager.ShowInfo(params)
--end




------------------------------------------------------------------
---UI粒子系统操作方法
------------------------------------------------------------------
-- 播放粒子系统及子节点中粒子系统
function UIManager.PlayUIParticleSystem(go, callback)
    go:SetActive(false)
    go:SetActive(true)
    local particles = go:GetComponentsInChildren(UnityEngine.ParticleSystem)
    if particles and particles.Length ~= 0 and particles[1] then
        particles[1]:Stop(true)
        particles[1]:Play(true)
        if callback then
            table.insert(playingParticleSystems, { particle = particles[1], callback = callback, time = 0 })
        end
    end
end
-- 停止粒子系统及子节点中粒子系统
function UIManager.StopUIParticleSystem(go)
    local particle = go:GetComponent("ParticleSystem")
    if particle then
        particle:Stop(true)
    end
    go:SetActive(false)
end
-- 遍历子节点中粒子系统
function UIManager.IsPlaying(go)
    local particles = go:GetComponentsInChildren(UnityEngine.ParticleSystem)
    for i = 1, particles.Length do
        local particle = particles[i]
        if particle.isPlaying then
            return true
        end
    end
    return false
end
-- 遍历子节点中粒子系统
function UIManager.IsStopped(go)
    local particles = go:GetComponentsInChildren(UnityEngine.ParticleSystem)
    for i = 1, particles.Length do
        local particle = particles[i]
        if not particle.isStopped then
            return false
        end
    end
    return true
end


return UIManager