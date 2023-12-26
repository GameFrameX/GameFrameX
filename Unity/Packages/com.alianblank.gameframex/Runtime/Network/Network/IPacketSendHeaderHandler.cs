using System.IO;

namespace GameFrameX.Network
{
    /// <summary>
    /// 网络消息包头接口。
    /// </summary>
    public interface IPacketSendHeaderHandler
    {
        /// <summary>
        /// 消息包头长度
        /// </summary>
        int PacketHeaderLength { get; }

        /// <summary>
        /// 获取网络消息包协议编号。
        /// </summary>
        int Id { get; }

        /// <summary>
        /// 获取网络消息包长度。
        /// </summary>
        int PacketLength { get; }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="messageObject">消息对象</param>
        /// <param name="cachedStream">缓存流</param>
        /// <param name="messageBodyBuffer">消息序列化完的二进制数组</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool Handler<T>(T messageObject, MemoryStream cachedStream, out byte[] messageBodyBuffer) where T : MessageObject;
    }
}