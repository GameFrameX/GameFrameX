<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX.Protobuf

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Protobuf?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Protobuf/releases)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)
[![CI](https://github.com/GameFrameX/GameFrameX.Protobuf/actions/workflows/proto-export.yml/badge.svg)](https://github.com/GameFrameX/GameFrameX.Protobuf/actions/workflows/proto-export.yml)

**All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams**

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#quick-start) · [Multi-Language Releases](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## Project Overview

GameFrameX.Protobuf is the unified network protocol definition repository for the GameFrameX framework. It uses Protocol Buffers 3 (`proto3`) and organizes message and error-code definitions by business module. Each `.proto` file is identified by a numeric module ID (the suffix in the filename), which is used for routing and error-code generation across client and server.

Code generation is driven by the [GameFrameX.Tools `ProtoExport`](https://github.com/GameFrameX/GameFrameX.Tools) tool. Pick whichever workflow fits you:

- **CI (zero setup)** — every `push` auto-exports all languages and publishes to the rolling [`latest` Release](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest). Just download.
- **Docker** — `docker run gameframex/gameframex-tools:latest ...`, no toolchain to install.
- **Local scripts** — build `Tools/ProtoExport` (.NET 10) yourself, drop the output into this repo's `Tools/` directory, then run `Proto2*Export.sh/.bat`. See [Export Tool](#export-tool) for details.

Full documentation is hosted at the [GameFrameX documentation site](https://gameframex.doc.alianblank.com/protobuf/require).

## Protocol Modules

| Proto File | Module | Description |
|------------|--------|-------------|
| `_0002_InnerBasic.proto` | 2 | Internal basic protocol |
| `_0010_Basic.proto` | 10 | Basic protocol |
| `_0020_Common.proto` | 20 | Common protocol (error codes, shared types) |
| `_0100_Bag.proto` | 100 | Inventory / bag protocol |
| `_0120_Social.proto` | 120 | Social protocol |
| `_-0120_Inner_Social.proto` | -120 | Internal social protocol (server-side) |
| `_0300_User.proto` | 300 | User / account protocol |
| `_0310_Attribute.proto` | 310 | Player attribute sync protocol |
| `_0400_Room.proto` | 400 | Room protocol |
| `_0410_RockPaperScissors.proto` | 410 | Rock-paper-scissors mini-game protocol |
| `_0500_Mail.proto` | 500 | Mail system protocol |

## Protocol Conventions

New to protobuf? This section is a step-by-step tutorial. Read it top to bottom and you'll be able to add a new protocol module even if you've never written a `.proto` file. Each step comes with a plain-language explanation, a minimal example, and the rule behind it. For the strict, tool-enforced rule list, see [Protocol Requirements](#protocol-requirements) below.

### Before You Start — Three Concepts in Plain Terms

- **Protobuf (`.proto`)** is a "form template" both sides agree on for exchanging data — like a printed order form where every field has a fixed name and box, so the client and server never misunderstand each other.
- **Module ID** is a "sorting number". Think of a courier's area codes: each business area (bag, mail, room…) gets one number, and every message is routed to the right handler by that number.
- **External vs Internal protocol** — External protocols are the "menu" the client can see and call; internal protocols are "back-kitchen signals" passed only between servers. The two must never mix, or the client could call something it shouldn't.

### Step 1 — Create the File

Every business area lives in its own file named `_<ModuleID:0000>_<Domain>.proto` — **every filename starts with `_`, followed by the module ID zero-padded to 4 digits**, so all files line up in numerical order identically in any file browser. The filename tells you the routing number and domain at a glance.

```protobuf
// File: _0100_Bag.proto
syntax = "proto3";      // always proto3 — the modern protobuf syntax
package Bag;            // the domain name (PascalCase)
option module = 100;    // the routing number; must match the 0100 in the filename
```

Line by line:

- `syntax = "proto3";` — declares the modern protobuf syntax. Every file starts with this line.
- `package Bag;` — this file's domain is "Bag". PascalCase means the first letter is uppercase.
- `option module = 100;` — assigns routing number 100. **It must equal the `0100` in the filename.**

Rules:

- Filename: `_<ModuleID:0000>_<Domain>.proto`, e.g. `_0500_Mail.proto`.
- Positive number = external protocol (client ↔ server); negative = internal (server ↔ server). A negative ID keeps its sign in the filename (`_-0120_Inner_Social.proto` for module -120); the leading `_` on every file keeps names valid (never starting with `-`) and uniformly sorted.
- Internal files start with `Inner`, e.g. `_0002_InnerBasic.proto`.

**Why** — Putting the module ID in the filename makes the filename itself the routing key: you can tell the domain at a glance, and two files can never quietly share one number. The `Inner` prefix tags internal protocols so they can be filtered out and never leak to the client.

### Step 2 — Define Your Data: Messages & Fields

A **message** is a "form" — a bundle of related fields. A **field** is one box on that form, with a name, a type, and a number.

```protobuf
message BagItem {
  int32 ItemId = 1; // item ID
  int64 Count = 2;  // item quantity
}
```

Line by line:

- `message BagItem { ... }` — defines a form named `BagItem`.
- `int32 ItemId = 1;` — a box named `ItemId`, type `int32` (a small integer), numbered `1`.
- `int64 Count = 2;` — a box named `Count`, type `int64` (a large integer), numbered `2`.
- The `// ...` at the end of a line is a comment that explains the field.

Rules:

- Field names are PascalCase; numbers start at 1 and go up without skipping.
- If you delete a field, block its number with `reserved` — never reuse a number.
- Every field needs a trailing comment.

How to pick a type (plain version):

| The value is… | Use | Example |
|---------------|-----|---------|
| A player / instance ID (can be huge) | `int64` | `PlayerId` |
| A config / item ID (small range) | `int32` | `ItemId` |
| A quantity (can stack up) | `int64` | `Count` |
| A timestamp | `int64` | `CreateTime` |
| Level / avatar (small, never negative) | `uint32` | `Level` |
| A status with fixed options | an enum (Step 4) | `RoomStatus` |
| A list / dictionary | `repeated` / `map` | `repeated RoomPlayerInfo` |

**Why** — Numbers must stay contiguous because a field number is its wire identifier: gaps waste space, and reusing a shipped number makes old clients' data land in the new field, silently corrupting it. Types follow "enough range, no overflow": big IDs use `int64`; small IDs use `int32` to save bytes.

### Step 3 — Make Them Talk: Request / Response / Notify

Now define how the client and server interact. There are three message roles, told apart by their name prefix:

| Prefix | Who starts it | Plain meaning |
|--------|---------------|---------------|
| `Req<Name>` | Client | "I'm asking you something" |
| `Resp<Name>` | Server replies | "Here's the answer" (same `<Name>` as the request) |
| `Notify<Name>` | Server pushes | "Heads up — something changed" (no prior request) |

```protobuf
message ReqMailList { ... }        // client asks for the mail list
message RespMailList { ... }       // server returns the list — note the matching name
message NotifyMailChanged { ... }  // server proactively pushes a mail update
message MailInfo { ... }           // a reusable data block used inside the above
```

Rules:

- Every request needs a same-named response: `ReqMailList` ↔ `RespMailList`.
- Use `Notify` only for server-initiated pushes.
- Pull shared data out into `<Name>Info` so it's defined once and reused.

**Why** — Pairing Req/Resp guarantees every question gets an answer; the matching name makes the pair obvious to people and code generators. `<Name>Info` avoids duplicating the same structure across multiple messages.

### Step 4 — Represent Status with Enums

An **enum** is a multiple-choice list — like an order status that can only be "pending / paid / shipped", nothing else.

```protobuf
enum RoomStatus {
  None = 0;     // no state / invalid
  Waiting = 1;  // waiting to start
  Playing = 3;  // game in progress
}
```

Rules:

- Enum names and values are PascalCase.
- The first value is always `0`, reserved for the default / none state (`None`, `Unknown`).

**Why** — proto3 forces the first value to be `0`. Keeping it as `None` / `Unknown` gives a safe default: an unset field reads as "no state" instead of accidentally matching a real one — preventing a whole class of bugs.

### Step 5 — Define Error Codes

When something fails, give it a number so both sides know exactly what went wrong. There are two layers:

**Generic codes** — common failures every module shares (bad parameters, insufficient cost, not found). They live in `_0020_Common.proto` as `OperationStatusCode`, numbered from `0` upward.

**Business codes** — failures specific to your module. The number is computed as **`ModuleID × 1000 + a 3-digit ordinal`**.

```protobuf
// Mail is module 500, so its codes start at 500001
// 500001 = 500 × 1000 + 1
enum MailErrorCode {
  MailNotFound = 500001;        // mail doesn't exist
  MailAlreadyDeleted = 500002;  // mail was already deleted
}
```

Rule: the client receives the code as a plain `int`. On success, leave it unset — proto3's default `0` then means "success", so the common case costs nothing to send.

**Why** — The formula makes a code self-describing: `500001` is obviously Mail's, it's globally unique with no coordination, and each module gets 1000 slots to grow. Sending success as "nothing" saves bytes because success is the majority of responses.

### Step 6 — Write Comments

Comments are the only documentation both sides share — a `.proto` file has no surrounding context, so without a comment the other end can only guess.

- Before a message: write its purpose.
- After a field or enum value: write what it means.
- If a field is an `int` that actually holds enum values, name the enum in parentheses, e.g. `// status (RoomStatus)`, so the reader knows where the valid values come from.

**Why** — An `int` alone doesn't reveal its valid set; naming the enum tells the reader exactly where to look.

### Full Example

A hypothetical `_0600_Quest` (quest system) module exercising every rule above:

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

## Protocol Requirements

The hard rules the `ProtoExport` tool enforces. Authoritative source: [GameFrameX.Tools README](https://github.com/GameFrameX/GameFrameX.Tools#readme).

### File Format

```protobuf
syntax = "proto3";     // Required: only proto3 is supported
package Basic;
option module = 10;    // Required: module ID must be defined
```

### Message Naming

- **Request**: `Req<Name>` (e.g. `ReqLogin`, `ReqHeartBeat`)
- **Response**: `Resp<Name>` (e.g. `RespLogin`)
- **Notification**: `Notify<Name>` (e.g. `NotifyBagInfoChanged`)
- All message, field, enum names and enum values must use **UpperCamelCase**.

### Module ID

| ID Range | Purpose |
|----------|---------|
| `0` ~ `32767` | Client ↔ Server |
| `-32768` ~ `-1` | Server ↔ Server (internal) |

### Field Numbering

- Message field numbers must be **less than 800** (values `>= 800` are system-reserved and will cause parse errors).
- `ErrorCode` is a **reserved field name** in `Resp` messages — do not define it manually. The tool auto-generates an `ErrorCode` field on every `Resp`.

### Restrictions

- **No nested types** — `message` / `enum` cannot be declared inside another message.
- **No RPC definitions** — `service` blocks are not supported.
- **Only proto3** — `syntax = "proto3";` is required; proto2 is not supported.

### Comment Standards

- A comment line **above** every `message` / `enum` describing its purpose.
- An **inline** comment at the end of every field / enum-value line.

### Server-Only Files

The export tool identifies server-only proto files by **filename suffix** `-s` or `_s` (e.g. `player-s.proto`, `economy_s.proto`). Pass `--isServer true` to include them; with the default `--isServer false` they are skipped, so server-only messages never leak to clients.

Internal protocols additionally carry a **negative module ID** for routing separation (see the Module ID table above).

> **Note on the current repository:** internal files here use an `Inner_` prefix together with a negative module ID (e.g. `_-0120_Inner_Social.proto`). Both the `-s`/`_s` suffix and the negative-ID convention achieve server-only routing — pick one and stay consistent within a module.

## Supported Export Languages

| Language | Mode & Flags | Local Script | Docker |
|----------|--------------|--------------|--------|
| C# (Server) | `csharp --isServer true` | `Proto2CsExport_Server.sh` / `.bat` | ✅ |
| C# (Client / Unity / Godot) | `csharp` | `Proto2CsExport_Client.sh` / `.bat` | ✅ |
| C++ | `cpp` | `Proto2CppExport.sh` / `.bat` | ✅ |
| Go | `go` | `Proto2GoExport.sh` / `.bat` | ✅ |
| Lua | `lua` | `Proto2LuaExport.sh` / `.bat` | ✅ |
| TypeScript | `typescript` | `Proto2TsExport.sh` / `.bat` | ✅ |
| TypeScript (LayaBox) | `typescript` | `Proto2TsExport_LayaBox.sh` | ✅ |

### Docker Examples

**C# (Server):**

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

**Go:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./GoServer/proto:/output \
  gameframex/gameframex-tools:latest \
  --mode go --inputPath /protos --outputPath /output --namespaceName proto
```

**TypeScript:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Laya/src/gameframex/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode typescript --inputPath /protos --outputPath /output
```

**Lua:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Defold/scripts/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode lua --importPath "./network/" --inputPath /protos --outputPath /output
```

**C++:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Unreal/Source/Proto:/output \
  gameframex/gameframex-tools:latest \
  --mode cpp \
  --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto
```

Path mapping: `-v <host>:<container>` mounts a host directory; `--inputPath` / `--outputPath` must reference the **container-side** paths (`/protos`, `/output`), not the host paths.

## Export Parameters

### Core

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `--mode` | Yes | - | `csharp` / `typescript` / `cpp` / `lua` / `go` |
| `--inputPath` | Yes | - | Directory containing the `.proto` files |
| `--outputPath` | Yes | - | Output directory for generated files |
| `--namespaceName` | No | `""` | C# namespace (or Go package last segment if dot-separated) |
| `--isGenerateErrorCode` | No | `true` | Auto-generate `ErrorCode` field on `Resp` messages |
| `--requireComments` | No | `none` | Comment validation level: `none` / `container` / `member` / `all` |

### C#

| Parameter | Default | Description |
|-----------|---------|-------------|
| `--usingStatements` | `""` | Using statements separated by `\|` (e.g. `"using System\|using ProtoBuf"`) |
| `--isGenerateDescription` | `false` | Generate `[System.ComponentModel.Description]` attributes |
| `--isServer` | `false` | Include server-only proto files (filename ends with `-s` or `_s`) |

### TypeScript

| Parameter | Default | Description |
|-----------|---------|-------------|
| `--importPath` | `"../network/"` | Import path prefix for generated import statements |
| `--isGenerateDescription` | `false` | Generate JSDoc-style comments |

### Legacy

| Parameter | Default | Description |
|-----------|---------|-------------|
| `--isGenerateErrorCodeExcelFile` | `true` | Generate the error-code Excel file |
| `--errorCodeExcelFilePath` | `""` | Custom path for the error-code Excel file |

## Docker

Pre-built images are available for `linux/amd64` and `linux/arm64`:

```bash
# Docker Hub
docker pull gameframex/gameframex-tools:latest

# GitHub Container Registry (GHCR)
docker pull ghcr.io/gameframex/gameframex.tools:latest
```

The image entrypoint is the `ProtoExport` tool — append parameters directly after the image name:

```bash
docker run --rm \
  -v /path/to/protos:/protos \
  -v /path/to/output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --inputPath /protos --outputPath /output
```

## CI Pipeline

This repository ships [`.github/workflows/proto-export.yml`](.github/workflows/proto-export.yml). It runs automatically on **every `push`** and on manual dispatch.

| Step | What happens |
|------|--------------|
| 1 | Pull `gameframex/gameframex-tools:latest` |
| 2 | Mount the `.proto` sources into the container at `/protos` |
| 3 | Export all six target languages in parallel (build matrix) |
| 4 | Collect each language's output as a workflow artifact |
| 5 | On `push` to `main`, (re)publish a rolling **`latest` Release** with all artifacts attached |

Download the latest generated code from the [Releases page](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) — no toolchain required.

## Export Tool

Code generation is driven by `ProtoExport`, a .NET 10 console app in the standalone [GameFrameX.Tools](https://github.com/GameFrameX/GameFrameX.Tools) repository. **This repo does not ship the binary** — pick one of three workflows (see [Quick Start](#quick-start)):

- **CI** — zero setup; download generated code from the latest Release.
- **Docker** — run the pre-built image, no local toolchain.
- **Local scripts** — build the tool yourself and drop the output into this repo's `Tools/` directory (full steps below).

### Tool repository

| Project | Repository | Description |
|---------|------------|-------------|
| GameFrameX.Tools | https://github.com/GameFrameX/GameFrameX.Tools | Source of the `ProtoExport` generator, full parameter docs, Docker image |

`ProtoExport` is a .NET 10 console project (`ProtoExport.csproj`, `OutputType=Exe`) that depends on the `GameFrameX.Foundation.Options` NuGet package for command-line parsing.

### Prerequisites

- **.NET 10 SDK** — required both to build the tool and to run the export scripts.
- Verify: `dotnet --version` should print `10.x.x`.

### Build

```bash
# 1. Clone the tool repository
git clone https://github.com/GameFrameX/GameFrameX.Tools.git
cd GameFrameX.Tools/ProtoExport

# 2. Build (Release)
dotnet build -c Release

# 3. Output lands in bin/Release/net10.0/
ls bin/Release/net10.0/
```

### Build artifacts

Copy these files from `GameFrameX.Tools/ProtoExport/bin/Release/net10.0/` into this repo's `Tools/` directory:

| File | Required | Purpose |
|------|:--------:|---------|
| `ProtoExport.dll` | ✅ | Main assembly |
| `ProtoExport.deps.json` | ✅ | Dependency manifest (required at runtime) |
| `ProtoExport.runtimeconfig.json` | ✅ | Runtime config (pins .NET 10) |
| `GameFrameX.Foundation.Options.dll` | ✅ | Command-line parsing dependency |
| `ProtoExport` / `ProtoExport.exe` | ⛔ | Native apphost — scripts don't use it |
| `ProtoExport.pdb` | ⛔ | Debug symbols |

```bash
# Copy the four required files into this repo's Tools/ directory
cp bin/Release/net10.0/ProtoExport.dll                   /path/to/GameFrameX.Protobuf/Tools/
cp bin/Release/net10.0/ProtoExport.deps.json             /path/to/GameFrameX.Protobuf/Tools/
cp bin/Release/net10.0/ProtoExport.runtimeconfig.json    /path/to/GameFrameX.Protobuf/Tools/
cp bin/Release/net10.0/GameFrameX.Foundation.Options.dll /path/to/GameFrameX.Protobuf/Tools/

# Or copy everything in one shot
cp bin/Release/net10.0/* /path/to/GameFrameX.Protobuf/Tools/
```

> The native launcher (`ProtoExport` on macOS/Linux, `ProtoExport.exe` on Windows) is optional — every `Proto2*` script launches the tool uniformly via `dotnet ./Tools/ProtoExport.dll`, which is cross-platform.

### Verify

```bash
cd /path/to/GameFrameX.Protobuf
./Proto2CsExport_Client.sh    # macOS / Linux
Proto2CsExport_Client.bat     # Windows
```

A line like `协议扫描完成: ... 导出 N 个，跳过 M 个` means the tool is ready.

### Relationship with the export scripts

Every `Proto2*.sh` / `.bat` script at the repo root:

1. Runs from the repo root;
2. Launches the generator you placed in `Tools/` via `dotnet ./Tools/ProtoExport.dll`;
3. Passes language-specific flags (`--mode`, `--isServer`, etc.).

So once `Tools/` holds the correct artifacts, **all scripts run directly** — you never touch per-language parameters by hand.

### Updating the tool

When `ProtoExport` is updated upstream, re-run "Build + copy artifacts" to overwrite the files in `Tools/`. Keep the tool version aligned with this repo's protocol spec — pull this repo's latest changes and rebuild the tool together.

## Quick Start

**Option A — Download from CI (zero setup):** grab the bundle for your language from the [latest Release](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest).

**Option B — Docker:**

```bash
docker run --rm \
  -v "$PWD":/protos \
  -v "$PWD/output":/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

**Option C — Local scripts:** build the tool yourself and drop it into `Tools/` (full steps in [Export Tool](#export-tool)), then from the repo root:

```bash
./Proto2CsExport_Server.sh   # C# (server)
./Proto2GoExport.sh          # Go
```

Every script invokes the generator in `Tools/` via `dotnet ./Tools/ProtoExport.dll`; see the [export documentation](https://gameframex.doc.alianblank.com/protobuf/require) for parameter details.