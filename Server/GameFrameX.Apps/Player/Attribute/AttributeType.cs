namespace GameFrameX.Apps.Player.Attribute;

/// <summary>
/// 玩家属性编号。最终值使用稳定主编号，派生槽使用固定偏移。
/// </summary>
public enum AttributeType
{
    /// <summary>
    /// 未知属性。
    /// </summary>
    None = 0,

    /// <summary>
    /// 生命最终值。
    /// </summary>
    Life = 1,

    /// <summary>
    /// 物理攻击最终值。
    /// </summary>
    PhysicalAttack = 2,

    /// <summary>
    /// 魔法攻击最终值。
    /// </summary>
    MagicAttack = 3,

    /// <summary>
    /// 物理防御最终值。
    /// </summary>
    PhysicalDefense = 4,

    /// <summary>
    /// 魔法防御最终值。
    /// </summary>
    MagicDefense = 5,

    /// <summary>
    /// 暴击最终值。
    /// </summary>
    Critical = 6,

    /// <summary>
    /// 爆伤最终值。
    /// </summary>
    CriticalDamage = 7,

    /// <summary>
    /// 精准最终值。
    /// </summary>
    Precision = 8,

    /// <summary>
    /// 格挡最终值。
    /// </summary>
    Block = 9,

    LifeBase = 10001,
    PhysicalAttackBase = 10002,
    MagicAttackBase = 10003,
    PhysicalDefenseBase = 10004,
    MagicDefenseBase = 10005,
    CriticalBase = 10006,
    CriticalDamageBase = 10007,
    PrecisionBase = 10008,
    BlockBase = 10009,

    LifeAdd = 20001,
    PhysicalAttackAdd = 20002,
    MagicAttackAdd = 20003,
    PhysicalDefenseAdd = 20004,
    MagicDefenseAdd = 20005,
    CriticalAdd = 20006,
    CriticalDamageAdd = 20007,
    PrecisionAdd = 20008,
    BlockAdd = 20009,

    LifePct = 30001,
    PhysicalAttackPct = 30002,
    MagicAttackPct = 30003,
    PhysicalDefensePct = 30004,
    MagicDefensePct = 30005,
    CriticalPct = 30006,
    CriticalDamagePct = 30007,
    PrecisionPct = 30008,
    BlockPct = 30009,

    LifeFinalAdd = 40001,
    PhysicalAttackFinalAdd = 40002,
    MagicAttackFinalAdd = 40003,
    PhysicalDefenseFinalAdd = 40004,
    MagicDefenseFinalAdd = 40005,
    CriticalFinalAdd = 40006,
    CriticalDamageFinalAdd = 40007,
    PrecisionFinalAdd = 40008,
    BlockFinalAdd = 40009,

    LifeFinalPct = 50001,
    PhysicalAttackFinalPct = 50002,
    MagicAttackFinalPct = 50003,
    PhysicalDefenseFinalPct = 50004,
    MagicDefenseFinalPct = 50005,
    CriticalFinalPct = 50006,
    CriticalDamageFinalPct = 50007,
    PrecisionFinalPct = 50008,
    BlockFinalPct = 50009
}
