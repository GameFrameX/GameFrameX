using System;
using System.Collections.Generic;
using GameFrameX.Apps.Player.Attribute;

namespace GameFrameX.Hotfix.Logic.Player.Attribute;

/// <summary>
/// 玩家登录初始化使用的第一版基础属性默认值。
/// </summary>
internal static class PlayerInitialAttributeDefaults
{
    // ponytail: 当前仓库没有玩家初始属性配置表；配置链路落地后把这里替换为配置读取。
    private static readonly IReadOnlyDictionary<AttributeType, long> DefaultBaseValues = new Dictionary<AttributeType, long>
    {
        [AttributeType.LifeBase] = 1000,
        [AttributeType.PhysicalAttackBase] = 100,
        [AttributeType.MagicAttackBase] = 100,
        [AttributeType.PhysicalDefenseBase] = 50,
        [AttributeType.MagicDefenseBase] = 50,
        [AttributeType.CriticalBase] = 0,
        [AttributeType.CriticalDamageBase] = 15000,
        [AttributeType.PrecisionBase] = 0,
        [AttributeType.BlockBase] = 0
    };

    /// <summary>
    /// 只补齐缺失的基础属性槽，并静默重算对应最终属性。
    /// </summary>
    /// <param name="values">玩家属性状态字典。</param>
    /// <returns>如果状态发生变化则返回 true。</returns>
    public static bool ApplyMissing(Dictionary<int, long> values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var changed = false;
        foreach (var attribute in DefaultBaseValues)
        {
            if (values.ContainsKey((int)attribute.Key))
            {
                continue;
            }

            changed |= PlayerAttributeMutation.ApplyValue(values, attribute.Key, attribute.Value, true).StateChanged;
        }

        return changed;
    }
}
