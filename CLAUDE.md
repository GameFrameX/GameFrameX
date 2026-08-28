# CLAUDE.md

本文件为 Claude Code (claude.ai/code) 在此代码库中工作时提供指导。

## 项目概述

GameFrameX 是一个综合性的游戏开发框架，支持多种客户端平台（Unity、LayaBox、Cocos Creator、Godot）和统一的 C# .NET 10.0 服务器。框架采用 Actor 模型架构，支持热更新。

## 构建和运行命令

### 服务器 (.NET 10.0)
```bash
# 构建服务器
dotnet build Server/Server.sln

# 本地运行游戏服务器
dotnet run --project Server/GameFrameX.Launcher --ServerType=Game --ServerId=1000 --APMPort=29090

# 使用 Docker 运行（包含 MongoDB）
cd Server && docker-compose up --build

# 运行 Foundation 测试
dotnet test Foundation/GameFrameX.Foundation.Tests
```

### 协议生成 (ProtoBuf)
```bash
# 导出服务器端代码
./Protobuf/Proto2CsExport_Server.sh

# 导出客户端代码
./Protobuf/Proto2CsExport_Client.sh

# 导出所有平台
./Protobuf/Proto2CsExport-All.bat  # 仅限 Windows
```

### 配置表生成 (LuBan)
```bash
# 生成服务器配置（JSON 格式）
./Config/gen-server-json.sh

# 生成客户端配置（JSON 格式）
./Config/gen-client-json.sh

# 生成二进制格式配置
./Config/gen-server-bin.sh
./Config/gen-client-bin.sh
```

## 架构说明

### 服务器架构（Actor 模型）

服务器遵循严格的 Actor 模型，采用状态-逻辑分离设计：

```
┌─────────────────────────────────────────────────────┐
│  GameFrameX.Hotfix/       (可热更逻辑层)             │
│  - Logic/             业务逻辑实现                   │
│  - StartUp/           热更启动处理器                 │
├─────────────────────────────────────────────────────┤
│  GameFrameX.Apps/         (持久化状态层)             │
│  - Account/           账户状态定义                   │
│  - Player/            玩家状态定义                   │
│  - Server/            服务器状态定义                 │
├─────────────────────────────────────────────────────┤
│  GameFrameX.Core/             (框架层)              │
│  - Actors/            Actor 系统基类                 │
│  - Components/        组件系统                       │
│  - Events/            事件系统                       │
│  - BaseHandler/       消息处理器                     │
├─────────────────────────────────────────────────────┤
│  网络层                                             │
│  - GameFrameX.NetWork/         TCP/UDP 基础         │
│  - GameFrameX.NetWork.HTTP/    HTTP 处理器          │
│  - GameFrameX.NetWork.Kcp/     KCP 协议             │
├─────────────────────────────────────────────────────┤
│  GameFrameX.DataBase.Mongo/   MongoDB 集成          │
└─────────────────────────────────────────────────────┘
```

### 核心设计模式

1. **组件-代理模式**：状态（Apps 层，不可热更）与逻辑（Hotfix 层，可热更）分离
2. **消息传递**：Actor 通过消息通信，避免锁竞争
3. **CacheState**：所有持久化实体继承 `CacheState`，实现自动 MongoDB 序列化

### 创建新功能

1. **定义状态**（在 `GameFrameX.Apps/` 中）：
```csharp
public class BagState : CacheState
{
    public List<ItemData> Items { get; set; } = new();
}
```

2. **创建组件**（在 `GameFrameX.Apps/` 中）：
```csharp
public class BagComponent : StateComponent<BagState> { }
```

3. **实现逻辑**（在 `GameFrameX.Hotfix/Logic/` 中）：
```csharp
public class BagComponentAgent : ComponentAgent<BagComponent>
{
    public Task AddItem(ItemData item) { /* 逻辑实现 */ }
}
```

## 项目结构

```
GameFrameX/
├── Config/              # LuBan 配置表
│   ├── Defines/         # LuBan 常量定义
│   ├── Excels/          # Excel 配置文件
│   └── Tools/           # LuBan CLI 工具
├── Protobuf/            # .proto 协议定义
├── Server/              # .NET 10.0 游戏服务器
│   ├── GameFrameX.Core/         Actor 系统、组件
│   ├── GameFrameX.Apps/         状态定义
│   ├── GameFrameX.Hotfix/       可热更逻辑
│   ├── GameFrameX.NetWork*/     网络层
│   ├── GameFrameX.DataBase.Mongo/  MongoDB 集成
│   └── GameFrameX.Launcher/     入口点
├── Foundation/          # 共享 .NET 基础库
│   └── GameFrameX.Foundation.*/  加密、扩展、哈希、JSON、日志、ORM 等
├── Unity/               # Unity3D 客户端
│   └── Assets/
│       ├── Hotfix/      # 可热更游戏代码
│       ├── Scripts/     # 核心脚本
│       └── Bundles/     # 资源包
├── Godot/               # Godot C# 客户端
├── LayaBox/             # LayaBox TypeScript 客户端
└── Tools/               # 协议导出工具
    └── ProtoExport/     # Proto 转 C#/TypeScript 工具
```

