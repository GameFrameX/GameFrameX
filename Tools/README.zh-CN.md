<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX.Tools

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Tools?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Tools/releases)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)

[![Discord](https://img.shields.io/badge/-5865F2?logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/-181717?logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Bilibili](https://img.shields.io/badge/-00A1D6?logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/-C71D23?logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)

**独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使**

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#快速开始) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | **简体中文** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 项目简介

ProtoExport 是面向多语言的 Proto 协议代码生成工具，支持导出 C#、TypeScript、C++、Lua、Go 五种目标语言，提供 CLI、跨平台 GUI 与 Docker 镜像三种使用方式。

### 功能特性

- 多语言导出：C#（`.cs`）、TypeScript（`.ts`）、C++（`.h`）、Lua（`.lua`）、Go（`.go`）
- 三种使用方式：CLI（`dotnet ProtoExport.dll`）、Avalonia GUI（ProtoExporterGUI）、Docker 镜像（`linux/amd64`、`linux/arm64`）
- 消息 ID 稳定性：lock 文件持久化 `(Module, 消息名) → SubId` 映射，协议号不随 proto 文本编辑漂移
- 服务端专属协议：以 `-s` / `_s` 结尾的 proto 文件仅在服务端导出时包含
- 注释校验：`none` / `container` / `member` / `all` 四级注释完整性检查
- GUI 配置持久化：按模式保存到 `Setting.json`，深度合并默认值，无损升级

## 快速开始

### 安装

`ProtoExport` 项目配置为将构建产物输出到**固定的同级路径**：`../Protobuf/Tools/`（相对于仓库根，即 `<工作区>/Protobuf/Tools/`）。路径以 `.csproj` 所在位置为锚点通过 `$(MSBuildThisFileDirectory)` 定位，任何将 `Tools` 与 `Protobuf` 克隆为同级目录的机器都能正确解析。

该固定路径供 `Protobuf` 仓库中的 `Proto2*Export.{sh,bat}` 脚本消费（如 `dotnet ./Tools/ProtoExport.dll ...`），脚本期望 DLL 位于 `Protobuf/Tools/ProtoExport.dll`。

```bash
# 在 Tools/ProtoExport 目录（或仓库根）执行
dotnet build ProtoExport/ProtoExport.csproj -c Release
# 产物落到：../Protobuf/Tools/ProtoExport.dll（不含 TFM/RID 子目录）
```

使固定路径生效的构建属性（定义于 `ProtoExport/ProtoExport.csproj`）：

- `OutputPath` = `$(MSBuildThisFileDirectory)../../Protobuf/Tools/` — Debug/Release 共用同一输出目录。
- `AppendTargetFrameworkToOutputPath` = `false` — 不生成 `net10.0/` 子目录。
- `AppendRuntimeIdentifierToOutputPath` = `false` — 不生成 `win-x64/`、`linux-arm64/` 子目录。

> 使用 `OutputPath` 而非 `OutDir`，以避免被 SDK 默认的 `output-paths` target 覆盖。

## 使用示例

| 模式 | 输出语言 | 文件扩展名 | 说明 |
|------|---------|-----------|------|
| `csharp` | C# | `.cs` | 适用于 Server、Unity、Godot、Stride、Flax 等 |
| `typescript` | TypeScript | `.ts` | 适用于 LayaAir、Cocos Creator、Phaser 等 |
| `cpp` | C++ | `.h` | 适用于 Unreal Engine 等 |
| `lua` | Lua | `.lua` | 适用于 Defold、Solar2D、Dora SSR 等 |
| `go` | Go | `.go` | 适用于 Go 游戏服务器等 |

### Docker 镜像

预构建的 Docker 镜像支持 `linux/amd64` 和 `linux/arm64` 架构。

**Docker Hub**

```bash
docker pull gameframex/gameframex-tools:latest
```

**GitHub Container Registry (GHCR)**

```bash
docker pull ghcr.io/gameframex/gameframex.tools:latest
```

**使用示例**

```bash
docker run --rm \
  -v /path/to/protos:/protos \
  -v /path/to/output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" --isGenerateDescription true --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

### C# 模式

生成带有 `[ProtoContract]` / `[ProtoMember]` 特性的 C# 代码。所有行为通过 CLI 参数控制，无硬编码的引擎特定逻辑。

#### 服务端导出

生成带有服务端 using 语句、`[Description]` 特性的代码，包含服务端专属 proto 文件。

**本地运行：**

```bash
dotnet ProtoExport.dll \
  --mode csharp \
  --isServer true \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" \
  --isGenerateDescription true \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Server/GameFrameX.Proto/Proto \
  --namespaceName GameFrameX.Proto.Proto \
  --isGenerateErrorCode true
```

**Docker 运行：**

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

#### Unity 导出

生成带有 Unity using 语句的代码，自动跳过服务端专属 proto 文件。

```bash
dotnet ProtoExport.dll \
  --mode csharp \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Unity/Assets/Hotfix/Proto \
  --namespaceName Hotfix.Proto \
  --isGenerateErrorCode true
```

#### Godot 导出

与 Unity 类似，使用 Godot 特定的命名空间。

```bash
dotnet ProtoExport.dll \
  --mode csharp \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Godot/Proto \
  --namespaceName Proto \
  --isGenerateErrorCode true
```

### TypeScript 模式

生成包含 `export namespace`、`export class` 和 `export enum` 的 `.ts` 文件，并生成聚合文件 `ProtoMessageRegister.ts`。自动跳过服务端专属 proto 文件。

#### 默认 import 路径

```bash
dotnet ProtoExport.dll \
  --mode typescript \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Laya/src/gameframex/protobuf \
  --isGenerateErrorCode true
```

#### 自定义 import 路径

```bash
dotnet ProtoExport.dll \
  --mode typescript \
  --importPath "./lib/network/" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../CocosCreator/assets/scripts/protobuf \
  --isGenerateErrorCode true
```

**Docker 运行：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Laya/src/gameframex/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode typescript --inputPath /protos --outputPath /output
```

### C++ 模式

生成带有 `#pragma once`、命名空间、`enum class` 和类定义的 C++ 头文件。继承 `MessageObject` 的类包含 `MESSAGE_ID` 和 `Clear()` 方法。

```bash
dotnet ProtoExport.dll \
  --mode cpp \
  --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Unreal/Source/Proto \
  --namespaceName GameFrameX.Proto
```

### Lua 模式

生成带有 LuaDoc（EmmyLua）类型注解和模块化消息定义的 `.lua` 文件，同时生成聚合文件 `ProtoMessageRegister.lua`。

```bash
dotnet ProtoExport.dll \
  --mode lua \
  --importPath "./network/" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Defold/scripts/protobuf
```

**Docker 运行：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Defold/scripts/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode lua --importPath "./network/" --inputPath /protos --outputPath /output
```

### Go 模式

生成带有 protobuf 标签的 Go struct 定义、枚举类型定义，以及聚合文件 `message_register.go`。使用 `--namespaceName` 作为 Go 包名（点分隔时取最后一段）。

```bash
dotnet ProtoExport.dll \
  --mode go \
  --usingStatements "google.golang.org/protobuf/runtime/protoimpl" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../GoServer/proto \
  --namespaceName proto
```

**Docker 运行：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./GoServer/proto:/output \
  gameframex/gameframex-tools:latest \
  --mode go --inputPath /protos --outputPath /output --namespaceName proto
```

### GUI 工具（ProtoExporterGUI）

跨平台 Avalonia GUI，封装 ProtoExport CLI，免去手敲命令行参数。

#### 功能特性

- 7 种导出模式（Server / Unity / Godot / TypeScript / C++ / Lua / Go）下拉切换
- 全部 CLI 参数可视化编辑（命名空间、using/import、注释校验级别、错误码、Description、服务器模式）
- 路径 FolderPicker 浏览选择（不再手敲）
- 中/英文界面实时切换
- 配置持久化（按模式分别保存到程序目录的 `Setting.json`，深度合并默认值，无损升级）
- 实时日志输出区

#### 构建发布

```bash
dotnet publish ProtoExporterGUI/ProtoExporterGUI.csproj -c Release -r win-x64 --no-self-contained
# 其他 RID：osx-arm64 / osx-x64 / linux-x64 / linux-arm64
```

产物为单文件可执行（不含运行时，需目标机预装 .NET 10 运行时）。

#### 使用步骤

1. 启动程序
2. 选择导出模式
3. 填写或浏览选择输入/输出路径
4. 点击导出
5. 查看日志区

GUI 与 CLI 功能等价；CI/自动化场景仍建议使用 CLI 或 Docker。

### 快捷导出脚本

`Protobuf/` 目录下提供了预置的导出脚本：

| 脚本 | 说明 |
|------|------|
| `Proto2CsExport_Server.sh/.bat` | 导出 C# 服务端代码 |
| `Proto2CsExport_Client.sh/.bat` | 导出 C# Unity 客户端代码 |
| `Proto2TsExport.sh/.bat` | 导出 TypeScript 代码 |
| `Proto2CppExport.sh/.bat` | 导出 C++ 代码 |
| `Proto2LuaExport.sh/.bat` | 导出 Lua 代码 |
| `Proto2GoExport.sh/.bat` | 导出 Go 代码 |

## API 参考

### Proto 协议规范

本工具对 `.proto` 文件格式有特定要求，请遵循以下规则以确保正确生成代码。

#### 文件格式要求

```protobuf
syntax = "proto3";     // 必须声明：仅支持 proto3
package Basic;
option module = 10;    // 模块 ID：文件名前缀（_0010_Name.proto）或此声明，二选一；并存时必须一致

// 请求心跳
message ReqHeartBeat
{
    int64 Timestamp = 1; // 时间戳
}
```

#### 消息命名规则

- **请求消息**：必须以 `Req` 开头（如 `ReqLogin`、`ReqHeartBeat`）
- **响应消息**：必须以 `Resp` 开头（如 `RespLogin`）
- **通知消息**：必须以 `Notify` 开头（如 `NotifyBagInfoChanged`）
- 消息名称、字段名称、枚举名称和枚举值必须使用 **UpperCamelCase**（大驼峰）命名

#### 模块 ID 规则

模块 ID 有两种声明方式，**文件名前缀优先**：

1. **文件名前缀**：`_<模块ID>_<名称>.proto`（如 `_0010_Basic.proto` → 模块 10、`_-0120_Inner_Social.proto` → 模块 -120；分隔符兼容 `-`，如 `_0010-Basic.proto`）
2. **option 声明**：`option module = <id>;`

规则：

- 两者并存时必须一致，否则构建报错
- 文件名以 `_` + 数字开头但缺少第二个分隔符（如 `_0010Basic`）视为命名错误，构建报错
- 两者皆无时构建报错

| ID 范围 | 用途 |
|---------|------|
| `0` ~ `32767` | 客户端-服务端通讯 |
| `-32768` ~ `-1` | 服务端-服务端通讯 |

#### 字段编号规则

- 消息字段编号必须**小于 800**（大于等于 800 为系统预留，会导致解析异常）
- `ErrorCode` 是响应消息中的保留字段名，请勿手动定义。`Resp` 消息会自动生成 `ErrorCode` 字段

#### 限制事项

- **不支持嵌套类型**：不允许在 message 内部嵌套 `message`、`enum` 或任何自定义类型
- **不支持 RPC 定义**：proto 文件中不允许使用 RPC 服务定义
- **仅支持 proto3**：必须声明 `syntax = "proto3";`，不支持 proto2

#### 注释规范

- 在 message 和 enum 定义**上方**添加注释：

```protobuf
// 请求心跳
message ReqHeartBeat
{
    int64 Timestamp = 1;
}
```

- 在字段行**末尾**添加行内注释：

```protobuf
// 玩家信息
message PlayerInfo
{
    int64 Id = 1;         // 角色ID
    string Name = 2;      // 角色名
    uint32 Level = 3;     // 角色等级
    int32 State = 4;      // 角色状态
}
```

#### 示例 Proto 文件

[TestProtos/](TestProtos/) 目录下提供了覆盖所有主要模式的示例 proto 文件：

| 文件 | 模式 | 模块 ID |
|------|------|---------|
| `heartbeat.proto` | 基础 Req/Resp | `1`（客户端-服务端） |
| `player.proto` | Req/Resp/Notify + enum + map | `2`（客户端-服务端） |
| `bag.proto` | enum + repeated + map + Notify | `3`（客户端-服务端） |
| `admin-s.proto` | 服务端专属协议（`-s` 后缀） | `99`（客户端-服务端） |
| `server-internal-s.proto` | 服务端间通讯（负值模块 ID） | `-1`（服务端-服务端） |

### 参数说明

#### 核心参数

| 参数 | 必填 | 默认值 | 说明 |
|------|------|--------|------|
| `--mode` | 是 | - | 语言模式：`csharp`、`typescript`、`cpp`、`lua`、`go` |
| `--inputPath` | 是 | - | `.proto` 协议文件目录路径 |
| `--outputPath` | 是 | - | 生成文件的输出路径 |
| `--namespaceName` | 否 | `""` | 生成代码的命名空间（仅 C# 有效，TypeScript 忽略） |
| `--isGenerateErrorCode` | 否 | `true` | 是否在响应消息中自动生成 `ErrorCode` 字段 |
| `--requireComments` | 否 | `none` | 注释校验级别：`none`（不校验）、`container`（message/enum 必须有注释）、`member`（字段/枚举成员必须有注释）、`all`（全部） |

#### C# 专属参数

| 参数 | 必填 | 默认值 | 说明 |
|------|------|--------|------|
| `--usingStatements` | 否 | `""` | using 语句，使用 `\|` 分隔（如 `"using System\|using ProtoBuf\|using System.Collections.Generic"`） |
| `--isGenerateDescription` | 否 | `false` | 是否生成 `[System.ComponentModel.Description]` 特性 |
| `--isServer` | 否 | `false` | 是否包含服务端专属 proto 文件（以 `-s` 或 `_s` 结尾的文件） |

> 注：布尔参数（`--isGenerateErrorCode`、`--isGenerateDescription`、`--isServer` 等）为开关语义：**出现即开启**，`--xxx false` 无法将其关闭（`false` 字面量会被忽略）；表中「默认值」指省略该参数时的取值（如 `--isGenerateErrorCode` 省略时即为开启）。

#### TypeScript 专属参数

| 参数 | 必填 | 默认值 | 说明 |
|------|------|--------|------|
| `--importPath` | 否 | `"../network/"` | 生成 import 语句的路径前缀 |
| `--isGenerateDescription` | 否 | `false` | 是否生成 JSDoc 风格注释 |

#### 其他参数

| 参数 | 必填 | 默认值 | 说明 |
|------|------|--------|------|
| `--isGenerateErrorCodeExcelFile` | 否 | `true` | 是否生成错误码 Excel 文件 |
| `--errorCodeExcelFilePath` | 否 | `""` | 错误码 Excel 文件的自定义路径 |

### 子 ID 稳定性（lock 文件）

#### 问题：行序自增会破坏协议

旧版本的 `MessageIdHandler` 按 proto 文件内的**行序**自增分配 SubId（每个模块从 10 起）。这意味着 proto 文件的任何结构性编辑都会导致该模块的 SubId 全线平移：

| 操作 | 后果 |
|------|------|
| 中间插入一条消息 | 其后所有消息的 SubId +1 |
| 调整 message 定义顺序 | 对应消息的 SubId 互换 |
| 删除一条消息 | 其后所有消息的 SubId -1 |

SubId 是运行时网络协议的寻址键（`MessageID = (Module << 16) | SubId`），一旦漂移，C# / C++ / Go / Lua / TypeScript 各端生成的注册表会与线上旧客户端**整体错位**：旧包携带的历史 Opcode 会被解析成另一条消息。行序只是文本属性，不应参与协议语义。

#### lock 机制

启用后，`MessageIdCoordinator` 会把 `(Module, 消息名) → SubId` 的映射持久化到一份 JSON lock 文件（`proto-message-ids.lock.json`），分配决策只看名字、不看行序：

| 场景 | 分配决策 |
|------|----------|
| 消息名已在 lock 中 | **沿用**历史 SubId，永不改写 |
| 消息名不在 lock 中 | **max(已用号) + 1**，不填洞 |
| lock 中存在但本次 proto 缺失（删除/重命名） | 移入 `retired` 段，**永不回收** |

关键规则：

- `schemaVersion`：lock 文件带 schema 版本号，版本不兼容时导出器直接报错，绝不静默重排
- 每个模块（`option module = <id>`）**独立计数**，跨模块 ID 空间不互通
- SubId 合法范围 `1..65535`（16 位），模块内起点为 `10`
- 序列化按 key 字典序输出，diff 只含真实变更，便于 PR review
- lock 文件写入为原子操作（先写 `.tmp` 再 rename）

#### 迁移步骤

```bash
# 1. 一次性生成种子：把「当前这一刻」的 Opcode 冻结为 lock 起点
bash tools/migrate-message-id-lock.sh
#    等价 CLI：
#    dotnet ProtoExport.dll --inputPath <protos> --outputPath <out> \
#      --mode csharp --messageIdLockPath ./proto-message-ids.lock.json --regenerate-lock

# 2. 检查并提交 lock 文件进 git
git add proto-message-ids.lock.json && git commit -m "chore(proto): freeze message id lock"

# 3. 之后所有导出（本地 + CI + Docker）都必须传 lock 路径
dotnet ProtoExport.dll ... --messageIdLockPath ./proto-message-ids.lock.json
```

> **注意**：迁移**不会回滚历史漂移**。旧版本下行序自增造成的错位已经发生，种子只是「冻结当前这一刻」，让分配从此稳定。若线上已存在漂移问题，需要另行做协议对齐。

相关 CLI 参数：

| 参数 | 说明 |
|------|------|
| `--messageIdLockPath` | lock 文件路径；**留空则禁用 lock 模式**，回退旧的行序自增行为（向后兼容） |
| `--print-lock` | 只读并打印 lock 文件内容，不触发导出（review 用） |
| `--regenerate-lock` | 把当前解析出的 Opcode 序列化为 lock 种子（一次性迁移用） |

#### merge conflict 处理

两个人在同一模块并发新增消息时，会各自算出相同的 `max+1`，合并时 lock 文件必然冲突。处理方式：

1. 取任意一边的 lock 内容（冲突双方内容等价，只是条目排序不同）
2. 重跑一次导出器（带 `--messageIdLockPath`），让后合入的新消息自动重新分配下一个可用号
3. 重新提交 lock 文件

**这是特性不是缺陷**：冲突显式化保证了「一号永不二主」——静默自动合并反而可能让两条消息共享同一个 SubId，在运行时造成解析错位。`retired` 段同理：被删除/重命名的旧号永久占用，新消息只能拿 `max+1`。

### Docker 路径映射

使用 Docker 时，路径映射规则如下：

- `-v <宿主机路径>:<容器内路径>` 将宿主机目录挂载到容器中
- `--inputPath` 和 `--outputPath` 必须填写**容器内路径**（如 `/protos`、`/output`），而非宿主机路径

```bash
# 示例：宿主机 ./my-protos -> 容器内 /protos
docker run --rm \
  -v $(pwd)/my-protos:/protos \
  -v $(pwd)/my-output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" \
  --isGenerateDescription true \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

## 依赖

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — 构建与 CLI 运行
- [Avalonia](https://avaloniaui.net/) — GUI 工具（ProtoExporterGUI）的跨平台 UI 框架
- [Docker](https://www.docker.com/)（可选）— 以容器方式运行导出器

## 文档与资源

- [官方文档](https://gameframex.doc.alianblank.com)
- [通讯协议规范](https://gameframex.doc.alianblank.com/zh-CN/protobuf/require.html)
- [注意事项](https://gameframex.doc.alianblank.com/zh-CN/protobuf/note.html)

## 社区与支持

![QQ](https://img.shields.io/badge/QQ-467608841%2F233840761-EB1923?style=for-the-badge&logo=qq&logoColor=white)
[![Bilibili](https://img.shields.io/badge/Bilibili-00A1D6?style=for-the-badge&logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?style=for-the-badge&logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)
[![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[<img src="https://cdn.jsdelivr.net/npm/devicon@2/icons/linkedin/linkedin-original.svg" height="28" alt="LinkedIn" />](https://www.linkedin.com/in/alianblank)
[![Reddit](https://img.shields.io/badge/Reddit-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/GameFrameX/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/alian_blank)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
[![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?style=for-the-badge&logo=bluesky&logoColor=white)](https://bsky.app/profile/alianblank.bsky.social)

## 更新日志

详见 [CHANGELOG.md](CHANGELOG.md)。

## 开源协议

详见 [LICENSE.md](LICENSE.md) 文件。

<!--
EN: See [LICENSE.md](LICENSE.md) for license information.
zh-CN: 详见 [LICENSE.md](LICENSE.md) 文件。
zh-TW: 詳見 [LICENSE.md](LICENSE.md) 檔案。
ja: 詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
ko: 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
-->
