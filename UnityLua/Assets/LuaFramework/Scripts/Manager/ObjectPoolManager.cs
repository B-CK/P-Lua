using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace LuaFramework
{
    //管理器管理池定时功能,定时功能均基于对象定时处理,暂时不做基于某类池的定时处理    
    //管理器限制池中资源总数量,大于数量直接删除最早且未使用资源
    //单类池限制最大数量,大于最大数量定时清理资源
    //-池中物回收方式-
    //IsEnable      -隐藏激活
    //IsVisible     -是否移出相机
    //IsDestroy     -是否销毁

    /// <summary>
    /// 对象池管理器，分普通类对象池+资源游戏对象池
    /// </summary>
    public class ObjectPoolManager : Manager
    {
        private Transform poolRootObject = null;
        private Dictionary<string, object> objectPools = new Dictionary<string, object>();
        private Dictionary<string, GameObjectPool> gameObjectPools = new Dictionary<string, GameObjectPool>();

        /// <summary>
        /// 默认最大50个对象
        /// </summary>
        public static int MAX_VALUE = 50;

        Transform PoolRootObject
        {
            get
            {
                if (poolRootObject == null)
                {
                    var objectPool = new GameObject("ObjectPool");
                    objectPool.transform.SetParent(transform);
                    objectPool.transform.localScale = Vector3.one;
                    objectPool.transform.localPosition = Vector3.zero;
                    poolRootObject = objectPool.transform;
                }
                return poolRootObject;
            }
        }

        public GameObjectPool CreatePool(string poolName, GameObject prefab, int maxSize = 5)
        {
            var pool = new GameObjectPool(poolName, prefab, maxSize, PoolRootObject);
            gameObjectPools[poolName] = pool;
            return pool;
        }

        public GameObjectPool GetPool(string poolName)
        {
            if (gameObjectPools.ContainsKey(poolName))
            {
                return gameObjectPools[poolName];
            }
            return null;
        }

        public GameObject Get(string poolName)
        {
            GameObject result = null;
            if (gameObjectPools.ContainsKey(poolName))
            {
                GameObjectPool pool = gameObjectPools[poolName];
                result = pool.Get();
                if (result == null)
                {
                    Debug.LogWarning("No object available in pool. Consider setting fixedSize to false.: " + poolName);
                }
            }
            else
            {
                Debug.LogError("Invalid pool name specified: " + poolName);
            }
            return result;
        }

        public void Release(string poolName, GameObject go)
        {
            if (gameObjectPools.ContainsKey(poolName))
            {
                GameObjectPool pool = gameObjectPools[poolName];
                pool.Put(poolName, go);
            }
            else
            {
                Debug.LogWarning("No pool available with name: " + poolName);
            }
        }

        ///-----------------------------------------------------------------------------------------------

        public ObjectPool<T> CreatePool<T>(int maxNum, bool createWhenIsFull) where T : class, new()
        {
            var type = typeof(T);
            var pool = new ObjectPool<T>(maxNum, createWhenIsFull);
            objectPools[type.Name] = pool;
            return pool;
        }

        public ObjectPool<T> GetPool<T>() where T : class, new()
        {
            var type = typeof(T);
            ObjectPool<T> pool = null;
            if (objectPools.ContainsKey(type.Name))
            {
                pool = objectPools[type.Name] as ObjectPool<T>;
            }
            return pool;
        }

        public T Get<T>() where T : class, new()
        {
            var pool = GetPool<T>();
            if (pool != null)
            {
                return pool.Get();
            }
            return default(T);
        }

        public void Put<T>(T obj) where T : class, new()
        {
            var pool = GetPool<T>();
            if (pool != null)
            {
                pool.Put(obj);
            }
        }
    }
}