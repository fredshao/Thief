using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ExportVersionInfo {
    private static readonly string versionInfoPath = Application.dataPath + "/../Bundles/BundleSize.info";
    private static readonly string BundlePath = Application.dataPath + "/../Bundles/Windows"; 

    [MenuItem("Violet/Export Version Info")]
    private static void ToExportVersionInfo() {
        DirectoryInfo info = new DirectoryInfo(BundlePath);
        FileInfo [] files = info.GetFiles();

        BundleUpdateSizeInfo updateSizeInfo = new BundleUpdateSizeInfo();
        updateSizeInfo.bundleSizeDict = new Dictionary<string, long>();

        foreach(var file in files) {
            if (string.IsNullOrEmpty(file.Extension)) {
                
                Debug.Log(file.Name + "  " + file.Length);
                updateSizeInfo.bundleSizeDict.Add(file.Name, file.Length);
            }
        }

        VEncoder encoder = new VEncoder();

        string json = encoder.ObjectToJson(updateSizeInfo.bundleSizeDict);
        encoder.ObjectToJsonFile(versionInfoPath, updateSizeInfo.bundleSizeDict);

        Ulog.Log("Json", json);
    }
}

