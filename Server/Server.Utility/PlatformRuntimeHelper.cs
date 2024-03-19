using System.Runtime.InteropServices;

namespace Server.Utility;

public static class PlatformRuntimeHelper
{
    public static bool IsLinux
    {
        get
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
    }
    public static bool IsOSX
    {
        get
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }
    }
    public static bool IsWindows
    {
        get
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }
}