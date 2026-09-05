namespace GameFrameX.ProtoExport;

/// <summary>
/// 导出日志静态网关。解耦库内部日志输出与具体宿主（CLI 走 Console，GUI 走 UI 文本框）。
/// </summary>
/// <remarks>
/// 设计动机：原实现中库内部各处直接 <c>Console.WriteLine</c>，GUI 宿主为捕获日志只能全局劫持
/// <c>Console.SetOut</c>——这是反模式（窗口多次构造会泄漏、与 CLI 共存场景冲突、无法分级）。
/// 改用静态网关后：CLI 默认走 <see cref="Console"/>（行为不变），GUI 在启动时设置 <see cref="WriteLine"/>
/// 指向自己的 <see cref="System.IO.StringWriter"/>，消除全局副作用。
/// </remarks>
public static class ExportLogger
{
    private static Action<string> s_writeLine = Console.WriteLine;

    /// <summary>
    /// 单行日志输出委托。默认写 Console，GUI 宿主可替换为 UI 文本框追加。
    /// 赋 null 视为恢复默认 Console 输出（防御后续调用 NRE）。
    /// 委托可能被多线程调用，实现方需自行保证线程安全（GUI 场景由 DispatcherTimer 单线程消费）。
    /// </summary>
    public static Action<string> WriteLine
    {
        get { return s_writeLine; }
        set { s_writeLine = value ?? Console.WriteLine; }
    }
}
