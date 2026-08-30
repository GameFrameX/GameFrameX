<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/documentation-docs-blue.svg)](https://gameframex.doc.alianblank.com)

[![Trendshift](https://trendshift.io/api/badge/repositories/20145)](https://trendshift.io/repositories/20145)

인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현

<br />

[문서](https://gameframex.doc.alianblank.com) · [빠른 시작](#빠른-시작) · QQ 그룹: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>

## 프로젝트 개요

**게임을 「아이디어 → 만들기 → 출시 운영」까지 싹 다 해주는 오픈소스 툴박스**

게임 만들 때 진짜 까다로운 건 「캐릭터 그리기, 스킬 만들기」가 아니라, 이런 조각들을 하나로 끌어모으는 일이에요:

- 플레이어 세이브 데이터는 어디에 저장하고, 어떻게 불러올까?
- 멀티플레이할 때 서버는 메시지를 어떻게 중계할까?
- 아이템, 스테이지, 레벨 같은 데이터는 누가 관리하고, 기획자가 고치면 어떡하지?
- 출시 이후에는 데이터를 어떻게 보고, 플레이어를 어떻게 관리하고, 새 버전은 어떻게 배포할까?

이런 「삽질」은 GameFrameX가 다 알아서 해주니까, 당신은 「내 게임이 재밌는가」에만 집중하면 돼요.

### 기능

| 원래 직접 다뤄야 했던 일 | GameFrameX가 알아서 준비해주는 것 |
|---|---|
| 멀티플레이 서버를 처음부터 짜기 | 바로 쓸 수 있는 고성능 서버(.NET으로 작성, 동시 접속자도 잘 버템) |
| 데이터를 어떻게 저장할지 | 플레이어 데이터는 MongoDB에(빠른 읽기/쓰기), 백오피스 데이터는 PostgreSQL에(안정적) |
| Excel 설정을 코드로 손 옮기기 | LuBan으로 클릭 한 번에 Excel을 코드와 데이터로 변환 |
| 클라이언트와 서버 「암호 맞추기」 | ProtoBuf로 프로토콜을 통일하고, 한 곳을 고치면 양 끝이 동기화 |
| 출시 후 아무것도 모르는 상태 | 관리 백오피스 웹 페이지가 기본 탑재, 데이터 조회 / 플레이어 관리 / 설정 배포 |
| 서버 배포가 머리 아픔 | Docker로 원클릭 패키징 배포, 속 편함 |

> 쉽게 말해: **한 사람도 소규모 팀처럼 온라인 게임 하나를 만들고, 운영까지 이어갈 수 있어요.**

**누가 쓰면 좋을까요:**

- **온라인 / 네트워크 게임**을 만들고 싶은데 「서버를 어떻게 세우지」에서 막힌 인디 개발자
- 아이디어를 검증할 **게임 프로토타입**을 빠르게 만들고 싶은 소규모 팀
- 「클라이언트 + 서버 + 백오피스」 전체 흐름을 처음부터 끝까지 배우고 싶은 학습자

### 도입 사례

| 게임 이름 | 출시 채널 | 출시 시기 |
|:---|:---|:---|
| 심야의 숯불구이 (深夜的烧烤店) | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |
| 연속 흑백 (连续黑白) | 더우인(抖音), 콰이서우(快手), 알리페이, 하모니OS, TapTap, iOS 등 | 2024-11 |

> GameFrameX로 출시 작품을 만들었나요? PR이나 issue로 위 표에 추가해 주세요.

## 빠른 시작

**이 저장소가 곧 완전한 프로젝트입니다**: git clone, Code → Download ZIP, 미러 사이트 다운로드 — 어떤 방식으로 받아도 그대로 돌아갑니다. 추가로 받을 저장소가 없어요.

3단계면 됩니다 (자세한 건 아래 [튜토리얼](#설치) 참고):

```shell
# 1. 로컬 데이터베이스 실행 (MongoDB, 계정 admin / admin)
cd docker/mongo && docker compose up -d

# 2. 서버 빌드 후 실행 (덮어쓰는 건 DB 연결뿐, 포트는 기본값 사용)
cd ../../Server && dotnet build
cd bin/app_debug
dotnet GameFrameX.Launcher.dll --DataBaseUrl="mongodb://admin:admin@localhost:27017/?authSource=admin"

# 3. Unity 2019.4.40f1로 Unity/ 프로젝트 열고, Assets/Scenes/Launcher.unity 연 뒤 Play
```

로그인 화면이 뜨고 캐릭터를 만들어 메인 도시에 들어가면 클라이언트↔서버 전체 경로가 연결된 거예요.

서버가 켜졌는지 궁금하면? 리스닝 포트를 확인하세요: `nc -z localhost 29100`(TCP)과 `nc -z localhost 28080`(HTTP)이 성공하면 살아 있는 거예요. (29090은 메트릭 포트로 **기본 비활성화**입니다 — 아래 포트 표 참고.)

### 설치

따라 하면 10~15분 정도 걸려요 (Unity 첫 임포트 포함).

#### 1단계: 프로젝트 다운로드

```shell
git clone https://github.com/GameFrameX/GameFrameX.git
cd GameFrameX
```

git을 안 쓰고 싶다면? GitHub 페이지의 **Code → Download ZIP**, 또는 [gitee 등 미러 사이트](https://gitee.com/GameFrameX/GameFrameX)에서 받아도 똑같아요.

#### 2단계: 환경 설치

| 설치 | 버전 | 어디서 |
|---|---|---|
| **.NET SDK** | **10.0 이상** | https://dotnet.microsoft.com/download |
| **Unity 에디터** | **2019.4.40f1** (Unity Hub → Installs → Install Editor → Archive에서) | https://unity.com/download |
| **Docker Desktop** | 최신 버전 | https://www.docker.com/ |

> **주의**: .NET 10은 서버와 설정표 생성 도구의 필수 요구사항이에요. 여기 틀리면 뒤에서 전부 막힙니다.

#### 3단계: 로컬 데이터베이스 실행

```shell
cd docker/mongo
docker compose up -d
```

뜨는 건 MongoDB예요: `mongodb://admin:admin@localhost:27017` (데이터는 `docker/mongo/database/`에 저장).

> PostgreSQL(`docker/postgres/`)은 관리 백오피스 Admin용이에요. 이 튜토리얼에서는 필요 없으니 안 켜도 돼요.

#### 4단계: 서버 빌드 후 실행

```shell
cd ../../Server
dotnet build
cd bin/app_debug
dotnet GameFrameX.Launcher.dll --DataBaseUrl="mongodb://admin:admin@localhost:27017/?authSource=admin"
```

**인자가 하나뿐인 이유는?** 기본 설정(`Server/GameFrameX.Launcher/StartUp/AppStartUpGame.cs` 참고)이 포트를 전부 열어두고 있거든요:

| 포트 | 용도 | 기본값 |
|---|---|---|
| 29100 | TCP: 게임 클라이언트 장기 연결 | 기본 활성화 |
| 28080 | HTTP: 로그인 등 API (`/game/api/...`) | 기본 활성화 |
| 29110 | WebSocket | 기본 비활성화, `--IsEnableWebSocket true`로 활성화 |
| 29090 | 헬스 체크 / 메트릭 | 기본 비활성화, `--IsOpenTelemetryMetrics true --MetricsPort 29090`로 활성화 |

덮어써야 할 건 `DataBaseUrl` 하나 — 기본값은 공개 데모 DB를 가리키니, 로컬 개발에서는 방금 띄운 MongoDB로 향하게 하세요.

**IDE면 더 간단해요**: Rider / Visual Studio로 `Server/Server.slnx` 열고 (`.slnx` 미지원이면 `Server.sln`), 시작 프로젝트를 `GameFrameX.Launcher`로, **Working directory를 `Server/bin/app_debug`로** 설정하고 명령줄 인자는 비워두세요 → 대신 `AppStartUpGame.cs`의 `DataBaseUrl` 기본값을 로컬 연결 문자열로 고치세요 (통합 저장소 내 파일 편집은 로컬 디버깅 전용. 위의 덮어쓰기 주의 참고).

**확인**: 터미널에서 `nc -z localhost 29100 && nc -z localhost 28080` — 성공하면 살아 있는 거예요 (또는 서버 로그에서 `has been started` / `Now listening on` 키워드를 확인하세요).

#### 5단계: Unity 클라이언트 접속

1. Unity Hub에서 **2019.4.40f1**로 저장소의 `Unity/` 폴더 열기 (처음엔 Package를 자동으로 받아와서 인터넷 필요, 좀 기다리세요)
2. 씬 `Assets/Scenes/Launcher.unity` 열기
3. **Play** 누르기

클라이언트는 기본으로 `127.0.0.1`(TCP 29100 / HTTP 28080)에 접속하고, 서버 기본 포트와 딱 맞아떨어져요. 설정을 바꿀 필요 없어요. 로그인 화면이 뜨고 캐릭터를 만들어 메인 도시에 들어가면 튜토리얼 완료!

> 다른 머신 / 원격 서버로 옮길 땐 두 곳을 고쳐요: TCP 주소는 `Unity/Assets/Hotfix/UI/Logic/UILogin/UIPlayerList.cs`(`serverIp` / `serverPort`), HTTP 주소는 `Unity/Assets/Hotfix/UI/Logic/UILogin/UILogin.cs` 등(`127.0.0.1:28080` 검색).

#### LayaAir 클라이언트를 쓰고 싶다면?

LayaAir IDE로 `LayaBox/`를 여세요. 진입점은 `src/Main.ts`. 주의점 두 가지: WebSocket은 **기본 비활성화**예요 — 먼저 서버를 `--IsEnableWebSocket true`로 시작하세요 (기본 WsPort는 29110인데, `nettest.ts` 기본값은 `ws://127.0.0.1:21100`이라 서로 안 맞아요 — 맞춰야 합니다). 접속 주소는 `LayaBox/src/gameframex/nettest.ts`에 있고, 프로토콜 생성은 `Protobuf/Proto2TsExport_LayaBox.sh`를 써요.

## 사용 예시

다운로드한 스냅샷에는 **생성된 산출물이 전부 들어 있어요** (설정 코드 / 데이터, 프로토콜 코드 — 다 갖춰져 있음) 그래서 바로 돌아가요. 다시 생성해야 하는 건 원본 파일을 고쳤을 때뿐이에요:

### Excel 설정을 고쳤다면 (`Config/Excels/Tables/`의 표)

| 고친 것 | 실행 | 산출물 위치 |
|---|---|---|
| 서버가 읽는 표 | `cd Config && sh gen-server-bin.sh` (Windows는 `gen-server-bin.bat` 더블클릭) | `Server/GameFrameX.Config/` |
| 클라이언트가 읽는 표 | `cd Config && sh gen-client-json.sh` | `Unity/Assets/` (코드 + 데이터) |

> 파일 이름에 규칙이 있어요: `영문자-영어이름-중국어이름.xlsx` (예: `D-ItemConfig-道具表-道具-1001.xlsx`). Excel의 앞 4행은 헤더(`##var` / `##type` / `##group` / 설명)고, 데이터는 5행부터예요. 자세한 규칙은 [GameFrameX.Config](https://github.com/GameFrameX/GameFrameX.Config)에서.

### 통신 프로토콜을 고쳤다면 (`Protobuf/*.proto`)

내보내기 도구는 저장소에 포함되지 않아요. 먼저 한 번 빌드하세요 (통합 저장소의 디렉터리 배치는 출력 경로 요건을 이미 만족해요):

```shell
cd Tools
dotnet build ProtoExport/ProtoExport.csproj -c Release   # 산출물은 ../Protobuf/Tools/로 자동 출력
cd ../Protobuf
sh Proto2CsExport_Server.sh    # 서버용 프로토콜 → Server/GameFrameX.Proto/
sh Proto2CsExport_Client.sh    # 클라이언트용 프로토콜 → Unity/Assets/Hotfix/Proto/
```

> 프로토콜 엄격 규칙: proto3만 지원. `option module = 10;` 필수. 메시지 이름은 `Req<이름>` / `Resp<이름>` / `Notify<이름>`. 필드 번호는 800 미만. 중첩 message 금지. 자세한 규칙은 [GameFrameX.Protobuf](https://github.com/GameFrameX/GameFrameX.Protobuf)에서.

### UI를 고쳤다면 (FairyGUI)

FairyGUI 에디터(≥5.0)로 `FairyGUIProject/Game.fairy`를 열고, 수정 뒤 **파일 → 퍼블리시, 「코드 생성」에 반드시 체크**하세요. 산출물은 `Unity/Assets/`(UI 에셋 + C# 바인딩 코드)로 자동 기록돼요.

> 초보자 최다 문제: 퍼블리시 후 Unity에서 클래스를 못 찾는 에러 → 십중팔구 「코드 생성」 체크를 깜빡한 거예요.

### 자주 겪는 함정

| 증상 | 원인 & 해결 |
|---|---|
| 서버 시작 시 DB 연결 에러 | `DataBaseUrl`을 안 넘김 — 기본값은 공개 데모 DB를 가리켜요. 4단계의 로컬 연결 문자열을 넘기세요 |
| IDE 실행이 크래시 / hotfix를 못 찾음 | Working directory가 `Server/bin/app_debug`가 아님 (서버는 「현재 디렉터리/hotfix」에서 핫업데이트 어셈블리를 로드해요) |
| Unity 첫 오픈이 패키지 받기에서 멈춤 | UPM 프라이빗 레지스트리(`gameframex.upm.alianblank.uk`)와 gitee(HybridCLR)에 인터넷 접근이 필요해요. 제한된 네트워크에선 멈춥니다 |
| 클라이언트가 서버에 연결 안 됨 | 포트 조합이 맞는지 확인: TCP 29100 / HTTP 28080. WebSocket 29110은 `--IsEnableWebSocket true`가 필요해요 (기본 비활성화). 서버 로그에 리스닝 목록이 나와요 |
| 이 저장소에서 코드 고쳤는데 다음 날 사라짐 | 통합 저장소는 매일 동기화로 덮어씁니다. 변경은 해당 소스 저장소에 커밋하세요 |
| LayaBox가 연결 안 됨 | WebSocket은 기본 비활성화 — 서버를 `--IsEnableWebSocket true`로 시작하세요. `nettest.ts` 기본 포트 21100도 서버 WsPort 29110과 맞추세요 |

## 아키텍처

이 저장소는 **통합 릴리스 저장소**예요 — 아래 7개 소스 저장소의 최신 코드를 매일 자동으로 같은 이름의 폴더에 동기화합니다. 한 번 받으면 모든 부품을 손에 넣고, **폴더도 처음부터 제자리에 있어요** (설정 생성과 프로토콜 내보내기는 상대 경로로 서로를 찾습니다 — 이름 바꾸거나 옮기지 마세요):

```
GameFrameX/                   # 프로젝트 루트
├── Server/                   # 게임 서버 (.NET 10, Actor 모델 + 핫업데이트)
├── Unity/                    # Unity 클라이언트 프로젝트 (HybridCLR 핫업데이트, YooAsset)
├── LayaBox/                  # LayaAir 클라이언트 프로젝트 (대체 클라이언트)
├── Config/                   # LuBan 설정표: 여기서 Excel 편집, 양 끝 코드를 한 번에 생성
├── Protobuf/                 # 통신 프로토콜: 여기서 .proto 편집, 각 단말용 코드를 한 번에 내보내기
├── FairyGUIProject/          # UI 편집 프로젝트 (FairyGUI 에디터에서 Game.fairy 열기)
├── Tools/                    # 보조 도구 (프로토콜 내보내기 CLI / GUI)
├── docker/                   # 로컬 데이터베이스 원커맨드 실행 (mongo / postgres)
├── scripts/                  # 통합 동기화 스크립트
└── README / LICENSE 등
```

| 디렉터리 | 대응 소스 저장소 (변경은 여기로 PR / Issue 보내세요) |
|------|------|
| `Server/` | https://github.com/GameFrameX/GameFrameX.Server |
| `Unity/` | https://github.com/GameFrameX/GameFrameX.Unity |
| `LayaBox/` | https://github.com/GameFrameX/GameFrameX.LayaBox |
| `Config/` | https://github.com/GameFrameX/GameFrameX.Config |
| `Protobuf/` | https://github.com/GameFrameX/GameFrameX.Protobuf |
| `FairyGUIProject/` | https://github.com/GameFrameX/GameFrameX.FairyGUIProject |
| `Tools/` | https://github.com/GameFrameX/GameFrameX.Tools |

> **주의**: **이 저장소의 `Server/`, `Unity/` 등을 직접 고치는 건 소용없어요** — 매일 자동 동기화가 덮어써 버립니다. 코드를 고치거나 PR을 보내려면 위 표의 해당 소스 저장소로 가세요.

**통합하지 않는 저장소** (필요할 때 가져가세요):

| 저장소 | 설명 |
|------|------|
| [GameFrameX.Foundation](https://github.com/GameFrameX/GameFrameX.Foundation) | 서버 기반 라이브러리, NuGet 패키지로 Server에서 참조 (빌드 시 자동 복원, clone 불필요) |
| [GameFrameX.Admin](https://github.com/GameFrameX/GameFrameX.Admin) | 관리 백오피스 (일부 소스 비공개), [라이브 데모](https://game.admin.web.vue.alianblank.com) |
| [GameFrameX.CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) / [Godot](https://github.com/GameFrameX/GameFrameX.Godot) | 다른 엔진 클라이언트 |
| [GameFrameX.Docs](https://github.com/GameFrameX/GameFrameX.Docs) | 문서 사이트 소스 |

## 플랫폼 지원

다음 주요 엔진을 지원합니다——어느 것을 써도 대응됩니다:

| 플랫폼 | 클라이언트 프로젝트 | 비고 |
|---|---|---|
| Unity | 본 저장소 내 `Unity/` | **2019.4.40f1**, HybridCLR 핫업데이트, YooAsset — 주 클라이언트 |
| LayaAir (LayaBox) | 본 저장소 내 `LayaBox/` | 대체 클라이언트, 진입점 `src/Main.ts` |
| Cocos Creator | [GameFrameX.CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) | 별도 저장소 |
| Godot | [GameFrameX.Godot](https://github.com/GameFrameX/GameFrameX.Godot) | 별도 저장소 |
| 서버 | 본 저장소 내 `Server/` | .NET 10, Actor 모델, 모든 클라이언트 공용 |

## 의존성

| 구성 요소 | 버전 | 용도 |
|------|------|------|
| **.NET SDK** | **10.0+** | 서버 빌드 및 실행 (Foundation 의존성은 NuGet으로 자동 복원, 첫 빌드 시 인터넷 필요) |
| **Unity** | **2019.4.40f1** | 클라이언트 `Unity/` 열기 (첫 임포트 시 Package를 가져오므로 인터넷 필요) |
| **Docker** | 최신 버전 | 로컬 MongoDB를 원커맨드로 실행 |

## 문서 및 자료

> 모든 사이트의 내용은 같아요. 열리는 아무거나 쓰세요.

- 메인: https://gameframex.doc.alianblank.com
- 미러 1: https://gameframex-docs.pages.dev
- 미러 2: https://gameframex.doc.cloudflare.alianblank.com
- 미러 3: https://gameframex.doc.vercel.alianblank.com

## 커뮤니티 및 지원

- [Discord](https://discord.gg/VDWUjWMDw9)
- [GitHub](https://github.com/GameFrameX/gameframex)
- [LinkedIn](https://www.linkedin.com/in/alianblank)
- [Reddit](https://www.reddit.com/r/GameFrameX/)
- [X](https://x.com/alian_blank)
- [YouTube](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
- [Bluesky](https://bsky.app/profile/alianblank.bsky.social)
- [Bilibili](https://www.bilibili.com/video/BV1yrpeepEn7)
- [Gitee](https://gitee.com/GameFrameX/gameframex)
- QQ 그룹: **467608841 / 233840761**

### 스폰서

![wechat.jpg](https://raw.githubusercontent.com/GameFrameX/GameFrameX/42e755df/Docs/imgs/wechat.jpg)

[AITKPARTY](https://aitkparty.com/)는 오픈소스 프로젝트 New API 기반으로 구축된 AI LLM API 중계/통합 서비스예요. 주요 대형 언어 모델에 대한 통일된 인터페이스를 제공해서, 여러 모델 공급사를 각각 연동하는 수고를 덜어줍니다.

### 기여자

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

### Star History

[![Star History Chart](https://star-history.dera.page/svg?repos=GameFrameX/GameFrameX,GameFrameX/GameFrameX.Unity,GameFrameX/GameFrameX.Server&type=date)](https://star-history.dera.page/#GameFrameX/GameFrameX&GameFrameX/GameFrameX.Unity&GameFrameX/GameFrameX.Server&type=date&legend=top-left)

## 변경 로그

[GitHub Releases](https://github.com/GameFrameX/GameFrameX/releases) 페이지를 참조하세요.

## 라이선스

[LICENSE.md](LICENSE.md)를 참조하세요.

> 모든 플러그인은 인터넷에서 온 것이며, 사용 시 각자 결제하세요. 권리를 침해받았다면 email로 알려주세요. 바로 내리겠습니다.
>
> 이 프로젝트는 현지 법률이 허용하지 않는 범위에서 사용해서는 안 됩니다. 기술 자체는 무죄이며, 남용하는 사람이 잘못입니다.
