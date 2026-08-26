using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ProtoExporterGUI;

internal static class MacOSApplicationIcon
{
    internal const string IconRelativePath = "Assets/logo.png";

    const string ObjectiveCLibrary = "/usr/lib/libobjc.A.dylib";

    public static void Apply()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        var iconPath = GetIconPath(AppContext.BaseDirectory);
        if (!File.Exists(iconPath))
        {
            return;
        }

        try
        {
            Apply(iconPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to apply macOS application icon: " + ex.Message);
        }
    }

    internal static string GetIconPath(string baseDirectory)
        => Path.Combine(baseDirectory, IconRelativePath);

    static void Apply(string iconPath)
    {
        var appClass = objc_getClass("NSApplication");
        var imageClass = objc_getClass("NSImage");
        if (appClass == IntPtr.Zero || imageClass == IntPtr.Zero)
        {
            return;
        }

        var app = objc_msgSend(appClass, sel_registerName("sharedApplication"));
        if (app == IntPtr.Zero)
        {
            return;
        }

        var nsPath = CreateNSString(iconPath);
        if (nsPath == IntPtr.Zero)
        {
            return;
        }

        var image = objc_msgSend(imageClass, sel_registerName("alloc"));
        image = objc_msgSend(image, sel_registerName("initWithContentsOfFile:"), nsPath);
        objc_msgSend(nsPath, sel_registerName("release"));
        if (image == IntPtr.Zero)
        {
            return;
        }

        objc_msgSend(app, sel_registerName("setApplicationIconImage:"), image);
        objc_msgSend(image, sel_registerName("release"));
    }

    static IntPtr CreateNSString(string value)
    {
        var utf8 = Marshal.StringToCoTaskMemUTF8(value);
        try
        {
            var nsStringClass = objc_getClass("NSString");
            if (nsStringClass == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            var nsString = objc_msgSend(nsStringClass, sel_registerName("alloc"));
            return nsString == IntPtr.Zero
                ? IntPtr.Zero
                : objc_msgSend(nsString, sel_registerName("initWithUTF8String:"), utf8);
        }
        finally
        {
            Marshal.FreeCoTaskMem(utf8);
        }
    }

    [DllImport(ObjectiveCLibrary)]
    static extern IntPtr objc_getClass(string name);

    [DllImport(ObjectiveCLibrary)]
    static extern IntPtr sel_registerName(string name);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr argument);
}
