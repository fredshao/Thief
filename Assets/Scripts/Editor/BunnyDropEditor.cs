using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AssetBundles.GraphTool;
using System.IO;

public class ThiefEditor : Editor {

    [MenuItem("BunnyTools/导出WindowsX64 Bundles")]
    public static void BuildAssetBundleForWindowsX64() {
        GenerateBundleInfo.ClearTmpBundleDirectory();
        AssetBundleGraphEditorWindow.BuildFromEditorScripts(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("BunnyTools/导出OSX Bundles")]
    public static void BuildAssetBundleForOSX() {
        GenerateBundleInfo.ClearTmpBundleDirectory();
        AssetBundleGraphEditorWindow.BuildFromEditorScripts(BuildTarget.StandaloneOSX);
    }

    [MenuItem("BunnyTools/导出iOS Bundles")]
    public static void BuildAssetBundleForiOS() {
        GenerateBundleInfo.ClearTmpBundleDirectory();
        AssetBundleGraphEditorWindow.BuildFromEditorScripts(BuildTarget.iOS);
    }

    [MenuItem("BunnyTools/导出Android Bundles")]
    public static void BuildAssetBundleForAndroid() {
        GenerateBundleInfo.ClearTmpBundleDirectory();
        AssetBundleGraphEditorWindow.BuildFromEditorScripts(BuildTarget.Android);
    }

    [MenuItem("BunnyTools/清除缓存的Bundle")]
    public static void ClearCachingBundles() {
        CachingClearEditor.ClearCachingBundles();
    }

    //[MenuItem("BunnyTools/导出资源表代码和Json")]
    //public static void GenerateAssetData() {
    //    AssetsGenerator.GenerateAssetData();
    //}

    //[MenuItem("BunnyTools/CSV转成CS类")]
    //public static void CSV2CSCode() {
    //    ConfigTableExportorEditor.CSV2CSCode();
    //}

    //[MenuItem("BunnyTools/导出CSV数据")]
    //public static void PackCSVData() {
    //    ConfigTableExportorEditor.PackCSVData();
    //}

    [MenuItem("BunnyTools/导出Bundle信息文件")]
    public static void ToGenerateBundleInfo() {
        GenerateBundleInfo.ToGenerateBundleInfo();
    }

}
