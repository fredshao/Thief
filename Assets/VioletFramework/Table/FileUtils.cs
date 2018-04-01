using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace FunEngine.Utils
{
    public class FileUtils
    {
        // ------------------------------------------------------------------------------
        public enum SearchTypeEnum
        {
            DirectoryOnly = 1,
            FileOnly = 2,
            All = 3
        }

        public static FileSystemInfo[] GetAllInDirectory(string fullPath, SearchOption searchOption, SearchTypeEnum searchType)
        {
            return GetAllInDirectory(new DirectoryInfo(fullPath), searchOption, searchType);
        }

        public static FileSystemInfo[] GetAllInDirectory(DirectoryInfo directoryInfo, SearchOption searchOption, SearchTypeEnum searchType)
        {
            if (searchType == SearchTypeEnum.DirectoryOnly)
            {
                return directoryInfo.GetDirectories("*", searchOption);
            }
            else if (searchType == SearchTypeEnum.FileOnly)
            {
                return directoryInfo.GetFiles("*.*", searchOption);
            }
            //
            DirectoryInfo[] directories = directoryInfo.GetDirectories("*", searchOption);
            FileInfo[] files = directoryInfo.GetFiles("*.*", searchOption);
            //
            FileSystemInfo[] list = new FileSystemInfo[directories.Length + files.Length];
            int index = 0;
            foreach (DirectoryInfo info in directories)
                list[index++] = info;
            foreach (FileInfo info in files)
                list[index++] = info;
            //
            return list;
        }


        public static FileInfo[] GetFilesInDirectory(string fullPath, string searchPattern, SearchOption searchOption)
        {
            return GetFilesInDirectory(new DirectoryInfo(fullPath), searchPattern, searchOption);
        }

        public static FileInfo[] GetFilesInDirectory(DirectoryInfo directoryInfo, string searchPattern, SearchOption searchOption)
        {
            return directoryInfo.GetFiles(searchPattern, searchOption);
        }

        public static bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public static void CreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        public static void DeleteDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);
        }

        public static void ReCreateDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
            Directory.CreateDirectory(directoryPath);
        }

        //复制文件夹
        public static void CopyDirectory(string sourcePath, string destinationPath, string[] ext = null, bool overwrite = true)
        {
            CopyAll(sourcePath, destinationPath, ext, overwrite, false);
        }

        public static void MoveDirectory(string sourcePath, string destinationPath, bool overwrite = true)
        {
            CopyAll(sourcePath, destinationPath, null, overwrite, true);
            //
            //删除SourcePath
            FileUtils.DeleteDirectory(sourcePath);
        }

        private static void CopyAll(string sourcePath, string destinationPath, string[] ext, bool overwrite, bool deleteSource)
        {
            DirectoryInfo info = new DirectoryInfo(sourcePath);
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                string destName = Path.Combine(destinationPath, fsi.Name);

                if (fsi is FileInfo)
                {
                    //如果是文件，复制文件
                    if (ext == null || Array.IndexOf<string>(ext, fsi.Extension.Substring(1)) >= 0)
                    {
                        File.Copy(fsi.FullName, destName, overwrite);
                        if (deleteSource)
                            File.Delete(fsi.FullName);
                    }
                }
                else
                {
                    //如果是文件夹，新建文件夹，递归
                    Directory.CreateDirectory(destName);
                    CopyAll(fsi.FullName, destName, ext, overwrite, deleteSource);
                }
            }
        }

        // ------------------------------------------------------------------------------
        // 获取指定文件的文件名
        // ------------------------------------------------------------------------------
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public static void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        public static void CopyFile(string sourcePath, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            //
            FileInfo fi = new FileInfo(sourcePath);
            File.Copy(fi.FullName, Path.Combine(directoryPath, fi.Name), true);
        }

        public static void MoveFile(string sourcePath, string directoryPath)
        {
            CopyFile(sourcePath, directoryPath);
            DeleteFile(sourcePath);
        }

        public static void CopyFileWithFullPath(string sourcePath, string fullPath)
        {
            FileUtils.WriteBinaryFile(FileUtils.OpenBinaryFile(sourcePath), fullPath);
        }

        public static void MoveFileWithFullPath(string sourcePath, string fullPath)
        {
            CopyFileWithFullPath(sourcePath, fullPath);
            DeleteFile(sourcePath);
        }

        public static string GetFileName(string filePath, bool needExtension)
        {
            return GetFileName(new FileInfo(filePath), needExtension);
        }
        public static string GetFileName(FileInfo info, bool needExtension)
        {
            if (needExtension)
                return info.Name;
            return info.Name.Substring(0, info.Name.LastIndexOf('.'));
        }

        public static string GetFileExtension(string filePath)
        {
            return GetFileExtension(new FileInfo(filePath));
        }
        public static string GetFileExtension(FileInfo info)
        {
            return info.Extension.Substring(1);
        }
        // public static string ChangeFileExtension(FileInfo info, string ext)
        // {
        //     return info.Name.Split('.')[0] + (ext.IndexOf('.') == 0 ? ext : "." + ext);
        // }

        //更新文件名称
        public static void ChangeFileName(string filePath, string newName)
        {
            string parent = filePath.Substring(0, filePath.LastIndexOf("/"));
            byte[] bytes = FileUtils.OpenBinaryFile(filePath);
            FileUtils.WriteBinaryFile(bytes, parent + "/" + newName);
            FileUtils.DeleteFile(filePath);
        }

        public static string GetFileParentFolderPath(string filePath)
        {
            //去除file协议前缀
            // if (filePath.IndexOf("file://") == 0)
            //     filePath = filePath.Replace("file://", "");
            //Log.Info(filePath);
            return new FileInfo(filePath).DirectoryName;
        }

        public static void CreateParentDirectory(string filePath)
        {
            string parentFolderName = new FileInfo(filePath).DirectoryName;
            if (!Directory.Exists(parentFolderName))
                Directory.CreateDirectory(parentFolderName);
        }

        // ------------------------------------------------------------------------------
        // 打开/写入文件
        // ------------------------------------------------------------------------------
        public static string OpenTxtFile(string filePath, System.Text.Encoding encoding = null)
        {
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs, encoding == null ? System.Text.Encoding.UTF8 : encoding))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            return String.Empty;
        }

        public static void WriteTxtFile(string txt, string filePath, System.Text.Encoding encoding = null)
        {
            try
            {
                //如果要写入的目录不存在
                CreateParentDirectory(filePath);
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    if (encoding == null)
                        encoding = new UTF8Encoding();
                    //
                    byte[] data = encoding.GetBytes(txt);
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("写入文件失败" + e);
            }
        }

        public static void AppendTxtFile(string txt, string filePath)
        {
            try
            {
                //如果要写入的目录不存在
                CreateParentDirectory(filePath);
                using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                {
                    byte[] data = new UTF8Encoding().GetBytes(txt);
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("AppendTxtFile失败 " + txt + "  " + e);
            }
        }

        public static XmlDocument OpenXmlFile(string filePath)
        {
            XmlDocument xml = null;
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        xml = new XmlDocument();
                        xml.Load(sr);
                    }

                }
            }
            return xml;
        }

        public static void WriteXmlFile(XmlDocument xml, string filePath)
        {
            //如果要写入的目录不存在
            CreateParentDirectory(filePath);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(true, true)))
                {
                    xml.Save(sw);
                    // XmlTextWriter writer = new XmlTextWriter(sw);
                    // writer.Formatting = Formatting.Indented;
                    // xml.WriteTo( writer );
                    // writer.Flush();
                }
            }
        }

        public static byte[] OpenBinaryFile(string filePath)
        {
            byte[] bytes = null;
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        bytes = new byte[fs.Length];
                        br.Read(bytes, 0, bytes.Length);
                    }
                }
            }
            return bytes;
        }

        public static void WriteBinaryFile(byte[] bytes, string filePath)
        {
            //如果要写入的目录不存在
            CreateParentDirectory(filePath);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(bytes);
                }
            }
        }

       


    }
}