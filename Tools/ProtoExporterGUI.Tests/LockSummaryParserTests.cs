using ProtoExporterGUI.Models;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// LockSummaryParser 的契约：
/// 1) 能从导出器输出的「[Lock] 涉及模块 N 个，新增 SubId M 条：…」日志行解析统计；
/// 2) 非 lock 统计行 / null / 空串返回 false，不抛异常；
/// 3) 新增清单为空（无新增）时仍可解析出 0。
/// </summary>
public class LockSummaryParserTests
{
    /// <summary>
    /// 标准统计行解析出模块数与新增数
    /// </summary>
    [Fact]
    public void StandardSummaryLine_ParsesModuleCountAndNewlyCount()
    {
        var ok = LockSummaryParser.TryParse("[Lock] 涉及模块 2 个，新增 SubId 3 条：1.ReqLogin, 2.ReqOpen", out var modules, out var newly);

        Assert.True(ok);
        Assert.Equal(2, modules);
        Assert.Equal(3, newly);
    }

    /// <summary>
    /// 无新增条目时解析出零
    /// </summary>
    [Fact]
    public void NoNewEntries_ParsesZero()
    {
        var ok = LockSummaryParser.TryParse("[Lock] 涉及模块 2 个，新增 SubId 0 条：", out var modules, out var newly);

        Assert.True(ok);
        Assert.Equal(2, modules);
        Assert.Equal(0, newly);
    }

    /// <summary>
    /// 英文统计行同样可解析
    /// </summary>
    [Fact]
    public void EnglishSummaryLine_AlsoParses()
    {
        var ok = LockSummaryParser.TryParse("[Lock] modules affected: 2, newly assigned SubIds: 3: 1.ReqLogin, 2.ReqOpen", out var modules, out var newly);

        Assert.True(ok);
        Assert.Equal(2, modules);
        Assert.Equal(3, newly);
    }

    /// <summary>
    /// 真实 CLI 输出行中英文均可解析
    /// </summary>
    [Fact]
    public void RealCliOutputLine_ParsesBothChineseAndEnglish()
    {
        // 样本采自真实导出输出：zh 首次分配（含清单）、en 幂等轮（新增 0，尾部空格保留）
        Assert.True(LockSummaryParser.TryParse("[Lock] 涉及模块 1 个，新增 SubId 2 条：60.ReqAlpha, 60.RespAlpha", out var zhModules, out var zhNewly));
        Assert.Equal(1, zhModules);
        Assert.Equal(2, zhNewly);

        Assert.True(LockSummaryParser.TryParse("[Lock] modules affected: 1, newly assigned SubIds: 0: ", out var enModules, out var enNewly));
        Assert.Equal(1, enModules);
        Assert.Equal(0, enNewly);
    }

    /// <summary>
    /// 普通日志行解析失败
    /// </summary>
    [Fact]
    public void OrdinaryLogLine_FailsToParse()
    {
        Assert.False(LockSummaryParser.TryParse("协议扫描完成: 共发现 5 个 .proto 文件", out _, out _));
        Assert.False(LockSummaryParser.TryParse("导出成功", out _, out _));
        Assert.False(LockSummaryParser.TryParse(null, out _, out _));
        Assert.False(LockSummaryParser.TryParse(string.Empty, out _, out _));
    }

    /// <summary>
    /// 行首有前缀或缩进_解析失败：正则锚定行首的 [Lock]，时间戳前缀 / 前导空格行均不匹配
    /// </summary>
    [Fact]
    public void LineWithPrefixOrIndent_FailsToParse()
    {
        Assert.False(LockSummaryParser.TryParse("12:00:00 [Lock] 涉及模块 2 个，新增 SubId 3 条：", out _, out _));
        Assert.False(LockSummaryParser.TryParse(" [Lock] modules affected: 2, newly assigned SubIds: 3", out _, out _));
        Assert.False(LockSummaryParser.TryParse("日志 [Lock] 涉及模块 2 个", out _, out _));
    }

    /// <summary>
    /// 数字超出int范围_解析失败：捕获的模块数为超长数字时 int.TryParse 失败，返回 false 不抛异常
    /// </summary>
    [Fact]
    public void NumberBeyondIntRange_FailsToParse()
    {
        var ok = LockSummaryParser.TryParse("[Lock] 涉及模块 99999999999 个，新增 SubId 3 条：", out var modules, out _);

        Assert.False(ok);
        Assert.Equal(0, modules);
    }
}
