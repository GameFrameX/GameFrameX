using System;
using GameFrameX.Apps.Player.Attribute;
using GameFrameX.Apps.Player.Attribute.Entity;
using GameFrameX.Proto.Proto;

namespace GameFrameX.Hotfix.Logic.Player.Attribute;

/// <summary>
/// 玩家属性同步消息构建器。把玩家属性状态和最终值变化转换为面向客户端的同步消息。
/// </summary>
public static class PlayerAttributeSyncBuilder
{
    /// <summary>
    /// 根据玩家属性状态构建完整属性快照消息，包含全部最终值槽及其 Base/Add/Pct 调试字段。
    /// </summary>
    /// <param name="state">玩家属性状态。</param>
    /// <returns>属性快照消息。</returns>
    public static NotifyPlayerAttributeSync BuildSnapshot(PlayerAttributeState state)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var message = new NotifyPlayerAttributeSync();
        foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType)))
        {
            if (attributeType == AttributeType.None)
            {
                continue;
            }

            if (!AttributeCore.IsFinalAttribute(attributeType))
            {
                continue;
            }

            message.Attributes.Add(BuildEntry(state, attributeType));
        }

        return message;
    }

    /// <summary>
    /// 根据最终属性变化构建增量同步消息。
    /// </summary>
    /// <param name="attributeType">发生变化的最终属性编号。</param>
    /// <param name="newValue">新的最终值。</param>
    /// <returns>属性增量消息。</returns>
    public static NotifyPlayerAttributeChanged BuildChanged(AttributeType attributeType, long newValue)
    {
        return new NotifyPlayerAttributeChanged
        {
            Type = (int)attributeType,
            Value = newValue,
        };
    }

    private static PlayerAttributeEntry BuildEntry(PlayerAttributeState state, AttributeType finalAttribute)
    {
        return new PlayerAttributeEntry
        {
            Type = (int)finalAttribute,
            Value = state.GetValue(finalAttribute),
            Base = state.GetValue(AttributeCore.GetSlotAttribute(finalAttribute, AttributeSlotKind.Base)),
            Add = state.GetValue(AttributeCore.GetSlotAttribute(finalAttribute, AttributeSlotKind.Add)),
            Pct = state.GetValue(AttributeCore.GetSlotAttribute(finalAttribute, AttributeSlotKind.Pct)),
        };
    }
}
