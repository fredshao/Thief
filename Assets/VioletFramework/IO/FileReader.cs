using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class FileReader {
    
    /// <summary>
    /// 同步从 StreamingAssets 目录读取文本文件
    /// </summary>
    /// <param name="_relativePath">相对路径，例如 StreaingAssets/config/data.txt 的相对路径为 config/data.txt </param>
    /// <returns></returns>
    public string ReadTextFileFromStreamingAssetPath(string _relativePath) {
        string path = String.Empty;
        if(Application.platform == RuntimePlatform.Android) {
            path = PlatformUtil.GetStreamingAssetPathForWWW() + "/" + _relativePath;
            WWW www = new WWW(path);
            while (!www.isDone) { }

            if(www.error != null) {
                Ulog.LogError(this, "文件读取失败: ", path);
                return String.Empty;
            }
            return www.text;
        } else {
            path = PlatformUtil.GetStreamingAssetPathForIO();
            if (!File.Exists(path)) {
                Ulog.LogError(this, "要读取的文件不存在: ", path);
                return String.Empty;
            } else {
                StreamReader reader = new StreamReader(path);
                if(reader == null) {
                    Ulog.LogError(this, "无法读取文件: ", path);
                    return String.Empty;
                }

                string text = reader.ReadToEnd();
                reader.Close();
                return text;
            }
        }
    }

    /// <summary>
    /// 异步从 StreamingAssets 目录读取文本
    /// </summary>
    /// <param name="_relativePath">相对路径，例如 StreaingAssets/config/data.txt 的相对路径为 config/data.txt </param>
    /// <param name="_callback">读完回调</param>
    /// <returns></returns>
    public void ReadTextFileFromStreamingAssetPathAsync(string _relativePath, Action<string> _callback) {
        string path = String.Empty;
        if(Application.platform == RuntimePlatform.Android) {
            path = PlatformUtil.GetStreamingAssetPathForWWW() + "/" + _relativePath;
            V.Instance.StartCoroutine(ReadyTextFileAsyncForAndroid(path, _callback));
        } else {
            path = PlatformUtil.GetStreamingAssetPathForUnityWebRequest() + "/" + _relativePath;
            ReadTextFileAsyncForOtherPlatFormExceptAndroid(path, _callback);
        }
    }

    /// <summary>
    /// 同步从 StreaingAssets 目录读取二进制文件
    /// </summary>
    /// <param name="_relativePath">相对路径，例如 StreaingAssets/config/data.txt 的相对路径为 config/data.txt</param>
    /// <returns></returns>
    public byte[] ReadBinaryFileFromStreamingAssetPath(string _relativePath) {
        string path = String.Empty;
        if (Application.platform == RuntimePlatform.Android) {
            path = PlatformUtil.GetStreamingAssetPathForWWW() + "/" + _relativePath;
            WWW www = new WWW(path);
            while (!www.isDone) { }

            if (www.error != null) {
                Ulog.LogError(this, "文件读取失败: ", path);
                return null;
            }
            return www.bytes;
        } else {
            path = PlatformUtil.GetStreamingAssetPathForIO();
            if (!File.Exists(path)) {
                Ulog.LogError(this, "要读取的文件不存在: ", path);
                return null;
            } else {
                byte[] bytes = null;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                    using (BinaryReader reader = new BinaryReader(fs)) {
                        bytes = new byte[fs.Length];
                        reader.Read(bytes, 0, bytes.Length);
                        reader.Close();
                        fs.Close();
                    }
                }
                return bytes;
            }
        }
    }

    /// <summary>
    /// 异步从 StreamingAssets 目录读取二进制文件
    /// </summary>
    /// <param name="_realtivePath">相对路径，例如 StreaingAssets/config/data.txt 的相对路径为 config/data.txt</param>
    /// <param name="_callback">读完回调</param>
    /// <returns></returns>
    public void ReadBinaryFileFromStreamingAssetPathAsync(string _relativePath, Action<byte[]> _callback) {
        string path = "";
        if(Application.platform == RuntimePlatform.Android) {
            path = PlatformUtil.GetStreamingAssetPathForWWW() + "/" + _relativePath;
            ReadyBinaryFileAsyncForAndroid(path, _callback);
        } else {
            path = PlatformUtil.GetStreamingAssetPathForUnityWebRequest() + "/" + _relativePath;
            ReadBinaryFileAsyncForOtherPlatFormExceptAndroid(path, _callback);
        }
    }

    /// <summary>
    /// 同步读取文本文件
    /// </summary>
    /// <param name="_absolutePath">绝对路径 例如： Application.persistentPath/aaa.txt , D:\Test\aaa.txt </param>
    /// <returns></returns>
    public string ReadTextFile(string _absolutePath) {
        return String.Empty;
    }

    /// <summary>
    /// 异步读取文本文件
    /// </summary>
    /// <param name="_absolutePath">绝对路径 例如： Application.persistentPath/aaa.txt , D:\Test\aaa.txt </param>
    /// <param name="_callback">读完回调</param>
    public void ReadTextFileAsync(string _absolutePath, Action<string> _callback) {

    }

    /// <summary>
    /// 同步读取二进制文件
    /// </summary>
    /// <param name="_absolutePath">绝对路径 例如： Application.persistentPath/aaa.txt , D:\Test\aaa.txt </param>
    /// <returns></returns>
    public byte[] ReadBinaryFile(string _absolutePath) {
        return null;
    }

    /// <summary>
    /// 异步读取二进制文件
    /// </summary>
    /// <param name="_absolutePath">绝对路径 例如： Application.persistentPath/aaa.txt , D:\Test\aaa.txt </param>
    /// <param name="_callback">读完回调</param>
    public void ReadBinaryFileAsync(string _absolutePath, Action<byte[]> _callback) {

    }


    /// <summary>
    ///  为Android平台异步从 StreaingAssets 目录读取文件
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private IEnumerator<WWW> ReadyTextFileAsyncForAndroid(string _path, Action<string> _callback) {
        WWW www = new WWW(_path);
        yield return www;
        if (www.error != null) {
            Ulog.LogError(this, "异步读取文件失败: " + _path + "  Error:" + www.error);
            if (_callback != null) {
                _callback(String.Empty);
            }
        } else {
            string text = www.text;
            if (_callback != null) {
                _callback(text);
            }
        }
    }

    /// <summary>
    /// 为除 Android 之外的其他平台异步从 StreamingAssets 目录读取文件
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="_callback"></param>
    private async void ReadTextFileAsyncForOtherPlatFormExceptAndroid(string _path, Action<string> _callback) {
        UnityWebRequest request = UnityWebRequest.Get(_path);
        AsyncOperation asyncOp = request.Send();
        while (!asyncOp.isDone) {
            await 0;
        }

        if (request.isNetworkError) {
            Ulog.LogError(this, "异步读取文件失败: " + _path + "  Error:" + request.error);
            if (_callback != null) {
                _callback(String.Empty);
            }
        } else {
            string text = request.downloadHandler.text;
            if (_callback != null) {
                _callback(text);
            }
        }
    }

    /// <summary>
    ///  为Android平台异步从 StreaingAssets 目录读取文件
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private IEnumerator<WWW> ReadyBinaryFileAsyncForAndroid(string _path, Action<byte[]> _callback) {
        WWW www = new WWW(_path);
        yield return www;
        if (www.error != null) {
            Ulog.LogError(this, "异步读取文件失败: " + _path + "  Error:" + www.error);
            if (_callback != null) {
                _callback(null);
            }
        } else {
            if (_callback != null) {
                _callback(www.bytes);
            }
        }
    }

    /// <summary>
    /// 为除 Android 之外的其他平台异步从 StreamingAssets 目录读取文件
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="_callback"></param>
    private async void ReadBinaryFileAsyncForOtherPlatFormExceptAndroid(string _path, Action<byte[]> _callback) {
        UnityWebRequest request = UnityWebRequest.Get(_path);
        AsyncOperation asyncOp = request.Send();
        while (!asyncOp.isDone) {
            await 0;
        }

        if (request.isNetworkError) {
            Ulog.LogError(this, "异步读取文件失败: " + _path + "  Error:" + request.error);
            if (_callback != null) {
                _callback(null);
            }
        } else {
            byte[] data = request.downloadHandler.data;
            if (_callback != null) {
                _callback(data);
            }
        }
    }


}
