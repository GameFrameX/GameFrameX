<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX.Tools

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Tools?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Tools/releases)
[![License](https://img.shields.io/badge/license-MIT+Apache%202.0-orange.svg)](LICENSE)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)

**獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使**

[📖 文檔](https://gameframex.doc.alianblank.com) • [💬 QQ群: 467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)

---

🌐 **語言**: [English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

---

</div>

# ProtoExport 工具

按語言匯出的 Proto 協議程式碼生成工具。支援 C#、TypeScript，C++ 和 Lua 正在開發中。

# 建置

`ProtoExport` 專案設定為將建置產物輸出到**固定的同級路徑**：`../Protobuf/Tools/`（相對於倉庫根，即 `<工作區>/Protobuf/Tools/`）。路徑以 `.csproj` 所在位置為錨點透過 `$(MSBuildThisFileDirectory)` 定位，任何將 `Tools` 與 `Protobuf` 克隆為同級目錄的機器都能正確解析。

該固定路徑供 `Protobuf` 倉庫中的 `Proto2*Export.{sh,bat}` 腳本消費（如 `dotnet ./Tools/ProtoExport.dll ...`），腳本期望 DLL 位於 `Protobuf/Tools/ProtoExport.dll`。

```bash
# 在 Tools/ProtoExport 目錄（或倉庫根）執行
dotnet build ProtoExport/ProtoExport.csproj -c Release
# 產物落到：../Protobuf/Tools/ProtoExport.dll（不含 TFM/RID 子目錄）
```

使固定路徑生效的建置屬性（定義於 `ProtoExport/ProtoExport.csproj`）：

- `OutputPath` = `$(MSBuildThisFileDirectory)../../Protobuf/Tools/` — Debug/Release 共用同一輸出目錄。
- `AppendTargetFrameworkToOutputPath` = `false` — 不產生 `net10.0/` 子目錄。
- `AppendRuntimeIdentifierToOutputPath` = `false` — 不產生 `win-x64/`、`linux-arm64/` 子目錄。

> 使用 `OutputPath` 而非 `OutDir`，以避免被 SDK 預設的 `output-paths` target 覆蓋。


# Docker

預建構的 Docker 映像檔支援 `linux/amd64` 和 `linux/arm64` 架構。

**Docker Hub**

```bash
docker pull gameframex/gameframex-tools:latest
```

**GitHub Container Registry (GHCR)**

```bash
docker pull ghcr.io/gameframex/gameframex.tools:latest
```

**使用範例**

```bash
docker run --rm \
  -v /path/to/protos:/protos \
  -v /path/to/output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" --isGenerateDescription true --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

# Proto 協議規範

本工具對 `.proto` 檔案格式有特定要求，請遵循以下規則以確保正確生成程式碼。

## 檔案格式要求

```protobuf
syntax = "proto3";     // 必須宣告：僅支援 proto3
package Basic;
option module = 10;    // 模組 ID：檔名前綴（_0010_Name.proto）或此宣告，二擇一；並存時必須一致

// 請求心跳
message ReqHeartBeat
{
    int64 Timestamp = 1; // 時間戳
}
```

## 訊息命名規則

- **請求訊息**：必須以 `Req` 開頭（如 `ReqLogin`、`ReqHeartBeat`）
- **回應訊息**：必須以 `Resp` 開頭（如 `RespLogin`）
- **通知訊息**：必須以 `Notify` 開頭（如 `NotifyBagInfoChanged`）
- 訊息名稱、欄位名稱、列舉名稱和列舉值必須使用 **UpperCamelCase**（大駝峰）命名

## 模組 ID 規則

模組 ID 有兩種宣告方式，**檔名前綴優先**：

1. **檔名前綴**：`_<模組ID>_<名稱>.proto`（如 `_0010_Basic.proto` → 模組 10、`_-0120_Inner_Social.proto` → 模組 -120；分隔符號相容 `-`，如 `_0010-Basic.proto`）
2. **option 宣告**：`option module = <id>;`

規則：

- 兩者並存時必須一致，否則建置報錯
- 檔名以 `_` + 數字開頭但缺少第二個分隔符號（如 `_0010Basic`）視為命名錯誤，建置報錯
- 兩者皆無時建置報錯

| ID 範圍 | 用途 |
|---------|------|
| `0` ~ `32767` | 用戶端-伺服器端通訊 |
| `-32768` ~ `-1` | 伺服器端-伺服器端通訊 |

## 欄位編號規則

- 訊息欄位編號必須**小於 800**（大於等於 800 為系統保留，會導致解析異常）
- `ErrorCode` 是回應訊息中的保留欄位名稱，請勿手動定義。`Resp` 訊息會自動生成 `ErrorCode` 欄位

## 限制事項

- **不支援巢狀型別**：不允許在 message 內部巢狀 `message`、`enum` 或任何自訂型別
- **不支援 RPC 定義**：proto 檔案中不允許使用 RPC 服務定義
- **僅支援 proto3**：必須宣告 `syntax = "proto3";`，不支援 proto2

## 註解規範

- 在 message 和 enum 定義**上方**新增註解：

```protobuf
// 請求心跳
message ReqHeartBeat
{
    int64 Timestamp = 1;
}
```

- 在欄位行**末尾**新增行內註解：

```protobuf
// 玩家資訊
message PlayerInfo
{
    int64 Id = 1;         // 角色ID
    string Name = 2;      // 角色名
    uint32 Level = 3;     // 角色等級
    int32 State = 4;      // 角色狀態
}
```

完整的協議規範請參考 [通訊協議規範](https://gameframex.doc.alianblank.com/zh-CN/protobuf/require.html) 和 [注意事項](https://gameframex.doc.alianblank.com/zh-CN/protobuf/note.html) 文件。

## 範例 Proto 檔案

[TestProtos/](TestProtos/) 目錄下提供了覆蓋所有主要模式的範例 proto 檔案：

| 檔案 | 模式 | 模組 ID |
|------|------|---------|
| `heartbeat.proto` | 基礎 Req/Resp | `1`（用戶端-伺服器端） |
| `player.proto` | Req/Resp/Notify + enum + map | `2`（用戶端-伺服器端） |
| `bag.proto` | enum + repeated + map + Notify | `3`（用戶端-伺服器端） |
| `admin-s.proto` | 伺服器端專屬協議（`-s` 後綴） | `99`（用戶端-伺服器端） |
| `server-internal-s.proto` | 伺服器端間通訊（負值模組 ID） | `-1`（伺服器端-伺服器端） |

---

# 參數說明

## 核心參數

| 參數 | 必填 | 預設值 | 說明 |
|------|------|--------|------|
| `--mode` | 是 | - | 語言模式：`csharp`、`typescript`、`cpp`、`lua` |
| `--inputPath` | 是 | - | `.proto` 協議檔案目錄路徑 |
| `--outputPath` | 是 | - | 生成檔案的輸出路徑 |
| `--namespaceName` | 否 | `""` | 生成程式碼的命名空間（僅 C# 有效，TypeScript 忽略） |
| `--isGenerateErrorCode` | 否 | `true` | 是否在回應訊息中自動生成 `ErrorCode` 欄位 |
| `--requireComments` | 否 | `none` | 註解驗證級別：`none`（不驗證）、`container`（message/enum 必須有註解）、`member`（欄位/列舉成員必須有註解）、`all`（全部） |

## C# 專屬參數

| 參數 | 必填 | 預設值 | 說明 |
|------|------|--------|------|
| `--usingStatements` | 否 | `""` | using 語句，使用 `\|` 分隔（如 `"using System\|using ProtoBuf\|using System.Collections.Generic"`） |
| `--isGenerateDescription` | 否 | `false` | 是否生成 `[System.ComponentModel.Description]` 特性 |
| `--isServer` | 否 | `false` | 是否包含伺服器端專屬 proto 檔案（以 `-s` 或 `_s` 結尾的檔案） |

## TypeScript 專屬參數

| 參數 | 必填 | 預設值 | 說明 |
|------|------|--------|------|
| `--importPath` | 否 | `"../network/"` | 生成 import 語句的路徑前綴 |
| `--isGenerateDescription` | 否 | `false` | 是否生成 JSDoc 風格註解 |

## 其他參數

| 參數 | 必填 | 預設值 | 說明 |
|------|------|--------|------|
| `--isGenerateErrorCodeExcelFile` | 否 | `true` | 是否生成錯誤碼 Excel 檔案 |
| `--errorCodeExcelFilePath` | 否 | `""` | 錯誤碼 Excel 檔案的自訂路徑 |

---

# 模式詳情與範例

| 模式 | 輸出語言 | 檔案副檔名 | 說明 |
|------|---------|-----------|------|
| `csharp` | C# | `.cs` | 適用於 Server、Unity、Godot、Stride、Flax 等 |
| `typescript` | TypeScript | `.ts` | 適用於 LayaAir、Cocos Creator、Phaser 等 |
| `cpp` | C++ | `.h` | 適用於 Unreal Engine 等 |
| `lua` | Lua | `.lua` | 適用於 Defold、Solar2D、Dora SSR 等 |
| `go` | Go | `.go` | 適用於 Go 遊戲伺服器等 |

## C# 模式

生成帶有 `[ProtoContract]` / `[ProtoMember]` 特性的 C# 程式碼。所有行為透過 CLI 參數控制，無硬編碼的引擎特定邏輯。

### 伺服器端匯出

生成帶有伺服器端 using 語句、`[Description]` 特性的程式碼，包含伺服器端專屬 proto 檔案。

**本機執行：**

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

**Docker 執行：**

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

### Unity 匯出

生成帶有 Unity using 語句的程式碼，自動跳過伺服器端專屬 proto 檔案。

```bash
dotnet ProtoExport.dll \
  --mode csharp \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Unity/Assets/Hotfix/Proto \
  --namespaceName Hotfix.Proto \
  --isGenerateErrorCode true
```

### Godot 匯出

與 Unity 類似，使用 Godot 特定的命名空間。

```bash
dotnet ProtoExport.dll \
  --mode csharp \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Godot/Proto \
  --namespaceName Proto \
  --isGenerateErrorCode true
```

## TypeScript 模式

生成包含 `export namespace`、`export class` 和 `export enum` 的 `.ts` 檔案，並生成聚合檔案 `ProtoMessageRegister.ts`。自動跳過伺服器端專屬 proto 檔案。

### 預設 import 路徑

```bash
dotnet ProtoExport.dll \
  --mode typescript \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Laya/src/gameframex/protobuf \
  --isGenerateErrorCode true
```

### 自訂 import 路徑

```bash
dotnet ProtoExport.dll \
  --mode typescript \
  --importPath "./lib/network/" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../CocosCreator/assets/scripts/protobuf \
  --isGenerateErrorCode true
```

**Docker 執行：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Laya/src/gameframex/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode typescript --inputPath /protos --outputPath /output
```

## C++ 模式

生成帶有 `#pragma once`、命名空間、`enum class` 和類別定義的 C++ 標頭檔。繼承 `MessageObject` 的類別包含 `MESSAGE_ID` 和 `Clear()` 方法。

```bash
dotnet ProtoExport.dll \
  --mode cpp \
  --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Unreal/Source/Proto \
  --namespaceName GameFrameX.Proto
```

## Lua 模式

生成帶有 LuaDoc（EmmyLua）型別註解和模組化訊息定義的 `.lua` 檔案，同時生成聚合檔案 `ProtoMessageRegister.lua`。

```bash
dotnet ProtoExport.dll \
  --mode lua \
  --importPath "./network/" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Defold/scripts/protobuf
```

**Docker 執行：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Defold/scripts/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode lua --importPath "./network/" --inputPath /protos --outputPath /output
```

## Go 模式

生成帶有 protobuf 標籤的 Go struct 定義、列舉型別定義，以及聚合檔案 `message_register.go`。使用 `--namespaceName` 作為 Go 包名（點分隔時取最後一段）。

```bash
dotnet ProtoExport.dll \
  --mode go \
  --usingStatements "google.golang.org/protobuf/runtime/protoimpl" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../GoServer/proto \
  --namespaceName proto
```

**Docker 執行：**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./GoServer/proto:/output \
  gameframex/gameframex-tools:latest \
  --mode go --inputPath /protos --outputPath /output --namespaceName proto
```

---

# GUI 工具（ProtoExporterGUI）

跨平台 Avalonia GUI，封裝 ProtoExport CLI，免去手敲命令列參數。

## 功能特性

- 7 種匯出模式（Server / Unity / Godot / TypeScript / C++ / Lua / Go）下拉切換
- 全部 CLI 參數視覺化編輯（命名空間、using/import、註解驗證級別、錯誤碼、Description、伺服器模式）
- 路徑 FolderPicker 瀏覽選擇（不再手敲）
- 中/英文介面即時切換
- 設定持久化（依模式分別儲存到程式目錄的 `Setting.json`，深度合併預設值，無損升級）
- 即時日誌輸出區

## 建置發佈

```bash
dotnet publish ProtoExporterGUI/ProtoExporterGUI.csproj -c Release -r win-x64 --no-self-contained
# 其他 RID：osx-arm64 / osx-x64 / linux-x64 / linux-arm64
```

產物為單檔可執行檔（不含執行階段，需目標機預先安裝 .NET 10 執行階段）。

## 使用步驟

1. 啟動程式
2. 選擇匯出模式
3. 填寫或瀏覽選擇輸入/輸出路徑
4. 點擊匯出
5. 檢視日誌區

GUI 與 CLI 功能等價；CI/自動化情境仍建議使用 CLI 或 Docker。

---

# 快捷匯出腳本

`Protobuf/` 目錄下提供了預設的匯出腳本：

| 腳本 | 說明 |
|------|------|
| `Proto2CsExport_Server.sh/.bat` | 匯出 C# 伺服器端程式碼 |
| `Proto2CsExport_Client.sh/.bat` | 匯出 C# Unity 用戶端程式碼 |
| `Proto2TsExport.sh/.bat` | 匯出 TypeScript 程式碼 |
| `Proto2CppExport.sh/.bat` | 匯出 C++ 程式碼 |
| `Proto2LuaExport.sh/.bat` | 匯出 Lua 程式碼 |
| `Proto2GoExport.sh/.bat` | 匯出 Go 程式碼 |

---

# Docker 路徑映射說明

使用 Docker 時，路徑映射規則如下：

- `-v <宿主機路徑>:<容器內路徑>` 將宿主機目錄掛載到容器中
- `--inputPath` 和 `--outputPath` 必須填寫**容器內路徑**（如 `/protos`、`/output`），而非宿主機路徑

```bash
# 範例：宿主機 ./my-protos -> 容器內 /protos
docker run --rm \
  -v $(pwd)/my-protos:/protos \
  -v $(pwd)/my-output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" \
  --isGenerateDescription true \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```
