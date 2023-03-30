using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public static class FileHelper
    {
        /// <summary>
        /// 获取目录下的所有文件
        /// </summary>
        /// <param name="files"></param>
        /// <param name="dir"></param>
        public static void GetAllFiles(List<string> files, string dir)
        {
            string[] fls = Directory.GetFiles(dir);
            foreach (string fl in fls)
            {
                files.Add(fl);
            }

            string[] subDirs = Directory.GetDirectories(dir);
            foreach (string subDir in subDirs)
            {
                GetAllFiles(files, subDir);
            }
        }

        /// <summary>
        /// 清理目录
        /// </summary>
        /// <param name="dir"></param>
        public static void CleanDirectory(string dir)
        {
            foreach (string subDir in Directory.GetDirectories(dir))
            {
                Directory.Delete(subDir, true);
            }

            foreach (string subFile in Directory.GetFiles(dir))
            {
                File.Delete(subFile);
            }
        }

        /// <summary>
        /// 目录复制
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="tgtDir"></param>
        /// <exception cref="Exception"></exception>
        public static void CopyDirectory(string srcDir, string tgtDir)
        {
            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("父目录不能拷贝到子目录！");
            }

            if (!source.Exists)
            {
                return;
            }

            if (!target.Exists)
            {
                target.Create();
            }

            FileInfo[] files = source.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
            }

            DirectoryInfo[] dirs = source.GetDirectories();

            for (int j = 0; j < dirs.Length; j++)
            {
                CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
            }
        }

        /// <summary>
        /// 复制文件到目标目录
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        /// <param name="overwrite">是否覆盖</param>
        public static void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            File.Copy(sourceFileName, destFileName, overwrite);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        public static void Delete(string path)
        {
            File.Delete(path);
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsExists(string path)
        {
            if (IsAndroidReadOnlyPath(path, out var readPath))
            {
                return BlankReadAssets.BlankReadAssets.IsFileExists(readPath);
            }

            return File.Exists(path);
        }

        private static bool IsAndroidReadOnlyPath(string path, out string readPath)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (PathHelper.NormalizePath(path).Contains(PathHelper.AppResPath))
                {
                    readPath = path.Substring(PathHelper.AppResPath.Length);
                    return true;
                }
            }

            readPath = null;
            return false;
        }

        /// <summary>
        /// 移动文件到目标目录
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        public static void Move(string sourceFileName, string destFileName)
        {
            Copy(sourceFileName, destFileName, true);
            Delete(sourceFileName);
        }

        /// <summary>
        /// 读取指定路径的文件内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(string path)
        {
            if (IsAndroidReadOnlyPath((path), out var readPath))
            {
                return BlankReadAssets.BlankReadAssets.Read(readPath);
            }

            return File.ReadAllBytes(path);
        }

        /// <summary>
        /// 读取指定路径的文件内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        /// <summary>
        /// 读取指定路径的文件内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path);
        }

        /// <summary>
        /// 写入指定路径的文件内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static void ReadAllLines(string path, byte[] buffer)
        {
            File.WriteAllBytes(path, buffer);
        }

        /// <summary>
        /// 写入指定路径的文件内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static void WriteAllLines(string path, string[] lines)
        {
            File.WriteAllLines(path, lines);
        }

        /// <summary>
        /// 写入指定路径的文件内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content"></param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static void WriteAllText(string path, string content, Encoding encoding)
        {
            File.WriteAllText(path, content, encoding);
        }

        /// <summary>
        /// 写入指定路径的文件内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}