<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX Unity

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Unity?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Unity/releases)
[![License](https://img.shields.io/badge/license-Apache%202.0-orange.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)

**All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams**

[📖 Documentation](https://gameframex.doc.alianblank.com) · [🚀 Quick Start](#quick-start) · [💬 QQ Group: 467608841](https://qm.qq.com/q/467608841)

---

🌐 **Language**: [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **English** | [日本語](README.ja.md) | [한국어](README.ko.md)

---

</div>

## Project Overview

GameFrameX Unity is the Unity client implementation of the GameFrameX comprehensive solution, providing 90+ modular components covering the entire game development pipeline: from core framework, asset management, UI systems, and network communication to advertising, payment, login, data analytics, and more. Compatible with Unity 2019.4.40 and above.

## Features

- **Modular Design** — Each component is an independent package, import on demand
- **Hot Update Support** — Integrated HybridCLR for C# hot updates
- **Multi-UI Solutions** — Supports both FairyGUI and UGUI
- **Async-First** — UniTask-based async/await programming model
- **Cross-Platform** — iOS, Android, WebGL, WeChat/Douyin/Alipay mini games, and more
- **Rich Operations Components** — Advertising (CSJ, TopOn), Payment (Alipay, Apple, Google), Login (QQ, WeChat, Apple, Facebook, Google), Data Analytics, Object Storage, and more

## Quick Start

### 1. Configure Scoped Registry

Add the GameFrameX scoped registry to your project's `Packages/manifest.json`:

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

### 2. Add Components

Add the packages you need under `dependencies`. Check the [Releases](https://github.com/GameFrameX/GameFrameX.Unity/releases) page for available versions.

<details>
<summary>Example: Adding common components</summary>

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

## Component List

Version numbers are fetched automatically from the [GameFrameX UPM Registry](https://gameframex.upm.alianblank.uk). Packages marked with `-` are third-party components installed via Git URL.

### Core Framework

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity | Core runtime and editor base (event pool, reference pool, task pool, object pool, variable system, etc.) | ![version](https://img.shields.io/npm/v/com.gameframex.unity?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.entry | Framework entry component | ![version](https://img.shields.io/npm/v/com.gameframex.unity.entry?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.event | Event system | ![version](https://img.shields.io/npm/v/com.gameframex.unity.event?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.fsm | Finite state machine | ![version](https://img.shields.io/npm/v/com.gameframex.unity.fsm?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.procedure | Procedure management | ![version](https://img.shields.io/npm/v/com.gameframex.unity.procedure?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.mono | MonoBehaviour wrapper | ![version](https://img.shields.io/npm/v/com.gameframex.unity.mono?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.coroutine | Coroutine component | ![version](https://img.shields.io/npm/v/com.gameframex.unity.coroutine?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.timer | Timer | ![version](https://img.shields.io/npm/v/com.gameframex.unity.timer?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.entity | Entity component system | ![version](https://img.shields.io/npm/v/com.gameframex.unity.entity?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.globalconfig | Global configuration management | ![version](https://img.shields.io/npm/v/com.gameframex.unity.globalconfig?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.debugger | Debugger | ![version](https://img.shields.io/npm/v/com.gameframex.unity.debugger?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Asset Management

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.asset | Asset loading and management | ![version](https://img.shields.io/npm/v/com.gameframex.unity.asset?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.download | File download component | ![version](https://img.shields.io/npm/v/com.gameframex.unity.download?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.tuyoogame.yooasset | YooAsset asset management (customized) | ![version](https://img.shields.io/npm/v/com.gameframex.unity.tuyoogame.yooasset?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gwiazdorrr.betterstreamingassets | StreamingAssets direct access | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gwiazdorrr.betterstreamingassets?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.builder | Build pipeline tools | ![version](https://img.shields.io/npm/v/com.gameframex.unity.builder?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### UI Framework

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.ui | UI base framework | ![version](https://img.shields.io/npm/v/com.gameframex.unity.ui?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.ui.fairygui | FairyGUI adapter | ![version](https://img.shields.io/npm/v/com.gameframex.unity.ui.fairygui?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.ui.ugui | UGUI adapter | ![version](https://img.shields.io/npm/v/com.gameframex.unity.ui.ugui?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Networking

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.web | HTTP network requests (with async/await) | ![version](https://img.shields.io/npm/v/com.gameframex.unity.web?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.web.protobuff | ProtoBuf network communication | ![version](https://img.shields.io/npm/v/com.gameframex.unity.web.protobuff?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.psygames.unitywebsocket | WebSocket networking library | ![version](https://img.shields.io/npm/v/com.gameframex.unity.psygames.unitywebsocket?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.webview | WebView embedded browser | ![version](https://img.shields.io/npm/v/com.gameframex.unity.webview?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Data & Configuration

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.config | Configuration management | ![version](https://img.shields.io/npm/v/com.gameframex.unity.config?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.localization | Localization / i18n | ![version](https://img.shields.io/npm/v/com.gameframex.unity.localization?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.focus-creative-games.luban | Luban config data generation | ![version](https://img.shields.io/npm/v/com.gameframex.unity.focus-creative-games.luban?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.google.protobuf | Protocol Buffers serialization | ![version](https://img.shields.io/npm/v/com.gameframex.unity.google.protobuf?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.google.flatbuffers | FlatBuffers serialization | ![version](https://img.shields.io/npm/v/com.gameframex.unity.google.flatbuffers?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.json.simplejson | SimpleJSON library | ![version](https://img.shields.io/npm/v/com.gameframex.unity.json.simplejson?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.xincger.litjson | LitJSON library | ![version](https://img.shields.io/npm/v/com.gameframex.unity.xincger.litjson?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Audio & Animation

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.sound | Audio playback and management | ![version](https://img.shields.io/npm/v/com.gameframex.unity.sound?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.demigiant.dotween | DoTween animation plugin | ![version](https://img.shields.io/npm/v/com.gameframex.unity.demigiant.dotween?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.esotericsoftware.spine.spine-unity | Spine animation runtime | ![version](https://img.shields.io/npm/v/com.gameframex.unity.esotericsoftware.spine.spine-unity?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Scene & Settings

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.scene | Scene management | ![version](https://img.shields.io/npm/v/com.gameframex.unity.scene?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.setting | Settings management | ![version](https://img.shields.io/npm/v/com.gameframex.unity.setting?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Hot Update

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.tencent.xlua | XLua (Tencent version) | ![version](https://img.shields.io/npm/v/com.gameframex.unity.tencent.xlua?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.xlua | XLua adapter | ![version](https://img.shields.io/npm/v/com.gameframex.unity.xlua?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Advertising

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.advertisement | Advertising base component | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.csj | CSJ (穿山甲) Advertising | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.csj?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.topon | TopOn aggregated advertising | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.topon?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.wechatminigame | WeChat Mini Game ads | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.wechatminigame?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.douyinminigame | Douyin Mini Game ads | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.douyinminigame?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.alipayminigame | Alipay Mini Game ads | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.alipayminigame?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.advertisement.kuaishouminigame | Kuaishou Mini Game ads | ![version](https://img.shields.io/npm/v/com.gameframex.unity.advertisement.kuaishouminigame?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Payment

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.payment | Payment base component | ![version](https://img.shields.io/npm/v/com.gameframex.unity.payment?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.payment.alipay | Alipay payment | ![version](https://img.shields.io/npm/v/com.gameframex.unity.payment.alipay?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.payment.apple | Apple in-app purchase | ![version](https://img.shields.io/npm/v/com.gameframex.unity.payment.apple?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.payment.google | Google Play in-app purchase | ![version](https://img.shields.io/npm/v/com.gameframex.unity.payment.google?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Login

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.login.apple | Apple Sign-In | ![version](https://img.shields.io/npm/v/com.gameframex.unity.login.apple?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.login.facebook | Facebook Login | ![version](https://img.shields.io/npm/v/com.gameframex.unity.login.facebook?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.login.google | Google Sign-In | ![version](https://img.shields.io/npm/v/com.gameframex.unity.login.google?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.login.qq | QQ Login | ![version](https://img.shields.io/npm/v/com.gameframex.unity.login.qq?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.login.wechat | WeChat Login | ![version](https://img.shields.io/npm/v/com.gameframex.unity.login.wechat?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Analytics

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.gameanalytics | Analytics base | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gameanalytics.gameanalytics | GameAnalytics SDK | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics.gameanalytics?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gameanalytics.gravity-engine | Gravity Engine | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics.gravity-engine?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gameanalytics.talkingdata | TalkingData | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics.talkingdata?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gameanalytics.talkingdata.sdk | TalkingData SDK | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics.talkingdata.sdk?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gameanalytics.grafanaloki | Grafana Loki logging | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gameanalytics.grafanaloki?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.gravityinfinite.gravity-engine | Gravity Engine adapter | ![version](https://img.shields.io/npm/v/com.gameframex.unity.gravityinfinite.gravity-engine?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.sentry | Sentry error tracking | ![version](https://img.shields.io/npm/v/com.gameframex.unity.sentry?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.adjust | Adjust attribution analytics | ![version](https://img.shields.io/npm/v/com.gameframex.unity.adjust?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Object Storage

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.objectstorage | Object storage base | ![version](https://img.shields.io/npm/v/com.gameframex.unity.objectstorage?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.objectstorage.aliyun | Alibaba Cloud OSS | ![version](https://img.shields.io/npm/v/com.gameframex.unity.objectstorage.aliyun?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.objectstorage.qiniu | Qiniu Cloud Kodo | ![version](https://img.shields.io/npm/v/com.gameframex.unity.objectstorage.qiniu?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.objectstorage.tencent | Tencent Cloud COS | ![version](https://img.shields.io/npm/v/com.gameframex.unity.objectstorage.tencent?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Mini Games

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.minigame.wechat | WeChat Mini Game adapter | - |
| com.gameframex.unity.tuyoogame.yooasset.minigame.alipay | YooAsset Alipay Mini Game | ![version](https://img.shields.io/npm/v/com.gameframex.unity.tuyoogame.yooasset.minigame.alipay?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.tuyoogame.yooasset.minigame.taptap | YooAsset TapTap Mini Game | ![version](https://img.shields.io/npm/v/com.gameframex.unity.tuyoogame.yooasset.minigame.taptap?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.tuyoogame.yooasset.minigame.tiktok | YooAsset Douyin Mini Game | ![version](https://img.shields.io/npm/v/com.gameframex.unity.tuyoogame.yooasset.minigame.tiktok?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Platform Tools

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.getchannel | Channel information retrieval | ![version](https://img.shields.io/npm/v/com.gameframex.unity.getchannel?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.operationclipboard | Clipboard operations | ![version](https://img.shields.io/npm/v/com.gameframex.unity.operationclipboard?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.readassets | StreamingAssets file reading | ![version](https://img.shields.io/npm/v/com.gameframex.unity.readassets?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.xcode | Xcode post-build auto configuration | ![version](https://img.shields.io/npm/v/com.gameframex.unity.xcode?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.systeminfo | Device unique identifier (OAID/IDFA) | ![version](https://img.shields.io/npm/v/com.gameframex.unity.systeminfo?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.sharesdk | ShareSDK social sharing | ![version](https://img.shields.io/npm/v/com.gameframex.unity.sharesdk?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |
| com.gameframex.unity.android | Android native tools | ![version](https://img.shields.io/npm/v/com.gameframex.unity.android?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Development Tools

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.yasirkula.debugconsole | Runtime debug console | ![version](https://img.shields.io/npm/v/com.gameframex.unity.yasirkula.debugconsole?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

### Third-party Libraries

| Package | Description | Version |
|:--------|:------------|:-------:|
| com.gameframex.unity.cysharp.unitask | UniTask async/await | ![version](https://img.shields.io/npm/v/com.gameframex.unity.cysharp.unitask?registry_uri=https://gameframex.upm.alianblank.uk&style=flat-square&color=green) |

## Ecosystem Architecture

GameFrameX consists of the repositories below: the build-time toolchain generates protocol and config code, while game clients, game servers and the operations backend cooperate at runtime.

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

### Core Projects

| Project | Role | Tech Stack | Integration |
|------|------|------|------|
| [Unity](https://github.com/GameFrameX/GameFrameX.Unity) | Unity client project: assembles 90+ framework packages from the GameFrameX UPM registry, HybridCLR hot update, login / bag / room samples | Unity 2019.4.40+, HybridCLR, YooAsset, FairyGUI, UniTask | Consumes Protobuf / Config / FairyGUIProject output; TCP 29100 + HTTP 28080 to Server, HTTP 20808 to Admin Client API |
| [Server](https://github.com/GameFrameX/GameFrameX.Server) | Actor-model game server: persistent Apps state layer strictly separated from the hot-updatable Hotfix logic layer; single- or multi-process deployment | .NET 10, MongoDB, Docker | Listens on TCP 29100 / WebSocket 29110 / HTTP 28080; HTTP management endpoints (mail, item grants, player management) for Admin |
| [Admin](https://github.com/GameFrameX/GameFrameX.Admin) | Multi-tenant operations backend: Admin (20802) / Client (20808) / Hub (20806) hosts + Vue console, covering game areas, announcements, mail, orders & payment, remote config, LiveOps | .NET 10, SqlSugar, PostgreSQL, Vue 3.5, PrimeVue | Calls Server through each game area's `HttpManageUrl`; Client API serves global info, login and payment callbacks to game clients |
| [luban](https://github.com/GameFrameX/luban) | GameFrameX fork of Luban: merged upstream v5.1.0, adds the `gameframex` table importer and `cs-dotnet-json` / `cs-dotnet-bin` code targets | .NET 10, Scriban | Publishes Luban tar.gz and Docker image; synced daily into Config `Tools/` |
| [Config](https://github.com/GameFrameX/GameFrameX.Config) | Config table repository: Excel data tables + schema + localization tables, one-click scripts generate client and server code and data | Excel, Luban | `gen-client-*` → Unity; `gen-server-json` → Server; `gen-server-bin` → Server.Source; `gen-godot-json` / `gen-cocos-json` / `gen-laya-json` → Godot / CocosCreator / LayaBox |
| [Protobuf](https://github.com/GameFrameX/GameFrameX.Protobuf) | Unified network protocol definitions: proto3, messages and error codes grouped by the numeric module ID in each file name (positive = client↔server, negative = server-internal) | proto3 | `Proto2CsExport_Server` → Server; `Proto2CsExport_Client` → Unity; `Proto2CsExport_Godot` / `Proto2TsExport_CocosCreator` / `Proto2TsExport_LayaBox` → other clients; CI publishes a six-language Release on every push |
| [Tools](https://github.com/GameFrameX/GameFrameX.Tools) | ProtoExport protocol code generator: C# / TypeScript / C++ / Lua / Go, available as CLI, Avalonia GUI and Docker image | .NET 10, Avalonia 12 | `ProtoExport.dll` synced weekly into Protobuf `Tools/`; Docker image `gameframex/gameframex-tools` |

### Peripheral Systems

| Project | Role | Tech Stack | Integration |
|------|------|------|------|
| [Foundation](https://github.com/GameFrameX/GameFrameX.Foundation) | Modular .NET libraries: localization, JSON, command-line options, logging, HTTP, encryption, hashing, ORM base entities, idempotency | .NET | NuGet packages referenced by Server, Server.Source, Admin, Tools |
| [SuperSocket](https://github.com/GameFrameX/GameFrameX.SuperSocket) | GameFrameX fork of SuperSocket: .NET 10 targets, dependency injection, KCP / ReliableSession for weak-network game traffic | .NET 10 | NuGet: Server (ClientEngine), Server.Source (Server / Kcp / Udp / WebSocket) |
| [Server.Source](https://github.com/GameFrameX/GameFrameX.Server.Source) | Server variant with the framework as in-repo source: service discovery, cross-process RemoteMessaging (TCP / KCP), multi-process deployment | .NET 10, MongoDB, Docker | Receives Config `gen-server-bin` output; multi-process via `docker-compose.multi.yml` |
| [Godot](https://github.com/GameFrameX/GameFrameX.Godot) | Godot .NET client with reusable runtime modules, editor plugins and hot-update assemblies | Godot 4.7, C# | Config `gen-godot-json`, Protobuf `Proto2CsExport_Godot`; TCP 29100 / WebSocket 29110 to Server |
| [CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) | Cocos Creator client, migrated from the Unity version | Cocos Creator 3.8, TypeScript | `npm run gen:proto` / `gen:config` call Protobuf / Config scripts; WebSocket to Server |
| [LayaBox](https://github.com/GameFrameX/GameFrameX.LayaBox) | LayaBox client | LayaAir, TypeScript | Config `gen-laya-json`, Protobuf `Proto2TsExport_LayaBox`; WebSocket to Server |
| [FairyGUIProject](https://github.com/GameFrameX/GameFrameX.FairyGUIProject) | FairyGUI editor project: design source of all game UI | FairyGUI Editor | Publishes UI packages to Unity `Assets/Bundles/UI` and code to `Assets/Hotfix/UI/FairyGUI/` |

## Documentation & Resources

- **Documentation**: https://gameframex.doc.alianblank.com
- **Mirror**: https://gameframex-docs.pages.dev
- **Mirror**: https://gameframex.doc.cloudflare.alianblank.com
- **Mirror**: https://gameframex.doc.vercel.alianblank.com

> All mirror sites have identical content.

## Community & Support

- **QQ Group**: 467608841

## License

[Apache License 2.0](LICENSE.md)

---

**Disclaimer**: Some components are derived from open-source projects on the internet and are intended for learning and communication purposes only. If any infringement is involved, please submit an Issue or contact us via Email, and we will remove it promptly.
