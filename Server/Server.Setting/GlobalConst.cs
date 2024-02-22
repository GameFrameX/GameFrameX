namespace Server.Setting;

/// <summary>
/// 全局常量类
/// </summary>
public static class GlobalConst
{
    /// <summary>
    /// SessionId Key
    /// </summary>
    public const string SessionIdKey = "SESSION_ID";

    /// <summary>
    /// 秒标记
    /// </summary>
    public const int SecondMask = 0b111111111111111111111111111111;

    /// <summary>
    /// 最大全局ID
    /// </summary>
    public const int MaxGlobalId = 10_000_000;

    /// <summary>
    /// 最小服务器ID
    /// </summary>
    public const int MinServerId = 1000;

    /// <summary>
    /// 最大服务器ID
    /// </summary>
    public const int MaxServerId = 9999;

    /// <summary>
    /// 最大Actor增量
    /// </summary>
    public const int MaxActorIncrease = 4095; // 4095

    /// <summary>
    /// 最大唯一增量
    /// </summary>
    public const int MaxUniqueIncrease = 524287; //524287

    /// <summary>
    /// 服务器ID或模块ID掩码
    /// </summary>
    public const int ServerIdOrModuleIdMask = 49; //49+14=63

    /// <summary>
    /// Actor类型标记
    /// </summary>
    public const int ActorTypeMask = 42; //42+7 = 49

    /// <summary>
    /// 时间戳标记
    /// </summary>
    public const int TimestampMask = 12; //12+30 =42

    /// <summary>
    /// 模块ID时间戳标记
    /// </summary>
    public const int ModuleIdTimestampMask = 19; //19+30 =42


    #region GlobalTimer 全局计时器

    /// <summary>
    /// 数据存储间隔 单位 毫秒
    /// </summary>
    public const int SaveIntervalInMilliSeconds = 300_000; //300_000;

    public const int MAGIC = 60;

    #endregion
}