## 关键约定

### Proto 协议命名
- 文件命名格式：`{类型}_{ID}.proto`（如 `Bag_100.proto`、`User_300.proto`）
- 服务器内部消息：前缀 `_` 且 ID 为负数（如 `_-120_InnerSocial_s.proto`）

### LuBan 配置分组
- `c` 分组：仅客户端配置
- `s` 分组：仅服务器配置
- 目标模块：`GameFrameX.Config`（服务器）、`Hotfix.Config`（客户端）

### 网络处理器
- HTTP 处理器继承 `BaseHttpHandler`，使用 `[HttpMessageMapping]` 特性
- TCP 处理器实现 `GameFrameX.Core/BaseHandler/` 中的消息处理器接口

### 数据库
- MongoDB 是主要游戏数据库
- 实体继承 `CacheState` 实现自动持久化
- Foundation 层使用 `EntityBase` 特性进行 ORM 映射

## 依赖环境

### 服务器
- .NET 10.0 SDK
- MongoDB 4.x+
- Docker（可选，用于容器化部署）

### Unity 客户端
- Unity 2021.3+
- HybridCLR（用于热更新）

### 配置工具
- LuBan 工具（包含在 `Config/Tools/` 中）

### Godot 客户端
- Godot 4.5.1+
- .NET 10.0 SDK

## Unity 到 Godot 包迁移状态

详细的迁移计划请参见 `Godot/Unity2Godot_Package_Migration_Plan.md`。

### 已迁移的包（18 个）

| Unity 包名 | Godot 包名 | 状态 |
|-----------|-----------|------|
| com.gameframex.unity | com.gameframex.godot | ✅ 已迁移 |
| com.gameframex.unity.asset | com.gameframex.godot.asset | ✅ 已迁移 |
| com.gameframex.unity.config | com.gameframex.godot.config | ✅ 已迁移 |
| com.gameframex.unity.download | com.gameframex.godot.download | ✅ 已迁移 |
| com.gameframex.unity.entity | com.gameframex.godot.entity | ✅ 已迁移 |
| com.gameframex.unity.entry | com.gameframex.godot.entry | ✅ 已迁移 |
| com.gameframex.unity.event | com.gameframex.godot.event | ✅ 已迁移 |
| com.gameframex.unity.fsm | com.gameframex.godot.fsm | ✅ 已迁移 |
| com.gameframex.unity.getchannel | com.gameframex.godot.getchannel | ✅ 已迁移 |
| com.gameframex.unity.globalconfig | com.gameframex.godot.globalconfig | ✅ 已迁移 |
| com.gameframex.unity.localization | com.gameframex.godot.localization | ✅ 已迁移 |
| com.gameframex.unity.network | com.gameframex.godot.network | ✅ 已迁移 |
| com.gameframex.unity.procedure | com.gameframex.godot.procedure | ✅ 已迁移 |
| com.gameframex.unity.setting | com.gameframex.godot.setting | ✅ 已迁移 |
| com.gameframex.unity.timer | com.gameframex.godot.timer | ✅ 已迁移 |
| com.gameframex.unity.web | com.gameframex.godot.web | ✅ 已迁移 |
| com.gameframex.unity.web.protobuff | com.gameframex.godot.web.protobuff | ✅ 已迁移 |
| com.gameframex.unity.tuyoogame.yooasset | com.gameframex.godot.assetsystem | ✅ 已迁移 |
| com.gameframex.unity.google.protobuf | com.gameframex.godot.google.protobuf | ✅ 已迁移 |

### 待迁移的包（3 个）

| Unity 包名 | Godot 包名 | 优先级 | 说明 |
|-----------|-----------|--------|------|
| com.gameframex.unity.ui | com.gameframex.godot.ui | 中 | UI 基础包，需适配 Godot Control 系统 |
| com.gameframex.unity.ui.fairygui | com.gameframex.godot.ui.fairygui | 低 | 需评估 Godot FairyGUI 社区支持 |
| com.gameframex.unity.ui.ugui | - | 暂缓 | Unity 专有，Godot 不支持 |

### 核心 API 映射

| Unity API | Godot API |
|-----------|-----------|
| `Awake` | `_Ready` |
| `Update` | `_Process` |
| `OnDestroy` | `_Notification` |
| `GameObject` | `Node` |
| `Transform` | `Node` / `Node3D` |
| `MonoBehaviour` | `Node` |
| `Debug.Log` | `GD.Print` |

## 语言要求

**始终使用中文进行对话和交流。**
