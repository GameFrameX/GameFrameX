namespace Server.Setting;

/// <summary>
/// 全局常量类
/// </summary>
public static class GlobalConst
{
    /// <summary>
    /// SessionId Key
    /// </summary>
    public const string SESSION_ID_KEY = "SESSION_ID";

    public const int SECOND_MASK = 0x3FFFFFFF;

    /// <summary>
    /// 最大全局ID
    /// </summary>
    public const int MAX_GLOBAL_ID = 10_000_000;

    /// <summary>
    /// 最小服务器ID
    /// </summary>
    public const int MIN_SERVER_ID = 1000;

    /// <summary>
    /// 最大服务器ID
    /// </summary>
    public const int MAX_SERVER_ID = 9999;

    public const int MAX_ACTOR_INCREASE = 4095; // 4095
    public const int MAX_UNIQUE_INCREASE = 524287; //524287


    public const int SERVERID_OR_MODULEID_MASK = 49; //49+14=63
    public const int ACTORTYPE_MASK = 42; //42+7 = 49
    public const int TIMESTAMP_MASK = 12; //12+30 =42
    public const int MODULEID_TIMESTAMP_MASK = 19; //19+30 =42


    #region GlobalTimer 全局计时器

    /// <summary>
    /// 数据存储间隔 单位 毫秒
    /// </summary>
    public const int SAVE_INTERVAL_IN_MilliSECONDS = 300_000; //300_000;

    public const int MAGIC = 60;

    #endregion
}