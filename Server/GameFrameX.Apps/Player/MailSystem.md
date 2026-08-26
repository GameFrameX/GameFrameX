---
type: change
module: apps
id: U1
slug: player-mail-system
title: 玩家邮件系统与运营发信
mode: strict
status: planned
created: 2026-07-19
updated: 2026-07-19
reviewed: 2026-07-19
depends_on: []
blocks: []
---

# change

> 本文件是 strict umbrella。本版（reviewed 2026-07-19，对应 Linear GFX-136）只做方案复审与边界定死，不做代码实现。复审把协议编号、状态机、懒创建算法、不可逆边界、HTTP 契约、跨仓库字段对齐、XML 注释硬性要求全部钉死，供后续 derived changes（C8 统一奖励发放、C9 邮件核心状态与懒创建、C10 Admin 发布 HTTP API 等）逐条对照实现。实现细则、字段类型、proto 文件、Handler 代码均留给 derived changes，本 umbrella 不写代码。

## 1. Proposal

### Problem

当前游戏服没有玩家游戏内邮件系统，只有后台直接给玩家发道具的 HTTP 入口和玩家背包状态。现有能力不能表达运营邮件、模板多语言、附件领取状态、条件群发、懒创建、撤回和过期作废等业务规则。一次性大改会让协议、玩家状态、HTTP API、Admin 对接和奖励发放边界互相阻塞。

### Goal

定义玩家游戏内邮件系统的 strict 总览：先把 MailCampaign、MailBox、MailState、附件状态、撤回、过期、删除和懒创建规则定死；不做代码实现。后续按 derived change 拆成「统一奖励发放」「邮件核心状态与懒创建」「Admin 发布 HTTP API」等可并行、有前置关系的实现链路。

### Background

- 项目 lessons 目录暂无可复用邮件经验。
- 现有背包发奖入口（`GameFrameX.Hotfix.Logic.Player.Bag.BagComponentAgent`）可作为普通道具发放的初始承接点，但邮件系统不能直接绑定背包实现；后续隐藏道具、月卡、终生卡、VIP 点和权益类资产需要统一发放接口承接。
- Admin 侧已有游戏业务邮件模板诉求，邮件模板、发布前确认、发布记录和渠道筛选由 Admin 负责；Server 负责接收发布 API、玩家命中判断、个人邮件实例和领取状态。
- 现有协议层已存在 `ServerInstanceId` 概念（`GameFrameX.Proto/BuiltIn/Player_-10.cs` 的 `ReqPlayerRegister` / `ReqPlayerUnRegister` / `NotifyPlayerOnLine` / `NotifyPlayerOffLine`，与 `ServerId` 区分）。业务意义上的 `ServerInstanceId` 在运行时取自当前服启动参数 `Setting.ServerId`（`GameFrameX.Launcher.StartUp.AppStartUpGame`，赋值 `GameServerConst.Game.Id`）。
- 本次复审产出（reviewed 2026-07-19，GFX-136）：协议编号、状态机、懒创建算法、不可逆边界集中表、HTTP 契约、跨仓库字段对齐、XML 注释硬性要求。原伞规划字段保留，未推翻。

## 2. Scope

### In Scope

- 只做 strict 方案复审与边界定死：协议编号、状态机、懒创建、撤回、过期、删除、不可逆边界、HTTP 契约、跨仓库字段契约、XML 注释硬性要求、derived change 规划。
- 定义 `MailCampaignState`（运营邮件发布记录）、`MailBoxState`（玩家邮件箱）、`MailState`（玩家个人邮件实例）、`MailAttachmentState`（附件）的字段语义与状态机。
- 定义 Server 玩家邮件协议集（列表/读信/领单/一键领/删除/变更通知）的编号与字段。
- 定义 Server HTTP API（Admin 发布/校验/撤回/查询）的路径、入参、出参、错误码。
- 定义统一附件发放接口边界与幂等键，引用 `follow-up-reward-grant-contract.md`。
- 定义跨仓库字段契约：Admin 前后端必须复用 Server 字段名，禁止平行字段。
- 定义 ServerInstanceId 绑定语义（命中判断取自 `Setting.ServerId`）。

