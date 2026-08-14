<div align="center">
    <a href="https://trendshift.io/repositories/20145" target="_blank"><img src="https://trendshift.io/api/badge/repositories/20145" alt="GameFrameX%2FGameFrameX | Trendshift" style="width: 250px; height: 55px;" width="250" height="55"/></a>
</div>

[简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **English** | [日本語](README.ja.md) | [한국어](README.ko.md)

# 🎮 What exactly is GameFrameX?

In one sentence: **an open-source toolbox that helps you take a game all the way "from idea → built → live and running."**

When you make a game, the really hard part usually isn't "drawing a character or coding a skill" — it's stitching all those pieces together:

- Where do player save files live? How do you read them back?
- In a multiplayer game, how does the server forward messages?
- Who manages items, levels, and progression data? What happens when a designer changes something?
- After launch, how do you read the data, manage players, and ship new versions?

All that heavy lifting is already done for you by GameFrameX — you just focus on the fun part: "is my game actually fun?"

It supports the major engines: **Unity, Cocos Creator, LayaAir (LayaBox), Godot** — whichever you use, it fits.

---

# 🧰 What headaches does it save you?

| What you'd otherwise DIY | What GameFrameX hands you out of the box |
|---|---|
| Writing a multiplayer server from scratch | A ready-to-go high-performance server (written in .NET, built to handle many concurrent players) |
| Figuring out how to store data | Player data goes to MongoDB (fast reads/writes), backend data goes to PostgreSQL (rock-solid) |
| Hand-carrying Excel configs into code | LuBan turns Excel into code and data in one click |
| Client and server "speaking the same language" | ProtoBuf unifies the protocol — change it once, both sides sync |
| Flying blind after launch | A built-in admin web panel for reading data / managing players / pushing configs |
| Server deployment giving you a headache | One-click packaging and deployment with Docker, hassle-free |

> Plain and simple: **even a solo developer can build and run an online game like a small team would.**

---

# 👤 Who is it for?

- Indie developers who want to make **online / multiplayer games** but are stuck on "how do I even do the server?"
- Small teams that want to quickly spin up a **game prototype** to validate an idea
- Learners who want to go through the full "client + server + backend" pipeline end-to-end

---

# 🗺️ What are all these repos for? (Repo map)

GameFrameX is a "bundle", but each dish in the bundle lives in **its own separate repo** (so you can maintain and upgrade them independently). Start with this table to get the big picture:

| Repo | In plain terms… | URL |
|---|---|---|
| 🏠 **Main repo (you are here)** | The "kitchen floor plan" — tells you which folder each piece belongs in | https://github.com/GameFrameX/GameFrameX |
| 🌐 **Server** | The game's brain: handles multiplayer, saves, and combat logic (evolved from GeekServer) | https://github.com/GameFrameX/GameFrameX.Server |
| 📊 **Config tables (LuBan)** | Fill in game data (items / levels / progression…) in Excel, generate code in one click | https://github.com/GameFrameX/GameFrameX.Config |
| 📡 **Protocol (ProtoBuf)** | The "rules of conversation" between client and server; defines the messages both sides exchange | https://github.com/GameFrameX/GameFrameX.Protobuf |
| 🎨 **UI project (FairyGUI)** | The source project for designing game UI in the FairyGUI editor | https://github.com/GameFrameX/GameFrameX.FairyGUIProject |
| 🛠️ **Tools** | assorted helper utilities | https://github.com/GameFrameX/GameFrameX.Tools |
| 💻 **Admin backend** | The web panel for managing data and players after launch (some source code is not open) | https://github.com/GameFrameX/GameFrameX.Admin |

Live admin demo 👉 https://game.admin.web.vue.alianblank.com

## 🎮 Client (pick one — download the one you use)

| Engine | URL |
|---|---|
| Unity | https://github.com/GameFrameX/GameFrameX.Unity |
| Cocos Creator | https://github.com/GameFrameX/GameFrameX.CocosCreator |
| LayaAir (LayaBox) | https://github.com/GameFrameX/GameFrameX.LayaBox |
| Godot | https://github.com/GameFrameX/GameFrameX.Godot |

---

# 📁 Why can't I just put folders wherever I want?

> ⚠️ **Important**: this framework locates files **by relative path** — kind of like the outlets in your house. Move the server from `Server/` to `MyServer/` and the whole chain loses its bearings.

So please follow the structure below and **place each repo in the folder it belongs in**:

```
GameFrameX/                  # Project root directory (name can be changed)
├── Config/                  # ← Put GameFrameX.Config here (Excel configs + LuBan export)
├── Protobuf/                # ← Put GameFrameX.Protobuf here (communication protocol)
├── FairyGUIProject/         # ← Put GameFrameX.FairyGUIProject here (UI editing project)
├── Server/                  # ← Put GameFrameX.Server here (game server)
├── Unity/                   # ← Put GameFrameX.Unity here (Unity client; swap for another engine if needed)
│   ├── Assets/              #    Unity assets folder
│   ├── Packages/            #    Unity packages
│   ├── ProjectSettings/     #    Unity project settings
│   └── UserSettings/        #    Unity user settings
├── Tools/                   # ← Put GameFrameX.Tools here (auxiliary tools)
├── docker/                  # Docker local runtime environment (MongoDB / PostgreSQL)
├── Docs/                    # Documentation (currently mostly GeekServer's original docs)
└── LICENSE.md               # Open-source license
```

> Want to switch to a different client engine? Just replace `Unity/` with the matching name (`Laya/`, `CocosCreator/`, `Godot/`) — same rule applies.

---

# 🔧 Get your environment ready first

Before you start, install the following (click the links to download from the official sites):

| What to install | Version | What it's for | Where to get it |
|---|---|---|---|
| **Git** | any recent version | Pulling the code for each repo | https://git-scm.com/ |
| **.NET SDK** | **10.0 or above** | Compiling/running the server, running the LuBan export tool | https://dotnet.microsoft.com/download |
| **Unity editor** | **2019.4.40f1** (compatible with 2019.4+) | Opening and running the Unity client | https://unity.com/download |
| **Docker** (optional but recommended) | any recent version | Spinning up local MongoDB / PostgreSQL databases with one click | https://www.docker.com/ |

> 💡 Both the server and the export tool depend on **.NET 10.0** — this is the most critical version requirement, so make sure you get it right.

---

# 🚀 From zero to running, step by step

**Step 1**: Create a folder for the project, open a terminal (cmd / PowerShell on Windows, Terminal on Mac / Linux), and `cd` into it.

**Step 2**: Pull down the "kitchen floor plan":

```shell
git clone https://github.com/GameFrameX/GameFrameX.git
```

This creates a `GameFrameX/` folder containing the project skeleton.

**Step 3**: Place each component into its **matching folder** inside `GameFrameX/` (the example below uses Unity; for other engines, swap the last line for the matching URL):

```shell
git clone https://github.com/GameFrameX/GameFrameX.Server.git ./GameFrameX/Server
git clone https://github.com/GameFrameX/GameFrameX.Config.git ./GameFrameX/Config
git clone https://github.com/GameFrameX/GameFrameX.Protobuf.git ./GameFrameX/Protobuf
git clone https://github.com/GameFrameX/GameFrameX.FairyGUIProject.git ./GameFrameX/FairyGUIProject
git clone https://github.com/GameFrameX/GameFrameX.Tools.git ./GameFrameX/Tools
git clone https://github.com/GameFrameX/GameFrameX.Unity.git ./GameFrameX/Unity
```

> These lines just mean "download repo X into folder X." **Do not rename the folders.**

**Step 4 (Start the local databases)**: If you have Docker installed, go into the two directories and spin up MongoDB and PostgreSQL (the server talks to MongoDB, the admin backend talks to PostgreSQL):

```shell
cd GameFrameX/docker/mongo && docker compose up -d
cd ../postgres && docker compose up -d
```

Once they're running, connect like this:
- MongoDB: `mongodb://admin:admin@localhost:27017`
- PostgreSQL: `localhost:5432`, username `postgres` / password `postgres`, initial database `gameframex`

> ⚠️ The credentials above are the local development defaults — they need to match the connection settings in `Server` / `Admin` for things to connect.

**Step 5 (Generate config code)**: Go into the `Config/` directory and run the LuBan export script inside to turn Excel into code and data that both the client and the server can use. See the 👉 [`GameFrameX.Config`](https://github.com/GameFrameX/GameFrameX.Config) instructions for the exact commands.

**Step 6 (Generate protocol code)**: Go into the `Protobuf/` directory and run the protocol export script to generate the message code used by each side. See the 👉 [`GameFrameX.Protobuf`](https://github.com/GameFrameX/GameFrameX.Protobuf) instructions for the exact commands.

**Step 7 (Optional)**: If you need them, open `Tools/` and compile the auxiliary utilities — see the 👉 [`GameFrameX.Tools`](https://github.com/GameFrameX/GameFrameX.Tools) instructions.

**Step 8 (Run it!)**: Open the `Unity/` project in Unity, start the server in `Server/`, and you're up and running 🎉

---

# 💬 Chat & feedback (suggestions, feature requests, bugs)

QQ group: **467608841**

# 📖 Documentation (it's genuinely being written, no rushing me 😅)

> All mirror sites have the same content — just pick whichever one opens for you.

- Main site: https://gameframex.doc.alianblank.com
- Mirror 1: https://gameframex-docs.pages.dev
- Mirror 2: https://gameframex.doc.cloudflare.alianblank.com
- Mirror 3: https://gameframex.doc.vercel.alianblank.com

---

# ☕ Buy the author a coffee

![wechat.jpg](Docs/imgs/wechat.jpg)

# 🎯 Who's using GameFrameX?

| Game | Launch channel | Launch date |
|:---|:---|:---|
| 深夜的烧烤店 (Late-Night BBQ Joint) | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |
| 连续黑白 (Continuous Black & White) | Douyin, Kuaishou, Alipay, HarmonyOS, TapTap, iOS, etc. | 2024-11 |

> Shipped a game built with GameFrameX? Feel free to open a PR or issue to add it to the list above — let's grow it together 🙌

# 👥 Contributors

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
                <a href="https://github.com/PingouinFerreux">
                    <img src="https://avatars.githubusercontent.com/u/212632237?v=4" width="100;" alt="PingouinFerreux"/>
                    <br />
                    <sub><b>PingouinFerreux</b></sub>
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

# 📜 Disclaimer

All plugins come from the internet; please pay for them yourself when using them. If anything here infringes your rights, send an email and I'll remove it — thank you.

This project may not be used for anything prohibited by your local laws. Technology itself is innocent; what's wrong is the people who abuse it.

# 💎 Sponsor

[AITKPARTY](https://aitkparty.com/) is an AI large-model API aggregator and relay service, built on the open-source project New API. It offers a unified interface so developers can easily tap into mainstream large language models — saving you the trouble of integrating multiple model providers yourself.
