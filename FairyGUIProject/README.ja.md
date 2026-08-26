<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX FairyGUI Project

[![License](https://img.shields.io/github/license/GameFrameX/GameFrameX.FairyGUIProject)](LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.FairyGUIProject)](https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases)
[![Documentation](https://img.shields.io/badge/Documentation-doc.alianblank.com-blue)](https://gameframex.doc.alianblank.com)

インディーゲームのフロント＆バックエンド統合ソリューション · インディーゲーム開発者の夢を応援する存在

<br />

[ドキュメント](https://gameframex.doc.alianblank.com) · [クイックスタート](#クイックスタート) · QQグループ: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **日本語** | [한국어](README.ko.md)

</div>

## このプロジェクトは何？

わかりやすく言うと：**これは GameFrameX のすべてのゲーム画面の「デザインソースファイル」です。**

Figma のファイルみたいなものだと考えてください——ただし入っているのはウェブのデザインデータではなく、ゲームの画面（ログイン画面、メイン画面、バックパック、ローディング画面……）です。**FairyGUI エディタ** という無料ツールで開き、ドラッグ＆ドロップで画面を作り、ボタンをワンクリックすれば、Unity でそのまま使える形にエクスポートできます。

エクスポートすると、2 つのものが手に入ります：

- **美術リソースパッケージ**（`.bytes` ファイル）：画面で使われる画像やアニメーションで、ゲーム実行時に Unity が読み込みます。
- **C# バインディングコード**：画面内の各ボタン、リスト、スライダーに対して型付きのプロパティを生成します。これにより、プログラマは `loginPanel.btn_start.onClick = ...` と書けて、文字列でコントロールを探す必要がなくなります。

C# コードは自分で書く必要はありません——プラグインが自動で生成してくれます。

## クイックスタート

全体の流れを一言で言うと、4 ステップです：

1. FairyGUI エディタで `Game.fairy` を**開く**。
2. どこかの画面を**編集する**——例えばログインボタンのテキストを変えるとか。
3. **公開をクリック**。同じ階層にある Unity プロジェクトに 2 つのものが現れます：
   - `../Unity/Assets/Bundles/UI/*.bytes` —— 美術リソース
   - `../Unity/Assets/Hotfix/UI/FairyGUI/.../*.cs` —— バインディングコード
4. **Unity で使う**：`UILoginPanel.CreateInstance()` を呼べばその画面を表示できます。

では、各ステップの具体的な例を見ていきましょう。まずは準備から。

## 準備するもの（必要なツール）

| ツール | 用途 | 入手先 |
|------|------|----------|
| FairyGUI エディタ ≥ 5.0 | 本プロジェクトを開いて編集するデザインツール | https://www.fairygui.com/ |
| Unity プロジェクト | エクスポートされたリソースパッケージとコードを受け取る | 本リポジトリと同じ階層に配置 |

> これは Unity のプラグインパッケージではなく、Unity Package Manager ではインストールできません。リポジトリをクローンして、あなたの Unity プロジェクトと同じ親ディレクトリに置けば OK です：
> ```
> git clone git@github.com:GameFrameX/GameFrameX.FairyGUIProject.git
> ```
> ディレクトリ構造はこうなります：
> ```
> <workspace>/
> ├── GameFrameX.FairyGUIProject/   ← 本リポジトリ（ここで Game.fairy を開く）
> └── Unity/                         ← あなたの Unity ゲーム（エクスポート結果を受け取る）
> ```

## ステップ 1：プロジェクトを開く

1. FairyGUI エディタ（5.0 以上）をインストールします。
2. 本リポジトリの **`Game.fairy`** をダブルクリックします。
3. エディタが開いたら、左側のパネルに **9 個の UI パッケージ** が表示されます。

> **例：** `UILogin` をクリックすると、ログイン画面のデザインが見えます：背景画像 1 枚、アカウント入力ボックス、パスワード入力ボックス、そして「ログイン」ボタン。

プロジェクトにはあらかじめ以下が設定されています（基本的には変更不要）：

- 解像度 1080 × 2160（縦画面スマホ）、スケールモードは `MatchWidthOrHeight`。
- フォント、配色、スクロールバーを一元管理し、`settings/Common.json` にまとめて書かれ、全体で共有されます。
- アトラス設定：2048 上限、ページ分割、2 の累乗、回転許可、画像トリミング（`settings/Publish.json`）、モバイル向けに最適化。
- 公開時に `UI` / `Res` / `Def` の 3 つのパッケージグループに分けます（`settings/PackageGroup.json`）。

## ステップ 2：UI パッケージを知る

**パッケージ（Package）** はフォルダのようなもので、関連する一連の画面と、その画面で使う美術リソースをひとまとめにします。本プロジェクトには 9 個のパッケージがあります：

| パッケージ | 内容 | 典型的な画面 |
|----|--------|----------------|
| `UILauncher` | 起動スプラッシュ | ゲーム起動時のロゴ |
| `UILoading` | ローディング画面 | リソース読み込み中のプログレスバー |
| `UILogin` | ログイン画面 | アカウント / パスワード / ログインボタン |
| `UIMain` | メイン画面 HUD | ログイン後のトップバーとメニュー |
| `UIBag` | バックパック | アイテムグリッド |
| `UIRoom` | ルーム / ロビー | ルームリスト、準備ボタン |
| `UIPlayer` | プレイヤーパネル | アバター、属性 |
| `UICommon` | 汎用コンポーネント | あちこちで再利用されるボタンなど |
| `UICommonAvatar` | 汎用アバター | アバターコントロール |

> **ヒント：** 名前がどれも `UI` で始まっているのは偶然ではありません——これは公開ルールで決められています（後述の「命名ルール」を参照）。

## ステップ 3：画面を編集する

> **例：ログインボタンの名前を変えてみる。**
>
> 1. `UILogin` パッケージを開く → `UILoginPanel` コンポーネントをダブルクリック。
> 2. ログインボタンを選択し、右側のプロパティパネルでテキストを `登录` から `Sign In` に変更。
> 3. 保存（Ctrl+S）。これで完了。

覚えておいてほしいのは、ここのデザイン変更は**公開する前**は視覚的なだけで、Unity プロジェクトにはまだ影響しないということです。

## ステップ 4：公開（エクスポート）

いよいよ魔法の瞬間です。

1. エディタで **ファイル → 公開** を実行します（ツールバーの公開ボタンでも OK）。
2. 公開ダイアログで、**「コード生成」** にチェックが入っていることを確認します。
3. エディタが同じ階層の Unity プロジェクトにファイルを書き出します：

```
../Unity/Assets/Bundles/UI/           ← *.bytes 美術リソースパッケージ
../Unity/Assets/Hotfix/UI/FairyGUI/   ← 生成された C# バインディングコード
```

> **バックグラウンドでプラグインがやっていること：** 公開時に `plugins/gencode/` のコード生成プラグインが走ります。「エクスポート」とマークされた各コンポーネントを読み取り、コンポーネントごとに `.cs` ファイルを生成し、さらに `PackageXxx.cs` を 1 つ追加で生成します。

> **注意：** コンポーネントに「エクスポート」マークがない、または公開時に「コード生成」のチェックを外していると、C# コードは生成されません——これが新人が一番よくハマる罠です（FAQ を参照）。

## ステップ 5：生成された C# コードの姿

`UILogin` を公開すると、以下のようなファイルが得られます（分かりやすくするため、関係ない細部は省略しています）：

```csharp
#if ENABLE_UI_FAIRYGUI
namespace Hotfix.UI
{
    public sealed partial class UILoginPanel : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UILoginPanel";

        public GButton btn_start { get; private set; }    // 自動バインド
        public GTextField txt_title { get; private set; } // 自動バインド

        public static UILoginPanel CreateInstance() { /* インスタンスを生成して返す */ }

        protected override void InitView()
        {
            btn_start  = (GButton)com.GetChild("btn_start");
            txt_title  = (GTextField)com.GetChild("txt_title");
        }
    }
}
#endif
```

こうして Unity 側のプログラマは、次のように使えます：

```csharp
var panel = UILoginPanel.CreateInstance();             // ログイン画面を表示
panel.btn_start.onClick.Add(() => Debug.Log("Login clicked")); // クリック時に発火
```

文字列で探す必要も、スペルミスの心配もありません——名前付きのコントロールは、すべて自動的に型付きプロパティになります。

## 命名とサイズのルール（公開時に強制チェック）

プラグインは**公開時にパッケージごとにチェックを行い**、どれか 1 つでもルール違反があれば公開を止めてエラーを出します。これらのルールは、生成されるコードをきれいで統一されたものにするためのものです。

以下、各ルールについて「OK / NG」の対照例と、エラーの見え方を示します。

### ルール 1：パッケージ名は `UI` で始まり、英字のみ

| ✅ OK | ❌ NG | どこがダメか |
|--------|--------|--------|
| `UILogin` | `Login` | `UI` のプレフィックスがない |
| `UIBag` | `UI_Login` | アンダースコアは使えない |
| `UIPlayer` | `UI1` | 数字は使えない |

違反時のエラー：`包名 'xxx' 必须以'UI'开头并且只能包含字母的大写驼峰命名`。

### ルール 2：コンポーネント名は `UI` で始まり、英字のみ

| ✅ OK | ❌ NG | どこがダメか |
|--------|--------|--------|
| `UILoginPanel` | `LoginPanel` | `UI` のプレフィックスがない |
| `UIBagItem` | `UILogin_Panel` | アンダースコアは使えない |

### ルール 3：コンポーネント名は、それが所属するパッケージ名で始まる

コンポーネントはいずれかのパッケージに属しているので、名前にパッケージ名をプレフィックスとして付けます。

| 所属パッケージ | ✅ OK | ❌ NG | どこがダメか |
|--------|--------|--------|--------|
| `UILogin` | `UILoginPanel` | `UIMainPanel` | プレフィックスは `UILogin` であるべき |
| `UIBag` | `UIBagItem` | `UILoginItem` | プレフィックスは `UIBag` であるべき |

### ルール 4：メンバー名はすべて小文字（小文字アルファベット + アンダースコア）

画面内で各コントロールにつける名前（変数名）はすべて小文字にします。**例外**：Controller は制限なく、また 3 つの予約名 `closeButton`、`dragArea`、`contentArea` もキャメルケースで OK です。

| ✅ OK | ❌ NG | どこがダメか |
|--------|--------|--------|
| `btn_start` | `BtnStart` | 大文字を含んでいる |
| `txt_title` | `txtTitle` | 大文字を含んでいる |
| `list_items` | `listItems` | 大文字を含んでいる |

### ルール 5：幅と高さはどちらも偶数でなければならない

エクスポート対象の各コンポーネント、および美術リソースを持つ各メンバーについて、幅と高さは偶数でなければなりません。

| ✅ OK | ❌ NG | どこがダメか |
|--------|--------|--------|
| 1080 × 1920 | 1081 × 1920 | 幅が奇数 |
| 200 × 80 | 200 × 81 | 高さが奇数 |

> **なぜ偶数？** モバイルでのピクセル中央揃えやアトラスのパッキングが正確にアラインでき、半ピクセルのぼやけを防ぐためです。

### プラグインが自動でやること（意識しなくて OK）

- **「エクスポート」** とマークされたコンポーネントだけが、ファクトリメソッド `CreateInstance()` / `CreateInstanceAsync()` を生成します。
- メンバーは型に応じて自動的にバインドされます：通常のオブジェクトは `GetChild`、Controller は `GetController`、Transition は `GetTransition` を使います。カスタムコンポーネントの場合は `Xxx.Create(...)` でラップします。
- パッケージをまたぐカスタムコンポーネントは、自動的に元のパッケージでの実際の型名を使います。
- 型名に `Scene` を含むメンバーは、解放時に自動的にその `Dispose()` を呼び出します。
- 生成コードの名前空間：デフォルトは `Hotfix.UI`。エクスポート先のパスに `Unity/Assets/Scripts` が含まれている場合は、自動的に `Unity.Startup` に切り替わります。
- 生成コードはすべて `#if ENABLE_UI_FAIRYGUI` で囲まれ、Unity 側でオン/オフしやすくなっています。

## よくある質問（FAQ）

**Q：公開時に「パッケージ名は UI で始まらなければならない」というエラーが出る。**
A：パッケージ名を `UI` 始まりにし、英字だけを使ってください。例：`UIBoss`。

**Q：公開時に「幅は偶数でなければならない」というエラーが出る。**
A：そのコンポーネントを開き、幅 / 高さを偶数に設定してください（右側のプロパティパネル → サイズ）。

**Q：公開しても C# コードが生成されない。**
A：だいたい次のどちらかが原因です：(1) 公開ダイアログで「コード生成」のチェックを入れ忘れた；(2) エディタでそのコンポーネントに「エクスポート」マークをつけていない。

**Q：コントロールに名前をつけたのに、生成されたコードに出てこない。**
A：名前に大文字が含まれているかもしれません。すべて小文字にしてください（ルール 4 を参照）。

**Q：新しい画面を追加したい場合は？**
A：(1) `UI` で始まる新しいパッケージを作るか、既存のパッケージを使います；(2) パッケージ内に新しいコンポーネントを作り、名前は `UI` + パッケージ名で始めます；(3) 使うコントロールにはすべて小文字の名前をつけます；(4) コンポーネントに「エクスポート」マークをつけます；(5) 幅と高さを偶数にします；(6) 公開。

## 依存関係

- FairyGUI エディタ ≥ 5.0（デザインツール）。
- 同じ階層にある Unity プロジェクト（エクスポートされたリソースパッケージとコードを受け取る用）。
- Unity 側に必要なもの：FairyGUI ランタイム、UniTask、GameFrameX（`Entity.Runtime`、`UI.Runtime`、`UI.FairyGUI.Runtime`、`Runtime`）。

## ドキュメントとリソース

- 公式ドキュメント：https://gameframex.doc.alianblank.com
- GitHub Releases：https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases
- FairyGUI 公式サイト：https://www.fairygui.com/

## コミュニティとサポート

- QQ グループ：467608841 / 233840761

## 変更履歴

完全な変更履歴は [GitHub Releases](https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases) を参照してください。

初版には、FairyGUI プロジェクトの骨組みと最初の UI アセットパッケージが含まれています。

## ライセンス

詳しくは [LICENSE.md](LICENSE.md) を参照してください。
