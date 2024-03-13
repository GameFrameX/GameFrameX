namespace Server.Utility;

/// <summary>
/// ID生成器，生成唯一ID
/// </summary>
public static class UtilityIdGenerator
{
    private static readonly DateTime utcTimeStart = new(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // 共享计数器
    private static long _counter = (long)(DateTime.UtcNow - utcTimeStart).TotalSeconds;

    /// <summary>
    /// 使用Interlocked.Increment生成唯一ID的方法
    /// </summary>
    /// <returns></returns>
    public static long GetNextUniqueId()
    {
        // 原子性地递增值
        return Interlocked.Increment(ref _counter);
    }
}