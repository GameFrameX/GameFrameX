using Serilog;

namespace Server.Log;

public static class LoggerHandler
{
    public static bool Start()
    {
        try
        {
            var logPath = @".\logs\log.txt"; // 日志文件存储的路径
            LogHelper.Info("init Log config...");
            Serilog.Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(logPath)
                .CreateLogger();
            return true;
        }
        catch (Exception e)
        {
            Serilog.Log.Error($"启动服务器失败,异常:{e}");
            return false;
        }
    }
}