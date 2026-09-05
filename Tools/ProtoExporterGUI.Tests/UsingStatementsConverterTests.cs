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
    /// <summary>
    /// PipeToMultiline：空值返回空字符串。
    /// </summary>
    [Fact]
    public void PipeToMultiline_EmptyValue_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, MainWindow.PipeToMultiline(null));
        Assert.Equal(string.Empty, MainWindow.PipeToMultiline(string.Empty));
    }

    /// <summary>
    /// PipeToMultiline：单条语句，无换行。
    /// </summary>
    [Fact]
    public void PipeToMultiline_SingleStatement_NoNewline()
    {
        // C# 单条 using 语句
        Assert.Equal("using System;", MainWindow.PipeToMultiline("using System;"));
        // C++ 单条 include
        Assert.Equal("#include <string>", MainWindow.PipeToMultiline("#include <string>"));
    }

    /// <summary>
    /// PipeToMultiline：多条语句，每条一行。
    /// </summary>
    [Fact]
    public void PipeToMultiline_MultipleStatements_OnePerLine()
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

    /// <summary>
    /// PipeToMultiline：去除空段和首尾空白。
    /// </summary>
    [Fact]
    public void PipeToMultiline_RemovesEmptySegmentsAndTrimsWhitespace()
    {
        // 含空段、前后空白
        var pipe = "  using System  ||  using ProtoBuf  |";
        var result = MainWindow.PipeToMultiline(pipe);
        var lines = result.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.Equal("using System", lines[0]);
        Assert.Equal("using ProtoBuf", lines[1]);
    }

    /// <summary>
    /// MultilineToPipe：空值返回空字符串。
    /// </summary>
    [Fact]
    public void MultilineToPipe_EmptyValue_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, MainWindow.MultilineToPipe(null));
        Assert.Equal(string.Empty, MainWindow.MultilineToPipe(string.Empty));
        Assert.Equal(string.Empty, MainWindow.MultilineToPipe("   "));
    }

    /// <summary>
    /// MultilineToPipe：单行。
    /// </summary>
    [Fact]
    public void MultilineToPipe_SingleLine()
    {
        Assert.Equal("using System", MainWindow.MultilineToPipe("using System"));
    }

    /// <summary>
    /// MultilineToPipe：多行用竖线拼接。
    /// </summary>
    [Fact]
    public void MultilineToPipe_JoinsMultipleLinesWithPipe()
    {
        var multiline = "using System" + Environment.NewLine +
                        "using ProtoBuf" + Environment.NewLine +
                        "using System.Collections.Generic";
        Assert.Equal("using System|using ProtoBuf|using System.Collections.Generic",
            MainWindow.MultilineToPipe(multiline));
    }

    /// <summary>
    /// MultilineToPipe：兼容三种换行符。
    /// </summary>
    [Fact]
    public void MultilineToPipe_SupportsThreeNewlineStyles()
    {
        // Windows CRLF
        Assert.Equal("a|b", MainWindow.MultilineToPipe("a\r\nb"));
        // 旧 Mac CR
        Assert.Equal("a|b", MainWindow.MultilineToPipe("a\rb"));
        // Unix LF
        Assert.Equal("a|b", MainWindow.MultilineToPipe("a\nb"));
    }

    /// <summary>
    /// MultilineToPipe：去除空行和首尾空白。
    /// </summary>
    [Fact]
    public void MultilineToPipe_RemovesEmptyLinesAndTrimsWhitespace()
    {
        var multiline = "  using System  " + Environment.NewLine +
                        Environment.NewLine +
                        "  using ProtoBuf  " + Environment.NewLine +
                        "   ";
        Assert.Equal("using System|using ProtoBuf", MainWindow.MultilineToPipe(multiline));
    }

    /// <summary>
    /// 往返转换：保持等价、幂等。
    /// </summary>
    [Fact]
    public void RoundTrip_PreservesEquivalence_IsIdempotent()
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

    /// <summary>
    /// 往返转换：空值稳定。
    /// </summary>
    [Fact]
    public void RoundTrip_EmptyValue_StaysStable()
    {
        Assert.Equal(string.Empty, MainWindow.MultilineToPipe(MainWindow.PipeToMultiline(string.Empty)));
        Assert.Equal(string.Empty, MainWindow.PipeToMultiline(MainWindow.MultilineToPipe(string.Empty)));
    }

    /// <summary>
    /// PipeToMultiline：纯空白输入返回空字符串（与 MultilineToPipe 的空白用例对称）。
    /// </summary>
    [Fact]
    public void PipeToMultiline_WhitespaceOnly_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, MainWindow.PipeToMultiline("   "));
        Assert.Equal(string.Empty, MainWindow.PipeToMultiline("\t"));
    }

    /// <summary>
    /// PipeToMultiline：仅分隔符输入返回空字符串（全部段为空被滤除）。
    /// </summary>
    [Fact]
    public void PipeToMultiline_SeparatorsOnly_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, MainWindow.PipeToMultiline("|||"));
        Assert.Equal(string.Empty, MainWindow.PipeToMultiline(" | | "));
    }

    /// <summary>
    /// MultilineToPipe：同一段文本内混合三种换行符，逐行拆分后统一拼接。
    /// </summary>
    [Fact]
    public void MultilineToPipe_MixedNewlineStylesInOneValue()
    {
        Assert.Equal("a|b|c|d", MainWindow.MultilineToPipe("a\r\nb\rc\nd"));
    }
}
