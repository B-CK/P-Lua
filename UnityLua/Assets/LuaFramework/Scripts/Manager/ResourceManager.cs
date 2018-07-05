#if ASYNC_MODE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaInterface;
using UnityEngine;
using UObject = UnityEngine.Object;

public class AssetBundleInfo {
    public AssetBundle m_AssetBundle;
    public int m_ReferencedCount;

    public AssetBundleInfo (AssetBundle assetBundle) {
        m_AssetBundle = assetBundle;
        m_ReferencedCount = 0;
    }
}

namespace LuaFramework {

    public class ResourceManager : Manager {
        string m_BaseDownloadingURL = "";
        string[] m_AllManifest = null;
        AssetBundleManifest m_AssetBundleManifest = null;
        Dictionary<string, string[]> dependenciesObj = new Dictionary<string, string[]> ();
        Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo> ();
        Dictionary<string, List<LoadAssetRequest>> m_LoadRequests = new Dictionary<string, List<LoadAssetRequest>> ();

        class LoadAssetRequest {
            public Type assetType;
            public string[] assetNames;
            public LuaFunction luaFunc;
            public Action<UObject[]> sharpFunc;
        }

        // Load AssetBundleManifest.
        public void Initialize (string manifestName, Action initOK) {
            m_BaseDownloadingURL = Util.GetRelativePath ();
            LoadAsset<AssetBundleManifest> (manifestName, new string[] { "AssetBundleManifest" }, delegate (UObject[] objs) {
                if (objs.Length > 0) {
                    m_AssetBundleManifest = objs[0] as AssetBundleManifest;
                    m_AllManifest = m_AssetBundleManifest.GetAllAssetBundles ();
                }
                if (initOK != null) initOK ();
            });
        }

        public void LoadPrefab (string abName, string assetName, Action<UObject[]> func) {
            LoadAsset<GameObject> (abName, new string[] { assetName }, func);
        }

        public void LoadPrefab (string abName, string[] assetNames, Action<UObject[]> func) {
            LoadAsset<GameObject> (abName, assetNames, func);
        }

        public void LoadPrefab (string abName, string[] assetNames, LuaFunction func) {
            LoadAsset<GameObject> (abName, assetNames, null, func);
        }

        string GetRealAssetPath (string abName) {
            if (abName.Equals (AppConst.AssetDir)) {
                return abName;
            }
            abName = abName.ToLower ();
            if (!abName.EndsWith (AppConst.ExtName)) {
                abName += AppConst.ExtName;
            }
            if (abName.Contains ("/")) {
                return abName;
            }
            //string[] paths = m_AssetBundleManifest.GetAllAssetBundles();  产生GC，需要缓存结果
            for (int i = 0; i < m_AllManifest.Length; i++) {
                int index = m_AllManifest[i].LastIndexOf ('/');
                string path = m_AllManifest[i].Remove (0, index + 1); //字符串操作函数都会产生GC
                if (path.Equals (abName)) {
                    return m_AllManifest[i];
                }
            }
            Debug.LogError ("GetRealAssetPath Error:>>" + abName);
            return null;
        }

        /// <summary>
        /// 载入素材
        /// </summary>
        void LoadAsset<T> (string abName, string[] assetNames, Action<UObject[]> action = null, LuaFunction func = null) where T : UObject {
            abName = GetRealAssetPath (abName);

            LoadAssetRequest request = new LoadAssetRequest ();
            request.assetType = typeof (T);
            request.assetNames = assetNames;
            request.luaFunc = func;
            request.sharpFunc = action;

            List<LoadAssetRequest> requests = null;
            if (!m_LoadRequests.TryGetValue (abName, out requests)) {
                requests = new List<LoadAssetRequest> ();
                requests.Add (request);
                m_LoadRequests.Add (abName, requests);
                StartCoroutine (OnLoadAsset<T> (abName));
            } else {
                requests.Add (request);
            }
        }

