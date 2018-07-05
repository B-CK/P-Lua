using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LuaFramework {

    /// <summary>
    /// </summary>
    public class Main : MonoBehaviour {
        string[] ss1 = { };
        string[] ss2 = null;

        private void Awake()
        {
            List<string> l1 = new List<string>(ss1);
            //List<string> l2 = new List<string>(ss2);
            Debug.LogError(l1.Count);
        }


        void Start() {
            AppFacade.Instance.StartUp();   //启动游戏
        }
    }
}