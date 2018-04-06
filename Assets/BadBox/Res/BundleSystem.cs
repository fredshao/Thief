using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public enum EN_BundleSystemState {
    None,                       // 空闲
    OnUnZipLocalBundle,         // 正在解压本地 AssetBundle
    OnLoadingManifest,          // 正在加载 Manifest
    OnLoadingRemoteBundle,      // 正在加载远程 Bundle

    AllLocalBundleUnZipSuccess, // 所有本地Bundle解压成功
    AllRemoteBundleLoadSuccess, // 所有远程Bundle下载成功
    AllBundleCachedAndLoaded,   // 所有的Bundle已经下载成功，不需要下载

    Error_UnZipLocalBundle,     // 解压本地Bundle出错
    Error_LoadLocalManifest,    // 加载本地Manifest出错
    Error_LoadManifest,         // 加载远程Manifest出错
    Error_LoadRemoteBundle,     // 加载远程Bundle出错
    Error_LoadCachedBundle,     // 加载缓存Bundle失败
}

public class BundleSystem : BaseModule {

    private const string BUNDLE_MANIFEST = "AssetBundleManifest";

    private int _totalBundles = 0;
    private int _currBundleIndex = 0;
    private string _currBundleName = string.Empty;
    private EN_BundleSystemState _bundleSystemState = EN_BundleSystemState.None;
    
    /// <summary>
    /// 当前 BundleSystem 状态
    /// </summary>
    public EN_BundleSystemState bundleSystemState {
        get { return _bundleSystemState; }
        private set { _bundleSystemState = value; }
    }

    /// <summary>
    /// 一共要加载多少个Bundle
    /// </summary>
    public int totalBundles {
        get { return _totalBundles; }
        private set { _totalBundles = value; }
    }

    /// <summary>
    /// 当前加载的 Bundle 索引
    /// </summary>
    public int currBundleIndex {
        get { return _currBundleIndex; }
        private set { _currBundleIndex = value; }
    }

    /// <summary>
    /// 正在加载的 Bundle 名字
    /// </summary>
    public string currBundleName {
        get { return _currBundleName; }
        private set { _currBundleName = value; }
    }

    public int progress {
        get {
            /*
            if (loader.isDown) {
                return 100;
            } else {
                int progress = (int)(loader.progress * 100 * 1.15f);
                progress = Mathf.Clamp(progress, 0, 100);
                return progress;
            }
            */

            int progress = (int)(((float)downloadedBytes / (float)totalNeedDownloadBytes) * 100);
            progress = Mathf.Clamp(progress, 0, 100);

            return progress;
        }
    }

    public ulong totalNeedDownloadBytes {
        get {
            return updateSizeInfo.totalNeedDownLoadSize;
        }
    }

    public ulong downloadedBytes {
        get {
            if (loader.isDown) {
                return updateSizeInfo.downloadedSize;
            } else {
                return updateSizeInfo.downloadedSize + loader.downloadedBytes;
            }
        }
    }

    private BundleLoader loader = new BundleLoader();

    private Dictionary<string, Hash128> bundleInfoDict = null;

    private BundleUpdateSizeInfo updateSizeInfo = new BundleUpdateSizeInfo();

    /// <summary>
    /// 已经加载完的AssetBundle
    /// </summary>
    private Dictionary<string, AssetBundle> loadedBundleDict = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// 哪一个资源在哪个Bundle里
    /// </summary>
    private Dictionary<string, string> assetToBundleDict = new Dictionary<string, string>();

    /// <summary>
    /// 已经到内存中的资源
    /// </summary>
    private Dictionary<string, UnityEngine.Object> loadedAssetDict = new Dictionary<string, UnityEngine.Object>();

    /// <summary>
    /// 资源请求回调，如果有多个异步请求同一个资源，则只会有一个加载
    /// </summary>
    private Dictionary<string, Listener<UnityEngine.Object>> assetRequestCallbackDict = new Dictionary<string, Listener<UnityEngine.Object>>();



