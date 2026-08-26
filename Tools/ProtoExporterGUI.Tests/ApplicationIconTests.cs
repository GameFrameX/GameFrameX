using System;
using System.IO;
using ProtoExporterGUI;
using Xunit;

namespace ProtoExporterGUI.Tests;

public class ApplicationIconTests
{
    [Fact]
    public void LogoPng_复制到运行目录供应用级图标使用()
    {
        var iconPath = MacOSApplicationIcon.GetIconPath(AppContext.BaseDirectory);

        Assert.True(File.Exists(iconPath), iconPath);
    }
}
