using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace LuaFramework
{

    /// <summary>
    /// </summary>
    public class Main : MonoBehaviour
    {
        void Start()
        {
            AppFacade.Instance.StartUp();   //启动游戏
        }
    }
}