### Out of Scope

- 不在本 umbrella 内实现任何运行时代码、proto 文件、Handler、Agent、HTTP Handler、状态实体。
- 不在本 umbrella 内实现隐藏道具、月卡、终生卡、VIP 点和权益类资产的具体状态模块。
- 不在本 umbrella 内实现 Admin 前端页面或 admin-api 后端的内部实现；只定义 Server 提供的契约。
- 不实现 SMTP / 外部真实邮箱能力。本系统仅指游戏内玩家邮件。
- 不允许发布后修改已发布邮件；如需修正文案或附件，必须创建新的发布版本。
- 不在本 umbrella 内修改 `apps/spec.md` 的业务段落；spec delta 在 derived change archive 时回写。

## 3. Spec Delta

### ADDED

#### 3.1 模块与协议编号（钉死）

- 邮件模块占位编号约定：玩家邮件 PlayerRpc 协议归入 `Player` 模块（`-10`，沿用 `Player_-10.cs`）；具体协议号在 derived change C9 中按现有 `Bag_100.cs` / `Player_-10.cs` 连续分配，并在本 umbrella 3.6 表登记，禁止与现有协议号冲突。
- Proto 消息一律 `[MessageTypeHandler((moduleId << 16) + opCode, (byte)MessageOperationType.Xxx)]` + `MessageObject, IRequestMessage/IResponseMessage` + `[ProtoMember(n)]`，沿用现有 `Player_-10.cs` 风格。
- 每个协议必须有 `MessageOperationType` 枚举名（如 `ReqMailList` / `RespMailList` / `NotifyMailChanged`），枚举名与类名一致，由 derived change 在 `GameFrameX.Proto` 补齐。

#### 3.2 运营邮件发布记录 `MailCampaignState`（不可变快照）

字段（语义定死，类型留给 derived change）：

- `CampaignId` — 运营邮件活动 ID，全局唯一。
- `CampaignVersion` — 发布版本号；同一 `CampaignId` 允许多版本，版本号单调递增，发布即冻结。
- `ServerInstanceIds` — 命中服务器实例集合；空集合 = 不限。命中判断：当前服 `Setting.ServerId ∈ ServerInstanceIds`。
- `ChannelIds` — 渠道集合；空集合 = 不限。游戏服不持有渠道运行参数，命中渠道来源由 Admin 发布前校验或后续玩家画像接口补齐（未补齐前，带渠道条件的 Campaign 默认不实例化，详见 4.5）。
- `MinLevel` / `MaxLevel` — 等级区间；空 = 不限。读取自玩家 `PlayerState.Level`。
- `PlayerCreatedAfter` / `PlayerCreatedBefore` — 玩家创建时间区间；空 = 不限。读取自玩家 `PlayerState` 创建时间。
- `MailType` — 邮件业务类型（系统/运营/补偿等）；仅作分类与展示，不影响状态机。
- `TemplateId` / `TemplateVersion` — 模板 ID 与版本；发布即冻结，模板后续编辑不影响已发布邮件。
- `TemplateArgs` — 模板参数；与模板版本一同冻结。
- `LocalizedContentSnapshot` — 多语言文案快照；发布即冻结，Admin 后续修改模板不回灌已发布邮件。
- `Attachments` — 附件定义列表（`MailAttachmentState` 附件元信息，不含领取状态）。
- `ExpireAt` — 绝对过期时间；空表示用 `ExpireDays` 相对计算。
- `ExpireDays` — 自玩家实例化起算的相对过期天数；与 `ExpireAt` 二选一，`ExpireAt` 优先。
- `ExpireAttachmentPolicy` — 过期附件策略枚举：`DiscardUnclaimed`（默认，未领取附件作废）/ `KeepUnclaimed`（保留待领取，需配置开启）/ `AutoClaim`（预留，默认不启用，需显式配置）。
- `Status` — Campaign 状态枚举（见 4.1）。
- `PublishedAt` — 发布时间；不可变。
- `RevokedAt` — 撤回时间；空表示未撤回，非空表示已撤回。

#### 3.3 玩家邮件箱 `MailBoxState`

