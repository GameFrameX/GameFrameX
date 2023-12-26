using System.Runtime.Remoting.Messaging;

namespace GameFrameX.Network
{
    /// <summary>
    /// 网络消息心跳包接口。
    /// </summary>
    public interface IPacketHeartBeatHandler
    {
        /// <summary>
        /// 每次心跳的间隔
        /// </summary>
        float HeartBeatInterval { get; }

        /// <summary>
        /// 几次心跳丢失。触发断开网络
        /// </summary>
        int MissHeartBeatCountByClose { get; }

        /// <summary>
        /// 处理器
        /// </summary>
        /// <returns></returns>
        MessageObject Handler();
    }
}