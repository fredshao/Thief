using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Violet.Resources;

/// <summary>
/// 资源模块
/// 资源的加载 ，更新，销毁，缓存等
/// </summary>
public class ResourceSystem : BaseModule {

    public bool usePool = true;
    private bool assetJsonLoaded = false;

    private Dictionary<int, AssetVO> assetDict = new Dictionary<int, AssetVO>();

    /// <summary>
    /// 读取资源Json表
    /// </summary>
    /// <param name="_jsonPath">Assets开始的路径，和Bundle路径保持一致</param>
    public bool LoadAssetJson(string _jsonPath) {

        if(assetJsonLoaded == true) {
            return true;
        }

        assetJsonLoaded = true;

        string json = "";
#if UNITY_EDITOR
        if (!VioletConfig.userConfig.UseBundleOnEditor) {
            _jsonPath = _jsonPath.Replace("Assets", "");
            _jsonPath = Application.dataPath + _jsonPath;

            if (!File.Exists(_jsonPath)) {
                Ulog.LogError(this, "资源数据加载失败:", _jsonPath);
                assetJsonLoaded = false;
                return false;
            }

            StreamReader reader = new StreamReader(_jsonPath);
            json = reader.ReadToEnd();
            reader.Close();

            InitAssetData(json);
            return true;
        }
#endif

        TextAsset textAsset = V.vBundle.GetAsset<TextAsset>(_jsonPath);
        json = textAsset.text;

        InitAssetData(json);

        return true;

    }

    /// <summary>
    /// 初始化资源数据
    /// </summary>
    /// <param name="_json"></param>
    private void InitAssetData(string _json) {
        AssetData data = JsonUtility.FromJson<AssetData>(_json);

        foreach (AssetVO VO in data.assetList) {
            assetDict.Add(VO.assetId, VO);
        }
        Ulog.Log(this, "Asset Json Loaded!");
    }


    /// <summary>
    /// 根据资源ID获取一个资源的数据结构
    /// </summary>
    /// <param name="_assetId"></param>
    /// <returns></returns>
    public AssetVO GetAssetVOById(int _assetId) {
        assetDict.TryGetValue(_assetId, out AssetVO vo);
        return vo;
    }

    /// <summary>
    /// 根据资源ID获取一个资源的路径(从Assets开始)
    /// </summary>
    /// <param name="_assetId"></param>
    /// <returns></returns>
    public string GetAssetPathById(int _assetId) {
        AssetVO VO = GetAssetVOById(_assetId);
        if(VO != null) {
            return VO.path;
        }
        Ulog.LogError(String.Format("不存在ID为 {0} 的资源", _assetId));
        return String.Empty;
    }

    /// <summary>
    /// 获取一个 GameObject (从原始Bundle资源克隆)
    /// </summary>
    /// <param name="_assetId"></param>
    /// <returns></returns>
    public GameObject GetGameObject(int _assetId) {
        AssetVO assetVO = GetAssetVOById(_assetId);
        if(assetVO == null) {
            Ulog.LogError(this, "找不到要加载的GameObject路径：", _assetId);
            return null;
        }

        string path = assetVO.path;
        GameObject rawObj = null;

#if UNITY_EDITOR
        if (!VioletConfig.userConfig.UseBundleOnEditor) {
            rawObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
#endif

        rawObj = V.vBundle.GetAsset<GameObject>(path);

        if(rawObj != null) {
            return GameObject.Instantiate(rawObj);
        }

        Ulog.LogError(this, "加载GameObject失败:", _assetId, path);
        return null;
    }

    /// <summary>
    /// 获取一个非GameObject的资源，直接返回，(引用，非克隆)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_assetId"></param>
    /// <returns></returns>
    public T GetAsset<T>(int _assetId) where T : UnityEngine.Object {

        if(typeof(T) == typeof(GameObject)) {
            Ulog.LogError(this, "要加载GameObject请直接使用 ResourceSystem 的 GetGameObject!");
            return GetGameObject(_assetId) as T;
        }

        AssetVO assetVO = GetAssetVOById(_assetId);
        if (assetVO == null) {
            Ulog.LogError(this, "找不到要加载的资源路径:", _assetId);
            return null;
        }

        string path = assetVO.path;
        T rawAsset = null;
#if UNITY_EDITOR
        if (!VioletConfig.userConfig.UseBundleOnEditor) {
            rawAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif

        rawAsset = V.vBundle.GetAsset<T>(path);
        if(rawAsset == null) {
            Ulog.LogError(this, "加载资源失败:", _assetId, path);
        }
        return rawAsset;
    }

   
    /// <summary>
    /// 加载配置表数据
    /// </summary>
    /// <param name="_fullPath">完整路径，和Bundle路径保持一致,例如： Assets/ABRes/ConfigData/ConfigData.bytes </param>
    /// <returns></returns>
    public byte[] GetConfigDataBytes(string _fullPath) {
#if UNITY_EDITOR
        if (!VioletConfig.userConfig.UseBundleOnEditor) {
            _fullPath = _fullPath.Replace("Assets", "");
            _fullPath = Application.dataPath + _fullPath;

            if (!File.Exists(_fullPath)) {
                Ulog.LogError(this, "ConfigDataPath 不存在:", _fullPath);
                return null;
            }

            FileStream fs = new FileStream(_fullPath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);
            byte[] bytes = new byte[fs.Length];
            reader.Read(bytes, 0, bytes.Length);
            reader.Close();
            fs.Close();

            return bytes;
        }
#endif
        TextAsset textAsset = V.vBundle.GetAsset<TextAsset>(_fullPath);
        if(textAsset != null) {
            return textAsset.bytes;
        }

        Ulog.LogError(this, "二进制文件加载失败：", _fullPath);

        return null;
    }


    /// <summary>
    /// 卸载一个 GameObject (缓存)
    /// </summary>
    /// <param name="_assetid"></param>
    /// <param name="_obj"></param>
    public void ReleaseGameObject(int _assetid, GameObject _obj) {

    }

    /// <summary>
    /// 直接删除一个 GameObject (不缓存)
    /// </summary>
    /// <param name="_obj"></param>
    public void ForceReleaseGameObject(GameObject _obj) {
        GameObject.Destroy(_obj);
    }

    /// <summary>
    /// 延迟一定时间写在一个 GameObject (缓存)
    /// </summary>
    /// <param name="_assetid"></param>
    /// <param name="_obj"></param>
    /// <param name="_delay"></param>
    public void ReleaseGameObject(int _assetid, GameObject _obj, float _delay) {

    }

    /// <summary>
    /// 延迟删除一个 GameObject (不缓存)
    /// </summary>
    /// <param name="_assetId"></param>
    /// <param name="_obj"></param>
    /// <param name="_delay"></param>
    public void ForceReleaseGameObject(int _assetId, GameObject _obj, float _delay) {

    }


}
