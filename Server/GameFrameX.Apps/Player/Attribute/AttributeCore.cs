using System;
using System.Collections.Generic;

namespace GameFrameX.Apps.Player.Attribute;

/// <summary>
/// 玩家属性编号与定点数重算核心。
/// </summary>
public static class AttributeCore
{
    /// <summary>
    /// 百分比基数，10000 表示 100%。
    /// </summary>
    public const int PercentBase = 10000;

    /// <summary>
    /// 判断属性是否为第一批最终值槽。
    /// </summary>
    /// <param name="attributeType">属性编号。</param>
    /// <returns>如果是最终值槽则返回 true。</returns>
    public static bool IsFinalAttribute(AttributeType attributeType)
    {
        switch (attributeType)
        {
            case AttributeType.Life:
            case AttributeType.PhysicalAttack:
            case AttributeType.MagicAttack:
            case AttributeType.PhysicalDefense:
            case AttributeType.MagicDefense:
            case AttributeType.Critical:
            case AttributeType.CriticalDamage:
            case AttributeType.Precision:
            case AttributeType.Block:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 从派生槽反推所属最终属性。最终值槽本身不会触发反向重算。
    /// </summary>
    /// <param name="attributeType">发生变化的属性编号。</param>
    /// <param name="finalAttribute">派生槽所属最终属性。</param>
    /// <returns>如果属性编号是已知派生槽则返回 true。</returns>
    public static bool TryGetFinalAttribute(AttributeType attributeType, out AttributeType finalAttribute)
    {
        finalAttribute = AttributeType.None;

        var value = (int)attributeType;
        var finalValue = value % PercentBase;
        var offset = value - finalValue;
        if (finalValue <= 0 || !IsDerivedSlotOffset(offset))
        {
            return false;
        }

        var candidate = (AttributeType)finalValue;
        if (!IsFinalAttribute(candidate))
        {
            return false;
        }

        finalAttribute = candidate;
        return true;
    }

    /// <summary>
    /// 取得最终属性对应的指定数值槽编号。
    /// </summary>
    /// <param name="finalAttribute">最终属性编号。</param>
    /// <param name="slotKind">数值槽类型。</param>
    /// <returns>指定数值槽编号。</returns>
    /// <exception cref="ArgumentOutOfRangeException">最终属性或数值槽无效。</exception>
    public static AttributeType GetSlotAttribute(AttributeType finalAttribute, AttributeSlotKind slotKind)
    {
        if (!IsFinalAttribute(finalAttribute))
        {
            throw new ArgumentOutOfRangeException(nameof(finalAttribute), finalAttribute, "必须传入已知最终属性。");
        }

        if (!IsSlotOffset((int)slotKind))
        {
            throw new ArgumentOutOfRangeException(nameof(slotKind), slotKind, "必须传入已知属性数值槽。");
        }

        return (AttributeType)((int)finalAttribute + (int)slotKind);
    }

    /// <summary>
    /// 使用属性字典重算指定最终属性，缺失派生槽按 0 处理。
    /// </summary>
    /// <param name="attributes">属性值字典。</param>
    /// <param name="finalAttribute">要重算的最终属性。</param>
    /// <returns>重算后的最终值。</returns>
    public static long RecalculateFinal(IReadOnlyDictionary<AttributeType, long> attributes, AttributeType finalAttribute)
    {
        if (attributes == null)
        {
            throw new ArgumentNullException(nameof(attributes));
        }

        return CalculateFinal(
            GetValue(attributes, GetSlotAttribute(finalAttribute, AttributeSlotKind.Base)),
            GetValue(attributes, GetSlotAttribute(finalAttribute, AttributeSlotKind.Add)),
            GetValue(attributes, GetSlotAttribute(finalAttribute, AttributeSlotKind.Pct)),
            GetValue(attributes, GetSlotAttribute(finalAttribute, AttributeSlotKind.FinalAdd)),
            GetValue(attributes, GetSlotAttribute(finalAttribute, AttributeSlotKind.FinalPct)));
    }

    /// <summary>
    /// 按统一公式重算最终属性值。
    /// </summary>
    /// <param name="baseValue">基础值。</param>
    /// <param name="addValue">加法修正。</param>
    /// <param name="pctValue">第一层百分比修正，10000 表示 100%。</param>
    /// <param name="finalAddValue">最终加法修正。</param>
    /// <param name="finalPctValue">最终百分比修正，10000 表示 100%。</param>
    /// <returns>最终属性值。</returns>
    public static long CalculateFinal(long baseValue, long addValue, long pctValue, long finalAddValue, long finalPctValue)
    {
        checked
        {
            var firstLayer = (baseValue + addValue) * (PercentBase + pctValue) / PercentBase;
            return (firstLayer + finalAddValue) * (PercentBase + finalPctValue) / PercentBase;
        }
    }

    private static long GetValue(IReadOnlyDictionary<AttributeType, long> attributes, AttributeType attributeType)
    {
        long value;
        return attributes.TryGetValue(attributeType, out value) ? value : 0L;
    }

    private static bool IsSlotOffset(int offset)
    {
        return offset == (int)AttributeSlotKind.Final || IsDerivedSlotOffset(offset);
    }

    private static bool IsDerivedSlotOffset(int offset)
    {
        switch (offset)
        {
            case (int)AttributeSlotKind.Base:
            case (int)AttributeSlotKind.Add:
            case (int)AttributeSlotKind.Pct:
            case (int)AttributeSlotKind.FinalAdd:
            case (int)AttributeSlotKind.FinalPct:
                return true;
            default:
                return false;
        }
    }
}
