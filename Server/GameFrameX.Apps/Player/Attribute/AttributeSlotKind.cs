namespace GameFrameX.Apps.Player.Attribute;

/// <summary>
/// 玩家属性数值槽偏移。
/// </summary>
public enum AttributeSlotKind
{
    /// <summary>
    /// 最终值槽，编号等于属性主编号。
    /// </summary>
    Final = 0,

    /// <summary>
    /// 基础值槽，通常来自配置、等级或初始化。
    /// </summary>
    Base = 10000,

    /// <summary>
    /// 加法修正槽。
    /// </summary>
    Add = 20000,

    /// <summary>
    /// 第一层百分比修正槽，百分比基数为 10000。
    /// </summary>
    Pct = 30000,

    /// <summary>
    /// 最终加法修正槽。
    /// </summary>
    FinalAdd = 40000,

    /// <summary>
    /// 最终百分比修正槽，百分比基数为 10000。
    /// </summary>
    FinalPct = 50000
}