        IEnumerator OnLoadAsset<T> (string abName) where T : UObject {
            AssetBundleInfo bundleInfo = GetLoadedAssetBundle (abName);
            if (bundleInfo == null) {
                yield return StartCoroutine (OnLoadAssetBundle (abName, typeof (T)));

                bundleInfo = GetLoadedAssetBundle (abName);
                if (bundleInfo == null) {
                    m_LoadRequests.Remove (abName);
                    Debug.LogError ("OnLoadAsset--->>>" + abName);
                    yield break;
                }
            }
            List<LoadAssetRequest> list = null;
            if (!m_LoadRequests.TryGetValue (abName, out list)) {
                m_LoadRequests.Remove (abName);
                yield break;
            }
            for (int i = 0; i < list.Count; i++) {
                string[] assetNames = list[i].assetNames;
                List<UObject> result = new List<UObject> ();

                AssetBundle ab = bundleInfo.m_AssetBundle;
                for (int j = 0; j < assetNames.Length; j++) {
                    string assetPath = assetNames[j];
                    AssetBundleRequest request = ab.LoadAssetAsync (assetPath, list[i].assetType);
                    yield return request;
                    result.Add (request.asset);

                    //T assetObj = ab.LoadAsset<T>(assetPath);
                    //result.Add(assetObj);
                }
                if (list[i].sharpFunc != null) {
                    list[i].sharpFunc (result.ToArray ());
                    list[i].sharpFunc = null;
                }
                if (list[i].luaFunc != null) {
                    list[i].luaFunc.Call ((object) result.ToArray ());
                    list[i].luaFunc.Dispose ();
                    list[i].luaFunc = null;
                }
                bundleInfo.m_ReferencedCount++;
            }
            m_LoadRequests.Remove (abName);
        }

        IEnumerator OnLoadAssetBundle (string abName, Type type) {
            string url = m_BaseDownloadingURL + abName;

            WWW download = null;
            if (type == typeof (AssetBundleManifest))
                download = new WWW (url);
            else {
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies (abName);
                if (dependencies.Length > 0) {
                    dependenciesObj.Add (abName, dependencies);
                    for (int i = 0; i < dependencies.Length; i++) {
                        string depName = dependencies[i];
                        AssetBundleInfo bundleInfo = null;
                        if (m_LoadedAssetBundles.TryGetValue (depName, out bundleInfo)) {
                            bundleInfo.m_ReferencedCount++;
                        } else if (!m_LoadRequests.ContainsKey (depName)) {
                            yield return StartCoroutine (OnLoadAssetBundle (depName, type));
                        }
                    }
                }
                download = WWW.LoadFromCacheOrDownload (url, m_AssetBundleManifest.GetAssetBundleHash (abName), 0);
            }
            yield return download;

            AssetBundle assetObj = download.assetBundle;
            if (assetObj != null) {
                m_LoadedAssetBundles.Add (abName, new AssetBundleInfo (assetObj));
            }
        }

        AssetBundleInfo GetLoadedAssetBundle (string abName) {
            AssetBundleInfo bundle = null;
            m_LoadedAssetBundles.TryGetValue (abName, out bundle);
            if (bundle == null) return null;

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if (!dependenciesObj.TryGetValue (abName, out dependencies))
                return bundle;

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies) {
                AssetBundleInfo dependentBundle;
                m_LoadedAssetBundles.TryGetValue (dependency, out dependentBundle);
                if (dependentBundle == null) return null;
            }
            return bundle;
        }

        /// <summary>
        /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="isThorough"></param>
        public void UnloadAssetBundle (string abName, bool isThorough = false) {
            abName = GetRealAssetPath (abName);
            Debug.Log (m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);
            UnloadAssetBundleInternal (abName, isThorough);
            UnloadDependencies (abName, isThorough);
            Debug.Log (m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);
        }

