using System.Collections.Generic;
using ProtoExporterGUI.Models;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// ModuleSourceParser 的契约：
/// 1) 能从导出器输出的 module 来源日志行收集 module → source 映射，中英文行格式均兼容
///    （导出器文案随 UI culture 切换：Package X =&gt; Module N (from s) / 包 X =&gt; 模块 N（来源 s））；
/// 2) 同一模块多次出现时，以后出现的为准（最后一次导出生效）；
/// 3) 无匹配行 / null 行 / null 集合返回空字典，不抛异常。
/// </summary>
public class ModuleSourceParserTests
{
    /// <summary>
    /// 英文来源行收集到模块与来源
    /// </summary>
    [Fact]
    public void EnglishSourceLine_CollectsModuleAndSource()
    {
        var map = ModuleSourceParser.Collect(new[]
        {
            "Package Basic => Module 10 (from fileName)",
            "Package ServerInternal => Module -1 (from option)",
        });

        Assert.Equal(2, map.Count);
        Assert.Equal("fileName", map[10]);
        Assert.Equal("option", map[-1]);
    }

    /// <summary>
    /// 中文来源行收集到模块与来源
    /// </summary>
    [Fact]
    public void ChineseSourceLine_CollectsModuleAndSource()
    {
        var map = ModuleSourceParser.Collect(new[]
        {
            "包 Basic => 模块 10（来源 fileName）",
            "包 ServerInternal => 模块 -1（来源 option）",
        });

        Assert.Equal(2, map.Count);
        Assert.Equal("fileName", map[10]);
        Assert.Equal("option", map[-1]);
    }

    /// <summary>
    /// 旧版冒号格式行同样兼容
    /// </summary>
    [Fact]
    public void LegacyColonFormatLine_AlsoSupported()
    {
        var map = ModuleSourceParser.Collect(new[]
        {
            "Package: Basic => Module: 10 (from fileName)",
        });

        Assert.Single(map);
        Assert.Equal("fileName", map[10]);
    }

    /// <summary>
    /// 同模块重复出现时以后出现的为准
    /// </summary>
    [Fact]
    public void RepeatedModule_LastOccurrenceWins()
    {
        var map = ModuleSourceParser.Collect(new[]
        {
            "Package Basic => Module 10 (from option)",
            "包 Basic => 模块 10（来源 fileName）",
        });

        Assert.Single(map);
        Assert.Equal("fileName", map[10]);
    }

    /// <summary>
    /// 普通日志行跳过不收集
    /// </summary>
    [Fact]
    public void OrdinaryLogLine_SkippedNotCollected()
    {
        var map = ModuleSourceParser.Collect(new[]
        {
            "协议扫描完成: 共发现 5 个 .proto 文件，导出 5 个，跳过 0 个（模式: 服务器）",
            "Proto scan completed: found 5 .proto files, exported 5, skipped 0 (mode: server)",
            "[SKIP] client build skips internal proto (moduleId=-1 < 0): _-0120_Inner_Social",
            "导出成功",
        });

        Assert.Empty(map);
    }

    /// <summary>
    /// 真实 CLI 输出行中英文均可解析
    /// </summary>
    [Fact]
    public void RealCliOutputLine_ParsesBothChineseAndEnglish()
    {
        // 样本采自主仓 11 个 proto 的真实导出输出（zh-CN / en-US culture 各跑一次）
        var map = ModuleSourceParser.Collect(new[]
        {
            "包 InnerSocial => 模块 -120（来源 fileName）",
            "Package Basic => Module 10 (from fileName)",
            "包 Plain => 模块 30（来源 option）",
            "Package ServerInternal => Module -1 (from option)",
        });

        Assert.Equal(4, map.Count);
        Assert.Equal("fileName", map[-120]);
        Assert.Equal("fileName", map[10]);
        Assert.Equal("option", map[30]);
        Assert.Equal("option", map[-1]);
    }

    /// <summary>
    /// null 行与 null 集合返回空字典不抛异常
    /// </summary>
    [Fact]
    public void NullLineAndNullCollection_ReturnsEmptyDictionaryWithoutThrowing()
    {
        Assert.Empty(ModuleSourceParser.Collect(null));
        Assert.Empty(ModuleSourceParser.Collect(new string[] { null, string.Empty }));
    }

    /// <summary>
    /// 空集合与空白行_返回空字典：零元素集合、全空白行均按无匹配处理，不抛异常
    /// </summary>
    [Fact]
    public void EmptyCollectionAndBlankLines_ReturnsEmptyDictionary()
    {
        Assert.Empty(ModuleSourceParser.Collect(new string[0]));
        Assert.Empty(ModuleSourceParser.Collect(new[] { "   ", "\t" }));
    }

    /// <summary>
    /// 模块号超short范围_静默跳过：正则能匹配但 TryParse 失败的行被丢弃，不影响其余合法行
    /// </summary>
    [Fact]
    public void ModuleNumberBeyondShortRange_SilentlySkipped()
    {
        var map = ModuleSourceParser.Collect(new[]
        {
            "Package Overflow => Module 99999 (from fileName)",
            "Package Basic => Module 10 (from option)",
        });

        Assert.Single(map);
        Assert.Equal("option", map[10]);
    }

    /// <summary>
    /// 大小写敏感_小写关键字不匹配：正则未开 IgnoreCase，"module" / "package" 小写形态不收集（行为固化）
    /// </summary>
    [Fact]
    public void LowerCaseKeywords_DoNotMatch()
    {
        var map = ModuleSourceParser.Collect(new[]
        {
            "package Basic => module 10 (from fileName)",
            "Package Basic => MODULE 10 (from fileName)",
        });

        Assert.Empty(map);
    }

    /// <summary>
    /// 中英文混搭行_同样兼容：格式的各段（箭头 / 模块词 / 来源括号）独立匹配，支持中英混排
    /// </summary>
    [Fact]
    public void MixedChineseEnglishLine_AlsoCollected()
    {
        var map = ModuleSourceParser.Collect(new[]
        {
            "Package Basic => 模块 10（来源 fileName）",
            "包 Basic => Module 20 (来源: option)",
        });

        Assert.Equal(2, map.Count);
        Assert.Equal("fileName", map[10]);
        Assert.Equal("option", map[20]);
    }
}
