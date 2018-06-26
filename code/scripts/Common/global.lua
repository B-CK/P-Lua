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

require "Common.define"

Class = require "Common.class"
LuaUtils = require "Common.utils"
CfgMgr = require "Cfg.CfgManager"

local _, err = LuaUtils.my_xpcall(CfgMgr.init)



