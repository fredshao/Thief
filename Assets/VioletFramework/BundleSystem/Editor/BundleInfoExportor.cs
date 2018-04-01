using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class BundleInfoExportor {
    //private static readonly string versionInfoPath = Application.dataPath + "/../Bundles/BundleSize.info";
    //private static readonly string BundlePath = Application.dataPath + "/../Bundles/Windows"; 

    public void ExportVersionInfo(string _versionInfoPath, string _bundlePath) {
        string versionInfoPath = _versionInfoPath;
        string BundlePath = _bundlePath;

        DirectoryInfo info = new DirectoryInfo(BundlePath);
        FileInfo [] files = info.GetFiles();

        BundleUpdateSizeInfo updateSizeInfo = new BundleUpdateSizeInfo();
        updateSizeInfo.bundleSizeDict = new Dictionary<string, long>();

        foreach(var file in files) {
            if (string.IsNullOrEmpty(file.Extension)) {
                
                Ulog.Log("Bundle:", file.Name, "  Size:", file.Length, "B");
                updateSizeInfo.bundleSizeDict.Add(file.Name, file.Length);
            }
        }

        Encoder encoder = new Encoder();

        string json = encoder.ObjectToJson(updateSizeInfo.bundleSizeDict);
        encoder.ObjectToJsonFile(versionInfoPath, updateSizeInfo.bundleSizeDict);
        Debug.Log("Bundle信息文件已导出");
        AssetDatabase.Refresh();
    }
}

