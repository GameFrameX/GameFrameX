<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/documentation-docs-blue.svg)](https://gameframex.doc.alianblank.com)
[![Trendshift](https://trendshift.io/api/badge/repositories/20145)](https://trendshift.io/repositories/20145)

独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#快速开始) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | **简体中文** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 项目简介

**一套帮你把游戏「从点子 → 做出来 → 上线运营」全包圆的开源工具箱。**

做游戏，真正费劲的往往不是「画个角色、写个技能」，而是把这些零碎拼到一块儿：

- 玩家的存档存哪儿？怎么读出来？
- 多人联机的时候，服务器怎么转发消息？
- 道具、关卡、等级这些数据谁来管，策划改了怎么办？
- 上线之后，怎么看数据、怎么管玩家、怎么发新版本？

这些「脏活累活」，GameFrameX 都替你干好了，你只管专心琢磨「我的游戏好不好玩」。

### 功能特性

| 你本来要自己折腾的事 | GameFrameX 直接给你准备好了 |
|---|---|
| 从零写一套联机服务器 | 现成的高性能服务器（.NET 写的，能扛多人同时在线） |
| 数据到底怎么存 | 玩家数据存 MongoDB（读写得快），后台数据存 PostgreSQL（稳） |
| Excel 配置手动搬到代码里 | 用 LuBan 一键把 Excel 变成代码和数据 |
| 客户端和服务器「对暗号」 | 用 ProtoBuf 统一协议，改一处、两端同步 |
| 上线后两眼一抹黑 | 自带管理后台网页，看数据 / 管玩家 / 发配置 |
| 部署服务器头大 | 用 Docker 一键打包部署，省心 |

> 说白了：**一个人也能像一个小团队那样，把一款联网游戏做出来、并且运营下去。**

**适合谁用：**

- 想做**联网 / 网游**、但被「服务器怎么搞」卡住的独立开发者
- 想快速搭个**游戏原型**验证点子的小团队
- 想完整学一遍「客户端 + 服务器 + 后台」全流程的学习者

### 案例展示

| 游戏名称 | 上线渠道 | 上线时间 |
|:---|:---|:---|
| 深夜的烧烤店 | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |
| 连续黑白 | 抖音、快手、支付宝、鸿蒙、TapTap、iOS 等 | 2024-11 |

> 用 GameFrameX 做出上线作品了？欢迎提 PR 或 issue 补充到上表，一起把名单做大。

## 快速开始

**本仓库是完整项目**：git clone、Code → Download ZIP、镜像站下载，任何方式拿到的都直接能跑，无需再拉取别的仓库。

三步跑通（细节见下方[教程](#安装)）：

```shell
# 1. 启动本地数据库（MongoDB，账号 admin / admin）
cd docker/mongo && docker compose up -d

# 2. 编译并启动服务器（只覆盖数据库连接，其余端口走默认值）
cd ../../Server && dotnet build
cd bin/app_debug
dotnet GameFrameX.Launcher.dll --DataBaseUrl="mongodb://admin:admin@localhost:27017/?authSource=admin"

# 3. 用 Unity 2019.4.40f1 打开 Unity/ 工程，打开 Assets/Scenes/Launcher.unity，点 Play
```

看到登录界面、能创建角色进入主城，就说明客户端↔服务器全链路通了。

服务器起了没有？终端里跑 `nc -z localhost 29100`（TCP）和 `nc -z localhost 28080`（HTTP）确认监听端口，能通就是活着。（29090 是性能指标端口，**默认关闭**——见下方端口表。）

### 安装

跟着做，大概 10~15 分钟（含 Unity 首次导入时间）。

#### 第 1 步：下载项目

```shell
git clone https://github.com/GameFrameX/GameFrameX.git
cd GameFrameX
```

不想用 git？GitHub 页面点 **Code → Download ZIP**，或从 [gitee 等镜像站](https://gitee.com/GameFrameX/GameFrameX) 下载，效果一样。

#### 第 2 步：装环境

| 要装 | 版本 | 哪里下 |
|---|---|---|
| **.NET SDK** | **10.0 或以上** | https://dotnet.microsoft.com/download |
| **Unity 编辑器** | **2019.4.40f1**（Unity Hub → Installs → Install Editor → Archive 里找） | https://unity.com/download |
| **Docker Desktop** | 任意新版 | https://www.docker.com/ |

> **注意**：.NET 10 是服务器和导表工具的硬要求，装错版本后面全卡。

#### 第 3 步：启动本地数据库

```shell
cd docker/mongo
docker compose up -d
```

起来的是 MongoDB：`mongodb://admin:admin@localhost:27017`（数据落在 `docker/mongo/database/`）。

> PostgreSQL（`docker/postgres/`）是给管理后台 Admin 用的，跑本教程用不到，可不启动。

#### 第 4 步：编译并启动服务器

```shell
cd ../../Server
dotnet build
cd bin/app_debug
dotnet GameFrameX.Launcher.dll --DataBaseUrl="mongodb://admin:admin@localhost:27017/?authSource=admin"
```

**为什么只传一个参数？** 服务器默认配置（见 `Server/GameFrameX.Launcher/StartUp/AppStartUpGame.cs`）已经开好了全套端口：

| 端口 | 用途 | 默认 |
|---|---|---|
| 29100 | TCP：游戏客户端长连接 | 默认开启 |
| 28080 | HTTP：登录等接口（`/game/api/...`） | 默认开启 |
| 29110 | WebSocket | 默认关闭，需 `--IsEnableWebSocket true` 开启 |
| 29090 | 性能指标 / 健康检查 | 默认关闭，需 `--IsOpenTelemetryMetrics true --MetricsPort 29090` 开启 |

唯一要覆盖的是 `DataBaseUrl`——默认值连的是演示用公网库，本地开发请指到自己刚起的 MongoDB。

**用 IDE 更简单**：用 Rider / Visual Studio 打开 `Server/Server.slnx`（不支持 `.slnx` 就开 `Server.sln`），启动项目选 `GameFrameX.Launcher`，**Working directory 设为 `Server/bin/app_debug`**，命令行参数留空——并在 `AppStartUpGame.cs` 里把 `DataBaseUrl` 默认值改成本地连接串（改的是聚合仓内的文件，仅本地调试用，见下方同步覆盖说明）。

**验证**：终端里跑 `nc -z localhost 29100 && nc -z localhost 28080`，能通就是活着（或看服务器日志里的 `has been started` / `Now listening on`）。

#### 第 5 步：Unity 客户端连接

1. Unity Hub 用 **2019.4.40f1** 打开仓库里的 `Unity/` 文件夹（首次打开会自动拉取 Package，需要联网，耐心等）
2. 打开场景 `Assets/Scenes/Launcher.unity`
3. 点 **Play**

客户端默认连 `127.0.0.1`（TCP 29100 / HTTP 28080），和服务器默认端口正好对上，不需要改任何配置。看到登录界面、创建角色进主城，教程就通关了。

> 换电脑 / 部署到远程服务器时，改这两处：TCP 地址在 `Unity/Assets/Hotfix/UI/Logic/UILogin/UIPlayerList.cs`（`serverIp` / `serverPort`），HTTP 地址在 `Unity/Assets/Hotfix/UI/Logic/UILogin/UILogin.cs` 等（搜 `127.0.0.1:28080`）。

#### 想用 LayaAir 客户端？

用 LayaAir IDE 打开 `LayaBox/`，入口 `src/Main.ts`。注意两点：WebSocket **默认关闭**——先把服务器用 `--IsEnableWebSocket true` 启动（默认 WsPort 29110；`nettest.ts` 默认连 `ws://127.0.0.1:21100`，与服务器**不一致**，改成一致才能连上）；连接地址在 `LayaBox/src/gameframex/nettest.ts`；协议生成用 `Protobuf/Proto2TsExport_LayaBox.sh`。

## 使用示例

下载下来的快照**自带全部生成产物**（配置代码 / 数据、协议代码都已就位），直接就能跑。只有当你改了源头文件，才需要重新生成：

### 改了 Excel 配置（`Config/Excels/Tables/` 下的表）

| 你改了 | 跑哪个 | 产物去哪 |
|---|---|---|
| 服务端要读的表 | `cd Config && sh gen-server-bin.sh`（Windows 双击 `gen-server-bin.bat`） | `Server/GameFrameX.Config/` |
| 客户端要读的表 | `cd Config && sh gen-client-json.sh` | `Unity/Assets/`（代码 + 数据） |

> 表文件名有讲究：`字母-英文名-中文名.xlsx`（如 `D-ItemConfig-道具表-道具-1001.xlsx`），Excel 里前 4 行是表头（`##var` / `##type` / `##group` / 说明），第 5 行起才是数据。完整规则看 [GameFrameX.Config](https://github.com/GameFrameX/GameFrameX.Config)。

### 改了通讯协议（`Protobuf/*.proto`）

导出工具不随仓库分发，先构建一次（聚合仓的目录布局已满足它的输出路径要求）：

```shell
cd Tools
dotnet build ProtoExport/ProtoExport.csproj -c Release   # 产物自动落到 ../Protobuf/Tools/
cd ../Protobuf
sh Proto2CsExport_Server.sh    # 服务端协议 → Server/GameFrameX.Proto/
sh Proto2CsExport_Client.sh    # 客户端协议 → Unity/Assets/Hotfix/Proto/
```

> 协议有硬规则：只支持 proto3；`option module = 10;` 必填；消息必须叫 `Req<名字>` / `Resp<名字>` / `Notify<名字>`；字段编号必须 < 800；禁止嵌套 message。完整规则看 [GameFrameX.Protobuf](https://github.com/GameFrameX/GameFrameX.Protobuf)。

### 改了 UI（FairyGUI）

用 FairyGUI 编辑器（≥5.0）打开 `FairyGUIProject/Game.fairy`，改完 **文件 → 发布，务必勾选「生成代码」**，产物自动写入 `Unity/Assets/`（UI 资源 + C# 绑定代码）。

> 新人最常见问题：发布完 Unity 里报找不到类 → 十有八九是发布时没勾「生成代码」。

### 常见坑

| 现象 | 原因 & 解法 |
|---|---|
| 服务器启动报连不上数据库 | `DataBaseUrl` 没传，默认连的是公网演示库；传第 4 步那条本地连接串 |
| IDE 里启动就闪退 / 找不到 hotfix | Working directory 没设成 `Server/bin/app_debug`（服务器从「当前目录/hotfix」加载热更程序集） |
| Unity 首次打开卡在拉包 | 需要联网访问 UPM 私有源（`gameframex.upm.alianblank.uk`）和 gitee（HybridCLR），网络受限会卡住 |
| 客户端连不上服务器 | 确认端口组对得上：TCP 29100 / HTTP 28080；WebSocket 29110 需 `--IsEnableWebSocket true`（默认关闭）；服务器日志里有监听列表 |
| 在本仓库改了代码，第二天没了 | 聚合仓每日同步会覆盖，改动请提交到对应源仓库 |
| LayaBox 连不上 | WebSocket 默认关闭——先用 `--IsEnableWebSocket true` 启动服务器；再把 `nettest.ts`（默认 21100）改成与服务器 WsPort 29110 一致 |

## 架构概览

本仓库是**聚合发布仓**——每天自动把下面 7 个源仓库的最新代码同步到同名文件夹里，所以你下载一次就拿到所有零件，而且**文件夹天生就在正确位置**（配置生成、协议导出都靠相对路径互相找到对方，别改名、别挪位置）：

```
GameFrameX/                   # 项目根目录
├── Server/                   # 游戏服务器（.NET 10，Actor 模型 + 热更新）
├── Unity/                    # Unity 客户端工程（含 HybridCLR 热更、YooAsset 资源）
├── LayaBox/                  # LayaAir 客户端工程（可选客户端）
├── Config/                   # LuBan 配置表：Excel 在这改，一键生成两端代码
├── Protobuf/                 # 通讯协议：.proto 在这改，一键导出各端代码
├── FairyGUIProject/          # UI 编辑工程（FairyGUI 编辑器打开 Game.fairy）
├── Tools/                    # 辅助工具（协议导出 CLI / GUI）
├── docker/                   # 本地数据库一键启动（mongo / postgres）
├── scripts/                  # 聚合同步脚本
└── README / LICENSE 等
```

| 目录 | 对应源仓库（改动请去这里提 PR / Issue） |
|------|------|
| `Server/` | https://github.com/GameFrameX/GameFrameX.Server |
| `Unity/` | https://github.com/GameFrameX/GameFrameX.Unity |
| `LayaBox/` | https://github.com/GameFrameX/GameFrameX.LayaBox |
| `Config/` | https://github.com/GameFrameX/GameFrameX.Config |
| `Protobuf/` | https://github.com/GameFrameX/GameFrameX.Protobuf |
| `FairyGUIProject/` | https://github.com/GameFrameX/GameFrameX.FairyGUIProject |
| `Tools/` | https://github.com/GameFrameX/GameFrameX.Tools |

> **注意**：**直接改本仓库里的 `Server/`、`Unity/` 等目录是没用的**——每天自动同步会把改动覆盖掉。要改代码、交 PR，请去上表对应的源仓库。

**不聚合的仓库**（按需自取）：

| 仓库 | 说明 |
|------|------|
| [GameFrameX.Foundation](https://github.com/GameFrameX/GameFrameX.Foundation) | 服务器底层库，以 NuGet 包形式被 Server 引用（构建时自动还原，无需 clone） |
| [GameFrameX.Admin](https://github.com/GameFrameX/GameFrameX.Admin) | 管理后台（部分源码不开源），[在线演示](https://game.admin.web.vue.alianblank.com) |
| [GameFrameX.CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) / [Godot](https://github.com/GameFrameX/GameFrameX.Godot) | 其他引擎客户端 |
| [GameFrameX.Docs](https://github.com/GameFrameX/GameFrameX.Docs) | 文档站源码 |

## 平台支持

支持这些主流引擎——你用哪个，它都吃得下：

| 平台 | 客户端工程 | 说明 |
|---|---|---|
| Unity | `Unity/` 本仓库内 | **2019.4.40f1**，HybridCLR 热更、YooAsset，主客户端 |
| LayaAir（LayaBox） | `LayaBox/` 本仓库内 | 备用客户端，入口 `src/Main.ts` |
| Cocos Creator | [GameFrameX.CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) | 独立仓库 |
| Godot | [GameFrameX.Godot](https://github.com/GameFrameX/GameFrameX.Godot) | 独立仓库 |
| 服务器 | `Server/` 本仓库内 | .NET 10，Actor 模型，所有客户端共用 |

## 依赖

| 组件 | 版本 | 用途 |
|------|------|------|
| **.NET SDK** | **10.0+** | 编译运行服务器（Foundation 依赖经 NuGet 自动还原，首次需联网） |
| **Unity** | **2019.4.40f1** | 打开客户端 `Unity/`（首次导入需联网拉取 Package） |
| **Docker** | 任意新版 | 一键启动本地 MongoDB |

## 文档与资源

> 文档真在写了，别催。所有站点内容一致，挑一个能打开的用就行。

- 主站：https://gameframex.doc.alianblank.com
- 备用 1：https://gameframex-docs.pages.dev
- 备用 2：https://gameframex.doc.cloudflare.alianblank.com
- 备用 3：https://gameframex.doc.vercel.alianblank.com

## 社区与支持

- QQ 群：**467608841 / 233840761**
- [Bilibili](https://www.bilibili.com/video/BV1yrpeepEn7)
- [Gitee](https://gitee.com/GameFrameX/gameframex)
- [Discord](https://discord.gg/VDWUjWMDw9)
- [GitHub](https://github.com/GameFrameX/gameframex)
- [LinkedIn](https://www.linkedin.com/in/alianblank)
- [Reddit](https://www.reddit.com/r/GameFrameX/)
- [X](https://x.com/alian_blank)
- [YouTube](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
- [Bluesky](https://bsky.app/profile/alianblank.bsky.social)

### 赞助

![wechat.jpg](https://raw.githubusercontent.com/GameFrameX/GameFrameX/42e755df/Docs/imgs/wechat.jpg)

[AITKPARTY](https://aitkparty.com/) 是一个 AI 大模型 API 中转聚合服务，基于开源项目 New API 搭建，提供统一接口让开发者便捷地访问主流大语言模型，省去自行对接多家模型供应商的麻烦。

### 贡献名单

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

## 更新日志

详见 [GitHub Releases](https://github.com/GameFrameX/GameFrameX/releases) 页面。

## 开源协议

详见 [LICENSE.md](LICENSE.md)。

> 所有插件均来自互联网，使用时请自行付费。如有侵权请发 email，本人会移除，谢谢。
>
> 该项目不得用于当地法律不允许的范围。技术本无罪，错的是滥用技术的人。
