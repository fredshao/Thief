using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

/// <summary>
/// 编码器
/// </summary>
public class Encoder {

    /// <summary>
    /// Object 转成 Json 字符串
    /// </summary>
    /// <param name="_obj"></param>
    /// <returns></returns>
    public string ObjectToJson(object _obj) {
        return JsonConvert.SerializeObject(_obj);
    }

    /// <summary>
    /// Object 转成 Json 文件并存盘
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="_obj"></param>
    public void ObjectToJsonFile(string _path, object _obj) {
        string dir = System.IO.Path.GetPathRoot(_path);
        if(Directory.Exists(dir) == false) {
            Directory.CreateDirectory(dir);
        }

        string jsonStr = ObjectToJson(_obj);

        FileStream fs = new FileStream(_path, FileMode.Create);
        StreamWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8);
        writer.Write(jsonStr);
        writer.Close();
        fs.Close();
    }

    /// <summary>
    /// Json 字符串转成 Object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_json"></param>
    /// <returns></returns>
    public T JsonToObject<T>(string _json) {
        return JsonConvert.DeserializeObject<T>(_json);
    }

    /// <summary>
    /// 从文件中读取 Json 字符串并转成 Object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_path"></param>
    /// <returns></returns>
    public T JsonFileToObject<T>(string _path) {
        if(File.Exists(_path) == false) {
            Debug.LogError("Json文件不存在:" + _path);
            return default(T);
        }

        FileStream fs = new FileStream(_path, FileMode.Open);
        StreamReader reader = new StreamReader(fs);
        string jsonStr = reader.ReadToEnd();
        reader.Close();
        fs.Close();

        return JsonToObject<T>(jsonStr);
    }


}
