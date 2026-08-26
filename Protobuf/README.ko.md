<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX.Protobuf

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Protobuf?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Protobuf/releases)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE.md)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)
[![CI](https://github.com/GameFrameX/GameFrameX.Protobuf/actions/workflows/proto-export.yml/badge.svg)](https://github.com/GameFrameX/GameFrameX.Protobuf/actions/workflows/proto-export.yml)

**인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현**

<br />

[문서](https://gameframex.doc.alianblank.com) · [빠른 시작](#빠른-시작) · [다국어 릴리스](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest) · QQ 그룹: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>

## 프로젝트 개요

GameFrameX.Protobuf는 GameFrameX 프레임워크의 통일된 네트워크 프로토콜 정의 리포지토리입니다. Protocol Buffers 3(`proto3`)를 채택하여, 메시지와 에러 코드 정의를 비즈니스 모듈별로 정리합니다. 각 `.proto` 파일은 숫자 모듈 ID(파일명 접미사)로 식별되며, 클라이언트와 서버 간의 메시지 라우팅 및 에러 코드 생성에 사용됩니다.

코드 생성은 [GameFrameX.Tools `ProtoExport`](https://github.com/GameFrameX/GameFrameX.Tools) 도구가 담당합니다. 다음 세 가지 워크플로 중 자신에게 맞는 것을 고르세요:

- **CI(설정 불필요)** —— 모든 `push` 시에 모든 언어로 자동 내보내기하여 롤링 [`latest` Release](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest)에 게시합니다. 다운로드만 하면 됩니다.
- **Docker** —— `docker run gameframex/gameframex-tools:latest ...` 한 줄이면 도구 체인 설치가 필요 없습니다.
- **로컬 스크립트** —— `Tools/ProtoExport`(.NET 10)를 직접 빌드하여 산출물을 이 리포지토리의 `Tools/` 디렉터리에 넣은 뒤 `Proto2*Export.sh/.bat` 스크립트를 실행합니다. 자세한 내용은 [내보내기 도구](#내보내기-도구)를 참조하세요.

전체 문서는 [GameFrameX 문서 사이트](https://gameframex.doc.alianblank.com/protobuf/require)에서 제공됩니다.

## 프로토콜 모듈

| Proto 파일 | 모듈 | 설명 |
|------------|------|------|
| `_0002_InnerBasic.proto` | 2 | 내부 기본 프로토콜 |
| `_0010_Basic.proto` | 10 | 기본 프로토콜 |
| `_0020_Common.proto` | 20 | 공용 프로토콜(에러 코드, 공유 타입) |
| `_0100_Bag.proto` | 100 | 가방(인벤토리) 프로토콜 |
| `_0120_Social.proto` | 120 | 소셜 프로토콜 |
| `_-0120_Inner_Social.proto` | -120 | 내부 소셜 프로토콜(서버 측) |
| `_0300_User.proto` | 300 | 사용자 / 계정 프로토콜 |
| `_0310_Attribute.proto` | 310 | 플레이어 속성 동기화 프로토콜 |
| `_0400_Room.proto` | 400 | 룸 프로토콜 |
| `_0410_RockPaperScissors.proto` | 410 | 가위바위보 미니게임 프로토콜 |
| `_0500_Mail.proto` | 500 | 메일 시스템 프로토콜 |

## 프로토콜 규칙

protobuf가 처음이신가요? 이 절은 단계별 튜토리얼입니다. 위에서부터 아래로 읽어나가면, `.proto` 파일을 한 번도 작성해 본 적이 없어도 새 프로토콜 모듈을 추가할 수 있게 됩니다. 각 단계에는 알기 쉬운 설명, 최소 예시, 그리고 그 배경의 규칙이 담겨 있습니다. 도구가 강제하는 엄격한 규칙 목록은 아래 [프로토콜 요구 사항](#프로토콜-요구-사항)을 참조하세요.

### 시작하기 전에 — 세 가지 쉬운 개념

- **Protobuf(`.proto`)** 는 양측이 합의한 '주문서 양식'입니다——인쇄된 주문서처럼 각 칸의 이름과 위치가 정해져 있어, 클라이언트와 서버가 같은 양식에 맞춰 채우므로 오해가 생기지 않습니다.
- **모듈 ID** 는 '분류 번호'입니다. 택배 회사의 지역 번호를 상상해 보세요: 각 업무(가방, 메일, 룸……)마다 번호가 하나씩 할당되고, 메시지는 그 번호로 올바른 담당자에게 배달됩니다.
- **외부 프로토콜 vs 내부 프로토콜** —— 외부 프로토콜은 클라이언트가 볼 수 있고 호출할 수 있는 '메뉴'이고, 내부 프로토콜은 서버 사이에서만 오가는 '주방 암호'입니다. 이 둘은 절대 섞여서는 안 되며, 섞이면 클라이언트가 호출하면 안 되는 것을 호출할 수 있게 됩니다.

### 1단계 — 파일 만들기

각 비즈니스 도메인은 자신만의 파일에 들어 있고, 파일명은 `_<ModuleID:0000>_<Domain>.proto` 입니다 —— **모든 파일명이 `_`로 시작하고 이어 4자리 0으로 채운 모듈 ID**가 오며, 어떤 파일 브라우저에서든 동일하게 모듈 번호 순으로 정렬됩니다. 파일명만 봐도 라우팅 번호와 도메인을 한눈에 알 수 있습니다.

```protobuf
// 파일명: _0100_Bag.proto
syntax = "proto3";      // 항상 proto3 —— 현행 protobuf 문법
package Bag;            // 도메인명(PascalCase)
option module = 100;    // 라우팅 번호. 파일명의 0100 과 일치해야 함
```

한 줄씩 설명:

- `syntax = "proto3";` —— 현행 protobuf 문법을 선언합니다. 모든 파일이 이 줄로 시작합니다.
- `package Bag;` —— 이 파일의 도메인은 'Bag'. PascalCase는 첫 글자가 대문자임을 뜻합니다.
- `option module = 100;` —— 라우팅 번호 100을 할당합니다.**파일명의 `0100` 과 완전히 일치해야 합니다.**

규칙:

- 파일명: `_<ModuleID:0000>_<Domain>.proto`(예: `_0500_Mail.proto`).
- 양수 = 외부 프로토콜(클라이언트 ↔ 서버), 음수 = 내부 프로토콜(서버 ↔ 서버). 음수 ID는 파일명에 부호를 그대로 남깁니다(`_-0120_Inner_Social.proto`는 module = -120을 뜻함); 모든 파일명이 `_`로 시작하므로 합법(`-`로 시작하지 않음)이며 정렬도 통일됩니다.
- 내부 파일은 `Inner` 로 시작. 예: `_0002_InnerBasic.proto`.

**이유** —— 모듈 ID를 파일명에 넣으면 파일명 자체가 라우팅 키가 됩니다: 도메인을 한눈에 알 수 있고, 두 파일이 몰래 같은 번호를 공유할 수도 없습니다. `Inner` 접두사는 내부 프로토콜에 표시를 붙여 내보낼 때 걸러지게 하고, 클라이언트에 유출되지 않게 합니다.

### 2단계 — 데이터 정의하기: 메시지와 필드

**메시지(message)** 는 '양식'입니다——관련 필드들의 묶음. **필드(field)** 는 양식 위의 한 칸으로, 이름·타입·번호를 가집니다.

```protobuf
message BagItem {
  int32 ItemId = 1; // 아이템 ID
  int64 Count = 2;  // 아이템 수량
}
```

한 줄씩 설명:

- `message BagItem { ... }` —— `BagItem` 이라는 양식을 정의합니다.
- `int32 ItemId = 1;` —— `ItemId` 라는 칸, 타입 `int32`(작은 정수), 번호 `1`.
- `int64 Count = 2;` —— `Count` 라는 칸, 타입 `int64`(큰 정수), 번호 `2`.
- 줄 끝의 `// ...` 는 주석으로, 이 필드가 무엇인지 설명합니다.

규칙:

- 필드명은 PascalCase. 번호는 1부터 연속적으로 올리고 건너뛰지 않는다.
- 필드를 삭제하면 `reserved` 로 그 번호를 묶어둔다——번호를 재사용하면 안 된다.
- 모든 필드에는 줄 끝 주석을 붙인다.

타입 고르기(쉬운 버전):

| 이 값은…… | 사용 | 예 |
|-----------|------|----|
| 플레이어 / 인스턴스 ID(커질 수 있음) | `int64` | `PlayerId` |
| 설정 / 아이템 ID(범위가 작음) | `int32` | `ItemId` |
| 수량(많이 쌓일 수 있음) | `int64` | `Count` |
| 타임스탬프 | `int64` | `CreateTime` |
| 레벨 / 아바타(작고 음수가 아님) | `uint32` | `Level` |
| 선택지가 정해진 상태 | 열거형(4단계 참고) | `RoomStatus` |
| 리스트 / 사전 | `repeated` / `map` | `repeated RoomPlayerInfo` |

**이유** —— 번호를 연속으로 유지하는 이유는, 필드 번호가 통신 시의 식별자이기 때문입니다: 빈 번호는 공간을 낭비하고, 출시된 번호를 재사용하면 이전 클라이언트의 데이터가 새 필드로 들어가 조용히 데이터 훼손을 일으킵니다. 타입은 '충분한 범위, 오버플로우 없음'을 따릅니다: 큰 ID는 `int64`, 작은 ID는 `int32` 로 전송량을 절약.

### 3단계 — 대화하게 만들기: 요청 / 응답 / 알림

이제 클라이언트와 서버가 어떻게 소통할지 정의합니다. 메시지 역할은 세 가지이고, 이름 접두사로 구분합니다:

| 접두사 | 누가 시작 | 쉬운 의미 |
|--------|----------|-----------|
| `Req<Name>` | 클라이언트 | '하나 물어볼게' |
| `Resp<Name>` | 서버가 답 | '이게 답이야'(이름은 요청과 동일) |
| `Notify<Name>` | 서버가 푸시 | '주의——변동 있음'(대응하는 요청 없음) |

```protobuf
message ReqMailList { ... }        // 클라이언트가 메일 목록을 요청
message RespMailList { ... }       // 서버가 목록을 반환——이름이 짝인 점에 주목
message NotifyMailChanged { ... }  // 서버가 능동적으로 메일 변화를 푸시
message MailInfo { ... }           // 재사용 가능한 데이터 블록, 위 셋 모두에서 쓰임
```

규칙:

- 모든 요청에는 같은 이름의 응답이 있어야 한다: `ReqMailList` ↔ `RespMailList`.
- `Notify` 는 서버 주도 푸시에만 쓴다.
- 공용 데이터는 `<Name>Info` 로 빼내어 한 번 정의하고 곳곳에서 재사용한다.

**이유** —— Req/Resp 페어를 필수로 하면 모든 질문에 답이 보장됩니다. 같은 이름 덕분에 사람과 코드 생성기 모두 누가 짝인지 한눈에 압니다. `<Name>Info` 는 같은 구조를 여러 메시지에서 반복 정의하는 일을 막아 줍니다.

### 4단계 — 열거형으로 상태 표현하기

**열거형(enum)** 은 객관식 문제입니다——주문 상태가 '결제 대기 / 결제 완료 / 배송됨'만 될 수 있는 것과 같습니다.

```protobuf
enum RoomStatus {
  None = 0;     // 상태 없음 / 무효
  Waiting = 1;  // 시작 대기
  Playing = 3;  // 게임 진행 중
}
```

규칙:

- 열거형명과 값은 모두 PascalCase.
- 첫 번째 값은 항상 `0`, 기본 / 없음 상태(`None`, `Unknown`)에 쓴다.

**이유** —— proto3 는 첫 값을 `0` 으로 강제합니다. 이를 `None` / `Unknown` 으로 정하면 안전한 기본값이 됩니다: 설정하지 않은 필드는 '상태 없음'으로 읽히고, 실제 상태에 잘못 들어맞지 않아 버그의 부류 전체를 막아 줍니다.

### 5단계 — 에러 코드 정의하기

실패하면 번호를 붙여, 양측이 정확히 무엇이 잘못되었는지 알 수 있게 합니다. 에러 코드는 두 계층입니다:

**공용 코드** —— 어느 모듈에서나 일어나는 흔한 실패(잘못된 파라미터, 비용 부족, 부재). 이들은 `_0020_Common.proto` 의 `OperationStatusCode` 에 있으며, `0` 부터 번호가 매겨집니다.

**비즈니스 코드** —— 그 모듈 특유의 실패. 번호는 공식으로 정해집니다: **`모듈 ID × 1000 + 세 자리 일련번호`**.

```protobuf
// 메일은 모듈 500 이므로, 에러 코드는 500001 부터 시작
// 500001 = 500 × 1000 + 1
enum MailErrorCode {
  MailNotFound = 500001;        // 메일이 존재하지 않음
  MailAlreadyDeleted = 500002;  // 메일이 이미 삭제됨
}
```

규칙: 클라이언트는 에러 코드를 일반 `int` 로 받습니다. 성공 시에는 설정하지 않고, proto3 의 기본값 `0` 이 '성공'을 의미하게 하여, 대부분의 경우 아무것도 보내지 않아도 됩니다.

**이유** —— 이 공식 덕에 번호는 스스로 소속을 드러냅니다: `500001` 은 한눈에 메일 모듈임이 드러나고, 조정 없이 전역 고유하며, 모듈마다 1000개의 확장 슬롯도 확보됩니다. 성공을 '아무것도 보내지 않음'으로 처리하는 것은 성공이 대부분이라 절약되는 전송량이 크기 때문입니다.

### 6단계 — 주석 달기

주석은 양측이 공유하는 유일한 문서입니다——`.proto` 파일에는 주변 컨텍스트가 없어서, 주석이 없으면 다른 쪽은 추측할 수밖에 없습니다.

- 메시지 앞: 그 용도를 적는다.
- 필드나 열거값 뒤: 무엇을 뜻하는지 적는다.
- 만약 `int` 필드가 실제로는 열거값을 담고 있다면, 괄호로 열거형명을 표시한다(예: `// 상태(RoomStatus)`). 독자가 유효한 값을 어디서 찾아야 할지 알게 한다.

**이유** —— `int` 만으로는 유효한 값의 집합을 알 수 없습니다. 열거형명을 표시하면 독자가 바로 답을 찾을 수 있습니다.

### 전체 예시

가상의 `_0600_Quest`(퀘스트 시스템) 모듈을 예로, 위의 모든 규칙을 적용합니다:

```protobuf
syntax = "proto3";
package Quest;
option module = 600;

// Quest business error codes (6 digits = module 600 + 3-digit ordinal)
enum QuestErrorCode {
  QuestNotFound = 600001;             // quest not found
  QuestNotCompleted = 600002;         // quest not completed
  QuestRewardAlreadyClaimed = 600003; // reward already claimed
}

// Quest status
enum QuestStatus {
  None = 0;        // no state
  Accepted = 1;    // accepted
  Completable = 2; // ready to complete
  Completed = 3;   // completed
  Claimed = 4;     // reward claimed
}

// Quest data view
message QuestInfo {
  int64 QuestId = 1;            // quest config ID
  QuestStatus Status = 2;       // quest status (QuestStatus)
  int64 Progress = 3;           // current progress
  int64 TargetProgress = 4;     // target progress
}

// Request quest list
message ReqQuestList {
}

// Response quest list
message RespQuestList {
  repeated QuestInfo Quests = 1; // quest list
}

// Request claim quest reward
message ReqClaimQuestReward {
  int64 QuestId = 1; // quest config ID
}

// Response claim quest reward
message RespClaimQuestReward {
  int64 QuestId = 1;       // quest config ID
  QuestStatus Status = 2;  // status after claim (QuestStatus)
}

// Quest change notification (server push)
message NotifyQuestChanged {
  repeated QuestInfo Quests = 1; // changed quests
}
```

## 프로토콜 요구 사항

`ProtoExport` 도구가 강제하는 hard rule 입니다. 권위 있는 출처: [GameFrameX.Tools README](https://github.com/GameFrameX/GameFrameX.Tools#readme).

### 파일 형식

```protobuf
syntax = "proto3";     // Required: only proto3 is supported
package Basic;
option module = 10;    // Required: module ID must be defined
```

### 메시지 명명

- **요청**: `Req<Name>`(예: `ReqLogin`, `ReqHeartBeat`)
- **응답**: `Resp<Name>`(예: `RespLogin`)
- **알림**: `Notify<Name>`(예: `NotifyBagInfoChanged`)
- 모든 메시지·필드·열거형 이름 및 열거값은 **UpperCamelCase** 를 사용해야 합니다.

### 모듈 ID

| ID 범위 | 용도 |
|---------|------|
| `0` ~ `32767` | 클라이언트 ↔ 서버 |
| `-32768` ~ `-1` | 서버 ↔ 서버(내부) |

### 필드 번호 매기기

- 메시지 필드 번호는 **800 미만**이어야 합니다(`>= 800` 값은 시스템 예약이며 파싱 에러를 발생시킵니다).
- `ErrorCode` 는 `Resp` 메시지에서 **예약된 필드명**입니다 —— 직접 정의하지 마세요. 도구가 모든 `Resp` 에 `ErrorCode` 필드를 자동 생성합니다.

### 제약 사항

- **중첩 타입 불가** —— 다른 메시지 안에서 `message` / `enum` 을 선언할 수 없습니다.
- **RPC 정의 불가** —— `service` 블록은 지원되지 않습니다.
- **proto3 전용** —— `syntax = "proto3";` 가 필수이며 proto2 는 지원되지 않습니다.

### 주석 표준

- 모든 `message` / `enum` **위**에 용도를 설명하는 주석 줄을 넣습니다.
- 모든 필드 / 열거값 줄 끝에 **인라인** 주석을 붙입니다.

### 서버 전용 파일

내보내기 도구는 서버 전용 proto 파일을 **파일명 접미사** `-s` 또는 `_s`(예: `player-s.proto`, `economy_s.proto`)로 식별합니다. 이들을 포함하려면 `--isServer true` 를 전달하고, 기본값 `--isServer false` 에서는 건너뛰므로 서버 전용 메시지가 클라이언트에 유출되지 않습니다.

내부 프로토콜은 라우팅 분리를 위해 추가로 **음수 모듈 ID**를 가집니다(위 모듈 ID 표 참조).

> **현재 리포지토리 참고:** 여기서 내부 파일은 `Inner_` 접두사와 음수 모듈 ID를 함께 사용합니다(예: `_-0120_Inner_Social.proto`). `-s`/`_s` 접미사와 음수 ID 규칙 모두 서버 전용 라우팅을 달성합니다 —— 하나를 선택하고 모듈 내에서 일관되게 유지하세요.

## 지원하는 내보내기 언어

| 언어 | 모드 및 플래그 | 로컬 스크립트 | Docker |
|------|----------------|---------------|--------|
| C# (서버) | `csharp --isServer true` | `Proto2CsExport_Server.sh` / `.bat` | ✅ |
| C# (클라이언트 / Unity / Godot) | `csharp` | `Proto2CsExport_Client.sh` / `.bat` | ✅ |
| C++ | `cpp` | `Proto2CppExport.sh` / `.bat` | ✅ |
| Go | `go` | `Proto2GoExport.sh` / `.bat` | ✅ |
| Lua | `lua` | `Proto2LuaExport.sh` / `.bat` | ✅ |
| TypeScript | `typescript` | `Proto2TsExport.sh` / `.bat` | ✅ |
| TypeScript (LayaBox) | `typescript` | `Proto2TsExport_LayaBox.sh` | ✅ |

### Docker 예시

**C# (서버):**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Server/GameFrameX.Proto/Proto:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" \
  --isGenerateDescription true \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

**Go:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./GoServer/proto:/output \
  gameframex/gameframex-tools:latest \
  --mode go --inputPath /protos --outputPath /output --namespaceName proto
```

**TypeScript:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Laya/src/gameframex/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode typescript --inputPath /protos --outputPath /output
```

**Lua:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Defold/scripts/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode lua --importPath "./network/" --inputPath /protos --outputPath /output
```

**C++:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Unreal/Source/Proto:/output \
  gameframex/gameframex-tools:latest \
  --mode cpp \
  --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto
```

경로 매핑: `-v <host>:<container>` 는 호스트 디렉터리를 마운트하고, `--inputPath` / `--outputPath` 는 **컨테이너 측** 경로(`/protos`, `/output`)를 참조해야 하며 호스트 경로가 아닙니다.

## 내보내기 매개변수

### 핵심

| 매개변수 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| `--mode` | Yes | - | `csharp` / `typescript` / `cpp` / `lua` / `go` |
| `--inputPath` | Yes | - | `.proto` 파일들이 들어 있는 디렉터리 |
| `--outputPath` | Yes | - | 생성된 파일의 출력 디렉터리 |
| `--namespaceName` | No | `""` | C# 네임스페이스(또는 점으로 구분된 경우 Go 패키지의 마지막 세그먼트) |
| `--isGenerateErrorCode` | No | `true` | `Resp` 메시지에 `ErrorCode` 필드 자동 생성 |
| `--requireComments` | No | `none` | 주석 검증 레벨: `none` / `container` / `member` / `all` |

### C#

| 매개변수 | 기본값 | 설명 |
|----------|--------|------|
| `--usingStatements` | `""` | `\|` 로 구분된 using 문(예: `"using System\|using ProtoBuf"`) |
| `--isGenerateDescription` | `false` | `[System.ComponentModel.Description]` 특성 생성 |
| `--isServer` | `false` | 서버 전용 proto 파일 포함(파일명이 `-s` 또는 `_s` 로 끝남) |

### TypeScript

| 매개변수 | 기본값 | 설명 |
|----------|--------|------|
| `--importPath` | `"../network/"` | 생성된 import 문의 import 경로 접두사 |
| `--isGenerateDescription` | `false` | JSDoc 스타일 주석 생성 |

### 레거시

| 매개변수 | 기본값 | 설명 |
|----------|--------|------|
| `--isGenerateErrorCodeExcelFile` | `true` | 에러 코드 Excel 파일 생성 |
| `--errorCodeExcelFilePath` | `""` | 에러 코드 Excel 파일의 사용자 지정 경로 |

## Docker

`linux/amd64` 와 `linux/arm64` 용 사전 빌드 이미지가 제공됩니다:

```bash
# Docker Hub
docker pull gameframex/gameframex-tools:latest

# GitHub Container Registry (GHCR)
docker pull ghcr.io/gameframex/gameframex.tools:latest
```

이미지의 entrypoint 는 `ProtoExport` 도구입니다 —— 이미지명 뒤에 매개변수를 직접 붙이면 됩니다:

```bash
docker run --rm \
  -v /path/to/protos:/protos \
  -v /path/to/output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --inputPath /protos --outputPath /output
```

## CI 파이프라인

이 리포지토리는 [`.github/workflows/proto-export.yml`](.github/workflows/proto-export.yml) 을 제공합니다. **모든 `push`** 및 수동 디스패치에서 자동으로 실행됩니다.

| 단계 | 수행 내용 |
|------|-----------|
| 1 | `gameframex/gameframex-tools:latest` 풀 |
| 2 | `.proto` 소스를 컨테이너의 `/protos` 에 마운트 |
| 3 | 6개 대상 언어를 병렬로 내보내기(build matrix) |
| 4 | 각 언어의 출력을 workflow artifact 로 수집 |
| 5 | `main` 으로의 `push` 시, 모든 artifact 를 첨부한 롤링 **`latest` Release** (재)게시 |

최신 생성 코드는 [Releases 페이지](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest)에서 다운로드할 수 있습니다 —— 도구 체인이 필요 없습니다.

## 내보내기 도구

이 리포지토리의 코드 생성은 독립 리포지토리 [GameFrameX.Tools](https://github.com/GameFrameX/GameFrameX.Tools)의 `ProtoExport` 도구(.NET 10 콘솔 앱)가 담당합니다. **이 리포지토리는 바이너리를 포함하지 않습니다**——세 가지 워크플로 중 하나를 선택하세요([빠른 시작](#빠른-시작) 참조):

- **CI** —— 설정 불필요. 최신 Release에서 생성된 코드를 다운로드하기만 하면 됩니다.
- **Docker** —— 사전 빌드된 이미지 실행. 로컬 도구 체인 불필요.
- **로컬 스크립트** —— 도구를 직접 빌드하여 산출물을 이 리포지토리의 `Tools/` 디렉터리에 넣습니다(절차는 아래).

### 도구 리포지토리

| 프로젝트 | 리포지토리 | 설명 |
|----------|-----------|------|
| GameFrameX.Tools | https://github.com/GameFrameX/GameFrameX.Tools | `ProtoExport` 생성기 소스, 전체 매개변수 문서, Docker 이미지 |

`ProtoExport`는 .NET 10 콘솔 프로젝트(`ProtoExport.csproj`, `OutputType=Exe`)이며, 명령줄 파싱을 위해 NuGet 패키지 `GameFrameX.Foundation.Options`에 의존합니다.

### 사전 요구 사항

- **.NET 10 SDK** —— 도구 빌드와 내보내기 스크립트 실행 모두에 필요합니다.
- 확인: `dotnet --version`이 `10.x.x`를 출력해야 합니다.

### 빌드

```bash
# 1. 도구 리포지토리 클론
git clone https://github.com/GameFrameX/GameFrameX.Tools.git
cd GameFrameX.Tools/ProtoExport

# 2. 빌드 (Release)
dotnet build -c Release

# 3. 산출물은 bin/Release/net10.0/에 생성
ls bin/Release/net10.0/
```

### 빌드 산출물

`GameFrameX.Tools/ProtoExport/bin/Release/net10.0/`에서 다음 파일들을 이 리포지토리의 `Tools/` 디렉터리로 복사합니다:

| 파일 | 필수 | 용도 |
|------|:----:|------|
| `ProtoExport.dll` | ✅ | 메인 어셈블리 |
| `ProtoExport.deps.json` | ✅ | 의존성 매니페스트 (런타임에 필요) |
| `ProtoExport.runtimeconfig.json` | ✅ | 런타임 설정 (.NET 10 지정) |
| `GameFrameX.Foundation.Options.dll` | ✅ | 명령줄 파싱 의존성 |
| `ProtoExport` / `ProtoExport.exe` | ⛔ | 네이티브 apphost——스크립트 미사용 |
| `ProtoExport.pdb` | ⛔ | 디버그 심볼 |

```bash
# 필수 4개 파일을 이 리포지토리의 Tools/로 복사
cp bin/Release/net10.0/ProtoExport.dll                   /path/to/GameFrameX.Protobuf/Tools/
cp bin/Release/net10.0/ProtoExport.deps.json             /path/to/GameFrameX.Protobuf/Tools/
cp bin/Release/net10.0/ProtoExport.runtimeconfig.json    /path/to/GameFrameX.Protobuf/Tools/
cp bin/Release/net10.0/GameFrameX.Foundation.Options.dll /path/to/GameFrameX.Protobuf/Tools/

# 또는 전체 산출물을 한 번에 복사
cp bin/Release/net10.0/* /path/to/GameFrameX.Protobuf/Tools/
```

> 네이티브 시작 프로그램(macOS/Linux의 `ProtoExport`, Windows의 `ProtoExport.exe`)은 선택 사항입니다——모든 `Proto2*` 스크립트는 `dotnet ./Tools/ProtoExport.dll`로 도구를 균일하게 시작하므로 크로스 플랫폼으로 일관됩니다.

### 검증

```bash
cd /path/to/GameFrameX.Protobuf
./Proto2CsExport_Client.sh    # macOS / Linux
Proto2CsExport_Client.bat     # Windows
```

`协议扫描完成: ... 导出 N 个，跳过 M 个` 와 같은 줄이 보이면 도구가 준비된 것입니다.

### 내보내기 스크립트와의 관계

리포지토리 루트의 각 `Proto2*.sh` / `.bat` 스크립트는:

1. 리포지토리 루트에서 실행되며;
2. `Tools/`에 넣은 생성기를 `dotnet ./Tools/ProtoExport.dll`로 시작하고;
3. 언어별 플래그(`--mode`, `--isServer` 등)를 전달합니다.

따라서 `Tools/`에 올바른 산출물만 있으면 **모든 스크립트가 바로 실행됩니다**——언어별 매개변수를 직접 다룰 필요가 없습니다.

### 도구 업데이트

`ProtoExport`가 상류에서 갱신되면, "빌드 + 산출물 복사"를 다시 실행하여 `Tools/`의 파일을 덮어씁니다. 도구 버전을 이 리포지토리의 프로토콜 규격과 동기화하세요——이 리포지토리의 최신 변경을 가져올 때 도구도 함께 다시 빌드하기를 권장합니다.

## 빠른 시작

**옵션 A — CI에서 다운로드(설정 불필요):** [최신 Release](https://github.com/GameFrameX/GameFrameX.Protobuf/releases/latest)에서 사용 언어의 번들을 받으세요.

**옵션 B — Docker:**

```bash
docker run --rm \
  -v "$PWD":/protos \
  -v "$PWD/output":/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

**옵션 C — 로컬 스크립트:** 도구를 직접 빌드하여 `Tools/`에 넣고(전체 절차는 [내보내기 도구](#내보내기-도구) 참조), 리포지토리 루트에서 실행합니다:

```bash
./Proto2CsExport_Server.sh   # C# (서버)
./Proto2GoExport.sh          # Go
```

모든 스크립트는 `dotnet ./Tools/ProtoExport.dll`로 `Tools/`의 생성기를 호출합니다. 매개변수 세부 사항은 [내보내기 문서](https://gameframex.doc.alianblank.com/protobuf/require)를 참조하세요.