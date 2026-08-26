<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX FairyGUI Project

[![License](https://img.shields.io/github/license/GameFrameX/GameFrameX.FairyGUIProject)](LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.FairyGUIProject)](https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases)
[![Documentation](https://img.shields.io/badge/Documentation-doc.alianblank.com-blue)](https://gameframex.doc.alianblank.com)

An all-in-one front-and-back-end solution for indie games · The dream-maker for indie game developers

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#quick-start) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## What is this project?

In plain words: **this is the "design source file" for every game screen in GameFrameX.**

Think of it as a Figma file — except instead of web design mockups, it holds game screens (login screens, main menus, inventories, loading screens…). You open it with a free tool called the **FairyGUI editor**, drag and drop to lay out a screen, then click a button and the screen gets exported into something Unity can use directly.

After exporting you get two things:

- **An art asset bundle** (a `.bytes` file): the images and animations used by the screen, loaded by Unity at runtime.
- **C# binding code**: for every button, list, and slider in the screen, a typed property is generated. That way programmers can write `loginPanel.btn_start.onClick = ...` instead of looking up widgets by string.

You don't have to write the C# by hand — the plugin generates it for you automatically.

## Quick Start

The whole flow in one breath is 4 steps:

1. **Open** `Game.fairy` in the FairyGUI editor.
2. **Edit** a screen — say, change the text of the login button.
3. **Click Publish**. Two things appear in the sibling Unity project:
   - `../Unity/Assets/Bundles/UI/*.bytes` — the art assets
   - `../Unity/Assets/Hotfix/UI/FairyGUI/.../*.cs` — the binding code
4. **Use it in Unity**: call `UILoginPanel.CreateInstance()` and the screen shows up.

Below we walk through each step with concrete examples. Let's start with prerequisites.

## Prerequisites (what you need)

| Tool | Purpose | Where to get it |
|------|---------|-----------------|
| FairyGUI editor ≥ 5.0 | The design tool to open and edit this project | https://www.fairygui.com/ |
| A Unity project | Receives the exported asset bundles and code | Place it as a sibling of this repo |

> This is not a Unity plugin package — you can't install it through Unity Package Manager. Just clone the repo and drop it in the same parent folder as your Unity project:
> ```
> git clone git@github.com:GameFrameX/GameFrameX.FairyGUIProject.git
> ```
> The folder layout looks like this:
> ```
> <workspace>/
> ├── GameFrameX.FairyGUIProject/   ← this repo (open Game.fairy here)
> └── Unity/                         ← your Unity game (receives export output)
> ```

## Step 1 — Open the project

1. Install the FairyGUI editor (5.0 or above).
2. Double-click **`Game.fairy`** in this repo.
3. Once the editor opens, the left panel shows **9 UI packages**.

> **Example:** Click `UILogin` and you'll see the login screen design: a background image, an account input, a password input, and a "Sign In" button.

The project is already pre-configured with these (usually you don't need to touch them):

- Resolution 1080 × 2160 (portrait phone), scale mode `MatchWidthOrHeight`.
- Shared fonts, colors, and scrollbars, all centralized in `settings/Common.json` and used globally.
- Atlas settings: 2048 max size, paging, power-of-two, allow rotation, trim images (`settings/Publish.json`), tuned for mobile.
- Publishing is split into three bundle groups: `UI` / `Res` / `Def` (`settings/PackageGroup.json`).

## Step 2 — Understand the packages

A **Package** is like a folder that groups a related set of screens together with the art assets they use. This project has 9 packages:

| Package | What it is | Typical screens inside |
|---------|------------|------------------------|
| `UILauncher` | Splash screen | Logo at game start |
| `UILoading` | Loading screen | Progress bar while assets load |
| `UILogin` | Login screen | Account / password / login button |
| `UIMain` | Main HUD | Top bar and menu after login |
| `UIBag` | Inventory | Item grid |
| `UIRoom` | Room / lobby | Room list, ready button |
| `UIPlayer` | Player panel | Avatar, attributes |
| `UICommon` | Common components | Buttons reused everywhere |
| `UICommonAvatar` | Common avatar | Avatar widget |

> **Tip:** The names all start with `UI` — that's not a coincidence, it's required by the publish rules (see "Naming rules" below).

## Step 3 — Edit a screen

> **Example: rename the login button.**
>
> 1. Open the `UILogin` package → double-click the `UILoginPanel` component.
> 2. Select the login button, then in the properties panel on the right, change its text from `登录` to `Sign In`.
> 3. Save (Ctrl+S). Done.

Remember: design changes here are purely visual **until you publish** — they don't affect the Unity project yet.

## Step 4 — Publish (export)

This is where the magic happens.

1. In the editor run **File → Publish** (or click the publish button on the toolbar).
2. In the publish dialog, make sure **"Generate Code"** is checked.
3. The editor writes the files into the sibling Unity project:

```
../Unity/Assets/Bundles/UI/           ← *.bytes art asset bundles
../Unity/Assets/Hotfix/UI/FairyGUI/   ← generated C# binding code
```

> **What the plugin does behind the scenes:** at publish time the code-gen plugin under `plugins/gencode/` runs. It reads every component marked "export", generates a `.cs` file per component, plus an extra `PackageXxx.cs`.

> **Note:** If a component isn't marked "export", or you forgot to check "Generate Code" when publishing, no C# is generated — this is the most common trap for newcomers (see the FAQ).

## Step 5 — What the generated C# looks like

After publishing `UILogin`, you'll get a file that looks something like this (simplified, unrelated details omitted):

```csharp
#if ENABLE_UI_FAIRYGUI
namespace Hotfix.UI
{
    public sealed partial class UILoginPanel : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UILoginPanel";

        public GButton btn_start { get; private set; }    // auto-bound
        public GTextField txt_title { get; private set; } // auto-bound

        public static UILoginPanel CreateInstance() { /* creates and returns an instance */ }

        protected override void InitView()
        {
            btn_start  = (GButton)com.GetChild("btn_start");
            txt_title  = (GTextField)com.GetChild("txt_title");
        }
    }
}
#endif
```

So a programmer in Unity can use it like this:

```csharp
var panel = UILoginPanel.CreateInstance();                   // show the login screen
panel.btn_start.onClick.Add(() => Debug.Log("Login clicked")); // fired on click
```

No string lookups, no typos — every named widget automatically becomes a typed property.

## Naming & size rules (enforced at publish)

The plugin **checks every package at publish time**. Breaking any rule stops the publish and throws an error. These rules exist to keep the generated code clean and consistent.

Each rule below comes with a "right / wrong" side-by-side and shows what the error looks like.

### Rule 1: Package names must start with `UI` and contain only letters

| ✅ Right | ❌ Wrong | Why it fails |
|----------|---------|--------------|
| `UILogin` | `Login` | No `UI` prefix |
| `UIBag` | `UI_Login` | Underscore not allowed |
| `UIPlayer` | `UI1` | Digits not allowed |

On violation the plugin reports: `包名 'xxx' 必须以'UI'开头并且只能包含字母的大写驼峰命名` (i.e. the package name must start with `UI` and use only PascalCase letters).

### Rule 2: Component names must start with `UI` and contain only letters

| ✅ Right | ❌ Wrong | Why it fails |
|----------|---------|--------------|
| `UILoginPanel` | `LoginPanel` | No `UI` prefix |
| `UIBagItem` | `UILogin_Panel` | Underscore not allowed |

### Rule 3: Component names must start with the name of their package

Components live inside a package, so they carry the package name as a prefix.

| Owning package | ✅ Right | ❌ Wrong | Why it fails |
|----------------|----------|---------|--------------|
| `UILogin` | `UILoginPanel` | `UIMainPanel` | Prefix should be `UILogin` |
| `UIBag` | `UIBagItem` | `UILoginItem` | Prefix should be `UIBag` |

### Rule 4: Member names must be all lowercase (lowercase letters + underscores)

Every control name (variable name) you assign inside a screen must be all lowercase. **Exception**: Controller is unrestricted, and the three reserved names `closeButton`, `dragArea`, and `contentArea` may also use camelCase.

| ✅ Right | ❌ Wrong | Why it fails |
|----------|---------|--------------|
| `btn_start` | `BtnStart` | Contains uppercase letters |
| `txt_title` | `txtTitle` | Contains uppercase letters |
| `list_items` | `listItems` | Contains uppercase letters |

### Rule 5: Width and height must both be even

Every exported component, and every member with art assets, must have even width and height.

| ✅ Right | ❌ Wrong | Why it fails |
|----------|---------|--------------|
| 1080 × 1920 | 1081 × 1920 | Width is odd |
| 200 × 80 | 200 × 81 | Height is odd |

> **Why even?** So that pixel-center alignment and atlas packing on mobile line up exactly, avoiding half-pixel blur.

### What the plugin does automatically (you don't have to worry about it)

- Only components marked **"export"** get factory methods `CreateInstance()` / `CreateInstanceAsync()` generated.
- Members are auto-bound by type: regular objects via `GetChild`, Controller via `GetController`, Transition via `GetTransition`; for custom components, it wraps them with `Xxx.Create(...)`.
- Cross-package custom components automatically use their original package's real type name.
- Members whose type name contains `Scene` automatically have `Dispose()` called on release.
- Generated-code namespace: default is `Hotfix.UI`; if the export path contains `Unity/Assets/Scripts`, it switches to `Unity.Startup`.
- All generated code is wrapped in `#if ENABLE_UI_FAIRYGUI` so it can be toggled on/off inside Unity.

## Troubleshooting (FAQ)

**Q: Publish errors with "package name must start with UI".**
A: Rename the package so it starts with `UI` and uses only letters, e.g. `UIBoss`.

**Q: Publish errors with "width must be even".**
A: Open the component and set both width and height to even numbers (properties panel on the right → size).

**Q: No C# code was generated after publishing.**
A: Usually one of two reasons: (1) you forgot to check "Generate Code" in the publish dialog; (2) the component wasn't marked as "export" in the editor.

**Q: I clearly named a control, but it doesn't appear in the generated code.**
A: Its name probably contains uppercase letters. Rename it to all lowercase (see Rule 4).

**Q: I want to add a new screen — what's the recipe?**
A: (1) Create a package starting with `UI`, or reuse an existing one; (2) inside the package, create a component whose name starts with `UI` + the package name; (3) give every control you'll use an all-lowercase name; (4) mark the component as "export"; (5) set width and height to even numbers; (6) publish.

## Dependencies

- FairyGUI editor ≥ 5.0 (the design tool).
- A sibling Unity project, to receive the exported asset bundles and code.
- On the Unity side you need: FairyGUI runtime, UniTask, GameFrameX (`Entity.Runtime`, `UI.Runtime`, `UI.FairyGUI.Runtime`, `Runtime`).

## Documentation & Resources

- Official docs: https://gameframex.doc.alianblank.com
- GitHub Releases: https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases
- FairyGUI official site: https://www.fairygui.com/

## Community & Support

- QQ groups: 467608841 / 233840761

## Changelog

See the full changelog at [GitHub Releases](https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases).

The initial release ships the FairyGUI project skeleton and the first batch of UI asset packages.

## License

See the [LICENSE.md](LICENSE.md) file.
