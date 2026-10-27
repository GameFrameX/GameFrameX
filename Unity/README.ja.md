<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX Unity

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Unity?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Unity/releases)
[![License](https://img.shields.io/badge/license-Apache%202.0-orange.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)

**インディゲーム開発者向けオールインワンソリューション · インディ開発者の夢を支援**

[📖 ドキュメント](https://gameframex.doc.alianblank.com) · [🚀 クイックスタート](#クイックスタート) · [💬 QQグループ：467608841](https://qm.qq.com/q/467608841)

---

🌐 **言語**: [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [English](README.md) | **日本語** | [한국어](README.ko.md)

---

</div>

## プロジェクト概要

GameFrameX Unity は、GameFrameX 総合ソリューションの Unity クライアント実装です。90 以上のモジュラーコンポーネントを提供し、コアフレームワーク、アセット管理、UI システム、ネットワーク通信から広告、決済、ログイン、アナリティクスなどの運用機能まで、ゲーム開発の全工程をカバーします。Unity 2019.4.40 以降に対応しています。

## 特徴

- **モジュラー設計** — 各コンポーネントは独立したパッケージとして提供され、必要に応じて導入可能
- **ホットアップデート対応** — HybridCLR を統合し、C# ホットアップデートをサポート
- **複数 UI ソリューション** — FairyGUI と UGUI の両方に対応
- **非同期優先** — UniTask ベースの async/await 非同期プログラミングモデル
- **全プラットフォーム対応** — iOS、Android、WebGL、WeChat/Douyin/Alipay ミニゲームなど
- **豊富な運用コンポーネント** — 広告（穿山甲、TopOn）、決済（Alipay、Apple、Google）、ログイン（QQ、WeChat、Apple、Facebook、Google）、アナリティクス、オブジェクトストレージなど

## クイックスタート

### 1. Scoped Registry の設定

プロジェクトの `Packages/manifest.json` に GameFrameX の scoped registry を追加します：

```json
{
  "scopedRegistries": [
    {
      "name": "GameFrameX",
      "url": "https://gameframex.upm.alianblank.uk",
      "scopes": [
        "com.gameframex"
      ]
    }
  ],
  "dependencies": {
    "com.gameframex.unity": "1.11.0"
  }
}
```

### 2. 必要なコンポーネントの追加

`dependencies` に必要なパッケージを追加します。バージョン番号は [Releases](https://github.com/GameFrameX/GameFrameX.Unity/releases) ページで確認できます。

<details>
<summary>例：よく使うコンポーネントの追加</summary>

```json
{
  "dependencies": {
    "com.gameframex.unity": "1.11.0",
    "com.gameframex.unity.asset": "2.0.0",
    "com.gameframex.unity.ui": "2.1.1",
    "com.gameframex.unity.ui.fairygui": "3.0.0",
    "com.gameframex.unity.procedure": "1.0.4",
    "com.gameframex.unity.event": "1.0.2",
    "com.gameframex.unity.fsm": "1.0.3",
    "com.gameframex.unity.network": "2.5.1",
    "com.gameframex.unity.sound": "1.0.6",
    "com.gameframex.unity.localization": "2.0.0"
  }
}
```

</details>

## コンポーネント一覧

バージョン番号は [GameFrameX UPM Registry](https://gameframex.upm.alianblank.uk) から自動取得されます。`-` とマークされているパッケージは Git URL 経由でインストールされるサードパーティコンポーネントです。

### コアフレームワーク

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity | コアランタイムおよびエディタ基盤（イベントプール、参照プール、タスクプール、オブジェクトプール、変数システムなど） | ![version](https://img.shields.io/npm/v/com.gameframex.unity?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.entry | フレームワークエントリコンポーネント | ![version](https://img.shields.io/npm/v/com.gameframex.unity.entry?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.event | イベントシステム | ![version](https://img.shields.io/npm/v/com.gameframex.unity.event?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.fsm | 有限ステートマシン | ![version](https://img.shields.io/npm/v/com.gameframex.unity.fsm?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.procedure | プロシージャ管理 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.procedure?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.mono | MonoBehaviour ラッパー | ![version](https://img.shields.io/npm/v/com.gameframex.unity.mono?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.coroutine | コルーチンコンポーネント | ![version](https://img.shields.io/npm/v/com.gameframex.unity.coroutine?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.timer | タイマー | ![version](https://img.shields.io/npm/v/com.gameframex.unity.timer?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.entity | エンティティコンポーネントシステム | ![version](https://img.shields.io/npm/v/com.gameframex.unity.entity?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.globalconfig | グローバル設定管理 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.globalconfig?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.debugger | デバッガー | ![version](https://img.shields.io/npm/v/com.gameframex.unity.debugger?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### アセット管理

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.asset | アセットの読み込みと管理 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.asset?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.download | ファイルダウンロードコンポーネント | ![version](https://img.shields.io/npm/v/com.gameframex.unity.download?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.tuyoogame.yooasset | YooAsset アセット管理（カスタム版） | ![version](https://img.shields.io/npm/v/com.gameframex.unity.tuyoogame.yooasset?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gwiazdorrr.betterstreamingassets | StreamingAssets 直接アクセス | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gwiazdorrr.betterstreamingassets?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.builder | ビルドパイプラインツール | ![version](https://img.shields.io/npm/v/com.gameframex.unity.builder?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### UI フレームワーク

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.ui | UI 基本フレームワーク | ![version](https://img.shields.io/npm/v/com.gameframex.unity.ui?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.ui.fairygui | FairyGUI アダプター | ![version](https://img.shields.io/npm/v/com.gameframex.unity.ui.fairygui?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.ui.ugui | UGUI アダプター | ![version](https://img.shields.io/npm/v/com.gameframex.unity.ui.ugui?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### ネットワーク通信

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.web | HTTP ネットワークリクエスト（async/await 対応） | ![version](https://img.shields.io/npm/v/com.gameframex.unity.web?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.web.protobuff | ProtoBuf ネットワーク通信 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.web.protobuff?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.psygames.unitywebsocket | WebSocket ネットワークライブラリ | ![version](https://img.shields.io/npm/v/com.gameframex.unity.psygames.unitywebsocket?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.webview | WebView インラインブラウザ | ![version](https://img.shields.io/npm/v/com.gameframex.unity.webview?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### データと設定

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.config | 設定管理 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.config?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.localization | ローカライゼーション/多言語対応 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.localization?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.focus-creative-games.luban | Luban 設定データ生成 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.focus-creative-games.luban?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.google.protobuf | Protocol Buffers シリアライズ | ![version](https://img.shields.io/npm/v/com.gameframex.unity.google.protobuf?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.google.flatbuffers | FlatBuffers シリアライズ | ![version](https://img.shields.io/npm/v/com.gameframex.unity.google.flatbuffers?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.json.simplejson | SimpleJSON ライブラリ | ![version](https://img.shields.io/npm/v/com.gameframex.unity.json.simplejson?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.xincger.litjson | LitJSON ライブラリ | ![version](https://img.shields.io/npm/v/com.gameframex.unity.xincger.litjson?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### オーディオとアニメーション

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.sound | オーディオの再生と管理 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.sound?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.demigiant.dotween | DoTween アニメーションプラグイン | ![version](https://img.shields.io/npm/v/com.gameframex.unity.demigiant.dotween?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.esotericsoftware.spine.spine-unity | Spine アニメーションランタイム | ![version](https://img.shields.io/npm/v/com.gameframex.unity.esotericsoftware.spine.spine-unity?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### シーンと設定

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.scene | シーン管理 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.scene?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.setting | 設定管理 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.setting?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### ホットアップデート

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.tencent.xlua | XLua（Tencent 版） | ![version](https://img.shields.io/npm/v/com.gameframex.unity.tencent.xlua?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.xlua | XLua アダプター | ![version](https://img.shields.io/npm/v/com.gameframex.unity.xlua?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### 広告

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.advertisement | 広告基本コンポーネント | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.csj | 穿山甲広告 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.csj?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.topon | TopOn アグリゲーション広告 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.topon?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.wechatminigame | WeChat ミニゲーム広告 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.wechatminigame?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.douyinminigame | Douyin ミニゲーム広告 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.douyinminigame?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.alipayminigame | Alipay ミニゲーム広告 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.alipayminigame?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.kuaishouminigame | Kuaishou ミニゲーム広告 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.kuaishouminigame?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### 決済

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.payment | 決済基本コンポーネント | ![version](https://img.shields.io/npm/v/com.gameframex.unity.payment?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.payment.alipay | Alipay 決済 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.payment.alipay?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.payment.apple | Apple アプリ内課金 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.payment.apple?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.payment.google | Google Play アプリ内課金 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.payment.google?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### ログイン

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.login.apple | Apple ログイン | ![version](https://img.shields.io/npm/v/com.gameframex.unity.login.apple?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.login.facebook | Facebook ログイン | ![version](https://img.shields.io/npm/v/com.gameframex.unity.login.facebook?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.login.google | Google ログイン | ![version](https://img.shields.io/npm/v/com.gameframex.unity.login.google?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.login.qq | QQ ログイン | ![version](https://img.shields.io/npm/v/com.gameframex.unity.login.qq?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.login.wechat | WeChat ログイン | ![version](https://img.shields.io/npm/v/com.gameframex.unity.login.wechat?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### アナリティクス

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.gameanalytics | アナリティクス基盤 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gameanalytics.gameanalytics | GameAnalytics SDK | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics.gameanalytics?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gameanalytics.gravity-engine | Gravity Engine | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics.gravity-engine?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gameanalytics.talkingdata | TalkingData | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics.talkingdata?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gameanalytics.talkingdata.sdk | TalkingData SDK | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics.talkingdata.sdk?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gameanalytics.grafanaloki | Grafana Loki ログ | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics.grafanaloki?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gravityinfinite.gravity-engine | Gravity Engine アダプター | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gravityinfinite.gravity-engine?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.sentry | Sentry エラートラッキング | ![version](https://img.shields.io/npm/v/com.gameframex.unity.sentry?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.adjust | Adjust アトリビューション分析 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.adjust?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### オブジェクトストレージ

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.objectstorage | オブジェクトストレージ基盤 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.objectstorage?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.objectstorage.aliyun | Alibaba Cloud OSS | ![version](https://img.shields.io/npm/v/com.gameframex.unity.objectstorage.aliyun?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.objectstorage.qiniu | Qiniu Cloud Kodo | ![version](https://img.shields.io/npm/v/com.gameframex.unity.objectstorage.qiniu?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.objectstorage.tencent | Tencent Cloud COS | ![version](https://img.shields.io/npm/v/com.gameframex.unity.objectstorage.tencent?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### ミニゲーム

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.minigame.wechat | WeChat ミニゲームアダプター | - |
| com.gameframex.unity.tuyoogame.yooasset.minigame.alipay | YooAsset Alipay ミニゲーム | ![version](https://img.shields.io/npm/v/com.gameframex.unity.tuyoogame.yooasset.minigame.alipay?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.tuyoogame.yooasset.minigame.taptap | YooAsset TapTap ミニゲーム | ![version](https://img.shields.io/npm/v/com.gameframex.unity.tuyoogame.yooasset.minigame.taptap?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.tuyoogame.yooasset.minigame.tiktok | YooAsset Douyin ミニゲーム | ![version](https://img.shields.io/npm/v/com.gameframex.unity.tuyoogame.yooasset.minigame.tiktok?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### プラットフォームツール

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.getchannel | チャネル情報の取得 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.getchannel?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.operationclipboard | クリップボード操作 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.operationclipboard?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.readassets | StreamingAssets ファイル読み込み | ![version](https://img.shields.io/npm/v/com.gameframex.unity.readassets?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.xcode | Xcode ビルド後自動設定 | ![version](https://img.shields.io/npm/v/com.gameframex.unity.xcode?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.systeminfo | デバイス一意識別子（OAID/IDFA） | ![version](https://img.shields.io/npm/v/com.gameframex.unity.systeminfo?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.sharesdk | ShareSDK ソーシャルシェア | ![version](https://img.shields.io/npm/v/com.gameframex.unity.sharesdk?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.android | Android ネイティブツール | ![version](https://img.shields.io/npm/v/com.gameframex.unity.android?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### 開発ツール

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.yasirkula.debugconsole | ランタイムデバッグコンソール | ![version](https://img.shields.io/npm/v/com.gameframex.unity.yasirkula.debugconsole?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### サードパーティライブラリ

| パッケージ名 | 説明 | バージョン |
|:-----|:-----|:----:|
| com.gameframex.unity.cysharp.unitask | UniTask async/await | ![version](https://img.shields.io/npm/v/com.gameframex.unity.cysharp.unitask?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

## エコシステム構成

GameFrameX は以下のリポジトリで構成されます。ビルド時のツールチェーンがプロトコルと設定のコードを生成し、実行時はゲームクライアント・ゲームサーバー・運営バックエンドが連携します。

```mermaid
%%{init: {"flowchart": {"curve": "linear", "nodeSpacing": 30, "rankSpacing": 50}}}%%
flowchart TB
    subgraph Toolchain["Build-time toolchain"]
        direction TB
        Luban["luban<br/>Luban fork"] -- "Luban.dll · daily sync" --> Config["Config<br/>Excel tables"]
        Tools["Tools<br/>ProtoExport"] -- "ProtoExport.dll · weekly sync" --> Protobuf["Protobuf<br/>proto3 definitions"]
        FairyGUI["FairyGUIProject<br/>UI source"]
    end
    subgraph Runtime["Runtime"]
        direction TB
        subgraph Clients["Game clients"]
            direction TB
            Unity["Unity<br/>Unity 2019.4.40+ · C#"]
            Godot["Godot<br/>Godot 4.7 · C#"]
            Cocos["CocosCreator<br/>Cocos 3.8 · TS"]
            Laya["LayaBox<br/>LayaAir · TS"]
        end
        subgraph Backend["Backend"]
            direction TB
            Admin["Admin<br/>Admin · Client · Hub API"]
            subgraph GameServers["Game servers"]
                direction TB
                Server["Server<br/>NuGet framework"]
                ServerSource["Server.Source<br/>source framework"]
            end
        end
    end
    subgraph Infrastructure["Infrastructure"]
        direction TB
        subgraph Storage["Storage"]
            direction TB
            Mongo[("MongoDB<br/>game servers")]
            Postgres[("PostgreSQL<br/>Admin")]
        end
        subgraph Libraries["Base libraries · NuGet"]
            direction TB
            Foundation["Foundation<br/>.NET base libraries"]
            SuperSocket["SuperSocket<br/>socket framework"]
        end
    end
    Toolchain -- "generated code · config data · UI packages" --> Runtime
    Clients -- "TCP / WebSocket / HTTP" --> Backend
    Admin -- "HTTP management API" --> GameServers
    Runtime -- "data storage · NuGet packages" --> Infrastructure
```

### コアプロジェクト

| プロジェクト | 役割 | 技術スタック | 成果物と連携 |
|------|------|------|------|
| [Unity](https://github.com/GameFrameX/GameFrameX.Unity) | Unity クライアントプロジェクト：GameFrameX UPM レジストリから 90+ のフレームワークパッケージを必要に応じて導入、HybridCLR によるホットアップデート、ログイン・バッグ・ルームのサンプル付き | Unity 2019.4.40+, HybridCLR, YooAsset, FairyGUI, UniTask | Protobuf / Config / FairyGUIProject の成果物を利用；TCP 29100 + HTTP 28080 で Server、HTTP 20808 で Admin Client API に接続 |
| [Server](https://github.com/GameFrameX/GameFrameX.Server) | Actor モデルのゲームサーバー：永続化状態の Apps 層とホットアップデート可能な Hotfix ロジック層を厳密に分離、単一／マルチプロセス構成に対応 | .NET 10, MongoDB, Docker | TCP 29100 / WebSocket 29110 / HTTP 28080 で待ち受け；Admin 向けにメール・アイテム付与・プレイヤー管理などの HTTP 管理 API を提供 |
| [Admin](https://github.com/GameFrameX/GameFrameX.Admin) | マルチテナント運営バックエンド：Admin（20802）/ Client（20808）/ Hub（20806）の 3 ホスト + Vue 管理画面。サーバー区、お知らせ、メール、注文・決済、リモート設定、ライブ運営をカバー | .NET 10, SqlSugar, PostgreSQL, Vue 3.5, PrimeVue | サーバー区ごとの `HttpManageUrl` で Server を呼び出し；Client API がゲームクライアントにグローバル情報・ログイン・決済コールバックを提供 |
| [luban](https://github.com/GameFrameX/luban) | Luban 設定ツールの GameFrameX フォーク：公式 v5.1.0 をマージし、`gameframex` テーブルインポーターと `cs-dotnet-json` / `cs-dotnet-bin` などの生成ターゲットを追加 | .NET 10, Scriban | Luban の tar.gz と Docker イメージを公開；毎日 Config の `Tools/` に同期 |
| [Config](https://github.com/GameFrameX/GameFrameX.Config) | 設定テーブルリポジトリ：Excel データ表 + スキーマ + ローカライズ表。スクリプト一つでクライアント・サーバーのコードとデータを生成 | Excel, Luban | `gen-client-*` → Unity; `gen-server-json` → Server; `gen-server-bin` → Server.Source; `gen-godot-json` / `gen-cocos-json` / `gen-laya-json` → Godot / CocosCreator / LayaBox |
| [Protobuf](https://github.com/GameFrameX/GameFrameX.Protobuf) | 統一ネットワークプロトコル定義：proto3。ファイル名の数値モジュール ID ごとにメッセージとエラーコードを整理（正数＝クライアント↔サーバー、負数＝サーバー内部） | proto3 | `Proto2CsExport_Server` → Server; `Proto2CsExport_Client` → Unity; `Proto2CsExport_Godot` / `Proto2TsExport_CocosCreator` / `Proto2TsExport_LayaBox` → 他のクライアント；push ごとに CI が 6 言語の Release を公開 |
| [Tools](https://github.com/GameFrameX/GameFrameX.Tools) | ProtoExport プロトコルコード生成ツール：C# / TypeScript / C++ / Lua / Go の 5 言語、CLI・Avalonia GUI・Docker の 3 形態 | .NET 10, Avalonia 12 | `ProtoExport.dll` を毎週 Protobuf の `Tools/` に同期；Docker イメージ `gameframex/gameframex-tools` |

### 周辺システム

| プロジェクト | 役割 | 技術スタック | 成果物と連携 |
|------|------|------|------|
| [Foundation](https://github.com/GameFrameX/GameFrameX.Foundation) | モジュール化された .NET ライブラリ群：ローカライズ、JSON、コマンドライン引数、ログ、HTTP、暗号化、ハッシュ、ORM 基底エンティティ、冪等性 | .NET | NuGet パッケージとして Server・Server.Source・Admin・Tools が参照 |
| [SuperSocket](https://github.com/GameFrameX/GameFrameX.SuperSocket) | SuperSocket の GameFrameX フォーク：.NET 10 ターゲット、依存性注入、弱いネットワーク向け KCP / ReliableSession | .NET 10 | NuGet：Server（ClientEngine）、Server.Source（Server / Kcp / Udp / WebSocket） |
| [Server.Source](https://github.com/GameFrameX/GameFrameX.Server.Source) | フレームワーク層をリポジトリ内ソースとして持つサーバー版：サービスディスカバリ、プロセス間 RemoteMessaging（TCP / KCP）、マルチプロセス構成 | .NET 10, MongoDB, Docker | Config `gen-server-bin` の出力先；`docker-compose.multi.yml` でマルチプロセス起動 |
| [Godot](https://github.com/GameFrameX/GameFrameX.Godot) | Godot .NET クライアント：再利用可能なランタイムモジュール、エディタープラグイン、ホットアップデートアセンブリ | Godot 4.7, C# | Config `gen-godot-json`, Protobuf `Proto2CsExport_Godot`；TCP 29100 / WebSocket 29110 で Server に接続 |
| [CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) | Unity 版をベースに移植した Cocos Creator クライアント | Cocos Creator 3.8, TypeScript | `npm run gen:proto` / `gen:config` が Protobuf / Config のスクリプトを呼び出し；WebSocket で Server に接続 |
| [LayaBox](https://github.com/GameFrameX/GameFrameX.LayaBox) | LayaBox クライアント | LayaAir, TypeScript | Config `gen-laya-json`, Protobuf `Proto2TsExport_LayaBox`；WebSocket で Server に接続 |
| [FairyGUIProject](https://github.com/GameFrameX/GameFrameX.FairyGUIProject) | FairyGUI エディタープロジェクト：全ゲーム UI のデザインソース | FairyGUI Editor | UI パッケージを Unity `Assets/Bundles/UI`、コードを `Assets/Hotfix/UI/FairyGUI/` に出力 |

## ドキュメントとリソース

- **ドキュメント**: https://gameframex.doc.alianblank.com
- **ミラーサイト**: https://gameframex-docs.pages.dev
- **ミラーサイト**: https://gameframex.doc.cloudflare.alianblank.com
- **ミラーサイト**: https://gameframex.doc.vercel.alianblank.com

> すべてのサイトの内容は同一です。

## コミュニティとサポート

- **QQ グループ**: 467608841

## ライセンス

[Apache License 2.0](LICENSE.md)

---

**免責事項**: 一部のコンポーネントはインターネット上のオープンソースプロジェクトに由来しており、学習・交流目的でのみ提供されています。権利侵害に該当する場合は、Issue またはメールにてご連絡ください。速やかに対応いたします。
