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
        if (_instance.ServerId < GlobalConst.MIN_SERVER_ID || _instance.ServerId > GlobalConst.MAX_SERVER_ID)
        {
            throw new Exception($"ServerId不合法{_instance.ServerId},需要在[{GlobalConst.MIN_SERVER_ID},{GlobalConst.MAX_SERVER_ID}]范围之内");
        }
    }

    public static T InsAs<T>() where T : BaseSetting
    {
        return (T) _instance;
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

    public static ServerType ServerType => _instance.ServerType;

    public static bool IsDebug => _instance.IsDebug;

    public static int ServerId => _instance.ServerId;

    public static string ServerName => _instance.ServerName;

    public static string LocalIp => _instance.LocalIp;

    public static string HttpCode => _instance.HttpCode;

    public static string HttpUrl => _instance.HttpUrl;

    public static int HttpPort => _instance.HttpPort;

    public static int TcpPort => _instance.TcpPort;

    public static int GrpcPort => _instance.GrpcPort;

    public static string MongoUrl => _instance.MongoUrl;

    public static string MongoDBName => _instance.MongoDBName;

    public static string LocalDBPrefix => _instance.LocalDBPrefix;

    public static string LocalDBPath => _instance.LocalDBPath;

    public static string Language => _instance.Language;

    public static string DataCenter => _instance.DataCenter;

    public static string CenterUrl => _instance.CenterUrl;

    public static int SDKType => _instance.SDKType;

    public static int DBModel => _instance.DBModel;
}