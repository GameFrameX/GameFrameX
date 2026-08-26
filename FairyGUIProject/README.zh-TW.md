<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX FairyGUI Project

[![License](https://img.shields.io/github/license/GameFrameX/GameFrameX.FairyGUIProject)](LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.FairyGUIProject)](https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases)
[![Documentation](https://img.shields.io/badge/Documentation-doc.alianblank.com-blue)](https://gameframex.doc.alianblank.com)

獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使

<br />

[文檔](https://gameframex.doc.alianblank.com) · [快速開始](#快速開始) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 這個專案是什麼？

用大白話說：**這是 GameFrameX 所有遊戲介面的「設計原始檔」。**

你可以把它想像成一個 Figma 檔案——只不過它裝的不是網頁設計稿，而是遊戲裡的介面（登入介面、主介面、背包、載入介面……）。你用一個叫 **FairyGUI 編輯器** 的免費工具打開它，拖拖拽拽畫出介面，然後點一個按鈕，就能把介面匯出成 Unity 能直接用的東西。

匯出後你會得到兩樣東西：

- **美術資源包**（`.bytes` 檔案）：介面裡用到的圖片、動畫，遊戲執行時由 Unity 載入。
- **C# 繫結程式碼**：給介面裡每個按鈕、列表、滑桿都生成一個帶型別的屬性。這樣程式設計師就能寫 `loginPanel.btn_start.onClick = ...`，而不用靠字串去找控制項。

C# 程式碼不用你手寫——插件會自動幫你生成。

## 快速開始

整個流程一口氣說完就是 4 步：

1. 用 FairyGUI 編輯器**打開** `Game.fairy`。
2. **編輯**某個介面——比如把登入按鈕的文字改一下。
3. **點擊發布**。同級的 Unity 專案裡會出現兩樣東西：
   - `../Unity/Assets/Bundles/UI/*.bytes` —— 美術資源
   - `../Unity/Assets/Hotfix/UI/FairyGUI/.../*.cs` —— 繫結程式碼
4. **在 Unity 裡用**：呼叫 `UILoginPanel.CreateInstance()` 就能顯示這個介面。

下面按每一步給出具體例子。先看準備工作。

## 準備工作（你需要裝什麼）

| 工具 | 用途 | 哪裡取得 |
|------|------|----------|
| FairyGUI 編輯器 ≥ 5.0 | 打開並編輯本專案的設計工具 | https://www.fairygui.com/ |
| 一個 Unity 專案 | 接收匯出的資源包和程式碼 | 放在本倉庫的同級目錄 |

> 這不是 Unity 插件包，不能用 Unity Package Manager 安裝。把倉庫克隆下來，和你的 Unity 專案放在同一個父目錄裡就行：
> ```
> git clone git@github.com:GameFrameX/GameFrameX.FairyGUIProject.git
> ```
> 目錄結構長這樣：
> ```
> <workspace>/
> ├── GameFrameX.FairyGUIProject/   ← 本倉庫（在這裡打開 Game.fairy）
> └── Unity/                         ← 你的 Unity 遊戲（接收匯出產物）
> ```

## 第 1 步：打開專案

1. 安裝 FairyGUI 編輯器（5.0 或以上）。
2. 雙擊本倉庫裡的 **`Game.fairy`**。
3. 編輯器打開後，左側面板能看到 **9 個 UI 包**。

> **示例：** 點擊 `UILogin`，你會看到登入介面的設計：一張背景圖、一個帳號輸入框、一個密碼輸入框，還有一個「登入」按鈕。

專案已經預先設定好這些（一般不用改）：

- 解析度 1080 × 2160（豎屏手機），縮放模式 `MatchWidthOrHeight`。
- 統一的字體、配色、捲動條，集中寫在 `settings/Common.json`，全域共用。
- 圖集設定：2048 上限、分頁、2 的冪、允許旋轉、裁剪圖像（`settings/Publish.json`），針對行動端最佳化。
- 發布時分成 `UI` / `Res` / `Def` 三個分包組（`settings/PackageGroup.json`）。

## 第 2 步：認識 UI 包

一個**包**就像一個資料夾，把一組相關的介面和它們用到的美術資源裝在一起。本專案有 9 個包：

| 包 | 是什麼 | 裡面的典型介面 |
|----|--------|----------------|
| `UILauncher` | 啟動閃屏 | 遊戲啟動時的 Logo |
| `UILoading` | 載入介面 | 載入資源時的進度條 |
| `UILogin` | 登入介面 | 帳號 / 密碼 / 登入按鈕 |
| `UIMain` | 主介面 HUD | 登入後的頂欄和選單 |
| `UIBag` | 背包 | 物品網格 |
| `UIRoom` | 房間 / 大廳 | 房間列表、準備按鈕 |
| `UIPlayer` | 玩家面板 | 頭像、屬性 |
| `UICommon` | 通用元件 | 到處複用的按鈕等 |
| `UICommonAvatar` | 通用頭像 | 頭像控制項 |

> **小秘訣：** 名字都以 `UI` 開頭不是巧合——這是發布規則要求的（見後面的「命名規則」）。

## 第 3 步：編輯一個介面

> **示例：把登入按鈕改個名字。**
>
> 1. 打開 `UILogin` 包 → 雙擊 `UILoginPanel` 元件。
> 2. 選取那個登入按鈕，在右側屬性面板把它的文字從 `登录` 改成 `Sign In`。
> 3. 儲存（Ctrl+S）。改好了。

記住：這裡的設計改動在**發布之前**只是視覺上的，還不會影響 Unity 專案。

## 第 4 步：發布（匯出）

這是見證奇蹟的一步。

1. 在編輯器裡執行 **檔案 → 發布**（或點工具列的發布按鈕）。
2. 在發布對話框裡，確保勾選了 **「生成程式碼」**。
3. 編輯器會把檔案寫到同級的 Unity 專案裡：

```
../Unity/Assets/Bundles/UI/           ← *.bytes 美術資源包
../Unity/Assets/Hotfix/UI/FairyGUI/   ← 生成的 C# 繫結程式碼
```

> **背後的插件做了什麼：** 發布時會執行 `plugins/gencode/` 下的程式碼生成插件。它讀取每個標記為「匯出」的元件，給每個元件生成一個 `.cs` 檔案，再額外生成一個 `PackageXxx.cs`。

> **注意：** 如果元件沒標記「匯出」，或者發布時沒勾「生成程式碼」，就不會生成 C# 程式碼——這是新人最常踩的坑（見 FAQ）。

## 第 5 步：生成的 C# 程式碼長什麼樣

發布 `UILogin` 之後，你會得到一個類似下面這樣的檔案（已簡化，省略了無關細節）：

```csharp
#if ENABLE_UI_FAIRYGUI
namespace Hotfix.UI
{
    public sealed partial class UILoginPanel : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UILoginPanel";

        public GButton btn_start { get; private set; }    // 自動繫結
        public GTextField txt_title { get; private set; } // 自動繫結

        public static UILoginPanel CreateInstance() { /* 建立並回傳實例 */ }

        protected override void InitView()
        {
            btn_start  = (GButton)com.GetChild("btn_start");
            txt_title  = (GTextField)com.GetChild("txt_title");
        }
    }
}
#endif
```

於是 Unity 裡的程式設計師就可以這樣用：

```csharp
var panel = UILoginPanel.CreateInstance();          // 顯示登入介面
panel.btn_start.onClick.Add(() => Debug.Log("點了登入")); // 點按鈕時觸發
```

不用查字串、不會拼錯——每個命名的控制項都自動變成一個帶型別的屬性。

## 命名與尺寸規則（發布時強制檢查）

插件在**發布時會逐個包檢查**，違反任何一條都會讓發布停下來並報錯。這些規則是為了讓生成的程式碼乾淨、統一。

下面每條都給出「正確 / 錯誤」對照，並說明報錯長什麼樣。

### 規則 1：包名必須以 `UI` 開頭，且只含字母

| ✅ 正確 | ❌ 錯誤 | 錯在哪 |
|--------|--------|--------|
| `UILogin` | `Login` | 沒有 `UI` 前綴 |
| `UIBag` | `UI_Login` | 不能有底線 |
| `UIPlayer` | `UI1` | 不能有數字 |

違反時報錯：`包名 'xxx' 必须以'UI'开头并且只能包含字母的大写驼峰命名`。

### 規則 2：元件名必須以 `UI` 開頭，且只含字母

| ✅ 正確 | ❌ 錯誤 | 錯在哪 |
|--------|--------|--------|
| `UILoginPanel` | `LoginPanel` | 沒有 `UI` 前綴 |
| `UIBagItem` | `UILogin_Panel` | 不能有底線 |

### 規則 3：元件名必須以它所屬的包名開頭

元件住在某個包裡，所以名字要帶上包名做前綴。

| 所在包 | ✅ 正確 | ❌ 錯誤 | 錯在哪 |
|--------|--------|--------|--------|
| `UILogin` | `UILoginPanel` | `UIMainPanel` | 前綴應該是 `UILogin` |
| `UIBag` | `UIBagItem` | `UILoginItem` | 前綴應該是 `UIBag` |

### 規則 4：成員名必須全小寫（小寫字母 + 底線）

介面裡你給每個控制項起的名字（變數名）必須全小寫。**例外**：Controller 不受限制，以及三個保留名 `closeButton`、`dragArea`、`contentArea` 也可以用駝峰。

| ✅ 正確 | ❌ 錯誤 | 錯在哪 |
|--------|--------|--------|
| `btn_start` | `BtnStart` | 含大寫字母 |
| `txt_title` | `txtTitle` | 含大寫字母 |
| `list_items` | `listItems` | 含大寫字母 |

### 規則 5：寬和高都必須是偶數

每個匯出的元件、以及每個帶美術資源的成員，寬和高都必須是偶數。

| ✅ 正確 | ❌ 錯誤 | 錯在哪 |
|--------|--------|--------|
| 1080 × 1920 | 1081 × 1920 | 寬是奇數 |
| 200 × 80 | 200 × 81 | 高是奇數 |

> **為什麼要偶數？** 為了讓行動端的像素居中、圖集打包都能精確對齊，避免出現半像素模糊。

### 插件還會自動做這些事（不用你管）

- 只有標記為 **「匯出」** 的元件才會生成工廠方法 `CreateInstance()` / `CreateInstanceAsync()`。
- 成員按型別自動繫結：普通物件用 `GetChild`，Controller 用 `GetController`，Transition 用 `GetTransition`；如果是自訂元件，會用 `Xxx.Create(...)` 包一層。
- 跨包的自訂元件，會自動用回它原始包裡的真實型別名。
- 型別名裡帶 `Scene` 的成員，釋放時會自動呼叫它的 `Dispose()`。
- 生成程式碼的命名空間：預設是 `Hotfix.UI`；如果匯出路徑裡包含 `Unity/Assets/Scripts`，則自動改成 `Unity.Startup`。
- 所有生成程式碼都用 `#if ENABLE_UI_FAIRYGUI` 包起來，方便在 Unity 裡開關。

## 常見問題（FAQ）

**Q：發布時報錯「包名必須以 UI 開頭」。**
A：把包名改成以 `UI` 開頭、且只用字母，例如 `UIBoss`。

**Q：發布時報錯「寬度必須為偶數」。**
A：打開那個元件，把寬 / 高都設成偶數（右側屬性面板 → 尺寸）。

**Q：發布後沒有生成 C# 程式碼。**
A：多半是兩個原因之一：(1) 發布對話框裡忘了勾「生成程式碼」；(2) 在編輯器裡沒把這個元件標記為「匯出」。

**Q：我明明命名了一個控制項，但它沒出現在生成的程式碼裡。**
A：它的名字可能含大寫字母。改成全小寫（見規則 4）。

**Q：我想新增一個介面，該怎麼做？**
A：(1) 新建一個以 `UI` 開頭的包，或用現有包；(2) 在包裡新建元件，名字以 `UI` + 包名開頭；(3) 給要用到的控制項起全小寫的名字；(4) 把元件標記為「匯出」；(5) 寬高設成偶數；(6) 發布。

## 依賴

- FairyGUI 編輯器 ≥ 5.0（設計工具）。
- 一個同級 Unity 專案，用來接收匯出的資源包和程式碼。
- Unity 側需要：FairyGUI 執行階段、UniTask、GameFrameX（`Entity.Runtime`、`UI.Runtime`、`UI.FairyGUI.Runtime`、`Runtime`）。

## 文檔與資源

- 官方文檔：https://gameframex.doc.alianblank.com
- GitHub Releases：https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases
- FairyGUI 官網：https://www.fairygui.com/

## 社群與支援

- QQ 群：467608841 / 233840761

## 更新日誌

完整更新日誌見 [GitHub Releases](https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases)。

首發版本包含 FairyGUI 專案骨架與首批 UI 資源包。

## 開源協議

詳見 [LICENSE.md](LICENSE.md) 檔案。
