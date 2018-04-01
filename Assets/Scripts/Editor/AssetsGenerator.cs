using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Violet.Editor.Resources;
public class AssetsGenerator : Editor {
    //[MenuItem("BunnyTools/导出资源表代码和Json")]
    public static void GenerateAssetData() {
        AssetDataGenerator generator = new AssetDataGenerator();
        generator.GenerateAssetData("Assets/Scripts/Logic/Constants/AssetConst.cs", "Assets/Res/ABRes/Data/assetData.json","Assets/Res/ABRes");
        AssetDatabase.Refresh();
    }
}