- `List` — 玩家个人邮件实例集合（`MailState`）。
- `CreatedCampaignVersions` — 已实例化的 `CampaignId@Version` 集合；懒创建去重键，防止重复实例化（含已撤回 Campaign，防止撤回后重实例化）。
- `UnreadCount` — 未读计数；冗余字段，由邮件读信流程维护，仅作展示。
- `LastSyncCampaignTime` — 上次懒同步游标；用于增量扫描发布记录，避免每次全量。
- `LastCleanupTime` — 上次过期清理时间；用于定时清理调度。

#### 3.4 玩家个人邮件实例 `MailState`

- `MailId` — 玩家邮件实例 ID；玩家内唯一。
- `CampaignId` / `CampaignVersion` — 来源发布记录与版本；与 `CreatedCampaignVersions` 对应。
- `MailType` — 继承自 Campaign。
- `Title` / `Content` — 渲染后文案（来自 `LocalizedContentSnapshot` 按玩家语言选取）；实例化即冻结。
- `TemplateId` / `TemplateVersion` / `TemplateArgs` — 继承自 Campaign，便于审计与重渲染。
- `CreateTime` — 实例化时间（懒创建发生时刻）。
- `ExpireTime` — 实例过期时间（按 Campaign `ExpireAt` 或 `CreateTime + ExpireDays` 计算，实例化时一次性写定）。
- `ReadTime` — 首次读信时间；空表示未读。
- `DeleteTime` — 删除时间；空表示未删除。
- `ReadStatus` — 读状态维度（见 4.2 / 4.3）。
- `AttachmentStatus` — 附件整体状态维度（见 4.2 / 4.3）。
- `MailStatus` — 邮件生命周期状态维度（见 4.2）。
- `Attachments` — 附件实例列表（`MailAttachmentState`，含领取状态）。

#### 3.5 附件 `MailAttachmentState`

- `AttachmentId` — 附件 ID；邮件内唯一。
- `RewardType` — 奖励类型（普通道具/隐藏道具/月卡/终生卡/VIP 点/权益等）。
- `ItemId` — 物品 ID。
- `Count` — 数量。
- `ExtraData` — 扩展数据（预留）。
- `ClaimStatus` — 领取状态（见 4.3）。
- `ClaimTime` — 首次领取时间；空表示未领取。

#### 3.6 邮件协议集（编号与字段语义定死）

| 协议 | 方向 | 语义 | 关键字段 |
|---|---|---|---|
| `ReqMailList` / `RespMailList` | C→S / S→C | 拉取邮件列表（触发懒同步） | 请求：分页游标；响应：邮件摘要列表、`UnreadCount`、是否还有更多 |
| `ReqMailRead` / `RespMailRead` | C→S / S→C | 读信（标记已读） | 请求：`MailId`；响应：完整文案、附件列表与领取状态 |
| `ReqMailClaimAttachment` / `RespMailClaimAttachment` | C→S / S→C | 领取单封邮件的指定附件 | 请求：`MailId`、`AttachmentId`；响应：发放结果、失败原因 |
| `ReqMailClaimAllAttachment` / `RespMailClaimAllAttachment` | C→S / S→C | 一键领取当前可领附件 | 请求：`MailId`（或全部）；响应：逐项发放结果 |
| `ReqMailDelete` / `RespMailDelete` | C→S / S→C | 删除邮件 | 请求：`MailId`；响应：是否删除成功、拒绝原因 |
| `NotifyMailChanged` | S→C | 邮件变更通知 | 新邮件、状态变更增量；客户端据此刷新 |

- 协议号占位（moduleId=-10 下的 opCode）在 derived change C9 中连续分配并登记到本表，禁止与现有 `Player_-10.cs` 协议号冲突。
- 所有响应消息必须携带 `ErrorCode`（沿用 `OperationStatusCode`），拒绝原因走统一错误码。

#### 3.7 Server HTTP API（Admin 调用，钉死路径与字段语义）

沿用 `[HttpMessageMapping(typeof(XxxHttpHandler))]` + `BaseHttpHandler.Action(ip, url, paramMap, messageObject)` 模式（参考 `GameFrameX.Hotfix/Logic/Http/Player/ReqPlayerListHttpHandler.cs`）。

