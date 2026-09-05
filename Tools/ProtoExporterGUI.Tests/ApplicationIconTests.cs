using System;
using System.IO;
using ProtoExporterGUI;
using Xunit;

namespace ProtoExporterGUI.Tests;

public class ApplicationIconTests
{
    /// <summary>
    /// Logo.png 复制到运行目录，供应用级图标使用。
    /// </summary>
    [Fact]
    public void LogoPng_CopiedToRunDirectoryForApplicationIcon()
    {
        var iconPath = MacOSApplicationIcon.GetIconPath(AppContext.BaseDirectory);

        Assert.True(File.Exists(iconPath), iconPath);
    }
}
