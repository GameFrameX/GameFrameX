using Server.Setting;

namespace Server.Core.StartUp.Attributes;

/// <summary>
/// 启动属性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class StartUpTagAttribute : Attribute
{
    /// <summary>
    /// 服务器类型
    /// </summary>
    public readonly ServerType ServerType;

    /// <summary>
    /// 启动优先级。值越小优先级越高
    /// </summary>
    public readonly int Priority;

    /// <summary>
    /// 构建启动属性
    /// </summary>
    /// <param name="serverType">服务器类型</param>
    /// <param name="priority">优先级，默认为1000</param>
    public StartUpTagAttribute(ServerType serverType, int priority = 1000)
    {
        ServerType = serverType;
        Priority = priority;
    }
}