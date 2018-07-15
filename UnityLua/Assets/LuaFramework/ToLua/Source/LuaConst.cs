﻿using LuaFramework;
using UnityEngine;

public static class LuaConst
{
#if UNITY_EDITOR
    public static string luaDir        = Application.dataPath + "/../../Code/Scripts";                         //lua逻辑代码目录
    public static string toluaDir      = Application.dataPath + "/../../Code/Scripts/Lua";                             //tolua lua文件目录
    public static string osDir         = "code";
#endif

    /// <summary>
    /// 手机运行时,lua脚本/配置目录
    /// </summary>
    public static string luaResDir     = Application.persistentDataPath + "/Scripts/";      //手机运行时lua文件下载目录    

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN    
    public static string zbsDir        = "D:/ZeroBraneStudio/lualibs/mobdebug";                                //ZeroBraneStudio目录       
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	public static string zbsDir        = "/Applications/ZeroBraneStudio.app/Contents/ZeroBraneStudio/lualibs/mobdebug";
#else
    public static string zbsDir        = luaResDir + "/mobdebug/";
#endif    

    public static bool openLuaSocket   = true;                                                                 //是否打开Lua Socket库
    public static bool openLuaDebugger = false;                                                                //是否连接lua调试器
}