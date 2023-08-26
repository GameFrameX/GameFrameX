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

    /// <summary>
    /// 是否是Debug 模式
    /// </summary>
    public bool IsDebug { get; init; }

    /// <summary>
    /// 服务器ID
    /// </summary>
    public int ServerId { get; init; }

    /// <summary>
    /// 服务器名称
    /// </summary>
    public string ServerName { get; init; }

    /// <summary>
    /// 本地IP
    /// </summary>
    public string LocalIp { get; init; }

    /// <summary>
    /// HTTP 响应码
    /// </summary>
    public string HttpCode { get; init; }

    /// <summary>
    /// Http 地址
    /// </summary>
    public string HttpUrl { get; init; }

    /// <summary>
    /// HTTP 端口
    /// </summary>
    public int HttpPort { get; init; }

    /// <summary>
    /// TCP 端口
    /// </summary>
    public int TcpPort { get; init; }

    /// <summary>
    /// GRPC 端口
    /// </summary>
    public int GrpcPort { get; init; }

    /// <summary>
    /// 数据库 地址
    /// </summary>
    public string MongoUrl { get; init; }

    /// <summary>
    /// 数据库名称
    /// </summary>
    public string MongoDBName { get; init; }

    /// <summary>
    /// 本地数据库前缀
    /// </summary>
    public string LocalDBPrefix { get; init; }

    /// <summary>
    /// 本地数据库路径
    /// </summary>
    public string LocalDBPath { get; init; }

    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; init; }

    /// <summary>
    /// 数据中心
    /// </summary>
    public string DataCenter { get; init; }

    /// <summary>
    /// 数据中心地址
    /// </summary>
    public string CenterUrl { get; init; }

    /// <summary>
    /// SDK 类型
    /// </summary>
    public int SDKType { get; set; }

    /// <summary>
    /// 数据库模式
    /// </summary>
    public int DBModel { get; set; }

    #endregion
}