    /// <summary>
    /// 异步解压本地 AssetBundle
    /// </summary>
    /// <returns></returns>
    public async Task UnZipLocalBundleAsync(string _localBundlePath) {
        bundleSystemState = EN_BundleSystemState.OnUnZipLocalBundle;
        totalBundles = 0;
        currBundleIndex = 0;
        currBundleName = BUNDLE_MANIFEST;

        BundleLoadResult manifestLoadResult = await loader.LoadBundleAsync(GetLocalManifestUrl(_localBundlePath));
        if(manifestLoadResult.state == BundleLoadState.Faild) {
            bundleSystemState = EN_BundleSystemState.Error_LoadLocalManifest;
            return;
        }

        AssetBundleManifest manifest = (AssetBundleManifest)manifestLoadResult.bundle.LoadAsset(BUNDLE_MANIFEST);
        Dictionary<string, Hash128> localBundleInfo = GetAllBundleInfoFromManifest(manifest);

        manifestLoadResult.bundle.Unload(true);

        totalBundles = localBundleInfo.Count;
        int bundleIndex = 0;

        var enumer = localBundleInfo.GetEnumerator();
        while (enumer.MoveNext()) {
            ++bundleIndex;
            string bundleName = enumer.Current.Key;
            Hash128 bundleHash = enumer.Current.Value;

            bundleSystemState = EN_BundleSystemState.OnUnZipLocalBundle;
            currBundleIndex = bundleIndex;
            currBundleName = bundleName;

            BundleLoadResult localBundleResult = await loader.LoadBundleAsync(GetLocalBundleUrl(_localBundlePath,bundleName),bundleHash);
            if(localBundleResult.state == BundleLoadState.Faild) {
                bundleSystemState = EN_BundleSystemState.Error_UnZipLocalBundle;
                return;
            } else {
                localBundleResult.bundle.Unload(true);
            }
        }

        bundleSystemState = EN_BundleSystemState.AllLocalBundleUnZipSuccess;
    }

    ///// <summary>
    ///// 异步加载 AssetBundle
    ///// </summary>
    ///// <returns></returns>
    //public async Task LoadBundlesAsync(string _remoteBundlePath) {
    //    bundleSystemState = EN_BundleSystemState.OnLoadingManifest;
    //    totalBundles = 0;
    //    currBundleIndex = 0;
    //    currBundleName = BUNDLE_MANIFEST;

    //    // Load Bundle Manifest
    //    BundleLoadResult manifestLoadResult = await loader.LoadBundleAsync(GetRemoteManifestUrl(_remoteBundlePath));

    //    if(manifestLoadResult.state == BundleLoadState.Faild) {
    //        bundleSystemState = EN_BundleSystemState.Error_LoadManifest;
    //        return;
    //    }

    //    AssetBundleManifest manifest = (AssetBundleManifest)manifestLoadResult.bundle.LoadAsset(BUNDLE_MANIFEST);
    //    bundleInfoDict = GetAllBundleInfoFromManifest(manifest);

    //    totalBundles = bundleInfoDict.Count;
    //    int bundleIndex = 0;

    //    var enumer = bundleInfoDict.GetEnumerator();
    //    while (enumer.MoveNext()) {
    //        ++bundleIndex;
    //        string bundleName = enumer.Current.Key;
    //        Hash128 bundleHash = enumer.Current.Value;

    //        bundleSystemState = EN_BundleSystemState.OnLoadingRemoveBundle;
    //        currBundleIndex = bundleIndex;
    //        currBundleName = bundleName;

    //        BundleLoadResult bundleLoadResult = await loader.LoadBundleAsync(GetRemoteBundleUrl(_remoteBundlePath,bundleName), bundleHash);

    //        if(bundleLoadResult.state == BundleLoadState.Faild) {
    //            bundleSystemState = EN_BundleSystemState.Error_LoadRemoveBundle;
    //            return;
    //        }

    //        SaveBundleInfo(bundleLoadResult.bundle);
    //    }

    //    bundleSystemState = EN_BundleSystemState.AllRemoveBundleLoadSuccess;
    //}


