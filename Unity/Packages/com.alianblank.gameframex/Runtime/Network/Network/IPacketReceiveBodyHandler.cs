using System.Buffers;

namespace GameFrameX.Network
{
    /// <summary>
    /// 网络消息包处理器接口。
    /// </summary>
    public interface IPacketReceiveBodyHandler
    {
        /// <summary>
        /// 网络消息包处理函数。
        /// </summary>
        /// <param name="source">网络消息包源。</param>
        /// <param name="messageObject"></param>
        bool Handler<T>(object source, out T messageObject) where T : MessageObject;
    }
}