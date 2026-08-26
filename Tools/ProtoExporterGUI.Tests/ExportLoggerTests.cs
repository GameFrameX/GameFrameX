using System;
using System.Collections.Generic;
using GameFrameX.ProtoExport;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// ExportLogger 日志网关单元测试。
/// </summary>
/// <remarks>
/// ExportLogger.WriteLine 是静态可变委托（默认 Console.WriteLine），GUI 宿主启动时替换为 UI 追加。
/// 委托是进程级共享状态，测试间会互相污染，故每个测试前后恢复默认 Console.WriteLine。
/// </remarks>
public class ExportLoggerTests : IDisposable
{
    public ExportLoggerTests()
    {
        RestoreDefault();
    }

    public void Dispose()
    {
        RestoreDefault();
    }

    private static void RestoreDefault()
        => ExportLogger.WriteLine = Console.WriteLine;

    /// <summary>
    /// 默认委托非空（指向 Console.WriteLine），保证 CLI 宿主行为不变。
    /// </summary>
    [Fact]
    public void 默认委托非空_指向Console()
    {
        Assert.NotNull(ExportLogger.WriteLine);
    }

    /// <summary>
    /// 替换委托后，WriteLine 调用必须走新委托。
    /// </summary>
    [Fact]
    public void 替换委托后调用走新委托()
    {
        var captured = new List<string>();
        ExportLogger.WriteLine = captured.Add;

        ExportLogger.WriteLine("hello");
        ExportLogger.WriteLine("world");

        Assert.Equal(new[] { "hello", "world" }, captured);
    }

    /// <summary>
    /// 再次赋值会覆盖前一个委托，确保 GUI 宿主可随时重定向输出。
    /// </summary>
    [Fact]
    public void 再次替换覆盖前一个委托()
    {
        var first = new List<string>();
        var second = new List<string>();
        ExportLogger.WriteLine = first.Add;
        ExportLogger.WriteLine = second.Add;

        ExportLogger.WriteLine("only-second");

        Assert.Empty(first);
        Assert.Single(second);
        Assert.Equal("only-second", second[0]);
    }
}
