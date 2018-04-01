using FunEngine.Core;
using FunEngine.Crypto;
using FunEngine.Encoding;
using FunEngine.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class ConfigManager {
    private static Dictionary<string, Dictionary<int, object>> _VOMap = new Dictionary<string, Dictionary<int, object>>();

    /*
    /// <summary>
    /// 同步加载所有CSV数据
    /// </summary>
    /// <param name="confVoPackageName">Config文件命名空间</param>
    /// <param name="path">数据文件Config.conf的父目录，例如: Application.streamingAssetPath </param>
    /// <param name="encrypted">导出数据的时候是否加密过</param>
    public void LoadConfigData(string _confVoPackageName, string _path, bool _encrypted = false) {
        byte[] confBytes = FileUtils.OpenBinaryFile(_path);
        if (_encrypted) {
            confBytes = DES.DecryptBytes("-0q,cfma", "qp[qomvma'u'cf", confBytes);
        }
        using (ByteArray byteArr = new ByteArray(confBytes)) {
            while (byteArr.bytesAvaliable > 0)
                InitializeCSV(_confVoPackageName, byteArr.ReadUTF());
        }
    }
    */

    /*
    /// <summary>
    /// 异步加载配置文件
    /// </summary>
    /// <param name="_confVoPackageName">VO类命名空间</param>
    /// <param name="_path">Config.conf的异步加载路径</param>
    /// <param name="_encrypted">导出的时候是否是加密过的</param>
    /// <returns></returns>
    public async Task LoadTablesAsync(string _path, bool _encrypted = false) {

        string fullPath = _path;
        UnityWebRequest request = UnityWebRequest.Get(fullPath);
        await request.Send();
       // await request.Send();
        if (request.isError) {

        } else {
            byte[] confBytes = request.downloadHandler.data;
            if (_encrypted) {
                confBytes = DES.DecryptBytes("-0q,cfma", "qp[qomvma'u'cf", confBytes);
            }
            using (ByteArray byteArr = new ByteArray(confBytes)) {
                while (byteArr.bytesAvaliable > 0)
                    InitializeCSV(_confVoPackageName, byteArr.ReadUTF());
            }
        }
    }

    public async Task LoadTablesAsync(byte[] _data, bool _encrypted = false) {
        await 0;

        using (ByteArray byteArr = new ByteArray(_data)) {
            while (byteArr.bytesAvaliable > 0)
                InitializeCSV(_confVoPackageName, byteArr.ReadUTF());
        }
    }



    /// <summary>
    /// 从二进制数据加载配置数据，用于打包成AssetBundle
    /// </summary>
    /// <param name="_data"></param>
    /// <param name="_encrypted"></param>
    public void LoadConfigDataFromByteArray(byte[] _data) {
        byte[] confBytes = _data;
        using (ByteArray byteArr = new ByteArray(confBytes)) {
            while (byteArr.bytesAvaliable > 0)
                InitializeCSV(_confVoPackageName, byteArr.ReadUTF());
        }
    }
    */

    /// <summary>
    /// 异步加载配置文件
    /// </summary>
    /// <param name="_confVoPackageName">VO类命名空间</param>
    /// <param name="_path">Config.conf的异步加载路径</param>
    /// <param name="_encrypted">导出的时候是否是加密过的</param>
    /// <returns></returns>
    public async Task LoadTablesAsync(string _path) {

        string fullPath = _path;
        UnityWebRequest request = UnityWebRequest.Get(fullPath);
        UnityEngine.AsyncOperation asyncOp = request.Send();

        while (!asyncOp.isDone) {
            await 0;
        }

        // await request.Send();
        if (request.isNetworkError) {
            UnityEngine.Debug.Log("读表出错:" + _path);
        } else {
            byte[] confBytes = request.downloadHandler.data;
            await LoadTablesAsync(confBytes);
        }
    }

    public async Task LoadTablesAsync(byte[] _tableData) {
        using (ByteArray byteArr = new ByteArray(_tableData)) {
            while(byteArr.bytesAvaliable > 0) {
                await 0;
                InitializeCSV(byteArr.ReadUTF());
            }
        }
    }


    /// <summary>
    /// 通过ID获取某一类中的一个VO
    /// 查询配置表的指定纪录 select * from TClass where id = id
    /// </summary>
    /// <typeparam name="TClass">VO类型</typeparam>
    /// <param name="id">ID</param>
    /// <returns></returns>
    public TClass GetConfVO<TClass>(int id) where TClass : AbstractConfVO {
        Type type = typeof(TClass);
        Dictionary<int, object> table;
        object data;
        if (ConfigManager._VOMap.TryGetValue(type.FullName, out table) && table.TryGetValue(id, out data)) {
            //如果是实例对象，直接返回
            if (data is AbstractConfVO) {
                return (TClass)data;
            } else {
                object raw = Activator.CreateInstance(type);
                fill(raw, table, (string[])data);
                TClass vo = (TClass)raw;
                table[vo.id] = vo;
                return vo;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取某一类型所有的ConfigVO
    /// </summary>
    /// <typeparam name="TClass">VO类型，例如ArmyConfigVO</typeparam>
    /// <returns></returns>
    public TClass[] GetConfVOList<TClass>() where TClass : AbstractConfVO {
        Type type = typeof(TClass);
        Dictionary<int, object> table;
        if (ConfigManager._VOMap.TryGetValue(type.FullName, out table)) {
            //如果已经缓存过列表
            if (table.ContainsKey(-10))
                return (TClass[])table[-10];

            //创建缓存表
            List<TClass> list = new List<TClass>();
            List<TClass> newVoList = null;
            foreach (KeyValuePair<int, object> pair in table) {
                if (pair.Key >= 0) {
                    if (pair.Value is AbstractConfVO) {
                        list.Add((TClass)pair.Value);
                    } else {
                        object raw = Activator.CreateInstance(type);
                        fill(raw, table, (string[])pair.Value);
                        TClass vo = (TClass)raw;

                        list.Add(vo);
                        if (newVoList == null)
                            newVoList = new List<TClass>();
                        newVoList.Add(vo);
                    }
                }
            }
            //将新创建的对象添加入表
            if (newVoList != null && newVoList.Count > 0) {
                int i = 0;
                while (i < newVoList.Count) {
                    TClass vo = newVoList[i];
                    table[vo.id] = vo;
                    i++;
                }
            }
            //缓存一下这个列表
            TClass[] returnObj = list.ToArray();
            table[-10] = returnObj;
            return returnObj;
        }
        return null;
    }


    /// <summary>
    /// 使用指定参数获取VO集合，
    /// 例如 GetConfVoList<ArmyConfigVO>("armyName","laserTank","armyIcon","normalTank");
    /// 则会获取所有armyName == laserTank, 或者 armyIcon == normalTank的VO
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="args"></param>
    /// <returns></returns>
    public TClass[] GetConfVOList<TClass>(params object[] args) where TClass : AbstractConfVO {
        Type type = typeof(TClass);
        TClass[] voList = GetConfVOList<TClass>();
        List<TClass> list = new List<TClass>();
        //遍历属性表中的属性
        int i = 0;
        while (i < voList.Length) {
            TClass vo = voList[i++];
            //
            int k = 0;
            while (k < args.Length) {
                string attrName = (string)args[k++];
                object value = args[k++];
                //判断属性与值是否相等
                PropertyInfo pInfo = type.GetProperty(attrName);
                if (pInfo != null) {
                    object currentValue = pInfo.GetValue(vo, null);
                    if (currentValue.Equals(value) || ReferenceEquals(currentValue, value)) {
                        list.Add(vo);
                        break;
                    }
                }
            }
        }
        return list.ToArray();
    }

    /// <summary>
    /// 获取满足条件的行，And
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="args"></param>
    /// <returns></returns>
    public TClass[] GetConfVOListAnd<TClass>(params object[] args) where TClass : AbstractConfVO {
        Type type = typeof(TClass);
        TClass[] voList = GetConfVOList<TClass>();
        List<TClass> list = new List<TClass>();
        //遍历属性表中的属性
        int i = 0;
        while (i < voList.Length) {
            TClass vo = voList[i++];
            //
            int k = 0;
            bool selected = true;
            while (k < args.Length) {
                string attrName = (string)args[k++];
                object value = args[k++];
                //判断属性与值是否相等
                PropertyInfo pInfo = type.GetProperty(attrName);
                if (pInfo != null) {
                    object currentValue = pInfo.GetValue(vo, null);

                    if(!currentValue.Equals(value)) {
                        selected = false;
                        break;
                    }
                }
            }

            if (selected) {
                list.Add(vo);
            }

        }
        return list.ToArray();
    }


    // --------------------------- 内部使用 -------------------------------

    private void InitializeCSV(string csv) {

        //读取csv格式，转变为二维数组
        List<string[]> list = CSV.Read(csv);

        list.RemoveAt(0);
        //UnityEngine.Debug.Log("[0]:" + list[0][0]);

        string className = "";
        
        className = list[0][0];

        UnityEngine.Debug.Log("读取表:" + className);

        //string 
        Dictionary<int, object> table = null;

        if (ConfigManager._VOMap.ContainsKey(className)) {
            table = ConfigManager._VOMap[className];
        } else {
            table = new Dictionary<int, object>();
            //缓存类型
            table[-1] = list[0];
            //属性表
            table[-2] = list[2];
            ConfigManager._VOMap[className] = table;
        }

        //数据
        for (int i = 4, row = list.Count; i < row; i++) {
            if (!String.IsNullOrEmpty(list[i][0])) {
                int id = Convert.ToInt32(list[i][0]);
                if (!table.ContainsKey(id)) {
                    table[id] = list[i];
                } else {
                    UnityEngine.Debug.LogError(typeof(ConfigManager) + "  " +  className +  " 存在相同ID的数据:" + id);
                }
            }
        }
        //FLog.Info(typeof(ConfigManager), "写入 " + className + " 配置表");
    }


    //填充csv数据
    private void fill(object vo, Dictionary<int, object> table, string[] data) {
        string[] classDefs = (string[])table[-1];
        string[] attrNames = (string[])table[-2];

        int i = 0;
        while (i < attrNames.Length) {
            string attrName = attrNames[i];
            if (String.IsNullOrEmpty(attrName) || attrName == "\r" || attrName == "argName") {
                i++;
                continue;
            }

            //1.普通属性 2.普通属性数组 3.外键属性 4.外键属性数组 5.内部类 6.内部类数组
            int codeType = getCodeType(classDefs[i], attrName);
            //普通属性
            if (codeType == 1) {
                setValue(vo, attrName, data[i]);
                i++;
            }
            //普通属性数组
            else if (codeType == 2) {
                string arrAttrName = "";
                int arrIndex = 0;
                getArrAttrNameAndIndex(attrName, ref arrAttrName, ref arrIndex);
                addValue(vo, arrAttrName, arrIndex, data[i]);
                i++;
            }
            //外键属性
            else if (codeType == 3) {
                setValue(vo, attrName, data[i]);
                i++;
            }
            //外键属性数组
            else if (codeType == 4) {
                string arrAttrName = "";
                int arrIndex = 0;
                getArrAttrNameAndIndex(attrName, ref arrAttrName, ref arrIndex);
                //创建外部属性
                addValue(vo, arrAttrName, arrIndex, data[i]);
                i++;
            }
            //内部类
            else if (codeType == 5) {
                string innerClassName = classDefs[i];
                string innerAttrName = attrName.Split('.')[0];
                object innerVo = ObjectUtils.CreateInstance(vo.GetType().FullName + "+" + innerClassName);
                setInnerVoValue(innerVo, innerClassName, innerAttrName, ref i, ref classDefs, ref attrNames, ref data);
                setValue(vo, innerAttrName, innerVo);
            }
            //内部类数组
            else if (codeType == 6) {
                string innerClassName = classDefs[i];
                string innerArrAttrName = "";
                int innerArrIndex = 0;
                getArrAttrNameAndIndex(attrName, ref innerArrAttrName, ref innerArrIndex);
                string innerAttrName2 = attrName.Split('.')[0];
                object innerVo = ObjectUtils.CreateInstance(vo.GetType().FullName + "+" + innerClassName);
                setInnerVoValue(innerVo, innerClassName, innerAttrName2, ref i, ref classDefs, ref attrNames, ref data);
                addValue(vo, innerArrAttrName, innerArrIndex, innerVo);
            } else {
                i++;
            }
        }

        //
        if (vo is AbstractConfVO)
            ((AbstractConfVO)vo).OnData(data);
    }


    //1.普通属性 2.普通属性数组 3.外键属性 4.外键属性数组 5.内部类 6.内部类数组
    private int getCodeType(string classDef, string attrName) {
        bool isArr = attrName.IndexOf('[') > 0;
        if (attrName.IndexOf('.') > 0)
            return isArr ? 6 : 5;
        return String.IsNullOrEmpty(classDef) ? (isArr ? 2 : 1) : (isArr ? 4 : 3);
    }


    //设置对象属性
    private void setValue(object vo, string attrName, object value) {
        PropertyInfo pInfo = vo.GetType().GetProperty(attrName);
        if (pInfo != null) {
            pInfo.SetValue(vo, Convert.ChangeType(value, pInfo.PropertyType), null);
        } else {
            throw new Exception(vo.GetType() + "对象，没有名为 " + attrName + " 的属性");
        }
    }


    //获取数组属性
    private void getArrAttrNameAndIndex(string attrName, ref string arrAttrName, ref int arrIndex) {
        if (attrName.IndexOf('[') < 0) {
            arrAttrName = attrName;
            arrIndex = -1;
        } else {
            string[] temp = attrName.Split('[');
            arrAttrName = temp[0];
            //
            string tempStr = temp[1].Split(']')[0];
            arrIndex = String.IsNullOrEmpty(tempStr) ? 0 : Convert.ToInt32(tempStr);
        }
    }


    //添加数组属性
    private void addValue(object vo, string arrAttrName, int arrIndex, object value) {
        string methodName = "AddTo" + upperFirstLetter(arrAttrName);
        //Debug.Log(methodName + "  " + arrIndex + "   " + value);
        MethodInfo mInfo = vo.GetType().GetMethod(methodName);
        if (mInfo != null) {
            //获取每个属性的类型
            ParameterInfo[] pars = mInfo.GetParameters();
            //Debug.Log(methodName + " " + arrIndex + "  " + value);
            mInfo.Invoke(vo, new object[] { arrIndex, Convert.ChangeType(value, pars[1].ParameterType) });
        } else {
            throw new Exception(vo.GetType() + "对象，没有名为 " + methodName + " 的方法");
        }
    }


    private string upperFirstLetter(string str) {
        return Char.ToUpper(str[0]) + str.Substring(1, str.Length - 1);
    }


    //设置内部类的属性
    private void setInnerVoValue(object innerVo,
                                        string innerClassName,
                                        string mark,
                                        ref int i, ref string[] classDefs, ref string[] attrNames, ref string[] data) {
        while (i < attrNames.Length) {
            if (attrNames[i].IndexOf(mark) < 0) {
                break;
            }
            //
            string attrName = attrNames[i].Split('.')[1];
            //1.普通属性 2.普通属性数组 3.外键属性 4.外键属性数组 5.内部类 6.内部类数组
            int codeType = 1;
            if (innerClassName != classDefs[i])
                codeType = getCodeType(classDefs[i], attrName);
            //普通属性
            if (codeType == 1) {
                setValue(innerVo, attrName, data[i]);
                i++;
            }
            //普通属性数组
            else if (codeType == 2) {
                string arrAttrName = "";
                int arrIndex = 0;
                getArrAttrNameAndIndex(attrName, ref arrAttrName, ref arrIndex);
                addValue(innerVo, arrAttrName, arrIndex, data[i]);
                i++;
            }
            //外键属性
            else if (codeType == 3) {
                setValue(innerVo, attrName, data[i]);
                i++;
            }
            //外键属性数组
            else if (codeType == 4) {
                string arrAttrName = "";
                int arrIndex = 0;
                getArrAttrNameAndIndex(attrName, ref arrAttrName, ref arrIndex);
                //创建外部属性
                addValue(innerVo, arrAttrName, arrIndex, data[i]);
                i++;
            } else {
                i++;
            }
        }
    }


}
       

public class AwaitAsyncOperation : UnityEngine.AsyncOperation {
    public AwaitAsyncOperation GetAwaiter() {
        return this;
    }
}