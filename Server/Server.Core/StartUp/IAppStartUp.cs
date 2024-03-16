using Server.Setting;

namespace Server.Core.StartUp;

/// <summary>
/// 程序启动器基类接口定义
/// </summary>
public interface IAppStartUp
{
    Task<string> AppExitToken { get; }
    ServerType ServerType { get; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="serverType">服务器类型</param>
    /// <param name="setting">启动设置</param>
    /// <param name="args">启动参数</param>
    /// <returns></returns>
    bool Init(ServerType serverType, BaseSetting setting, string[] args);

    /// <summary>
    /// 启动
    /// </summary>
    Task EnterAsync();

    /// <summary>
    /// 终止服务器
    /// </summary>
    /// <param name="message">终止原因</param>
    Task Stop(string message = "");
}