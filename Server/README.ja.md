<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX Server

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-Apache%202.0-orange.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex.doc.alianblank.com-brightgreen.svg)](https://gameframex.doc.alianblank.com)

インディゲーム開発者向けオールインワンソリューション · インディ開発者の夢を支援

[📖 ドキュメント](https://gameframex.doc.alianblank.com) · [🚀 クイックスタート](#quick-start) · [💬 QQグループ: 467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)

---

🌐 **言語**: [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **日本語** | [한국어](README.ko.md)

---

</div>

## プロジェクト概要

GameFrameX Server は、C# .NET 10.0 で構築された高性能ゲームサーバーフレームワークです。**Actor モデル**アーキテクチャと**ホットアップデート**機能を備えています。永続的な状態とビジネスロジックを厳密に分離し、マルチプレイオンラインゲームのシナリオに特化して設計されています。

> 設計哲学：複雑さよりもシンプルさを

## コア機能

- **Actor モデル** — TPL DataFlow に基づくロックフリーの高同時実行アーキテクチャ。メッセージパッシングにより従来のロック競合を回避
- **状態・ロジック分離** — 永続データ（Apps 層）とホットアップデート可能なビジネスロジック（Hotfix 層）の厳密な分離
- **ゼロダウンタイムホットアップデート** — サーバーの再起動なしに、実行時にビジネスロジックアセンブリを差し替え可能
- **マルチプロトコルネットワーク** — TCP、WebSocket、HTTP プロトコルをサポート。メッセージのエンコード/デコードと圧縮を内蔵
- **MongoDB 永続化** — CacheState に基づく透過的な ORM マッピング。自動シリアライズ/デシリアライズ
- **Source Generator** — Roslyn ベースの Agent コード生成。Actor メッセージキューのスケジューリングを自動化
- **コンフィグテーブルシステム** — Luban コンフィグ生成の統合。JSON ホットリロード対応
- **OpenTelemetry** — Prometheus メトリクスエクスポート、ヘルスチェック、パフォーマンス監視を内蔵

## アーキテクチャ

```
┌──────────────────────────────────────────────────────────────┐
│                       Client Layer                           │
├──────────────────────────────────────────────────────────────┤
│                      Network Layer                           │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐    │
│  │   TCP    │  │WebSocket │  │   HTTP   │  │   KCP    │    │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘    │
├──────────────────────────────────────────────────────────────┤
│                  Hotfix Layer (Hot-updatable)                │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  ComponentAgent  │  HttpHandler  │  EventHandler     │   │
│  └──────────────────────────────────────────────────────┘   │
├──────────────────────────────────────────────────────────────┤
│                  Apps Layer (Non-hot-updatable)              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                  │
│  │  State   │  │Component │  │ActorType │                  │
│  └──────────┘  └──────────┘  └──────────┘                  │
├──────────────────────────────────────────────────────────────┤
│                 Framework Layer (NuGet Packages)             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐    │
│  │  Core    │  │ NetWork  │  │ DataBase │  │ Monitor  │    │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘    │
├──────────────────────────────────────────────────────────────┤
│                     Database Layer                           │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │                      MongoDB                             │ │
│  └──────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
```

## クイックスタート

### 前提条件

| 依存関係 | バージョン |
| :--- | :--- |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ |
| [MongoDB](https://www.mongodb.com/try/download/community) | 4.x+ |
| IDE | Visual Studio 2022 / JetBrains Rider / VS Code |

### インストールと実行

ローカル開発の既定ルール:

- まず `Server.slnx` を開きます。より簡潔でレビューしやすいためです。IDE が `.slnx` をサポートしていない場合は `Server.sln` を開きます。
- 通常のローカルデバッグでは `Program arguments` / `Command line arguments` は空のままにします。既定設定は `GameFrameX.Launcher/StartUp/AppStartUpGame.cs` にあります。
- ポートや MongoDB を変更するなど、既定値を上書きする場合だけ起動引数を渡します。

#### 方法 1：Docker でまとめて起動

```bash
git clone https://github.com/GameFrameX/Server_main.git
cd Server_main
docker compose up -d --build
```

#### 方法 2：ターミナルでローカル起動

先に MongoDB を起動します。

```bash
docker compose up -d mongodb
```

その後、ビルドして出力ディレクトリから Launcher を起動します。

```bash
git clone https://github.com/GameFrameX/Server_main.git
cd Server_main
dotnet restore
dotnet build Server.sln
cd bin/app_debug
dotnet GameFrameX.Launcher.dll \
    --ServerId=1000 \
    --ServerType=Game \
    --IsEnableTcp=true \
    --InnerPort=29100 \
    --MetricsPort=29090 \
    --IsEnableHttp=true \
    --HttpPort=28080 \
    --IsEnableWebSocket=true \
    --WsPort=29110 \
    --HttpIsDevelopment=true \
    --MinModuleId=10 \
    --MaxModuleId=9999 \
    --TagName=GameFrameX \
    --DataBaseUrl="mongodb+srv://gameframex:f9v42aU9DVeFNfAF@gameframex.8taphic.mongodb.net/?retryWrites=true&w=majority" \
    --DataBaseName=gameframex
```

#### 方法 3：Rider でデバッグ

1. `Server.slnx` を開きます。Rider が `.slnx` をサポートしていない場合は `Server.sln` を開きます。
2. MongoDB を起動します: `docker compose up -d mongodb`。
3. `GameFrameX.Launcher` の `.NET Project` 実行構成を作成または編集します。
4. `Working directory` を `$SolutionDir$/bin/app_debug` に設定します。
5. `Program arguments` は空のままにします。
6. Debug を実行します。`bin/app_debug` がない場合は、先に `dotnet build Server.sln` を一度実行してください。

#### 方法 4：Visual Studio でデバッグ

1. `Server.slnx` を開きます。Visual Studio が `.slnx` をサポートしていない場合は `Server.sln` を開きます。
2. `GameFrameX.Launcher` をスタートアッププロジェクトに設定します。
3. MongoDB を起動します: `docker compose up -d mongodb`。
4. `Working directory` をソリューション配下の `bin\app_debug` に設定します。
5. `Command line arguments` は空のままにします。
6. F5 でデバッグを開始します。出力ディレクトリがない場合は、先に Build Solution を実行してください。

#### 必要な場合だけ既定設定を上書き

```bash
dotnet GameFrameX.Launcher.dll \
    --ServerId=1000 \
    --ServerType=Game \
    --IsEnableTcp=true \
    --InnerPort=29100 \
    --MetricsPort=29090 \
    --IsEnableHttp=true \
    --HttpPort=28080 \
    --IsEnableWebSocket=true \
    --WsPort=29110 \
    --HttpIsDevelopment=true \
    --MinModuleId=10 \
    --MaxModuleId=9999 \
    --TagName=GameFrameX \
    --DataBaseUrl="mongodb+srv://gameframex:f9v42aU9DVeFNfAF@gameframex.8taphic.mongodb.net/?retryWrites=true&w=majority" \
    --DataBaseName=gameframex
```

### デプロイメントの確認

| エンドポイント | URL | 説明 |
| :--- | :--- | :--- |
| ヘルスチェック | `http://localhost:29090/health` | サービスのヘルス状態 |
| メトリクス | `http://localhost:29090/metrics` | Prometheus メトリクス |
| テスト API | `http://localhost:28080/game/api/test` | HTTP 接続テスト |

## プロジェクト構成

```
Server_main/
├── GameFrameX.Launcher/         # アプリケーションのエントリポイント（実行可能ファイル）
│   ├── Program.cs               # ブートストラップ：状態型とプロトコルメッセージの登録
│   └── StartUp/
│       └── AppStartUpGame.cs    # ゲームサーバー起動フロー
│
├── GameFrameX.Hotfix/           # ホットアップデート層（ビジネスロジック、実行時差し替え可能）
│   ├── Logic/
│   │   ├── Http/                # HTTP リクエストハンドラー
│   │   │   ├── TestHttpHandler.cs
│   │   │   ├── ReloadHttpHandler.cs
│   │   │   ├── Player/          # アカウントログイン、オンライン照会
│   │   │   └── Bag/             # アイテム送信
│   │   ├── Player/
│   │   │   ├── Bag/             # バッグコンポーネントエージェント
│   │   │   ├── Login/           # ログイン/ログアウトロジック
│   │   │   └── Pet/             # ペットコンポーネントエージェント
│   │   ├── Account/             # アカウントコンポーネントエージェント
│   │   └── Server/              # サーバーグローバルコンポーネントエージェント
│   └── StartUp/                 # ホットアップデート起動フロー
│       ├── AppStartUpHotfixGameByEntry.cs    # エントリポイント
│       ├── AppStartUpHotfixGameByMain.cs     # ネットワーク/接続管理
│       ├── AppStartUpHotfixGameByHeart.cs    # ハートビート処理
│       └── AppStartUpHotfixGameByGateWay.cs  # ゲートウェイ通信
│
├── GameFrameX.Apps/             # アプリケーション状態層（ホットアップデート不可）
│   ├── ActorType.cs             # Actor 型 enum 定義
│   ├── Account/Login/           # ログイン状態 + コンポーネント
│   ├── Player/
│   │   ├── Bag/                 # バッグ状態 + コンポーネント
│   │   ├── Player/              # プレイヤー状態 + コンポーネント
│   │   └── Pet/                 # ペット状態 + コンポーネント
│   ├── Server/                  # サーバーグローバル状態 + コンポーネント
│   └── Common/
│       ├── Session/             # セッション管理（SessionManager）
│       ├── Event/               # イベント ID 定義
│       └── EventData/           # イベント引数
│
├── GameFrameX.Config/           # コンフィグテーブルシステム（Luban 生成）
│   ├── ConfigComponent.cs       # コンフィグ読み込みシングルトン
│   ├── Tables/                  # 生成されたコンフィグテーブルクラス
│   ├── TablesItem/              # コンフィグデータモデル
│   └── json/                    # JSON コンフィグデータファイル
│
├── GameFrameX.Proto/            # ネットワークプロトコル定義
│   ├── Basic_10.cs              # 基本プロトコル（ハートビート等）
│   ├── Bag_100.cs               # バッグプロトコル
│   ├── User_300.cs              # ユーザー/アカウントプロトコル
│   └── BuiltIn/                 # 組み込みシステムプロトコル
│
├── GameFrameX.Architecture.Analyzers/        # コンパイル時アーキテクチャルール
│
├── Server.slnx                  # 新しい IDE に推奨するソリューション
├── Server.sln                   # .slnx 非対応 IDE 用のフォールバック
├── Dockerfile                   # Docker マルチステージビルド
├── docker-compose.yml           # Docker Compose オーケストレーション
└── LICENSE.md                   # Apache License 2.0
```

## 使用例

### コアパターン：State - Component - Agent

GameFrameX は3層分離パターンを採用しており、サーバーのダウンタイムなしにビジネスロジックのホットアップデートが可能です。

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   State      │────▶│  Component   │◀────│ ComponentAgent│
│  (Data Def)  │     │   (Shell)    │     │  (Logic)     │
│  Apps Layer  │     │  Apps Layer  │     │ Hotfix Layer │
│  Not Hot-fix │     │  Not Hot-fix │     │  Hot-fixable │
└──────────────┘     └──────────────┘     └──────────────┘
```

#### ステップ 1：State の定義（Apps 層）

`CacheState` を継承して永続データ構造を定義します。フレームワークが MongoDB のシリアライズを自動的に処理します。

```csharp
// GameFrameX.Apps/Player/Bag/Entity/BagState.cs
public sealed class BagState : CacheState
{
    public Dictionary<int, BagItemState> List { get; set; } = new();
}

public sealed class BagItemState
{
    public long ItemId { get; set; }
    public long Count { get; set; }
}
```

#### ステップ 2：Component の作成（Apps 層）

`StateComponent<TState>` を継承して、状態とロジックの橋渡しとなるコンポーネントを作成します。

```csharp
// GameFrameX.Apps/Player/Bag/Component/BagComponent.cs
[ComponentType(GlobalConst.ActorTypePlayer)]
public class BagComponent : StateComponent<BagState> { }
```

#### ステップ 3：ビジネスロジックの実装（Hotfix 層）

`StateComponentAgent<TComponent, TState>` を継承して、ホットアップデート可能なビジネスコードを記述します。

```csharp
// GameFrameX.Hotfix/Logic/Player/Bag/BagComponentAgent.cs
public class BagComponentAgent : StateComponentAgent<BagComponent, BagState>
{
    public async Task OnAddBagItem(INetWorkChannel netWorkChannel,
        ReqAddItem message, RespAddItem response)
    {
        // Validate item config exists
        foreach (var item in message.ItemDic)
        {
            if (!ConfigComponent.Instance.GetConfig<TbItemConfig>()
                .TryGet(item.Key, out var _))
            {
                response.ErrorCode = (int)OperationStatusCode.NotFound;
                return;
            }
        }

        await UpdateChanged(netWorkChannel, message.ItemDic);
    }

    public async Task<BagState> UpdateChanged(INetWorkChannel netWorkChannel,
        Dictionary<int, long> itemDic)
    {
        var bagState = OwnerComponent.State;
        var notify = new NotifyBagInfoChanged();

        foreach (var item in itemDic)
        {
            if (bagState.List.TryGetValue(item.Key, out var value))
            {
                value.Count += item.Value;
            }
            else
            {
                bagState.List[item.Key] = new BagItemState
                {
                    Count = item.Value, ItemId = item.Key
                };
            }
        }

        await netWorkChannel.WriteAsync(notify);
        await OwnerComponent.WriteStateAsync(); // Auto-persist to MongoDB
        return bagState;
    }
}
```

### HTTP ハンドラー

`BaseHttpHandler` を継承し、`[HttpMessageMapping]` で登録します。

```csharp
// GameFrameX.Hotfix/Logic/Http/TestHttpHandler.cs
[HttpMessageMapping(typeof(TestHttpHandler))]
[HttpMessageResponse(typeof(HttpTestResponse))]
[Description("Test API endpoint")]
public sealed class TestHttpHandler : BaseHttpHandler
{
    public override Task<string> Action(string ip, string url,
        Dictionary<string, object> parameters)
    {
        var response = new HttpTestResponse { Message = "hello" };
        return Task.FromResult(HttpJsonResult.SuccessString(response));
    }
}
```

### RPC メッセージハンドラー

フレームワークは、ComponentAgent の自動注入を行う2つの RPC ハンドラー基底クラスを提供しています。

```csharp
// Player-level message (bound to a specific player Actor)
[MessageMapping(typeof(ReqAddItem))]
internal sealed class AddItemHandler
    : PlayerRpcComponentHandler<BagComponentAgent, ReqAddItem, RespAddItem>
{
    protected override async Task ActionAsync(ReqAddItem request, RespAddItem response)
    {
        await ComponentAgent.OnAddBagItem(NetWorkChannel, request, response);
    }
}

// Global-level message (bound to the server Actor)
[MessageMapping(typeof(ReqHeartBeat))]
internal sealed class HeartBeatHandler
    : GlobalRpcComponentHandler<ServerComponentAgent, ReqHeartBeat, RespHeartBeat>
{
    protected override Task ActionAsync(ReqHeartBeat request, RespHeartBeat response)
    {
        return Task.CompletedTask;
    }
}
```

### イベント処理

`[Event]` 属性を使用してイベント ID をバインドします。フレームワークが自動的に対応する ComponentAgent にディスパッチします。

```csharp
[Event(EventId.PlayerLogin)]
internal sealed class PlayerLoginEventHandler : EventListener<PlayerComponentAgent>
{
    protected override Task HandleEvent(PlayerComponentAgent agent, GameEventArgs args)
    {
        return agent.OnLogin();
    }
}
```

### Agent メソッド属性

Roslyn Source Generator が Actor メッセージキューのスケジューリングを自動的に処理します。属性を使って呼び出し動作を制御できます。

| 属性 | 説明 | 使用例 |
| :--- | :--- | :--- |
| `[Service]` | デフォルトモード。メソッド呼び出しは Actor メッセージキューにエンキュー | すべてのビジネスメソッド |
| `[ThreadSafe]` | メッセージキューをスキップし、直接呼び出し（スレッドセーフな実装が必要） | 読み取り専用操作、ステートレスな計算 |
| `[Discard]` | ファイアアンドフォーゲット。戻り値を待機しない | ロギング、結果が不要な統計処理 |
| `[TimeOut(ms)]` | メッセージキュー呼び出しのタイムアウトを設定 | タイムアウト制御が必要な長時間操作 |

```csharp
public class ServerComponentAgent : StateComponentAgent<ServerComponent, ServerState>
{
    // Enqueued to Actor message queue
    [Service]
    public virtual Task<bool> IsOnline(long roleId) { ... }

    // Skip message queue, thread-safe direct call
    [Service]
    [ThreadSafe]
    public virtual long FirstStartTime()
    {
        return State.FirstStartTime;
    }

    // Fire-and-forget, non-blocking
    [Service]
    [Discard]
    public virtual ValueTask AddOnlineRole(long roleId)
    {
        OwnerComponent.OnlineSet.Add(roleId);
        return ValueTask.CompletedTask;
    }
}
```

### コンフィグテーブルアクセス

`ConfigComponent` シングルトンを使用して、Luban 生成のコンフィグテーブルにアクセスします。

```csharp
var config = ConfigComponent.Instance.GetConfig<TbItemConfig>();

// Safe query with TryGet
if (config.TryGet(itemId, out var itemConfig))
{
    // Use itemConfig.Name, itemConfig.Type, etc.
}
```

### データベース操作

`GameDb` 静的クラスを使用して MongoDB の CRUD 操作を行います。

```csharp
// Query
var state = await GameDb.FindAsync<LoginState>(
    m => m.UserName == userName && m.Password == password);

// Add or update
await GameDb.AddOrUpdateAsync(loginState);

// List query
var list = await GameDb.FindListAsync<LoginState>(m => m.Id != 0);

// Delete
var count = await GameDb.DeleteAsync(state);
```

### ホットアップデートの仕組み

ホットアップデートシステムにより、サーバーを停止することなくビジネスロジックを差し替えることができます。

- **Apps 層**（`GameFrameX.Apps`）：状態定義とコンポーネントのシェルを含む — **ホットアップデート不可**
- **Hotfix 層**（`GameFrameX.Hotfix`）：すべてのビジネスロジックを含む — **ホットアップデート可能**
- **Hotfix アセンブリ**は `hotfix/` ディレクトリに出力され、`HotfixManager` によって実行時に読み込まれます

```bash
# Trigger via HTTP endpoint (with version number)
curl "http://localhost:28080/game/api/Reload?version=1.0.1"
```

### アーキテクチャアナライザー

`GameFrameX.Architecture.Analyzers` は Roslyn analyzer プロジェクトで、Apps/Hotfix アーキテクチャをコンパイル時に強制します。analyzer として参照されるのは `GameFrameX.Apps`、`GameFrameX.Hotfix`、`GameFrameX.Launcher` のみです。`GameFrameX.Config`、`GameFrameX.Proto`、テストアセンブリは設計上無視されます。

| ID | ルール |
| :--- | :--- |
| `GFX0001` | `BaseCacheState` のサブクラスは `GameFrameX.Apps` に定義する必要があります。 |
| `GFX0002` | `StateComponent<TState>` のサブクラスは `GameFrameX.Apps` に定義する必要があります。 |
| `GFX0003` | `BaseHttpHandler` のサブクラスは `GameFrameX.Hotfix` に定義する必要があります。 |
| `GFX0004` | `IHotfixBridge` の実装は `GameFrameX.Hotfix` に定義する必要があります。 |
| `GFX0005` | `IComponentAgent` の実装は `GameFrameX.Hotfix` に定義する必要があります。 |
| `GFX0006` | `[MessageMapping]` ハンドラーは `GameFrameX.Hotfix` に定義する必要があります。 |
| `GFX0007` | `[MessageRpcMapping]` ハンドラーは `GameFrameX.Hotfix` に定義する必要があります。 |
| `GFX0008` | `IEventListener` の実装は `GameFrameX.Hotfix` に定義する必要があります。 |
| `GFX0009` | `ITimerHandler` の実装は `GameFrameX.Hotfix` に定義する必要があります。 |
| `GFX0010` | `[MessageMapping]` ハンドラーは `sealed` である必要があります。 |
| `GFX0011` | `IEventListener` の実装は `sealed` である必要があります。 |
| `GFX0012` | `[MessageMapping]` ハンドラーの型名は `Handler` で終わる必要があります。 |
| `GFX0013` | `IEventListener` 実装の型名は `EventListener` で終わる必要があります。 |
| `GFX0014` | `IComponentAgent` 実装の型名は `ComponentAgent` で終わる必要があります。 |

## ドキュメントとリソース

### 既定設定の上書き

| パラメーター | 説明 | デフォルト | 例 |
| :--- | :--- | :--- | :--- |
| `ServerType` | サーバーの種類 | — | `Game` |
| `ServerId` | 一意のサーバー ID | — | `1000` |
| `IsEnableTcp` | TCP サービスを有効化 | `false` | `true` |
| `InnerPort` | TCP 内部ポート | — | `29100` |
| `IsEnableHttp` | HTTP サービスを有効化 | `false` | `true` |
| `HttpPort` | HTTP サービスポート | `0` | `28080` |
| `IsEnableWebSocket` | WebSocket サービスを有効化 | `false` | `true` |
| `WsPort` | WebSocket サービスポート | `0` | `29110` |
| `MetricsPort` | Prometheus メトリクスポート | `0` | `29090` |
| `HttpIsDevelopment` | HTTP 開発モードを有効化 | `false` | `true` |
| `MinModuleId` | ゲームサーバーが処理する最小モジュール ID | — | `10` |
| `MaxModuleId` | ゲームサーバーが処理する最大モジュール ID | — | `9999` |
| `TagName` | 実行時タグ名 | — | `GameFrameX` |
| `DataBaseUrl` | MongoDB 接続文字列 | — | `mongodb://localhost:27017` |
| `DataBaseName` | データベース名 | — | `gameframex` |

> **[!NOTE]**
> ローカルデバッグでは `GameFrameX.Launcher/StartUp/AppStartUpGame.cs` の既定設定を使用できるため、引数は不要です。上記の表はよく使う上書きパラメータです。全設定項目（ネットワーク、ログ、Actor、監視、セキュリティなど）については **[Configuration Management — GameFrameX.Server.Source](https://github.com/GameFrameX/GameFrameX.Server.Source#configuration-management)** を参照してください。

```bash
dotnet GameFrameX.Launcher.dll \
    --ServerId=1000 \
    --ServerType=Game \
    --IsEnableTcp=true \
    --InnerPort=29100 \
    --MetricsPort=29090 \
    --IsEnableHttp=true \
    --HttpPort=28080 \
    --IsEnableWebSocket=true \
    --WsPort=29110 \
    --HttpIsDevelopment=true \
    --MinModuleId=10 \
    --MaxModuleId=9999 \
    --TagName=GameFrameX \
    --DataBaseUrl="mongodb+srv://gameframex:f9v42aU9DVeFNfAF@gameframex.8taphic.mongodb.net/?retryWrites=true&w=majority" \
    --DataBaseName=gameframex
```

### Docker デプロイ

```bash
docker-compose up --build
```

| ポート | プロトコル | 説明 |
| :--- | :--- | :--- |
| `29090` | HTTP | APM メトリクス / ヘルスチェック |
| `29100` | TCP | ゲームクライアント接続 |
| `29110` | WebSocket | WebSocket 接続 |
| `28080` | HTTP | HTTP API |

### メッセージプロトコル

メッセージ ID はモジュール ID のシフトで計算されます：`(moduleId << 16) + seqId`

| モジュール | ID 範囲 | ファイル | 説明 |
| :--- | :--- | :--- | :--- |
| System | -10 ~ -1 | `Player_-10.cs`, `Service_-3.cs` | 組み込みシステムプロトコル |
| Basic | 10 | `Basic_10.cs` | ハートビート、サーバー準備完了通知 |
| Bag | 100 | `Bag_100.cs` | アイテム CRUD、使用、合成 |
| User | 300 | `User_300.cs` | ログイン、登録、キャラクター一覧 |

### 技術スタック

| コンポーネント | 技術 |
| :--- | :--- |
| ランタイム | .NET 10.0 |
| データベース | MongoDB |
| ネットワーク | SuperSocket |
| シリアライズ | protobuf-net |
| コンフィグ生成 | Luban |
| コード生成 | Roslyn Source Generator |
| 監視 | OpenTelemetry + Prometheus |
| オブジェクトマッピング | Mapster |
| コンテナ化 | Docker + Docker Compose |

## コミュニティとサポート

- [公式ドキュメント](https://gameframex.doc.alianblank.com)
- [GitHub Organization](https://github.com/GameFrameX)
- [Gitee ミラー](https://gitee.com/GameFrameX)
- [イシュートラッカー](https://github.com/GameFrameX/GameFrameX/issues)
- [ディスカッション](https://github.com/GameFrameX/GameFrameX/discussions)

### コントリビュート

1. このリポジトリをフォーク
2. フィーチャーブランチを作成（`git checkout -b feature/amazing-feature`）
3. 変更をコミット（`git commit -m 'feat: add some feature'`）
4. ブランチにプッシュ（`git push origin feature/amazing-feature`）
5. Pull Request を作成

## ライセンス

このプロジェクトは [Apache ライセンス 2.0](https://www.apache.org/licenses/LICENSE-2.0) のもとで公開されています。詳細は [LICENSE.md](LICENSE.md) ファイルをご覧ください。
