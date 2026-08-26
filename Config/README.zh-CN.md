<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX.Config

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Config?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Config/releases)
[![License](https://img.shields.io/badge/license-Apache%202.0-orange.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)

**独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使**

[📖 文档](https://gameframex.doc.alianblank.com/zh-CN) • [🚀 快速开始](#新手实战) • [💬 QQ群: 870596322](https://qm.qq.com/q/IrE4RSmqgY)

---

🌐 **语言**: [English](README.md) | **简体中文** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

---

</div>

## 这是什么？

**GameFrameX.Config 是一个「配置表工具」**。

简单说：**策划在 Excel 里填游戏数据，本工具会自动把它们变成代码和数据文件，游戏程序（客户端和服务端）都能直接用。**

打个比方——Excel 表就是你的「游戏数据字典」，Config 负责把这本字典翻译成程序能直接读懂的格式。策划只管填表，程序只管读数据，中间这步 Config 帮你自动完成。

它基于开源工具 [Luban](https://github.com/GameFrameX/luban) 构建（GameFrameX 做了定制增强）。

## 能帮你做什么？

**如果你是策划：**

- 在熟悉的 Excel 里填数据就行（道具、成就、声音、多语言文本……）
- 改完表交给程序「生成一下」，数据就同步到游戏里了
- 不用碰代码

**如果你是开发者：**

- 跑一个脚本，自动得到 C# 的配置类 + 数据文件
- 直接在代码里 `tables.TbXxx.Get(id)` 读取，不用手写解析
- 客户端（Unity）和服务端（.NET）各生成一份，类型还对得上

## 先搞懂几个词

| 词 | 大白话解释 |
|----|-----------|
| **配置表** | 游戏的数据表，存在 Excel 里。比如道具表、成就表、等级表。 |
| **客户端** | 玩家那边的游戏程序，这里是 Unity 做的。 |
| **服务端** | 服务器上跑的程序，这里是 .NET 做的。 |
| **生成** | 把 Excel 转成程序能直接用的代码和数据，这一步自动完成。 |
| **多语言（本地化）** | 同一条文字有多个语言版本（中文/英文/日文/韩文……），玩家看到哪种取决于设置。 |

## 文件夹里都有什么

```
Config/
├── Defines/        ← 自带的数据类型（坐标等）
├── Excels/         ← 你填的 Excel 都放这里（最重要）
│   ├── Tables/     ← 游戏数据表（道具、成就等）
│   └── Local/      ← 多语言文本
├── Tools/          ← 工具本体（不用动）
├── luban.conf      ← 工具配置（一般不用动）
└── gen-*.bat/.sh   ← 生成脚本（双击或运行就用）
```

**重点看这几个：**

- **`Excels/Tables/`** —— 游戏数据表放这里。比如道具表、成就表。
- **`Excels/Local/`** —— 多语言文本放这里。同一条文字的各国翻译。
- **`Excels/__tables__.xlsx`、`__beans__.xlsx`、`__enums__.xlsx`** —— 这三个是「高级定义表」，用来定义复杂的字段类型（比如枚举、结构体）。新手可以先不管，用最简单的 `int`、`string` 就能填表。
- **`Defines/`** —— 工具自带的类型定义（比如坐标 `vec2/vec3/vec4`），客户端和服务端会自动适配各自的坐标类型。
- **`Tools/`** —— 工具本体，不用动。
- **`gen-client-json.bat`、`gen-server-bin.bat`** —— 生成脚本，**这是你最常点的东西**。

## 新手实战

下面带你从零做一张「道具表」，走完一遍完整流程。跟着做一遍，你就全懂了。

### 第 1 步：新建 Excel 文件

在 `Excels/Tables/` 文件夹里，新建一个 Excel 文件，名字叫：

```
D-MyItem-我的道具表.xlsx
```

**名字怎么来的？记住一个公式：`字母 - 英文名 - 中文名`**

- `D` —— 一个字母，方便在文件夹里排序找文件，随便取（用 A/B/C/D 都行）
- `MyItem` —— 英文名，**会变成代码里的类名**（自动加 `Tb` 前缀 → `TbMyItem`）
- `我的道具表` —— 中文名，给人看的，写啥都行

### 第 2 步：填表头（前 4 行是「说明书」）

打开文件，前 4 行是固定的「表头」，告诉工具这张表有哪些字段：

| 行 | 填什么 | 本例 |
|----|--------|------|
| 第 1 行 `##var` | 字段名（英文） | `id`、`name`、`price` |
| 第 2 行 `##type` | 字段类型 | `int`、`text`、`int` |
| 第 3 行 `##group` | 字段分组（一般留空） | 空、空、空 |
| 第 4 行 `##` | 中文说明（给人看） | 道具ID、道具名、价格 |

填出来长这样：

| ##var | id | name | price |
|-------|----|------|-------|
| ##type | int | text | int |
| ##group | | | |
| ## | 道具ID | 道具名 | 价格 |

> 这四行的第一格（`##var`、`##type`、`##group`、`##`）是固定标记，必须照写。

### 第 3 步：填数据（第 5 行开始）

表头下面就是真正的数据，一行一条：

| ##var | id | name | price |
|-------|----|------|-------|
| ##type | int | text | int |
| ##group | | | |
| ## | 道具ID | 道具名 | 价格 |
| | 10001 | diamond | 10 |
| | 10002 | coin | 1 |

- `id` 用数字（`int`）
- `name` 填的是一个**多语言 key**（`text` 类型），实际显示的文字在 `Excels/Local/` 里翻译。这里填 `diamond`，再到本地化表里写 `diamond` = 钻石/钻石/ダイヤ…。
- `price` 用数字

### 第 4 步：生成代码

回到 `Config` 文件夹：

- **Windows**：双击 `gen-client-json.bat`
- **Mac / Linux**：终端里运行 `sh gen-client-json.sh`

等它跑完（看到 `pause` 或没报错就行）。

### 第 5 步：拿到结果

工具会自动在旁边的 `Unity` 文件夹里生成两个东西：

- **数据文件**（JSON）：里面是你的道具数据
- **代码文件**（C#）：里面有个 `TbMyItem` 类，就是你的道具表

### 第 6 步：在代码里用

```csharp
// 拿到 id 为 10001 的道具
var item = tables.TbMyItem.Get(10001);

// 道具名会自动变成当前语言（比如中文显示「钻石」）
Debug.Log(item.Name);
Debug.Log(item.Price); // 10
```

**完事！** 你在 Excel 里填的数据，就这样变成游戏里能直接用的代码了 ✅

## 怎么给表起名字

上面用过那个公式，这里讲全：

```
字母 - 英文名 - 中文名.xlsx
字母 - 英文名 - 分组 - 中文名.xlsx      ← 想限制只给某一端用时
```

**三段含义：**

| 段 | 是什么 | 规则 | 例子 |
|----|--------|------|------|
| **字母** | 排序用的单字母，方便找文件 | 随便一个字母或数字 | `C`、`D`、`S`、`L` |
| **英文名** | 会变成代码类名 `Tb英文名` | 只能用英文，**不能写中文** | `ItemConfig` → `TbItemConfig` |
| **中文名** | 给人看的名字 | 随便写，可加多个 `-` | `道具表`、`道具表-1001` |

**⚠️ 注意：英文名绝对不能写中文**，否则工具会报错：*"不支持中文表名"*。

**想只给客户端或服务端用？** 在英文名和中文名中间加个分组标记：

| 文件名 | 效果 |
|--------|------|
| `D-ItemConfig-道具表.xlsx` | 客户端、服务端**都用**（默认） |
| `D-ItemConfig-c-道具表.xlsx` | **只有客户端**用 |
| `D-ItemConfig-s-道具表.xlsx` | **只有服务端**用 |

> `c` = 客户端，`s` = 服务端。不加分组就两边都生成。

**现有表的名字对照：**

| 文件名 | 生成的类名 |
|--------|-----------|
| `C-AchievementConfig-成就表.xlsx` | `TbAchievementConfig` |
| `D-ItemConfig-道具表-道具-1001.xlsx` | `TbItemConfig` |
| `S-SoundsConfig-声音表.xlsx` | `TbSoundsConfig` |
| `L-Localization-成就.xlsx` | `TbLocalization` |

## 表里该怎么填

每张数据表的前 4 行是固定「表头」：

| 行 | 标记 | 填什么 |
|----|------|--------|
| 1 | `##var` | 字段名（英文，如 `id`、`name`） |
| 2 | `##type` | 字段类型（见下表） |
| 3 | `##group` | 字段分组，一般留空 |
| 4 | `##` | 中文说明，给自己和同事看 |

**常用字段类型：**

| 类型 | 意思 | 例子 |
|------|------|------|
| `int` | 整数 | `10001` |
| `string` | 普通文字（不翻译） | `icon_diamond` |
| `text` | 多语言文字（填 key，实际文字在 `Local/` 里） | `diamond` |
| `bool` | 是/否 | `true` / `false` |
| `float` | 小数 | `1.5` |
| 枚举名 | 在 `__enums__.xlsx` 里定义过的类型 | `ItemType` |

> `text` 和 `string` 的区别：`text` 是要翻译的多语言文字（填一个 key），`string` 是不翻译的普通文字（直接填内容）。

**一个填好的例子（成就表片段）：**

| ##var | id | image | name | achievement_content |
|-------|----|-------|------|---------------------|
| ##type | int | int | text | text |
| ##group | | | | |
| ## | ID | 图标id | 成就Key | 成就内容Key |
| | 900001 | 101 | achievement_001 | achievement_001_desc |

## 一张表太大了怎么办

当一张表数据特别多（比如道具上千条），可以**拆成几个文件**，工具会自动把它们合并成一张表。

**怎么拆？** 只要**英文名一样**就行，中文名随便写来区分：

```
D-ItemConfig-道具表-1-1000.xlsx      ← 第 1~1000 个道具
D-ItemConfig-道具表-1001-2000.xlsx   ← 第 1001~2000 个道具
D-ItemConfig-道具表-2001-3000.xlsx   ← 第 2001~3000 个道具
```

这三个文件的英文名都是 `ItemConfig`，工具会自动合并成一个 `TbItemConfig`。

**多语言表也是这么分的**（按模块拆）：

```
L-Localization-成就.xlsx    ┐
L-Localization-文本.xlsx    ├→ 合并成一个 TbLocalization
L-Localization-UI.xlsx      ┘
```

> 中文名里的编号、分类（比如 `1-1000`、`成就`）只是给人看的，工具不解析，你怎么方便怎么写。

## 怎么生成代码

### 先准备好

1. 装好 **.NET SDK**（工具靠它运行）
2. 在 `Config` 文件夹旁边，要有 `Unity` 和 `Server` 两个文件夹（生成的代码会放进去）

### 生成客户端（Unity）数据

- **Windows**：双击 `gen-client-json.bat`
- **Mac / Linux**：`sh gen-client-json.sh`

生成的东西去哪了：

- 数据 → `../Unity/Assets/Bundles/Config`
- 代码 → `../Unity/Assets/Hotfix/Config/Generate`

### 生成服务端（.NET）数据

- **Windows**：双击 `gen-server-bin.bat`
- **Mac / Linux**：`sh gen-server-bin.sh`

生成的东西去哪了：

- 数据 → `../Server/GameFrameX.Config/Json`
- 代码 → `../Server/GameFrameX.Config/Config`

> 四个脚本的组合：`gen-{端}-{格式}.{sh/bat}`，端 = `client`/`server`，格式 = `json`（人能读）/ `bin`（更小更快）。

## 生成的代码怎么用

**客户端（Unity）里：**

```csharp
// tables 是配置管理器，工具会自动生成
// TbItemConfig 就是你填的「道具表」，Get(id) 按id查
var item = tables.TbItemConfig.Get(10001);
Debug.Log($"名字:{item.Name}, 价格:{item.Price}");

// 遍历所有道具
foreach (var it in tables.TbItemConfig.DataList)
{
    Debug.Log(it.Name);
}
```

**服务端（.NET）里：**

```csharp
var item = tables.TbItemConfig.Get(10001);
Console.WriteLine($"{item.Name}: {item.Price}");
```

> `text` 类型的字段（如 `Name`）会自动显示成玩家当前语言，不用你手动判断语言。

## 生成的代码去了哪里

工具按「端」分别生成，互不干扰：

| 生成给谁 | 用哪个脚本 | 代码命名空间 |
|----------|-----------|-------------|
| **客户端**（Unity） | `gen-client-*` | `Hotfix.Config` |
| **服务端**（.NET） | `gen-server-*` | `GameFrameX.Config` |
| **两边都要** | 各跑一次对应脚本 | 各自的 |

> 简单记：客户端用 `client` 脚本，服务端用 `server` 脚本，需要哪端就跑哪个。

## 现在仓库里有哪些表

目前自带这些演示表：

| 表 | 文件 | 内容 |
|----|------|------|
| 成就 | `Excels/Tables/C-AchievementConfig-成就表.xlsx` | 成就定义 |
| 道具 | `Excels/Tables/D-ItemConfig-道具表-道具-1001.xlsx` | 道具定义 |
| 声音 | `Excels/Tables/S-SoundsConfig-声音表.xlsx` | 声音定义 |
| 多语言-成就 | `Excels/Local/L-Localization-成就.xlsx` | 成就的多语言文本 |
| 多语言-文本 | `Excels/Local/L-Localization-文本.xlsx` | 通用多语言文本 |
| 多语言-UI | `Excels/Local/L-Localization-UI.xlsx` | UI 的多语言文本 |

想加新表？照着「新手实战」的步骤来就行。

## 需要什么环境

- **.NET SDK** —— 运行工具用（去 [dot.net](https://dotnet.microsoft.com/) 下）
- **Excel**（或 WPS、Numbers 等能编辑 `.xlsx` 的软件）—— 填表用
- **系统** —— Windows、Mac、Linux 都行

## 开源协议

本项目基于 [Apache License 2.0](LICENSE.md) 协议开源，免费用、可商用。

## 相关链接

- [文档](https://gameframex.doc.alianblank.com)
- [GitHub 仓库](https://github.com/GameFrameX/GameFrameX.Config)
- [问题反馈](https://github.com/GameFrameX/GameFrameX.Config/issues)
- [Luban（GameFrameX 定制版）](https://github.com/GameFrameX/luban)
- [Luban（原版上游）](https://github.com/focus-creative-games/luban)