| API | 方法 | 路径（占位，最终由 HttpHandler 类名生成） | 入参 | 出参 | 行为 |
|---|---|---|---|---|---|
| 发布邮件 | POST | `/api/mailCampaign/publish` | `ReqPublishMailCampaign`（Campaign 全字段 + 附件 + 过期策略） | `RespPublishMailCampaign`（`CampaignId`、`CampaignVersion`、`PublishedAt`） | 写入不可变 `MailCampaignState` 快照；返回新版本号 |
| 预览/校验 | POST | `/api/mailCampaign/preview` | `ReqPreviewMailCampaign`（同发布入参） | `RespPreviewMailCampaign`（`HitCount` 预估、校验错误列表） | 不落库；仅校验字段与预估命中 |
| 撤回 | POST | `/api/mailCampaign/revoke` | `ReqRevokeMailCampaign`（`CampaignId`、`CampaignVersion`、撤回原因） | `RespRevokeMailCampaign`（受影响实例数） | 置 Campaign `RevokedAt`；按 4.6 处理已实例化邮件 |
| 查询发布状态 | GET | `/api/mailCampaign/query` | `ReqQueryMailCampaign`（`CampaignId` 或分页） | `RespQueryMailCampaign`（Campaign 列表、状态、实例化计数） | 只读 |

- 发布接口必须保证幂等：同一 `CampaignId + CampaignVersion` 重复发布返回已存在版本，不新建快照。
- 发布接口不存在修改入口；Admin 修改文案必须发新版本。
- HTTP 错误码与 `OperationStatusCode` 对齐；撤回/删除拒绝需返回明确原因（已领取/有未领附件等）。

#### 3.8 统一附件发放接口（边界定死，引用 follow-up）

- 邮件领取只提交领取上下文与附件列表，调用统一发放接口；不直接写 `BagState` 或隐藏资产状态。
- 幂等键：`RoleId + SourceType + SourceId + TraceId`，其中 `SourceId = mail:<MailId>:attachment:<AttachmentId>`，`TraceId` 由 Campaign + 玩家 + 附件维度生成。
- 普通道具一期复用背包写入路径；隐藏资产、月卡、终生卡、VIP 点、权益详见 `follow-up-reward-grant-contract.md`。
- 重复领取（同 `SourceId`）由发放接口短路返回上次结果，邮件侧不重复发奖、不重复扣减附件。

### MODIFIED

- 玩家登录或邮件列表拉取流程需触发邮件懒同步（`MailBoxState.LastSyncCampaignTime` 游标推进）。
- 普通背包发奖逻辑需被统一发放接口复用，避免邮件领取重复实现背包写入。
- 协议与 Handler 分层需新增邮件模块，并遵守现有 `[MessageMapping]` / `PlayerRpcComponentHandler<,>` / `StateComponentAgent<,>` / `[HttpMessageMapping]` / `BaseHttpHandler` 模式。

### REMOVED

- none

## 4. 状态机与不可逆边界（复审核心，钉死）

### 4.1 Campaign 状态机

`MailCampaignState.Status` 枚举与流转：

```
[Admin 侧 Draft，不属于 Server] ──发布──> Published ──撤回──> Revoked
                                            │
                                            └── ExpireAt 到期 ──> (Published + 时间维度过期，Status 不变；过期作用于实例)
```

- Server 端 Campaign 只有两态：`Published` / `Revoked`。Draft 属于 Admin 侧，Server 不接收。
- 过期不是 Campaign 状态：`ExpireAt` 到期后 Campaign 仍为 `Published`，过期作用在玩家实例（见 4.7）。
- `Published → Revoked` 单向不可逆；撤回后 Campaign 永不回到 `Published`。
- `Revoked` 是终态；新需求必须发新 `CampaignVersion` 或新 `CampaignId`。

### 4.2 MailState 状态机（玩家实例）

`MailState.MailStatus` 枚举与流转（单一生命周期状态机）：

```
Unread ──读信──> Read ──附件全领──> Claimed ──删除──> Deleted
  │                │
  │                ├──(有附件未领)──> PartialClaimed ──附件全领──> Claimed
  │                │
  └──撤回/过期──> Revoked / Expired（终态）
```