        void UnloadDependencies (string abName, bool isThorough) {
            string[] dependencies = null;
            if (!dependenciesObj.TryGetValue (abName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies) {
                UnloadAssetBundleInternal (dependency, isThorough);
            }
            dependenciesObj.Remove (abName);
        }

        void UnloadAssetBundleInternal (string abName, bool isThorough) {
            AssetBundleInfo bundle = GetLoadedAssetBundle (abName);
            if (bundle == null) return;

            if (--bundle.m_ReferencedCount <= 0) {
                if (m_LoadRequests.ContainsKey (abName)) {
                    return; //如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可
                }
                bundle.m_AssetBundle.Unload (isThorough);
                m_LoadedAssetBundles.Remove (abName);
                Debug.Log (abName + " has been unloaded successfully");
            }
        }
    }
}
#else

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaInterface;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LuaFramework
{
    enum ResourceLoadType
    {
        Default = 0,                        // 资源已加载,直接取资源
        Persistent = 1 << 0,                // 永驻内存的资源
        Cache = 1 << 1,                     // Asset需要缓存

        UnLoad = 1 << 4,                    // 利用www加载并且处理后是否立即unload ab,如不卸载,则在指定时间后清理

        Immediate = 1 << 5,                 // 需要立即加载
        // 加载方式
        LoadBundleFromFile = 1 << 6,        // 利用AssetBundle.LoadFromFile加载
        LoadBundleFromWWW = 1 << 7,         // 利用WWW 异步加载 AssetBundle
        ReturnAssetBundle = 1 << 8,         // 返回scene AssetBundle,默认unload ab
    }
    class CacheObject
    {
        public Object obj;
        public float lastUseTime;
    }
    class AssetLoadTask
    {
        public uint id;
        public List<uint> parentTaskIds;
        public int loadType;
        public string path;
        public Action<Object> actions;
        public List<string> dependencies;
        public int loadedDependenciesCount = 0;
        public void Reset()
        {
            id = 0;
            parentTaskIds = null;
            path = string.Empty;
            actions = null;
            dependencies = null;
            loadedDependenciesCount = 0;
        }
    }
    /// <summary>
    /// AssetBundle加载完毕,后续处理
    /// </summary>
    class LoadTask
    {
        public AssetLoadTask task;
        public AssetBundle bundle;
    }

    public class ResourceManager : Manager
    {
        //资源存储策略
        private Dictionary<string, Object> persistantObjects = new Dictionary<string, Object>();//key-依赖路径
        private Dictionary<string, CacheObject> cacheObjects = new Dictionary<string, CacheObject>();
        //加载的AB资源包
        private Dictionary<string, AssetBundle> dependenciesObj = new Dictionary<string, AssetBundle>();//key-依赖路径

        //加载任务
        private Dictionary<string, AssetLoadTask> loadingFiles = new Dictionary<string, AssetLoadTask>();//key-文件名
        private Dictionary<uint, AssetLoadTask> loadingTasks = new Dictionary<uint, AssetLoadTask>();//key-任务ID
        private ObjectPool<AssetLoadTask> assetLoadTaskPool = new ObjectPool<AssetLoadTask>();//加载资源的相关信息
        private Queue<AssetLoadTask> delayAssetLoadTask = new Queue<AssetLoadTask>();
        private ObjectPool<LoadTask> loadTaskPool = new ObjectPool<LoadTask>();
        private Queue<LoadTask> delayLoadTask = new Queue<LoadTask>();

        private bool canStartCleanupMemeory = true;
        private float cleanupMemoryLastTime;
        private float cleanupCacheBundleLastTime;
        private float cleanupDependenciesLastTime;
        private const float cleanupMemInterval = 180;
        private const float cleanupCacheInterval = 120;
        private const float cleanupDepInterval = 30;

        private AssetBundleManifest manifest;
        private string preloadListPath = "config/preloadlist.txt";
        private List<string> preloadList = new List<string>();


        private Dictionary<string, int> refCount = new Dictionary<string, int>();
        private Dictionary<string, float> refDelTime = new Dictionary<string, float>();
        private const int defaultMaxTaskCount = 10;
        private int currentTaskCount = 0;

        private static uint nextTaskId;

        public int MaxTaskCount { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            int memorySize = 1024 * 1024; //内存大小1GB
            if (memorySize <= 1024 * 1024)
            {
                MaxTaskCount = 5;
            }

            byte[] stream = null;
            string uri = string.Empty;
            uri = Util.DataPath + AppConst.AssetDir;
            if (!File.Exists(uri)) return;
            stream = File.ReadAllBytes(uri);
            assetbundle = AssetBundle.LoadFromMemory(stream);
            manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            //获取预加载资源列表
            //加载预加载资源
            //TODO
        }

        public bool IsLoading(uint taskId)
        {
            return loadingTasks.ContainsKey(taskId);
        }
        public void RemoveTask(uint taskId, Action<Object> action)
        {
            if (IsLoading(taskId))
            {
                AssetLoadTask oldTask = null;
                if (loadingTasks.TryGetValue(taskId, out oldTask))
                {
                    if (null != action)
                    {
                        oldTask.actions -= action;
                    }
                }
            }
        }
        public uint AddTask(string file, Action<Object> action)
        {
            return AddTask(file, action, (int)ResourceLoadType.LoadBundleFromFile);
        }
        public uint AddTask(string file, Action<Object> action, int loadType)
        {
            return AddTask(file, action, loadType, 0);
        }
        private uint AddTask(string file, Action<Object> action, int loadType, uint parentTaskId)
        {
            if (string.IsNullOrEmpty(file))
            {
                return 0;
            }
            string fileReplace = file.Replace(@"\", @" / ");

            string lowerFile = fileReplace.ToLower();
            Object obj;
            if (persistantObjects.TryGetValue(lowerFile, out obj))
            {
                action(obj);
                return 0;
            }

            CacheObject cacheObject;
            if (cacheObjects.TryGetValue(lowerFile, out cacheObject))
            {
                cacheObject.lastUseTime = Time.realtimeSinceStartup;

                action(cacheObject.obj);
                return 0;
            }

            AssetLoadTask oldTask;
            if (loadingFiles.TryGetValue(lowerFile, out oldTask))
            {//资源加载任务-正在进行中
                if (action != null)
                {
                    oldTask.actions += action;
                }

                if ((loadType & (int)ResourceLoadType.Persistent) > 0)
                {
                    oldTask.loadType |= (int)ResourceLoadType.Persistent;
                }

                if (parentTaskId != 0)
                {
                    if (oldTask.parentTaskIds == null)
                    {
                        oldTask.parentTaskIds = new List<uint>();
                        Debug.LogErrorFormat("resource path {0} type is mixed, dependency resource or not ", oldTask.path);
                    }
                    oldTask.parentTaskIds.Add(parentTaskId);
                }

                return 0;
            }

            //加载资源到内存
            uint id = ++nextTaskId;
            List<uint> ptList = null;
            if (parentTaskId != 0)
            {
                ptList = new List<uint>();
                ptList.Add(parentTaskId);
            }
            string[] deps = manifest.GetAllDependencies(lowerFile);
            var task = assetLoadTaskPool.Get();
            {
                task.id = id;
                task.parentTaskIds = ptList;
                task.path = lowerFile;
                task.loadType = loadType;
                task.actions = action;
                task.dependencies = deps == null ? null : new List<string>(deps);
                task.loadedDependenciesCount = 0;
            };
            loadingFiles[lowerFile] = task;
            loadingTasks[id] = task;
            if (dependenciesObj.ContainsKey(task.path))
            {//依赖资源已被加载
                AddRefCount(task.path);
            }
            //任务数量达最大时,延迟加载资源[需要排队]
            if (currentTaskCount < MaxTaskCount)
                DoTask(task);
            else
                delayAssetLoadTask.Enqueue(task);

            return id;
        }
        private void AddRefCount(string bundlename)
        {
            string[] dependencies = manifest.GetAllDependencies(bundlename);
            if (dependencies != null && dependencies.Length > 0)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    string depname = dependencies[i];
                    if (!persistantObjects.ContainsKey(depname))
                    {
                        if (!refCount.ContainsKey(depname))
                        {
                            refCount[depname] = 0;
                        }
                        refCount[depname]++;
                    }
                }
            }
        }
        private void RemoveRefCount(string bundlename)
        {
            string[] dependencies = manifest.GetAllDependencies(bundlename);
            if (dependencies != null && dependencies.Length > 0)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    string depname = dependencies[i];
                    if (refCount.ContainsKey(depname))
                    {
                        refCount[depname]--;
                        if (refCount[depname] <= 0)
                        {
                            refDelTime[depname] = Time.realtimeSinceStartup;
                        }
                    }
                }
            }
        }
        private void DoTask(AssetLoadTask task)
        {
            if (task.dependencies == null)
            {
                DoImmediateTask(task);
            }
            else
            {
                if (task.loadedDependenciesCount >= task.dependencies.Count)
                {
                    DoImmediateTask(task);
                }
                else
                {
                    int i = task.loadedDependenciesCount;
                    for (; i < task.dependencies.Count; ++i)
                    {
                        if (dependenciesObj.ContainsKey(task.dependencies[i]) || persistantObjects.ContainsKey(task.dependencies[i]))
                        {
                            task.loadedDependenciesCount += 1;
                            if (task.loadedDependenciesCount >= task.dependencies.Count)
                            {
                                DoImmediateTask(task);
                                return;
                            }
                        }
                        else
                        {
                            AddTask(task.dependencies[i], null, task.loadType, task.id);
                        }
                    }
                }
            }
        }
        private void DoImmediateTask(AssetLoadTask task)
        {
            currentTaskCount += 1;
            if ((task.loadType & (int)ResourceLoadType.LoadBundleFromWWW) != 0)
            {
                StartCoroutine(LoadBundleFromWWW(task));
            }
            else if ((task.loadType & (int)ResourceLoadType.LoadBundleFromFile) != 0)
            {
                LoadBundleFromFile(task);
            }
            else
            {
                currentTaskCount -= 1;
                Debug.LogErrorFormat("Unknown task loadtype:{0} path:{1}", task.loadType, task.path);
            }
        }
        private IEnumerator LoadBundleFromWWW(AssetLoadTask task)
        {
            string path = Util.DataPath + task.path;
            var www = new WWW(path);
            yield return www;
            if (null != www.error)
                Debug.LogErrorFormat("LoadBundleAsync: {0} failed! www error:{1}", task.path, www.error);
            OnBundleLoaded(task, www.assetBundle);
            www.Dispose();
        }
        private void LoadBundleFromFile(AssetLoadTask task)
        {
            string path = Util.DataPath + task.path;
            AssetBundle ab = AssetBundle.LoadFromFile(path);
            OnBundleLoaded(task, ab);
        }
        //AssetBundle加载完毕
        private void OnBundleLoaded(AssetLoadTask task, AssetBundle ab)
        {
            currentTaskCount -= 1;
            Object obj = null;
            if (ab == null)
            {
                Debug.LogErrorFormat("LoadBundle: {0} failed! assetBundle == NULL!", task.path);
                OnAseetsLoaded(task, ab, obj);
            }
            else
            {
                var loadTask = loadTaskPool.Get();
                loadTask.task = task;
                loadTask.bundle = ab;
                delayLoadTask.Enqueue(loadTask);
            }
        }
        void Update()
        {
            CleanupCacheBundle();
            CleanupMemoryInterval();
            CleanupDependenciesInterval();
            //定时清理池内资源
            //TODO
            //执行延迟加载任务
            DoDelayTasks();
        }
        private void DoDelayTasks()
        {
            if (delayAssetLoadTask.Count > 0)
            {
                while (delayAssetLoadTask.Count > 0 && currentTaskCount < MaxTaskCount)
                {
                    AssetLoadTask task = delayAssetLoadTask.Dequeue();
                    DoTask(task);
                }
            }
            if (delayLoadTask.Count > 0)
            {
                var maxLoadTime = 0.02f;
                var startTime = Time.realtimeSinceStartup;
                while (delayLoadTask.Count > 0 && Time.realtimeSinceStartup - startTime < maxLoadTime)
                {
                    LoadTask loadTask = delayLoadTask.Dequeue();
                    var task = loadTask.task;
                    var bundle = loadTask.bundle;

                    Object obj = null;
                    if (bundle != null)
                    {
                        if (!bundle.isStreamedSceneAssetBundle)
                        {
                            var objs = bundle.LoadAllAssets();
                            if (objs.Length > 0)
                                obj = objs[0];
                            if (obj == null)
                            {
                                Debug.LogErrorFormat("LoadBundle: {0} ! No Assets in Bundle!", task.path);
                            }
                        }
                    }
                    OnAseetsLoaded(task, bundle, obj);
                    loadTaskPool.Put(loadTask);
                }
            }
        }
        //Asset加载完毕,可能是依赖资源,也可能是主资源
        private void OnAseetsLoaded(AssetLoadTask task, AssetBundle ab, Object obj)
        {
            string[] dependencies = manifest.GetAllDependencies(task.path);
            if (dependencies == null || dependencies.Length == 0)
            {
                RemoveRefCount(task.path);
            }

            loadingFiles.Remove(task.path);
            loadingTasks.Remove(task.id);

            //主资源加载完毕
            if (task.actions != null && task.parentTaskIds == null)
            {
                Delegate[] delegates = task.actions.GetInvocationList();
                foreach (var dele in delegates)
                {
                    var action = (Action<Object>)dele;
                    try
                    {
                        if ((task.loadType & (int)ResourceLoadType.ReturnAssetBundle) > 0)
                        {
                            action(ab);
                        }
                        else
                        {
                            action(obj);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Load Bundle {0} DoAction Exception: {1}", task.path, e);
                    }
                }
            }
            
            //缓存加载的资源对象[非AB]
            if (ab != null && task.parentTaskIds == null)
            {
                if ((task.loadType & (int)ResourceLoadType.Persistent) > 0)
                {
                    persistantObjects[task.path] = obj;
                    if ((task.loadType & (int)ResourceLoadType.UnLoad) > 0)
                    {
                        ab.Unload(false);
                    }
                }
                else
                {
                    if ((task.loadType & (int)ResourceLoadType.Cache) > 0)
                    {
                        var cacheObject = new CacheObject
                        {
                            lastUseTime = Time.realtimeSinceStartup,
                            obj = obj
                        };

                        cacheObjects[task.path] = cacheObject;
                    }
                    if ((task.loadType & (int)ResourceLoadType.ReturnAssetBundle) == 0)
                    {
                        ab.Unload(false);
                    }
                }
            }

            //依赖资源加载完毕
            if (task.parentTaskIds != null)
            {
                dependenciesObj[task.path] = ab;
                for (int i = 0; i < task.parentTaskIds.Count; ++i)
                {
                    uint taskid = task.parentTaskIds[i];
                    AssetLoadTask pt = null;
                    if (loadingTasks.TryGetValue(taskid, out pt))
                    {
                        pt.loadedDependenciesCount += 1;
                        if (pt.loadedDependenciesCount >= pt.dependencies.Count)
                        {
                            DoTask(pt);
                        }
                    }
                }
            }

            task.Reset();
            assetLoadTaskPool.Put(task);
        }

        //定时清理功能       
        //-内存清理 
        public void CleanupMemoryInterval()
        {
            if (Time.realtimeSinceStartup > cleanupMemoryLastTime + cleanupMemInterval)
            {
                CleanupMemoryNow();
            }
        }
        public void CleanupMemoryNow()
        {
            if (canStartCleanupMemeory)
            {
                canStartCleanupMemeory = false;
                cleanupMemoryLastTime = Time.realtimeSinceStartup;
                StartCoroutine(CleanupMemoryAsync());
            }
        }
        private IEnumerator CleanupMemoryAsync()
        {
            yield return Resources.UnloadUnusedAssets();
            GC.Collect();
            canStartCleanupMemeory = true;
            cleanupMemoryLastTime = Time.realtimeSinceStartup;
        }
        //-缓存包清理
        private void CleanupCacheBundle()
        {
            if (cacheObjects.Count <= 0) return;
            if (!(Time.realtimeSinceStartup > cleanupCacheBundleLastTime + 10)) return;

            var now = cleanupCacheBundleLastTime = Time.realtimeSinceStartup;

            var tempList = new List<string>();
            foreach (var pair in cacheObjects)
            {
                if (now > pair.Value.lastUseTime + cleanupCacheInterval)
                {
                    tempList.Add(pair.Key);
                    if (null != pair.Value.obj)
                    {
                        //Debug.LogError(" try to destroy object: " + pair.Key);
                        if (pair.Value.obj is GameObject)
                        {
                            UnityEngine.Object.DestroyImmediate(pair.Value.obj, true);
                        }
                        else
                        {
                            Resources.UnloadAsset(pair.Value.obj);
                        }
                    }
                }
            }
            foreach (var bundle in tempList)
            {
                cacheObjects.Remove(bundle);
            }
        }
        //-依赖包清理
        public void CleanupDependenciesInterval()
        {
            if (Time.realtimeSinceStartup > cleanupDependenciesLastTime + cleanupDepInterval)
            {
                CleanupDependenciesNow();
            }
        }
        public void CleanupDependenciesNow()
        {
            if (refCount == null || refDelTime == null || dependenciesObj == null)
            {
                return;
            }
            cleanupDependenciesLastTime = Time.realtimeSinceStartup;
            List<string> refCountToRemove = new List<string>();
            foreach (var pairs in refCount)
            {
                if (pairs.Value <= 0)
                {
                    if (dependenciesObj.ContainsKey(pairs.Key) &&
                        refDelTime.ContainsKey(pairs.Key) &&
                        Time.realtimeSinceStartup - refDelTime[pairs.Key] > 60)
                    {
                        if (dependenciesObj.ContainsKey(pairs.Key))
                        {
                            if (dependenciesObj[pairs.Key] != null)
                                dependenciesObj[pairs.Key].Unload(false);
                            dependenciesObj[pairs.Key] = null;
                            dependenciesObj.Remove(pairs.Key);
                        }
                        refDelTime.Remove(pairs.Key);
                        refCountToRemove.Add(pairs.Key);
                    }
                }
            }
            foreach (var remove in refCountToRemove)
            {
                refCount.Remove(remove);
            }
        }

        //其他操作        
        void OnDestroy()
        {
            if (manifest != null) manifest = null;
            foreach (KeyValuePair<string, Object> pair in persistantObjects)
            {
                if (pair.Value != null)
                {
                    if (pair.Value is GameObject)
                    {
                        UnityEngine.Object.DestroyImmediate(pair.Value, true);
                    }
                    else
                    {
                        Resources.UnloadAsset(pair.Value);
                    }

                }
            }

            persistantObjects.Clear();

            foreach (var pair in dependenciesObj)
            {
                if (pair.Value != null)
                {
                    pair.Value.Unload(true);
                }
            }
            dependenciesObj.Clear();

            foreach (var pair in cacheObjects)
            {
                if (pair.Value != null && pair.Value.obj != null)
                {
                    if (pair.Value.obj is GameObject)
                    {
                        UnityEngine.Object.DestroyImmediate(pair.Value.obj, true);
                    }
                    else
                    {
                        Resources.UnloadAsset(pair.Value.obj);
                    }
                }
            }
            cacheObjects.Clear();
            Debug.Log("~ResourceManager was destroy!");
        }


        //******************************************** 
        private string[] m_Variants = { };
        private AssetBundle shared, assetbundle;
        private Dictionary<string, AssetBundle> bundles;

        void Awake() { }

        /// <summary>
        /// 载入素材
        /// </summary>
        public T LoadAsset<T>(string abname, string assetname) where T : UnityEngine.Object
        {
            abname = abname.ToLower();
            AssetBundle bundle = LoadAssetBundle(abname);
            return bundle.LoadAsset<T>(assetname);
        }

        public void LoadPrefab(string abName, string[] assetNames, LuaFunction func)
        {
            abName = abName.ToLower();
            List<Object> result = new List<Object>();
            for (int i = 0; i < assetNames.Length; i++)
            {
                Object go = LoadAsset<Object>(abName, assetNames[i]);
                if (go != null) result.Add(go);
            }
            if (func != null) func.Call((object)result.ToArray());
        }

        /// <summary>
        /// 载入AssetBundle
        /// </summary>
        /// <param name="abname "></param>
        /// <returns></returns>
        public AssetBundle LoadAssetBundle(string abname)
        {
            if (!abname.EndsWith(AppConst.ExtName))
            {
                abname += AppConst.ExtName;
            }
            AssetBundle bundle = null;
            if (!bundles.ContainsKey(abname))
            {
                byte[] stream = null;
                string uri = Util.DataPath + abname;
                Debug.LogWarning(" LoadFile:: >> " + uri);
                LoadDependencies(abname);

                stream = File.ReadAllBytes(uri);
                bundle = AssetBundle.LoadFromMemory(stream); //关联数据的素材绑定
                bundles.Add(abname, bundle);
            }
            else
            {
                bundles.TryGetValue(abname, out bundle);
            }
            return bundle;
        }

        /// <summary>
        /// 载入依赖
        /// </summary>
        void LoadDependencies(string name)
        {
            if (manifest == null)
            {
                Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize() ");
                return;
            }
            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = manifest.GetAllDependencies(name);
            if (dependencies.Length == 0) return;

            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies.
            for (int i = 0; i < dependencies.Length; i++)
            {
                LoadAssetBundle(dependencies[i]);
            }
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        string RemapVariantName(string assetBundleName)
        {
            string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();

            // If the asset bundle doesn't have variant, simply return.
            if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
                return assetBundleName;

            string[] split = assetBundleName.Split('.');

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (int i = 0; i < bundlesWithVariant.Length; i++)
            {
                string[] curSplit = bundlesWithVariant[i].Split('.');
                if (curSplit[0] != split[0])
                    continue;

                int found = System.Array.IndexOf(m_Variants, curSplit[1]);
                if (found != -1 && found < bestFit)
                {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }
            if (bestFitIndex != -1)
                return bundlesWithVariant[bestFitIndex];
            else
                return assetBundleName;
        }


    }
}
#endif