using NLog;
using Server.EntryUtility;
using Server.Launcher.Common;

namespace Server.Launcher
{
    internal static class Program
    {
        private static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            await AppEnter.Entry(AppStartUp.Enter, Log, args);
        }
    }
}