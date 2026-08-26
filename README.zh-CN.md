<div align="center">
    <a href="https://trendshift.io/repositories/20145" target="_blank"><img src="https://trendshift.io/api/badge/repositories/20145" alt="GameFrameX%2FGameFrameX | Trendshift" style="width: 250px; height: 55px;" width="250" height="55"/></a>
</div>

**简体中文** | [繁體中文](README.zh-TW.md) | [English](README.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

## 快速开始

任何方式下载本仓库（git clone / Code → Download ZIP / 镜像站下载）都是完整项目，无需额外拉取。

| 组件 | 版本 | 用途 |
|------|------|------|
| .NET SDK | 8.0+ | 运行服务器 `Server/`（Foundation 依赖经 NuGet 自动还原，需联网） |
| Unity | 2021.3+ | 打开客户端 `Unity/`（首次导入需联网拉取 Package） |
| Docker（可选） | - | `cd Server && docker-compose up` 一键起 MongoDB |
| LayaAir（可选） | - | 打开 `LayaBox/` 客户端 |

运行服务器：

    dotnet run --project Server/GameFrameX.Launcher --ServerType=Game --ServerId=1000 --APMPort=29090

### 子仓库索引

本仓库为**聚合发布仓**（每日自动同步下列源仓库），⚠️ 直接改动会在下次同步被覆盖，PR / Issue 请前往对应源仓库：

| 目录 | 源仓库 |
|------|--------|
| `Unity/` | GameFrameX/GameFrameX.Unity |
| `Server/` | GameFrameX/GameFrameX.Server |
| `LayaBox/` | GameFrameX/GameFrameX.LayaBox |
| `Tools/` | GameFrameX/GameFrameX.Tools |
| `Config/` | GameFrameX/GameFrameX.Config |
| `Protobuf/` | GameFrameX/GameFrameX.Protobuf |
| `FairyGUIProject/` | GameFrameX/GameFrameX.FairyGUIProject |
| （不聚合） | GameFrameX/GameFrameX.Foundation（NuGet 发包仓）· GameFrameX/GameFrameX.Docs（文档站） |

---

# 🎮 GameFrameX 是个啥？

一句话：**一套帮你把游戏「从点子 → 做出来 → 上线运营」全包圆的开源工具箱。**

做游戏，真正费劲的往往不是「画个角色、写个技能」，而是把这些零碎拼到一块儿：

- 玩家的存档存哪儿？怎么读出来？
- 多人联机的时候，服务器怎么转发消息？
- 道具、关卡、等级这些数据谁来管，策划改了怎么办？
- 上线之后，怎么看数据、怎么管玩家、怎么发新版本？

这些「脏活累活」，GameFrameX 都替你干好了，你只管专心琢磨「我的游戏好不好玩」。

支持这些主流引擎：**Unity、Cocos Creator、LayaAir（LayaBox）、Godot** —— 你用哪个，它都吃得下。

---

# 🧰 它能帮你省掉哪些麻烦？

| 你本来要自己折腾的事 | GameFrameX 直接给你准备好了 |
|---|---|
| 从零写一套联机服务器 | 现成的高性能服务器（.NET 写的，能扛多人同时在线） |
| 数据到底怎么存 | 玩家数据存 MongoDB（读写得快），后台数据存 PostgreSQL（稳） |
| Excel 配置手动搬到代码里 | 用 LuBan 一键把 Excel 变成代码和数据 |
| 客户端和服务器「对暗号」 | 用 ProtoBuf 统一协议，改一处、两端同步 |
| 上线后两眼一抹黑 | 自带管理后台网页，看数据 / 管玩家 / 发配置 |
| 部署服务器头大 | 用 Docker 一键打包部署，省心 |

> 说白了：**一个人也能像一个小团队那样，把一款联网游戏做出来、并且运营下去。**

---

# 👤 适合谁用？

- 想做**联网 / 网游**、但被「服务器怎么搞」卡住的独立开发者
- 想快速搭个**游戏原型**验证点子的小团队
- 想完整学一遍「客户端 + 服务器 + 后台」全流程的学习者

---

# 🗺️ 这堆仓库都是干嘛的？（仓库地图）

GameFrameX 是个「全家桶」，但全家桶里每道菜都装在**各自独立的仓库**里（方便单独维护、单独升级）。先看这张表建立全局印象：

| 仓库 | 通俗说就是… | 地址 |
|---|---|---|
| 🏠 **主仓库（就是这儿）** | 「厨房平面图」——告诉你所有零件该放哪个文件夹 | https://github.com/GameFrameX/GameFrameX |
| 🌐 **服务器** | 游戏的大脑，管联机、存档、战斗逻辑（基于 GeekServer 演化而来） | https://github.com/GameFrameX/GameFrameX.Server |
| 📊 **配置表（LuBan）** | 用 Excel 填游戏数据（道具 / 关卡 / 等级…），一键生成代码 | https://github.com/GameFrameX/GameFrameX.Config |
| 📡 **通讯协议（ProtoBuf）** | 客户端和服务器「说话的规矩」，定义双方互通的消息 | https://github.com/GameFrameX/GameFrameX.Protobuf |
| 🎨 **UI 工程（FairyGUI）** | 用 FairyGUI 编辑器画游戏界面的源工程 | https://github.com/GameFrameX/GameFrameX.FairyGUIProject |
| 🛠️ **工具集** | 一些辅助小工具 | https://github.com/GameFrameX/GameFrameX.Tools |
| 💻 **管理后台** | 上线后管数据、管玩家的网页（部分源码不开源） | https://github.com/GameFrameX/GameFrameX.Admin |

后台在线演示 👉 https://game.admin.web.vue.alianblank.com

## 🎮 客户端（四选一即可，用哪个下哪个）

| 引擎 | 地址 |
|---|---|
| Unity | https://github.com/GameFrameX/GameFrameX.Unity |
| Cocos Creator | https://github.com/GameFrameX/GameFrameX.CocosCreator |
| LayaAir（LayaBox） | https://github.com/GameFrameX/GameFrameX.LayaBox |
| Godot | https://github.com/GameFrameX/GameFrameX.Godot |

---

# 📁 文件夹为什么不能乱放？

> ⚠️ **重点**：这套框架是**靠相对路径**找文件的，就像家里的插座位置——你把服务器从 `Server/` 挪到 `MyServer/`，整条链路就找不着北了。

所以请按下面的结构，把各仓库**放到它该在的文件夹**里：

```
GameFrameX/                  # 项目根目录（名字可改）
├── Config/                  # ← 把 GameFrameX.Config 放这里（Excel 配置 + LuBan 导表）
├── Protobuf/                # ← 把 GameFrameX.Protobuf 放这里（通讯协议）
├── FairyGUIProject/         # ← 把 GameFrameX.FairyGUIProject 放这里（UI 编辑工程）
├── Server/                  # ← 把 GameFrameX.Server 放这里（游戏服务器）
├── Unity/                   # ← 把 GameFrameX.Unity 放这里（Unity 客户端，按需换成别的引擎）
│   ├── Assets/              #    Unity 资源目录
│   ├── Packages/            #    Unity 包
│   ├── ProjectSettings/     #    Unity 工程设置
│   └── UserSettings/        #    Unity 用户设置
├── Tools/                   # ← 把 GameFrameX.Tools 放这里（辅助工具）
├── docker/                  # Docker 本地运行环境（MongoDB / PostgreSQL）
├── Docs/                    # 文档（目前主要是 GeekServer 的原始文档）
└── LICENSE.md               # 开源许可证
```

> 想换别的客户端引擎？把 `Unity/` 换成对应名字即可（`Laya/`、`CocosCreator/`、`Godot/`），规则一样。

---

# 🔧 先把环境准备好

开始前，请先装好下面这些（点链接去官网下）：

| 要装的东西 | 版本 | 干啥用 | 哪里下 |
|---|---|---|---|
| **Git** | 任意新版 | 拉取各个仓库的代码 | https://git-scm.com/ |
| **.NET SDK** | **10.0 或以上** | 编译运行服务器、跑 LuBan 导表工具 | https://dotnet.microsoft.com/download |
| **Unity 编辑器** | **2019.4.40f1**（兼容 2019.4+） | 打开、运行 Unity 客户端 | https://unity.com/download |
| **Docker**（可选但推荐） | 任意新版 | 一键启动本地数据库 MongoDB / PostgreSQL | https://www.docker.com/ |

> 💡 服务器和导表工具都依赖 **.NET 10.0**，这是最关键的版本要求，一定装对。

---

# 🚀 从零开始，手把手跑起来

**第 1 步**：新建一个文件夹放项目，打开终端（Windows 用 cmd / PowerShell，Mac / Linux 用终端），`cd` 进去。

**第 2 步**：把「厨房平面图」下下来：

```shell
git clone https://github.com/GameFrameX/GameFrameX.git
```

这会生成一个 `GameFrameX/` 文件夹，里面就是项目骨架。

**第 3 步**：把各零件放到 `GameFrameX/` 里**对应的文件夹**（下面以 Unity 为例；用别的引擎就把最后一行换成对应地址）：

```shell
git clone https://github.com/GameFrameX/GameFrameX.Server.git ./GameFrameX/Server
git clone https://github.com/GameFrameX/GameFrameX.Config.git ./GameFrameX/Config
git clone https://github.com/GameFrameX/GameFrameX.Protobuf.git ./GameFrameX/Protobuf
git clone https://github.com/GameFrameX/GameFrameX.FairyGUIProject.git ./GameFrameX/FairyGUIProject
git clone https://github.com/GameFrameX/GameFrameX.Tools.git ./GameFrameX/Tools
git clone https://github.com/GameFrameX/GameFrameX.Unity.git ./GameFrameX/Unity
```

> 这几行的意思就是「把 XX 仓库的内容，下到 XX 文件夹里」。**文件夹名千万别改**。

**第 4 步（启动本地数据库）**：装了 Docker 的话，分别进两个目录把 MongoDB 和 PostgreSQL 起起来（服务器连 MongoDB、后台连 PostgreSQL）：

```shell
cd GameFrameX/docker/mongo && docker compose up -d
cd ../postgres && docker compose up -d
```

启动成功后这样连：
- MongoDB：`mongodb://admin:admin@localhost:27017`
- PostgreSQL：`localhost:5432`，账号 `postgres` / 密码 `postgres`，初始库 `gameframex`

> ⚠️ 以上账号密码是本地开发默认值，要和 `Server` / `Admin` 里的连接配置对齐才能连上。

**第 5 步（生成配置代码）**：进 `Config/` 目录，跑里面的 LuBan 导表脚本，把 Excel 变成客户端和服务器都能用的代码与数据。具体命令看 👉 [`GameFrameX.Config`](https://github.com/GameFrameX/GameFrameX.Config) 的说明。

**第 6 步（生成协议代码）**：进 `Protobuf/` 目录，跑协议导出脚本，生成各端收发消息用的代码。具体命令看 👉 [`GameFrameX.Protobuf`](https://github.com/GameFrameX/GameFrameX.Protobuf) 的说明。

**第 7 步（可选）**：需要的话打开 `Tools/` 编译一下辅助工具，看 👉 [`GameFrameX.Tools`](https://github.com/GameFrameX/GameFrameX.Tools) 的说明。

**第 8 步（开跑！）**：用 Unity 打开 `Unity/` 工程，启动 `Server/` 里的服务器，就能跑起来体验了 🎉

---

# 💬 交流 & 反馈（建议、需求、BUG）

QQ 群：**467608841**

# 📖 文档（真在写了，别催 😅）

> 所有站点内容一致，挑一个能打开的用就行。

- 主站：https://gameframex.doc.alianblank.com
- 备用 1：https://gameframex-docs.pages.dev
- 备用 2：https://gameframex.doc.cloudflare.alianblank.com
- 备用 3：https://gameframex.doc.vercel.alianblank.com

---

# ☕ 请作者喝杯咖啡

![wechat.jpg](Docs/imgs/wechat.jpg)

# 🎯 谁在用 GameFrameX？

| 游戏名称 | 上线渠道 | 上线时间 |
|:---|:---|:---|
| 深夜的烧烤店 | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |
| 连续黑白 | 抖音、快手、支付宝、鸿蒙、TapTap、iOS 等 | 2024-11 |

> 用 GameFrameX 做出上线作品了？欢迎提 PR 或 issue 补充到上表，一起把名单做大 🙌

# 👥 贡献名单

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

# 📜 免责声明

所有插件均来自互联网，使用时请自行付费。如有侵权请发 email，本人会移除，谢谢。

该项目不得用于当地法律不允许的范围。技术本无罪，错的是滥用技术的人。

# 💎 赞助商

[AITKPARTY](https://aitkparty.com/) 是一个 AI 大模型 API 中转聚合服务，基于开源项目 New API 搭建，提供统一接口让开发者便捷地访问主流大语言模型，省去自行对接多家模型供应商的麻烦。
