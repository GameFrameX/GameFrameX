namespace Server.DBServer.State;

/// <summary>
/// 回存时间戳
/// </summary>
public class SaveTimestamp
{
    /// <summary>
    /// State.FullName_State.Id
    /// </summary>
    public string Key => StateName + "_" + StateId;

    /// <summary>
    /// 状态名称
    /// </summary>
    public string StateName { set; get; }

    /// <summary>
    /// 状态ID
    /// </summary>
    public string StateId { set; get; }

    /// <summary>
    /// 回存时间戳
    /// </summary>
    public long Timestamp { get; set; }
}