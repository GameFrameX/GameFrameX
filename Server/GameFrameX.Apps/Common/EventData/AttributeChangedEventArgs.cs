using GameFrameX.Apps.Player.Attribute;
using GameFrameX.Core.Abstractions.Events;

namespace GameFrameX.Apps.Common.EventData;

/// <summary>
/// 玩家最终属性变化事件参数。
/// </summary>
public sealed class AttributeChangedEventArgs : GameEventArgs
{
    /// <summary>
    /// 玩家ID。
    /// </summary>
    public long PlayerId { get; }

    /// <summary>
    /// 发生变化的最终属性。
    /// </summary>
    public AttributeType AttributeType { get; }

    /// <summary>
    /// 触发重算的属性槽。
    /// </summary>
    public AttributeType SourceAttributeType { get; }

    /// <summary>
    /// 旧最终属性值。
    /// </summary>
    public long OldValue { get; }

    /// <summary>
    /// 新最终属性值。
    /// </summary>
    public long NewValue { get; }

    /// <summary>
    /// 初始化属性变化事件参数。
    /// </summary>
    /// <param name="playerId">玩家ID。</param>
    /// <param name="attributeType">发生变化的最终属性。</param>
    /// <param name="sourceAttributeType">触发重算的属性槽。</param>
    /// <param name="oldValue">旧最终属性值。</param>
    /// <param name="newValue">新最终属性值。</param>
    public AttributeChangedEventArgs(long playerId, AttributeType attributeType, AttributeType sourceAttributeType, long oldValue, long newValue)
    {
        PlayerId = playerId;
        AttributeType = attributeType;
        SourceAttributeType = sourceAttributeType;
        OldValue = oldValue;
        NewValue = newValue;
    }
}
