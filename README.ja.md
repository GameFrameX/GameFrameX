<div align="center">

# 🎮 GameFrameX

**ゲームを「アイデア → 制作 → リリース運営」までまるごとサポートするオープンソースのツールキット**

[![Trendshift](https://trendshift.io/api/badge/repositories/20145)](https://trendshift.io/repositories/20145)

[简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [English](README.md) | **日本語** | [한국어](README.ko.md)

</div>

## クイックスタート（5分で起動）

**このリポジトリは完全なプロジェクトです**：git clone、Code → Download ZIP、ミラーサイトからのダウンロード——どの方法で入手してもそのまま動きます。追加のリポジトリ取得は不要です。

| コンポーネント | バージョン | 用途 |
|------|------|------|
| **.NET SDK** | **10.0+** | サーバーのビルド・実行（Foundation 依存は NuGet で自動復元、初回はネット接続が必要） |
| **Unity** | **2019.4.40f1** | クライアント `Unity/` を開く（初回インポート時に Package 取得のためネット接続が必要） |
| **Docker** | 最新版 | ローカル MongoDB をワンコマンドで起動 |

3ステップで動きます（詳細は下の[チュートリアル](#-チュートリアルゼロからログインまで)へ）：

```shell
# 1. ローカルデータベースを起動（MongoDB、アカウント admin / admin）
cd docker/mongo && docker compose up -d

# 2. サーバーをビルドして起動（上書きするのは DB 接続だけ、ポートはデフォルト値を使用）
cd ../../Server && dotnet build
cd bin/app_debug
dotnet GameFrameX.Launcher.dll --DataBaseUrl="mongodb://admin:admin@localhost:27017/?authSource=admin"

# 3. Unity 2019.4.40f1 で Unity/ プロジェクトを開き、Assets/Scenes/Launcher.unity を開いて Play
```

ログイン画面が表示され、キャラクターを作成してメインの街に入れれば、クライアント↔サーバーの全経路が通っています 🎉

サーバーが起動したか確認するには？ブラウザで `http://localhost:29090/health` を開いて応答があれば稼働中です。

---

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
| ゼロからオンラインサーバーを書く | そのまま使える高性能サーバー（.NET 製、多人数の同時接続に耐えられます） |
| データをどう保存するか | プレイヤーデータは MongoDB（読み書き高速）、バックオフィスデータは PostgreSQL（安定） |
| Excel の設定を手動でコードに反映 | LuBan で Excel をワンクリックでコードとデータに変換 |
| クライアントとサーバー間の「合言葉」 | ProtoBuf でプロトコルを統一、一箇所変更すれば両側に同期 |
| リリース後に何も見えない | 管理画面の Web ページを同梱、データ確認 / プレイヤー管理 / 設定配信が可能 |
| サーバーのデプロイが頭痛の種 | Docker でワンクリックのパッケージングとデプロイ、安心 |

> 要するに：**一人でも、小さなチームのように、オンラインゲームを作って運営できます。**

---

# 👤 どんな人に向いてる？

- **オンライン / ネットゲーム**を作りたいけど「サーバーをどうやって構築するか」で詰まっている個人開発者
- アイデアを検証する**ゲームプロトタイプ**を素早く作りたい小規模チーム
- 「クライアント + サーバー + バックオフィス」の全流程を最初から最後まで学びたい学習者

---

# 📦 リポジトリ構成：なぜダウンロードするとこういう構成なの？

このリポジトリは**集約リリースリポジトリ**です——下記 7 つのソースリポジトリの最新コードを毎日自動で同名フォルダに同期しています。ダウンロード 1 回で全パーツが手に入り、しかも**フォルダは最初から正しい位置にあります**（設定生成やプロトコル出力は相対パスで互いを見つけ合うので、リネームや移動はしないでください）：

```
GameFrameX/                   # プロジェクトルート
├── Server/                   # ゲームサーバー（.NET 10、Actor モデル + ホットアップデート）
├── Unity/                    # Unity クライアントプロジェクト（HybridCLR ホットアップデート、YooAsset）
├── LayaBox/                  # LayaAir クライアントプロジェクト（代替クライアント）
├── Config/                   # LuBan 設定表：ここで Excel を編集、両端のコードを一括生成
├── Protobuf/                 # 通信プロトコル：ここで .proto を編集、各端末用コードを一括出力
├── FairyGUIProject/          # UI 編集プロジェクト（FairyGUI エディターで Game.fairy を開く）
├── Tools/                    # 補助ツール（プロトコル出力 CLI / GUI）
├── docker/                   # ローカルデータベースをワンコマンド起動（mongo / postgres）
├── scripts/                  # 集約同期スクリプト
└── README / LICENSE など
```

| ディレクトリ | 対応するソースリポジトリ（変更はこちらへ PR / Issue を） |
|------|------|
| `Server/` | https://github.com/GameFrameX/GameFrameX.Server |
| `Unity/` | https://github.com/GameFrameX/GameFrameX.Unity |
| `LayaBox/` | https://github.com/GameFrameX/GameFrameX.LayaBox |
| `Config/` | https://github.com/GameFrameX/GameFrameX.Config |
| `Protobuf/` | https://github.com/GameFrameX/GameFrameX.Protobuf |
| `FairyGUIProject/` | https://github.com/GameFrameX/GameFrameX.FairyGUIProject |
| `Tools/` | https://github.com/GameFrameX/GameFrameX.Tools |

> ⚠️ **このリポジトリ内の `Server/`、`Unity/` などを直接編集しても無意味です**——毎日の自動同期で上書きされます。コードを変更したり PR を出したりする場合は、上の表の対応するソースリポジトリへどうぞ。

**集約対象外のリポジトリ**（必要に応じて）：

| リポジトリ | 説明 |
|------|------|
| [GameFrameX.Foundation](https://github.com/GameFrameX/GameFrameX.Foundation) | サーバー基盤ライブラリ。NuGet パッケージとして Server から参照（ビルド時に自動復元、clone 不要） |
| [GameFrameX.Admin](https://github.com/GameFrameX/GameFrameX.Admin) | 管理バックオフィス（一部ソース非公開）、[デモ](https://game.admin.web.vue.alianblank.com) |
| [GameFrameX.CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) / [Godot](https://github.com/GameFrameX/GameFrameX.Godot) | 他エンジンのクライアント |
| [GameFrameX.Docs](https://github.com/GameFrameX/GameFrameX.Docs) | ドキュメントサイトのソース |

---

# 🚀 チュートリアル：ゼロからログインまで

順にやっていくと 10〜15 分程度です（Unity の初回インポート込み）。

## ステップ 1：プロジェクトをダウンロード

```shell
git clone https://github.com/GameFrameX/GameFrameX.git
cd GameFrameX
```

git を使いたくない？GitHub ページの **Code → Download ZIP**、または [gitee などのミラーサイト](https://gitee.com/GameFrameX/GameFrameX)からダウンロードしても同じです。

## ステップ 2：環境を整える

| インストール | バージョン | 入手先 |
|---|---|---|
| **.NET SDK** | **10.0 以上** | https://dotnet.microsoft.com/download |
| **Unity エディター** | **2019.4.40f1**（Unity Hub → Installs → Install Editor → Archive から） | https://unity.com/download |
| **Docker Desktop** | 最新版 | https://www.docker.com/ |

> 💡 .NET 10 はサーバーと設定表生成ツールの必須要件です。ここを間違えると後で全部詰まります。

## ステップ 3：ローカルデータベースを起動

```shell
cd docker/mongo
docker compose up -d
```

起動するのは MongoDB です：`mongodb://admin:admin@localhost:27017`（データは `docker/mongo/database/` に保存）。

> PostgreSQL（`docker/postgres/`）は管理バックオフィス Admin 用です。このチュートリアルでは不要なので起動しなくて OK です。

## ステップ 4：サーバーをビルドして起動

```shell
cd ../../Server
dotnet build
cd bin/app_debug
dotnet GameFrameX.Launcher.dll --DataBaseUrl="mongodb://admin:admin@localhost:27017/?authSource=admin"
```

**引数が一つだけなのはなぜ？** デフォルト設定（`Server/GameFrameX.Launcher/StartUp/AppStartUpGame.cs` 参照）でポートが全部開いています：

| ポート | 用途 |
|---|---|
| 29100 | TCP：ゲームクライアントの常時接続 |
| 28080 | HTTP：ログインなどの API（`/game/api/...`） |
| 29110 | WebSocket |
| 29090 | ヘルスチェック / メトリクス |

上書きが必要なのは `DataBaseUrl` だけです——デフォルトは公開デモ用データベースを指しているので、ローカル開発では自分の MongoDB に向けてください。

**IDE ならもっと簡単**：Rider / Visual Studio で `Server/Server.slnx` を開き（`.slnx` 非対応なら `Server.sln`）、スタートアッププロジェクトを `GameFrameX.Launcher` に、**Working directory を `Server/bin/app_debug` に**設定し、コマンドライン引数は空のまま → `AppStartUpGame.cs` の `DataBaseUrl` デフォルト値をローカルの接続文字列に書き換えてください（集約リポジトリ内ファイルの編集はローカルデバッグ専用です。上書きの注意を参照）。

**確認**：ブラウザで `http://localhost:29090/health` を開き、応答があれば稼働しています。

## ステップ 5：Unity クライアントで接続

1. Unity Hub で **2019.4.40f1** を使い、リポジトリの `Unity/` フォルダを開く（初回は Package の自動取得でネット接続が必要。気長にお待ちを）
2. シーン `Assets/Scenes/Launcher.unity` を開く
3. **Play** ▶️ を押す

クライアントはデフォルトで `127.0.0.1`（TCP 29100 / HTTP 28080）に接続し、サーバーのデフォルトポートとちょうど一致します。設定変更は不要です。ログイン画面が表示され、キャラクターを作成してメインの街に入れたらチュートリアル完了です。

> 別マシン / リモートサーバーへ移すときは 2 箇所変更します：TCP アドレスは `Unity/Assets/Hotfix/UI/Logic/UILogin/UIPlayerList.cs`（`serverIp` / `serverPort`）、HTTP アドレスは `Unity/Assets/Hotfix/UI/Logic/UILogin/UILogin.cs` など（`127.0.0.1:28080` を検索）。

## LayaAir クライアントを使いたい？

LayaAir IDE で `LayaBox/` を開きます。入口は `src/Main.ts`。注意点が 2 つ：接続先は `LayaBox/src/gameframex/nettest.ts`（デフォルト `ws://127.0.0.1:21100` はサーバーのデフォルト WebSocket ポート **29110 と不一致**。揃えないと繋がりません）。プロトコル生成には `Protobuf/Proto2TsExport_LayaBox.sh` を使います。

---

# 🔁 日常開発：編集後に再生成するには

ダウンロードしたスナップショットには**生成済み成果物がすべて同梱されています**（設定コード / データ、プロトコルコード——全部揃っている）ので、そのまま動きます。再生成が必要なのは元ファイルを変更したときだけです：

## Excel 設定を変更したら（`Config/Excels/Tables/` の表）

| 変更対象 | 実行コマンド | 出力先 |
|---|---|---|
| サーバーが読む表 | `cd Config && sh gen-server-bin.sh`（Windows は `gen-server-bin.bat` をダブルクリック） | `Server/GameFrameX.Config/` |
| クライアントが読む表 | `cd Config && sh gen-client-json.sh` | `Unity/Assets/`（コード + データ） |

> ファイル名には規則があります：`英字-英語名-中国語名.xlsx`（例：`D-ItemConfig-道具表-道具-1001.xlsx`）。Excel の先頭 4 行はヘッダー（`##var` / `##type` / `##group` / 説明）で、データは 5 行目から。詳細なルールは [GameFrameX.Config](https://github.com/GameFrameX/GameFrameX.Config) へ。

## 通信プロトコルを変更したら（`Protobuf/*.proto`）

出力ツールはリポジトリに同梱されていません。最初に 1 回ビルドします（集約リポジトリのレイアウトは出力パスの要件を満たしています）：

```shell
cd Tools
dotnet build ProtoExport/ProtoExport.csproj -c Release   # 成果物は ../Protobuf/Tools/ に自動出力
cd ../Protobuf
sh Proto2CsExport_Server.sh    # サーバー用プロトコル → Server/GameFrameX.Proto/
sh Proto2CsExport_Client.sh    # クライアント用プロトコル → Unity/Assets/Hotfix/Proto/
```

> プロトコルの厳格なルール：proto3 のみ。`option module = 10;` 必須。メッセージ名は `Req<名前>` / `Resp<名前>` / `Notify<名前>`。フィールド番号は 800 未満。ネストした message は禁止。詳細は [GameFrameX.Protobuf](https://github.com/GameFrameX/GameFrameX.Protobuf) へ。

## UI を変更したら（FairyGUI）

FairyGUI エディター（≥5.0）で `FairyGUIProject/Game.fairy` を開き、編集後に **ファイル → パブリッシュ。「コード生成」に必ずチェック**を入れてください。成果物は `Unity/Assets/`（UI アセット + C# バインディングコード）に自動書き込まれます。

> 初心者の最頻出問題：パブリッシュ後に Unity でクラスが見つからないエラー → 10 中 8、9 は「コード生成」チェックを入れ忘れています。

---

# ⚠️ よくあるハマりどころ（初心者向け）

| 症状 | 原因と解決策 |
|---|---|
| サーバー起動時に DB 接続エラー | `DataBaseUrl` 未指定——デフォルトは公開デモ DB を指します。ステップ 4 のローカル接続文字列を渡してください |
| IDE 起動でクラッシュ / hotfix が見つからない | Working directory が `Server/bin/app_debug` になっていない（サーバーは「カレントディレクトリ/hotfix」からホットアップデートを読み込みます） |
| Unity の初回オープンでパッケージ取得から進まない | UPM プライベートレジストリ（`gameframex.upm.alianblank.uk`）と gitee（HybridCLR）へのネットアクセスが必要。制限されたネットワークでは止まります |
| クライアントがサーバーに繋がらない | ポート組み合わせが合っているか確認：TCP 29100 / HTTP 28080 / WS 29110。サーバーログにリスニング一覧が出ます |
| このリポジトリでコードを編集したのに翌日消えた | 集約リポジトリは毎日同期で上書きします。変更は対応するソースリポジトリへコミットしてください |
| LayaBox が繋がらない | `nettest.ts` のデフォルトポート 21100 ≠ サーバーの 29110。揃えてください |

---

# 💬 コミュニティ & フィードバック（提案、要望、バグ）

QQ グループ：**467608841**

# 📖 ドキュメント

> どのサイトも内容は同じです。開けるものを使ってください。

- メイン：https://gameframex.doc.alianblank.com
- ミラー 1：https://gameframex-docs.pages.dev
- ミラー 2：https://gameframex.doc.cloudflare.alianblank.com
- ミラー 3：https://gameframex.doc.vercel.alianblank.com

---

# ☕ 作者にコーヒーをおごる

![wechat.jpg](https://raw.githubusercontent.com/GameFrameX/GameFrameX/42e755df/Docs/imgs/wechat.jpg)

# 🎯 GameFrameX を使っている作品

| ゲーム名 | リリース渠道 | リリース時期 |
|:---|:---|:---|
| 深夜的烧烤店（真夜中の焼き鳥屋） | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |
| 連続黑白 | Douyin、Kuaishou、Alipay、HarmonyOS、TapTap、iOS など | 2024-11 |

> GameFrameX でリリース作品を作りました？PR や issue で上の表に追加してください 🙌

# 👥 コントリビューター

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

[![Star History Chart](https://star-history.dera.page/svg?repos=GameFrameX/GameFrameX,GameFrameX/GameFrameX.Unity,GameFrameX/GameFrameX.Server,GameFrameX/GameFrameX.Admin&type=Date)](https://star-history.dera.page/#GameFrameX/GameFrameX&GameFrameX/GameFrameX.Unity&GameFrameX/GameFrameX.Server&GameFrameX.GameFrameX.Admin&type=Date)

# 📜 免責事項

すべてのプラグインはインターネット上のものです。使用の際は各自で料金をお支払いください。権利を侵害している場合は email でお知らせください。削除いたします。

このプロジェクトは現地の法律が許可しない範囲で使用してはなりません。テクノロジー自体は無罪であり、濫用する人間が悪いのです。

# 💎 スポンサー

[AITKPARTY](https://aitkparty.com/) は、オープンソースプロジェクト New API をベースに構築された AI LLM API リレー / アグリゲーションサービスです。主要な大規模言語モデルへの統一インターフェースを提供し、各プロバイダーとの個別統合の手間を省きます。
