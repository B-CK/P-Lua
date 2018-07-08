Util = LuaFramework.Util;
AppConst = LuaFramework.AppConst;
LuaHelper = LuaFramework.LuaHelper;
ByteBuffer = LuaFramework.ByteBuffer;

ResMgr = LuaHelper.GetResManager();
SoundMgr = LuaHelper.GetSoundManager();
NetworkMgr = LuaHelper.GetNetManager();

WWW = UnityEngine.WWW;
GameObject = UnityEngine.GameObject;

require "Common.Define"
Local = require "Common.Local"

Class = require "Common.Class"
LuaUtils = require "Common.Utils"
CfgMgr = require "Cfg.CfgManager"

local _, err = LuaUtils.my_xpcall(CfgMgr.init)