- 初态：`Unread`（实例化时）。
- `Read` / `PartialClaimed` / `Claimed` 为活跃态，可继续流转。
- `Deleted` / `Revoked` / `Expired` 为终态，不可回退。
- `MailStatus` 是生命周期主线，与 `ReadStatus` / `AttachmentStatus` 正交（见 4.3）。
- 删除只允许在 `AttachmentStatus == AllClaimed 或 NoAttachment` 时进入 `Deleted`（见 4.8）。

### 4.3 ReadStatus / AttachmentStatus / ClaimStatus（正交维度）

为避免与 `MailStatus` 字段语义重叠，定死如下：

- `ReadStatus`（邮件读信维度）：`Unread` / `Read`。读信幂等，重复读信不重复触发。
- `AttachmentStatus`（邮件附件整体维度，便于列表展示）：`NoAttachment`（无附件）/ `Unclaimable`（有附件但不可领，如撤回/过期）/ `PartialClaimed` / `AllClaimed`。
- `ClaimStatus`（单个附件维度，`MailAttachmentState.ClaimStatus`）：`Claimable`（可领）/ `Claimed`（已领）/ `Discarded`（过期/撤回作废）。
- 这三个维度是 `MailStatus` 的派生展示字段，单一事实来源是 `MailStatus` + 各 `MailAttachmentState.ClaimStatus` + `ReadTime`。derived change 实现时必须保证派生一致性，不允许独立写入造成漂移。

### 4.4 不可逆边界集中表（硬性约束，每条配 XML 注释要求）

| # | 边界 | 触发点 | 拒绝/行为 | 涉及 C# 类型/字段（须补中文 XML 注释） |
|---|---|---|---|---|
| B1 | 发布后不可修改 | 发布接口之后 | 不存在修改入口；改文案必须发新版本 | `MailCampaignState` 全字段、`MailCampaignState.Status` |
| B2 | 未领取附件禁止删除 | `ReqMailDelete` | 拒绝并返回 `UnclaimedAttachment` 原因 | `MailState.MailStatus`、`MailState.AttachmentStatus`、`ReqMailDelete` 拒绝路径 |
| B3 | 撤回不回滚已领取资产 | Campaign 撤回 | 已 `Claimed` 的附件状态与资产不变；仅未领附件置 `Discarded` | `MailCampaignState.RevokedAt`、`MailAttachmentState.ClaimStatus` |
| B4 | 过期未领取附件默认作废且可配置 | 过期清理 | 按 `ExpireAttachmentPolicy`：默认 `DiscardUnclaimed`，可配 `KeepUnclaimed`；`AutoClaim` 预留需显式开启 | `MailCampaignState.ExpireAttachmentPolicy`、`MailState.ExpireTime` |
| B5 | 懒创建幂等 | 登录/拉列表/同步 | 同一 `CampaignId@Version` 仅实例化一次；撤回 Campaign 记入 `CreatedCampaignVersions` 防重实例化 | `MailBoxState.CreatedCampaignVersions`、`MailBoxState.LastSyncCampaignTime` |
| B6 | 领取幂等 | `ReqMailClaimAttachment` 重复调用 | 同 `SourceId` 短路返回上次结果，不重复发奖、不重复扣附件 | `MailAttachmentState.ClaimStatus`、`MailAttachmentState.ClaimTime`、发放接口幂等键 |

每条边界的 C# 类型/属性/函数在 derived change 实现时必须补中文 XML 注释，注释需写明「不可逆」「单向」「幂等」等语义，呼应 issue 修改要求。

### 4.5 懒创建算法（钉死）

触发点（三处，任一触发即执行同步）：

1. 玩家登录完成后。
2. 客户端 `ReqMailList` 拉取。
3. 显式同步（Admin 触发或定时任务）。

算法（伪代码语义，实现留给 derived change）：

