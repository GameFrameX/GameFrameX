using Server.NetWork;
using Server.NetWork.Messages;

namespace Server.Launcher.Message;

public class NetWorkChannelHelper : INetWorkChannelHelper
{
    /// <summary>
    /// 发送消息
    /// </summary>
    public Func<IMessage, byte[]> OnSendMessage { get; set; }

    /// <summary>
    /// 收到消息
    /// </summary>
    public Action<ISession, byte[], long, long> OnReceiveMessage { get; set; }

    /// <summary>
    /// 网络连接断开
    /// </summary>
    public Action OnDisconnected { get; set; }

    /// <summary>
    /// 网络连接成功
    /// </summary>
    public Action OnConnected { get; set; }

    /// <summary>
    /// 网络错误
    /// </summary>
    public Action<string> OnError { get; set; }
}