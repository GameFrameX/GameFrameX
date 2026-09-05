<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX.Protobuf

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Protobuf?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Protobuf/releases)
[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)
[![CI](https://github.com/GameFrameX/GameFrameX.Protobuf/actions/workflows/proto-export.yml/badge.svg)](https://github.com/GameFrameX/GameFrameX.Protobuf/actions/workflows/proto-export.yml)

[![Discord](https://img.shields.io/badge/-5865F2?logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/-181717?logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Bilibili](https://img.shields.io/badge/-00A1D6?logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/-C71D23?logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)

**独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使**

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#快速开始) · [多语言 Release](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | **简体中文** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 项目简介

GameFrameX.Protobuf 是 GameFrameX 框架的统一网络协议定义仓库。采用 Protocol Buffers 3（`proto3`），按业务模块组织消息与错误码定义。每个 `.proto` 文件以数字模块 ID（文件名后缀）标识，用于客户端与服务端的消息路由和错误码生成。

代码生成由 [GameFrameX.Tools `ProtoExport`](https://github.com/GameFrameX/GameFrameX.Tools) 工具驱动。可任选一种工作流：

- **CI（零配置）** —— 每次 `push` 都会自动导出全部语言并发布到滚动更新的 [`latest` Release](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest)，直接下载即可。
- **Docker** —— `docker run gameframex/gameframex-tools:latest ...`，无需本地工具链。
- **本地脚本** —— `Tools/` 目录的 `ProtoExport` 产物由流水线每周自动同步，clone 后直接运行 `Proto2*Export.sh/.bat`。详见[导出工具](#导出工具)。

完整文档托管在 [GameFrameX 文档站](https://gameframex.doc.alianblank.com/protobuf/require)。

### 功能特性

- 统一的 `proto3` 协议定义，按数字模块 ID 组织
- 仓库自带脚本，一条命令导出 C#、C++、Go、Lua、TypeScript
- 每次 `push` 由 CI 自动发布全部语言产物到滚动更新的 `latest` Release
- Docker 镜像 + `Tools/` 产物每周自动同步，无需配置本地工具链

## 快速开始

### 安装

**方案 A —— 从 CI 下载（零配置）：** 从[最新 Release](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) 获取你所需语言的产物包。

**方案 B —— Docker：**

```bash
docker run --rm \
  -v "$PWD":/protos \
  -v "$PWD/output":/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

**方案 C —— 本地脚本：** `Tools/` 产物已自动同步就绪（需本地 .NET 10 SDK），在仓库根目录直接运行：

```bash
./Proto2CsExport_Server.sh   # C#（服务端）
./Proto2GoExport.sh          # Go
```

## 使用示例

在仓库根目录直接运行自带脚本本地导出：

```bash
./Proto2CsExport_Server.sh   # C#（服务端）
./Proto2GoExport.sh          # Go
```

所有脚本通过 `dotnet ./Tools/ProtoExport.dll` 启动 `Tools/` 下自动同步的生成器。完整参数列表见[导出参数](#导出参数)，细节见[导出文档](https://gameframex.doc.alianblank.com/protobuf/require)。

## 协议模块

| Proto 文件 | 模块 | 说明 |
|------------|------|------|
| `_0002_InnerBasic.proto` | 2 | 内部基础协议 |
| `_0010_Basic.proto` | 10 | 基础协议 |
| `_0020_Common.proto` | 20 | 通用协议（错误码、共享类型） |
| `_0100_Bag.proto` | 100 | 背包协议 |
| `_0120_Social.proto` | 120 | 社交协议 |
| `_-0120_Inner_Social.proto` | -120 | 内部社交协议（服务端） |
| `_0300_User.proto` | 300 | 用户 / 账号协议 |
| `_0310_Attribute.proto` | 310 | 玩家属性同步协议 |
| `_0400_Room.proto` | 400 | 房间协议 |
| `_0410_RockPaperScissors.proto` | 410 | 石头剪刀布小游戏协议 |
| `_0500_Mail.proto` | 500 | 邮件系统协议 |

## 协议规范

第一次接触 protobuf？本节是一个循序渐进的教程。从头读到尾，哪怕你从没写过 `.proto` 文件，也能学会新增一个协议模块。每一步都包含大白话说明、最小示例，以及背后的规则。严格的、由工具强制执行的规则列表见下方的[协议要求](#协议要求)。

### 动手之前 —— 三个大白话概念

- **Protobuf（`.proto`）** 是双方约定好的"填表模板"——就像一张印好的订单，每个格子都有固定的名字和位置，客户端和服务端照着填，绝不会互相误会。
- **模块 ID** 是一个"分拣号"。可以想象快递公司的区域编号：每类业务（背包、邮件、房间……）各分一个号，消息就按这个号被投递到对应的处理人。
- **对外协议 vs 内部协议** —— 对外协议是客户端能看见、能调用的"菜单"；内部协议是只在服务端之间传递的"后厨暗号"。两者绝不能混，否则客户端可能调到不该调的东西。

### 第 1 步 —— 创建文件

每个业务域放在自己的文件里，文件名叫 `_<ModuleID:0000>_<Domain>.proto`——**所有文件名都以 `_` 开头，接 4 位补零的模块 ID**，这样在任何文件管理器里都按模块号数值升序排列，且排序结果与环境无关。文件名一眼就能看出路由号和所属业务域。

```protobuf
// 文件名：_0100_Bag.proto
syntax = "proto3";      // 永远用 proto3 —— 当前的 protobuf 语法
package Bag;            // 业务域名（PascalCase）
option module = 100;    // 路由号；必须和文件名里的 0100 对上
```

逐行解读：

- `syntax = "proto3";` —— 声明使用当前的 protobuf 语法。每个文件都以此开头。
- `package Bag;` —— 这个文件的业务域是"Bag"。PascalCase 指首字母大写。
- `option module = 100;` —— 分配路由号 100。**它必须和文件名里的 `0100` 完全一致。**

规则：

- 文件名：`_<ModuleID:0000>_<Domain>.proto`，如 `_0500_Mail.proto`。
- 正数 = 对外协议（客户端 ↔ 服务端）；负数 = 内部协议（服务端 ↔ 服务端）。负数 ID 在文件名里保留负号（`_-0120_Inner_Social.proto` 表示 module = -120）；所有文件名都以 `_` 开头，既保证合法（不以 `-` 开头），又统一排序。
- 内部文件以 `Inner` 开头，如 `_0002_InnerBasic.proto`。

**为什么** —— 把模块 ID 写进文件名，文件名本身就是路由键：一眼能看出属于哪个业务域，两个文件也绝不可能悄悄占用同一个号。`Inner` 前缀给内部协议打了标记，方便导出时过滤掉，不会泄露给客户端。

### 第 2 步 —— 定义数据：消息与字段

**消息（message）** 是一张"表"——一组相关字段的集合。**字段（field）** 是表里的一个格子，有名字、有类型、有编号。

```protobuf
message BagItem {
  int32 ItemId = 1; // 道具 ID
  int64 Count = 2;  // 道具数量
}
```

逐行解读：

- `message BagItem { ... }` —— 定义了一张名叫 `BagItem` 的表。
- `int32 ItemId = 1;` —— 一个名叫 `ItemId` 的格子，类型 `int32`（小整数），编号 `1`。
- `int64 Count = 2;` —— 一个名叫 `Count` 的格子，类型 `int64`（大整数），编号 `2`。
- 行尾的 `// ...` 是注释，用来说明这个字段是什么意思。

规则：

- 字段名用 PascalCase；编号从 1 开始连续往上加，不要跳号。
- 如果删除了某个字段，要用 `reserved` 把它的编号占住——绝不能复用编号。
- 每个字段都要写行尾注释。

类型怎么选（大白话版）：

| 这个值是…… | 用 | 示例 |
|------------|-----|------|
| 玩家 / 实例 ID（可能很大） | `int64` | `PlayerId` |
| 配置 / 道具 ID（范围小） | `int32` | `ItemId` |
| 数量（可能堆很高） | `int64` | `Count` |
| 时间戳 | `int64` | `CreateTime` |
| 等级 / 头像（小、不会为负） | `uint32` | `Level` |
| 有固定几个选项的状态 | 枚举（见第 4 步） | `RoomStatus` |
| 列表 / 字典 | `repeated` / `map` | `repeated RoomPlayerInfo` |

**为什么** —— 编号必须连续，是因为字段编号就是它在传输时的身份标识：跳号会浪费空间，而复用已发布的编号，会让旧客户端的数据被塞进新字段，悄悄造成数据错乱。类型遵循"够用、不溢出"：大 ID 用 `int64`，小 ID 用 `int32` 省流量。

### 第 3 步 —— 让它们对话：请求 / 响应 / 通知

现在定义客户端和服务端怎么交互。一共有三种消息角色，靠名字前缀区分：

| 前缀 | 谁发起 | 大白话 |
|------|--------|--------|
| `Req<Name>` | 客户端 | "我问你个事" |
| `Resp<Name>` | 服务端回答 | "这是答案"（名字和请求一致） |
| `Notify<Name>` | 服务端推送 | "注意——有变化"（没有对应的请求） |

```protobuf
message ReqMailList { ... }        // 客户端要邮件列表
message RespMailList { ... }       // 服务端返回列表——注意名字是对上的
message NotifyMailChanged { ... }  // 服务端主动推送邮件变化
message MailInfo { ... }           // 一个可复用的数据块，上面几个都会用到
```

规则：

- 每个请求都要有一个同名的响应：`ReqMailList` ↔ `RespMailList`。
- `Notify` 只用于服务端主动推送。
- 把共用数据抽成 `<Name>Info`，定义一次、到处复用。

**为什么** —— 强制 Req/Resp 配对，保证每个问题都有答案；同名让人和代码生成器都能一眼看出谁和谁是一对。`<Name>Info` 避免在多个消息里重复定义同样的结构。

### 第 4 步 —— 用枚举表示状态

**枚举（enum）** 是一道多选题——比如订单状态只能是"待付款 / 已付款 / 已发货"，不能是别的。

```protobuf
enum RoomStatus {
  None = 0;     // 无状态 / 无效
  Waiting = 1;  // 等待开始
  Playing = 3;  // 游戏进行中
}
```

规则：

- 枚举名和枚举值都用 PascalCase。
- 第一个值永远是 `0`，留给默认 / 无状态（`None`、`Unknown`）。

**为什么** —— proto3 强制第一个值必须是 `0`。把它定为 `None` / `Unknown` 作为安全默认值：没赋值的字段读出来是"无状态"，而不是误命中某个真实状态——这样能避免一整类 bug。

### 第 5 步 —— 定义错误码

出错时给它一个编号，双方就能准确知道到底哪里错了。错误码分两层：

**通用错误码** —— 各模块都会遇到的常见失败（参数错误、消耗不足、不存在）。它们放在 `_0020_Common.proto` 的 `OperationStatusCode` 里，从 `0` 往上编号。

**业务错误码** —— 你这个模块特有的失败。编号按公式算：**`模块 ID × 1000 + 三位序号`**。

```protobuf
// 邮件是模块 500，所以它的错误码从 500001 开始
// 500001 = 500 × 1000 + 1
enum MailErrorCode {
  MailNotFound = 500001;        // 邮件不存在
  MailAlreadyDeleted = 500002;  // 邮件已被删除
}
```

规则：客户端把错误码当作普通 `int` 接收。成功时不赋值——proto3 的默认 `0` 就代表"成功"，所以大多数情况什么都不用传。

**为什么** —— 这个公式让编号自带身份：`500001` 一看就是邮件模块的，全局唯一不用协调，每个模块还预留了 1000 个号位可以扩展。成功当"什么都不传"，是因为成功占大多数，省下的流量很可观。

### 第 6 步 —— 写注释

注释是双方共用的唯一文档——`.proto` 文件没有上下文，不写注释，另一端只能靠猜。

- 消息前面：写它的用途。
- 字段或枚举值后面：写它代表什么。
- 如果一个 `int` 字段实际装的是枚举值，用括号标出枚举名，比如 `// 状态（RoomStatus）`，让读者知道合法值去哪查。

**为什么** —— 光一个 `int` 看不出它有哪些合法取值；标出枚举名，读者就能直接找到答案。

### 完整示例

以虚构的 `_0600_Quest`（任务系统）模块为例，覆盖上述所有规则：

```protobuf
syntax = "proto3";
package Quest;
option module = 600;

// Quest business error codes (6 digits = module 600 + 3-digit ordinal)
enum QuestErrorCode {
  QuestNotFound = 600001;             // quest not found
  QuestNotCompleted = 600002;         // quest not completed
  QuestRewardAlreadyClaimed = 600003; // reward already claimed
}

// Quest status
enum QuestStatus {
  None = 0;        // no state
  Accepted = 1;    // accepted
  Completable = 2; // ready to complete
  Completed = 3;   // completed
  Claimed = 4;     // reward claimed
}

// Quest data view
message QuestInfo {
  int64 QuestId = 1;            // quest config ID
  QuestStatus Status = 2;       // quest status (QuestStatus)
  int64 Progress = 3;           // current progress
  int64 TargetProgress = 4;     // target progress
}

// Request quest list
message ReqQuestList {
}

// Response quest list
message RespQuestList {
  repeated QuestInfo Quests = 1; // quest list
}

// Request claim quest reward
message ReqClaimQuestReward {
  int64 QuestId = 1; // quest config ID
}

// Response claim quest reward
message RespClaimQuestReward {
  int64 QuestId = 1;       // quest config ID
  QuestStatus Status = 2;  // status after claim (QuestStatus)
}

// Quest change notification (server push)
message NotifyQuestChanged {
  repeated QuestInfo Quests = 1; // changed quests
}
```

## 协议要求

以下是 `ProtoExport` 工具强制执行的硬性规则。权威来源：[GameFrameX.Tools README](https://github.com/GameFrameX/GameFrameX.Tools#readme)。

### 文件格式

```protobuf
syntax = "proto3";     // Required: only proto3 is supported
package Basic;
option module = 10;    // Required: module ID must be defined
```

### 消息命名

- **请求**：`Req<Name>`（如 `ReqLogin`、`ReqHeartBeat`）
- **响应**：`Resp<Name>`（如 `RespLogin`）
- **通知**：`Notify<Name>`（如 `NotifyBagInfoChanged`）
- 所有 message、field、enum 名与枚举值必须使用 **UpperCamelCase**。

### 模块 ID

| ID 范围 | 用途 |
|---------|------|
| `0` ~ `32767` | 客户端 ↔ 服务端 |
| `-32768` ~ `-1` | 服务端 ↔ 服务端（内部） |

### 字段编号

- message 字段编号必须**小于 800**（`>= 800` 的值由系统保留，会导致解析错误）。
- `ErrorCode` 是 `Resp` 消息中的**保留字段名**——不要手动定义。工具会在每个 `Resp` 上自动生成 `ErrorCode` 字段。

### 限制

- **禁止嵌套类型** —— 不能在另一个 message 中声明 `message` / `enum`。
- **禁止 RPC 定义** —— 不支持 `service` 块。
- **仅支持 proto3** —— 必须使用 `syntax = "proto3";`，不支持 proto2。

### 注释规范

- 每个 `message` / `enum` 上方需有一行注释，描述其用途。
- 每行 field / 枚举值末尾需有**行内**注释。

### 仅服务端文件

导出工具通过**文件名后缀** `-s` 或 `_s`（如 `player-s.proto`、`economy_s.proto`）识别仅服务端的 proto 文件。传入 `--isServer true` 可将其纳入；默认 `--isServer false` 时它们会被跳过，因此仅服务端的消息永远不会泄露给客户端。

内部协议另外使用**负的模块 ID** 实现路由隔离（参见上方模块 ID 表）。

> **关于当前仓库的说明：** 这里的内部文件采用 `Inner_` 前缀加负模块 ID 的写法（如 `_-0120_Inner_Social.proto`）。`-s`/`_s` 后缀和负 ID 约定都能实现仅服务端路由——选择其中一种，并在一个模块内保持一致。

## 支持的导出语言

| 语言 | Mode 与参数 | 本地脚本 | Docker |
|------|-------------|----------|--------|
| C#（服务端） | `csharp --isServer true` | `Proto2CsExport_Server.sh` / `.bat` | 是 |
| C#（客户端 / Unity / Godot） | `csharp` | `Proto2CsExport_Client.sh` / `.bat` | 是 |
| C++ | `cpp` | `Proto2CppExport.sh` / `.bat` | 是 |
| Go | `go` | `Proto2GoExport.sh` / `.bat` | 是 |
| Lua | `lua` | `Proto2LuaExport.sh` / `.bat` | 是 |
| TypeScript | `typescript` | `Proto2TsExport.sh` / `.bat` | 是 |
| TypeScript (LayaBox) | `typescript` | `Proto2TsExport_LayaBox.sh` | 是 |

### Docker 示例

**C#（服务端）：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Server/GameFrameX.Proto/Proto:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" \
  --isGenerateDescription true \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

**Go：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./GoServer/proto:/output \
  gameframex/gameframex-tools:latest \
  --mode go --inputPath /protos --outputPath /output --namespaceName proto
```

**TypeScript：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Laya/src/gameframex/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode typescript --inputPath /protos --outputPath /output
```

**Lua：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Defold/scripts/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode lua --importPath "./network/" --inputPath /protos --outputPath /output
```

**C++：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Unreal/Source/Proto:/output \
  gameframex/gameframex-tools:latest \
  --mode cpp \
  --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto
```

路径映射：`-v <host>:<container>` 挂载宿主机目录；`--inputPath` / `--outputPath` 必须引用**容器内**路径（`/protos`、`/output`），而非宿主机路径。

## 导出参数

### 核心参数

| 参数 | 必填 | 默认值 | 说明 |
|------|------|--------|------|
| `--mode` | 是 | - | `csharp` / `typescript` / `cpp` / `lua` / `go` |
| `--inputPath` | 是 | - | 存放 `.proto` 文件的目录 |
| `--outputPath` | 是 | - | 生成文件的输出目录 |
| `--namespaceName` | 否 | `""` | C# namespace（对于 Go，若以点分隔则取最后一段作为 package 名） |
| `--isGenerateErrorCode` | 否 | `true` | 是否在 `Resp` 消息上自动生成 `ErrorCode` 字段 |
| `--requireComments` | 否 | `none` | 注释校验级别：`none` / `container` / `member` / `all` |

### C#

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `--usingStatements` | `""` | using 语句，以 `\|` 分隔（如 `"using System\|using ProtoBuf"`） |
| `--isGenerateDescription` | `false` | 是否生成 `[System.ComponentModel.Description]` 特性 |
| `--isServer` | `false` | 是否纳入仅服务端的 proto 文件（文件名以 `-s` 或 `_s` 结尾） |

### TypeScript

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `--importPath` | `"../network/"` | 生成的 import 语句的路径前缀 |
| `--isGenerateDescription` | `false` | 是否生成 JSDoc 风格注释 |

### 旧版参数（Legacy）

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `--isGenerateErrorCodeExcelFile` | `true` | 是否生成错误码 Excel 文件 |
| `--errorCodeExcelFilePath` | `""` | 错误码 Excel 文件的自定义路径 |

## Docker

预构建镜像支持 `linux/amd64` 和 `linux/arm64`：

```bash
# Docker Hub
docker pull gameframex/gameframex-tools:latest

# GitHub Container Registry (GHCR)
docker pull ghcr.io/gameframex/gameframex.tools:latest
```

镜像入口点即 `ProtoExport` 工具——直接在镜像名后追加参数：

```bash
docker run --rm \
  -v /path/to/protos:/protos \
  -v /path/to/output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --inputPath /protos --outputPath /output
```

## CI 流水线

本仓库自带 [`.github/workflows/proto-export.yml`](.github/workflows/proto-export.yml)。它在**每次 `push`** 时自动运行，也支持手动触发。

| 步骤 | 发生什么 |
|------|----------|
| 1 | 拉取 `gameframex/gameframex-tools:latest` |
| 2 | 将 `.proto` 源码挂载到容器的 `/protos` |
| 3 | 并行导出全部六种目标语言（构建 matrix） |
| 4 | 将每种语言的输出收集为 workflow artifact |
| 5 | 当 `push` 到 `main` 时，（重新）发布滚动更新的 **`latest` Release**，附带全部 artifact |

在 [Releases 页面](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) 下载最新生成的代码——无需任何工具链。

## 导出工具

本仓库的代码生成由独立的 [GameFrameX.Tools](https://github.com/GameFrameX/GameFrameX.Tools) 仓库中的 `ProtoExport` 工具驱动（一个 .NET 10 控制台程序）。**`Tools/` 目录内置该工具的二进制产物，由流水线每周自动同步**——clone 后即可直接运行本地脚本，无需自行构建（见[快速开始](#快速开始)）：

- **CI** —— 零配置，直接从最新 Release 下载生成代码。
- **Docker** —— 运行预构建镜像，无需本地工具链。
- **本地脚本** —— 直接使用 `Tools/` 下每周自动同步的产物；需要立即更新时，手动触发同步流水线或自行构建覆盖（见下文）。

### 工具仓库

| 项目 | 仓库地址 | 说明 |
|------|----------|------|
| GameFrameX.Tools | https://github.com/GameFrameX/GameFrameX.Tools | `ProtoExport` 生成器源码、完整参数文档、Docker 镜像 |

`ProtoExport` 是一个 .NET 10 控制台项目（`ProtoExport.csproj`，`OutputType=Exe`），依赖 NuGet 包 `GameFrameX.Foundation.Options` 做命令行参数解析。

### 环境要求

- **.NET 10 SDK** —— 运行导出脚本需要它（脚本通过 `dotnet` 启动工具）；自行构建工具时同样需要。
- 验证：`dotnet --version` 应输出 `10.x.x`。

### 自动同步（默认）

`Tools/` 产物由 **Tools Sync** 流水线（`.github/workflows/tools-sync.yml`）维护：每周一 09:00（北京时间）自动从上游 `main` 分支构建 Release 产物，有变化才提交。需要立即同步时，在仓库 **Actions → Tools Sync → Run workflow** 手动触发。

### 自行构建（可选覆盖）

上游约定 `GameFrameX.Tools` 与本仓库克隆到同级目录，构建产物直接输出到本仓库的 `Tools/`：

```bash
# 1. 与本仓库同级克隆工具仓库
git clone https://github.com/GameFrameX/GameFrameX.Tools.git
cd GameFrameX.Tools/ProtoExport

# 2. 构建（Release）—— csproj 的 OutputPath 固定输出到同级 Protobuf/Tools/
dotnet build -c Release
```

### 产物清单

`Tools/` 目录只包含以下 4 个必需文件（自动同步与手动构建均只需这些）：

| 文件 | 必需 | 作用 |
|------|:----:|------|
| `ProtoExport.dll` | 是 | 主程序集 |
| `ProtoExport.deps.json` | 是 | 依赖描述（运行时必需） |
| `ProtoExport.runtimeconfig.json` | 是 | 运行时配置（指定 .NET 10） |
| `GameFrameX.Foundation.Options.dll` | 是 | 命令行参数解析依赖 |

构建输出中的 `ProtoExport.pdb`（调试符号）与原生启动器（macOS/Linux 的 `ProtoExport`、Windows 的 `ProtoExport.exe`）不会被同步——所有 `Proto2*` 脚本统一通过 `dotnet ./Tools/ProtoExport.dll` 启动工具，跨平台一致。

### 验证

```bash
cd /path/to/GameFrameX.Protobuf
./Proto2CsExport_Client.sh    # macOS / Linux
Proto2CsExport_Client.bat     # Windows
```

看到 `协议扫描完成: ... 导出 N 个，跳过 M 个` 即表示工具就绪。

### 与导出脚本的关系

仓库根目录的每个 `Proto2*.sh` / `.bat` 脚本都会：

1. 从仓库根目录运行；
2. 通过 `dotnet ./Tools/ProtoExport.dll` 启动 `Tools/` 下自动同步的生成器；
3. 传入对应语言的参数（`--mode`、`--isServer` 等）。

因此**只要 `Tools/` 下有正确的产物，所有脚本即可直接运行**——无需关心各脚本的参数细节。

### 更新工具

`ProtoExport` 上游迭代后，**Tools Sync** 流水线会在每周同步时自动覆盖 `Tools/` 下的旧文件（也可手动触发立即同步）。拉取本仓库最新变更即可获得最新的工具版本。

## 依赖

| 依赖 | 用途 |
|------|------|
| [GameFrameX.Tools `ProtoExport`](https://github.com/GameFrameX/GameFrameX.Tools) | 驱动全部导出的代码生成器（.NET 10 控制台程序） |
| [`gameframex/gameframex-tools`](https://hub.docker.com/r/gameframex/gameframex-tools) Docker 镜像 | 容器化导出，无需本地工具链 |
| .NET 10 SDK | 仅运行本地导出脚本时需要 |

## 文档与资源

- [协议文档](https://gameframex.doc.alianblank.com/protobuf/require) —— 协议规范与导出指南
- [GameFrameX.Tools](https://github.com/GameFrameX/GameFrameX.Tools) —— `ProtoExport` 源码、完整参数文档、Docker 镜像
- [Releases](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) —— 滚动发布的全语言生成代码包
- [导出流水线](.github/workflows/proto-export.yml) 与 [Tools Sync 流水线](.github/workflows/tools-sync.yml)

## 社区与支持

![QQ](https://img.shields.io/badge/QQ-467608841%2F233840761-EB1923?style=for-the-badge&logo=qq&logoColor=white)
[![Bilibili](https://img.shields.io/badge/Bilibili-00A1D6?style=for-the-badge&logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?style=for-the-badge&logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[<img src="https://cdn.jsdelivr.net/npm/devicon@2/icons/linkedin/linkedin-original.svg" height="28" alt="LinkedIn" />](https://www.linkedin.com/in/alianblank)
[![Reddit](https://img.shields.io/badge/Reddit-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/GameFrameX/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/alian_blank)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
[![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?style=for-the-badge&logo=bluesky&logoColor=white)](https://bsky.app/profile/alianblank.bsky.social)

## 更新日志

见 [Releases 页面](https://github.com/GameFrameX/GameFrameX.Protobuf/releases)——每次 `push` 到 `main` 都会重新发布滚动更新的 `latest` Release，附带最新生成的代码。

## 开源协议

详见 [LICENSE.md](LICENSE.md) 文件。

<!--
EN: See [LICENSE.md](LICENSE.md) for license information.
zh-CN: 详见 [LICENSE.md](LICENSE.md) 文件。
zh-TW: 詳見 [LICENSE.md](LICENSE.md) 檔案。
ja: 詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
ko: 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
-->
