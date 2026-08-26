<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX Server

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-Apache%202.0-orange.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex.doc.alianblank.com-brightgreen.svg)](https://gameframex.doc.alianblank.com)

**인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현**

[📖 문서](https://gameframex.doc.alianblank.com) · [🚀 빠른 시작](#빠른-시작) · [💬 QQ 그룹: 467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)

---

🌐 **언어**: [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

---

</div>

## 프로젝트 개요

GameFrameX Server는 C# .NET 10.0 기반의 고성능 게임 서버 프레임워크로, **Actor 모델** 아키텍처와 **핫 업데이트**를 지원합니다. 이 프레임워크는 영속 상태와 비즈니스 로직의 엄격한 분리를 적용하며, 멀티플레이어 온라인 게임 시나리오에 특별히 설계되었습니다.

> 설계 철학: 복잡함보다는 단순함

## 핵심 기능

- **Actor 모델** — TPL DataFlow 기반의 락-프리 고동시성 아키텍처로, 메시지 전달을 통해 전통적인 락 경합을 방지합니다
- **상태-로직 분리** — 영속 데이터(Apps 레이어)와 핫 업데이트 가능한 비즈니스 로직(Hotfix 레이어)의 엄격한 분리
- **무중단 핫 업데이트** — 서버 재시작 없이 런타임에 비즈니스 로직 어셈블리 교체 가능
- **멀티 프로토콜 네트워킹** — TCP, WebSocket, HTTP 프로토콜 지원, 내장 메시지 인코딩/디코딩 및 압축 기능
- **MongoDB 영속화** — CacheState 기반 투명 ORM 매핑으로 자동 직렬화/역직렬화 지원
- **소스 제너레이터** — Roslyn 기반 Agent 코드 생성으로 자동 Actor 메시지 큐 스케줄링
- **설정 테이블 시스템** — Luban 설정 생성 통합 및 JSON 핫 리로딩 지원
- **OpenTelemetry** — 내장 Prometheus 메트릭 내보내기, 헬스 체크 및 성능 모니터링

## 아키텍처

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

## 빠른 시작

### 사전 요구 사항

| 종속성 | 버전 |
| :--- | :--- |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ |
| [MongoDB](https://www.mongodb.com/try/download/community) | 4.x+ |
| IDE | Visual Studio 2022 / JetBrains Rider / VS Code |

### 설치 및 실행

로컬 개발 기본 규칙:

- 먼저 `Server.slnx`를 여세요. 더 간결하고 리뷰하기 쉽습니다. IDE가 `.slnx`를 지원하지 않으면 `Server.sln`을 여세요.
- 일반적인 로컬 디버깅에서는 `Program arguments` / `Command line arguments`를 비워 둡니다. 기본 설정은 `GameFrameX.Launcher/StartUp/AppStartUpGame.cs`에 있습니다.
- 포트나 MongoDB를 바꾸는 등 기본값을 재정의해야 할 때만 시작 인자를 전달합니다.

#### 방법 1: Docker로 한 번에 실행

```bash
git clone https://github.com/GameFrameX/Server_main.git
cd Server_main
docker compose up -d --build
```

#### 방법 2: 터미널에서 로컬 실행

먼저 MongoDB를 시작합니다.

```bash
docker compose up -d mongodb
```

그 다음 빌드하고 출력 디렉터리에서 Launcher를 실행합니다.

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

#### 방법 3: Rider로 디버그

1. `Server.slnx`를 엽니다. Rider가 `.slnx`를 지원하지 않으면 `Server.sln`을 여세요.
2. MongoDB를 시작합니다: `docker compose up -d mongodb`.
3. `GameFrameX.Launcher`의 `.NET Project` 실행 구성을 만들거나 수정합니다.
4. `Working directory`를 `$SolutionDir$/bin/app_debug`로 설정합니다.
5. `Program arguments`를 비워 둡니다.
6. Debug를 실행합니다. `bin/app_debug`가 없다면 먼저 `dotnet build Server.sln`을 한 번 실행하세요.

#### 방법 4: Visual Studio로 디버그

1. `Server.slnx`를 엽니다. Visual Studio가 `.slnx`를 지원하지 않으면 `Server.sln`을 여세요.
2. `GameFrameX.Launcher`를 시작 프로젝트로 설정합니다.
3. MongoDB를 시작합니다: `docker compose up -d mongodb`.
4. `Working directory`를 솔루션 아래의 `bin\app_debug`로 설정합니다.
5. `Command line arguments`를 비워 둡니다.
6. F5로 디버그를 시작합니다. 출력 디렉터리가 없다면 먼저 Build Solution을 실행하세요.

#### 필요할 때만 기본값 재정의

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

### 배포 확인

| 엔드포인트 | URL | 설명 |
| :--- | :--- | :--- |
| 헬스 체크 | `http://localhost:29090/health` | 서비스 상태 확인 |
| 메트릭 | `http://localhost:29090/metrics` | Prometheus 메트릭 |
| 테스트 API | `http://localhost:28080/game/api/test` | HTTP 연결 테스트 |

## 프로젝트 구조

```
Server_main/
├── GameFrameX.Launcher/         # 애플리케이션 진입점 (실행 파일)
│   ├── Program.cs               # 부트스트랩: 상태 타입 및 프로토콜 메시지 등록
│   └── StartUp/
│       └── AppStartUpGame.cs    # 게임 서버 시작 흐름
│
├── GameFrameX.Hotfix/           # 핫 업데이트 레이어 (비즈니스 로직, 런타임 교체 가능)
│   ├── Logic/
│   │   ├── Http/                # HTTP 요청 핸들러
│   │   │   ├── TestHttpHandler.cs
│   │   │   ├── ReloadHttpHandler.cs
│   │   │   ├── Player/          # 계정 로그인, 온라인 조회
│   │   │   └── Bag/             # 아이템 발송
│   │   ├── Player/
│   │   │   ├── Bag/             # 가방 컴포넌트 에이전트
│   │   │   ├── Login/           # 로그인/로그아웃 로직
│   │   │   └── Pet/             # 펫 컴포넌트 에이전트
│   │   ├── Account/             # 계정 컴포넌트 에이전트
│   │   └── Server/              # 서버 글로벌 컴포넌트 에이전트
│   └── StartUp/                 # 핫 업데이트 시작 흐름
│       ├── AppStartUpHotfixGameByEntry.cs    # 진입점
│       ├── AppStartUpHotfixGameByMain.cs     # 네트워크/연결 관리
│       ├── AppStartUpHotfixGameByHeart.cs    # 하트비트 처리
│       └── AppStartUpHotfixGameByGateWay.cs  # 게이트웨이 통신
│
├── GameFrameX.Apps/             # 애플리케이션 상태 레이어 (핫 업데이트 불가)
│   ├── ActorType.cs             # Actor 타입 열거형 정의
│   ├── Account/Login/           # 로그인 상태 + 컴포넌트
│   ├── Player/
│   │   ├── Bag/                 # 가방 상태 + 컴포넌트
│   │   ├── Player/              # 플레이어 상태 + 컴포넌트
│   │   └── Pet/                 # 펫 상태 + 컴포넌트
│   ├── Server/                  # 서버 글로벌 상태 + 컴포넌트
│   └── Common/
│       ├── Session/             # 세션 관리 (SessionManager)
│       ├── Event/               # 이벤트 ID 정의
│       └── EventData/           # 이벤트 인자
│
├── GameFrameX.Config/           # 설정 테이블 시스템 (Luban 생성)
│   ├── ConfigComponent.cs       # 설정 로딩 싱글톤
│   ├── Tables/                  # 생성된 설정 테이블 클래스
│   ├── TablesItem/              # 설정 데이터 모델
│   └── json/                    # JSON 설정 데이터 파일
│
├── GameFrameX.Proto/            # 네트워크 프로토콜 정의
│   ├── Basic_10.cs              # 기본 프로토콜 (하트비트 등)
│   ├── Bag_100.cs               # 가방 프로토콜
│   ├── User_300.cs              # 사용자/계정 프로토콜
│   └── BuiltIn/                 # 내장 시스템 프로토콜
│
├── GameFrameX.Architecture.Analyzers/        # 컴파일 타임 아키텍처 규칙
│
├── Server.slnx                  # 새 IDE에 권장하는 솔루션 파일
├── Server.sln                   # .slnx 미지원 IDE용 대체 솔루션
├── Dockerfile                   # Docker 다단계 빌드
├── docker-compose.yml           # Docker Compose 오케스트레이션
└── LICENSE.md                   # Apache License 2.0
```

## 사용 예시

### 핵심 패턴: State - Component - Agent

GameFrameX는 서버 무중단으로 비즈니스 로직을 핫 업데이트할 수 있는 3계층 분리 패턴을 사용합니다.

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   State      │────▶│  Component   │◀────│ ComponentAgent│
│  (Data Def)  │     │   (Shell)    │     │  (Logic)     │
│  Apps Layer  │     │  Apps Layer  │     │ Hotfix Layer │
│  Not Hot-fix │     │  Not Hot-fix │     │  Hot-fixable │
└──────────────┘     └──────────────┘     └──────────────┘
```

#### 1단계: State 정의 (Apps 레이어)

`CacheState`를 상속하여 영속 데이터 구조를 정의합니다. 프레임워크가 MongoDB 직렬화를 자동으로 처리합니다.

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

#### 2단계: Component 생성 (Apps 레이어)

`StateComponent<TState>`를 상속하여 상태와 로직 간의 브릿지 역할을 합니다.

```csharp
// GameFrameX.Apps/Player/Bag/Component/BagComponent.cs
[ComponentType(GlobalConst.ActorTypePlayer)]
public class BagComponent : StateComponent<BagState> { }
```

#### 3단계: 비즈니스 로직 구현 (Hotfix 레이어)

`StateComponentAgent<TComponent, TState>`를 상속하여 핫 업데이트 가능한 비즈니스 코드를 작성합니다.

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

### HTTP 핸들러

`BaseHttpHandler`를 상속하고 `[HttpMessageMapping]`으로 등록합니다.

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

### RPC 메시지 핸들러

프레임워크는 자동 ComponentAgent 주입이 포함된 두 가지 RPC 핸들러 기본 클래스를 제공합니다:

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

### 이벤트 처리

`[Event]` 속성을 사용하여 이벤트 ID를 바인딩합니다. 프레임워크가 자동으로 해당 ComponentAgent에 디스패치합니다.

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

### Agent 메서드 속성

Roslyn 소스 제너레이터가 Actor 메시지 큐 스케줄링을 자동으로 처리합니다. 속성을 통해 호출 동작을 제어합니다:

| 속성 | 설명 | 사용 사례 |
| :--- | :--- | :--- |
| `[Service]` | 기본 모드; 메서드 호출이 Actor 메시지 큐에 enqueue됩니다 | 모든 비즈니스 메서드 |
| `[ThreadSafe]` | 메시지 큐를 건너뛰고 직접 호출합니다 (스레드 안전 구현 필요) | 읽기 전용 작업, 무상태 연산 |
| `[Discard]` | 실행 후 결과를 기다리지 않습니다 | 로깅, 통계 등 결과가 필요 없는 작업 |
| `[TimeOut(ms)]` | 메시지 큐 호출에 대한 타임아웃을 설정합니다 | 타임아웃 제어가 필요한 긴 작업 |

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

### 설정 테이블 접근

`ConfigComponent` 싱글톤을 사용하여 Luban 생성 설정 테이블에 접근합니다:

```csharp
var config = ConfigComponent.Instance.GetConfig<TbItemConfig>();

// Safe query with TryGet
if (config.TryGet(itemId, out var itemConfig))
{
    // Use itemConfig.Name, itemConfig.Type, etc.
}
```

### 데이터베이스 작업

`GameDb` 정적 클래스를 사용하여 MongoDB CRUD 작업을 수행합니다:

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

### 핫 업데이트 메커니즘

핫 업데이트 시스템은 서버를 중지하지 않고 비즈니스 로직을 교체할 수 있습니다.

- **Apps 레이어** (`GameFrameX.Apps`): 상태 정의와 컴포넌트 셸을 포함 — **핫 업데이트 불가**
- **Hotfix 레이어** (`GameFrameX.Hotfix`): 모든 비즈니스 로직을 포함 — **핫 업데이트 가능**
- **Hotfix 어셈블리**는 `hotfix/` 디렉토리에 출력되며, 런타임에 `HotfixManager`에 의해 로드됩니다

```bash
# Trigger via HTTP endpoint (with version number)
curl "http://localhost:28080/game/api/Reload?version=1.0.1"
```

### 아키텍처 분석기

`GameFrameX.Architecture.Analyzers`는 Apps/Hotfix 아키텍처를 컴파일 시점에 강제하는 Roslyn analyzer 프로젝트입니다. analyzer로 참조되는 프로젝트는 `GameFrameX.Apps`, `GameFrameX.Hotfix`, `GameFrameX.Launcher`뿐이며, `GameFrameX.Config`, `GameFrameX.Proto`, 테스트 어셈블리는 설계상 무시됩니다.

| ID | 규칙 |
| :--- | :--- |
| `GFX0001` | `BaseCacheState` 하위 클래스는 `GameFrameX.Apps`에 정의해야 합니다. |
| `GFX0002` | `StateComponent<TState>` 하위 클래스는 `GameFrameX.Apps`에 정의해야 합니다. |
| `GFX0003` | `BaseHttpHandler` 하위 클래스는 `GameFrameX.Hotfix`에 정의해야 합니다. |
| `GFX0004` | `IHotfixBridge` 구현 클래스는 `GameFrameX.Hotfix`에 정의해야 합니다. |
| `GFX0005` | `IComponentAgent` 구현 클래스는 `GameFrameX.Hotfix`에 정의해야 합니다. |
| `GFX0006` | `[MessageMapping]` 핸들러는 `GameFrameX.Hotfix`에 정의해야 합니다. |
| `GFX0007` | `[MessageRpcMapping]` 핸들러는 `GameFrameX.Hotfix`에 정의해야 합니다. |
| `GFX0008` | `IEventListener` 구현 클래스는 `GameFrameX.Hotfix`에 정의해야 합니다. |
| `GFX0009` | `ITimerHandler` 구현 클래스는 `GameFrameX.Hotfix`에 정의해야 합니다. |
| `GFX0010` | `[MessageMapping]` 핸들러는 `sealed`여야 합니다. |
| `GFX0011` | `IEventListener` 구현 클래스는 `sealed`여야 합니다. |
| `GFX0012` | `[MessageMapping]` 핸들러 타입 이름은 `Handler`로 끝나야 합니다. |
| `GFX0013` | `IEventListener` 구현 타입 이름은 `EventListener`로 끝나야 합니다. |
| `GFX0014` | `IComponentAgent` 구현 타입 이름은 `ComponentAgent`로 끝나야 합니다. |

## 문서 및 자료

### 기본 설정 재정의

| 매개변수 | 설명 | 기본값 | 예시 |
| :--- | :--- | :--- | :--- |
| `ServerType` | 서버 타입 | — | `Game` |
| `ServerId` | 고유 서버 ID | — | `1000` |
| `IsEnableTcp` | TCP 서비스 활성화 여부 | `false` | `true` |
| `InnerPort` | TCP 내부 포트 | — | `29100` |
| `IsEnableHttp` | HTTP 서비스 활성화 여부 | `false` | `true` |
| `HttpPort` | HTTP 서비스 포트 | `0` | `28080` |
| `IsEnableWebSocket` | WebSocket 서비스 활성화 여부 | `false` | `true` |
| `WsPort` | WebSocket 서비스 포트 | `0` | `29110` |
| `MetricsPort` | Prometheus 메트릭 포트 | `0` | `29090` |
| `HttpIsDevelopment` | HTTP 개발 모드 활성화 여부 | `false` | `true` |
| `MinModuleId` | 게임 서버가 처리하는 최소 모듈 ID | — | `10` |
| `MaxModuleId` | 게임 서버가 처리하는 최대 모듈 ID | — | `9999` |
| `TagName` | 런타임 태그 이름 | — | `GameFrameX` |
| `DataBaseUrl` | MongoDB 연결 문자열 | — | `mongodb://localhost:27017` |
| `DataBaseName` | 데이터베이스 이름 | — | `gameframex` |

> **[!NOTE]**
> 로컬 디버깅은 `GameFrameX.Launcher/StartUp/AppStartUpGame.cs`의 기본 설정을 사용할 수 있으므로 인자가 필요 없습니다. 위 표는 자주 쓰는 재정의 매개변수입니다. 전체 설정 항목(네트워크, 로그, Actor, 모니터링, 보안 등)은 **[Configuration Management — GameFrameX.Server.Source](https://github.com/GameFrameX/GameFrameX.Server.Source#configuration-management)**를 참조하세요.

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

### Docker 배포

```bash
docker-compose up --build
```

| 포트 | 프로토콜 | 설명 |
| :--- | :--- | :--- |
| `29090` | HTTP | APM 메트릭 / 헬스 체크 |
| `29100` | TCP | 게임 클라이언트 연결 |
| `29110` | WebSocket | WebSocket 연결 |
| `28080` | HTTP | HTTP API |

### 메시지 프로토콜

메시지 ID는 모듈 ID 시프트로 계산됩니다: `(moduleId << 16) + seqId`

| 모듈 | ID 범위 | 파일 | 설명 |
| :--- | :--- | :--- | :--- |
| 시스템 | -10 ~ -1 | `Player_-10.cs`, `Service_-3.cs` | 내장 시스템 프로토콜 |
| 기본 | 10 | `Basic_10.cs` | 하트비트, 서버 준비 알림 |
| 가방 | 100 | `Bag_100.cs` | 아이템 CRUD, 사용, 합성 |
| 사용자 | 300 | `User_300.cs` | 로그인, 회원가입, 캐릭터 목록 |

### 기술 스택

| 구성 요소 | 기술 |
| :--- | :--- |
| 런타임 | .NET 10.0 |
| 데이터베이스 | MongoDB |
| 네트워킹 | SuperSocket |
| 직렬화 | protobuf-net |
| 설정 생성 | Luban |
| 코드 생성 | Roslyn Source Generator |
| 모니터링 | OpenTelemetry + Prometheus |
| 객체 매핑 | Mapster |
| 컨테이너화 | Docker + Docker Compose |

## 커뮤니티 및 지원

- [공식 문서](https://gameframex.doc.alianblank.com)
- [GitHub 조직](https://github.com/GameFrameX)
- [Gitee 미러](https://gitee.com/GameFrameX)
- [이슈 트래커](https://github.com/GameFrameX/GameFrameX/issues)
- [디스커션](https://github.com/GameFrameX/GameFrameX/discussions)

### 기여 방법

1. 이 저장소를 Fork합니다
2. 기능 브랜치를 생성합니다 (`git checkout -b feature/amazing-feature`)
3. 변경 사항을 커밋합니다 (`git commit -m 'feat: add some feature'`)
4. 브랜치에 푸시합니다 (`git push origin feature/amazing-feature`)
5. Pull Request를 생성합니다

## 라이선스

이 프로젝트는 [Apache 라이선스 2.0](https://www.apache.org/licenses/LICENSE-2.0)을 따릅니다. 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하십시오.
