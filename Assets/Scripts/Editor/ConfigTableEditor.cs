using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class ConfigTableEditor : Editor {
    /*
    [MenuItem("BunnyTools/生成CSV代码类")]
    public static void ToGenerateCSVCode() {
        if (!HardwareUtil.IsWindows()) {
            EditorUtility.DisplayDialog("Warning", "你的系统不是Windows，配表工具无法运行，请联系使用Windows的小伙伴忙吧", "好的");
            return;
        } else {
            GenerateCSVCode();
        }
    }

    public static void GenerateCSVCode() {
        string workDir = Application.dataPath + "/../TableTools/";
        Process proc = new Process();
        proc.StartInfo.WorkingDirectory = workDir;
        proc.StartInfo.FileName = "生成CSV类代码.bat";
        proc.Start();
        proc.WaitForExit();
    }

    [MenuItem("BunnyTools/打包CSV数据")]
    public static void ToPackageCSVData() {
        if (!HardwareUtil.IsWindows()) {
            EditorUtility.DisplayDialog("Warning", "你的系统不是Windows，配表工具无法运行，请联系使用Windows的小伙伴忙吧","好的");
            return;
        } else {
            PackageCSVData();
        }
    }

    public static void PackageCSVData() {
        string workDir = Application.dataPath + "/../TableTools/";
        Process proc = new Process();
        proc.StartInfo.WorkingDirectory = workDir;
        proc.StartInfo.FileName = "打包CSV数据.bat";
        proc.Start();
        proc.WaitForExit();
        Ulog.Log("配置表数据已打包!");
    }
    */

}
