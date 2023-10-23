using System;
using System.IO;

public static class StringExtension
{
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static int[] SplitToIntArray(this string str, char sep = '+')
    {
        if (string.IsNullOrEmpty(str))
            return Array.Empty<int>();

        var arr = str.Split(sep);
        int[] ret = new int[arr.Length];
        for (int i = 0; i < arr.Length; ++i)
        {
            if (int.TryParse(arr[i], out var t))
                ret[i] = t;
        }

        return ret;
    }

    public static int[][] SplitTo2IntArray(this string str, char sep1 = ';', char sep2 = '+')
    {
        if (string.IsNullOrEmpty(str))
            return Array.Empty<int[]>();

        var arr = str.Split(sep1);
        if (arr.Length <= 0)
            return Array.Empty<int[]>();

        int[][] ret = new int[arr.Length][];

        for (int i = 0; i < arr.Length; ++i)
            ret[i] = arr[i].SplitToIntArray(sep2);
        return ret;
    }

    /// <summary>
    /// 根据字符串创建目录,递归
    /// </summary>
    public static void CreateAsDirectory(this string path, bool isFile = false)
    {
        if (isFile)
            path = Path.GetDirectoryName(path);
        if (!Directory.Exists(path))
        {
            CreateAsDirectory(path, true);
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// 从指定字符串中的指定位置处开始读取一行。
    /// </summary>
    /// <param name="rawString">指定的字符串。</param>
    /// <param name="position">从指定位置处开始读取一行，读取后将返回下一行开始的位置。</param>
    /// <returns>读取的一行字符串。</returns>
    public static string ReadLine(this string rawString, ref int position)
    {
        if (position < 0)
        {
            return null;
        }

        int length = rawString.Length;
        int offset = position;
        while (offset < length)
        {
            char ch = rawString[offset];
            switch (ch)
            {
                case '\r':
                case '\n':
                    if (offset > position)
                    {
                        string line = rawString.Substring(position, offset - position);
                        position = offset + 1;
                        if ((ch == '\r') && (position < length) && (rawString[position] == '\n'))
                        {
                            position++;
                        }

                        return line;
                    }

                    offset++;
                    position++;
                    break;

                default:
                    offset++;
                    break;
            }
        }

        if (offset > position)
        {
            string line = rawString.Substring(position, offset - position);
            position = offset;
            return line;
        }

        return null;
    }
}