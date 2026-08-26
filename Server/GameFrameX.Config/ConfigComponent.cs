using System.Text.Json;
using GameFrameX.Core.Config;
using GameFrameX.Foundation.Logger;

namespace GameFrameX.Config;

public class ConfigComponent
{
    private readonly ConfigManager _configManager;

    private ConfigComponent()
    {
        _configManager = new ConfigManager();
        Tables = new TablesComponent();
    }

    public static ConfigComponent Instance { get; } = new();

    private TablesComponent Tables { get; }

    public async Task LoadConfig()
    {
        Tables.Init(Instance);
        LogHelper.Info("Load Config Start...");
        Instance.RemoveAllConfigs();
        await Tables.LoadAsync(Loader);
        LogHelper.Info("Load Config End...");
        LogHelper.Info("== load success ==");
    }

    private static async Task<ByteBuf> Loader(string file, bool tag)
    {
        var configBytes = await File.ReadAllBytesAsync($"json/{file}.bytes");
        return ByteBuf.Wrap(configBytes);
    }

    private static async Task<JsonElement> Loader(string file)
    {
        var configJson = await File.ReadAllTextAsync($"json/{file}.json");
        var jsonElement = JsonDocument.Parse(configJson).RootElement;
        return jsonElement;
    }

    /// <summary>
    /// 获取指定全局配置项。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetConfig<T>() where T : IDataTable
    {
        if (HasConfig<T>())
        {
            var configName = typeof(T).Name;
            var config = _configManager.GetConfig(configName);
            if (config != null)
            {
                return (T)config;
            }
        }

        return default;
    }

    /// <summary>
    /// 检查是否存在指定全局配置项。
    /// </summary>
    /// <returns>指定的全局配置项是否存在。</returns>
    public bool HasConfig<T>() where T : IDataTable
    {
        var configName = typeof(T).Name;
        return _configManager.HasConfig(configName);
    }

    /// <summary>
    /// 移除指定全局配置项。
    /// </summary>
    /// <returns>是否移除全局配置项成功。</returns>
    public bool RemoveConfig<T>() where T : IDataTable<T>
    {
        var configName = typeof(T).Name;
        return _configManager.RemoveConfig(configName);
    }

    /// <summary>
    /// 清空所有全局配置项。
    /// </summary>
    public void RemoveAllConfigs()
    {
        _configManager.RemoveAllConfigs();
    }

    /// <summary>
    /// 增加
    /// </summary>
    /// <param name="configName"></param>
    /// <param name="dataTable"></param>
    public void Add(string configName, IDataTable dataTable)
    {
        _configManager.AddConfig(configName, dataTable);
    }
}