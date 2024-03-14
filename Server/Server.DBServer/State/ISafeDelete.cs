namespace Server.DBServer.State;

/// <summary>
/// 软删除标记
/// </summary>
public interface ISafeDelete
{
    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public long DeleteTime { get; set; }
}