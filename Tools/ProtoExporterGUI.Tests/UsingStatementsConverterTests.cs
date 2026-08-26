using System;
using ProtoExporterGUI.Views;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// 验证 using 语句在 UI（多行）与数据层（| 分隔）之间的双向转换。
/// 数据层 LauncherOptions.UsingStatements 以 | 分隔，与 CLI --usingStatements 契约一致；
/// UI 每行一条便于编辑，转换必须保持等价且幂等。
/// </summary>
public class UsingStatementsConverterTests
{
    [Fact]
    public void PipeToMultiline_空值返回空字符串()
    {
        Assert.Equal(string.Empty, MainWindow.PipeToMultiline(null));
        Assert.Equal(string.Empty, MainWindow.PipeToMultiline(string.Empty));
    }

    [Fact]
    public void PipeToMultiline_单条语句_无换行()
    {
        // C# 单条 using 语句
        Assert.Equal("using System;", MainWindow.PipeToMultiline("using System;"));
        // C++ 单条 include
        Assert.Equal("#include <string>", MainWindow.PipeToMultiline("#include <string>"));
    }

    [Fact]
    public void PipeToMultiline_多条语句_每条一行()
    {
        var pipe = "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions";
        var result = MainWindow.PipeToMultiline(pipe);
        var lines = result.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        Assert.Equal(4, lines.Length);
        Assert.Equal("using System", lines[0]);
        Assert.Equal("using ProtoBuf", lines[1]);
        Assert.Equal("using System.Collections.Generic", lines[2]);
        Assert.Equal("using GameFrameX.NetWork.Abstractions", lines[3]);
    }

    [Fact]
    public void PipeToMultiline_去除空段和首尾空白()
    {
        // 含空段、前后空白
        var pipe = "  using System  ||  using ProtoBuf  |";
        var result = MainWindow.PipeToMultiline(pipe);
        var lines = result.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.Equal("using System", lines[0]);
        Assert.Equal("using ProtoBuf", lines[1]);
    }

    [Fact]
    public void MultilineToPipe_空值返回空字符串()
    {
        Assert.Equal(string.Empty, MainWindow.MultilineToPipe(null));
        Assert.Equal(string.Empty, MainWindow.MultilineToPipe(string.Empty));
        Assert.Equal(string.Empty, MainWindow.MultilineToPipe("   "));
    }

    [Fact]
    public void MultilineToPipe_单行()
    {
        Assert.Equal("using System", MainWindow.MultilineToPipe("using System"));
    }

    [Fact]
    public void MultilineToPipe_多行用竖线拼接()
    {
        var multiline = "using System" + Environment.NewLine +
                        "using ProtoBuf" + Environment.NewLine +
                        "using System.Collections.Generic";
        Assert.Equal("using System|using ProtoBuf|using System.Collections.Generic",
            MainWindow.MultilineToPipe(multiline));
    }

    [Fact]
    public void MultilineToPipe_兼容三种换行符()
    {
        // Windows CRLF
        Assert.Equal("a|b", MainWindow.MultilineToPipe("a\r\nb"));
        // 旧 Mac CR
        Assert.Equal("a|b", MainWindow.MultilineToPipe("a\rb"));
        // Unix LF
        Assert.Equal("a|b", MainWindow.MultilineToPipe("a\nb"));
    }

    [Fact]
    public void MultilineToPipe_去除空行和首尾空白()
    {
        var multiline = "  using System  " + Environment.NewLine +
                        Environment.NewLine +
                        "  using ProtoBuf  " + Environment.NewLine +
                        "   ";
        Assert.Equal("using System|using ProtoBuf", MainWindow.MultilineToPipe(multiline));
    }

    [Fact]
    public void 往返转换_保持等价_幂等()
    {
        // 真实场景：Server 模式的 using 集合
        var original = "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages";

        // | → 多行 → |，应与原始等价
        var roundTrip = MainWindow.MultilineToPipe(MainWindow.PipeToMultiline(original));
        Assert.Equal(original, roundTrip);

        // 二次往返稳定（幂等）
        var secondRoundTrip = MainWindow.MultilineToPipe(MainWindow.PipeToMultiline(roundTrip));
        Assert.Equal(roundTrip, secondRoundTrip);
    }

    [Fact]
    public void 往返转换_空值稳定()
    {
        Assert.Equal(string.Empty, MainWindow.MultilineToPipe(MainWindow.PipeToMultiline(string.Empty)));
        Assert.Equal(string.Empty, MainWindow.PipeToMultiline(MainWindow.MultilineToPipe(string.Empty)));
    }
}
