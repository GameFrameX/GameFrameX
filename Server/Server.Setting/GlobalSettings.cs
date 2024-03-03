using Newtonsoft.Json;

namespace Server.Setting;

/// <summary>
/// 全局设置
/// </summary>
public static class GlobalSettings
{
    private static readonly List<BaseSetting> Settings = new List<BaseSetting>(16);
    public static bool IsAppRunning { get; set; }
    public static DateTime LaunchTime { get; set; }
    public static bool IsDebug { get; set; }
    public static int ServerId { get; set; }

    public static void Load<T>(string path) where T : BaseSetting
    {
        Settings.Clear();
        var configJson = File.ReadAllText(path);
        var settings = JsonConvert.DeserializeObject<List<T>>(configJson) ?? throw new InvalidOperationException();

        foreach (var setting in settings)
        {
            if (setting.ServerId < GlobalConst.MinServerId || setting.ServerId > GlobalConst.MaxServerId)
            {
                throw new Exception($"ServerId不合法{setting.ServerId},需要在[{GlobalConst.MinServerId},{GlobalConst.MaxServerId}]范围之内");
            }

            Settings.Add(setting);
        }
    }

    public static List<T> GetSettings<T>() where T : BaseSetting
    {
        List<T> result = new List<T>();
        foreach (var setting in Settings)
        {
            result.Add((setting as T)!);
        }

        return result;
    }

    public static List<T?> GetSettings<T>(ServerType serverType) where T : BaseSetting
    {
        List<T?> result = new List<T?>();
        foreach (var setting in Settings)
        {
            if ((setting.ServerType &= serverType) != 0)
            {
                result.Add(setting as T);
            }
        }

        return result;
    }

    public static T? GetSetting<T>(ServerType serverType) where T : BaseSetting
    {
        foreach (var setting in Settings)
        {
            if ((setting.ServerType &= serverType) != 0)
            {
                return setting as T;
            }
        }

        return null;
    }
}