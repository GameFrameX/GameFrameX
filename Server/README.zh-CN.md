<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX Server

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-Apache%202.0-orange.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex.doc.alianblank.com-brightgreen.svg)](https://gameframex.doc.alianblank.com)

**独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使**

[📖 文档](https://gameframex.doc.alianblank.com) · [🚀 快速开始](#快速开始) · [💬 QQ群: 467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)

---

🌐 **语言**: [English](README.md) | **简体中文** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

---

</div>

## 项目简介

GameFrameX Server 是基于 C# .NET 10.0 开发的高性能游戏服务器框架，采用 **Actor 模型**设计，支持**热更新**机制。框架将持久化状态与业务逻辑严格分离，专为多人在线游戏场景设计。

> 设计理念：大道至简，以简化繁

## 核心特性

- **Actor 模型** — 基于 TPL DataFlow 的无锁高并发架构，通过消息传递避免传统锁竞争
- **状态-逻辑分离** — 持久化数据（Apps 层）与可热更业务逻辑（Hotfix 层）严格分离
- **零停机热更新** — 运行时替换业务逻辑程序集，无需重启服务器
- **多协议网络** — 支持 TCP、WebSocket、HTTP 协议，内置消息编解码与压缩
- **MongoDB 持久化** — 基于 CacheState 的透明 ORM 映射，自动序列化/反序列化
- **源码生成器** — 基于 Roslyn 的 Agent 代码生成，自动处理 Actor 消息队列调度
- **配置表系统** — 集成 Luban 配置生成，支持 JSON 热加载
- **OpenTelemetry** — 内置 Prometheus 指标导出、健康检查、性能监控

## 架构概览

```
┌──────────────────────────────────────────────────────────────┐
│                         客户端层                              │
├──────────────────────────────────────────────────────────────┤
│                         网络层                                │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐    │
│  │   TCP    │  │WebSocket │  │   HTTP   │  │   KCP    │    │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘    │
├──────────────────────────────────────────────────────────────┤
│                     Hotfix 层（可热更）                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  ComponentAgent  │  HttpHandler  │  EventHandler     │   │
│  └──────────────────────────────────────────────────────┘   │
├──────────────────────────────────────────────────────────────┤
│                      Apps 层（不可热更）                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                  │
│  │  State   │  │Component │  │ActorType │                  │
│  └──────────┘  └──────────┘  └──────────┘                  │
├──────────────────────────────────────────────────────────────┤
│                    框架层（NuGet 包）                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐    │
│  │  Core    │  │ NetWork  │  │ DataBase │  │ Monitor  │    │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘    │
├──────────────────────────────────────────────────────────────┤
│                      数据库层                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │                      MongoDB                             │ │
│  └──────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
```

## 快速开始

### 环境要求

| 依赖 | 版本要求 |
| :--- | :--- |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ |
| [MongoDB](https://www.mongodb.com/try/download/community) | 4.x+ |
| IDE | Visual Studio 2022 / JetBrains Rider / VS Code |

### 安装与运行

本地开发默认规则：

- 优先打开 `Server.slnx`。它更简洁、更容易审查；如果 IDE 不支持 `.slnx`，再打开 `Server.sln`。
- 普通本地调试时 `Program arguments` / `Command line arguments` 留空。默认配置在 `GameFrameX.Launcher/StartUp/AppStartUpGame.cs`。
- 只有需要覆盖默认值时才传启动参数，例如换端口或连接其它 MongoDB。

#### 方式 1：Docker 一键启动

```bash
git clone https://github.com/GameFrameX/Server_main.git
cd Server_main
docker compose up -d --build
```

#### 方式 2：命令行本地启动

先启动 MongoDB：

```bash
docker compose up -d mongodb
```

再编译并从输出目录启动 Launcher：

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

#### 方式 3：Rider 调试

1. 打开 `Server.slnx`；如果 Rider 版本不支持 `.slnx`，再打开 `Server.sln`。
2. 启动 MongoDB：`docker compose up -d mongodb`。
3. 创建或编辑 `GameFrameX.Launcher` 的 `.NET Project` 运行配置。
4. `Working directory` 设置为 `$SolutionDir$/bin/app_debug`。
5. `Program arguments` 留空。
6. 点击 Debug。第一次调试前如果 `bin/app_debug` 不存在，先执行一次 `dotnet build Server.sln`。

#### 方式 4：Visual Studio 调试

1. 打开 `Server.slnx`；如果 Visual Studio 版本不支持 `.slnx`，再打开 `Server.sln`。
2. 将 `GameFrameX.Launcher` 设为启动项目。
3. 启动 MongoDB：`docker compose up -d mongodb`。
4. `Working directory` 设置为解决方案目录下的 `bin\app_debug`。
5. `Command line arguments` 留空。
6. 按 F5 启动调试。第一次调试前如果输出目录不存在，先执行一次 Build Solution。

#### 需要时覆盖默认配置

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

### 验证部署

| 端点 | 地址 | 说明 |
| :--- | :--- | :--- |
| 健康检查 | `http://localhost:29090/health` | 服务健康状态 |
| 指标监控 | `http://localhost:29090/metrics` | Prometheus 指标 |
| 测试接口 | `http://localhost:28080/game/api/test` | HTTP 连通性测试 |

## 项目结构

```
Server_main/
├── GameFrameX.Launcher/         # 应用入口点（可执行程序）
│   ├── Program.cs               # 启动引导：注册状态类型与协议消息
│   └── StartUp/
│       └── AppStartUpGame.cs    # 游戏服务器启动流程
│
├── GameFrameX.Hotfix/           # 热更新层（业务逻辑，可运行时替换）
│   ├── Logic/
│   │   ├── Http/                # HTTP 请求处理器
│   │   │   ├── TestHttpHandler.cs
│   │   │   ├── ReloadHttpHandler.cs
│   │   │   ├── Player/          # 账户登录、在线查询
│   │   │   └── Bag/             # 道具发放
│   │   ├── Player/
│   │   │   ├── Bag/             # 背包组件代理
│   │   │   ├── Login/           # 登录/登出逻辑
│   │   │   └── Pet/             # 宠物组件代理
│   │   ├── Account/             # 账号组件代理
│   │   └── Server/              # 服务器全局组件代理
│   └── StartUp/                 # 热更新启动流程
│       ├── AppStartUpHotfixGameByEntry.cs    # 加载入口
│       ├── AppStartUpHotfixGameByMain.cs     # 网络/连接管理
│       ├── AppStartUpHotfixGameByHeart.cs    # 心跳处理
│       └── AppStartUpHotfixGameByGateWay.cs  # 网关通信
│
├── GameFrameX.Apps/             # 应用状态层（不可热更）
│   ├── ActorType.cs             # Actor 类型枚举定义
│   ├── Account/Login/           # 登录状态 + 组件
│   ├── Player/
│   │   ├── Bag/                 # 背包状态 + 组件
│   │   ├── Player/              # 玩家状态 + 组件
│   │   └── Pet/                 # 宠物状态 + 组件
│   ├── Server/                  # 服务器全局状态 + 组件
│   └── Common/
│       ├── Session/             # 会话管理（SessionManager）
│       ├── Event/               # 事件 ID 定义
│       └── EventData/           # 事件参数
│
├── GameFrameX.Config/           # 配置表系统（Luban 生成）
│   ├── ConfigComponent.cs       # 配置加载单例
│   ├── Tables/                  # 生成的配置表类
│   ├── TablesItem/              # 配置数据模型
│   └── json/                    # JSON 配置数据文件
│
├── GameFrameX.Proto/            # 网络协议定义
│   ├── Basic_10.cs              # 基础协议（心跳等）
│   ├── Bag_100.cs               # 背包协议
│   ├── User_300.cs              # 用户/账号协议
│   └── BuiltIn/                 # 内置系统协议
│
├── GameFrameX.Architecture.Analyzers/        # 编译期架构规则检查器
│
├── Server.slnx                  # 新版本 IDE 推荐使用的解决方案文件
├── Server.sln                   # 不支持 .slnx 的 IDE 备用解决方案
├── Dockerfile                   # Docker 多阶段构建
├── docker-compose.yml           # Docker Compose 编排
└── LICENSE.md                   # Apache License 2.0
```

## 使用示例

### 核心模式：状态-组件-代理

GameFrameX 采用三层分离的开发模式，确保业务逻辑可以在不停服的情况下热更新。

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   State      │────▶│  Component   │◀────│ ComponentAgent│
│  （数据定义）  │     │  （组件外壳） │     │  （业务逻辑）  │
│   Apps 层    │     │   Apps 层    │     │  Hotfix 层   │
│  不可热更    │     │  不可热更    │     │   可热更      │
└──────────────┘     └──────────────┘     └──────────────┘
```

#### 第 1 步：定义状态（Apps 层）

继承 `CacheState`，定义持久化数据结构。框架自动处理 MongoDB 序列化。

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

#### 第 2 步：创建组件（Apps 层）

继承 `StateComponent<TState>`，作为状态和逻辑的桥梁。

```csharp
// GameFrameX.Apps/Player/Bag/Component/BagComponent.cs
[ComponentType(GlobalConst.ActorTypePlayer)]
public class BagComponent : StateComponent<BagState> { }
```

#### 第 3 步：实现业务逻辑（Hotfix 层）

继承 `StateComponentAgent<TComponent, TState>`，编写可热更的业务代码。

```csharp
// GameFrameX.Hotfix/Logic/Player/Bag/BagComponentAgent.cs
public class BagComponentAgent : StateComponentAgent<BagComponent, BagState>
{
    public async Task OnAddBagItem(INetWorkChannel netWorkChannel,
        ReqAddItem message, RespAddItem response)
    {
        // 校验物品配置是否存在
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
        await OwnerComponent.WriteStateAsync(); // 自动持久化到 MongoDB
        return bagState;
    }
}
```

### HTTP 处理器

继承 `BaseHttpHandler`，使用 `[HttpMessageMapping]` 注册路由。

```csharp
// GameFrameX.Hotfix/Logic/Http/TestHttpHandler.cs
[HttpMessageMapping(typeof(TestHttpHandler))]
[HttpMessageResponse(typeof(HttpTestResponse))]
[Description("测试通讯接口")]
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

### RPC 消息处理器

框架提供两种 RPC 处理器基类，自动注入 ComponentAgent：

```csharp
// 玩家级别消息（绑定特定玩家 Actor）
[MessageMapping(typeof(ReqAddItem))]
internal sealed class AddItemHandler
    : PlayerRpcComponentHandler<BagComponentAgent, ReqAddItem, RespAddItem>
{
    protected override async Task ActionAsync(ReqAddItem request, RespAddItem response)
    {
        await ComponentAgent.OnAddBagItem(NetWorkChannel, request, response);
    }
}

// 全局级别消息（绑定服务器 Actor）
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

### 事件处理

使用 `[Event]` 特性绑定事件 ID，框架自动分发给对应的 ComponentAgent。

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

### Agent 方法标注

使用 Roslyn 源码生成器自动处理 Actor 消息队列调度。通过特性标注控制调用行为：

| 特性 | 说明 | 适用场景 |
| :--- | :--- | :--- |
| `[Service]` | 默认模式，方法调用进入 Actor 消息队列排队执行 | 所有业务方法 |
| `[ThreadSafe]` | 跳过消息队列，直接调用（要求方法线程安全） | 纯读操作、无状态计算 |
| `[Discard]` | 即发即弃，不等待返回值 | 日志、统计等不需要结果的场景 |
| `[TimeOut(ms)]` | 为消息队列调用设置超时时间 | 需要超时控制的长操作 |

```csharp
public class ServerComponentAgent : StateComponentAgent<ServerComponent, ServerState>
{
    // 进入 Actor 消息队列排队执行
    [Service]
    public virtual Task<bool> IsOnline(long roleId) { ... }

    // 跳过消息队列，线程安全直接调用
    [Service]
    [ThreadSafe]
    public virtual long FirstStartTime()
    {
        return State.FirstStartTime;
    }

    // 即发即弃，不阻塞调用方
    [Service]
    [Discard]
    public virtual ValueTask AddOnlineRole(long roleId)
    {
        OwnerComponent.OnlineSet.Add(roleId);
        return ValueTask.CompletedTask;
    }
}
```

### 配置表访问

使用 `ConfigComponent` 单例访问 Luban 生成的配置表：

```csharp
var config = ConfigComponent.Instance.GetConfig<TbItemConfig>();

// 使用 TryGet 安全查询
if (config.TryGet(itemId, out var itemConfig))
{
    // 使用 itemConfig.Name, itemConfig.Type 等
}
```

### 数据库操作

通过 `GameDb` 静态类进行 MongoDB CRUD 操作：

```csharp
// 查询
var state = await GameDb.FindAsync<LoginState>(
    m => m.UserName == userName && m.Password == password);

// 新增/更新
await GameDb.AddOrUpdateAsync(loginState);

// 列表查询
var list = await GameDb.FindListAsync<LoginState>(m => m.Id != 0);

// 删除
var count = await GameDb.DeleteAsync(state);
```

### 热更新机制

热更新系统允许在不停服的情况下替换业务逻辑。

- **Apps 层**（`GameFrameX.Apps`）：包含状态定义和组件外壳，**不可热更**
- **Hotfix 层**（`GameFrameX.Hotfix`）：包含所有业务逻辑，**可热更**
- **Hotfix 程序集**输出到 `hotfix/` 目录，运行时由 `HotfixManager` 加载

```bash
# 通过 HTTP 接口触发（指定版本号）
curl "http://localhost:28080/game/api/Reload?version=1.0.1"
```

### 架构分析器

`GameFrameX.Architecture.Analyzers` 是 Roslyn 分析器项目，用于在编译期强制约束 Apps/Hotfix 分层架构。它只作为 analyzer 引用到 `GameFrameX.Apps`、`GameFrameX.Hotfix` 和 `GameFrameX.Launcher`；`GameFrameX.Config`、`GameFrameX.Proto` 和测试程序集会按设计忽略。

| ID | 规则 |
| :--- | :--- |
| `GFX0001` | `BaseCacheState` 子类必须定义在 `GameFrameX.Apps`。 |
| `GFX0002` | `StateComponent<TState>` 子类必须定义在 `GameFrameX.Apps`。 |
| `GFX0003` | `BaseHttpHandler` 子类必须定义在 `GameFrameX.Hotfix`。 |
| `GFX0004` | `IHotfixBridge` 实现类必须定义在 `GameFrameX.Hotfix`。 |
| `GFX0005` | `IComponentAgent` 实现类必须定义在 `GameFrameX.Hotfix`。 |
| `GFX0006` | `[MessageMapping]` 处理器必须定义在 `GameFrameX.Hotfix`。 |
| `GFX0007` | `[MessageRpcMapping]` 处理器必须定义在 `GameFrameX.Hotfix`。 |
| `GFX0008` | `IEventListener` 实现类必须定义在 `GameFrameX.Hotfix`。 |
| `GFX0009` | `ITimerHandler` 实现类必须定义在 `GameFrameX.Hotfix`。 |
| `GFX0010` | `[MessageMapping]` 处理器必须标记为 `sealed`。 |
| `GFX0011` | `IEventListener` 实现类必须标记为 `sealed`。 |
| `GFX0012` | `[MessageMapping]` 处理器类型名必须以 `Handler` 结尾。 |
| `GFX0013` | `IEventListener` 实现类类型名必须以 `EventListener` 结尾。 |
| `GFX0014` | `IComponentAgent` 实现类类型名必须以 `ComponentAgent` 结尾。 |

## 文档与资源

### 配置覆盖参数

| 配置项 | 说明 | 默认值 | 示例 |
| :--- | :--- | :--- | :--- |
| `ServerType` | 服务器类型 | — | `Game` |
| `ServerId` | 服务器唯一标识 | — | `1000` |
| `IsEnableTcp` | 是否启用 TCP 服务 | `false` | `true` |
| `InnerPort` | TCP 内部通信端口 | — | `29100` |
| `IsEnableHttp` | 是否启用 HTTP 服务 | `false` | `true` |
| `HttpPort` | HTTP 服务端口 | `0` | `28080` |
| `IsEnableWebSocket` | 是否启用 WebSocket 服务 | `false` | `true` |
| `WsPort` | WebSocket 服务端口 | `0` | `29110` |
| `MetricsPort` | Prometheus 指标端口 | `0` | `29090` |
| `HttpIsDevelopment` | 是否启用 HTTP 开发模式 | `false` | `true` |
| `MinModuleId` | 游戏服务器处理的最小模块 ID | — | `10` |
| `MaxModuleId` | 游戏服务器处理的最大模块 ID | — | `9999` |
| `TagName` | 运行时标签名称 | — | `GameFrameX` |
| `DataBaseUrl` | MongoDB 连接字符串 | — | `mongodb://localhost:27017` |
| `DataBaseName` | 数据库名称 | — | `gameframex` |

> **[!NOTE]**
> 本地调试可以直接使用 `GameFrameX.Launcher/StartUp/AppStartUpGame.cs` 中的默认配置，不需要传参。上表列出的是常见覆盖参数。完整配置项（网络、日志、Actor、监控、安全等）请查阅 **[Configuration Management — GameFrameX.Server.Source](https://github.com/GameFrameX/GameFrameX.Server.Source#configuration-management)**。

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

### Docker 部署

```bash
docker-compose up --build
```

| 端口 | 协议 | 说明 |
| :--- | :--- | :--- |
| `29090` | HTTP | APM 指标 / 健康检查 |
| `29100` | TCP | 游戏客户端连接 |
| `29110` | WebSocket | WebSocket 连接 |
| `28080` | HTTP | HTTP API |

### 消息协议

消息 ID 由模块 ID 位移计算：`(moduleId << 16) + seqId`

| 模块 | ID 范围 | 文件 | 说明 |
| :--- | :--- | :--- | :--- |
| 系统 | -10 ~ -1 | `Player_-10.cs`, `Service_-3.cs` | 内置系统协议 |
| 基础 | 10 | `Basic_10.cs` | 心跳、服务器就绪通知 |
| 背包 | 100 | `Bag_100.cs` | 物品增删改查、使用、合成 |
| 用户 | 300 | `User_300.cs` | 登录、注册、角色列表 |

### 技术栈

| 组件 | 技术 |
| :--- | :--- |
| 运行时 | .NET 10.0 |
| 数据库 | MongoDB |
| 网络框架 | SuperSocket |
| 序列化 | protobuf-net |
| 配置生成 | Luban |
| 代码生成 | Roslyn Source Generator |
| 监控 | OpenTelemetry + Prometheus |
| 对象映射 | Mapster |
| 容器化 | Docker + Docker Compose |

## 社区与支持

- [官方文档](https://gameframex.doc.alianblank.com)
- [GitHub 组织](https://github.com/GameFrameX)
- [Gitee 镜像](https://gitee.com/GameFrameX)
- [问题反馈](https://github.com/GameFrameX/GameFrameX/issues)
- [社区讨论](https://github.com/GameFrameX/GameFrameX/discussions)

### 贡献指南

1. Fork 本仓库
2. 创建功能分支（`git checkout -b feature/amazing-feature`）
3. 提交更改（`git commit -m 'feat: 添加某个功能'`）
4. 推送到分支（`git push origin feature/amazing-feature`）
5. 创建 Pull Request

## 开源协议

本项目采用 [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0) 分发。详见 [LICENSE.md](LICENSE.md) 文件。
