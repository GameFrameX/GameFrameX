// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System.Collections.Generic;
using GameFrameX.Apps.Player.Attribute;
using GameFrameX.Apps.Player.Attribute.Entity;
using GameFrameX.Hotfix.Logic.Player.Attribute;
using GameFrameX.Proto.Proto;
using Xunit;

namespace GameFrameX.Hotfix.Tests;

/// <summary>
/// 玩家属性系统端到端闭环验收（GFX-145 / umbrella U2）单元测试。覆盖 acceptance criteria 列出的六个验收点：
/// 公式重算 / 状态读写 / Agent 读写 / 变化事件 / 登录初始化幂等 / 快照增量同步构建，外加 AttributeType 编号规则前置。
/// 通过对纯函数 / 纯逻辑入口（<see cref="AttributeCore" /> / <see cref="PlayerAttributeMutation" /> /
/// <see cref="PlayerInitialAttributeDefaults" /> / <see cref="PlayerAttributeSyncBuilder" />）直接断言，避免引入 Actor 运行时。
/// </summary>
public class AttributeSystemTests
{
    // ================================================================================
    // 公式重算（AttributeCore.CalculateFinal / RecalculateFinal）
    // 统一公式：Final = (((Base + Add) * (10000 + Pct) / 10000) + FinalAdd) * (10000 + FinalPct) / 10000
    // ================================================================================

    /// <summary>纯基础值（无任何修正）→ 最终值等于基础值。</summary>
    [Fact]
    public void CalculateFinal_OnlyBase_ReturnsBase()
    {
        Assert.Equal(1000L, AttributeCore.CalculateFinal(1000, 0, 0, 0, 0));
    }

    /// <summary>Base + Add 线性叠加。</summary>
    [Fact]
    public void CalculateFinal_BasePlusAdd()
    {
        Assert.Equal(150L, AttributeCore.CalculateFinal(100, 50, 0, 0, 0));
    }

    /// <summary>第一层百分比 Pct=5000（50%）放大数据。</summary>
    [Fact]
    public void CalculateFinal_FirstLayerPct()
    {
        // ((1000 + 0) * 15000 / 10000) * 10000 / 10000 = 1500
        Assert.Equal(1500L, AttributeCore.CalculateFinal(1000, 0, 5000, 0, 0));
    }

    /// <summary>最终加法 FinalAdd 在第一层之后叠加。</summary>
    [Fact]
    public void CalculateFinal_FinalAdd()
    {
        // ((1000) + 200) * 10000 / 10000 = 1200
        Assert.Equal(1200L, AttributeCore.CalculateFinal(1000, 0, 0, 200, 0));
    }

    /// <summary>最终百分比 FinalPct=2000（20%）放大数据。</summary>
    [Fact]
    public void CalculateFinal_FinalPct()
    {
        // (1000) * 12000 / 10000 = 1200
        Assert.Equal(1200L, AttributeCore.CalculateFinal(1000, 0, 0, 0, 2000));
    }

    /// <summary>五个槽位同时生效，按统一公式逐层计算（含整数除法截断）。</summary>
    [Fact]
    public void CalculateFullFormula_AllSlots()
    {
        // firstLayer = (100 + 20) * 11000 / 10000 = 132000 / 10000 = 132
        // result     = (132 + 50) * 10500 / 10000 = 182 * 10500 / 10000 = 1911000 / 10000 = 191
        Assert.Equal(191L, AttributeCore.CalculateFinal(100, 20, 1000, 50, 500));
    }

    /// <summary>整数除法截断：Base=1、Pct=3333（33.33%）应截断回 1，不进位。</summary>
    [Fact]
    public void CalculateFinal_IntegerTruncation()
    {
        // 1 * 13333 / 10000 = 1（截断，非四舍五入）
        Assert.Equal(1L, AttributeCore.CalculateFinal(1, 0, 3333, 0, 0));
    }

    /// <summary>全部槽位缺失（默认 0）→ 最终值为 0。</summary>
    [Fact]
    public void RecalculateFinal_EmptyDictionary_DefaultsZero()
    {
        var values = new Dictionary<AttributeType, long>();

        Assert.Equal(0L, AttributeCore.RecalculateFinal(values, AttributeType.Life));
    }

    /// <summary>只有 Base 槽位时，最终值等于 Base。</summary>
    [Fact]
    public void RecalculateFinal_OnlyBaseSlot()
    {
        var values = new Dictionary<AttributeType, long>
        {
            [AttributeType.LifeBase] = 1000,
        };

        Assert.Equal(1000L, AttributeCore.RecalculateFinal(values, AttributeType.Life));
    }

