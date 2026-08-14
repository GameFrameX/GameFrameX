<div align="center">
    <a href="https://trendshift.io/repositories/7536" target="_blank"><img src="https://trendshift.io/api/badge/repositories/7536" alt="GameFrameX%2FGameFrameX | Trendshift" style="width: 250px; height: 55px;" width="250" height="55"/></a>
</div>

[简体中文](README.zh-CN.md) | **繁體中文** | [English](README.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

# 🎮 GameFrameX 是個啥？

一句話：**一套幫你把遊戲「從點子 → 做出來 → 上線營運」全包辦的開源工具箱。**

做遊戲，真正費勁的往往不是「畫個角色、寫個技能」，而是把這些零碎拼到一塊兒：

- 玩家的存檔存哪兒？怎麼讀出來？
- 多人連線時，伺服器怎麼轉發訊息？
- 道具、關卡、等級這些資料誰來管，策劃改了怎麼辦？
- 上線之後，怎麼看資料、怎麼管玩家、怎麼發新版本？

這些「髒活累活」，GameFrameX 都替你幹好了，你只管專心琢磨「我的遊戲好不好玩」。

支援這些主流引擎：**Unity、Cocos Creator、LayaAir（LayaBox）、Godot** —— 你用哪個，它都吃得下。

---

# 🧰 它能幫你省掉哪些麻煩？

| 你本來要自己折騰的事 | GameFrameX 直接給你準備好了 |
|---|---|
| 從零寫一套連線伺服器 | 現成的高效能伺服器（.NET 寫的，能扛多人同時在線） |
| 資料到底怎麼存 | 玩家資料存 MongoDB（讀寫得快），後台資料存 PostgreSQL（穩） |
| Excel 配置手動搬到程式裡 | 用 LuBan 一鍵把 Excel 變成程式碼和資料 |
| 客戶端和伺服器「對暗號」 | 用 ProtoBuf 統一協定，改一處、兩端同步 |
| 上線後兩眼一抹黑 | 自帶管理後台網頁，看資料 / 管玩家 / 發配置 |
| 部署伺服器頭大 | 用 Docker 一鍵打包部署，省心 |

> 說白了：**一個人也能像一個小團隊那樣，把一款連網遊戲做出來、並且營運下去。**

---

# 👤 適合誰用？

- 想做**連網 / 網遊**、但被「伺服器怎麼搞」卡住的獨立開發者
- 想快速搭個**遊戲原型**驗證點子的小團隊
- 想完整學一遍「客戶端 + 伺服器 + 後台」全流程的學習者

---

# 🗺️ 這堆倉庫都是幹嘛的？（倉庫地圖）

GameFrameX 是個「全家桶」，但全家桶裡每道菜都裝在**各自獨立的倉庫**裡（方便單獨維護、單獨升級）。先看這張表建立全域印象：

| 倉庫 | 通俗說就是… | 地址 |
|---|---|---|
| 🏠 **主倉庫（就是這兒）** | 「廚房平面圖」——告訴你所有零件該放哪個資料夾 | https://github.com/GameFrameX/GameFrameX |
| 🌐 **伺服器** | 遊戲的大腦，管連線、存檔、戰鬥邏輯（基於 GeekServer 演化而來） | https://github.com/GameFrameX/GameFrameX.Server |
| 📊 **配置表（LuBan）** | 用 Excel 填遊戲資料（道具 / 關卡 / 等級…），一鍵產生程式碼 | https://github.com/GameFrameX/GameFrameX.Config |
| 📡 **通訊協定（ProtoBuf）** | 客戶端和伺服器「說話的規矩」，定義雙方互通的訊息 | https://github.com/GameFrameX/GameFrameX.Protobuf |
| 🎨 **UI 工程（FairyGUI）** | 用 FairyGUI 編輯器畫遊戲介面的源工程 | https://github.com/GameFrameX/GameFrameX.FairyGUIProject |
| 🛠️ **工具集** | 一些輔助小工具 | https://github.com/GameFrameX/GameFrameX.Tools |
| 💻 **管理後台** | 上線後管資料、管玩家的網頁（部分源碼不開源） | https://github.com/GameFrameX/GameFrameX.Admin |

後台線上示範 👉 https://game.admin.web.vue.alianblank.com

## 🎮 客戶端（四選一即可，用哪個下哪個）

| 引擎 | 地址 |
|---|---|
| Unity | https://github.com/GameFrameX/GameFrameX.Unity |
| Cocos Creator | https://github.com/GameFrameX/GameFrameX.CocosCreator |
| LayaAir（LayaBox） | https://github.com/GameFrameX/GameFrameX.LayaBox |
| Godot | https://github.com/GameFrameX/GameFrameX.Godot |

---

# 📁 資料夾為什麼不能亂放？

> ⚠️ **重點**：這套框架是**靠相對路徑**找檔案的，就像家裡的插座位置——你把伺服器從 `Server/` 挪到 `MyServer/`，整條鏈路就找不著北了。

所以請按下面的結構，把各倉庫**放到它該在的資料夾**裡：

```
GameFrameX/                  # 專案根目錄（名字可改）
├── Config/                  # ← 把 GameFrameX.Config 放這裡（Excel 配置 + LuBan 導表）
├── Protobuf/                # ← 把 GameFrameX.Protobuf 放這裡（通訊協定）
├── FairyGUIProject/         # ← 把 GameFrameX.FairyGUIProject 放這裡（UI 編輯工程）
├── Server/                  # ← 把 GameFrameX.Server 放這裡（遊戲伺服器）
├── Unity/                   # ← 把 GameFrameX.Unity 放這裡（Unity 客戶端，按需換成別的引擎）
│   ├── Assets/              #    Unity 資源目錄
│   ├── Packages/            #    Unity 包
│   ├── ProjectSettings/     #    Unity 工程設定
│   └── UserSettings/        #    Unity 使用者設定
├── Tools/                   # ← 把 GameFrameX.Tools 放這裡（輔助工具）
├── docker/                  # Docker 本地執行環境（MongoDB / PostgreSQL）
├── Docs/                    # 文件（目前主要是 GeekServer 的原始文件）
└── LICENSE.md               # 開源授權條款
```

> 想換別的客戶端引擎？把 `Unity/` 換成對應名字即可（`Laya/`、`CocosCreator/`、`Godot/`），規則一樣。

---

# 🔧 先把環境準備好

開始前，請先裝好下面這些（點連結去官網下）：

| 要裝的東西 | 版本 | 幹啥用 | 哪裡下 |
|---|---|---|---|
| **Git** | 任意新版 | 拉取各個倉庫的程式碼 | https://git-scm.com/ |
| **.NET SDK** | **10.0 或以上** | 編譯執行伺服器、跑 LuBan 導表工具 | https://dotnet.microsoft.com/download |
| **Unity 編輯器** | **2019.4.40f1**（相容 2019.4+） | 開啟、執行 Unity 客戶端 | https://unity.com/download |
| **Docker**（可選但推薦） | 任意新版 | 一鍵啟動本地資料庫 MongoDB / PostgreSQL | https://www.docker.com/ |

> 💡 伺服器和導表工具都依賴 **.NET 10.0**，這是最關鍵的版本要求，一定裝對。

---

# 🚀 從零開始，手把手跑起來

**第 1 步**：新建一個資料夾放專案，開啟終端機（Windows 用 cmd / PowerShell，Mac / Linux 用終端機），`cd` 進去。

**第 2 步**：把「廚房平面圖」下載下來：

```shell
git clone https://github.com/GameFrameX/GameFrameX.git
```

這會生成一個 `GameFrameX/` 資料夾，裡面就是專案骨架。

**第 3 步**：把各零件放到 `GameFrameX/` 裡**對應的資料夾**（下面以 Unity 為例；用別的引擎就把最後一行換成對應地址）：

```shell
git clone https://github.com/GameFrameX/GameFrameX.Server.git ./GameFrameX/Server
git clone https://github.com/GameFrameX/GameFrameX.Config.git ./GameFrameX/Config
git clone https://github.com/GameFrameX/GameFrameX.Protobuf.git ./GameFrameX/Protobuf
git clone https://github.com/GameFrameX/GameFrameX.FairyGUIProject.git ./GameFrameX/FairyGUIProject
git clone https://github.com/GameFrameX/GameFrameX.Tools.git ./GameFrameX/Tools
git clone https://github.com/GameFrameX/GameFrameX.Unity.git ./GameFrameX/Unity
```

> 這幾行的意思就是「把 XX 倉庫的內容，下到 XX 資料夾裡」。**資料夾名千萬別改**。

**第 4 步（啟動本地資料庫）**：裝了 Docker 的話，分別進兩個目錄把 MongoDB 和 PostgreSQL 起來（伺服器連 MongoDB、後台連 PostgreSQL）：

```shell
cd GameFrameX/docker/mongo && docker compose up -d
cd ../postgres && docker compose up -d
```

啟動成功後這樣連：
- MongoDB：`mongodb://admin:admin@localhost:27017`
- PostgreSQL：`localhost:5432`，帳號 `postgres` / 密碼 `postgres`，初始庫 `gameframex`

> ⚠️ 以上帳號密碼是本地開發預設值，要和 `Server` / `Admin` 裡的連線設定對齊才能連上。

**第 5 步（產生配置程式碼）**：進 `Config/` 目錄，跑裡面的 LuBan 導表腳本，把 Excel 變成客戶端和伺服器都能用的程式碼與資料。具體命令看 👉 [`GameFrameX.Config`](https://github.com/GameFrameX/GameFrameX.Config) 的說明。

**第 6 步（產生協定程式碼）**：進 `Protobuf/` 目錄，跑協定匯出腳本，產生各端收發訊息用的程式碼。具體命令看 👉 [`GameFrameX.Protobuf`](https://github.com/GameFrameX/GameFrameX.Protobuf) 的說明。

**第 7 步（可選）**：需要的話開啟 `Tools/` 編譯一下輔助工具，看 👉 [`GameFrameX.Tools`](https://github.com/GameFrameX/GameFrameX.Tools) 的說明。

**第 8 步（開跑！）**：用 Unity 開啟 `Unity/` 工程，啟動 `Server/` 裡的伺服器，就能跑起來體驗了 🎉

---

# 💬 交流 & 回饋（建議、需求、BUG）

QQ 群：**467608841**

# 📖 文件（真在寫了，別催 😅）

> 所有站點內容一致，挑一個能開啟的用就行。

- 主站：https://gameframex.doc.alianblank.com
- 備用 1：https://gameframex-docs.pages.dev
- 備用 2：https://gameframex.doc.cloudflare.alianblank.com
- 備用 3：https://gameframex.doc.vercel.alianblank.com

---

# ☕ 請作者喝杯咖啡

![wechat.jpg](Docs/imgs/wechat.jpg)

# 🎯 誰在用 GameFrameX？

| 遊戲名稱 | 上線管道 | 上線時間 |
|:---|:---|:---|
| 深夜的燒烤店 | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |
| 連續黑白 | 抖音、快手、支付寶、鴻蒙、TapTap、iOS 等 | 2024-11 |

> 用 GameFrameX 做出上線作品了？歡迎提 PR 或 issue 補充到上表，一起把名單做大 🙌

# 👥 貢獻名單

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

## Star History

[![Star History Chart](https://star-history.dera.page/svg?repos=GameFrameX/GameFrameX,GameFrameX/GameFrameX.Unity,GameFrameX/GameFrameX.Server,GameFrameX/GameFrameX.Admin&type=Date)](https://star-history.dera.page/#GameFrameX/GameFrameX&GameFrameX/GameFrameX.Unity&GameFrameX/GameFrameX.Server&GameFrameX/GameFrameX.Admin&type=Date)

# 📜 免責聲明

所有外掛均來自網際網路，使用時請自行付費。如有侵權請發 email，本人會移除，謝謝。

該專案不得用於當地法律不允許的範圍。技術本無罪，錯的是濫用技術的人。

# 💎 贊助商

[AITKPARTY](https://aitkparty.com/) 是一個 AI 大模型 API 中繼聚合服務，基於開源專案 New API 搭建，提供統一介面讓開發者便捷地存取主流大語言模型，省去自行對接多家模型供應商的麻煩。
