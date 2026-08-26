using System.Collections.Concurrent;
using GameFrameX.Core.Config;

namespace GameFrameX.Config;

/// <summary>
/// 全局配置管理器。
/// </summary>
internal sealed partial class ConfigManager : IConfigManager
{
    private readonly ConcurrentDictionary<string, IDataTable> m_ConfigDatas;

    /// <summary>
    /// 初始化全局配置管理器的新实例。
    /// </summary>
    public ConfigManager()
    {
        m_ConfigDatas = new ConcurrentDictionary<string, IDataTable>(StringComparer.Ordinal);
    }

    /// <summary>
    /// 获取全局配置项数量。
    /// </summary>
    public int Count
    {
        get { return m_ConfigDatas.Count; }
    }

    /// <summary>
    /// 检查是否存在指定全局配置项。
    /// </summary>
    /// <param name="configName">要检查全局配置项的名称。</param>
    /// <returns>指定的全局配置项是否存在。</returns>
    public bool HasConfig(string configName)
    {
        return m_ConfigDatas.TryGetValue(configName, out _);
    }


    /// <summary>
    /// 增加指定全局配置项。
    /// </summary>
    /// <param name="configName">要增加全局配置项的名称。</param>
    /// <param name="configValue">全局配置项的值。</param>
    /// <returns>是否增加全局配置项成功。</returns>
    public void AddConfig(string configName, IDataTable configValue)
    {
        var isExist = m_ConfigDatas.TryGetValue(configName, out var value);
        if (isExist)
        {
            return;
        }

        m_ConfigDatas.TryAdd(configName, configValue);
    }

    /// <summary>
    /// 移除指定全局配置项。
    /// </summary>
    /// <param name="configName">要移除全局配置项的名称。</param>
    public bool RemoveConfig(string configName)
    {
        if (!HasConfig(configName))
        {
            return false;
        }

        return m_ConfigDatas.TryRemove(configName, out _);
    }

    /// <summary>
    /// 获取指定全局配置项。
    /// </summary>
    /// <param name="configName">要获取全局配置项的名称。</param>
    /// <returns>要获取全局配置项的全局配置项。</returns>
    public IDataTable GetConfig(string configName)
    {
        return m_ConfigDatas.TryGetValue(configName, out var value) ? value : null; //GetConfig()
    }

    /// <summary>
    /// 清空所有全局配置项。
    /// </summary>
    public void RemoveAllConfigs()
    {
        m_ConfigDatas.Clear();
    }
}