    /// <summary>多个派生槽位组合，重算结果符合统一公式。</summary>
    [Fact]
    public void RecalculateFinal_MultipleSlots()
    {
        var values = new Dictionary<AttributeType, long>
        {
            [AttributeType.LifeBase] = 1000,
            [AttributeType.LifeAdd] = 500,
            [AttributeType.LifePct] = 5000, // +50%
        };

        // ((1000 + 500) * 15000 / 10000) = 2250
        Assert.Equal(2250L, AttributeCore.RecalculateFinal(values, AttributeType.Life));
    }

    /// <summary>不同最终属性互不串扰：物理攻击的重算只读物理攻击派生槽。</summary>
    [Fact]
    public void RecalculateFinal_PerAttributeIsolation()
    {
        var values = new Dictionary<AttributeType, long>
        {
            [AttributeType.LifeBase] = 1000,
            [AttributeType.PhysicalAttackBase] = 200,
        };

        Assert.Equal(1000L, AttributeCore.RecalculateFinal(values, AttributeType.Life));
        Assert.Equal(200L, AttributeCore.RecalculateFinal(values, AttributeType.PhysicalAttack));
    }

    // ================================================================================
    // 状态读写（PlayerAttributeState.GetValue）
    // ================================================================================

    /// <summary>字典缺失属性默认读取为 0。</summary>
    [Fact]
    public void State_GetValue_MissingReturnsZero()
    {
        var state = new PlayerAttributeState();

        Assert.Equal(0L, state.GetValue(AttributeType.Life));
        Assert.Equal(0L, state.GetValue(AttributeType.LifeBase));
    }

    /// <summary>已写入的属性命中返回实际值。</summary>
    [Fact]
    public void State_GetValue_PresentReturnsValue()
    {
        var state = new PlayerAttributeState();
        state.Values[(int)AttributeType.LifeBase] = 1234L;

        Assert.Equal(1234L, state.GetValue(AttributeType.LifeBase));
    }

    // ================================================================================
    // AttributeType / AttributeSlotKind 编号规则前置
    // ================================================================================

    /// <summary>第一批九项基础最终属性编号 1~9 连续，且均为最终值槽。</summary>
    [Fact]
    public void AttributeType_FirstBatchFinalsAreContiguous()
    {
        Assert.Equal(1, (int)AttributeType.Life);
        Assert.Equal(2, (int)AttributeType.PhysicalAttack);
        Assert.Equal(3, (int)AttributeType.MagicAttack);
        Assert.Equal(4, (int)AttributeType.PhysicalDefense);
        Assert.Equal(5, (int)AttributeType.MagicDefense);
        Assert.Equal(6, (int)AttributeType.Critical);
        Assert.Equal(7, (int)AttributeType.CriticalDamage);
        Assert.Equal(8, (int)AttributeType.Precision);
        Assert.Equal(9, (int)AttributeType.Block);

        Assert.True(AttributeCore.IsFinalAttribute(AttributeType.Life));
        Assert.True(AttributeCore.IsFinalAttribute(AttributeType.Block));
    }

    /// <summary>派生槽（Base/Add/Pct/...）不是最终值槽。</summary>
    [Fact]
    public void AttributeType_DerivedSlotsAreNotFinal()
    {
        Assert.False(AttributeCore.IsFinalAttribute(AttributeType.LifeBase));
        Assert.False(AttributeCore.IsFinalAttribute(AttributeType.LifeAdd));
        Assert.False(AttributeCore.IsFinalAttribute(AttributeType.LifePct));
        Assert.False(AttributeCore.IsFinalAttribute(AttributeType.LifeFinalAdd));
        Assert.False(AttributeCore.IsFinalAttribute(AttributeType.LifeFinalPct));
        Assert.False(AttributeCore.IsFinalAttribute(AttributeType.None));
    }

    /// <summary>数值槽偏移：Base=10000、Add=20000、Pct=30000、FinalAdd=40000、FinalPct=50000。</summary>
    [Fact]
    public void SlotKind_OffsetsAreStable()
    {
        Assert.Equal(10000, (int)AttributeSlotKind.Base);
        Assert.Equal(20000, (int)AttributeSlotKind.Add);
        Assert.Equal(30000, (int)AttributeSlotKind.Pct);
        Assert.Equal(40000, (int)AttributeSlotKind.FinalAdd);
        Assert.Equal(50000, (int)AttributeSlotKind.FinalPct);
    }

