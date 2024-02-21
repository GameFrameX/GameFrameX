using Newtonsoft.Json;

namespace Server.Setting;

/// <summary>
/// 全局设置
/// </summary>
public static class GlobalSettings
{
    private static BaseSetting _instance = null!;

    public static void Load<T>(string path, ServerType serverType) where T : BaseSetting
    {
        var configJson = File.ReadAllText(path);
        _instance = JsonConvert.DeserializeObject<T>(configJson) ?? throw new InvalidOperationException();
        _instance.ServerType = serverType;
        if (_instance.ServerId < GlobalConst.MinServerId || _instance.ServerId > GlobalConst.MaxServerId)
        {
            throw new Exception($"ServerId不合法{_instance.ServerId},需要在[{GlobalConst.MinServerId},{GlobalConst.MaxServerId}]范围之内");
        }
    }

    public static T InsAs<T>() where T : BaseSetting
    {
        return (T)_instance;
    }

    public static bool IsLocal(int serverId) => _instance.IsLocal(serverId);

    /// <summary>
    /// 启动时间
    /// </summary>
    public static DateTime LaunchTime
    {
        get => _instance.LaunchTime;
        set => _instance.LaunchTime = value;
    }

    /// <summary>
    /// 是否正在运行中
    /// </summary>
    public static bool IsAppRunning
    {
        get => _instance.AppRunning;
        set => _instance.AppRunning = value;
    }

    /// <summary>
    /// 服务器类型
    /// </summary>
    public static ServerType ServerType => _instance.ServerType;

    /// <summary>
    /// 是否是Debug 模式
    /// </summary>
    public static bool IsDebug => _instance.IsDebug;

    /// <summary>
    /// 服务器ID
    /// </summary>
    public static int ServerId => _instance.ServerId;

    /// <summary>
    /// 服务器名称
    /// </summary>
    public static string ServerName => _instance.ServerName;

    /// <summary>
    /// 本地IP
    /// </summary>
    public static string LocalIp => _instance.LocalIp;

    /// <summary>
    /// HTTP 响应码
    /// </summary>
    public static string HttpCode => _instance.HttpCode;

    /// <summary>
    /// Http 地址
    /// </summary>
    public static string HttpUrl => _instance.HttpUrl;

    /// <summary>
    /// HTTP 端口
    /// </summary>
    public static int HttpPort => _instance.HttpPort;

    /// <summary>
    /// HTTPS 端口
    /// </summary>
    public static int HttpsPort => _instance.HttpsPort;

    /// <summary>
    /// TCP 端口
    /// </summary>
    public static int TcpPort => _instance.TcpPort;


    /// <summary>
    /// WebSocket 端口
    /// </summary>
    public static int WsPort => _instance.WsPort;

    /// <summary>
    /// WebSocket 加密端口
    /// </summary>
    public static int WssPort => _instance.WssPort;

    /// <summary>
    /// GRPC 端口
    /// </summary>
    public static int GrpcPort => _instance.GrpcPort;

    /// <summary>
    /// 数据库 地址
    /// </summary>
    public static string DataBaseUrl => _instance.MongoUrl;

    /// <summary>
    /// 数据库名称
    /// </summary>
    public static string DataBaseName => _instance.MongoDBName;

    /// <summary>
    /// 语言
    /// </summary>
    public static string Language => _instance.Language;

    /// <summary>
    /// 数据中心
    /// </summary>
    public static string DataCenter => _instance.DataCenter;

    /// <summary>
    /// 数据中心地址
    /// </summary>
    public static string CenterUrl => _instance.CenterUrl;

    /// <summary>
    /// SDK 类型
    /// </summary>
    public static int SDKType => _instance.SDKType;

    public static string WssCertFilePath => _instance.WssCertFilePath;
}