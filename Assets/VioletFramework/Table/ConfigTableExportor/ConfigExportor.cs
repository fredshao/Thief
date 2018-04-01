using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigTable.Editor {
    //值对象类导出器
    public class ConfigExportor
    {
        private static string _packageName = "Fun.Data.Config";
        private static string _sourcePath = "";
        private static string _targetPath = "";
        private static string _innerClassCodeSpliter = "//#InnerClassCode - created by ConfVoExportor script#\n";
        private static string _disposeCodeSpliter = "//#DisposeCode - created by ConfVoExportor script#\n";
        private static string _attrCodeSpliter = "//#AttrCode - created by ConfVoExportor script#\n";



        /// <summary>
        /// 将所有CSV数据打包成一个二进制文件
        /// </summary>
        /// <param name="encrypt">是否加密数据</param>
        public static void ExportConfig(string _csvSourcePath, string _configFilePath, bool encrypt = false)
        {
            //AssetDatabase.Refresh();
            string fullSourcePath = _csvSourcePath;
            string fullTargetPath = _configFilePath;

            ByteArray bytes = new ByteArray();
            FileInfo[] fileList = FileUtils.GetFilesInDirectory(fullSourcePath, "*.csv", SearchOption.AllDirectories);
            for (int i = 0, total = fileList.Length; i < total; i++)
            {
                string txt = FileUtils.OpenTxtFile(fileList[i].FullName,System.Text.Encoding.GetEncoding("gb2312"));
                if (txt.StartsWith("server", StringComparison.CurrentCultureIgnoreCase))
                {
                    System.Console.WriteLine("xxxxxxxxxxxxxxxxx> Ignore Server Table:" + fileList[i].Name); 
                }
                else
                {
                    System.Console.WriteLine("=================> Write Table:" + fileList[i].Name);
                    bytes.WriteUTF(txt);
                }
                    
            }
            FileUtils.WriteBinaryFile(bytes.bytes, fullTargetPath);
        }


        /// <summary>
        /// 根据csv文件导出C#代码
        /// </summary>
        /// <param name="packageName">命名空间</param>
        /// <param name="sourcePath">CSV文件的路径</param>
        /// <param name="targetPath">导出的C# VO类存放路径</param>
        public static void ExportVoCodeFromCSV(string packageName,
                                               string sourcePath,
                                               string targetPath)
        {
            //设置变量
            _packageName = packageName;
            _sourcePath = sourcePath;
            _targetPath = targetPath;

            //读取每一个csv文件并生成代码
            //FileInfo[] csvList = FileUtils.GetFilesInDirectory(_sourcePath, "*.csv", SearchOption.TopDirectoryOnly);
            FileInfo[] csvList = FileUtils.GetFilesInDirectory(_sourcePath, "*.csv", SearchOption.AllDirectories);
            for (int i = 0, total = csvList.Length; i < total; i++)
            {
                string csv = FileUtils.OpenTxtFile(csvList[i].FullName,System.Text.Encoding.GetEncoding("gb2312"));
                List<string[]> datas = CSV.Read(csv);
                if (datas[0][0].StartsWith("server", StringComparison.CurrentCultureIgnoreCase))
                {
                    System.Console.WriteLine("xxxxxxxxxxxxxx> Ignore Server Table:" + csvList[i].Name);
                }
                else
                {
                    System.Console.WriteLine("读取文件:" + csvList[i].Name);
                    datas.RemoveAt(0);
                    exportCode(ref datas);
                }
            }
            //FLog.Info(typeof(ConfigExportor), "值对象代码生成完毕");
            System.Console.WriteLine("值对象代码生成完毕");
        }




        // -------------------------- 内部使用 -------------------------

        //导出值对象的代码
        private static void exportCode(ref List<string[]> datas)
        {
            //类定义
            string[] classDefs = datas[0];
            //注释
            string[] comments = datas[1];
            //属性名
            string[] attrNames = datas[2];
            //属性类型
            string[] attrTypes = datas[3];

            //---------------------------------
            //类名
            string className = classDefs[0];
            //类文件名
            string classPath = className + ".cs";
            //类文件完整路径
            string classFullPath = _targetPath + "/" + classPath;

            System.Console.WriteLine("准备写入值对象代码：" + classFullPath);

            //类代码
            string code = "";
            if (!FileUtils.FileExists(classFullPath))
            {
                code = getCodeTemplate(className);
            }
            else
            {
                code = FileUtils.OpenTxtFile(classFullPath);
            }

            //生成代码
            string innerClassCode = "";
            string disposeCode = "";
            string attrCode = "";
            Dictionary<string, bool> classMap = new Dictionary<string, bool>();
            //遍历所有的属性
            int i = 0;
            while (i < attrNames.Length)
            {
                string attrName = attrNames[i];
                if (String.IsNullOrEmpty(attrName) || attrName == "\r" || attrName == "id" || attrName == "name" || attrName == "description" || attrName == "resID" || attrName == "argName")
                {
                    i++;
                    continue;
                }

                //1.普通属性 2.普通属性数组 3.外键属性 4.外键属性数组 5.内部类 6.内部类数组
                int codeType = getCodeType(classDefs[i], attrName);
                //普通属性
                if (codeType == 1)
                {
                    setAttrCode(ref attrCode, comments[i], attrTypes[i], attrName);
                    i++;
                }
                //普通属性数组
                else if (codeType == 2)
                {
                    string arrAttrName = attrName.Split('[')[0];
                    setArrAttrCode(ref attrCode, ref disposeCode, comments[i], attrTypes[i], arrAttrName);
                    //跳过相同的变量
                    i++;
                    skipSameAttr(ref i, arrAttrName, ref attrNames);
                }
                //外键属性
                else if (codeType == 3)
                {
                    setRefAttrCode(ref attrCode, ref disposeCode, comments[i], attrTypes[i], classDefs[i], attrName);
                    i++;
                }
                //外键属性数组
                else if (codeType == 4)
                {
                    string arrAttrName = attrName.Split('[')[0];
                    setRefArrAttrCode(ref attrCode, ref disposeCode, comments[i], attrTypes[i], classDefs[i], arrAttrName);
                    //跳过相同的变量
                    i++;
                    skipSameAttr(ref i, arrAttrName, ref attrNames);
                }
                //内部类或者内部类数组
                else if (codeType == 5)
                {
                    //内部类名称
                    string innerClassName = classDefs[i];
                    string innerAttrName  = attrName.Split('.')[0];
                    setInnerClassAttrCode(ref attrCode, ref disposeCode, comments[i], innerClassName, innerAttrName);

                    //如果未定义过内部类
                    if (!classMap.ContainsKey(innerClassName))
                    {
                        classMap[innerClassName] = true;
                        string innerCode = getInnerClassCodeTemplate(innerClassName);
                        string innerDisposeCode = "";
                        string innerAttrCode = "";
                        setInnerClassCode(innerClassName, innerAttrName,
                                          ref innerAttrCode,
                                          ref innerDisposeCode,
                                          ref i, ref classDefs, ref comments, ref attrNames, ref attrTypes);
                        innerCode = combineCode(innerCode, _disposeCodeSpliter, innerDisposeCode, false, "");
                        innerCode = combineCode(innerCode, _attrCodeSpliter, innerAttrCode, false, "");
                        innerClassCode += innerCode;
                    }
                    else
                    {
                        i++;
                        skipSameAttr(ref i, innerAttrName, ref attrNames);
                    }
                }
                //内部类或者内部类数组
                else if (codeType == 6)
                {
                    //内部类名称
                    string innerClassName = classDefs[i];
                    string innerAttrName = attrName.Split('[')[0];
                    string innerAttrName2 = attrName.Split('.')[0];
                    setArrAttrCode(ref attrCode, ref disposeCode, comments[i], innerClassName, innerAttrName);

                    //如果未定义过内部类
                    if (!classMap.ContainsKey(innerClassName))
                    {
                        classMap[innerClassName] = true;
                        string innerCode = getInnerClassCodeTemplate(innerClassName);
                        string innerDisposeCode = "";
                        string innerAttrCode = "";
                        setInnerClassCode(innerClassName, innerAttrName2,
                                          ref innerAttrCode,
                                          ref innerDisposeCode,
                                          ref i, ref classDefs, ref comments, ref attrNames, ref attrTypes);
                        innerCode = combineCode(innerCode, _disposeCodeSpliter, innerDisposeCode, false, "");
                        innerCode = combineCode(innerCode, _attrCodeSpliter, innerAttrCode, false, "");
                        innerClassCode += innerCode;
                    }
                    else
                    {
                        i++;
                    }
                    skipSameAttr(ref i, innerAttrName, ref attrNames);
                }
                else
                {
                    i++;
                }
            }

            //如果是枚举类,生成枚举类代码
            if (className.IndexOf("Enum") > 0)
            {
                string enumCode = "";
                setEnumCode(className, ref enumCode, ref datas);
                innerClassCode = enumCode + innerClassCode;
            }


            //替换代码
            code = combineCode(code, _innerClassCodeSpliter, innerClassCode, true, "\t\t");
            code = combineCode(code, _disposeCodeSpliter, disposeCode, true, "\t\t\t");
            code = combineCode(code, _attrCodeSpliter, attrCode, true, "\t\t");

            FileUtils.WriteTxtFile(code, classFullPath,System.Text.Encoding.UTF8);
            //FLog.Info(typeof(ConfigExportor), "写入值对象代码:", classFullPath);
            System.Console.WriteLine("写入值对象代码成功：" + classFullPath);
        }

        //获取类的类型
        private static string lowerFirst(string str)
        {
            return Char.ToLower(str[0]) + str.Substring(1, str.Length - 1);
        }

        private static string upperFirst(string str)
        {
            return Char.ToUpper(str[0]) + str.Substring(1, str.Length - 1);
        }

        //跳过相同的字段
        private static void skipSameAttr(ref int i, string attrName, ref string[] attrNames)
        {
            while (i < attrNames.Length)
            {
                if (attrNames[i].IndexOf(attrName) < 0)
                    break;
                else
                    i++;
            }
        }

        //替换代码
        private static string combineCode(string code, string spliter, string content, bool needSpliter, string tab)
        {
            string[] temp = code.Split(new string[1] { spliter }, StringSplitOptions.None);
            // if (temp.Length < 3)
            // {
            //     Debug.Log(code);
            //     Debug.Log(spliter);
            //     Debug.Log(content);
            //     Debug.Log(temp.Length);
            // }

            code = temp[0];
            if (needSpliter)
                code += spliter;
            code += content;
            if (needSpliter)
                code += tab + spliter;
            code += temp[2];
            return code;
        }

        //-------------------------------------------------------------------------------
        //1.普通属性 2.普通属性数组 3.外键属性 4.外键属性数组 5.内部类 6.内部类数组
        private static int getCodeType(string classDef, string attrName)
        {
            bool isArr = attrName.IndexOf('[') > 0;
            if (attrName.IndexOf('.') > 0)
                return isArr ? 6 : 5;
            return String.IsNullOrEmpty(classDef) ? (isArr ? 2 : 1) : (isArr ? 4 : 3);
        }

        //创建类模版
        private static string getCodeTemplate(string className)
        {
            string code = "";
            code += "using UnityEngine;\n";
            code += "using System;\n";
            code += "using System.Collections.Generic;\n";
            code += "using FunEngine.Core;\n";
            //code += "using FunEngine.Interfaces;\n";
            //code += "using FunEngine.Managers;\n";
            //code += "using FunEngine.Utils;\n";
            //code += "using " + _packageName + ";\n";
            //code += _usingSpliter;
            //code += _usingSpliter;
            code += "\n";
            //code += "namespace " + _packageName + "\n";
            //code += "{\n";
            code += "\n";
            code += "\tpublic class " + className + " : AbstractConfVO\n";
            code += "\t{\n";
            code += "\n";
            code += "\t\t" + _innerClassCodeSpliter;
            //code += "#InnerClassCode#";
            code += _innerClassCodeSpliter;
            code += "\n";
            code += "\t\t//析构函数.\n";
            code += "\t\t~" + className + " ()\n";
            code += "\t\t{\n";
            code += "\t\t\tthis.Dispose ();\n";
            code += "\t\t}\n\n";

            code += "\t\tpublic override void Dispose()\n";
            code += "\t\t{\n";
            code += "\t\t\tbase.Dispose();\n";
            code += "\t\t\t" + _disposeCodeSpliter;
            //code += "#DisposeCode#";
            code += _disposeCodeSpliter;
            code += "\t\t}\n\n";

            //粘贴属性代码
            code += "\t\t" + _attrCodeSpliter;
            //code += "#AttrCode#";
            code += _attrCodeSpliter;

            code += "\t}\n";
            //code += "}";
            return code;
        }

        //获取内部类模版
        private static string getInnerClassCodeTemplate(string innerClassName)
        {
            string code = "";
            code += "\t\tpublic class " + innerClassName + " : IDisposable\n";
            code += "\t\t{\n";
            //构造器/析构函数代码
            code += "\t\t\t//析构函数.\n";
            code += "\t\t\t~" + innerClassName + " ()\n";
            code += "\t\t\t{\n";
            code += "\t\t\t\tthis.Dispose ();\n";
            code += "\t\t\t}\n\n";

            code += "\t\t\tpublic void Dispose()\n";
            code += "\t\t\t{\n";
            code += _disposeCodeSpliter;
            //code += "#DisposeCode#";
            code += _disposeCodeSpliter;
            code += "\t\t\t}\n\n";

            //粘贴属性代码
            code += _attrCodeSpliter;
            //code += "#AttrCode#";
            code += _attrCodeSpliter;

            code += "\t\t}\n";
            return code;
        }

        //普通属性
        private static void setAttrCode(ref string attrCode,
                                        string comment,
                                        string attrType,
                                        string attrName,
                                        string tab = "\t\t")
        {
            //写注释
            attrCode += tab + "//" + comment + ".\n";

            attrCode += tab + "public " + attrType + " " + attrName + "\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\tget;\n";
            attrCode += tab + "\tset;\n";
            attrCode += tab + "}\n";
            attrCode += tab + "\n";
        }

        //外键属性
        private static void setRefAttrCode(ref string attrCode,
                                           ref string disposeCode,
                                           string comment,
                                           string idType,
                                           string refType,
                                           string attrName,
                                           string tab = "\t\t")
        {
            string idAttrName = attrName;
            string refAttrName = attrName.Replace("ID", "");

            //写注释
            attrCode += tab + "//" + comment + ".\n";

            attrCode += tab + "private " + idType + " _" + idAttrName + ";\n";
            attrCode += tab + "private " + refType + " _" + refAttrName + ";\n";

            attrCode += tab + "public " + idType + " " + idAttrName + "\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\tset\n";
            attrCode += tab + "\t{\n";
            attrCode += tab + "\t\tthis._" + idAttrName + " = value;\n";
            attrCode += tab + "\t\tthis._" + refAttrName + " = null;\n";
            attrCode += tab + "\t}\n";

            attrCode += tab + "\tget\n";
            attrCode += tab + "\t{\n";
            attrCode += tab + "\t\treturn this._" + idAttrName + ";\n";
            attrCode += tab + "\t}\n";

            attrCode += tab + "}\n";

            attrCode += tab + "public " + refType + " " + refAttrName + "\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\tget\n";
            attrCode += tab + "\t{\n";
            attrCode += tab + "\t\tif (this._" + refAttrName + " == null && this._" + idAttrName + " >=0 )\n";
            attrCode += tab + "\t\t\tthis._" + refAttrName + " = ConfigManager.GetConfVO<" + refType + ">(this._" + idAttrName + ");\n";
            attrCode += tab + "\t\t return this._" + refAttrName + ";\n";
            attrCode += tab + "\t}\n";
            attrCode += tab + "}\n";
            attrCode += "\n";

            disposeCode += tab + "\tthis._" + refAttrName + " = null;\n";
        }

        //数组
        private static void setArrAttrCode(ref string attrCode,
                                           ref string disposeCode,
                                           string comment,
                                           string attrType,
                                           string arrAttrName,
                                           string tab = "\t\t")
        {
            //写注释
            attrCode += tab + "//" + comment + ".\n";

            attrCode += tab + "private List<" + attrType + "> _" + arrAttrName + ";\n";
            //添加对象方法
            attrCode += tab + "public void AddTo" + upperFirst(arrAttrName) + "(int index , " + attrType + " data)\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\tif(this._" + arrAttrName + " == null)\n";
            attrCode += tab + "\t\tthis._" + arrAttrName + " = new List<" + attrType + ">();\n";
            attrCode += tab + "\tthis._" + arrAttrName + ".Add(data);\n";
            attrCode += tab + "}\n";

            attrCode += tab + "public " + attrType + " GetFrom" + upperFirst(arrAttrName) + "(int index)\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\t\treturn this._" + arrAttrName + "[index];\n";
            attrCode += tab + "}\n";

            attrCode += tab + "public " + attrType + "[] " + arrAttrName + "\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\tget\n";
            attrCode += tab + "\t{\n";
            attrCode += tab + "\t\treturn this._" + arrAttrName + ".ToArray();\n";
            attrCode += tab + "\t}\n";
            attrCode += tab + "}\n";
            attrCode += "\n";

            //disposeCode
            disposeCode += tab + "\tif(this._" + arrAttrName + " != null)\n";
            disposeCode += tab + "\t\tthis._" + arrAttrName + ".Clear();\n";
            disposeCode += tab + "\tthis._" + arrAttrName + " = null;\n";
        }

        private static void setRefArrAttrCode(ref string attrCode,
                                              ref string disposeCode,
                                              string comment,
                                              string idType,
                                              string refType,
                                              string arrAttrName,
                                              string tab = "\t\t")
        {
            string arrIDAttrName = arrAttrName + "IDs";

            //写注释
            attrCode += tab + "//" + comment + ".\n";

            attrCode += tab + "private List<" + idType + "> _" + arrIDAttrName + ";\n";
            attrCode += tab + "private List<" + refType + "> _" + arrAttrName + ";\n";

            //添加对象方法
            attrCode += tab + "public void AddTo" + upperFirst(arrAttrName) + "(int index , " + idType + " id)\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\tif(this._" + arrIDAttrName + " == null)\n";
            attrCode += tab + "\t\tthis._" + arrIDAttrName + " = new List<" + idType + ">();\n";
            attrCode += tab + "\tthis._" + arrIDAttrName + ".Add(id);\n";
            attrCode += tab + "}\n";

            attrCode += tab + "public " + refType + " GetFrom" + upperFirst(arrAttrName) + "(int index)\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\t\treturn this." + arrAttrName + "[index];\n";
            attrCode += tab + "}\n";

            attrCode += tab + "public " + idType + "[] " + arrIDAttrName + "\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\tget\n";
            attrCode += tab + "\t{\n";
            attrCode += tab + "\t\treturn this._" + arrIDAttrName + ".ToArray();\n";
            attrCode += tab + "\t}\n";
            attrCode += tab + "}\n";
            attrCode += "\n";
            
            attrCode += tab + "public " + refType + "[] " + arrAttrName + "\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\tget\n";
            attrCode += tab + "\t{\n";
            attrCode += tab + "\t\tif(this._" + arrAttrName + " == null)\n";
            attrCode += tab + "\t\t{\n";
            attrCode += tab + "\t\t\tthis._" + arrAttrName + " = new List<" + refType + ">();\n";
            attrCode += tab + "\t\t\tfor(int i = 0; i < this._" + arrIDAttrName + ".Count; i++)\n";
            attrCode += tab + "\t\t\t{\n";
            attrCode += tab + "\t\t\t\t" + idType + " id = this._" + arrIDAttrName + "[i];\n";
            //attrCode += tab + "\t\t\t\tthis._" + arrAttrName + "[id] = (i > 0 && id == 0)? null : ConfigManager.GetConfVo<" + refType + ">(id);\n";
            attrCode += tab + "\t\t\t\tif(id >= 0) {\n";
            attrCode += tab + tab + tab + "this._" + arrAttrName + ".Add(ConfigManager.GetConfVO<" + refType + ">(id));\n";
            attrCode += tab + "\t\t\t\t}\n";
            attrCode += tab + "\t\t\t}\n";
            attrCode += tab + "\t\t}\n";
            attrCode += tab + "\t\treturn this._" + arrAttrName + ".ToArray();\n";
            attrCode += tab + "\t}\n";
            attrCode += tab + "}\n";
            attrCode += "\n";

            //disposeCode
            disposeCode += tab + "\tif(this._" + arrIDAttrName + " != null)\n";
            disposeCode += tab + "\t\tthis._" + arrIDAttrName + ".Clear();\n";
            disposeCode += tab + "\tthis._" + arrIDAttrName + " = null;\n";
            disposeCode += tab + "\tif(this._" + arrAttrName + " != null)\n";
            disposeCode += tab + "\t\tthis._" + arrAttrName + ".Clear();\n";
            disposeCode += tab + "\tthis._" + arrAttrName + " = null;\n";
        }

        //内部类属性
        private static void setInnerClassAttrCode(ref string attrCode,
                ref string disposeCode,
                string comment,
                string attrType,
                string attrName,
                string tab = "\t\t")
        {
            //写注释
            attrCode += tab + "//" + comment + ".\n";

            attrCode += tab + "public " + attrType + " " + attrName + "\n";
            attrCode += tab + "{\n";
            attrCode += tab + "\tget;\n";
            attrCode += tab + "\tset;\n";
            attrCode += tab + "}\n";
            attrCode += tab + "\n";

            //disposeCode
            disposeCode += tab + "\tif(this." + attrName + " != null)\n";
            disposeCode += tab + "\t\tthis." + attrName + ".Dispose();\n";
            disposeCode += tab + "\tthis." + attrName + " = null;\n";
        }

        //生成内部类代码
        private static void setInnerClassCode(string innerClassName,
                                              string mark,
                                              ref string innerAttrCode,
                                              ref string innerDisposeCode,
                                              ref int i,
                                              ref string[] classDefs, ref string[] comments, ref string[] attrNames, ref string[] attrTypes)
        {
            while (i < attrNames.Length)
            {
                if (attrNames[i].IndexOf(mark) < 0)
                {
                    break;
                }

                //
                string attrName = attrNames[i].Split('.')[1];
                //1.普通属性 2.普通属性数组 3.外键属性 4.外键属性数组 5.内部类 6.内部类数组
                int codeType = 1;
                if (innerClassName != classDefs[i])
                    codeType = getCodeType(classDefs[i], attrName);
                //普通属性
                if (codeType == 1)
                {
                    setAttrCode(ref innerAttrCode, comments[i], attrTypes[i], attrName, "\t\t\t");
                    i++;
                }
                //普通属性数组
                else if (codeType == 2)
                {
                    string arrAttrName = attrName.Split('[')[0];
                    setArrAttrCode(ref innerAttrCode, ref innerDisposeCode, comments[i], attrTypes[i], arrAttrName, "\t\t\t");
                    //跳过相同的变量
                    i++;
                    skipSameAttr(ref i, arrAttrName, ref attrNames);
                }
                //外键属性
                else if (codeType == 3)
                {
                    setRefAttrCode(ref innerAttrCode, ref innerDisposeCode, comments[i], attrTypes[i], classDefs[i], attrName, "\t\t\t");
                    i++;
                }
                //外键属性数组
                else if (codeType == 4)
                {
                    string arrAttrName = attrName.Split('[')[0];
                    setRefArrAttrCode(ref innerAttrCode, ref innerDisposeCode, comments[i], attrTypes[i], classDefs[i], arrAttrName, "\t\t\t");
                    //跳过相同的变量
                    i++;
                    skipSameAttr(ref i, arrAttrName, ref attrNames);
                }
                else
                {
                    i++;
                }
            }
        }

        //获取枚举类代码
        private static void setEnumCode(string className,
                                        ref string enumCode,
                                        ref List<string[]> datas,
                                        string tab = "\t\t")
        {
            string arrString = "";

            int i = 4;
            while (i < datas.Count)
            {
                if (String.IsNullOrEmpty(datas[i][0]))
                {
                    break;
                }
                //
                if (datas[i].Length > 2)
                    enumCode += tab + "//" + datas[i][2] + ".\n";
                enumCode += tab + "public static " + className + " " + datas[i][1] + "\n";
                enumCode += tab + "{\n";
                enumCode += tab + "\tget\n";
                enumCode += tab + "\t{\n";
                enumCode += tab + "\t\treturn ConfigManager.GetConfVO<" + className + ">(" + datas[i][0] + ");\n";
                enumCode += tab + "\t}\n";
                enumCode += tab + "}\n";
                arrString += datas[i][1] + ",";
                i++;
            }

            enumCode += tab + "\n";
            //添加数组属性
            enumCode += tab + "private static " + className + "[] _list;\n";
            enumCode += tab + "public static " + className + "[] list\n";
            enumCode += tab + "{\n";
            enumCode += tab + "\tget\n";
            enumCode += tab + "\t{\n";
            enumCode += tab + "\t\tif (_list==null )\n";
            enumCode += tab + "\t\t{\n";
            enumCode += tab + "\t\t\t_list = new " + className + "[]{ " + arrString.Substring(0, arrString.Length - 1) + " };\n";
            enumCode += tab + "\t\t}\n";
            enumCode += tab + "\t\treturn _list;\n";
            enumCode += tab + "\t}\n";
            enumCode += tab + "}\n";

            enumCode += tab + "public static " + className + " GetByID(int id)\n";
            enumCode += tab + "{\n";
            enumCode += tab + "\treturn list[id];\n";
            enumCode += tab + "}\n";
        }
    }
}