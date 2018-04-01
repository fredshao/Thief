using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AssetBundles.GraphTool;

public class BuildBundleEditor : Editor { 
    //[MenuItem("BunnyTools/执行完整Bundle打包流程")]
    public static void Build() {
        AssetBundleGraphEditorWindow.BuildFromMenu();
    }
}
