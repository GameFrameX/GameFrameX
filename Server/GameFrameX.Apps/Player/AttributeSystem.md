# 玩家属性系统最小规格

## 目的

第一版属性系统只覆盖玩家属性，不覆盖完整战斗、Buff、装备、技能或协议同步。目标是先固定状态结构、属性编号、计算规则和分层边界，让后续实现按小任务推进。

现有 `PlayerState` 只保存账号、名字、等级、头像、状态、登录时间和离线时间等基础资料。战斗属性会持续扩展，且会被配置、成长、装备、Buff、技能和同步协议共同影响。如果继续在 `PlayerState` 上追加固定属性字段，玩家基础状态会变成宽表，后续每新增一个属性都要修改状态结构、持久化结构和同步契约。因此玩家属性必须使用独立组件承载。

## 分层边界

### GameFrameX.Apps

- 新增 `AttributeState`，保存玩家属性 Key/Value 数值槽。
- 新增 `AttributeComponent : StateComponent<AttributeState>`，作为非热更状态组件。
- `AttributeState` 只保存可持久化的属性数据，不承载业务规则、配置读取或通知发送。
- `PlayerState` 不追加攻击、防御、生命、速度等固定属性字段，只保留玩家基础资料。

### GameFrameX.Hotfix

- 新增 `AttributeComponentAgent`，作为热更逻辑入口。
- 负责初始化玩家属性、应用属性变化、统一重算最终值、写回状态、发送属性变化通知。
- 后续登录流程只调用 `AttributeComponentAgent` 的初始化或读取入口，不直接操作属性字典。

### GameFrameX.Config

- 后续只提供属性初始值、成长或模板配置读取入口。
- 本次不修改 Luban 表结构，不生成新配置类，不变更现有 JSON 配置数据。

### GameFrameX.Proto

- 后续单独定义属性快照和增量同步协议。
- 本次不修改协议文件，不改变登录响应结构。

## AttributeType 编号规则

每个基础属性使用一个稳定主编号 `N`，`Final` 值直接使用 `N`。派生槽通过固定偏移表达来源：

| 槽位 | 编号 | 说明 |
|---|---:|---|
| `Final` | `N` | 最终值，只读结果槽 |
| `Base` | `N + 10000` | 基础值，来自配置、等级或初始化 |
| `Add` | `N + 20000` | 加法修正 |
| `Pct` | `N + 30000` | 第一层百分比修正 |
| `FinalAdd` | `N + 40000` | 最终加法修正 |
| `FinalPct` | `N + 50000` | 最终百分比修正 |

示例：最大生命主编号为 `1001` 时，`MaxHp` 的最终值槽为 `1001`，基础值槽为 `11001`，加法槽为 `21001`，百分比槽为 `31001`，最终加法槽为 `41001`，最终百分比槽为 `51001`。

## 定点数规则

- 所有核心属性值使用 `long`。
- 百分比基数使用 `10000`，即 `10000 = 100%`，`1500 = 15%`。
- 重算过程使用整数运算，暂不引入浮点数。
- 除法按 C# 整数除法截断处理；需要四舍五入时由后续任务显式定义。

统一公式：

```text
Final = (((Base + Add) * (10000 + Pct) / 10000) + FinalAdd) * (10000 + FinalPct) / 10000
```

## 最小状态模型

第一版 `AttributeState` 只需要一个属性字典：

```csharp
public sealed class AttributeState : CacheState
{
    public Dictionary<int, long> Values { get; set; } = new();
}
```

其中 key 为 `AttributeType` 编号，value 为定点整数属性值。`Final` 槽由 `AttributeComponentAgent` 统一重算写入，调用方不直接手写最终值。

## 后续实现任务

1. GFX-124：实现 `AttributeType` 编号与属性重算核心，只包含编号常量、派生槽计算和公式自检。
2. GFX-125：增加玩家属性状态组件，在 `GameFrameX.Apps` 中加入 `AttributeState` 和 `AttributeComponent`。
3. GFX-126：增加 `AttributeComponentAgent` 与变化事件，在 `GameFrameX.Hotfix` 中封装初始化、修改、重算和通知。
4. GFX-127：登录流程初始化玩家基础属性，从配置或默认模板创建首版属性值。
5. GFX-128：增加玩家属性同步协议，单独定义快照和增量消息。

后续任务必须按顺序推进。任何 Buff、装备、技能或战斗公式扩展都在这些基础任务验收后另开任务。