    /// <summary>GetSlotAttribute：最终属性 + 槽偏移 = 派生槽编号。</summary>
    [Fact]
    public void GetSlotAttribute_CombinesFinalAndOffset()
    {
        Assert.Equal(AttributeType.LifeBase, AttributeCore.GetSlotAttribute(AttributeType.Life, AttributeSlotKind.Base));
        Assert.Equal(AttributeType.LifeAdd, AttributeCore.GetSlotAttribute(AttributeType.Life, AttributeSlotKind.Add));
        Assert.Equal(AttributeType.LifePct, AttributeCore.GetSlotAttribute(AttributeType.Life, AttributeSlotKind.Pct));
        Assert.Equal(AttributeType.LifeFinalAdd, AttributeCore.GetSlotAttribute(AttributeType.Life, AttributeSlotKind.FinalAdd));
        Assert.Equal(AttributeType.LifeFinalPct, AttributeCore.GetSlotAttribute(AttributeType.Life, AttributeSlotKind.FinalPct));
    }

    /// <summary>TryGetFinalAttribute：派生槽反推最终属性成功。</summary>
    [Fact]
    public void TryGetFinalAttribute_DerivedSlotResolves()
    {
        Assert.True(AttributeCore.TryGetFinalAttribute(AttributeType.LifeBase, out var life));
        Assert.Equal(AttributeType.Life, life);

        Assert.True(AttributeCore.TryGetFinalAttribute(AttributeType.PhysicalAttackPct, out var pa));
        Assert.Equal(AttributeType.PhysicalAttack, pa);

        Assert.True(AttributeCore.TryGetFinalAttribute(AttributeType.BlockFinalPct, out var blk));
        Assert.Equal(AttributeType.Block, blk);
    }

    /// <summary>TryGetFinalAttribute：最终值槽本身和 None 不触发反向重算。</summary>
    [Fact]
    public void TryGetFinalAttribute_FinalAndNone_ReturnsFalse()
    {
        Assert.False(AttributeCore.TryGetFinalAttribute(AttributeType.Life, out var final));
        Assert.Equal(AttributeType.None, final);

        Assert.False(AttributeCore.TryGetFinalAttribute(AttributeType.None, out var none));
        Assert.Equal(AttributeType.None, none);
    }

    // ================================================================================
    // Agent 读写 + 变化事件（PlayerAttributeMutation.ApplyValue）
    // Agent.Set/Add 的纯逻辑内核：写派生槽 → 重算最终值 → 判定 StateChanged/ShouldDispatch
    // ================================================================================

    /// <summary>写 Base 槽：触发最终值重算，StateChanged=true、ShouldDispatch=true（最终值变化）。</summary>
    [Fact]
    public void ApplyValue_BaseSlot_RecalculatesAndDispatches()
    {
        var values = new Dictionary<int, long>();

        var result = PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeBase, 1000, false);

