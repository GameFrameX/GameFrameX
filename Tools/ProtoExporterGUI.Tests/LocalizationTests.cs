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

    [Fact]
    public void WindowTitle_按当前语言显示友好名称()
    {
        Localization.Instance.SetCulture("zh-CN");
        Assert.Equal("ProtoExporterGUI 协议导出工具", Localization.Instance.WindowTitle);

        Localization.Instance.SetCulture("en");
        Assert.Equal("ProtoExporterGUI", Localization.Instance.WindowTitle);
    }
}
