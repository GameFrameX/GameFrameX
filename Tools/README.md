<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX.Tools

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Tools?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Tools/releases)
[![License](https://img.shields.io/badge/license-MIT+Apache%202.0-orange.svg)](LICENSE)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)

**All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams**

[📖 Documentation](https://gameframex.doc.alianblank.com) • [💬 QQ Group: 467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)

---

🌐 **Language**: **English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

---

</div>

# ProtoExport Tool

A language-oriented tool for converting Proto protocol files into multi-language code. Supports C#, TypeScript, C++ (planned), and Lua (planned).

# Build

The `ProtoExport` project is configured to emit its build output to a **fixed sibling path**: `../Protobuf/Tools/` (relative to the repository root, i.e. `<workspace>/Protobuf/Tools/`). The path is anchored to the `.csproj` location via `$(MSBuildThisFileDirectory)`, so it resolves identically on any machine that clones `Tools` and `Protobuf` as sibling directories.

This fixed location is consumed by the `Proto2*Export.{sh,bat}` scripts in the `Protobuf` repo (e.g. `dotnet ./Tools/ProtoExport.dll ...`), which expect the DLL at `Protobuf/Tools/ProtoExport.dll`.

```bash
# From the Tools/ProtoExport directory (or the repo root)
dotnet build ProtoExport/ProtoExport.csproj -c Release
# Output lands at: ../Protobuf/Tools/ProtoExport.dll  (no TFM/RID subdirectory)
```

Build properties that make the fixed path work (set in `ProtoExport/ProtoExport.csproj`):

- `OutputPath` = `$(MSBuildThisFileDirectory)../../Protobuf/Tools/` — single output dir for Debug and Release.
- `AppendTargetFrameworkToOutputPath` = `false` — no `net10.0/` subdirectory.
- `AppendRuntimeIdentifierToOutputPath` = `false` — no `win-x64/`, `linux-arm64/` subdirectory.

> `OutputPath` is used instead of `OutDir` to avoid being overridden by the SDK's default `output-paths` target.


# Docker

Pre-built Docker images are available for `linux/amd64` and `linux/arm64`.

**Docker Hub**

```bash
docker pull gameframex/gameframex-tools:latest
```

**GitHub Container Registry (GHCR)**

```bash
docker pull ghcr.io/gameframex/gameframex.tools:latest
```

**Usage**

```bash
docker run --rm \
  -v /path/to/protos:/protos \
  -v /path/to/output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" --isGenerateDescription true --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

# Proto Protocol Specification

This tool has specific requirements for `.proto` file formatting. Please follow the rules below to ensure correct code generation.

## File Format Requirements

```protobuf
syntax = "proto3";     // Required: only proto3 is supported
package Basic;
option module = 10;    // Required: module ID must be defined

// Request heartbeat
message ReqHeartBeat
{
    int64 Timestamp = 1; // Timestamp
}
```

## Message Naming Rules

- **Request messages**: Must start with `Req` (e.g., `ReqLogin`, `ReqHeartBeat`)
- **Response messages**: Must start with `Resp` (e.g., `RespLogin`)
- **Notification messages**: Must start with `Notify` (e.g., `NotifyBagInfoChanged`)
- All message names, field names, enum names, and enum values must use **UpperCamelCase**

## Module ID Rules

Module ID is defined via `option module = <id>;`:

| ID Range | Purpose |
|----------|---------|
| `0` ~ `32767` | Client-Server communication |
| `-32768` ~ `-1` | Server-Server communication |

## Field Numbering Rules

- Message field numbers must be **less than 800** (values >= 800 are system-reserved and will cause parse errors)
- `ErrorCode` is a reserved field name in response messages — do not define it manually. `Resp` messages automatically generate an `ErrorCode` field

## Restrictions

- **No nested types**: Nesting of `message`, `enum`, or any custom type inside another message is not supported
- **No RPC definitions**: RPC service definitions in proto files are not supported
- **Only proto3**: `syntax = "proto3";` is required; proto2 is not supported

## Comment Standards

- Add a comment line **above** message and enum definitions:

```protobuf
// Request heartbeat
message ReqHeartBeat
{
    int64 Timestamp = 1;
}
```

- Add **inline** comments at the end of field lines:

```protobuf
// Player information
message PlayerInfo
{
    int64 Id = 1;         // Player ID
    string Name = 2;      // Player name
    uint32 Level = 3;     // Player level
    int32 State = 4;      // Player state
}
```

For the complete protocol specification, see the [Protocol Requirements](https://gameframex.doc.alianblank.com/en-US/protobuf/require.html) and [Notes](https://gameframex.doc.alianblank.com/en-US/protobuf/note.html) documentation.

## Example Proto Files

The [TestProtos/](TestProtos/) directory contains example proto files covering all major patterns:

| File | Pattern | Module ID |
|------|---------|-----------|
| `heartbeat.proto` | Basic Req/Resp | `1` (client-server) |
| `player.proto` | Req/Resp/Notify + enum + map | `2` (client-server) |
| `bag.proto` | enum + repeated + map + Notify | `3` (client-server) |
| `admin-s.proto` | Server-only proto (`-s` suffix) | `99` (client-server) |
| `server-internal-s.proto` | Server-server communication (negative module ID) | `-1` (server-server) |

---

# 子 ID 稳定性（lock 文件）

## 问题：行序自增会破坏协议

旧版本的 `MessageIdHandler` 按 proto 文件内的**行序**自增分配 SubId（每个模块从 10 起）。这意味着 proto 文件的任何结构性编辑都会导致该模块的 SubId 全线平移：

| 操作 | 后果 |
|------|------|
| 中间插入一条消息 | 其后所有消息的 SubId +1 |
| 调整 message 定义顺序 | 对应消息的 SubId 互换 |
| 删除一条消息 | 其后所有消息的 SubId -1 |

SubId 是运行时网络协议的寻址键（`MessageID = (Module << 16) | SubId`），一旦漂移，C# / C++ / Go / Lua / TypeScript 各端生成的注册表会与线上旧客户端**整体错位**：旧包携带的历史 Opcode 会被解析成另一条消息。行序只是文本属性，不应参与协议语义。

## lock 机制

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

## 迁移步骤

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

## merge conflict 处理

两个人在同一模块并发新增消息时，会各自算出相同的 `max+1`，合并时 lock 文件必然冲突。处理方式：

1. 取任意一边的 lock 内容（冲突双方内容等价，只是条目排序不同）
2. 重跑一次导出器（带 `--messageIdLockPath`），让后合入的新消息自动重新分配下一个可用号
3. 重新提交 lock 文件

**这是特性不是缺陷**：冲突显式化保证了「一号永不二主」——静默自动合并反而可能让两条消息共享同一个 SubId，在运行时造成解析错位。`retired` 段同理：被删除/重命名的旧号永久占用，新消息只能拿 `max+1`。

---

# Parameter Reference

## Core Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `--mode` | Yes | - | Language mode: `csharp`, `typescript`, `cpp`, `lua` |
| `--inputPath` | Yes | - | Path to the `.proto` files directory |
| `--outputPath` | Yes | - | Output path for generated files |
| `--namespaceName` | No | `""` | Namespace for generated code (C# only, ignored by TypeScript) |
| `--isGenerateErrorCode` | No | `true` | Whether to auto-generate `ErrorCode` field in response messages |
| `--requireComments` | No | `none` | Comment validation level: `none` (no validation), `container` (message/enum must have comments), `member` (fields/enum members must have comments), `all` (both) |

## C# Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `--usingStatements` | No | `""` | Using statements separated by `\|` (e.g., `"using System\|using ProtoBuf\|using System.Collections.Generic"`) |
| `--isGenerateDescription` | No | `false` | Whether to generate `[System.ComponentModel.Description]` attributes |
| `--isServer` | No | `false` | Whether to include server-only proto files (files ending with `-s` or `_s`) |

## TypeScript Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `--importPath` | No | `"../network/"` | Import path prefix for generated import statements |
| `--isGenerateDescription` | No | `false` | Whether to generate JSDoc-style comments |

## Legacy Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `--isGenerateErrorCodeExcelFile` | No | `true` | Whether to generate error code Excel file |
| `--errorCodeExcelFilePath` | No | `""` | Custom path for error code Excel file |

---

# Mode Details and Examples

| Mode | Output Language | File Extension | Description |
|------|----------------|----------------|-------------|
| `csharp` | C# | `.cs` | For Server, Unity, Godot, Stride, Flax, etc. |
| `typescript` | TypeScript | `.ts` | For LayaAir, Cocos Creator, Phaser, etc. |
| `cpp` | C++ | `.h` | For Unreal Engine, etc. |
| `lua` | Lua | `.lua` | For Defold, Solar2D, Dora SSR, etc. |
| `go` | Go | `.go` | For Go game servers, etc. |

## C# Mode

Generates C# code with `[ProtoContract]` / `[ProtoMember]` attributes. All behavior is controlled via CLI parameters — no hardcoded engine-specific logic.

### Server Export

Generates code with server-specific using statements, `[Description]` attributes, and includes server-only proto files.

**Local:**

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

**Docker:**

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

### Unity Export

Generates code with Unity-specific using statements. Server-only proto files are automatically skipped.

```bash
dotnet ProtoExport.dll \
  --mode csharp \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Unity/Assets/Hotfix/Proto \
  --namespaceName Hotfix.Proto \
  --isGenerateErrorCode true
```

### Godot Export

Same as Unity but with Godot-specific namespace.

```bash
dotnet ProtoExport.dll \
  --mode csharp \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Godot/Proto \
  --namespaceName Proto \
  --isGenerateErrorCode true
```

## TypeScript Mode

Generates `.ts` files with `export namespace`, `export class`, and `export enum`, plus an aggregated `ProtoMessageRegister.ts` file. Server-only proto files are automatically skipped.

### Default Import Path

```bash
dotnet ProtoExport.dll \
  --mode typescript \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Laya/src/gameframex/protobuf \
  --isGenerateErrorCode true
```

### Custom Import Path

```bash
dotnet ProtoExport.dll \
  --mode typescript \
  --importPath "./lib/network/" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../CocosCreator/assets/scripts/protobuf \
  --isGenerateErrorCode true
```

**Docker:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Laya/src/gameframex/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode typescript --inputPath /protos --outputPath /output
```

## C++ Mode

Generates C++ header files with `#pragma once`, namespace, `enum class`, and class definitions. Classes with `MessageObject` base include `MESSAGE_ID` and `Clear()` method.

```bash
dotnet ProtoExport.dll \
  --mode cpp \
  --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Unreal/Source/Proto \
  --namespaceName GameFrameX.Proto
```

## Lua Mode

Generates `.lua` files with LuaDoc (EmmyLua) type annotations and module-based message definitions. Includes a `ProtoMessageRegister.lua` aggregate file.

```bash
dotnet ProtoExport.dll \
  --mode lua \
  --importPath "./network/" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Defold/scripts/protobuf
```

**Docker:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Defold/scripts/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode lua --importPath "./network/" --inputPath /protos --outputPath /output
```

## Go Mode

Generates Go struct definitions with protobuf tags, enum type definitions, and a `message_register.go` aggregate file. Uses `--namespaceName` as the Go package name (last segment if dot-separated).

```bash
dotnet ProtoExport.dll \
  --mode go \
  --usingStatements "google.golang.org/protobuf/runtime/protoimpl" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../GoServer/proto \
  --namespaceName proto
```

**Docker:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./GoServer/proto:/output \
  gameframex/gameframex-tools:latest \
  --mode go --inputPath /protos --outputPath /output --namespaceName proto
```

---

# GUI Tool (ProtoExporterGUI)

A cross-platform Avalonia GUI that wraps the ProtoExport CLI, so you can generate code without typing command-line arguments by hand.

## Features

- Switch between 7 export modes (Server / Unity / Godot / TypeScript / C++ / Lua / Go) from a dropdown
- Edit every CLI parameter visually (namespace, using/import, comment validation level, error codes, Description, server mode)
- Browse and pick paths with the folder picker (no more typing paths by hand)
- Switch the UI between Chinese and English on the fly
- Persistent configuration (saved per mode to `Setting.json` in the program directory, deep-merged with defaults for lossless upgrades)
- Real-time log output panel

## Build & Publish

```bash
dotnet publish ProtoExporterGUI/ProtoExporterGUI.csproj -c Release -r win-x64 --no-self-contained
# Other RIDs: osx-arm64 / osx-x64 / linux-x64 / linux-arm64
```

The output is a single-file executable (runtime not included; the target machine needs the .NET 10 runtime preinstalled).

## Usage

1. Launch the app
2. Select the export mode
3. Fill in or browse for the input/output paths
4. Click Export
5. Check the log panel

The GUI is functionally equivalent to the CLI; for CI/automation scenarios, the CLI or Docker is still recommended.

---

# Quick Export Scripts

Pre-built scripts are available in the `Protobuf/` directory:

| Script | Description |
|--------|-------------|
| `Proto2CsExport_Server.sh/.bat` | Export C# for Server |
| `Proto2CsExport_Client.sh/.bat` | Export C# for Unity Client |
| `Proto2TsExport.sh/.bat` | Export TypeScript |
| `Proto2CppExport.sh/.bat` | Export C++ |
| `Proto2LuaExport.sh/.bat` | Export Lua |
| `Proto2GoExport.sh/.bat` | Export Go |

---

# Docker Path Mapping

When using Docker, paths are mapped as follows:

- `-v <host-path>:<container-path>` mounts a host directory into the container
- `--inputPath` and `--outputPath` must reference the **container-side** paths (e.g. `/protos`, `/output`), not the host paths

```bash
# Example: host ./my-protos -> container /protos
docker run --rm \
  -v $(pwd)/my-protos:/protos \
  -v $(pwd)/my-output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" \
  --isGenerateDescription true \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```
