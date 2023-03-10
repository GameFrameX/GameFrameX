using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelConverter.Utils
{
    public class FileUtil
    {

        public static void ClearDirectory(string path, bool recursion = true, string filter = "")
        {
            List<string> fileList = GetFileList(path, true, filter);
            foreach (string filePath in fileList)
            {
                File.Delete(filePath);
            }
        }

        public static List<string> GetFileList(string path, bool recursion = false, string filter="")
        {
            List<string> fileList = new List<string>();
            if (!Directory.Exists(path))
                return fileList;
            DirectoryInfo dir = new DirectoryInfo(path);
            System.IO.FileInfo[] fil = dir.GetFiles();
            foreach (System.IO.FileInfo f in fil)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    if (filter.Contains(".xlsx") && f.Name.Contains("~$"))
                        continue;
                    filter = filter.ToLower();
                    if (filter.IndexOf(f.Extension.ToLower()) >= 0)
                        fileList.Add(f.FullName);
                }
                else
                {
                    fileList.Add(f.FullName);
                }
            }
            //获取子文件夹内的文件列表，递归遍历
            if (recursion)
            {
                DirectoryInfo[] dii = dir.GetDirectories();
                foreach (DirectoryInfo d in dii)
                {
                    fileList.AddRange(GetFileList(d.FullName, recursion));
                }
            }
            return fileList;
        }


    }
}
