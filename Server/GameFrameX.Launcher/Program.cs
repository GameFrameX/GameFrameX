using GameFrameX.NetWork.Abstractions;

namespace GameFrameX.Launcher;

internal static class Program
{
    private static async Task Main(string[] args)
    {
#if DEBUG
        LogOptions.Default.IsConsole = true;
#endif
        await GameApp.Entry(args, () =>
        {
            CacheStateTypeManager.Init();
            MessageProtoHelper.Init(typeof(MessageProtoHandler).Assembly);
        }, LogAction);
    }

    private static void LogAction(LogOptions options)
    {
        // 发布之后控制台不输出
#if DEBUG
        options.IsConsole = true;
#else
        options.IsConsole = false;
#endif
    }
}