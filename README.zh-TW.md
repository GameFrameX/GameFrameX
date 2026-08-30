<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/documentation-docs-blue.svg)](https://gameframex.doc.alianblank.com)
[![Trendshift](https://trendshift.io/api/badge/repositories/20145)](https://trendshift.io/repositories/20145)

獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使

<br />

[文檔](https://gameframex.doc.alianblank.com) · [快速開始](#快速開始) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 項目簡介

**一套幫你把遊戲「從點子 → 做出來 → 上線營運」全包辦的開源工具箱。**

做遊戲，真正費勁的往往不是「畫個角色、寫個技能」，而是把這些零碎拼到一塊兒：

- 玩家的存檔存哪兒？怎麼讀出來？
- 多人連線時，伺服器怎麼轉發訊息？
- 道具、關卡、等級這些資料誰來管，策劃改了怎麼辦？
- 上線之後，怎麼看資料、怎麼管玩家、怎麼發新版本？

這些「髒活累活」，GameFrameX 都替你幹好了，你只管專心琢磨「我的遊戲好不好玩」。

### 功能特性

| 你本來要自己折騰的事 | GameFrameX 直接給你準備好了 |
|---|---|
| 從零寫一套連線伺服器 | 現成的高效能伺服器（.NET 寫的，能扛多人同時在線） |
| 資料到底怎麼存 | 玩家資料存 MongoDB（讀寫得快），後台資料存 PostgreSQL（穩） |
| Excel 配置手動搬到程式裡 | 用 LuBan 一鍵把 Excel 變成程式碼和資料 |
| 用戶端和伺服器「對暗號」 | 用 ProtoBuf 統一協定，改一處、兩端同步 |
| 上線後兩眼一抹黑 | 自帶管理後台網頁，看資料 / 管玩家 / 發配置 |
| 部署伺服器頭大 | 用 Docker 一鍵打包部署，省心 |

> 說白了：**一個人也能像一個小團隊那樣，把一款連網遊戲做出來、並且營運下去。**

**適合誰用：**

- 想做**連線 / 網遊**、但被「伺服器怎麼搞」卡住的獨立開發者
- 想快速搭個**遊戲原型**驗證點子的小團隊
- 想完整學一遍「用戶端 + 伺服器 + 後台」全流程的學習者

### 案例展示

| 遊戲名稱 | 上線渠道 | 上線時間 |
|:---|:---|:---|
| 深夜的燒烤店 | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |
| 連續黑白 | 抖音、快手、支付寶、鴻蒙、TapTap、iOS 等 | 2024-11 |

> 用 GameFrameX 做出上線作品了？歡迎提 PR 或 issue 補充到上表。

## 快速開始

**本倉庫是完整專案**：git clone、Code → Download ZIP、鏡像站下載，任何方式拿到的都直接能跑，無需再拉取別的倉庫。

三步跑通（細節見下方[安裝教學](#安裝)）：

```shell
# 1. 啟動本地資料庫（MongoDB，帳號 admin / admin）
cd docker/mongo && docker compose up -d

# 2. 編譯並啟動伺服器（只覆蓋資料庫連線，其餘連接埠走預設值）
cd ../../Server && dotnet build
cd bin/app_debug
dotnet GameFrameX.Launcher.dll --DataBaseUrl="mongodb://admin:admin@localhost:27017/?authSource=admin"

# 3. 用 Unity 2019.4.40f1 開啟 Unity/ 工程，開啟 Assets/Scenes/Launcher.unity，點 Play
```

看到登入介面、能建立角色進入主城，就表示用戶端↔伺服器全鏈路通了。

伺服器起了沒？檢查監聽連接埠：`nc -z localhost 29100`（TCP）和 `nc -z localhost 28080`（HTTP）——成功即在線。（29090 是效能指標連接埠，**預設關閉**——見下方連接埠表。）

### 安裝

跟著做，大概 10~15 分鐘（含 Unity 首次匯入時間）。

#### 第 1 步：下載專案

```shell
git clone https://github.com/GameFrameX/GameFrameX.git
cd GameFrameX
```

不想用 git？GitHub 頁面點 **Code → Download ZIP**，或從 [gitee 等鏡像站](https://gitee.com/GameFrameX/GameFrameX) 下載，效果一樣。

#### 第 2 步：裝環境

| 要裝 | 版本 | 哪裡下 |
|---|---|---|
| **.NET SDK** | **10.0 或以上** | https://dotnet.microsoft.com/download |
| **Unity 編輯器** | **2019.4.40f1**（Unity Hub → Installs → Install Editor → Archive 裡找） | https://unity.com/download |
| **Docker Desktop** | 任意新版 | https://www.docker.com/ |

> **注意**：.NET 10 是伺服器和導表工具的硬性要求，裝錯版本後面全卡。

#### 第 3 步：啟動本地資料庫

```shell
cd docker/mongo
docker compose up -d
```

起來的是 MongoDB：`mongodb://admin:admin@localhost:27017`（資料落在 `docker/mongo/database/`）。

> PostgreSQL（`docker/postgres/`）是給管理後台 Admin 用的，跑本教學用不到，可不啟動。

#### 第 4 步：編譯並啟動伺服器

```shell
cd ../../Server
dotnet build
cd bin/app_debug
dotnet GameFrameX.Launcher.dll --DataBaseUrl="mongodb://admin:admin@localhost:27017/?authSource=admin"
```

**為什麼只傳一個參數？** 伺服器預設配置（見 `Server/GameFrameX.Launcher/StartUp/AppStartUpGame.cs`）已經開好了全套連接埠：

| 連接埠 | 用途 | 預設 |
|---|---|---|
| 29100 | TCP：遊戲用戶端長連線 | 預設開啟 |
| 28080 | HTTP：登入等介面（`/game/api/...`） | 預設開啟 |
| 29110 | WebSocket | 預設關閉，需 `--IsEnableWebSocket true` 開啟 |
| 29090 | 效能指標 / 健康檢查 | 預設關閉，需 `--IsOpenTelemetryMetrics true --MetricsPort 29090` 開啟 |

唯一要覆蓋的是 `DataBaseUrl`——預設值連的是演示用公網庫，本地開發請指到自己剛起的 MongoDB。

**用 IDE 更簡單**：用 Rider / Visual Studio 開啟 `Server/Server.slnx`（不支援 `.slnx` 就開 `Server.sln`），啟動專案選 `GameFrameX.Launcher`，**Working directory 設為 `Server/bin/app_debug`**，命令列參數留空——但要在 `AppStartUpGame.cs` 裡把 `DataBaseUrl` 預設值改成本地連線字串（改的是聚合倉內的檔案，僅本地除錯用，見上方同步覆蓋說明）。

**驗證**：終端執行 `nc -z localhost 29100 && nc -z localhost 28080`——成功即在線（或看伺服器日誌中的 `has been started` / `Now listening on`）。

#### 第 5 步：Unity 用戶端連線

1. Unity Hub 用 **2019.4.40f1** 開啟倉庫裡的 `Unity/` 資料夾（首次開啟會自動拉取 Package，需要連網，耐心等）
2. 開啟場景 `Assets/Scenes/Launcher.unity`
3. 點 **Play**

用戶端預設連 `127.0.0.1`（TCP 29100 / HTTP 28080），和伺服器預設連接埠正好對上，不需要改任何配置。看到登入介面、建立角色進主城，教學就通關了。

> 換電腦 / 部署到遠端伺服器時，改這兩處：TCP 位址在 `Unity/Assets/Hotfix/UI/Logic/UILogin/UIPlayerList.cs`（`serverIp` / `serverPort`），HTTP 位址在 `Unity/Assets/Hotfix/UI/Logic/UILogin/UILogin.cs` 等（搜 `127.0.0.1:28080`）。

#### 想用 LayaAir 用戶端？

用 LayaAir IDE 開啟 `LayaBox/`，入口 `src/Main.ts`。注意兩點：WebSocket **預設關閉**——先把伺服器用 `--IsEnableWebSocket true` 啟動（預設 WsPort 29110；`nettest.ts` 預設連 `ws://127.0.0.1:21100`，兩者**對不上**，改成一致）；連線位址在 `LayaBox/src/gameframex/nettest.ts`；協定生成用 `Protobuf/Proto2TsExport_LayaBox.sh`。

## 使用範例

下載下來的快照**自帶全部生成產物**（配置程式碼 / 資料、協定程式碼都已就位），直接就能跑。只有當你改了源頭檔案，才需要重新生成：

### 改了 Excel 配置（`Config/Excels/Tables/` 下的表）

| 你改了 | 跑哪個 | 產物去哪 |
|---|---|---|
| 伺服器端要讀的表 | `cd Config && sh gen-server-bin.sh`（Windows 雙擊 `gen-server-bin.bat`） | `Server/GameFrameX.Config/` |
| 用戶端要讀的表 | `cd Config && sh gen-client-json.sh` | `Unity/Assets/`（程式碼 + 資料） |

> 表檔名有講究：`字母-英文名-中文名.xlsx`（如 `D-ItemConfig-道具表-道具-1001.xlsx`），Excel 裡前 4 行是表頭（`##var` / `##type` / `##group` / 說明），第 5 行起才是資料。完整規則看 [GameFrameX.Config](https://github.com/GameFrameX/GameFrameX.Config)。

### 改了通訊協定（`Protobuf/*.proto`）

匯出工具不隨倉庫分發，先建置一次（聚合倉的目錄布局已滿足它的輸出路徑要求）：

```shell
cd Tools
dotnet build ProtoExport/ProtoExport.csproj -c Release   # 產物自動落到 ../Protobuf/Tools/
cd ../Protobuf
sh Proto2CsExport_Server.sh    # 伺服器端協定 → Server/GameFrameX.Proto/
sh Proto2CsExport_Client.sh    # 用戶端協定 → Unity/Assets/Hotfix/Proto/
```

> 協定有硬規則：只支援 proto3；`option module = 10;` 必填；訊息必須叫 `Req<名字>` / `Resp<名字>` / `Notify<名字>`；欄位編號必須 < 800；禁止巢狀 message。完整規則看 [GameFrameX.Protobuf](https://github.com/GameFrameX/GameFrameX.Protobuf)。

### 改了 UI（FairyGUI）

用 FairyGUI 編輯器（≥5.0）開啟 `FairyGUIProject/Game.fairy`，改完 **檔案 → 發布，務必勾選「生成程式碼」**，產物自動寫入 `Unity/Assets/`（UI 資源 + C# 綁定程式碼）。

> 新人最常見問題：發布完 Unity 裡報找不到類 → 十有八九是發布時沒勾「生成程式碼」。

### 常見坑

| 現象 | 原因 & 解法 |
|---|---|
| 伺服器啟動報連不上資料庫 | `DataBaseUrl` 沒傳，預設連的是公網演示庫；傳第 4 步那條本地連線字串 |
| IDE 裡啟動就閃退 / 找不到 hotfix | Working directory 沒設成 `Server/bin/app_debug`（伺服器從「當前目錄/hotfix」載入熱更程式集） |
| Unity 首次開啟卡在拉包 | 需要連網存取 UPM 私有源（`gameframex.upm.alianblank.uk`）和 gitee（HybridCLR），網路受限會卡住 |
| 用戶端連不上伺服器 | 確認連接埠組對得上：TCP 29100 / HTTP 28080；WebSocket 29110 需 `--IsEnableWebSocket true`（預設關閉）；伺服器日誌裡有監聽列表 |
| 在本倉庫改了程式碼，第二天沒了 | 聚合倉每日同步會覆蓋，改動請提交到對應源倉庫 |
| LayaBox 連不上 | WebSocket 預設關閉——先把伺服器用 `--IsEnableWebSocket true` 啟動；另外把 `nettest.ts`（預設 21100）改成與伺服器 WsPort 29110 一致 |

## 架構概覽

本倉庫是**聚合發布倉**——每天自動把下面 7 個源倉庫的最新程式碼同步到同名資料夾裡，所以你下載一次就拿到所有零件，而且**資料夾天生就在正確位置**（配置生成、協定匯出都靠相對路徑互相找到對方，別改名、別挪位置）：

```
GameFrameX/                   # 專案根目錄
├── Server/                   # 遊戲伺服器（.NET 10，Actor 模型 + 熱更新）
├── Unity/                    # Unity 用戶端工程（含 HybridCLR 熱更、YooAsset 資源）
├── LayaBox/                  # LayaAir 用戶端工程（可選用戶端）
├── Config/                   # LuBan 配置表：Excel 在這改，一鍵生成兩端程式碼
├── Protobuf/                 # 通訊協定：.proto 在這改，一鍵匯出各端程式碼
├── FairyGUIProject/          # UI 編輯工程（FairyGUI 編輯器開啟 Game.fairy）
├── Tools/                    # 輔助工具（協定匯出 CLI / GUI）
├── docker/                   # 本地資料庫一鍵啟動（mongo / postgres）
├── scripts/                  # 聚合同步腳本
└── README / LICENSE 等
```

| 目錄 | 對應源倉庫（改動請去這裡提 PR / Issue） |
|------|------|
| `Server/` | https://github.com/GameFrameX/GameFrameX.Server |
| `Unity/` | https://github.com/GameFrameX/GameFrameX.Unity |
| `LayaBox/` | https://github.com/GameFrameX/GameFrameX.LayaBox |
| `Config/` | https://github.com/GameFrameX/GameFrameX.Config |
| `Protobuf/` | https://github.com/GameFrameX/GameFrameX.Protobuf |
| `FairyGUIProject/` | https://github.com/GameFrameX/GameFrameX.FairyGUIProject |
| `Tools/` | https://github.com/GameFrameX/GameFrameX.Tools |

> **注意**：**直接改本倉庫裡的 `Server/`、`Unity/` 等目錄是沒用的**——每天自動同步會把改動覆蓋掉。要改程式碼、交 PR，請去上表對應的源倉庫。

**不聚合的倉庫**（按需自取）：

| 倉庫 | 說明 |
|------|------|
| [GameFrameX.Foundation](https://github.com/GameFrameX/GameFrameX.Foundation) | 伺服器底層庫，以 NuGet 包形式被 Server 引用（建置時自動還原，無需 clone） |
| [GameFrameX.Admin](https://github.com/GameFrameX/GameFrameX.Admin) | 管理後台（部分原始碼不開源），[線上演示](https://game.admin.web.vue.alianblank.com) |
| [GameFrameX.CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) / [Godot](https://github.com/GameFrameX/GameFrameX.Godot) | 其他引擎用戶端 |
| [GameFrameX.Docs](https://github.com/GameFrameX/GameFrameX.Docs) | 文件站原始碼 |

## 平台支援

支援這些主流引擎——你用哪個，它都吃得下：

| 平台 | 客戶端工程 | 說明 |
|---|---|---|
| Unity | `Unity/` 本倉庫內 | **2019.4.40f1**，HybridCLR 熱更新、YooAsset，主要用戶端 |
| LayaAir（LayaBox） | `LayaBox/` 本倉庫內 | 備用用戶端，入口 `src/Main.ts` |
| Cocos Creator | [GameFrameX.CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) | 獨立倉庫 |
| Godot | [GameFrameX.Godot](https://github.com/GameFrameX/GameFrameX.Godot) | 獨立倉庫 |
| 伺服器 | `Server/` 本倉庫內 | .NET 10，Actor 模型，所有用戶端共用 |

## 依賴

| 元件 | 版本 | 用途 |
|------|------|------|
| **.NET SDK** | **10.0+** | 編譯執行伺服器（Foundation 依賴經 NuGet 自動還原，首次需連網） |
| **Unity** | **2019.4.40f1** | 開啟用戶端 `Unity/`（首次匯入需連網拉取 Package） |
| **Docker** | 任意新版 | 一鍵啟動本地 MongoDB |

## 文檔與資源

> 所有站點內容一致，挑一個能開啟的用就行。

- 主站：https://gameframex.doc.alianblank.com
- 備用 1：https://gameframex-docs.pages.dev
- 備用 2：https://gameframex.doc.cloudflare.alianblank.com
- 備用 3：https://gameframex.doc.vercel.alianblank.com

## 社區與支援

- QQ 群：**467608841 / 233608841**
- [Bilibili](https://www.bilibili.com/video/BV1yrpeepEn7)
- [Gitee](https://gitee.com/GameFrameX/gameframex)
- [Discord](https://discord.gg/VDWUjWMDw9)
- [GitHub](https://github.com/GameFrameX/gameframex)
- [LinkedIn](https://www.linkedin.com/in/alianblank)
- [Reddit](https://www.reddit.com/r/GameFrameX/)
- [X](https://x.com/alian_blank)
- [YouTube](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
- [Bluesky](https://bsky.app/profile/alianblank.bsky.social)

### 贊助

![wechat.jpg](https://raw.githubusercontent.com/GameFrameX/GameFrameX/42e755df/Docs/imgs/wechat.jpg)

[AITKPARTY](https://aitkparty.com/) 是一個 AI 大模型 API 中轉聚合服務，基於開源專案 New API 搭建，提供統一介面讓開發者便捷地存取主流大型語言模型，省去自行對接多家模型供應商的麻煩。

### 貢獻名單

<!-- readme: contributors -start -->
<table>
	<tbody>
		<tr>
            <td align="center">
                <a href="https://github.com/AlianBlank">
                    <img src="https://avatars.githubusercontent.com/u/1950044?v=4" width="100;" alt="AlianBlank"/>
                    <br />
                    <sub><b>Blank</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/blankalian">
                    <img src="https://avatars.githubusercontent.com/u/147848600?v=4" width="100;" alt="blankalian"/>
                    <br />
                    <sub><b>blankalian</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/bambom">
                    <img src="https://avatars.githubusercontent.com/u/11567449?v=4" width="100;" alt="bambom"/>
                    <br />
                    <sub><b>bambom</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/PlayerYF">
                    <img src="https://avatars.githubusercontent.com/u/56374327?v=4" width="100;" alt="PlayerYF"/>
                    <br />
                    <sub><b>PlayerYF</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/baiwanziaaa">
                    <img src="https://avatars.githubusercontent.com/u/56676921?v=4" width="100;" alt="baiwanziaaa"/>
                    <br />
                    <sub><b>Pilipala</b></sub>
                </a>
            </td>
		</tr>
	<tbody>
</table>
<!-- readme: contributors -end -->

### Star History

[![Star History Chart](https://star-history.dera.page/svg?repos=GameFrameX/GameFrameX,GameFrameX/GameFrameX.Unity,GameFrameX/GameFrameX.Server&type=date)](https://star-history.dera.page/#GameFrameX/GameFrameX&GameFrameX/GameFrameX.Unity&GameFrameX/GameFrameX.Server&type=date&legend=top-left)

## 更新日誌

詳見 [GitHub Releases](https://github.com/GameFrameX/GameFrameX/releases) 頁面。

## 開源協議

詳見 [LICENSE.md](LICENSE.md)。

> 所有插件均來自網際網路，使用時請自行付費。如有侵權請發 email，本人會移除，謝謝。
>
> 該專案不得用於當地法律不允許的範圍。技術本無罪，錯的是濫用技術的人。
