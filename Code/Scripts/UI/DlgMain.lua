local unpack    = unpack
local UIMgr     = require "Logic.UIManager"
local LuaHelper = LuaHelper
---@type UnityEngine.EventSystems.EventTriggerType
local EvtType   = UnityEngine.EventSystems.EventTriggerType

local name
local fields
local gameObject

local function Show(params)

end
local function Refresh(params)

end

local function Hide()

end
local function Destroy()

end
local function Init(params)
    name, gameObject, fields = unpack(params)
    LuaHelper.AddClick(fields.Button_UGUI, function()
        local ugui = {
            [0] = "UI System完善",
            [1] = "控件扩展与完善",
            [2] = "UI事件系统完善",
            [3] = "ttf,otf等字体精简",
            [4] = "艺术字制作工具及流程",
        }
        PrintTable(ugui, "UGUI 系统模块入口!")
    end)
    LuaHelper.AddClick(fields.Button_DOTween, function()
        local dotween = {
            [0] = "熟悉DOTween插件",
            [1] = "UI 动画制作与控制流",
        }
        PrintTable(dotween, "DOTween 动画模块入口!")
    end)
    LuaHelper.AddClick(fields.Button_FSM, function()
        local fsm = {
            [1] = "FPS有限状态机运行原理",
            [2] = "角色行为继承数据处理原理",
            [3] = "角色行为触发事件数据处理原理",
        }
        PrintTable(fsm, "FSM有限状态机 模块入口!")
    end)
end

return {
    Init = Init,
    Show = Show,
    Hide = Hide,
    Destroy = Destroy,
    Refresh = Refresh,
}