Util = LuaFramework.Util;
AppConst = LuaFramework.AppConst;
LuaHelper = LuaFramework.LuaHelper;
ByteBuffer = LuaFramework.ByteBuffer;

ResMgr = LuaHelper.GetResManager();
PanelMgr = LuaHelper.GetPanelManager();
SoundMgr = LuaHelper.GetSoundManager();
NetworkMgr = LuaHelper.GetNetManager();

WWW = UnityEngine.WWW;
GameObject = UnityEngine.GameObject;

Class = require "Common.class"
LuaUtils = require "Common.utils"
require "Common.define"
CfgMgr = require "Cfg.CfgManager"
CfgMgr.init()

local super = {name = "base", enable = true}
super.__index = super
local child = {pos = 1}
setmetatable(child, super)

print(child.name)
print(child.pos)
print(child.enable)

LuaUtils.Call = { Func = "System.Func", Table = { Arg1 = "1", Arg2 = 2 } }