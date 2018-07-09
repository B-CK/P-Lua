local unpack = unpack
local UIMgr = require "Logic.UIManager"
local LuaHelper = LuaHelper
---@type UnityEngine.EventSystems.EventTriggerType
local EvtType = UnityEngine.EventSystems.EventTriggerType

local name
local fields
---@type LuaFramework.LuaBehaviour
local luaBehaviour

local function Show(params)

end

local function Hide()

end

local function Destroy()

end

local function Refresh(params)

end

local function Init(params)
    name, luaBehaviour, fields = unpack(params)

end

return {
    Init    = Init,
    Show    = Show,
    Hide    = Hide,
    Destroy = Destroy,
    Refresh = Refresh,
}