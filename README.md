<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/documentation-docs-blue.svg)](https://gameframex.doc.alianblank.com)
[![Trendshift](https://trendshift.io/api/badge/repositories/20145)](https://trendshift.io/repositories/20145)

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#quick-start) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## Project Overview

**An open-source toolbox that helps you take a game all the way "from idea → built → live and running."**

When you make a game, the really hard part usually isn't "drawing a character or coding a skill" — it's stitching all those pieces together:

- Where do player save files live? How do you read them back?
- In a multiplayer game, how does the server forward messages?
- Who manages items, levels, and progression data? What happens when a designer changes something?
- After launch, how do you read the data, manage players, and ship new versions?

All that heavy lifting is already done for you by GameFrameX — you just focus on the fun part: "is my game actually fun?"

### Features

| What you'd otherwise DIY | What GameFrameX hands you out of the box |
|---|---|
| Writing a multiplayer server from scratch | A ready-made high-performance server (written in .NET, built for many concurrent players) |
| Figuring out how to store data | Player data in MongoDB (fast), backend data in PostgreSQL (rock-solid) |
| Hand-carrying Excel configs into code | LuBan turns Excel into code and data in one click |
| Client and server "speaking the same language" | ProtoBuf unifies the protocol — change once, both sides sync |
| Flying blind after launch | A built-in admin web panel for reading data / managing players / pushing configs |
| Server deployment giving you a headache | One-click packaging and deployment with Docker |

> Plain and simple: **even a solo developer can build and run an online game like a small team would.**

**Who is it for:**

- Indie developers who want to make **online / multiplayer games** but are stuck on "how do I even do the server?"
- Small teams that want to quickly spin up a **game prototype** to validate an idea
- Learners who want to go through the full "client + server + backend" pipeline end-to-end

### Showcase

| Game | Channels | Live since |
|:---|:---|:---|
| 深夜的烧烤店 (Midnight BBQ) | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |
| 连续黑白 | Douyin, Kuaishou, Alipay, HarmonyOS, TapTap, iOS, etc. | 2024-11 |

> Shipped a game with GameFrameX? Open a PR or issue to add it to the list.

## Quick Start

**This repo IS the complete project**: git clone, Code → Download ZIP, or any mirror site — whatever way you download it, it runs as-is. No extra pulls needed.

Three steps (details in the [tutorial](#installation) below):

```shell
# 1. Start the local database (MongoDB, user admin / admin)
cd docker/mongo && docker compose up -d

# 2. Build & start the server (override only the DB connection; ports use defaults)
cd ../../Server && dotnet build
cd bin/app_debug
dotnet GameFrameX.Launcher.dll --DataBaseUrl="mongodb://admin:admin@localhost:27017/?authSource=admin"

# 3. Open the Unity/ project with Unity 2019.4.40f1, load Assets/Scenes/Launcher.unity, press Play
```

If you see the login screen and can create a character into the main city, the full client↔server loop works.

Is the server up? Check the listening ports: `nc -z localhost 29100` (TCP) and `nc -z localhost 28080` (HTTP) — success means it's alive. (Port 29090 is the metrics port and is **off by default** — see the port table below.)

### Installation

Follow along — about 10–15 minutes (Unity first import included).

#### Step 1: Download the project

```shell
git clone https://github.com/GameFrameX/GameFrameX.git
cd GameFrameX
```

Don't want git? **Code → Download ZIP** on GitHub, or grab it from a mirror like [gitee](https://gitee.com/GameFrameX/GameFrameX) — same result.

#### Step 2: Install the prerequisites

| Install | Version | Where |
|---|---|---|
| **.NET SDK** | **10.0 or newer** | https://dotnet.microsoft.com/download |
| **Unity Editor** | **2019.4.40f1** (Unity Hub → Installs → Install Editor → Archive) | https://unity.com/download |
| **Docker Desktop** | any recent | https://www.docker.com/ |

> **Note**: .NET 10 is a hard requirement for the server and the table-generation tool — get this one right.

#### Step 3: Start the local database

```shell
cd docker/mongo
docker compose up -d
```

That's MongoDB: `mongodb://admin:admin@localhost:27017` (data lands in `docker/mongo/database/`).

> PostgreSQL (`docker/postgres/`) serves the Admin backend — this tutorial doesn't need it.

#### Step 4: Build & start the server

```shell
cd ../../Server
dotnet build
cd bin/app_debug
dotnet GameFrameX.Launcher.dll --DataBaseUrl="mongodb://admin:admin@localhost:27017/?authSource=admin"
```

**Why only one argument?** The defaults (see `Server/GameFrameX.Launcher/StartUp/AppStartUpGame.cs`) already open the full port set:

| Port | Purpose | Default |
|---|---|---|
| 29100 | TCP: long-lived game client connections | on |
| 28080 | HTTP: login and other APIs (`/game/api/...`) | on |
| 29110 | WebSocket | off — start with `--IsEnableWebSocket true` |
| 29090 | metrics / health | off — start with `--IsOpenTelemetryMetrics true --MetricsPort 29090` |

The only thing to override is `DataBaseUrl` — the default points at a public demo database; point it at the MongoDB you just started.

**Even simpler with an IDE**: open `Server/Server.slnx` with Rider / Visual Studio (`Server.sln` if `.slnx` isn't supported), set the startup project to `GameFrameX.Launcher`, **set Working directory to `Server/bin/app_debug`**, leave arguments empty — and change the `DataBaseUrl` default in `AppStartUpGame.cs` to your local connection string (that edits a file inside the aggregated repo, fine for local debugging — see the overwrite note below).

**Verify**: `nc -z localhost 29100 && nc -z localhost 28080` in a terminal — success means it's alive (or check the server log for `has been started` / `Now listening on`).

#### Step 5: Connect the Unity client

1. Open the `Unity/` folder with **2019.4.40f1** via Unity Hub (first open pulls Packages — needs internet, be patient)
2. Load the scene `Assets/Scenes/Launcher.unity`
3. Press **Play**

The client defaults to `127.0.0.1` (TCP 29100 / HTTP 28080), matching the server's default ports — no config changes needed. Seeing the login screen and creating a character into the main city means the tutorial is complete.

> Moving to another machine / a remote server? Change two spots: the TCP address in `Unity/Assets/Hotfix/UI/Logic/UILogin/UIPlayerList.cs` (`serverIp` / `serverPort`), and the HTTP address in `Unity/Assets/Hotfix/UI/Logic/UILogin/UILogin.cs` etc. (search for `127.0.0.1:28080`).

#### Prefer the LayaAir client?

Open `LayaBox/` with the LayaAir IDE; entry point `src/Main.ts`. Two gotchas: WebSocket is **off by default** — start the server with `--IsEnableWebSocket true` first (default WsPort 29110; `nettest.ts` defaults to `ws://127.0.0.1:21100`, which does NOT match — align them); the connect address lives in `LayaBox/src/gameframex/nettest.ts`; protocol generation uses `Protobuf/Proto2TsExport_LayaBox.sh`.

## Usage Examples

The downloaded snapshot **ships with all generated artifacts** (config code/data, protocol code — all in place), so it runs as-is. Only regenerate when you change a source file:

### After editing Excel configs (`Config/Excels/Tables/`)

| What you changed | Run | Output goes to |
|---|---|---|
| tables the server reads | `cd Config && sh gen-server-bin.sh` (Windows: double-click `gen-server-bin.bat`) | `Server/GameFrameX.Config/` |
| tables the client reads | `cd Config && sh gen-client-json.sh` | `Unity/Assets/` (code + data) |

> File naming matters: `letter-EnglishName-ChineseName.xlsx` (e.g. `D-ItemConfig-道具表-道具-1001.xlsx`); the first 4 rows in each sheet are the header (`##var` / `##type` / `##group` / description), data starts at row 5. Full rules in [GameFrameX.Config](https://github.com/GameFrameX/GameFrameX.Config).

### After editing the protocol (`Protobuf/*.proto`)

The export tool is not shipped in the repo — build it once (the aggregated layout already satisfies its output-path requirements):

```shell
cd Tools
dotnet build ProtoExport/ProtoExport.csproj -c Release   # output lands in ../Protobuf/Tools/ automatically
cd ../Protobuf
sh Proto2CsExport_Server.sh    # server protocol → Server/GameFrameX.Proto/
sh Proto2CsExport_Client.sh    # client protocol → Unity/Assets/Hotfix/Proto/
```

> Protocol hard rules: proto3 only; `option module = 10;` is mandatory; messages must be named `Req<Name>` / `Resp<Name>` / `Notify<Name>`; field numbers must be < 800; no nested messages. Full rules in [GameFrameX.Protobuf](https://github.com/GameFrameX/GameFrameX.Protobuf).

### After editing UI (FairyGUI)

Open `FairyGUIProject/Game.fairy` with the FairyGUI editor (≥5.0), then **File → Publish — make sure "generate code" is checked**; output is written into `Unity/Assets/` (UI assets + C# binding code) automatically.

> Most common newbie issue: Unity reports missing classes after publishing → 9 times out of 10 the "generate code" checkbox wasn't ticked.

### Common Pitfalls

| Symptom | Cause & fix |
|---|---|
| Server fails to start, DB connection error | `DataBaseUrl` not passed — the default points at the public demo DB; pass the local connection string from the installation steps |
| IDE launch crashes / hotfix not found | Working directory not set to `Server/bin/app_debug` (the server loads hot-update assemblies from `<cwd>/hotfix`) |
| Unity first open stuck fetching packages | Needs internet access to the UPM registry (`gameframex.upm.alianblank.uk`) and gitee (HybridCLR); restricted networks will stall |
| Client can't reach the server | Make sure the port set matches: TCP 29100 / HTTP 28080; WebSocket 29110 needs `--IsEnableWebSocket true` (off by default); the server log lists what it's listening on |
| Your code edits vanished the next day | The daily sync overwrites the aggregated repo — commit changes to the corresponding source repo |
| LayaBox can't connect | WebSocket is off by default — start the server with `--IsEnableWebSocket true`; also align `nettest.ts` (defaults to 21100) with the server's WsPort 29110 |

## Architecture

This is an **aggregated release repo** — the latest code of the 7 source repos below is synced daily into same-named folders. One download gets you every piece, and **the folders are already in the right places** (config generation and protocol export find each other via relative paths — don't rename or move them):

```
GameFrameX/                   # project root
├── Server/                   # game server (.NET 10, Actor model + hot-update)
├── Unity/                    # Unity client project (HybridCLR hot-update, YooAsset)
├── LayaBox/                  # LayaAir client project (alternative client)
├── Config/                   # LuBan config tables: edit Excel here, generate code for both ends
├── Protobuf/                 # protocol: edit .proto here, export code for every end
├── FairyGUIProject/          # UI editing project (open Game.fairy in the FairyGUI editor)
├── Tools/                    # helper tools (protocol-export CLI / GUI)
├── docker/                   # one-command local databases (mongo / postgres)
├── scripts/                  # aggregation sync scripts
└── README / LICENSE etc.
```

| Directory | Source repo (send PRs / Issues here) |
|------|------|
| `Server/` | https://github.com/GameFrameX/GameFrameX.Server |
| `Unity/` | https://github.com/GameFrameX/GameFrameX.Unity |
| `LayaBox/` | https://github.com/GameFrameX/GameFrameX.LayaBox |
| `Config/` | https://github.com/GameFrameX/GameFrameX.Config |
| `Protobuf/` | https://github.com/GameFrameX/GameFrameX.Protobuf |
| `FairyGUIProject/` | https://github.com/GameFrameX/GameFrameX.FairyGUIProject |
| `Tools/` | https://github.com/GameFrameX/GameFrameX.Tools |

> **Warning**: **Editing `Server/`, `Unity/`, etc. inside THIS repo is pointless** — the daily sync will overwrite your changes. To change code or send PRs, go to the corresponding source repo in the table above.

**Repos NOT aggregated** (take them as needed):

| Repo | Notes |
|------|------|
| [GameFrameX.Foundation](https://github.com/GameFrameX/GameFrameX.Foundation) | Server foundation libraries, referenced by Server as NuGet packages (restored automatically at build, no clone needed) |
| [GameFrameX.Admin](https://github.com/GameFrameX/GameFrameX.Admin) | Admin backend (some source code not open), [live demo](https://game.admin.web.vue.alianblank.com) |
| [GameFrameX.CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) / [Godot](https://github.com/GameFrameX/GameFrameX.Godot) | clients for other engines |
| [GameFrameX.Docs](https://github.com/GameFrameX/GameFrameX.Docs) | docs site source |

## Platform Support

It supports the major engines — whichever you use, it fits:

| Platform | Client project | Notes |
|---|---|---|
| Unity | `Unity/` in this repo | **2019.4.40f1**, HybridCLR hot-update, YooAsset — primary client |
| LayaAir (LayaBox) | `LayaBox/` in this repo | alternative client, entry `src/Main.ts` |
| Cocos Creator | [GameFrameX.CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) | separate repo |
| Godot | [GameFrameX.Godot](https://github.com/GameFrameX/GameFrameX.Godot) | separate repo |
| Server | `Server/` in this repo | .NET 10, Actor model, shared by all clients |

## Dependencies

| Component | Version | Purpose |
|------|------|------|
| **.NET SDK** | **10.0+** | Build & run the server (Foundation dependency restored via NuGet, internet needed on first build) |
| **Unity** | **2019.4.40f1** | Open the client in `Unity/` (first import fetches Packages, internet needed) |
| **Docker** | any recent | One-command local MongoDB |

## Documentation & Resources

> All sites serve the same content — use whichever opens for you.

- Main: https://gameframex.doc.alianblank.com
- Mirror 1: https://gameframex-docs.pages.dev
- Mirror 2: https://gameframex.doc.cloudflare.alianblank.com
- Mirror 3: https://gameframex.doc.vercel.alianblank.com

## Community & Support

- [Discord](https://discord.gg/VDWUjWMDw9)
- [GitHub](https://github.com/GameFrameX/gameframex)
- [LinkedIn](https://www.linkedin.com/in/alianblank)
- [Reddit](https://www.reddit.com/r/GameFrameX/)
- [X](https://x.com/alian_blank)
- [YouTube](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
- [Bluesky](https://bsky.app/profile/alianblank.bsky.social)
- [Bilibili](https://www.bilibili.com/video/BV1yrpeepEn7)
- [Gitee](https://gitee.com/GameFrameX/gameframex)
- QQ group: **467608841 / 233840761**

### Sponsor

![wechat.jpg](https://raw.githubusercontent.com/GameFrameX/GameFrameX/42e755df/Docs/imgs/wechat.jpg)

[AITKPARTY](https://aitkparty.com/) is an AI LLM API relay/aggregation service built on the open-source New API project, giving developers one unified interface to major language models — no need to integrate each provider yourself.

### Contributors

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

## Changelog

See the [GitHub Releases](https://github.com/GameFrameX/GameFrameX/releases) page.

## License

See [LICENSE.md](LICENSE.md).

> All plugins come from the internet; pay for them yourself when used. If anything infringes your rights, email me and I'll remove it, thanks.
>
> This project must not be used where local law forbids it. Technology is innocent; those who abuse it are not.