        Assert.True(result.StateChanged);
        Assert.True(result.ShouldDispatch);
        Assert.Equal(AttributeType.Life, result.FinalAttributeType);
        Assert.Equal(0L, result.OldFinalValue);
        Assert.Equal(1000L, result.NewFinalValue);
        Assert.Equal(1000L, values[(int)AttributeType.LifeBase]);
        Assert.Equal(1000L, values[(int)AttributeType.Life]);
    }

    /// <summary>写 Add 槽：最终值线性叠加并重算。</summary>
    [Fact]
    public void ApplyValue_AddSlot_RecalculatesFinal()
    {
        var values = new Dictionary<int, long>();
        PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeBase, 1000, true);

        var result = PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeAdd, 500, false);

        Assert.True(result.StateChanged);
        Assert.Equal(AttributeType.Life, result.FinalAttributeType);
        Assert.Equal(1500L, result.NewFinalValue);
        Assert.Equal(1500L, values[(int)AttributeType.Life]);
    }

    /// <summary>写 Pct 槽：第一层百分比生效。</summary>
    [Fact]
    public void ApplyValue_PctSlot_RecalculatesFinal()
    {
        var values = new Dictionary<int, long>();
        PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeBase, 1000, true);

        var result = PlayerAttributeMutation.ApplyValue(values, AttributeType.LifePct, 5000, false); // +50%

        Assert.Equal(1500L, result.NewFinalValue);
        Assert.Equal(1500L, values[(int)AttributeType.Life]);
    }

    /// <summary>写 FinalAdd / FinalPct 槽：最终层修正生效。</summary>
    [Fact]
    public void ApplyValue_FinalSlots_RecalculatesFinal()
    {
        var values = new Dictionary<int, long>();
        PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeBase, 1000, true);

        var addResult = PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeFinalAdd, 200, false);
        Assert.Equal(1200L, addResult.NewFinalValue);

        var pctResult = PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeFinalPct, 2000, false); // +20%
        // (1200) * 12000 / 10000 = 1440
        Assert.Equal(1440L, pctResult.NewFinalValue);
    }

    /// <summary>同值写入：不产生变化（StateChanged=false），不派发。</summary>
    [Fact]
    public void ApplyValue_SameValue_NotChanged()
    {
        var values = new Dictionary<int, long>();
        PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeBase, 1000, true);

        var result = PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeBase, 1000, false);

        Assert.False(result.StateChanged);
        Assert.False(result.ShouldDispatch);
    }

    /// <summary>静默写入：最终值即使变化也不派发事件（ShouldDispatch=false）。</summary>
    [Fact]
    public void ApplyValue_Silent_DoesNotDispatch()
    {
        var values = new Dictionary<int, long>();

        var result = PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeBase, 1000, silent: true);

        Assert.True(result.StateChanged);
        Assert.False(result.ShouldDispatch);
        Assert.Equal(1000L, values[(int)AttributeType.Life]);
    }

    /// <summary>派生槽变化但最终值未变（整数截断）：StateChanged=true、ShouldDispatch=false，不发送无意义变化事件。</summary>
    [Fact]
    public void ApplyValue_DerivedChanged_FinalUnchanged_NoDispatch()
    {
        var values = new Dictionary<int, long>();
        // Base=2 → Life=2
        PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeBase, 2, true);

        // Pct=1（0.01%）：firstLayer = 2 * 10001 / 10000 = 2（截断），Life 仍为 2
        var result = PlayerAttributeMutation.ApplyValue(values, AttributeType.LifePct, 1, false);

        Assert.True(result.StateChanged);              // Pct 槽确实 0→1
        Assert.False(result.ShouldDispatch);           // 最终值未变，不派发
        Assert.Equal(2L, result.OldFinalValue);
        Assert.Equal(2L, result.NewFinalValue);
    }

    // ================================================================================
    // 登录初始化幂等（PlayerInitialAttributeDefaults.ApplyMissing）
    // ================================================================================

    /// <summary>空状态首次初始化：补齐全部九项基础槽并重算对应最终值，返回 changed=true。
    /// 注意默认值为 0 的基础槽（Critical/Precision/Block）不落盘——oldValue(0)==value(0) 视为 NotChanged，
    /// 通过 GetValue 读取仍得到 0，符合"字典缺失默认 0"语义。</summary>
    [Fact]
    public void InitializeDefaults_FirstCall_FillsAllBaseSlots()
    {
        var values = new Dictionary<int, long>();

        var changed = PlayerInitialAttributeDefaults.ApplyMissing(values);

        Assert.True(changed);
        // 非零默认值的基础槽被落盘
        Assert.Equal(1000L, Get(values, AttributeType.LifeBase));
        Assert.Equal(100L, Get(values, AttributeType.PhysicalAttackBase));
        Assert.Equal(100L, Get(values, AttributeType.MagicAttackBase));
        Assert.Equal(50L, Get(values, AttributeType.PhysicalDefenseBase));
        Assert.Equal(50L, Get(values, AttributeType.MagicDefenseBase));
        Assert.Equal(15000L, Get(values, AttributeType.CriticalDamageBase));
        // 默认值为 0 的基础槽不落盘，但 GetValue 读取仍为 0（可读取的基础属性最终值）
        Assert.Equal(0L, Get(values, AttributeType.CriticalBase));
        Assert.Equal(0L, Get(values, AttributeType.PrecisionBase));
        Assert.Equal(0L, Get(values, AttributeType.BlockBase));
        // 非零基础槽对应的最终值被重算并落盘（玩家登录后拥有可读取的基础属性最终值）
        Assert.Equal(1000L, Get(values, AttributeType.Life));
        Assert.Equal(100L, Get(values, AttributeType.PhysicalAttack));
        Assert.Equal(15000L, Get(values, AttributeType.CriticalDamage));
        // 默认 0 的最终值不落盘，读取为 0
        Assert.Equal(0L, Get(values, AttributeType.Critical));
    }

    /// <summary>幂等：二次调用不产生任何变化（changed=false），不重复叠加。</summary>
    [Fact]
    public void InitializeDefaults_SecondCall_NoChange()
    {
        var values = new Dictionary<int, long>();
        PlayerInitialAttributeDefaults.ApplyMissing(values);
        var snapshotLife = Get(values, AttributeType.Life);
        var snapshotPa = Get(values, AttributeType.PhysicalAttack);

        var changed = PlayerInitialAttributeDefaults.ApplyMissing(values);

        Assert.False(changed);
        Assert.Equal(snapshotLife, Get(values, AttributeType.Life));
        Assert.Equal(snapshotPa, Get(values, AttributeType.PhysicalAttack));
    }

    /// <summary>已有基础槽不覆盖：模拟玩家成长（LifeBase 被装备/升级提升），初始化只补缺失槽，不重置成长。</summary>
    [Fact]
    public void InitializeDefaults_PreservesExistingGrowth()
    {
        var values = new Dictionary<int, long>();
        // 玩家已通过成长把 LifeBase 提升到 9999，初始化不得覆盖
        PlayerAttributeMutation.ApplyValue(values, AttributeType.LifeBase, 9999, true);

        PlayerInitialAttributeDefaults.ApplyMissing(values);

        Assert.Equal(9999L, Get(values, AttributeType.LifeBase));            // 成长保留，未被重置为默认 1000
        Assert.Equal(100L, Get(values, AttributeType.PhysicalAttackBase));   // 其它缺失槽补默认
        Assert.Equal(9999L, Get(values, AttributeType.Life));                // 成长后的 Life 最终值保持
    }

    // ================================================================================
    // 快照 / 增量同步构建（PlayerAttributeSyncBuilder）
    // ================================================================================

    /// <summary>快照包含全部九项最终属性条目。</summary>
    [Fact]
    public void BuildSnapshot_ContainsAllFinalAttributes()
    {
        var state = new PlayerAttributeState();
        state.Values[(int)AttributeType.LifeBase] = 1000;

        var snapshot = PlayerAttributeSyncBuilder.BuildSnapshot(state);

        Assert.Equal(9, snapshot.Attributes.Count);
        Assert.Contains(snapshot.Attributes, e => e.Type == (int)AttributeType.Life);
        Assert.Contains(snapshot.Attributes, e => e.Type == (int)AttributeType.Block);
    }

    /// <summary>快照条目字段映射：Type/Value/Base/Add/Pct 正确，且派生槽缺失时调试字段默认 0。
    /// 通过 mutation 写入派生槽以保证最终值槽被重算落盘（直接写派生槽不会触发重算）。</summary>
    [Fact]
    public void BuildSnapshot_EntryMapsFields()
    {
        var state = new PlayerAttributeState();
        PlayerAttributeMutation.ApplyValue(state.Values, AttributeType.LifeBase, 1000, true);
        PlayerAttributeMutation.ApplyValue(state.Values, AttributeType.LifeAdd, 200, true);
        PlayerAttributeMutation.ApplyValue(state.Values, AttributeType.LifePct, 5000, true);
        // 此时 Life 最终值 = ((1000+200)*15000/10000) = 1800

        var snapshot = PlayerAttributeSyncBuilder.BuildSnapshot(state);
        var life = snapshot.Attributes.Find(e => e.Type == (int)AttributeType.Life);

        Assert.NotNull(life);
        Assert.Equal((int)AttributeType.Life, life.Type);
        Assert.Equal(1800L, life.Value);
        Assert.Equal(1000L, life.Base);
        Assert.Equal(200L, life.Add);
        Assert.Equal(5000L, life.Pct);
    }

    /// <summary>增量同步消息字段映射：Type/Value。</summary>
    [Fact]
    public void BuildChanged_MapsTypeAndValue()
    {
        var changed = PlayerAttributeSyncBuilder.BuildChanged(AttributeType.Life, 1800L);

        Assert.Equal((int)AttributeType.Life, changed.Type);
        Assert.Equal(1800L, changed.Value);
    }

    /// <summary>快照只输出最终值槽：派生槽不作为独立条目出现（服务端权威，客户端只消费最终值 + 调试字段）。</summary>
    [Fact]
    public void BuildSnapshot_ExcludesDerivedSlots()
    {
        var state = new PlayerAttributeState();

        var snapshot = PlayerAttributeSyncBuilder.BuildSnapshot(state);

        Assert.DoesNotContain(snapshot.Attributes, e => e.Type == (int)AttributeType.LifeBase);
        Assert.DoesNotContain(snapshot.Attributes, e => e.Type == (int)AttributeType.LifePct);
    }

    // ---------- fixtures ----------

    /// <summary>读取属性值，缺失键返回 0，对齐 PlayerAttributeState.GetValue 的"字典缺失默认 0"语义。</summary>
    private static long Get(Dictionary<int, long> values, AttributeType attributeType)
    {
        return values.TryGetValue((int)attributeType, out var value) ? value : 0L;
    }
}
