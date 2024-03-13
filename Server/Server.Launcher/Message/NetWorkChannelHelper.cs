using Server.NetWork;

namespace Server.Launcher.Message;

public class NetWorkChannelHelper : INetWorkChannelHelper
{
    /// <summary>
    /// 网络连接断开
    /// </summary>
    public Action<string> OnDisconnected { get; set; }

    /// <summary>
    /// 网络连接成功
    /// </summary>
    public Action OnConnected { get; set; }

    /// <summary>
    /// 网络错误
    /// </summary>
    public Action<string> OnError { get; set; }
}