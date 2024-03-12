using Server.NetWork.Messages;

namespace Server.NetWork;

/// <summary>
/// 网络渠道帮助类
/// </summary>
public interface INetWorkChannelHelper
{
    /// <summary>
    /// 发送消息
    /// </summary>
    Func<IMessage, byte[]> OnSendMessage { get; set; }

    /// <summary>
    /// 收到消息
    /// </summary>
    Action<ISession, byte[], long, long> OnReceiveMessage { get; set; }

    /// <summary>
    /// 网络连接断开
    /// </summary>
    Action? OnDisconnected { get; set; }

    /// <summary>
    /// 网络连接成功
    /// </summary>
    Action? OnConnected { get; set; }

    /// <summary>
    /// 网络错误
    /// </summary>
    Action<string>? OnError { get; set; }
}