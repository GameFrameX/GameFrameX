using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace Server.Log;

public static class LoggerHandler
{
    static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static bool Start()
    {
        try
        {
            Console.WriteLine("init NLog config...");
            LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
            LogManager.Configuration = new XmlLoggingConfiguration("Configs/app_log.config");
            LogManager.AutoShutdown = false;
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"启动服务器失败,异常:{e}");
            return false;
        }
    }
}