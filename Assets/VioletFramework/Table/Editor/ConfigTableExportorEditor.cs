using UnityEngine;
using UnityEditor;
using System.IO;

public class ConfigTableExportorEditor : Editor {

    //[MenuItem("BunnyTools/CSV转成CS类")]
    public static void CSV2CSCode() {
        string csvTablePath = Application.dataPath + "/../../../Public/configTable/";
        string csCodePath = Application.dataPath + "/Scripts/Model/ConfigVO/";
        Debug.Log(csCodePath);
        if (!Directory.Exists(csCodePath)) {
            Directory.CreateDirectory(csCodePath);
        }
        ConfigTable.Editor.ConfigExportor.ExportVoCodeFromCSV("", csvTablePath, csCodePath);
        AssetDatabase.Refresh();
    }

    //[MenuItem("BunnyTools/导出CSV数据")]
    public static void PackCSVData() {
        string csvTablePath = Application.dataPath + "/../../../Public/configTable/";
        string csvDataPath = Application.dataPath + "/Res/ABRes/Data/Config.bytes";
        ConfigTable.Editor.ConfigExportor.ExportConfig(csvTablePath, csvDataPath);
        AssetDatabase.Refresh();
        Ulog.Log("配置表导出成功");
    }
}
