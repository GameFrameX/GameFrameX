<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX.Config

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Config?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Config/releases)
[![License](https://img.shields.io/badge/license-Apache%202.0-orange.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)

**インディゲーム開発者向けオールインワンソリューション · インディ開発者の夢を支援**

[📖 ドキュメント](https://gameframex.doc.alianblank.com/ja) • [🚀 クイックスタート](#初心者ハンズオン) • [💬 QQグループ: 870596322](https://qm.qq.com/q/IrE4RSmqgY)

---

🌐 **言語**: [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **日本語** | [한국어](README.ko.md)

---

</div>

## これは何？

**GameFrameX.Config は「設定テーブルツール」です。**

簡単に言うと：**プランナー（企画）が Excel にゲームデータを入力すると、このツールが自動的にコードとデータファイルに変換して、ゲームプログラム（クライアントとサーバー）でそのまま使えるようにします。**

例えて言うと——Excel の表はあなたの「ゲームデータ辞書」で、Config がこの辞書をプログラムが直接読み取れる形式に翻訳してくれます。プランナーは表を埋めるだけ、プログラマーはデータを読むだけ、その間の作業を Config が自動で済ませてくれます。

オープンソースツール [Luban](https://github.com/GameFrameX/luban) をベースに構築されています（GameFrameX がカスタマイズして強化しています）。

## 何ができる？

**もしあなたがプランナー（企画）：**

- なじみのある Excel でデータを入力するだけ（アイテム、実績、サウンド、多言語テキスト……）
- 表を直したらプログラマーに「生成して」と渡すだけで、データがゲームに同期されます
- コードに触る必要はありません

**もしあなたが開発者：**

- スクリプトを1回実行するだけで、C# の設定クラス + データファイルが自動的に手に入ります
- コード内で `tables.TbXxx.Get(id)` と直接読み出せて、パースを手書きする必要はありません
- クライアント（Unity）とサーバー（.NET）用にそれぞれ生成され、型も一致します

## まず覚える言葉

| 言葉 | わかりやすく説明 |
|----|-----------|
| **設定テーブル** | ゲームのデータ表で、Excel に保存します。例えばアイテム表、実績表、レベル表など。 |
| **クライアント** | プレイヤー側のゲームプログラムです。ここでは Unity で作ります。 |
| **サーバー** | サーバー上で動くプログラムです。ここでは .NET で作ります。 |
| **生成** | Excel をプログラムがそのまま使えるコードとデータに変換すること。この手順は自動で完了します。 |
| **多言語化（ローカライズ）** | 同じテキストが複数の言語（中国語/英語/日本語/韓国語……）で用意されていること。プレイヤーが見る言語は設定次第です。 |

## フォルダの中身

```
Config/
├── Defines/        ← 最初から用意されたデータ型（座標など）
├── Excels/         ← あなたが入力する Excel はすべてここへ（最重要）
│   ├── Tables/     ← ゲームデータ表（アイテム、実績など）
│   └── Local/      ← 多言語テキスト
├── Tools/          ← ツール本体（触らなくて OK）
├── luban.conf      ← ツール設定（通常は触らなくて OK）
└── gen-*.bat/.sh   ← 生成スクリプト（ダブルクリックまたは実行するだけ）
```

**ここを重点的に見てください：**

- **`Excels/Tables/`** —— ゲームデータ表はここに置きます。例えばアイテム表、実績表など。
- **`Excels/Local/`** —— 多言語テキストはここに置きます。同じテキストの各言語への翻訳です。
- **`Excels/__tables__.xlsx`、`__beans__.xlsx`、`__enums__.xlsx`** —— この3つは「上級者向け定義表」で、複雑なフィールド型（列挙型や構造体など）を定義するのに使います。初心者は気にせず、一番シンプルな `int`、`string` だけで表を埋められます。
- **`Defines/`** —— ツール組み込みの型定義（例えば座標 `vec2/vec3/vec4`）で、クライアントとサーバーそれぞれの座標型に自動的に適合します。
- **`Tools/`** —— ツール本体です。触る必要はありません。
- **`gen-client-json.bat`、`gen-server-bin.bat`** —— 生成スクリプトです。**これが一番よく使うものです**。

## 初心者ハンズオン

ここではゼロから「アイテム表」を1つ作り、一通りの流れを体験します。一度やってみれば、すべて分かります。

### ステップ1：Excelファイルを作る

`Excels/Tables/` フォルダに、新しい Excel ファイルを作ります。名前は：

```
D-MyItem-我的道具表.xlsx
```

**名前の付け方は？ 法則を覚えてください：`英字 - 英語名 - 中国語名`**

- `D` —— 英字1文字。フォルダ内でファイルを並べ替えて探しやすくするためのもので、何でも構いません（A/B/C/D どれでも OK）
- `MyItem` —— 英語名。**コード内のクラス名になります**（自動的に `Tb` 接頭辞が付いて → `TbMyItem`）
- `我的道具表` —— 中国語名。人間が読むためのもので、何を書いても OK

### ステップ2：ヘッダーを埋める（最初の4行が「説明書」）

ファイルを開くと、最初の4行は決まった「ヘッダー」で、この表にどんなフィールドがあるかをツールに教えます：

| 行 | 何を埋めるか | この例での値 |
|----|--------|------|
| 1行目 `##var` | フィールド名（英語） | `id`、`name`、`price` |
| 2行目 `##type` | フィールド型 | `int`、`text`、`int` |
| 3行目 `##group` | フィールドグループ（通常は空欄） | 空、空、空 |
| 4行目 `##` | 中国語での説明（人間向け） | アイテムID、アイテム名、価格 |

埋めるとこうなります：

| ##var | id | name | price |
|-------|----|------|-------|
| ##type | int | text | int |
| ##group | | | |
| ## | アイテムID | アイテム名 | 価格 |

> この4行の最初のセル（`##var`、`##type`、`##group`、`##`）は固定のマーカーなので、必ずそのまま書いてください。

### ステップ3：データを埋める（5行目から）

ヘッダーの下が実際のデータで、1行が1件です：

| ##var | id | name | price |
|-------|----|------|-------|
| ##type | int | text | int |
| ##group | | | |
| ## | アイテムID | アイテム名 | 価格 |
| | 10001 | diamond | 10 |
| | 10002 | coin | 1 |

- `id` は数値（`int`）
- `name` には**多言語キー**（`text` 型）を入れます。実際に表示されるテキストは `Excels/Local/` で翻訳します。ここに `diamond` と入れ、ローカライズ表で `diamond` = ダイヤ/钻石/ダイヤ… を書いておきます。
- `price` は数値

### ステップ4：コードを生成する

`Config` フォルダに戻ります：

- **Windows**：`gen-client-json.bat` をダブルクリック
- **Mac / Linux**：ターミナルで `sh gen-client-json.sh` を実行

完了するまで待ちます（`pause` が出るか、エラーがなければ OK）。

### ステップ5：結果を確認する

ツールが自動的に隣の `Unity` フォルダに2つのものを生成します：

- **データファイル**（JSON）：中にあなたのアイテムデータが入っています
- **コードファイル**（C#）：中に `TbMyItem` クラスがあり、それがあなたのアイテム表です

### ステップ6：コードで使う

```csharp
// id が 10001 のアイテムを取得
var item = tables.TbMyItem.Get(10001);

// アイテム名は自動的に現在の言語になります（例：日本語では「ダイヤ」）
Debug.Log(item.Name);
Debug.Log(item.Price); // 10
```

**完了です！** Excel で入力したデータが、ゲームでそのまま使えるコードになりました ✅

## テーブルの名前の付け方

上で使った法則を、ここで全部説明します：

```
英字 - 英語名 - 中国語名.xlsx
英字 - 英語名 - グループ - 中国語名.xlsx      ← 特定の端にだけ使わせたい時
```

**3つの部分の意味：**

| 部分 | 何か | ルール | 例 |
|----|--------|------|------|
| **英字** | 並べ替え用のアルファベット1文字。ファイルを探しやすくする | 任意の文字または数字 | `C`、`D`、`S`、`L` |
| **英語名** | コードのクラス名 `Tb英語名` になる | 英字のみ、**中国語は使えない** | `ItemConfig` → `TbItemConfig` |
| **中国語名** | 人間が読むための名前 | 自由に書いて OK、`-` を複数追加してもよい | `道具表`、`道具表-1001` |

**⚠️ 注意：英語名には絶対に中国語を使わないでください**。さもないとツールがエラーを出します：*"中国語のテーブル名は対応していません"*。

**クライアントまたはサーバーだけで使いたい？** 英語名と中国語名の間にグループマーカーを入れます：

| ファイル名 | 効果 |
|--------|------|
| `D-ItemConfig-道具表.xlsx` | クライアントもサーバーも**両方で使う**（デフォルト） |
| `D-ItemConfig-c-道具表.xlsx` | **クライアントだけで**使う |
| `D-ItemConfig-s-道具表.xlsx` | **サーバーだけで**使う |

> `c` = クライアント、`s` = サーバー。グループを付けなければ両方に生成されます。

**既存テーブルの名前対応表：**

| ファイル名 | 生成されるクラス名 |
|--------|-----------|
| `C-AchievementConfig-成就表.xlsx` | `TbAchievementConfig` |
| `D-ItemConfig-道具表-道具-1001.xlsx` | `TbItemConfig` |
| `S-SoundsConfig-声音表.xlsx` | `TbSoundsConfig` |
| `L-Localization-成就.xlsx` | `TbLocalization` |

## 表の埋め方

各データ表の最初の4行は決まった「ヘッダー」です：

| 行 | マーカー | 何を埋めるか |
|----|------|--------|
| 1 | `##var` | フィールド名（英語、例：`id`、`name`） |
| 2 | `##type` | フィールド型（下表参照） |
| 3 | `##group` | フィールドグループ、通常は空欄 |
| 4 | `##` | 中国語での説明、自分や同僚が読むため |

**よく使うフィールド型：**

| 型 | 意味 | 例 |
|------|------|------|
| `int` | 整数 | `10001` |
| `string` | 通常のテキスト（翻訳しない） | `icon_diamond` |
| `text` | 多言語テキスト（キーを入れ、実際のテキストは `Local/` で） | `diamond` |
| `bool` | はい/いいえ | `true` / `false` |
| `float` | 小数 | `1.5` |
| 列挙型名 | `__enums__.xlsx` で定義した型 | `ItemType` |

> `text` と `string` の違い：`text` は翻訳される多言語テキスト（キーを入れる）、`string` は翻訳されない通常のテキスト（内容を直接入れる）です。

**埋めた例（実績表の抜粋）：**

| ##var | id | image | name | achievement_content |
|-------|----|-------|------|---------------------|
| ##type | int | int | text | text |
| ##group | | | | |
| ## | ID | アイコンid | 実績キー | 実績内容キー |
| | 900001 | 101 | achievement_001 | achievement_001_desc |

## 表が大きすぎる時は

1枚の表のデータが特に多い場合（例えばアイテムが1000件超）、**複数のファイルに分割**できます。ツールが自動的に1枚の表にマージします。

**どう分ける？** **英語名が同じ**であればよく、中国語名は区別しやすいように何を書いても OK：

```
D-ItemConfig-道具表-1-1000.xlsx      ← アイテム 1〜1000 個目
D-ItemConfig-道具表-1001-2000.xlsx   ← アイテム 1001〜2000 個目
D-ItemConfig-道具表-2001-3000.xlsx   ← アイテム 2001〜3000 個目
```

これら3つのファイルの英語名はすべて `ItemConfig` で、ツールが自動的に1つの `TbItemConfig` にマージします。

**多言語表も同様に分けます**（モジュールごとに分割）：

```
L-Localization-成就.xlsx    ┐
L-Localization-文本.xlsx    ├→ 1つの TbLocalization にマージ
L-Localization-UI.xlsx      ┘
```

> 中国語名の中の番号や分類（`1-1000`、`成就` など）は人間が読むためのもので、ツールは解釈しません。自分が分かりやすいように書けば OK です。

## コードの生成方法

### 準備する

1. **.NET SDK** をインストールしておく（ツールはこれで動きます）
2. `Config` フォルダの隣に、`Unity` と `Server` の2つのフォルダを用意する（生成されたコードがそこに入ります）

### クライアント（Unity）データを生成

- **Windows**：`gen-client-json.bat` をダブルクリック
- **Mac / Linux**：`sh gen-client-json.sh`

生成物の行き先：

- データ → `../Unity/Assets/Bundles/Config`
- コード → `../Unity/Assets/Hotfix/Config/Generate`

### サーバー（.NET）データを生成

- **Windows**：`gen-server-bin.bat` をダブルクリック
- **Mac / Linux**：`sh gen-server-bin.sh`

生成物の行き先：

- データ → `../Server/GameFrameX.Config/Json`
- コード → `../Server/GameFrameX.Config/Config`

> 4つのスクリプトの組み合わせ：`gen-{端}-{形式}.{sh/bat}`、端 = `client`/`server`、形式 = `json`（人が読める）/ `bin`（より小さく速い）。

## 生成したコードの使い方

**クライアント（Unity）の場合：**

```csharp
// tables は設定マネージャーで、ツールが自動生成します
// TbItemConfig はあなたが埋めた「アイテム表」で、Get(id) で id 検索します
var item = tables.TbItemConfig.Get(10001);
Debug.Log($"名前:{item.Name}, 価格:{item.Price}");

// 全アイテムを順に見る
foreach (var it in tables.TbItemConfig.DataList)
{
    Debug.Log(it.Name);
}
```

**サーバー（.NET）の場合：**

```csharp
var item = tables.TbItemConfig.Get(10001);
Console.WriteLine($"{item.Name}: {item.Price}");
```

> `text` 型のフィールド（例：`Name`）は、プレイヤーの現在の言語で自動的に表示されます。言語を手動で判定する必要はありません。

## コードはどこに出る？

ツールは「端」ごとに別々に生成し、互いに干渉しません：

| 誰向け | どのスクリプト | コードの名前空間 |
|----------|-----------|-------------|
| **クライアント**（Unity） | `gen-client-*` | `Hotfix.Config` |
| **サーバー**（.NET） | `gen-server-*` | `GameFrameX.Config` |
| **両方** | それぞれのスクリプトを1回ずつ実行 | それぞれ別 |

> 簡単に覚えるには：クライアントは `client` スクリプト、サーバーは `server` スクリプトで、必要な端のものを実行すれば OK。

## リポジトリにあるテーブル

現在、以下のデモ表が同梱されています：

| テーブル | ファイル | 内容 |
|----|------|------|
| 実績 | `Excels/Tables/C-AchievementConfig-成就表.xlsx` | 実績の定義 |
| アイテム | `Excels/Tables/D-ItemConfig-道具表-道具-1001.xlsx` | アイテムの定義 |
| サウンド | `Excels/Tables/S-SoundsConfig-声音表.xlsx` | サウンドの定義 |
| 多言語-実績 | `Excels/Local/L-Localization-成就.xlsx` | 実績の多言語テキスト |
| 多言語-テキスト | `Excels/Local/L-Localization-文本.xlsx` | 汎用多言語テキスト |
| 多言語-UI | `Excels/Local/L-Localization-UI.xlsx` | UI の多言語テキスト |

新しい表を追加したい？「初心者ハンズオン」の手順に従えば OK です。

## 必要な環境

- **.NET SDK** —— ツールを動かすのに必要（[dot.net](https://dotnet.microsoft.com/) からダウンロード）
- **Excel**（または WPS、Numbers など `.xlsx` を編集できるソフト）—— 表を埋めるのに必要
- **OS** —— Windows、Mac、Linux どれでも OK

## ライセンス

本プロジェクトは [Apache License 2.0](LICENSE.md) に基づきオープンソース化されており、無料で利用でき、商用利用も可能です。

## 関連リンク

- [ドキュメント](https://gameframex.doc.alianblank.com)
- [GitHub リポジトリ](https://github.com/GameFrameX/GameFrameX.Config)
- [問題報告](https://github.com/GameFrameX/GameFrameX.Config/issues)
- [Luban（GameFrameX カスタム版）](https://github.com/GameFrameX/luban)
- [Luban（オリジナル上流版）](https://github.com/focus-creative-games/luban)
