using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public class GenerateBundleInfo : Editor {
    private static string versionInfoPath = Application.dataPath + "/../Bundles/BundleSize.info";
    private static string bundlePath = Application.dataPath + "/../Bundles/";
    private static string targetBundlePath = Application.dataPath + "/../../../Update/AssetBundle/";
    public static BuildTarget buildTarget = BuildTarget.NoTarget;

    //[MenuItem("BunnyTools/导出Bundle信息文件")]
    public static void ToGenerateBundleInfo() {
        BundleInfoExportor exportor = new BundleInfoExportor();
        exportor.ExportVersionInfo(versionInfoPath, bundlePath);
    }

    //[MenuItem("BunnyTools/TestCopyFile")]
    public static void CopyBundle() {
        // 获取导出来的Bundle
        List<string> files = new List<string>();
        DirectoryInfo folder = new DirectoryInfo(bundlePath);
        foreach(FileInfo file in folder.GetFiles()) {
            if(string.IsNullOrEmpty(file.Extension) || file.Extension == ".info") {
                files.Add(file.FullName);
            }
        }

        // 清理目标Bundle目录(先删除，再重建)
        string bundleCopyToPath = targetBundlePath + GetBundleRootFolderNameByBuildTarget(buildTarget) + "/";
        if (Directory.Exists(bundleCopyToPath)) {
            Directory.Delete(bundleCopyToPath, true);
        }
        DirectoryInfo bundleCopyToDirInfo = Directory.CreateDirectory(bundleCopyToPath);
        
        // 将Bundle拷贝到目标Bundle
        foreach(string bundleFile in files) {
            string targetPath = bundleCopyToPath + System.IO.Path.GetFileName(bundleFile);
            File.Copy(bundleFile, targetPath);
        }

        if (HardwareUtil.IsWindows()) {
            Ulog.Log("Bundle 拷贝完成，可以提交");
            System.Diagnostics.Process.Start(@"C:\Windows\explorer.exe", bundleCopyToDirInfo.FullName);
        } else {
            EditorUtility.DisplayDialog("信息", "Bundle流程已完成，请手动拷贝导出的Bundle到提交目录，然后提交!","好的");
        }
    }

    private static string GetBundleRootFolderNameByBuildTarget(BuildTarget _buildTarget) {
        switch (_buildTarget) {
            case BuildTarget.Android: { return "Android"; }
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.StandaloneOSX:
                return "OSX";
        }
        return "NO_TARGET";
    }
}
