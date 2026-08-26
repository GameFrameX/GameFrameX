using System;
using System.Collections.Generic;
using GameFrameX.Apps.Player.Attribute;

namespace GameFrameX.Hotfix.Logic.Player.Attribute;

/// <summary>
/// 玩家属性状态写入与最终值重算逻辑。
/// </summary>
public static class PlayerAttributeMutation
{
    /// <summary>
    /// 将属性值写入状态字典，并返回最终属性变化判定。
    /// </summary>
    /// <param name="values">状态字典。</param>
    /// <param name="attributeType">属性编号。</param>
    /// <param name="value">新属性值。</param>
    /// <param name="silent">是否静默写入。</param>
    /// <returns>属性写入结果。</returns>
    public static AttributeChangeResult ApplyValue(Dictionary<int, long> values, AttributeType attributeType, long value, bool silent)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var oldValue = GetValue(values, attributeType);
        if (oldValue == value)
        {
            return AttributeChangeResult.NotChanged(attributeType, value);
        }

        values[(int)attributeType] = value;

        if (AttributeCore.TryGetFinalAttribute(attributeType, out var finalAttribute))
        {
            var oldFinalValue = GetValue(values, finalAttribute);
            var newFinalValue = AttributeCore.RecalculateFinal(ToAttributeDictionary(values), finalAttribute);
            values[(int)finalAttribute] = newFinalValue;
            return new AttributeChangeResult(attributeType, finalAttribute, oldFinalValue, newFinalValue, true, oldFinalValue != newFinalValue && !silent);
        }

        if (AttributeCore.IsFinalAttribute(attributeType))
        {
            return new AttributeChangeResult(attributeType, attributeType, oldValue, value, true, !silent);
        }

        return new AttributeChangeResult(attributeType, AttributeType.None, 0L, 0L, true, false);
    }

    private static long GetValue(Dictionary<int, long> values, AttributeType attributeType)
    {
        long value;
        return values.TryGetValue((int)attributeType, out value) ? value : 0L;
    }

    private static Dictionary<AttributeType, long> ToAttributeDictionary(Dictionary<int, long> values)
    {
        var attributes = new Dictionary<AttributeType, long>();
        foreach (var item in values)
        {
            attributes[(AttributeType)item.Key] = item.Value;
        }

        return attributes;
    }
}

/// <summary>
/// 属性写入后的最终属性变化判定。
/// </summary>
public sealed class AttributeChangeResult
{
    public AttributeType SourceAttributeType { get; }

    public AttributeType FinalAttributeType { get; }

    public long OldFinalValue { get; }

    public long NewFinalValue { get; }

    public bool StateChanged { get; }

    public bool ShouldDispatch { get; }

    public AttributeChangeResult(AttributeType sourceAttributeType, AttributeType finalAttributeType, long oldFinalValue, long newFinalValue, bool stateChanged, bool shouldDispatch)
    {
        SourceAttributeType = sourceAttributeType;
        FinalAttributeType = finalAttributeType;
        OldFinalValue = oldFinalValue;
        NewFinalValue = newFinalValue;
        StateChanged = stateChanged;
        ShouldDispatch = shouldDispatch;
    }

    public static AttributeChangeResult NotChanged(AttributeType sourceAttributeType, long value)
    {
        return new AttributeChangeResult(sourceAttributeType, AttributeType.None, value, value, false, false);
    }
}
