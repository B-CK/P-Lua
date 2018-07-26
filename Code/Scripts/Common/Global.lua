Util = LuaFramework.Util;
AppConst = LuaFramework.AppConst;
LuaHelper = LuaFramework.LuaHelper;
ByteBuffer = LuaFramework.ByteBuffer;

ResMgr = LuaHelper.GetResManager();
SoundMgr = LuaHelper.GetSoundManager();
NetworkMgr = LuaHelper.GetNetManager();

WWW = UnityEngine.WWW;
GameObject = UnityEngine.GameObject;

Bit = require "bit"
require "Common.Define"
Local = require "Common.Local"
Class = require "Common.Class"
LuaUtils = require "Common.Utils"
CfgMgr = require "Cfg.CfgManager"
List = require"Common.List"
Queue = require"Common.Queue"
Stack = require"Common.Stack"

local _, err = LuaUtils.Myxpcall(CfgMgr.Init)


