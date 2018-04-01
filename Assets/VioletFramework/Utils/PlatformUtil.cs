using System;
using System.Collections.Generic;
using UnityEngine;

public class PlatformUtil {

    /// <summary>
    /// 获取相对各平台的Bundle路径
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public static string GetRelativeBundlePath(string bundleName) {
        switch (Application.platform) {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer: {
                    return "Windows/" + bundleName;
                }
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer: {
                    return "OSX/" + bundleName;
                }
            case RuntimePlatform.IPhonePlayer: {
                    return "iOS/" + bundleName;
                }
            case RuntimePlatform.Android: {
                    return "Android/" + bundleName;
                }
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取相对于各平台的 MainManifest 路径
    /// </summary>
    /// <returns></returns>
    public static string GetRelativeMainManifestBundlePath() {
        switch (Application.platform) {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer: {
                    return "Windows/Windows";
                }
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor: {
                    return "OSX/OSX";
                }
            case RuntimePlatform.IPhonePlayer: {
                    return "iOS/iOS";
                }
            case RuntimePlatform.Android: {
                    return "Android/Android";
                }
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取 StreamingAssetPath 的 IO 方式读取在各平台中的路径
    /// NOTE: 不支持 Android
    /// </summary>
    /// <returns></returns>
    public static string GetStreamingAssetPathForIO() {
        if (Application.platform == RuntimePlatform.Android) {
            Ulog.LogError("Android 不支持使用 IO 方式 从 StreamingAssetPath 读取文件，请使用 WWW");
            return String.Empty;
        } else {
            return Application.streamingAssetsPath;
        }
    }

    /// <summary>
    /// 获取 StreamingAssetPath 的 UnityWebRequest 的方式读取在各平台中的路径
    /// NOTE: 不支持 Android
    /// </summary>
    /// <returns></returns>
    public static string GetStreamingAssetPathForUnityWebRequest() {
        if (Application.platform == RuntimePlatform.Android) {
            Ulog.LogError("Android不支持使用 UnityWebRequest 从 StreamingAssetPath 读取文件，请使用 WWW");
            return String.Empty;
        } else {
            return "file://" + Application.streamingAssetsPath;
        }
    }

    /// <summary>
    /// 获取 StreamingAssetPath 的 WWW 方式读取在各平台中的路径
    /// </summary>
    /// <returns></returns>
    public static string GetStreamingAssetPathForWWW() {
        if(Application.platform == RuntimePlatform.Android) {
            return Application.streamingAssetsPath;
        } else {
            return "file://" + Application.streamingAssetsPath;
        }
    }

    /// <summary>
    /// 获取使用 UnityWebRequest 读取文件时的路径前缀
    /// </summary>
    /// <returns></returns>
    public static string GetPathPrefixForUnityWebRequest() {
        if(Application.platform == RuntimePlatform.Android) {
            Ulog.LogError("Android不存在这种情况");
            return String.Empty;
        } else {
            return "file://";
        }
    }
}
