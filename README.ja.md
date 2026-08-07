<div align="center">
    <a href="https://trendshift.io/repositories/7536" target="_blank"><img src="https://trendshift.io/api/badge/repositories/7536" alt="GameFrameX%2FGameFrameX | Trendshift" style="width: 250px; height: 55px;" width="250" height="55"/></a>
</div>

[简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [English](README.md) | **日本語** | [한국어](README.ko.md)

# 🎮 GameFrameX って何？

一言でいうと：**ゲームを「アイデア → 制作 → リリース運営」までまるごとサポートするオープンソースのツールキットです。**

ゲーム作りで本当に大変なのは「キャラクターを描く、スキルを実装する」ではなく、こうしたバラバラの要素を一つにまとめることです：

- プレイヤーのセーブデータはどこに保存する？どうやって読み出す？
- マルチプレイの時、サーバーはどうメッセージを転送する？
- アイテム、ステージ、レベルといったデータは誰が管理する？企画が変更したらどうする？
- リリース後、どうデータを見る、プレイヤーを管理する、新バージョンを出す？

こうした「面倒で地道な作業」を GameFrameX が一括で請け負います。あなたは「私のゲームが面白いかどうか」に集中するだけで済みます。

次の主流エンジンに対応しています：**Unity、Cocos Creator、LayaAir（LayaBox）、Godot** —— どれを使っていても対応できます。

---

# 🧰 どんな手間を省ける？

| 自分でやると面倒なこと | GameFrameX が用意してくれるもの |
|---|---|
| ゼロからオンラインサーバーを書く | そのまま使える高性能サーバー（.NET 製、多人同時接続に耐えられます） |
| データをどう保存するか | プレイヤーデータは MongoDB（読み書き高速）、バックオフィスデータは PostgreSQL（安定） |
| Excel の設定を手動でコードに反映 | LuBan で Excel をワンクリックでコードとデータに変換 |
| クライアントとサーバー間の「合言葉」 | ProtoBuf でプロトコルを統一、一箇所変更すれば両側に同期 |
| リリース後に何も見えない | 管理画面の Web ページを同梱、データ確認 / プレイヤー管理 / 設定配信が可能 |
| サーバーのデプロイが頭痛の種 | Docker でワンクリックのパッケージングとデプロイ、安心 |

| 要点：**一人でも、小さなチームのように、オンラインゲームを作って運営できます。**

---

# 👤 どんな人に向いてる？

- **オンライン / ネットゲーム**を作りたいけど、「サーバーをどうするか」で行き詰まっている個人開発者
- さっと**ゲームのプロトタイプ**を組んでアイデアを検証したい小規模チーム
- 「クライアント + サーバー + バックオフィス」の全工程を一通り学びたい学習者

---

# 🗺️ このへんのリポジトリは何？（リポジトリマップ）

GameFrameX は「ファミリーバンド」のようなもので、各コンポーネントは**それぞれ独立したリポジトリ**に入っています（個別にメンテナンス・アップグレードしやすいように）。まずは以下の表で全体像を掴んでください：

| リポジトリ | ひとことで言うと… | URL |
|---|---|---|
| 🏠 **メインリポジトリ（ここ）** | 「厨房の平面図」——すべてのパーツをどのフォルダに置くかを示します | https://github.com/GameFrameX/GameFrameX |
| 🌐 **サーバー** | ゲームの頭脳、オンライン接続、セーブ、バトルロジックを管理（GeekServer をベースに発展） | https://github.com/GameFrameX/GameFrameX.Server |
| 📊 **設定テーブル（LuBan）** | Excel でゲームデータ（アイテム / ステージ / レベル…）を入力、ワンクリックでコード生成 | https://github.com/GameFrameX/GameFrameX.Config |
| 📡 **通信プロトコル（ProtoBuf）** | クライアントとサーバー間の「話し方のルール」、双方でやり取りするメッセージを定義 | https://github.com/GameFrameX/GameFrameX.Protobuf |
| 🎨 **UI プロジェクト（FairyGUI）** | FairyGUI エディタでゲーム画面を描くためのソースプロジェクト | https://github.com/GameFrameX/GameFrameX.FairyGUIProject |
| 🛠️ **ツール集** | 補助的な小道具類 | https://github.com/GameFrameX/GameFrameX.Tools |
| 💻 **管理画面** | リリース後にデータとプレイヤーを管理する Web ページ（一部ソースは非公開） | https://github.com/GameFrameX/GameFrameX.Admin |

管理画面のデモはこちら 👉 https://game.admin.web.vue.alianblank.com

## 🎮 クライアント（どれか一つを選べば OK、使うものをダウンロード）

| エンジン | URL |
|---|---|
| Unity | https://github.com/GameFrameX/GameFrameX.Unity |
| Cocos Creator | https://github.com/GameFrameX/GameFrameX.CocosCreator |
| LayaAir（LayaBox） | https://github.com/GameFrameX/GameFrameX.LayaBox |
| Godot | https://github.com/GameFrameX/GameFrameX.Godot |

---

# 📁 フォルダはなぜ適当に置いちゃダメ？

> ⚠️ **重要**：このフレームワークは**相対パス**でファイルを探します。家のコンセントの位置みたいなもの——サーバーを `Server/` から `MyServer/` に移動すると、経路全体が迷子になります。

なので、下記の構造に従って、各リポジトリを**正しいフォルダに**置いてください：

```
GameFrameX/                  # プロジェクトのルートディレクトリ（名前は変更可能）
├── Config/                  # ← GameFrameX.Config をここに置く（Excel 設定 + LuBan 変換）
├── Protobuf/                # ← GameFrameX.Protobuf をここに置く（通信プロトコル）
├── FairyGUIProject/         # ← GameFrameX.FairyGUIProject をここに置く（UI 編集プロジェクト）
├── Server/                  # ← GameFrameX.Server をここに置く（ゲームサーバー）
├── Unity/                   # ← GameFrameX.Unity をここに置く（Unity クライアント、必要に応じて別エンジンに置き換え）
│   ├── Assets/              #    Unity のアセットディレクトリ
│   ├── Packages/            #    Unity のパッケージ
│   ├── ProjectSettings/     #    Unity のプロジェクト設定
│   └── UserSettings/        #    Unity のユーザー設定
├── Tools/                   # ← GameFrameX.Tools をここに置く（補助ツール）
├── docker/                  # Docker のローカル実行環境（MongoDB / PostgreSQL）
├── Docs/                    # ドキュメント（現在は主に GeekServer のオリジナル文書）
└── LICENSE.md               # オープンソースライセンス
```

> 別のクライアントエンジンに変えたい？ `Unity/` を対応する名前に置き換えれば OK です（`Laya/`、`CocosCreator/`、`Godot/`）、ルールは同じです。

---

# 🔧 まずは環境を整える

始める前に、以下をインストールしてください（リンクから公式サイトへダウンロードできます）：

| インストールするもの | バージョン | 用途 | ダウンロード先 |
|---|---|---|---|
| **Git** | 任意の新バージョン | 各リポジトリのコードを取得 | https://git-scm.com/ |
| **.NET SDK** | **10.0 以上** | サーバーのビルド・実行、LuBan 変換ツールの実行 | https://dotnet.microsoft.com/download |
| **Unity エディタ** | **2019.4.40f1**（2019.4+ と互換） | Unity クライアントを開いて実行 | https://unity.com/download |
| **Docker**（任意だが推奨） | 任意の新バージョン | ローカルデータベース MongoDB / PostgreSQL をワンクリックで起動 | https://www.docker.com/ |

> 💡 サーバーと変換ツールはどちらも **.NET 10.0** に依存しています。これが最も重要なバージョン要件なので、必ず正しくインストールしてください。

---

# 🚀 ゼロから始める、手取り足取りのセットアップ

**ステップ 1**：プロジェクトを置くフォルダを新規作成し、ターミナル（Windows は cmd / PowerShell、Mac / Linux はターミナル）を開いて、`cd` で移動します。

**ステップ 2**：「厨房の平面図」をダウンロードします：

```shell
git clone https://github.com/GameFrameX/GameFrameX.git
```

これで `GameFrameX/` フォルダが生成され、中にプロジェクトの骨組みが入っています。

**ステップ 3**：各パーツを `GameFrameX/` 内の**対応するフォルダ**に配置します（以下は Unity の例です。別のエンジンを使う場合は最後の行を該当 URL に置き換えてください）：

```shell
git clone https://github.com/GameFrameX/GameFrameX.Server.git ./GameFrameX/Server
git clone https://github.com/GameFrameX/GameFrameX.Config.git ./GameFrameX/Config
git clone https://github.com/GameFrameX/GameFrameX.Protobuf.git ./GameFrameX/Protobuf
git clone https://github.com/GameFrameX/GameFrameX.FairyGUIProject.git ./GameFrameX/FairyGUIProject
git clone https://github.com/GameFrameX/GameFrameX.Tools.git ./GameFrameX/Tools
git clone https://github.com/GameFrameX/GameFrameX.Unity.git ./GameFrameX/Unity
```

> これらの行は「XX リポジトリの内容を、XX フォルダにダウンロードする」という意味です。**フォルダ名は絶対に変えないでください**。

**ステップ 4（ローカルデータベースを起動）**：Docker をインストール済みの場合、2 つのディレクトリでそれぞれ MongoDB と PostgreSQL を起動します（サーバーは MongoDB に、管理画面は PostgreSQL に接続します）：

```shell
cd GameFrameX/docker/mongo && docker compose up -d
cd ../postgres && docker compose up -d
```

起動に成功したら、次のように接続します：
- MongoDB：`mongodb://admin:admin@localhost:27017`
- PostgreSQL：`localhost:5432`、アカウント `postgres` / パスワード `postgres`、初期データベース `gameframex`

> ⚠️ 上記のアカウントとパスワードはローカル開発時のデフォルト値です。`Server` / `Admin` 側の接続設定と一致させていないと接続できません。

**ステップ 5（設定コードを生成）**：`Config/` ディレクトリに移動し、中の LuBan 変換スクリプトを実行して、Excel をクライアントとサーバーの両方で使えるコードとデータに変換します。具体的なコマンドは 👉 [`GameFrameX.Config`](https://github.com/GameFrameX/GameFrameX.Config) の説明を参照してください。

**ステップ 6（プロトコルコードを生成）**：`Protobuf/` ディレクトリに移動し、プロトコルエクスポートスクリプトを実行して、各端末でメッセージを送受信するためのコードを生成します。具体的なコマンドは 👉 [`GameFrameX.Protobuf`](https://github.com/GameFrameX/GameFrameX.Protobuf) の説明を参照してください。

**ステップ 7（任意）**：必要に応じて `Tools/` を開いて補助ツールをビルドします。👉 [`GameFrameX.Tools`](https://github.com/GameFrameX/GameFrameX.Tools) の説明を参照してください。

**ステップ 8（いよいよ実行！）**：Unity で `Unity/` プロジェクトを開き、`Server/` のサーバーを起動すれば、動作を体験できます 🎉

---

# 💬 コミュニティ & フィードバック（ご意見、ご要望、バグ報告）

QQ グループ：**467608841**

# 📖 ドキュメント（ちゃんと書いてます、急かさないで 😅）

> すべてのサイトの内容は同じです。開ける方を一つ選んで使ってください。

- メインサイト：https://gameframex.doc.alianblank.com
- 予備 1：https://gameframex-docs.pages.dev
- 予備 2：https://gameframex.doc.cloudflare.alianblank.com
- 予備 3：https://gameframex.doc.vercel.alianblank.com

---

# ☕ 作者にコーヒーをごちそうする

![wechat.jpg](Docs/imgs/wechat.jpg)

# 🎯 GameFrameX を使っているのは？

| ゲーム名 | リリース先 | リリース時期 |
|:---|:---|:---|
| 深夜的烧烤店（深夜の焼肉屋） | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |
| 连续黑白（連続白黒） | 抖音、快手、支付宝、鴻蒙、TapTap、iOS など | 2024-11 |

> GameFrameX でリリース済みの作品がありますか？ PR や issue で上の表に追加してください、リストを一緒に大きくしていきましょう 🙌

# 👥 貢献者一覧

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

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=GameFrameX/GameFrameX,GameFrameX/GameFrameX.Unity,GameFrameX/GameFrameX.Server,GameFrameX/GameFrameX.Admin&type=Date)](https://star-history.com/embed?secret=Z2hwX0l1VlJVYlE0RUhIZE9hS2pVZ21ISVozNFNNSUdETDMycmZEWQ==#GameFrameX/GameFrameX&GameFrameX/GameFrameX.Unity&GameFrameX/GameFrameX.Server&GameFrameX/GameFrameX.Admin&Date)

# 📜 免責事項

すべてのプラグインはインターネット上のもので、利用時は各自で料金をお支払いください。権利侵害にお心当たりのある方は email にてご連絡ください。確認の上、該当コンテンツを削除いたします。

本プロジェクトは、お住まいの地域の法律で認められない範囲において使用してはなりません。技術自体に罪はなく、悪いのはそれを悪用する人です。

# 💎 スポンサー

[AITKPARTY](https://aitkparty.com/) は AI 大規模モデル API の中継・集約サービスで、オープンソースプロジェクト New API をベースに構築されています。統一インターフェースを提供し、開発者が主要な大規模言語モデルに手軽にアクセスできるようにし、複数のモデルプロバイダーと個別に連携する手間を省けます。
