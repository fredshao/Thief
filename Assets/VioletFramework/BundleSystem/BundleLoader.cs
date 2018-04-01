using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class BundleLoader {
    public float progress {
        get {
            if(currWebRequest != null) {
                return currWebRequest.downloadProgress;
            }
            return 0.0f;
        }
    }

    public bool isDown {
        get {
            if(currWebRequest != null) {
                return currWebRequest.isDone;
            } else {
                return false;
            }
        }
    }

    public ulong downloadedBytes {
        get {
            if(currWebRequest != null) {
                return currWebRequest.downloadedBytes;
            } else {
                return 0;
            }
        }
    }

    private UnityWebRequest currWebRequest = null;
     
    public async Task<BundleLoadResult> LoadBundleAsync(string _bundleUrl, Hash128 _bundleHash) {
        BundleLoadResult result = new BundleLoadResult();
        UnityWebRequest request = UnityWebRequest.GetAssetBundle(_bundleUrl, _bundleHash,0);
        currWebRequest = request;
        await request.Send();

        AssetBundle bundle = null;

        if (!request.isNetworkError) {
            bundle = DownloadHandlerAssetBundle.GetContent(request);
        }

        if(bundle == null) {
            result.state = BundleLoadState.Faild;
        } else {
            result.state = BundleLoadState.Success;
            result.bundle = bundle;
        }

        return result;
    }

    /// <summary>
    /// 异步加载 Bundle , 一般用来加载 Manifest 文件，以每次强制从网络加载
    /// </summary>
    /// <param name="_bundleUrl"></param>
    /// <returns></returns>
    public async Task<BundleLoadResult> LoadBundleAsync(string _bundleUrl) {
        BundleLoadResult result = new BundleLoadResult();
        UnityWebRequest request = UnityWebRequest.GetAssetBundle(_bundleUrl);
        currWebRequest = request;
        await request.Send();

        AssetBundle bundle = null;

        if (!request.isNetworkError) {
            bundle = DownloadHandlerAssetBundle.GetContent(request);
        }

        if (bundle == null) {
            result.state = BundleLoadState.Faild;
        } else {
            result.state = BundleLoadState.Success;
            result.bundle = bundle;
        }

        return result;
    }


}
