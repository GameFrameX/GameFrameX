using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 压缩帮助类
    /// </summary>
    public static class ZipHelper
    {
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] content)
        {
            //return content;
            Deflater compressor = new Deflater();
            compressor.SetLevel(Deflater.BEST_COMPRESSION);

            compressor.SetInput(content);
            compressor.Finish();

            using (MemoryStream bos = new MemoryStream(content.Length))
            {
                var buf = new byte[4096];
                while (!compressor.IsFinished)
                {
                    int n = compressor.Deflate(buf);
                    bos.Write(buf, 0, n);
                }

                return bos.ToArray();
            }
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] content)
        {
            return Decompress(content, 0, content.Length);
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="content"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] content, int offset, int count)
        {
            //return content;
            Inflater decompressor = new Inflater();
            decompressor.SetInput(content, offset, count);

            using (MemoryStream bos = new MemoryStream(content.Length))
            {
                var buf = new byte[4096];
                while (!decompressor.IsFinished)
                {
                    int n = decompressor.Inflate(buf);
                    bos.Write(buf, 0, n);
                }

                return bos.ToArray();
            }
        }
    }
}