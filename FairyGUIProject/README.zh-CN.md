<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX FairyGUI Project

[![License](https://img.shields.io/github/license/GameFrameX/GameFrameX.FairyGUIProject)](LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.FairyGUIProject)](https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases)
[![Documentation](https://img.shields.io/badge/Documentation-doc.alianblank.com-blue)](https://gameframex.doc.alianblank.com)

独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#快速开始) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | **简体中文** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 这个项目是什么？

用大白话说：**这是 GameFrameX 所有游戏界面的"设计源文件"。**

你可以把它想象成一个 Figma 文件——只不过它装的不是网页设计稿，而是游戏里的界面（登录界面、主界面、背包、加载界面……）。你用一个叫 **FairyGUI 编辑器** 的免费工具打开它，拖拖拽拽画出界面，然后点一个按钮，就能把界面导出成 Unity 能直接用的东西。

导出后你会得到两样东西：

- **美术资源包**（`.bytes` 文件）：界面里用到的图片、动画，游戏运行时由 Unity 加载。
- **C# 绑定代码**：给界面里每个按钮、列表、滑动条都生成一个带类型的属性。这样程序员就能写 `loginPanel.btn_start.onClick = ...`，而不用靠字符串去找控件。

C# 代码不用你手写——插件会自动帮你生成。

## 快速开始

整个流程一口气说完就是 4 步：

1. 用 FairyGUI 编辑器**打开** `Game.fairy`。
2. **编辑**某个界面——比如把登录按钮的文字改一下。
3. **点击发布**。同级的 Unity 工程里会出现两样东西：
   - `../Unity/Assets/Bundles/UI/*.bytes` —— 美术资源
   - `../Unity/Assets/Hotfix/UI/FairyGUI/.../*.cs` —— 绑定代码
4. **在 Unity 里用**：调用 `UILoginPanel.CreateInstance()` 就能显示这个界面。

下面按每一步给出具体例子。先看准备工作。

## 准备工作（你需要装什么）

| 工具 | 用途 | 哪里获取 |
|------|------|----------|
| FairyGUI 编辑器 ≥ 5.0 | 打开并编辑本工程的设计工具 | https://www.fairygui.com/ |
| 一个 Unity 工程 | 接收导出的资源包和代码 | 放在本仓库的同级目录 |

> 这不是 Unity 插件包，不能用 Unity Package Manager 安装。把仓库克隆下来，和你的 Unity 工程放在同一个父目录里就行：
> ```
> git clone git@github.com:GameFrameX/GameFrameX.FairyGUIProject.git
> ```
> 目录结构长这样：
> ```
> <workspace>/
> ├── GameFrameX.FairyGUIProject/   ← 本仓库（在这里打开 Game.fairy）
> └── Unity/                         ← 你的 Unity 游戏（接收导出产物）
> ```

## 第 1 步：打开工程

1. 安装 FairyGUI 编辑器（5.0 或以上）。
2. 双击本仓库里的 **`Game.fairy`**。
3. 编辑器打开后，左侧面板能看到 **9 个 UI 包**。

> **示例：** 点击 `UILogin`，你会看到登录界面的设计：一张背景图、一个账号输入框、一个密码输入框，还有一个"登录"按钮。

工程已经预先配置好这些（一般不用改）：

- 分辨率 1080 × 2160（竖屏手机），缩放模式 `MatchWidthOrHeight`。
- 统一的字体、配色、滚动条，集中写在 `settings/Common.json`，全局共用。
- 图集设置：2048 上限、分页、2 的幂、允许旋转、裁剪图像（`settings/Publish.json`），针对移动端优化。
- 发布时分成 `UI` / `Res` / `Def` 三个分包组（`settings/PackageGroup.json`）。

## 第 2 步：认识 UI 包

一个**包（Package）** 就像一个文件夹，把一组相关的界面和它们用到的美术资源装在一起。本工程有 9 个包：

| 包 | 是什么 | 里面的典型界面 |
|----|--------|----------------|
| `UILauncher` | 启动闪屏 | 游戏启动时的 Logo |
| `UILoading` | 加载界面 | 加载资源时的进度条 |
| `UILogin` | 登录界面 | 账号 / 密码 / 登录按钮 |
| `UIMain` | 主界面 HUD | 登录后的顶栏和菜单 |
| `UIBag` | 背包 | 物品网格 |
| `UIRoom` | 房间 / 大厅 | 房间列表、准备按钮 |
| `UIPlayer` | 玩家面板 | 头像、属性 |
| `UICommon` | 通用组件 | 到处复用的按钮等 |
| `UICommonAvatar` | 通用头像 | 头像控件 |

> **小贴士：** 名字都以 `UI` 开头不是巧合——这是发布规则要求的（见后面的"命名规则"）。

## 第 3 步：编辑一个界面

> **示例：把登录按钮改个名字。**
>
> 1. 打开 `UILogin` 包 → 双击 `UILoginPanel` 组件。
> 2. 选中那个登录按钮，在右侧属性面板把它的文字从 `登录` 改成 `Sign In`。
> 3. 保存（Ctrl+S）。改好了。

记住：这里的设计改动在**发布之前**只是视觉上的，还不会影响 Unity 工程。

## 第 4 步：发布（导出）

这是见证奇迹的一步。

1. 在编辑器里执行 **文件 → 发布**（或点工具栏的发布按钮）。
2. 在发布对话框里，确保勾选了 **「生成代码」**。
3. 编辑器会把文件写到同级的 Unity 工程里：

```
../Unity/Assets/Bundles/UI/           ← *.bytes 美术资源包
../Unity/Assets/Hotfix/UI/FairyGUI/   ← 生成的 C# 绑定代码
```

> **背后的插件做了什么：** 发布时会运行 `plugins/gencode/` 下的代码生成插件。它读取每个标记为「导出」的组件，给每个组件生成一个 `.cs` 文件，再额外生成一个 `PackageXxx.cs`。

> **注意：** 如果组件没标记「导出」，或者发布时没勾「生成代码」，就不会生成 C# 代码——这是新人最常踩的坑（见 FAQ）。

## 第 5 步：生成的 C# 代码长什么样

发布 `UILogin` 之后，你会得到一个类似下面这样的文件（已简化，省略了无关细节）：

```csharp
#if ENABLE_UI_FAIRYGUI
namespace Hotfix.UI
{
    public sealed partial class UILoginPanel : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UILoginPanel";

        public GButton btn_start { get; private set; }    // 自动绑定
        public GTextField txt_title { get; private set; } // 自动绑定

        public static UILoginPanel CreateInstance() { /* 创建并返回实例 */ }

        protected override void InitView()
        {
            btn_start  = (GButton)com.GetChild("btn_start");
            txt_title  = (GTextField)com.GetChild("txt_title");
        }
    }
}
#endif
```

于是 Unity 里的程序员就可以这样用：

```csharp
var panel = UILoginPanel.CreateInstance();          // 显示登录界面
panel.btn_start.onClick.Add(() => Debug.Log("点了登录")); // 点按钮时触发
```

不用查字符串、不会拼错——每个命名的控件都自动变成一个带类型的属性。

## 命名与尺寸规则（发布时强制检查）

插件在**发布时会逐个包检查**，违反任何一条都会让发布停下来并报错。这些规则是为了让生成的代码干净、统一。

下面每条都给出"正确 / 错误"对照，并说明报错长什么样。

### 规则 1：包名必须以 `UI` 开头，且只含字母

| ✅ 正确 | ❌ 错误 | 错在哪 |
|--------|--------|--------|
| `UILogin` | `Login` | 没有 `UI` 前缀 |
| `UIBag` | `UI_Login` | 不能有下划线 |
| `UIPlayer` | `UI1` | 不能有数字 |

违反时报错：`包名 'xxx' 必须以'UI'开头并且只能包含字母的大写驼峰命名`。

### 规则 2：组件名必须以 `UI` 开头，且只含字母

| ✅ 正确 | ❌ 错误 | 错在哪 |
|--------|--------|--------|
| `UILoginPanel` | `LoginPanel` | 没有 `UI` 前缀 |
| `UIBagItem` | `UILogin_Panel` | 不能有下划线 |

### 规则 3：组件名必须以它所属的包名开头

组件住在某个包里，所以名字要带上包名做前缀。

| 所在包 | ✅ 正确 | ❌ 错误 | 错在哪 |
|--------|--------|--------|--------|
| `UILogin` | `UILoginPanel` | `UIMainPanel` | 前缀应该是 `UILogin` |
| `UIBag` | `UIBagItem` | `UILoginItem` | 前缀应该是 `UIBag` |

### 规则 4：成员名必须全小写（小写字母 + 下划线）

界面里你给每个控件起的名字（变量名）必须全小写。**例外**：Controller 不受限制，以及三个保留名 `closeButton`、`dragArea`、`contentArea` 也可以用驼峰。

| ✅ 正确 | ❌ 错误 | 错在哪 |
|--------|--------|--------|
| `btn_start` | `BtnStart` | 含大写字母 |
| `txt_title` | `txtTitle` | 含大写字母 |
| `list_items` | `listItems` | 含大写字母 |

### 规则 5：宽和高都必须是偶数

每个导出的组件、以及每个带美术资源的成员，宽和高都必须是偶数。

| ✅ 正确 | ❌ 错误 | 错在哪 |
|--------|--------|--------|
| 1080 × 1920 | 1081 × 1920 | 宽是奇数 |
| 200 × 80 | 200 × 81 | 高是奇数 |

> **为什么要偶数？** 为了让移动端的像素居中、图集打包都能精确对齐，避免出现半像素模糊。

### 插件还会自动做这些事（不用你管）

- 只有标记为 **「导出」** 的组件才会生成工厂方法 `CreateInstance()` / `CreateInstanceAsync()`。
- 成员按类型自动绑定：普通对象用 `GetChild`，Controller 用 `GetController`，Transition 用 `GetTransition`；如果是自定义组件，会用 `Xxx.Create(...)` 包一层。
- 跨包的自定义组件，会自动用回它原始包里的真实类型名。
- 类型名里带 `Scene` 的成员，释放时会自动调用它的 `Dispose()`。
- 生成代码的命名空间：默认是 `Hotfix.UI`；如果导出路径里包含 `Unity/Assets/Scripts`，则自动改成 `Unity.Startup`。
- 所有生成代码都用 `#if ENABLE_UI_FAIRYGUI` 包起来，方便在 Unity 里开关。

## 常见问题（FAQ）

**Q：发布时报错"包名必须以 UI 开头"。**
A：把包名改成以 `UI` 开头、且只用字母，例如 `UIBoss`。

**Q：发布时报错"宽度必须为偶数"。**
A：打开那个组件，把宽 / 高都设成偶数（右侧属性面板 → 尺寸）。

**Q：发布后没有生成 C# 代码。**
A：多半是两个原因之一：(1) 发布对话框里忘了勾「生成代码」；(2) 在编辑器里没把这个组件标记为「导出」。

**Q：我明明命名了一个控件，但它没出现在生成的代码里。**
A：它的名字可能含大写字母。改成全小写（见规则 4）。

**Q：我想新增一个界面，该怎么做？**
A：(1) 新建一个以 `UI` 开头的包，或用现有包；(2) 在包里新建组件，名字以 `UI` + 包名开头；(3) 给要用到的控件起全小写的名字；(4) 把组件标记为「导出」；(5) 宽高设成偶数；(6) 发布。

## 依赖

- FairyGUI 编辑器 ≥ 5.0（设计工具）。
- 一个同级 Unity 工程，用来接收导出的资源包和代码。
- Unity 侧需要：FairyGUI 运行时、UniTask、GameFrameX（`Entity.Runtime`、`UI.Runtime`、`UI.FairyGUI.Runtime`、`Runtime`）。

## 文档与资源

- 官方文档：https://gameframex.doc.alianblank.com
- GitHub Releases：https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases
- FairyGUI 官网：https://www.fairygui.com/

## 社区与支持

- QQ 群：467608841 / 233840761

## 更新日志

完整更新日志见 [GitHub Releases](https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases)。

首发版本包含 FairyGUI 工程骨架与首批 UI 资产包。

## 开源协议

详见 [LICENSE.md](LICENSE.md) 文件。
