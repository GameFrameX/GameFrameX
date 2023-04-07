namespace UnityGameFramework.Procedure
{
    /// <summary>
    /// HTTP网页请求的消息响应结构
    /// </summary>
    public sealed class HttpJsonResult
    {
        /// <summary>
        /// 响应码0 为成功
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 响应数据.
        /// </summary>
        public string data { get; set; }
    }
}