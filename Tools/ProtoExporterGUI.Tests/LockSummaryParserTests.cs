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
    [Fact]
    public void 标准统计行_解析出模块数与新增数()
    {
        var ok = LockSummaryParser.TryParse("[Lock] 涉及模块 2 个，新增 SubId 3 条：1.ReqLogin, 2.ReqOpen", out var modules, out var newly);

        Assert.True(ok);
        Assert.Equal(2, modules);
        Assert.Equal(3, newly);
    }

    [Fact]
    public void 无新增条目_解析出零()
    {
        var ok = LockSummaryParser.TryParse("[Lock] 涉及模块 2 个，新增 SubId 0 条：", out var modules, out var newly);

        Assert.True(ok);
        Assert.Equal(2, modules);
        Assert.Equal(0, newly);
    }

    [Fact]
    public void 普通日志行_解析失败()
    {
        Assert.False(LockSummaryParser.TryParse("协议扫描完成: 共发现 5 个 .proto 文件", out _, out _));
        Assert.False(LockSummaryParser.TryParse("导出成功", out _, out _));
        Assert.False(LockSummaryParser.TryParse(null, out _, out _));
        Assert.False(LockSummaryParser.TryParse(string.Empty, out _, out _));
    }
}