```
SyncPlayerMail(player, mailbox):
    campaigns = QueryPublishedCampaigns(since=mailbox.LastSyncCampaignTime)
    for c in campaigns (按 PublishedAt 升序):
        key = c.CampaignId + "@" + c.CampaignVersion
        if key in mailbox.CreatedCampaignVersions: continue      # 幂等（B5）
        if c.Status == Revoked:                                  # 撤回 Campaign 防重实例化
            mailbox.CreatedCampaignVersions.add(key); continue
        if c.ChannelIds 非空 and 玩家渠道未补齐: continue         # 渠道条件未补齐前跳过（不实例化，也不记 key，待补齐后再判断）
        if not MatchFilter(c, player): continue                  # ServerInstanceId/Level/创建时间/渠道 命中
        mail = Instantiate(c, player)                            # 渲染文案、计算 ExpireTime、复制附件元信息（不含领取状态）
        mailbox.List.add(mail)
        mailbox.CreatedCampaignVersions.add(key)
    mailbox.LastSyncCampaignTime = now
```

- `MatchFilter` 命中规则：`ServerInstanceIds` 空 or 含 `Setting.ServerId`；`ChannelIds` 空 or 含玩家渠道；`MinLevel/MaxLevel` 空 or 玩家 `PlayerState.Level` 落在区间；`PlayerCreatedAfter/Before` 空 or 玩家创建时间落在区间。
- 渠道来源未补齐时：带渠道条件的 Campaign **不实例化也不记 key**（与撤回 Campaign 区别：撤回是永久跳过，渠道未补齐是条件待定，后续补齐后可实例化）。此差异在 `CreatedCampaignVersions` 注释中写明。
- 实例化时一次性写定 `MailState.ExpireTime`，后续不重算。

### 4.6 撤回算法（钉死，呼应 B3）

```
RevokeCampaign(campaignId, version):
    campaign.Status = Revoked
    campaign.RevokedAt = now
    for 每个已实例化该 Campaign 的 MailBoxState:
        for mail in mailbox.List where mail.CampaignId+Version 命中:
            if mail.MailStatus in (Deleted, Expired): continue   # 终态不动
            for att in mail.Attachments:
                if att.ClaimStatus == Claimed: continue          # B3 不回滚已领
                att.ClaimStatus = Discarded
            if mail 所有附件均为 Claimed 或 NoAttachment:
                保持原 MailStatus（不强制作废，玩家可自行删除）
            else:
                mail.MailStatus = Revoked                         # 终态
    Admin 返回受影响实例数
```

- 已 `Claimed` 的资产与附件状态**不变**（B3 核心）。
- 未实例化的 Campaign：撤回仅置 Campaign 状态，懒创建算法的防重实例化分支兜底（B5）。

### 4.7 过期算法（钉死，呼应 B4）

```
ExpireSweep(mailbox, now):
    for mail in mailbox.List:
        if mail.MailStatus in (Deleted, Revoked): continue
        if mail.ExpireTime > now: continue
        switch campaign.ExpireAttachmentPolicy:
            case DiscardUnclaimed:                                # 默认
                for att in mail.Attachments where ClaimStatus == Claimable:
                    att.ClaimStatus = Discarded
                mail.MailStatus = Expired
            case KeepUnclaimed:
                # 保留待领取，仅标记邮件视图过期，附件仍可领（可配置开启）
                mail.MailStatus = Expired  # 或新增 ExpiredKeep 视图标记，由 derived change 决定
            case AutoClaim: # 预留，默认不启用
                # 调统一发放接口自动领取未领附件，需显式配置
    mailbox.LastCleanupTime = now
```

- 默认策略 `DiscardUnclaimed` 满足「过期未领取附件自动作废」。
- 策略可配置（Campaign 级 `ExpireAttachmentPolicy`），不全局写死。

### 4.8 删除算法（钉死，呼应 B2）

```
DeleteMail(mailbox, mailId):
    mail = mailbox.List[mailId]
    if mail.AttachmentStatus in (Unclaimable, AllClaimed, NoAttachment):
        mail.MailStatus = Deleted
        mail.DeleteTime = now
        return Ok
    return Reject(UnclaimedAttachment)                            # B2 拒绝
```

- 仅当无附件或附件全部已领/已作废时允许删除。
- `Deleted` 为终态，不可恢复。

## 5. 跨仓库字段契约（钉死，呼应 issue 修改要求）

