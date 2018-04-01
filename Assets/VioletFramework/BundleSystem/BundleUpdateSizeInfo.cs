using System;
using System.Collections.Generic;
using UnityEngine;

public class BundleUpdateSizeInfo {
    public Dictionary<string, long> bundleSizeDict = null;
    public ulong totalNeedDownLoadSize = 0;
    public ulong downloadedSize = 0;

    public void Init(string _jsonStr, Dictionary<string,Hash128> _needDownloadBundles) {
        totalNeedDownLoadSize = 0;
        downloadedSize = 0;
        bundleSizeDict = V.vEncoder.JsonToObject <Dictionary<string, long>> (_jsonStr);

        var checkEnumer = _needDownloadBundles.GetEnumerator();
        while (checkEnumer.MoveNext()) {
            string name = checkEnumer.Current.Key;

            if (bundleSizeDict.ContainsKey(name)) {
                totalNeedDownLoadSize += (ulong)bundleSizeDict[name];
            }
        }
    }

    public void OnPartDownloaded(ulong _downloadedBytes) {
        downloadedSize += _downloadedBytes;
    }
}
