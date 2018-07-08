using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LuaFramework
{
    //public class GameObjectItem
    //{
    //    public GameObject prefab;
    //    public float lastUsedTime;
    //}

    public class GameObjectPool
    {
        private int maxSize;
        private string poolName;
        private Transform poolRoot;
        private GameObject poolObjectPrefab;
        private Stack<GameObject> availableObjStack = new Stack<GameObject>();


        public GameObjectPool(string poolName, GameObject poolObjectPrefab, int maxSize, Transform pool)
        {
            this.poolName = poolName;
            this.maxSize = maxSize;
            this.poolRoot = pool;
            this.poolObjectPrefab = poolObjectPrefab;
        }
        public int Count { get { return availableObjStack == null ? 0 : availableObjStack.Count; } }
        public void SetDefaultCount(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddObjectToPool(NewObjectInstance());
            }
        }
        public GameObject Get()
        {
            GameObject go = null;
            if (availableObjStack.Count > 0)
            {
                go = availableObjStack.Pop();
            }
            else
            {
                Debug.LogWarning("No object available & cannot grow pool: " + poolName);
            }
            go.SetActive(true);
            return go;
        }
        //o(1)
        public void Put(string pool, GameObject po)
        {
            if (poolName.Equals(pool))
            {
                AddObjectToPool(po);
            }
            else
            {
                Debug.LogError(string.Format("Trying to add object to incorrect pool {0} ", poolName));
            }
        }
        //o(1)
        private void AddObjectToPool(GameObject go)
        {
            //add to pool
            go.SetActive(false);
            availableObjStack.Push(go);
            go.transform.SetParent(poolRoot, true);
        }

        private GameObject NewObjectInstance()
        {
            return GameObject.Instantiate(poolObjectPrefab) as GameObject;
        }





        ////定时清理功能
        //private float cleanInterval = -1;
        //private float cacheTime = -1;

        //public void OpenCleanFunc(float interval, float cacheTime)
        //{
        //    this.cleanInterval = interval;
        //    this.cacheTime = cacheTime;
        //    //添加到资源管理器中,等待清理
        //    //TODO
        //}
        //public void CloseCleanFunc()
        //{
        //    this.cleanInterval = -1;
        //    this.cacheTime = -1;
        //    //移除清理功能
        //    //TODO
        //}
    }
}
