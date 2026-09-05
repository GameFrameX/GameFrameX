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

**獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使**

<br />

[文檔](https://gameframex.doc.alianblank.com) · [快速開始](#快速開始) · [多語言 Release](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 項目簡介

GameFrameX.Protobuf 是 GameFrameX 框架的統一網路協議定義倉庫。採用 Protocol Buffers 3（`proto3`），按業務模組組織訊息與錯誤碼定義。每個 `.proto` 檔案以數字模組 ID（檔名後綴）標識，用於客戶端與伺服器端的訊息路由和錯誤碼生成。

程式碼生成由 [GameFrameX.Tools `ProtoExport`](https://github.com/GameFrameX/GameFrameX.Tools) 工具驅動。挑選適合你的工作流程：

- **CI（零配置）** —— 每次 `push` 都會自動匯出所有語言並發布到滾動更新的 [`latest` Release](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest)。直接下載即可。
- **Docker** —— `docker run gameframex/gameframex-tools:latest ...`，無需安裝任何工具鏈。
- **本地腳本** —— `Tools/` 目錄的 `ProtoExport` 產物由流水線每週自動同步，clone 後直接執行 `Proto2*Export.sh/.bat`。詳見[匯出工具](#匯出工具)。

完整文件託管於 [GameFrameX 文檔站](https://gameframex.doc.alianblank.com/protobuf/require)。

### 功能特性

- 統一的 `proto3` 協議定義，按數字模組 ID 組織
- 倉庫自帶腳本，一條命令匯出 C#、C++、Go、Lua、TypeScript
- 每次 `push` 由 CI 自動發布所有語言產物到滾動更新的 `latest` Release
- Docker 映像檔 + `Tools/` 產物每週自動同步，無需配置本地工具鏈

## 快速開始

### 安裝

**選項 A —— 從 CI 下載（零配置）：** 從[最新 Release](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) 取得你所需語言的套件。

**選項 B —— Docker：**

```bash
docker run --rm \
  -v "$PWD":/protos \
  -v "$PWD/output":/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

**選項 C —— 本地腳本：** `Tools/` 產物已自動同步就緒（需本地 .NET 10 SDK），在倉庫根目錄直接執行：

```bash
./Proto2CsExport_Server.sh   # C#（伺服器）
./Proto2GoExport.sh          # Go
```

## 使用範例

在倉庫根目錄直接執行自帶腳本本地匯出：

```bash
./Proto2CsExport_Server.sh   # C#（伺服器）
./Proto2GoExport.sh          # Go
```

所有腳本透過 `dotnet ./Tools/ProtoExport.dll` 啟動 `Tools/` 下自動同步的產生器。完整參數列表見[匯出參數](#匯出參數)，細節見[匯出文件](https://gameframex.doc.alianblank.com/protobuf/require)。

## 協議模組

| Proto 檔案 | 模組 | 說明 |
|------------|------|------|
| `_0002_InnerBasic.proto` | 2 | 內部基礎協議 |
| `_0010_Basic.proto` | 10 | 基礎協議 |
| `_0020_Common.proto` | 20 | 通用協議（錯誤碼、共享類型） |
| `_0100_Bag.proto` | 100 | 背包協議 |
| `_0120_Social.proto` | 120 | 社交協議 |
| `_-0120_Inner_Social.proto` | -120 | 內部社交協議（伺服器端） |
| `_0300_User.proto` | 300 | 使用者 / 帳號協議 |
| `_0310_Attribute.proto` | 310 | 玩家屬性同步協議 |
| `_0400_Room.proto` | 400 | 房間協議 |
| `_0410_RockPaperScissors.proto` | 410 | 猜拳小遊戲協議 |
| `_0500_Mail.proto` | 500 | 郵件系統協議 |

## 協議規範

第一次接觸 protobuf？本節是一個循序漸進的教學。從頭讀到尾，哪怕你從沒寫過 `.proto` 檔案，也能學會新增一個協議模組。每一步都包含大白話說明、最小範例，以及背後的規則。工具強制執行的嚴格規則清單，請見下方的[協議要求](#協議要求)。

### 動手之前 —— 三個大白話概念

- **Protobuf（`.proto`）** 是雙方約定好的「填表範本」——就像一張印好的訂單，每個格子都有固定的名稱和位置，客戶端和伺服器照著填，絕不會互相誤會。
- **模組 ID** 是一個「分揀號」。可以想像快遞公司的區域編號：每類業務（背包、郵件、房間……）各分一個號，訊息就按這個號被投遞到對應的處理人。
- **對外協議 vs 內部協議** —— 對外協議是客戶端能看見、能呼叫的「菜單」；內部協議是只在伺服器之間傳遞的「後廚暗號」。兩者絕不能混，否則客戶端可能呼叫到不該呼叫的東西。

### 第 1 步 —— 建立檔案

每個業務域放在自己的檔案裡，檔名叫 `_<ModuleID:0000>_<Domain>.proto`——**所有檔名都以 `_` 開頭，接 4 位補零的模組 ID**，這樣在任何檔案管理器裡都按模組號數值升序排列，且排序結果與環境無關。檔名一眼就能看出路由號和所屬業務域。

```protobuf
// 檔名：_0100_Bag.proto
syntax = "proto3";      // 永遠用 proto3 —— 當前的 protobuf 語法
package Bag;            // 業務域名（PascalCase）
option module = 100;    // 路由號；必須和檔名裡的 0100 對上
```

逐行解讀：

- `syntax = "proto3";` —— 宣告使用當前的 protobuf 語法。每個檔案都以此開頭。
- `package Bag;` —— 這個檔案的業務域是「Bag」。PascalCase 指首字母大寫。
- `option module = 100;` —— 分配路由號 100。**它必須和檔名裡的 `0100` 完全一致。**

規則：

- 檔名：`_<ModuleID:0000>_<Domain>.proto`，如 `_0500_Mail.proto`。
- 正數 = 對外協議（客戶端 ↔ 伺服器）；負數 = 內部協議（伺服器 ↔ 伺服器）。負數 ID 在檔名裡保留負號（`_-0120_Inner_Social.proto` 表示 module = -120）；所有檔名都以 `_` 開頭，既保證合法（不以 `-` 開頭），又統一排序。
- 內部檔案以 `Inner` 開頭，如 `_0002_InnerBasic.proto`。

**為什麼** —— 把模組 ID 寫進檔名，檔名本身就是路由鍵：一眼能看出屬於哪個業務域，兩個檔案也絕不可能悄悄佔用同一個號。`Inner` 前綴給內部協議打了標記，方便匯出時過濾掉，不會洩露給客戶端。

### 第 2 步 —— 定義資料：訊息與欄位

**訊息（message）** 是一張「表」——一組相關欄位的集合。**欄位（field）** 是表裡的一個格子，有名稱、有型別、有編號。

```protobuf
message BagItem {
  int32 ItemId = 1; // 道具 ID
  int64 Count = 2;  // 道具數量
}
```

逐行解讀：

- `message BagItem { ... }` —— 定義了一張名叫 `BagItem` 的表。
- `int32 ItemId = 1;` —— 一個名叫 `ItemId` 的格子，型別 `int32`（小整數），編號 `1`。
- `int64 Count = 2;` —— 一個名叫 `Count` 的格子，型別 `int64`（大整數），編號 `2`。
- 行尾的 `// ...` 是註釋，用來說明這個欄位是什麼意思。

規則：

- 欄位名用 PascalCase；編號從 1 開始連續往上加，不要跳號。
- 如果刪除了某個欄位，要用 `reserved` 把它的編號佔住——絕不能重複使用編號。
- 每個欄位都要寫行尾註釋。

型別怎麼選（大白話版）：

| 這個值是…… | 用 | 範例 |
|------------|-----|------|
| 玩家 / 實例 ID（可能很大） | `int64` | `PlayerId` |
| 設定 / 道具 ID（範圍小） | `int32` | `ItemId` |
| 數量（可能堆很高） | `int64` | `Count` |
| 時間戳記 | `int64` | `CreateTime` |
| 等級 / 頭像（小、不會為負） | `uint32` | `Level` |
| 有固定幾個選項的狀態 | 列舉（見第 4 步） | `RoomStatus` |
| 列表 / 字典 | `repeated` / `map` | `repeated RoomPlayerInfo` |

**為什麼** —— 編號必須連續，是因為欄位編號就是它在傳輸時的身份識別：跳號會浪費空間，而重複使用已發布的編號，會讓舊客戶端的資料被塞進新欄位，悄悄造成資料錯亂。型別遵循「夠用、不溢出」：大 ID 用 `int64`，小 ID 用 `int32` 省流量。

### 第 3 步 —— 讓它們對話：請求 / 回應 / 通知

現在定義客戶端和伺服器怎麼互動。一共有三種訊息角色，靠名稱前綴區分：

| 前綴 | 誰發起 | 大白話 |
|------|--------|--------|
| `Req<Name>` | 客戶端 | 「我問你個事」 |
| `Resp<Name>` | 伺服器回答 | 「這是答案」（名稱和請求一致） |
| `Notify<Name>` | 伺服器推送 | 「注意——有變化」（沒有對應的請求） |

```protobuf
message ReqMailList { ... }        // 客戶端要郵件列表
message RespMailList { ... }       // 伺服器返回列表——注意名稱是對上的
message NotifyMailChanged { ... }  // 伺服器主動推送郵件變化
message MailInfo { ... }           // 一個可重複使用的資料區塊，上面幾個都會用到
```

規則：

- 每個請求都要有一個同名的回應：`ReqMailList` ↔ `RespMailList`。
- `Notify` 只用於伺服器主動推送。
- 把共用資料抽成 `<Name>Info`，定義一次、到處重用。

**為什麼** —— 強制 Req/Resp 配對，保證每個問題都有答案；同名讓人和程式碼產生器都能一眼看出誰和誰是一對。`<Name>Info` 避免在多個訊息裡重複定義同樣的結構。

### 第 4 步 —— 用列舉表示狀態

**列舉（enum）** 是一道多選題——比如訂單狀態只能是「待付款 / 已付款 / 已出貨」，不能是別的。

```protobuf
enum RoomStatus {
  None = 0;     // 無狀態 / 無效
  Waiting = 1;  // 等待開始
  Playing = 3;  // 遊戲進行中
}
```

規則：

- 列舉名和列舉值都用 PascalCase。
- 第一個值永遠是 `0`，留給預設 / 無狀態（`None`、`Unknown`）。

**為什麼** —— proto3 強制第一個值必須是 `0`。把它定為 `None` / `Unknown` 作為安全預設值：沒賦值的欄位讀出來是「無狀態」，而不是誤命中某個真實狀態——這樣能避免一整類 bug。

### 第 5 步 —— 定義錯誤碼

出錯時給它一個編號，雙方就能準確知道到底哪裡錯了。錯誤碼分兩層：

**通用錯誤碼** —— 各模組都會遇到的常見失敗（參數錯誤、消耗不足、不存在）。它們放在 `_0020_Common.proto` 的 `OperationStatusCode` 裡，從 `0` 往上編號。

**業務錯誤碼** —— 你這個模組特有的失敗。編號按公式算：**`模組 ID × 1000 + 三位序號`**。

```protobuf
// 郵件是模組 500，所以它的錯誤碼從 500001 開始
// 500001 = 500 × 1000 + 1
enum MailErrorCode {
  MailNotFound = 500001;        // 郵件不存在
  MailAlreadyDeleted = 500002;  // 郵件已被刪除
}
```

規則：客戶端把錯誤碼當作普通 `int` 接收。成功時不賦值——proto3 的預設 `0` 就代表「成功」，所以大多數情況什麼都不用傳。

**為什麼** —— 這個公式讓編號自帶身份：`500001` 一看就是郵件模組的，全域唯一不用協調，每個模組還預留了 1000 個號位可以擴充。成功當「什麼都不傳」，是因為成功佔大多數，省下的流量很可觀。

### 第 6 步 —— 寫註釋

註釋是雙方共用的唯一文件——`.proto` 檔案沒有上下文，不寫註釋，另一端只能靠猜。

- 訊息前面：寫它的用途。
- 欄位或列舉值後面：寫它代表什麼。
- 如果一個 `int` 欄位實際裝的是列舉值，用括號標出列舉名，比如 `// 狀態（RoomStatus）`，讓讀者知道合法值去哪查。

**為什麼** —— 光一個 `int` 看不出它有哪些合法取值；標出列舉名，讀者就能直接找到答案。

### 完整範例

以虛構的 `_0600_Quest`（任務系統）模組為例，涵蓋上述所有規則：

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

## 協議要求

`ProtoExport` 工具強制執行的硬性規則。權威來源：[GameFrameX.Tools README](https://github.com/GameFrameX/GameFrameX.Tools#readme)。

### 檔案格式

```protobuf
syntax = "proto3";     // 必填：僅支援 proto3
package Basic;
option module = 10;    // 必填：必須定義模組 ID
```

### 訊息命名

- **請求**：`Req<Name>`（如 `ReqLogin`、`ReqHeartBeat`）
- **回應**：`Resp<Name>`（如 `RespLogin`）
- **通知**：`Notify<Name>`（如 `NotifyBagInfoChanged`）
- 所有訊息、欄位、列舉名稱與列舉值必須使用 **UpperCamelCase**。

### 模組 ID

| ID 範圍 | 用途 |
|---------|------|
| `0` ~ `32767` | 客戶端 ↔ 伺服器 |
| `-32768` ~ `-1` | 伺服器 ↔ 伺服器（內部） |

### 欄位編號

- 訊息欄位編號必須**小於 800**（`>= 800` 的值為系統保留，會導致解析錯誤）。
- `ErrorCode` 是 `Resp` 訊息中的**保留欄位名**——不要手動定義。工具會在每個 `Resp` 上自動生成 `ErrorCode` 欄位。

### 限制

- **禁止巢狀型別** —— 不能在另一個 `message` 內部宣告 `message` / `enum`。
- **禁止 RPC 定義** —— 不支援 `service` 區塊。
- **僅支援 proto3** —— 必須使用 `syntax = "proto3";`；不支援 proto2。

### 註釋標準

- 每個 `message` / `enum` **上方**必須有一行註釋描述其用途。
- 每個欄位 / 列舉值行尾必須有**行內**註釋。

### 僅伺服器檔案

匯出工具透過**檔名後綴** `-s` 或 `_s`（如 `player-s.proto`、`economy_s.proto`）識別僅伺服器的 proto 檔案。傳入 `--isServer true` 才會包含它們；預設 `--isServer false` 時會跳過，因此僅伺服器訊息永遠不會洩露給客戶端。

內部協議額外以**負模組 ID** 做路由隔離（見上方的模組 ID 表）。

> **關於當前倉庫的說明：** 這裡的內部檔案使用 `Inner_` 前綴加負模組 ID（如 `_-0120_Inner_Social.proto`）。`-s`/`_s` 後綴與負 ID 約定都能實現僅伺服器路由——擇一使用，並在同一模組內保持一致。

## 支援的匯出語言

| 語言 | 模式與旗標 | 本地腳本 | Docker |
|------|-----------|---------|--------|
| C#（伺服器） | `csharp --isServer true` | `Proto2CsExport_Server.sh` / `.bat` | 是 |
| C#（客戶端 / Unity / Godot） | `csharp` | `Proto2CsExport_Client.sh` / `.bat` | 是 |
| C++ | `cpp` | `Proto2CppExport.sh` / `.bat` | 是 |
| Go | `go` | `Proto2GoExport.sh` / `.bat` | 是 |
| Lua | `lua` | `Proto2LuaExport.sh` / `.bat` | 是 |
| TypeScript | `typescript` | `Proto2TsExport.sh` / `.bat` | 是 |
| TypeScript (LayaBox) | `typescript` | `Proto2TsExport_LayaBox.sh` | 是 |

### Docker 範例

**C#（伺服器）：**

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

路徑對應：`-v <host>:<container>` 掛載主機目錄；`--inputPath` / `--outputPath` 必須引用**容器內**路徑（`/protos`、`/output`），而非主機路徑。

## 匯出參數

### 核心

| 參數 | 必填 | 預設 | 說明 |
|------|------|------|------|
| `--mode` | 是 | - | `csharp` / `typescript` / `cpp` / `lua` / `go` |
| `--inputPath` | 是 | - | 存放 `.proto` 檔案的目錄 |
| `--outputPath` | 是 | - | 生成檔案的輸出目錄 |
| `--namespaceName` | 否 | `""` | C# 命名空間（Go 套件最後一段，以點分隔） |
| `--isGenerateErrorCode` | 否 | `true` | 在 `Resp` 訊息上自動生成 `ErrorCode` 欄位 |
| `--requireComments` | 否 | `none` | 註釋驗證級別：`none` / `container` / `member` / `all` |

### C#

| 參數 | 預設 | 說明 |
|------|------|------|
| `--usingStatements` | `""` | using 語句，以 `\|` 分隔（如 `"using System\|using ProtoBuf"`） |
| `--isGenerateDescription` | `false` | 生成 `[System.ComponentModel.Description]` 特性 |
| `--isServer` | `false` | 包含僅伺服器 proto 檔案（檔名以 `-s` 或 `_s` 結尾） |

### TypeScript

| 參數 | 預設 | 說明 |
|------|------|------|
| `--importPath` | `"../network/"` | 生成 import 語句的前綴路徑 |
| `--isGenerateDescription` | `false` | 生成 JSDoc 風格註釋 |

### 舊版

| 參數 | 預設 | 說明 |
|------|------|------|
| `--isGenerateErrorCodeExcelFile` | `true` | 生成錯誤碼 Excel 檔案 |
| `--errorCodeExcelFilePath` | `""` | 錯誤碼 Excel 檔案的自訂路徑 |

## Docker

預建映像檔支援 `linux/amd64` 與 `linux/arm64`：

```bash
# Docker Hub
docker pull gameframex/gameframex-tools:latest

# GitHub Container Registry (GHCR)
docker pull ghcr.io/gameframex/gameframex.tools:latest
```

映像檔的 entrypoint 就是 `ProtoExport` 工具——直接在映像檔名後面附加參數即可：

```bash
docker run --rm \
  -v /path/to/protos:/protos \
  -v /path/to/output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --inputPath /protos --outputPath /output
```

## CI 流水線

本倉庫附帶 [`.github/workflows/proto-export.yml`](.github/workflows/proto-export.yml)。它會在**每次 `push`** 以及手動觸發時自動執行。

| 步驟 | 內容 |
|------|------|
| 1 | 拉取 `gameframex/gameframex-tools:latest` |
| 2 | 將 `.proto` 來源掛載到容器的 `/protos` |
| 3 | 以建置矩陣並行匯出全部六種目標語言 |
| 4 | 將每種語言的輸出收集為 workflow artifact |
| 5 | 在 `push` 到 `main` 時，（重新）發布滾動更新的 **`latest` Release**，並附帶所有 artifact |

從 [Releases 頁面](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) 下載最新的生成程式碼——無需任何工具鏈。

## 匯出工具

本倉庫的程式碼生成由獨立的 [GameFrameX.Tools](https://github.com/GameFrameX/GameFrameX.Tools) 倉庫中的 `ProtoExport` 工具驅動（一個 .NET 10 主控台程式）。**`Tools/` 目錄內建該工具的二進位產物，由流水線每週自動同步**——clone 後即可直接執行本地腳本，無需自行建置（見[快速開始](#快速開始)）：

- **CI** —— 零設定，直接從最新 Release 下載產生的程式碼。
- **Docker** —— 執行預建映像檔，無需本地工具鏈。
- **本地腳本** —— 直接使用 `Tools/` 下每週自動同步的產物；需要立即更新時，手動觸發同步流水線或自行建置覆蓋（見下文）。

### 工具倉庫

| 專案 | 倉庫位址 | 說明 |
|------|----------|------|
| GameFrameX.Tools | https://github.com/GameFrameX/GameFrameX.Tools | `ProtoExport` 產生器原始碼、完整參數文件、Docker 映像檔 |

`ProtoExport` 是一個 .NET 10 主控台專案（`ProtoExport.csproj`，`OutputType=Exe`），依賴 NuGet 套件 `GameFrameX.Foundation.Options` 做命令列參數解析。

### 環境需求

- **.NET 10 SDK** —— 執行匯出腳本需要它（腳本透過 `dotnet` 啟動工具）；自行建置工具時同樣需要。
- 驗證：`dotnet --version` 應輸出 `10.x.x`。

### 自動同步（預設）

`Tools/` 產物由 **Tools Sync** 流水線（`.github/workflows/tools-sync.yml`）維護：每週一 09:00（北京時間）自動從上游 `main` 分支建置 Release 產物，有變化才提交。需要立即同步時，在倉庫 **Actions → Tools Sync → Run workflow** 手動觸發。

### 自行建置（可選覆蓋）

上游約定 `GameFrameX.Tools` 與本倉庫克隆到同級目錄，建置產物直接輸出到本倉庫的 `Tools/`：

```bash
# 1. 與本倉庫同級克隆工具倉庫
git clone https://github.com/GameFrameX/GameFrameX.Tools.git
cd GameFrameX.Tools/ProtoExport

# 2. 建置（Release）—— csproj 的 OutputPath 固定輸出到同級 Protobuf/Tools/
dotnet build -c Release
```

### 產物清單

`Tools/` 目錄只包含以下 4 個必要檔案（自動同步與手動建置均只需這些）：

| 檔案 | 必要 | 作用 |
|------|:----:|------|
| `ProtoExport.dll` | 是 | 主程式集 |
| `ProtoExport.deps.json` | 是 | 相依描述（執行時必要） |
| `ProtoExport.runtimeconfig.json` | 是 | 執行時設定（指定 .NET 10） |
| `GameFrameX.Foundation.Options.dll` | 是 | 命令列參數解析相依 |

建置輸出中的 `ProtoExport.pdb`（除錯符號）與原生啟動器（macOS/Linux 的 `ProtoExport`、Windows 的 `ProtoExport.exe`）不會被同步——所有 `Proto2*` 腳本統一透過 `dotnet ./Tools/ProtoExport.dll` 啟動工具，跨平台一致。

### 驗證

```bash
cd /path/to/GameFrameX.Protobuf
./Proto2CsExport_Client.sh    # macOS / Linux
Proto2CsExport_Client.bat     # Windows
```

看到 `协议扫描完成: ... 导出 N 个，跳过 M 个` 即表示工具就緒。

### 與匯出腳本的關係

倉庫根目錄的每個 `Proto2*.sh` / `.bat` 腳本都會：

1. 從倉庫根目錄執行；
2. 透過 `dotnet ./Tools/ProtoExport.dll` 啟動 `Tools/` 下自動同步的產生器；
3. 傳入對應語言的參數（`--mode`、`--isServer` 等）。

因此**只要 `Tools/` 下有正確的產物，所有腳本即可直接執行**——無需關心各腳本的參數細節。

### 更新工具

`ProtoExport` 上游迭代後，**Tools Sync** 流水線會在每週同步時自動覆蓋 `Tools/` 下的舊檔案（也可手動觸發立即同步）。拉取本倉庫最新變更即可獲得最新的工具版本。

## 依賴

| 依賴 | 用途 |
|------|------|
| [GameFrameX.Tools `ProtoExport`](https://github.com/GameFrameX/GameFrameX.Tools) | 驅動全部匯出的程式碼產生器（.NET 10 主控台程式） |
| [`gameframex/gameframex-tools`](https://hub.docker.com/r/gameframex/gameframex-tools) Docker 映像檔 | 容器化匯出，無需本地工具鏈 |
| .NET 10 SDK | 僅執行本地匯出腳本時需要 |

## 文檔與資源

- [協議文檔](https://gameframex.doc.alianblank.com/protobuf/require) —— 協議規範與匯出指南
- [GameFrameX.Tools](https://github.com/GameFrameX/GameFrameX.Tools) —— `ProtoExport` 原始碼、完整參數文件、Docker 映像檔
- [Releases](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) —— 滾動發布的全語言生成程式碼包
- [匯出流水線](.github/workflows/proto-export.yml) 與 [Tools Sync 流水線](.github/workflows/tools-sync.yml)

## 社區與支援

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

## 更新日誌

見 [Releases 頁面](https://github.com/GameFrameX/GameFrameX.Protobuf/releases)——每次 `push` 到 `main` 都會重新發布滾動更新的 `latest` Release，附帶最新生成的程式碼。

## 開源協議

詳見 [LICENSE.md](LICENSE.md) 檔案。

<!--
EN: See [LICENSE.md](LICENSE.md) for license information.
zh-CN: 详见 [LICENSE.md](LICENSE.md) 文件。
zh-TW: 詳見 [LICENSE.md](LICENSE.md) 檔案。
ja: 詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
ko: 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
-->
