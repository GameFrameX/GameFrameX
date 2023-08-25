using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MessagePackCompiler
{
    public static class Tools
    {
        public static string Enumerable2String<T>(this IEnumerable<T> list)
        {
            return "[" + string.Join(",", list) + "]";
        }

        public static void CreateAsDirectory(this string path, bool isFile = false)
        {
            if (isFile)
                path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void CreateDirectory(this string path, bool delFirset = true)
        {
            if (Directory.Exists(path))
            {
                if (delFirset)
                {
                    Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                }
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }

    }
}