    /// <summary>
    /// 异步加载 AssetBundle, 显示要下载的文件大小，以及现在下载了多少
    /// </summary>
    /// <returns></returns>
    public async Task LoadBundlesAsync(string _remoteBundlePath, string _bundleSizeFile, Action<EN_BundleSystemState> _callback) {
        bundleSystemState = EN_BundleSystemState.OnLoadingManifest;
        totalBundles = 0;
        currBundleIndex = 0;
        currBundleName = BUNDLE_MANIFEST;

        // Load Bundle Manifest
        BundleLoadResult manifestLoadResult = await loader.LoadBundleAsync(GetRemoteManifestUrl(_remoteBundlePath));

        if (manifestLoadResult.state == BundleLoadState.Faild) {
            bundleSystemState = EN_BundleSystemState.Error_LoadManifest;
            if(_callback != null) {
                _callback(bundleSystemState);
            }
            return;
        }

        AssetBundleManifest manifest = (AssetBundleManifest)manifestLoadResult.bundle.LoadAsset(BUNDLE_MANIFEST);
        bundleInfoDict = GetAllBundleInfoFromManifest(manifest);

        Dictionary<string, Hash128> unCachedBundles = new Dictionary<string, Hash128>();
        Dictionary<string, Hash128> cachedBundles = new Dictionary<string, Hash128>();

        // Check which bundles are cached, and which bundles uncached
        var checkEnumer = bundleInfoDict.GetEnumerator();
        while (checkEnumer.MoveNext()) {
            string bundleName = checkEnumer.Current.Key;
            Hash128 bundleHash = checkEnumer.Current.Value;
            string bundleUrl = GetRemoteBundleUrl(_remoteBundlePath, bundleName);
            bool isCached = Caching.IsVersionCached(bundleUrl, bundleHash);

            if (isCached) {
                cachedBundles.Add(bundleName, bundleHash);
            } else {
                unCachedBundles.Add(bundleName, bundleHash);
            }
        }

        totalBundles = unCachedBundles.Count;
        int bundleIndex = 0;

        // first, load cached bundles
        var cachedBundleEnumer = cachedBundles.GetEnumerator();
        while (cachedBundleEnumer.MoveNext()) {
            string bundleName = cachedBundleEnumer.Current.Key;
            Hash128 bundleHash = cachedBundleEnumer.Current.Value;
            string bundleUrl = GetRemoteBundleUrl(_remoteBundlePath, bundleName);

            Ulog.Log("加载缓存的Bundle: ", bundleName);

            BundleLoadResult bundleLoadResult = await loader.LoadBundleAsync(bundleUrl, bundleHash);

            if(bundleLoadResult.state == BundleLoadState.Faild) {
                bundleSystemState = EN_BundleSystemState.Error_LoadCachedBundle;
                if (_callback != null) {
                    _callback(bundleSystemState);
                }
                return;
            }

            SaveBundleInfo(bundleLoadResult.bundle);
        }

        if (unCachedBundles.Count > 0) {
            // Download Bundle Size Info File
            string bundleSizeFileUrl = GetRemoteBundleUrl(_remoteBundlePath, _bundleSizeFile);
            UnityWebRequest sizeInfoRequest = UnityWebRequest.Get(bundleSizeFileUrl);
            await sizeInfoRequest.SendWebRequest();

            if (sizeInfoRequest.isNetworkError) {
                bundleSystemState = EN_BundleSystemState.Error_LoadRemoteBundle;
                if (_callback != null) {
                    _callback(bundleSystemState);
                }
                return;
            }

            string jsonText = sizeInfoRequest.downloadHandler.text.Substring(1);
            updateSizeInfo.Init(jsonText, unCachedBundles);

            // then, load uncached bundles
            var uncachedBundleEnumer = unCachedBundles.GetEnumerator();
            while (uncachedBundleEnumer.MoveNext()) {
                ++bundleIndex;
                string bundleName = uncachedBundleEnumer.Current.Key;
                Hash128 bundleHash = uncachedBundleEnumer.Current.Value;
                string bundleUrl = GetRemoteBundleUrl(_remoteBundlePath, bundleName);

                Ulog.Log("加载未缓存的Bundle: ", bundleName);

                bundleSystemState = EN_BundleSystemState.OnLoadingRemoteBundle;
                currBundleIndex = bundleIndex;
                currBundleName = bundleName;

                BundleLoadResult bundleLoadResult = await loader.LoadBundleAsync(bundleUrl, bundleHash);

                if (bundleLoadResult.state == BundleLoadState.Faild) {
                    bundleSystemState = EN_BundleSystemState.Error_LoadRemoteBundle;
                    if (_callback != null) {
                        _callback(bundleSystemState);
                    }
                    return;
                }

                updateSizeInfo.OnPartDownloaded(loader.downloadedBytes);

                SaveBundleInfo(bundleLoadResult.bundle);
            }

            bundleSystemState = EN_BundleSystemState.AllRemoteBundleLoadSuccess;
        } else {
            bundleSystemState = EN_BundleSystemState.AllBundleCachedAndLoaded;
        }

        if (_callback != null) {
            _callback(bundleSystemState);
        }
    }



