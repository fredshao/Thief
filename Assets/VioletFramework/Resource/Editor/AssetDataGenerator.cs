using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Violet.Resources;

namespace Violet.Editor.Resources {

    public class AssetDataGenerator {

        public void GenerateAssetData(string _csFileTargetPath, string _jsonFileTargetPath , string _resPath) {
            string[] searchPath = new string[] { _resPath };
            string[] prefabs = AssetDatabase.FindAssets("t:prefab t:texture2D t:material", searchPath);

            if(prefabs == null || prefabs.Length == 0) {
                Debug.LogError("没有扫描到 Prefab Texture2D Material");
                return;
            }

            AssetData assetData = new AssetData();
            //assetData.assetList = new AssetVO[prefabs.Length];
            List<AssetVO> assetList = new List<AssetVO>();


            for(int i = 0; i <prefabs.Length; ++i) {
                string assetPath = AssetDatabase.GUIDToAssetPath(prefabs[i]);
                string assetName = System.IO.Path.GetFileName(assetPath);
                string[] nameArr = assetName.Split('_');

                if(nameArr.Length < 2) {
                    continue;
                }

                if(int.TryParse(nameArr[0], out int id)) {
                    string extension = System.IO.Path.GetExtension(assetName);
                    EN_AssetType type = GetAssetTypeByExtension(extension);
                    AssetVO VO = new AssetVO(id, assetPath, assetName, type);
                    assetList.Add(VO);
                }
            }

            assetData.assetList = assetList.ToArray();

            if (CheckAsset(assetData)) {
                SaveAssetDataToJson(_jsonFileTargetPath, assetData);
                GenerateAssetDataCode(_csFileTargetPath, assetData);
            } else {
                Debug.LogError("资源检查未通过，生成数据失败!");
                return;
            }

            Debug.Log("资源Json和代码文件生成成功!");
        }

        public EN_AssetType GetAssetTypeByExtension(string _extension) {
            _extension = _extension.ToLower();
            if (_extension.Contains(".prefab")) {
                return EN_AssetType.Prefab;
            } else if (_extension.Contains(".mat")) {
                return EN_AssetType.Material;
            } else if (_extension.Contains(".png") ||
                      _extension.Contains(".jpg") ||
                      _extension.Contains(".jpeg") ||
                      _extension.Contains(".psd") ||
                      _extension.Contains(".tga")) {
                return EN_AssetType.Texture2D;
            }

            return EN_AssetType.Unknow;
        }


        private bool CheckAsset(AssetData _data) {
            Dictionary<int, AssetVO> assetDict = new Dictionary<int, AssetVO>();
            bool isOk = true;
            foreach(AssetVO vo in _data.assetList) {
                if(assetDict.ContainsKey(vo.assetId) == false) {
                    assetDict.Add(vo.assetId, vo);
                } else {
                    isOk = false;
                    Debug.LogError("资源ID冲突：" + vo.assetName + "  " + assetDict[vo.assetId].assetName);
                }
            }

            return isOk;
        }

        private void SaveAssetDataToJson(string _jsonFilePath, AssetData _data) {
            string json = JsonUtility.ToJson(_data);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(_jsonFilePath);
            writer.Write(json);
            writer.Close();
        }

        private void GenerateAssetDataCode(string _codeFilePath, AssetData _data) {
           // System.IO.FileStream fs = new System.IO.FileStream(_codeFilePath, System.IO.FileMode.OpenOrCreate);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(_codeFilePath);

            writer.WriteLine("public static class AssetConst {\n");
            for(int i = 0; i < _data.assetList.Length; ++i) {
                AssetVO asset = _data.assetList[i];
                string assetName = asset.assetName;

                string suffix = "";
                switch (asset.assetType) {
                    case EN_AssetType.Prefab: { suffix = "_prefab"; } break;
                    case EN_AssetType.Material: { suffix = "_mat"; } break;
                    case EN_AssetType.Texture2D: { suffix = "_img"; } break;
                }

                string assetNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(assetName).ToLower();
                assetNameWithoutExtension = assetNameWithoutExtension.Replace('.', '_');
                assetNameWithoutExtension = assetNameWithoutExtension.Replace('(', '_');
                assetNameWithoutExtension = assetNameWithoutExtension.Replace(')', '_');

                string propertyName = "ID_" + assetNameWithoutExtension + suffix;

                string code = "\tpublic static int " + propertyName + " = " + asset.assetId + ";";
                writer.WriteLine(code);
            }

            writer.Write("}");
            writer.Close();
        }
    }

   

   

  }