- Server 是字段定义的唯一来源。Admin 前端、Admin 后端（admin-api）必须直接复用 Server Proto / HTTP API 的字段名、枚举名、语义，**禁止发明平行字段**。
- 字段命名一律沿用本 umbrella 3.2–3.7 的命名（`CampaignId`、`CampaignVersion`、`ServerInstanceIds`、`ChannelIds`、`MinLevel/MaxLevel`、`PlayerCreatedAfter/Before`、`MailType`、`TemplateId/TemplateVersion`、`TemplateArgs`、`LocalizedContentSnapshot`、`Attachments`、`ExpireAt`、`ExpireDays`、`ExpireAttachmentPolicy`、`Status`、`PublishedAt`、`RevokedAt`、`MailId`、`AttachmentId`、`RewardType`、`ClaimStatus`、`MailStatus`、`ReadStatus`、`AttachmentStatus`）。
- Admin 侧文案/筛选/发布前确认/撤回原因等 UI 表层字段可扩展，但落到 Server 接口的请求/响应字段必须与上表一致。
- 涉及用户可见文案的 Admin 前端改动必须走 i18n（issue 修改要求），与本契约正交，由 Admin 仓库负责。

## 6. XML 注释硬性要求（钉死，呼应 issue 修改要求）

derived change 实现时，涉及业务状态、筛选命中、领取、撤回、过期策略的 C# 类型/属性/函数必须补中文 XML 注释，注释需写明：

- 不可逆边界（B1–B6 中相关条目）。
- 单向状态流转（4.1 / 4.2）。
- 幂等语义（B5 / B6、发放接口幂等键）。
- `ServerInstanceId` 绑定 `Setting.ServerId` 的语义（避免与协议层 `ServerId` 混淆）。

`GameFrameX.Architecture.Analyzers` 现有 `CacheStateAssemblyAnalyzer` / `StateComponentAssemblyAnalyzer` / `MessageMappingAssemblyAnalyzer` / `HttpHandlerAssemblyAnalyzer` 已强制结构约束；XML 注释属人工规范，本 umbrella 不新增分析器规则，由 code review 把关。

## 7. Tasks

本 umbrella 自身任务（复审产出）：

- [x] 复审协议编号与字段语义（3.1 / 3.6 / 3.7）。
- [x] 钉死 Campaign / MailState 状态机与正交维度（4.1–4.3）。
- [x] 集中不可逆边界表 B1–B6（4.4）。
- [x] 形式化懒创建 / 撤回 / 过期 / 删除算法（4.5–4.8）。
- [x] 钉死跨仓库字段契约（5）与 XML 注释硬性要求（6）。
- [x] 规划 derived changes 与依赖（第 8 节）。
- [ ] 本 umbrella 进入 executing 后，按 derived change 逐条实现；archive 时把 spec delta 回写 `apps/spec.md`。

后续 derived changes 范围（不在本 umbrella 实现，仅登记依赖）：

- [ ] 设计并实现运营邮件发布记录状态或存储实体（Campaign 字段、不可变版本、过期策略）。
- [ ] 新增玩家邮件状态、邮件组件与组件代理（懒创建、列表、已读、领取、删除、过期清理、撤回作废）。
- [ ] 新增邮件协议与 Handler（列表、读信、领单、一键领、删除、变更通知）。
- [ ] 新增 Server HTTP API（Admin 发布、校验、撤回、查询）。
- [ ] 实现筛选规则（ServerInstanceId 命中 `Setting.ServerId`，渠道为 Admin 发布条件，等级与创建时间从玩家状态读取）。
- [ ] 实现发布不可变、删除拒绝、过期策略、撤回不回滚四类边界（B1–B4）。
- [ ] 定义并接入统一附件发放接口；第一版落地普通道具，隐藏资产详见 follow-up。
- [ ] 为状态、筛选命中、领取、撤回、过期策略补中文 XML 注释（第 6 节）。
- [ ] 更新 `apps/spec.md`，记录玩家邮件系统、运营发布记录与 Admin API 边界。

## 8. Derived Changes 规划（依赖编排）