    /// <summary>
    /// 同步获取一个资源
    /// </summary>
    /// <param name="_fullAssetName">资源全路径，来自资源Json清单</param>
    /// <returns>返回原始资源(不克隆)</returns>
    public UnityEngine.Object GetAsset(string _fullAssetName) {
        _fullAssetName = _fullAssetName.ToLower();
        if (!loadedAssetDict.ContainsKey(_fullAssetName)) {
            assetToBundleDict.TryGetValue(_fullAssetName, out string bundleName);
            if (String.IsNullOrEmpty(bundleName)) {
                UnityEngine.Debug.LogError(String.Format("无法找到资源 {0} 所在的 Bundle", _fullAssetName));
                return null;
            }

            loadedBundleDict.TryGetValue(bundleName, out AssetBundle bundle);
            if(bundle == null) {
                UnityEngine.Debug.LogError(String.Format("无法找到Bundle: {0}", bundleName));
                return null;
            }

            UnityEngine.Object asset = bundle.LoadAsset(_fullAssetName);

            if(asset != null) {
                loadedAssetDict.Add(_fullAssetName, asset);
            } else {
                UnityEngine.Debug.LogError(String.Format("无法从Bundle中加载资源 {0}", _fullAssetName));
                return null;
            }
        }

        return loadedAssetDict[_fullAssetName];
    }

    public T GetAsset<T>(string _fullAssetName) where T : UnityEngine.Object {
        _fullAssetName = _fullAssetName.ToLower();
        if (!loadedAssetDict.ContainsKey(_fullAssetName)) {
            assetToBundleDict.TryGetValue(_fullAssetName, out string bundleName);
            if (String.IsNullOrEmpty(bundleName)) {
                UnityEngine.Debug.LogError(String.Format("无法找到资源 {0} 所在的 Bundle", _fullAssetName));
                return null;
            }

            loadedBundleDict.TryGetValue(bundleName, out AssetBundle bundle);
            if (bundle == null) {
                UnityEngine.Debug.LogError(String.Format("无法找到Bundle: {0}", bundleName));
                return null;
            }

            UnityEngine.Object asset = bundle.LoadAsset(_fullAssetName);

            if (asset != null) {
                loadedAssetDict.Add(_fullAssetName, asset);
            } else {
                UnityEngine.Debug.LogError(String.Format("无法从Bundle中加载资源 {0}", _fullAssetName));
                return null;
            }
        }

        if(typeof(T) == typeof(GameObject)) {
            return GameObject.Instantiate(loadedAssetDict[_fullAssetName] as T);
        }

        return loadedAssetDict[_fullAssetName] as T;
    }


    /// <summary>
    /// 异步获取一个资源
    /// </summary>
    /// <param name="_fullAssetName">资源全路径</param>
    /// <param name="_callback">l加载完后的回调</param>
    public async Task GetAssetAsync(string _fullAssetName, Listener<UnityEngine.Object> _callback) {

        // 将回调添加到字典
        if (_callback != null) {
            assetRequestCallbackDict[_fullAssetName] += _callback;
        }

        // 缓存中不存在资源，需要加载
        if (!loadedAssetDict.ContainsKey(_fullAssetName)) {

            // 获取资源所在的 Bundle 名字
            assetToBundleDict.TryGetValue(_fullAssetName, out string bundleName);
            if (String.IsNullOrEmpty(bundleName)) {
                UnityEngine.Debug.LogError(String.Format("无法找到资源 {0} 所在的 Bundle", _fullAssetName));
                CallbackAsNull(_fullAssetName);
                return;
            }

            // 获取资源所在的 Bundle
            loadedBundleDict.TryGetValue(bundleName, out AssetBundle bundle);
            if (bundle == null) {
                UnityEngine.Debug.LogError(String.Format("无法找到Bundle: {0}", bundleName));
                CallbackAsNull(_fullAssetName);
                return;
            }

            // 异步加载资源
            AssetBundleRequest request = bundle.LoadAssetAsync(_fullAssetName);
            await request;

            if (request == null || request.asset == null) {
                UnityEngine.Debug.LogError(String.Format("从Bundle中异步加载资源 {0} 失败", _fullAssetName));
                CallbackAsNull(_fullAssetName);
                return;
            }

            // 资源缓存
            UnityEngine.Object obj = request.asset;
            loadedAssetDict.Add(_fullAssetName, obj);
        }

        // 资源加载完回调
        if (assetRequestCallbackDict[_fullAssetName] != null) {
            assetRequestCallbackDict[_fullAssetName](loadedAssetDict[_fullAssetName]);
            assetRequestCallbackDict[_fullAssetName] = null;
        }
    }

