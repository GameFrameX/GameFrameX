using System.Globalization;
using System.Threading;
using GameFrameX.ProtoExport;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// Loc 本地化网关的契约：
/// 1) 同一 key 按 CurrentUICulture 切换返回对应语言（en 卫星 / zh 回退 neutral 简中）；
/// 2) 未知 key 返回 key 本身（暴露遗漏条目，不抛异常）。
/// </summary>
/// <remarks>
/// 线程 culture 是共享状态：测试内显式设置并在 finally 恢复，避免污染并行测试。
/// </remarks>
public class LocTests
{
    /// <summary>
    /// 同一 key 按 UICulture 切换语言
    /// </summary>
    [Fact]
    public void SameKey_SwitchesLanguageByCurrentUICulture()
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var en = Loc.Log_ModuleMismatch;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
            var zh = Loc.Log_ModuleMismatch;

            Assert.Equal("Module ID mismatch", en);
            Assert.Equal("模块 ID 不一致", zh);
        }
        finally
        {
            Thread.CurrentThread.CurrentUICulture = original;
        }
    }

    /// <summary>
    /// 复合格式占位符在两种语言下参数顺序一致
    /// </summary>
    [Fact]
    public void CompositeFormatPlaceholders_ParameterOrderConsistentInBothLanguages()
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var en = string.Format(Loc.Err_ModuleMismatch, "F", 1, 2);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
            var zh = string.Format(Loc.Err_ModuleMismatch, "F", 1, 2);

            Assert.Contains("F", en);
            Assert.Contains("F", zh);
            Assert.Contains("1", en);
            Assert.Contains("1", zh);
            Assert.Contains("2", en);
            Assert.Contains("2", zh);
        }
        finally
        {
            Thread.CurrentThread.CurrentUICulture = original;
        }
    }

    /// <summary>
    /// 未知 key 返回 key 本身不抛异常
    /// </summary>
    [Fact]
    public void UnknownKey_ReturnsKeyItselfWithoutThrowing()
    {
        Assert.Equal("No_Such_Key_Should_Ever_Exist", Loc.Get("No_Such_Key_Should_Ever_Exist"));
    }

    /// <summary>
    /// null 或空 key 原样返回不抛异常（ResourceManager.GetString(null) 会抛 ArgumentNullException，网关层已拦截）
    /// </summary>
    [Fact]
    public void NullOrEmptyKey_ReturnsAsIsWithoutThrowing()
    {
        Assert.Null(Loc.Get(null));
        Assert.Equal(string.Empty, Loc.Get(string.Empty));
    }
}
