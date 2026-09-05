using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ProtoExporterGUI.Resources;
using Xunit;

namespace ProtoExporterGUI.Tests;

public class LocalizationTests : IDisposable
{
    readonly CultureInfo _originalCurrentCulture = Thread.CurrentThread.CurrentUICulture;
    readonly CultureInfo _originalDefaultCulture = CultureInfo.DefaultThreadCurrentUICulture;

    public void Dispose()
    {
        Thread.CurrentThread.CurrentUICulture = _originalCurrentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _originalDefaultCulture;
    }

    /// <summary>
    /// WindowTitle 按当前语言显示友好名称。
    /// </summary>
    [Fact]
    public void WindowTitle_ShowsFriendlyNamePerCurrentCulture()
    {
        Localization.Instance.SetCulture("zh-CN");
        Assert.Equal("ProtoExporterGUI 协议导出工具", Localization.Instance.WindowTitle);

        Localization.Instance.SetCulture("en");
        Assert.Equal("ProtoExporterGUI", Localization.Instance.WindowTitle);
    }

    /// <summary>
    /// 空白culture码_静默忽略语言不变：null / 空串 / 纯空白的 culture 码不切换语言、不抛异常。
    /// </summary>
    [Fact]
    public void BlankCultureCode_KeepsCurrentLanguage()
    {
        Localization.Instance.SetCulture("en");
        var before = Localization.Instance.WindowTitle;

        var ex = Record.Exception(() =>
        {
            Localization.Instance.SetCulture(null);
            Localization.Instance.SetCulture(string.Empty);
            Localization.Instance.SetCulture("   ");
        });

        Assert.Null(ex);
        var after = Localization.Instance.WindowTitle;
        Assert.Equal(before, after);
        Assert.Equal("ProtoExporterGUI", after);
    }

    /// <summary>
    /// 无效culture码_静默回退语言不变：不存在的 culture 代码被 CultureNotFoundException 分支吞掉，语言保持。
    /// </summary>
    [Fact]
    public void InvalidCultureCode_KeepsCurrentLanguage()
    {
        Localization.Instance.SetCulture("zh-CN");
        var before = Localization.Instance.WindowTitle;

        var ex = Record.Exception(() => Localization.Instance.SetCulture("xx-INVALID-NO-SUCH-CULTURE"));

        Assert.Null(ex);
        Assert.Equal(before, Localization.Instance.WindowTitle);
    }

    /// <summary>
    /// 索引器缺失key_返回key本身：与 Loc.Get 同款兜底，暴露遗漏的翻译条目而非抛异常。
    /// </summary>
    [Fact]
    public void MissingKey_ReturnsKeyItself()
    {
        Assert.Equal("No_Such_Key_Should_Ever_Exist", Localization.Instance["No_Such_Key_Should_Ever_Exist"]);
    }

    /// <summary>
    /// 切换语言_对每个具名属性触发PropertyChanged：Avalonia 绑定刷新契约，
    /// 触发序列必须与 LocalizedPropertyNames 登记表逐项一致（新增属性漏登记会被此测试抓住）。
    /// </summary>
    [Fact]
    public void SetCulture_RaisesPropertyChangedForEveryRegisteredProperty()
    {
        var expected = (string[])typeof(Localization)
            .GetField("LocalizedPropertyNames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .GetValue(null);
        Assert.NotEmpty(expected);

        var events = new List<string>();
        void Handler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            events.Add(e.PropertyName);
        }

        Localization.Instance.PropertyChanged += Handler;
        try
        {
            Localization.Instance.SetCulture("en");
        }
        finally
        {
            Localization.Instance.PropertyChanged -= Handler;
        }

        Assert.Equal(expected, events);
    }
}