    // ----------------------------- internal function -------------------------------
    /// <summary>
    /// 异步加载资源空回调(没有加载到指定的资源)
    /// </summary>
    /// <param name="_assetLoadCallbackName"></param>
    private void CallbackAsNull(string _assetLoadCallbackName) {
        if (assetRequestCallbackDict[_assetLoadCallbackName] != null) {
            assetRequestCallbackDict[_assetLoadCallbackName](null);
            assetRequestCallbackDict[_assetLoadCallbackName] = null; 
        }
    }

    /// <summary>
    /// 保存一个 Bundle 的信息，将Bundle 保存到字典，将Bundle里面的资源名字保存到字典
    /// </summary>
    /// <param name="_bundle"></param>
    private void SaveBundleInfo(AssetBundle _bundle) {
        string bundleName = _bundle.name;
        if (!loadedBundleDict.ContainsKey(bundleName)) {
            loadedBundleDict.Add(bundleName, _bundle);
        }

        string[] allAssets = _bundle.GetAllAssetNames();
        for(int i = 0; i < allAssets.Length; ++i) {
            string fullAssetName = allAssets[i];        // Full Asset path , 如 Asset/Res/UI/main.png
            if (!assetToBundleDict.ContainsKey(fullAssetName)) {
                assetToBundleDict.Add(fullAssetName, bundleName);
            }
        }
    }

    /// <summary>
    /// 从 Manifest 中获取所有的 Bundle 名字和 Bundle Hash128
    /// </summary>
    /// <param name="_manifest"></param>
    /// <returns></returns>
    private Dictionary<string, Hash128> GetAllBundleInfoFromManifest(AssetBundleManifest _manifest) {
        Dictionary<string, Hash128> bundleInfoDict = new Dictionary<string, Hash128>();
        string[] allBundleNames = _manifest.GetAllAssetBundles();
        for(int i = 0; i < allBundleNames.Length; ++i) {
            string bundleName = allBundleNames[i];
            Hash128 bundleHash = _manifest.GetAssetBundleHash(bundleName);
            bundleInfoDict.Add(bundleName, bundleHash);
        }

        return bundleInfoDict;
    }

    /// <summary>
    /// 获取Bundle的远程地址
    /// </summary>
    /// <param name="_bundleName"></param>
    /// <returns></returns>
    private string GetRemoteBundleUrl(string _remoteBundlePath, string _bundleName) {
        return _remoteBundlePath + GetBundleRootByPlatform() + "/" + _bundleName;
    }

    /// <summary>
    /// 获取远程Manifest地址
    /// </summary>
    /// <param name="_remoteBundlePath"></param>
    /// <returns></returns>
    private string GetRemoteManifestUrl(string _remoteBundlePath) {
        return _remoteBundlePath + GetBundleRootByPlatform() + "/" + GetBundleRootByPlatform();
    }

    /// <summary>
    /// 获取本地Manifest地址
    /// </summary>
    /// <param name="_localBundlePath"></param>
    /// <returns></returns>
    private string GetLocalManifestUrl(string _localBundlePath) {
        return _localBundlePath + GetBundleRootByPlatform() + "/" + GetBundleRootByPlatform();
    }

    /// <summary>
    /// 获取本地 Bundle 地址
    /// </summary>
    /// <param name="_bundleName"></param>
    /// <returns></returns>
    private string GetLocalBundleUrl(string _localBundlePath, string _bundleName) {
        return _localBundlePath + GetBundleRootByPlatform() + "/" + _bundleName;
    }

    /// <summary>
    /// 获取平台相关的Bundle主目录地址
    /// </summary>
    /// <returns></returns>
    private string GetBundleRootByPlatform() {
        switch (Application.platform) {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer: {
                    return "Windows";
                }
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer: {
                    return "OSX";
                }
            case RuntimePlatform.Android: {
                    return "Android";
                }

            case RuntimePlatform.IPhonePlayer: {
                    return "iOS";
                }
        }

        return "";
    }
}