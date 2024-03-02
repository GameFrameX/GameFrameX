using Server.Setting;

namespace Server.ServerManager;

/// <summary>
/// 服务器信息
/// </summary>
public sealed class ServerInfo
{
    public ServerInfo(ServerType type, string serverName, long serverId, string localIp, int innerPort)
    {
        Type = type;
        ServerName = serverName;
        ServerId = serverId;
        LocalIp = localIp;
        InnerPort = innerPort;
    }

    /// <summary>
    /// 服务器类型
    /// </summary>
    public ServerType Type { get; }

    /// <summary>
    /// 服务器名称
    /// </summary>
    public string ServerName { get; }

    /// <summary>
    /// 服务器ID
    /// </summary>
    public long ServerId { get; }

    /// <summary>
    /// 本地IP
    /// </summary>
    public string LocalIp { get; }

    /// <summary>
    /// 内部端口
    /// </summary>
    public int InnerPort { get; }

    /// <summary>
    /// 服务器状态
    /// </summary>
    public ServerStatus Status { get; set; }

    /// <summary>
    /// 服务器状态
    /// </summary>
    public ServerStatusInfo StatusInfo { get; set; } = new ServerStatusInfo();
}