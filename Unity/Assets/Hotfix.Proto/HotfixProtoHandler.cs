using System.Reflection;

/// <summary>
/// 这个类是用来标记协议程序集的。
/// </summary>
public static class HotfixProtoHandler
{
    /// <summary>
    /// 当前程序集
    /// </summary>
    public static Assembly CurrentAssembly
    {
        get { return typeof(HotfixProtoHandler).Assembly; }
    }
}