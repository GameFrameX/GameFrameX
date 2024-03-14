namespace Server.DBServer.State;

/// <summary>
/// 创建标记
/// </summary>
public interface ISafeCreate
{
    /// <summary>
    /// 创建人
    /// </summary>
    public long CreateId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreateTime { get; set; }
}