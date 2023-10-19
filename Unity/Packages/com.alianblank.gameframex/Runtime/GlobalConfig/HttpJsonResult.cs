using Newtonsoft.Json;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// HTTP网页请求的消息响应结构
    /// </summary>
    public sealed class HttpJsonResult
    {
        /// <summary>
        /// 响应码0 为成功
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// 响应数据.
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }
    }
}