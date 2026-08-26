<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX.Config

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Config?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Config/releases)
[![License](https://img.shields.io/badge/license-Apache%202.0-orange.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)

**All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams**

[📖 Documentation](https://gameframex.doc.alianblank.com/) • [🚀 Quick Start](#beginner-walkthrough) • [💬 QQ Group: 870596322](https://qm.qq.com/q/IrE4RSmqgY)

---

🌐 **Language**: **English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

---

</div>

## What is this?

**GameFrameX.Config is a "config table tool."**

In plain words: **Game designers fill in game data in Excel, and this tool automatically turns it into code and data files that both the game client and server can use directly.**

Think of it this way — an Excel table is your "game data dictionary," and Config translates that dictionary into a format the program can read directly. Designers just fill in the tables, programmers just read the data, and Config handles the middle step for you automatically.

It's built on the open-source tool [Luban](https://github.com/GameFrameX/luban) (with custom enhancements by GameFrameX).

## What can it do for you?

**If you're a game designer:**

- Just fill in data in the Excel you already know (items, achievements, sounds, localized text...)
- After editing, hand it to a programmer to "generate," and the data syncs into the game
- You never have to touch code

**If you're a developer:**

- Run one script and automatically get C# config classes + data files
- Read data directly in code with `tables.TbXxx.Get(id)` — no need to hand-write parsers
- Client (Unity) and server (.NET) each get their own copy, and the types line up

## Key terms in plain English

| Term | Plain English explanation |
|------|---------------------------|
| **Config table** | A game data table stored in Excel. For example, the item table, achievement table, or level table. |
| **Client** | The game program on the player's side — here, built with Unity. |
| **Server** | The program running on the server — here, built with .NET. |
| **Generate** | The automatic step that turns Excel into code and data the program can use directly. |
| **Localization** | The same text exists in several language versions (Chinese / English / Japanese / Korean...); which one the player sees depends on their settings. |

## What's in the folders

```
Config/
├── Defines/        ← Built-in data types (coordinates, etc.)
├── Excels/         ← Your Excel files go here (most important)
│   ├── Tables/     ← Game data tables (items, achievements, etc.)
│   └── Local/      ← Localized text
├── Tools/          ← The tool itself (don't touch)
├── luban.conf      ← Tool config (usually don't touch)
└── gen-*.bat/.sh   ← Generate scripts (double-click or run to use)
```

**Focus on these:**

- **`Excels/Tables/`** — Game data tables go here. For example, the item table, the achievement table.
- **`Excels/Local/`** — Localized text goes here. Translations of the same text into different languages.
- **`Excels/__tables__.xlsx`, `__beans__.xlsx`, `__enums__.xlsx`** — These three are "advanced definition tables" used to define complex field types (like enums and structs). Beginners can ignore them at first — `int` and `string` are enough to fill in a table.
- **`Defines/`** — Built-in type definitions that ship with the tool (like coordinates `vec2/vec3/vec4`). The client and server auto-adapt to their own coordinate types.
- **`Tools/`** — The tool itself, don't touch.
- **`gen-client-json.bat`, `gen-server-bin.bat`** — The generate scripts. **This is what you'll click most often.**

## Beginner walkthrough

Let's walk through making an "item table" from scratch, covering the whole flow once. Follow along once and you'll get it all.

### Step 1: Create the Excel file

In the `Excels/Tables/` folder, create a new Excel file named:

```
D-MyItem-我的道具表.xlsx
```

**Where does the name come from? Remember this formula: `a letter, an English name, a Chinese name`**

- `D` — A single letter, just to keep files sorted and easy to find in the folder. Any letter works (A/B/C/D all fine).
- `MyItem` — The English name. **This becomes the class name in code** (a `Tb` prefix is auto-added → `TbMyItem`).
- `我的道具表` — The Chinese name, for humans to read. Write whatever you want.

### Step 2: Fill in the header (the first 4 rows are the "instruction manual")

Open the file. The first 4 rows are the fixed "header" that tells the tool which fields this table has:

| Row | What to fill | This example |
|-----|--------------|--------------|
| Row 1 `##var` | Field name (English) | `id`, `name`, `price` |
| Row 2 `##type` | Field type | `int`, `text`, `int` |
| Row 3 `##group` | Field group (usually leave empty) | empty, empty, empty |
| Row 4 `##` | Description for humans | Item ID, Item name, Price |

Filled in, it looks like this:

| ##var | id | name | price |
|-------|----|------|-------|
| ##type | int | text | int |
| ##group | | | |
| ## | Item ID | Item name | Price |

> The first cell of each of these four rows (`##var`, `##type`, `##group`, `##`) is a fixed marker — write it exactly as shown.

### Step 3: Fill in the data (from row 5)

Below the header is the real data — one row per entry:

| ##var | id | name | price |
|-------|----|------|-------|
| ##type | int | text | int |
| ##group | | | |
| ## | Item ID | Item name | Price |
| | 10001 | diamond | 10 |
| | 10002 | coin | 1 |

- `id` uses a number (`int`)
- `name` is filled with a **localization key** (type `text`). The actual text shown to players is translated in `Excels/Local/`. For example, fill in `diamond` here, then in the localization table write `diamond` = 钻石 / 鑽石 / ダイヤ…
- `price` uses a number

### Step 4: Generate the code

Back in the `Config` folder:

- **Windows**: Double-click `gen-client-json.bat`
- **Mac / Linux**: Run `sh gen-client-json.sh` in the terminal

Wait for it to finish (you're good when you see `pause` or no errors).

### Step 5: Get the output

The tool automatically generates two things in the `Unity` folder next door:

- **Data file** (JSON): contains your item data
- **Code file** (C#): contains a `TbMyItem` class — that's your item table

### Step 6: Use it in code

```csharp
// Get the item whose id is 10001
var item = tables.TbMyItem.Get(10001);

// The item name automatically shows the current language (e.g. "钻石" in Chinese)
Debug.Log(item.Name);
Debug.Log(item.Price); // 10
```

**Done!** The data you filled in Excel has now become code the game can use directly ✅

## How to name a table

We used that formula above — here's the full version:

```
letter - English name - Chinese name.xlsx
letter - English name - group - Chinese name.xlsx      ← when you want to limit it to only one side
```

**What the three parts mean:**

| Part | What it is | Rules | Example |
|------|------------|-------|---------|
| **Letter** | A single letter for sorting, makes files easy to find | Any letter or digit | `C`, `D`, `S`, `L` |
| **English name** | Becomes the code class name `Tb<English name>` | English only, **no Chinese** | `ItemConfig` → `TbItemConfig` |
| **Chinese name** | A name for humans to read | Write whatever, extra `-` allowed | `道具表`, `道具表-1001` |

**⚠️ Note: the English name must never contain Chinese**, or the tool will throw an error saying Chinese table names aren't supported.

**Want it only for the client or only for the server?** Add a group tag between the English name and the Chinese name:

| Filename | Effect |
|----------|--------|
| `D-ItemConfig-道具表.xlsx` | Both client and server **use it** (default) |
| `D-ItemConfig-c-道具表.xlsx` | **Only the client** uses it |
| `D-ItemConfig-s-道具表.xlsx` | **Only the server** uses it |

> `c` = client, `s` = server. No group tag means both sides get it.

**Existing table names at a glance:**

| Filename | Generated class name |
|----------|----------------------|
| `C-AchievementConfig-成就表.xlsx` | `TbAchievementConfig` |
| `D-ItemConfig-道具表-道具-1001.xlsx` | `TbItemConfig` |
| `S-SoundsConfig-声音表.xlsx` | `TbSoundsConfig` |
| `L-Localization-成就.xlsx` | `TbLocalization` |

## How to fill in a table

The first 4 rows of every data table are the fixed "header":

| Row | Marker | What to fill |
|-----|--------|--------------|
| 1 | `##var` | Field name (English, e.g. `id`, `name`) |
| 2 | `##type` | Field type (see table below) |
| 3 | `##group` | Field group, usually leave empty |
| 4 | `##` | Description, for you and your colleagues |

**Common field types:**

| Type | Meaning | Example |
|------|---------|---------|
| `int` | Integer | `10001` |
| `string` | Plain text (not translated) | `icon_diamond` |
| `text` | Localized text (fill in a key; the real text lives in `Local/`) | `diamond` |
| `bool` | Yes/no | `true` / `false` |
| `float` | Decimal | `1.5` |
| enum name | A type defined in `__enums__.xlsx` | `ItemType` |

> The difference between `text` and `string`: `text` is localized text that gets translated (you fill in a key), `string` is plain text that isn't translated (you fill in the content directly).

**A filled-in example (a fragment of the achievement table):**

| ##var | id | image | name | achievement_content |
|-------|----|-------|------|---------------------|
| ##type | int | int | text | text |
| ##group | | | | |
| ## | ID | Icon id | Achievement Key | Achievement content Key |
| | 900001 | 101 | achievement_001 | achievement_001_desc |

## Table too big? Split it

When one table has a lot of data (say, thousands of items), you can **split it across several files** — the tool will auto-merge them into one table.

**How to split?** Just make sure **the English name is the same**; the Chinese name can be whatever helps you tell them apart:

```
D-ItemConfig-道具表-1-1000.xlsx      ← items 1~1000
D-ItemConfig-道具表-1001-2000.xlsx   ← items 1001~2000
D-ItemConfig-道具表-2001-3000.xlsx   ← items 2001~3000
```

All three files share the English name `ItemConfig`, so the tool auto-merges them into one `TbItemConfig`.

**The localization tables work the same way** (split by module):

```
L-Localization-成就.xlsx    ┐
L-Localization-文本.xlsx    ├→ merged into one TbLocalization
L-Localization-UI.xlsx      ┘
```

> Numbers or categories in the Chinese name (like `1-1000`, `成就`) are just for humans — the tool doesn't parse them, write whatever makes your life easier.

## How to generate the code

### Get ready

1. Install the **.NET SDK** (the tool runs on it)
2. Next to the `Config` folder, you need `Unity` and `Server` folders (the generated code goes in there)

### Generate client (Unity) data

- **Windows**: Double-click `gen-client-json.bat`
- **Mac / Linux**: `sh gen-client-json.sh`

Where the generated stuff goes:

- Data → `../Unity/Assets/Bundles/Config`
- Code → `../Unity/Assets/Hotfix/Config/Generate`

### Generate server (.NET) data

- **Windows**: Double-click `gen-server-bin.bat`
- **Mac / Linux**: `sh gen-server-bin.sh`

Where the generated stuff goes:

- Data → `../Server/GameFrameX.Config/Json`
- Code → `../Server/GameFrameX.Config/Config`

> The four scripts follow this pattern: `gen-{side}-{format}.{sh/bat}`, where side = `client`/`server` and format = `json` (human-readable) / `bin` (smaller and faster).

## How to use the generated code

**In the client (Unity):**

```csharp
// "tables" is the config manager, auto-generated by the tool
// TbItemConfig is the "item table" you filled in; Get(id) looks up by id
var item = tables.TbItemConfig.Get(10001);
Debug.Log($"Name:{item.Name}, Price:{item.Price}");

// Loop through all items
foreach (var it in tables.TbItemConfig.DataList)
{
    Debug.Log(it.Name);
}
```

**In the server (.NET):**

```csharp
var item = tables.TbItemConfig.Get(10001);
Console.WriteLine($"{item.Name}: {item.Price}");
```

> Fields of type `text` (like `Name`) automatically show the player's current language — you don't have to handle languages yourself.

## Where the generated code goes

The tool generates separately for each "side," so they don't interfere:

| Generated for | Which script | Code namespace |
|---------------|--------------|----------------|
| **Client** (Unity) | `gen-client-*` | `Hotfix.Config` |
| **Server** (.NET) | `gen-server-*` | `GameFrameX.Config` |
| **Both sides** | Run the matching script once for each | Each side gets its own |

> Easy way to remember: the client uses the `client` script, the server uses the `server` script — run whichever side you need.

## Tables in this repo

These demo tables ship with the repo right now:

| Table | File | Content |
|-------|------|---------|
| Achievement | `Excels/Tables/C-AchievementConfig-成就表.xlsx` | Achievement definitions |
| Item | `Excels/Tables/D-ItemConfig-道具表-道具-1001.xlsx` | Item definitions |
| Sound | `Excels/Tables/S-SoundsConfig-声音表.xlsx` | Sound definitions |
| Localization - Achievement | `Excels/Local/L-Localization-成就.xlsx` | Achievement localized text |
| Localization - Text | `Excels/Local/L-Localization-文本.xlsx` | General localized text |
| Localization - UI | `Excels/Local/L-Localization-UI.xlsx` | UI localized text |

Want to add a new table? Just follow the steps in "Beginner walkthrough."

## Requirements

- **.NET SDK** — runs the tool (download it from [dot.net](https://dotnet.microsoft.com/))
- **Excel** (or WPS, Numbers, or any app that can edit `.xlsx`) — for filling in tables
- **OS** — Windows, Mac, and Linux all work

## License

This project is open-sourced under the [Apache License 2.0](LICENSE.md) — free to use, including for commercial purposes.

## Related links

- [Documentation](https://gameframex.doc.alianblank.com)
- [GitHub repository](https://github.com/GameFrameX/GameFrameX.Config)
- [Issue feedback](https://github.com/GameFrameX/GameFrameX.Config/issues)
- [Luban (GameFrameX custom version)](https://github.com/GameFrameX/luban)
- [Luban (original upstream)](https://github.com/focus-creative-games/luban)
