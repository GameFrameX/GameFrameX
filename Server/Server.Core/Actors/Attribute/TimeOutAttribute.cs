namespace Server.Core.Actors;

/// <summary>
/// 超时时间(毫秒)
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TimeOutAttribute : Attribute
{
    /// <summary>
    /// 超时时间
    /// </summary>
    public int Timeout { get; }

    public TimeOutAttribute(int timeout)
    {
        Timeout = timeout;
    }
}