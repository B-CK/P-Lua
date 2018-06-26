--table嵌套后的字段需要在定义处,一次性定义完,如果分离定义则无法在提示中显示出来

--开始初始化类以及常量数据
require "Common.global"
require "Common.functions"
require("3rd.cjson.lua2json")

--模块管理器初始化
local _modules = {
    "Logic.CtrlManager"
}

function Main()
    for _, name in ipairs(_modules) do
        local module = require(name)
        module.Init()
    end
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()
end