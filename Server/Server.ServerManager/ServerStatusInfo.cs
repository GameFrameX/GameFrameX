namespace Server.ServerManager;

/// <summary>
/// 服务器状态信息
/// </summary>
public sealed class ServerStatusInfo
{
    /// <summary>
    /// 承载上限
    /// </summary>
    public int MaxLoad { get; set; } = int.MaxValue;

    /// <summary>
    /// 当前承载
    /// </summary>
    public int CurrentLoad { get; set; } = 0;

    public override string ToString()
    {
        return $"MaxLoad:{MaxLoad},CurrentLoad:{CurrentLoad}";
    }
}