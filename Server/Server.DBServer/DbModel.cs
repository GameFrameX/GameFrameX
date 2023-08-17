namespace Server.DBServer;

public enum DbModel
{
    /// <summary>
    /// 内嵌做主存,mongodb备份
    /// </summary>
    Embedded,

    /// <summary>
    /// mongodb主存,存储失败再存内嵌
    /// </summary>
    Mongodb,
}