| 占位 ID | Slug 建议 | Title | Depends On | 对应 issue 阻塞任务 |
|---|---|---|---|---|
| C8 | unified-reward-grant-v1 | 实现统一奖励发放接口一期 | U1 | P0 Server: 实现统一奖励发放接口一期 |
| C9 | player-mail-core-and-lazy-create | 实现玩家邮件核心状态与懒创建 | C8 | P1 Server: 实现玩家邮件核心状态与懒创建 |
| C10 | admin-mail-publish-http-api | 提供 Admin 邮件发布 HTTP API | U1 | P1 Server: 提供 Admin 邮件发布 HTTP API |
| —（非 Server） | — | admin-api 邮件模板/发布记录/Server 发布调用 | C10 | P1 后端：admin-api 实现邮件模板、发布记录和 Server 发布调用 |

- 编号为建议占位，实际认领时由各自 worker 在 `dev/index.md` 与 `apps/changes/index.md` 登记确认。
- C8 与 C10 可并行（均只依赖 U1）；C9 依赖 C8（邮件领取走统一发放接口）。
- admin-api（非 Server 仓库）不在本 umbrella 实现范围，但必须遵循第 5 节跨仓库字段契约。

## 9. Verification

### Tests

- 构建：`dotnet build Server.sln`
- 测试：`dotnet test Server.sln --no-restore`
- 本 umbrella 为方案复审，不直接产生运行时代码；上述命令用于确认复审改动未破坏仓库构建（仅文档变更，预期 PASS）。
- derived change 落地后须补充覆盖：
  - 玩家命中可选筛选条件后懒创建邮件。
  - 同一 `CampaignId@Version` 不重复创建个人邮件（B5）。
  - 未领取附件的邮件删除失败（B2）。
  - 已领取附件的邮件可删除。
  - 过期邮件未领取附件按 `ExpireAttachmentPolicy` 处理（默认作废，B4）。
  - 撤回后未实例化邮件不再生成（B5）。
  - 撤回后已实例化未领取邮件作废（B3，已领不动）。
  - 已领取邮件撤回不回滚奖励（B3）。
  - 领取接口重复调用不重复发奖（B6）。
- HTTP smoke（C10 落地后）：
  - Admin 发布 API 成功创建发布记录。
  - 发布后无修改入口；重复发布同版本返回已存在。
  - 撤回 API 对未实例化和已实例化邮件状态生效。

### Acceptance Criteria

- Admin 能通过 Server API 发布模板化、多语言、带附件的运营邮件。
- 已发布邮件不可修改，发布前确认由 Admin 负责，Server API 保证发布记录不可变（B1）。
- 玩家只在命中条件时懒创建邮件；服务器、渠道、等级和创建时间条件均可为空（4.5）。
- 业务 `ServerInstanceId` 与当前源码 `Setting.ServerId`、Admin 游戏服务器唯一 ID 的绑定语义在 API 文档或字段注释中明确（3.2 / 第 6 节）。
- 未领取或未领取完附件的邮件无法删除（B2）。
- 过期未领取附件按默认策略自动作废，且策略可配置（B4）。
- 撤回不会影响已领取资产，不会为未实例化玩家继续创建邮件（B3 / B5）。
- 邮件领取通过统一发放接口，邮件模块不直接写隐藏道具或权益资产状态（3.8）。
- 有新鲜构建和关键状态流转测试证据。
- strict 方案复审（本 umbrella）完成后才能进入 execute；derived changes 按第 8 节依赖编排。

### Review Notes

- ID: U1
- Title: 玩家邮件系统与运营发信
- Slug: player-mail-system
- Type: umbrella
- Status: planned
- Backlog: none
- Parent Change: none
- Derived Changes: [C8, C9, C10]（建议占位，见第 8 节）
- Depends On: []
- Blocks: []
- Mode: strict
- Risk: 跨协议、玩家状态、HTTP API、Admin 对接和奖励发放边界；重复领取、撤回、过期和模板版本不可变是高风险状态语义，已在第 4 节集中钉死。
- Dependencies: Admin 邮件模板与发布前确认；Server 启动参数 `ServerInstanceId`（取自 `Setting.ServerId`）；普通背包发奖路径；后续隐藏资产统一发放接口（C8）。
