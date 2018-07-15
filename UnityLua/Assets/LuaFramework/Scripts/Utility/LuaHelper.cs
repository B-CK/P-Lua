using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Reflection;
using LuaInterface;
using System;
using UnityEngine.UI;
using System.IO;

namespace LuaFramework
{
    public static class LuaHelper
    {

        /// <summary>
        /// getType
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public static System.Type GetType(string classname)
        {
            Assembly assb = Assembly.GetExecutingAssembly();  //.GetExecutingAssembly();
            System.Type t = null;
            t = assb.GetType(classname); ;
            if (t == null)
            {
                t = assb.GetType(classname);
            }
            return t;
        }

        ///// <summary>
        ///// 面板管理器
        ///// </summary>
        //public static PanelManager GetPanelManager() {
        //    return AppFacade.Instance.GetManager<PanelManager>(ManagerName.Panel);
        //}

        /// <summary>
        /// 资源管理器
        /// </summary>
        public static ResourceManager GetResManager()
        {
            return AppFacade.Instance.GetManager<ResourceManager>(ManagerName.Resource);
        }

        /// <summary>
        /// 网络管理器
        /// </summary>
        public static NetworkManager GetNetManager()
        {
            return AppFacade.Instance.GetManager<NetworkManager>(ManagerName.Network);
        }

        /// <summary>
        /// 音乐管理器
        /// </summary>
        public static SoundManager GetSoundManager()
        {
            return AppFacade.Instance.GetManager<SoundManager>(ManagerName.Sound);
        }

        /// <summary>
        /// pbc/pblua函数回调
        /// </summary>
        /// <param name="func"></param>
        public static void OnCallLuaFunc(LuaByteBuffer data, LuaFunction func)
        {
            if (func != null) func.Call(data);
            Debug.LogWarning("OnCallLuaFunc length:>>" + data.buffer.Length);
        }

        /// <summary>
        /// cjson函数回调
        /// </summary>
        /// <param name="data"></param>
        /// <param name="func"></param>
        public static void OnJsonCallFunc(string data, LuaFunction func)
        {
            Debug.LogWarning("OnJsonCallback data:>>" + data + " lenght:>>" + data.Length);
            if (func != null) func.Call(data);
        }

        public static void Concat<T>(LuaTable table, Dictionary<string, T> dic)
        {
            foreach (var pairs in dic)
            {
                table[pairs.Key] = pairs.Value;
            }
        }

        public static void Concat<T>(LuaTable table, Dictionary<int, T> dic)
        {
            foreach (var pairs in dic)
            {
                table[pairs.Key] = pairs.Value;
            }
        }

        public static bool ScriptExists(string name)
        {
            string modulePath = name.Replace('.', '/') + ".lua";
            if (Application.isMobilePlatform)
                return File.Exists(string.Format("{0}/{1}.lua", LuaConst.luaResDir, modulePath));
            if (Application.isEditor)
                return File.Exists(string.Format("{0}/{1}.lua", LuaConst.luaDir, modulePath));
            return false;
        }

        /// <summary>
        /// UI事件添加函数
        /// </summary>
        /// <param name="go"></param>
        /// <param name="triggerType"></param>
        /// <param name="func"></param>
        public static void AddTrigger(GameObject go, EventTriggerType triggerType, LuaFunction func)
        {
            EventTrigger evtTrigger = go.GetComponent<EventTrigger>();
            if (evtTrigger == null)
                evtTrigger = go.AddComponent<EventTrigger>();

            EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
            trigger.AddListener(o => func.Call(o));
            evtTrigger.triggers.Add(new EventTrigger.Entry()
            {
                eventID = triggerType,
                callback = trigger,
            });
        }
        public static void ClearTrigger(GameObject go, EventTriggerType triggerType)
        {
            EventTrigger evtTrigger = go.GetComponent<EventTrigger>();
            if (evtTrigger == null) return;
            evtTrigger.triggers.RemoveAll(trigger => trigger.eventID == triggerType);
        }
        /// <summary>
        /// Button点击事件
        /// </summary>
        /// <param name="button"></param>
        /// <param name="func"></param>
        public static void AddClick(Button button, LuaFunction func)
        {
            button.onClick.AddListener(() => func.Call(button.gameObject));
        }
        public static void ClearClick(Button button, LuaFunction func)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}