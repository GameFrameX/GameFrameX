namespace Server.Log;

public static class LogHelper
{
    public static void Debug(string msg, params object[] args)
    {
        Serilog.Log.Information(msg, args);
    }

    public static void Error(string msg, params object[] args)
    {
        Serilog.Log.Error(msg, args);
    }

    public static void Info(string msg, params object[] args)
    {
        Serilog.Log.Information(msg, args);
    }

    public static void Warn(string msg, params object[] args)
    {
        Serilog.Log.Warning(msg, args);
    }

    public static void Fatal(string msg)
    {
        Serilog.Log.Fatal(msg);
    }

    public static void Fatal(Exception msg)
    {
        Serilog.Log.Fatal(msg, msg.Message);
    }

    public static void Info(Exception msg)
    {
        Serilog.Log.Information(msg.Message);
    }
}