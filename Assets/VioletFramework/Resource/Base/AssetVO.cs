using System;
using System.Collections.Generic;

namespace Violet.Resources {
    public enum EN_AssetType {
        Unknow,
        Prefab,
        Texture2D,
        Material,
    }

    [System.Serializable]
    public class AssetVO {
        public int assetId;
        public string path;
        public string assetName;
        public EN_AssetType assetType;
        public int preloadCount;

        public AssetVO(int _assetId, string _path, string _assetName, EN_AssetType _type, int _preloadCount = 0) {
            assetId = _assetId;
            path = _path;
            assetName = _assetName;
            assetType = _type;
            preloadCount = _preloadCount;
        }
    }
}