using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameFrameX.Web
{
    public interface IWebManager
    {
        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        Task<string> GetToString(string url);

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        Task<byte[]> GetToBytes(string url);

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <returns></returns>
        Task<string> GetToString(string url, Dictionary<string, string> queryString);


        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <returns></returns>
        Task<byte[]> GetToBytes(string url, Dictionary<string, string> queryString);


        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        Task<string> GetToString(string url, Dictionary<string, string> queryString, Dictionary<string, string> header);


        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        Task<byte[]> GetToBytes(string url, Dictionary<string, string> queryString, Dictionary<string, string> header);


        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">请求参数</param>
        /// <returns></returns>
        Task<string> PostToString(string url, Dictionary<string, string> from);

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <returns></returns>
        Task<string> PostToString(string url, Dictionary<string, string> from, Dictionary<string, string> queryString);

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        Task<string> PostToString(string url, Dictionary<string, string> from, Dictionary<string, string> queryString, Dictionary<string, string> header);
        
        
        
        
        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">请求参数</param>
        /// <returns></returns>
        Task<byte[]> PostToBytes(string url, Dictionary<string, string> from);

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <returns></returns>
        Task<byte[]> PostToBytes(string url, Dictionary<string, string> from, Dictionary<string, string> queryString);

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        Task<byte[]> PostToBytes(string url, Dictionary<string, string> from, Dictionary<string, string> queryString, Dictionary<string, string> header);

        
        
    }
}