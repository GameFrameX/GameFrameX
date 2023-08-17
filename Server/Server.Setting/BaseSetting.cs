namespace Server.Setting;

public abstract class BaseSetting
{
    public virtual bool IsLocal(int serverId)
    {
        return serverId == ServerId;
    }

    /// <summary>
    /// 启动时间
    /// </summary>
    public DateTime LaunchTime { get; set; }

    /// <summary>
    /// 是否正在运行中
    /// </summary>
    public volatile bool AppRunning = false;

    /// <summary>
    /// 服务器类型
    /// </summary>
    public ServerType ServerType { get; set; }

    #region from config

    public bool IsDebug { get; init; }

    public int ServerId { get; init; }

    public string ServerName { get; init; }

    public string LocalIp { get; init; }

    public string HttpCode { get; init; }

    public string HttpUrl { get; init; }

    public int HttpPort { get; init; }

    public int TcpPort { get; init; }

    public int GrpcPort { get; init; }

    public string MongoUrl { get; init; }

    public string MongoDBName { get; init; }

    public string LocalDBPrefix { get; init; }

    public string LocalDBPath { get; init; }

    public string Language { get; init; }

    public string DataCenter { get; init; }

    public string CenterUrl { get; init; }

    public int SDKType { get; set; }

    public int DBModel { get; set; }

    #endregion
}