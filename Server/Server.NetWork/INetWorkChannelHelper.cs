using Server.NetWork.Messages;

namespace Server.NetWork;

/// <summary>
/// 网络渠道帮助类
/// </summary>
public interface INetWorkChannelHelper
{
    /// <summary>
    /// 网络连接断开
    /// </summary>
    Action<string>? OnDisconnected { get; set; }

    /// <summary>
    /// 网络连接成功
    /// </summary>
    Action? OnConnected { get; set; }

    /// <summary>
    /// 网络错误
    /// </summary>
    Action<string>? OnError { get; set; }
}