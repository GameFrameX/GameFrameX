<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX.Tools

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Tools?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Tools/releases)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)

[![Discord](https://img.shields.io/badge/-5865F2?logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/-181717?logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Bilibili](https://img.shields.io/badge/-00A1D6?logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/-C71D23?logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)

**인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현**

<br />

[문서](https://gameframex.doc.alianblank.com) · [빠른 시작](#빠른-시작) · QQ 그룹: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>

## 프로젝트 개요

ProtoExport는 여러 언어를 대상으로 하는 Proto 프로토콜 코드 생성 도구입니다. C#, TypeScript, C++, Lua, Go 5가지 대상 언어 내보내기를 지원하며, CLI, 크로스 플랫폼 GUI, Docker 이미지 세 가지 사용 방식을 제공합니다.

### 기능

- 다중 언어 내보내기: C#(`.cs`), TypeScript(`.ts`), C++(`.h`), Lua(`.lua`), Go(`.go`)
- 세 가지 사용 방식: CLI(`dotnet ProtoExport.dll`), Avalonia GUI(ProtoExporterGUI), Docker 이미지(`linux/amd64`, `linux/arm64`)
- 메시지 ID 안정성: lock 파일에 `(Module, 메시지 이름) → SubId` 매핑을 영속화하여, 프로토콜 번호가 proto 텍스트 편집으로 드리프트되지 않습니다
- 서버 전용 프로토콜: `-s` / `_s`로 끝나는 proto 파일은 서버 내보내기 시에만 포함됩니다
- 주석 검증: `none` / `container` / `member` / `all` 4단계 주석 완전성 검사
- GUI 설정 영속화: 모드별로 `Setting.json`에 저장하며, 기본값과 깊이 병합하여 무손실 업그레이드

## 빠른 시작

### 설치

`ProtoExport` 프로젝트는 빌드 산출물을 **고정된 형제 경로**인 `../Protobuf/Tools/`(저장소 루트 기준, 즉 `<워크스페이스>/Protobuf/Tools/`)로 출력하도록 설정되어 있습니다. 경로는 `.csproj` 위치를 `$(MSBuildThisFileDirectory)`로 기준 삼아 고정되므로, `Tools`와 `Protobuf`를 형제 디렉토리로 클론한 모든 머신에서 동일하게 해석됩니다.

이 고정 경로는 `Protobuf` 저장소의 `Proto2*Export.{sh,bat}` 스크립트(예: `dotnet ./Tools/ProtoExport.dll ...`)가 소비하며, DLL은 `Protobuf/Tools/ProtoExport.dll`에 있을 것으로 기대합니다.

```bash
# Tools/ProtoExport 디렉토리(또는 저장소 루트)에서 실행
dotnet build ProtoExport/ProtoExport.csproj -c Release
# 산출물 위치: ../Protobuf/Tools/ProtoExport.dll (TFM/RID 하위 디렉토리 없음)
```

고정 경로를 가능하게 하는 빌드 속성(`ProtoExport/ProtoExport.csproj`에 설정):

- `OutputPath` = `$(MSBuildThisFileDirectory)../../Protobuf/Tools/` — Debug/Release 공통 출력 디렉토리.
- `AppendTargetFrameworkToOutputPath` = `false` — `net10.0/` 하위 디렉토리 생성 안 함.
- `AppendRuntimeIdentifierToOutputPath` = `false` — `win-x64/`, `linux-arm64/` 하위 디렉토리 생성 안 함.

> SDK 기본 `output-paths` target에 덮어씌워지지 않도록 `OutDir` 대신 `OutputPath`를 사용합니다.

## 사용 예시

| 모드 | 출력 언어 | 파일 확장자 | 설명 |
|------|----------|------------|------|
| `csharp` | C# | `.cs` | Server, Unity, Godot, Stride, Flax 등에 적용 |
| `typescript` | TypeScript | `.ts` | LayaAir, Cocos Creator, Phaser 등에 적용 |
| `cpp` | C++ | `.h` | Unreal Engine 등에 적용 |
| `lua` | Lua | `.lua` | Defold, Solar2D, Dora SSR 등에 적용 |
| `go` | Go | `.go` | Go 게임 서버 등에 적용 |

### Docker 이미지

사전 빌드된 Docker 이미지는 `linux/amd64` 및 `linux/arm64` 아키텍처를 지원합니다.

**Docker Hub**

```bash
docker pull gameframex/gameframex-tools:latest
```

**GitHub Container Registry (GHCR)**

```bash
docker pull ghcr.io/gameframex/gameframex.tools:latest
```

**사용 예시**

```bash
docker run --rm \
  -v /path/to/protos:/protos \
  -v /path/to/output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" --isGenerateDescription true --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

### C# 모드

`[ProtoContract]` / `[ProtoMember]` 속성이 포함된 C# 코드를 생성합니다. 모든 동작은 CLI 매개변수로 제어되며, 엔진별 하드코딩이 없습니다.

#### 서버 내보내기

서버용 using 문, `[Description]` 속성이 포함된 코드를 생성하며 서버 전용 proto 파일을 포함합니다.

**로컬 실행:**

```bash
dotnet ProtoExport.dll \
  --mode csharp \
  --isServer true \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" \
  --isGenerateDescription true \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Server/GameFrameX.Proto/Proto \
  --namespaceName GameFrameX.Proto.Proto \
  --isGenerateErrorCode true
```

**Docker 실행:**

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

#### Unity 내보내기

Unity용 using 문이 포함된 코드를 생성하며 서버 전용 proto 파일은 자동으로 건너뜁니다.

```bash
dotnet ProtoExport.dll \
  --mode csharp \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Unity/Assets/Hotfix/Proto \
  --namespaceName Hotfix.Proto \
  --isGenerateErrorCode true
```

#### Godot 내보내기

Unity와 유사하지만 Godot 전용 네임스페이스를 사용합니다.

```bash
dotnet ProtoExport.dll \
  --mode csharp \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Godot/Proto \
  --namespaceName Proto \
  --isGenerateErrorCode true
```

### TypeScript 모드

`export namespace`, `export class`, `export enum`이 포함된 `.ts` 파일과 집계 파일 `ProtoMessageRegister.ts`를 생성합니다. 서버 전용 proto 파일은 자동으로 건너뜁니다.

#### 기본 import 경로

```bash
dotnet ProtoExport.dll \
  --mode typescript \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Laya/src/gameframex/protobuf \
  --isGenerateErrorCode true
```

#### 사용자 지정 import 경로

```bash
dotnet ProtoExport.dll \
  --mode typescript \
  --importPath "./lib/network/" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../CocosCreator/assets/scripts/protobuf \
  --isGenerateErrorCode true
```

**Docker 실행:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Laya/src/gameframex/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode typescript --inputPath /protos --outputPath /output
```

### C++ 모드

`#pragma once`, 네임스페이스, `enum class`, 클래스 정의가 포함된 C++ 헤더 파일을 생성합니다. `MessageObject`를 상속하는 클래스에는 `MESSAGE_ID`와 `Clear()` 메서드가 포함됩니다.

```bash
dotnet ProtoExport.dll \
  --mode cpp \
  --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Unreal/Source/Proto \
  --namespaceName GameFrameX.Proto
```

### Lua 모드

LuaDoc(EmmyLua) 타입 어노테이션과 모듈 기반 메시지 정의가 포함된 `.lua` 파일을 생성합니다. 집계 파일 `ProtoMessageRegister.lua`도 함께 생성됩니다.

```bash
dotnet ProtoExport.dll \
  --mode lua \
  --importPath "./network/" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Defold/scripts/protobuf
```

**Docker 실행:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./Defold/scripts/protobuf:/output \
  gameframex/gameframex-tools:latest \
  --mode lua --importPath "./network/" --inputPath /protos --outputPath /output
```

### Go 모드

protobuf 태그가 포함된 Go struct 정의, enum 타입 정의, 집계 파일 `message_register.go`를 생성합니다. `--namespaceName`은 Go 패키지 이름으로 사용됩니다(점으로 구분된 경우 마지막 세그먼트).

```bash
dotnet ProtoExport.dll \
  --mode go \
  --usingStatements "google.golang.org/protobuf/runtime/protoimpl" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../GoServer/proto \
  --namespaceName proto
```

**Docker 실행:**

```bash
docker run --rm \
  -v ./Protobuf:/protos \
  -v ./GoServer/proto:/output \
  gameframex/gameframex-tools:latest \
  --mode go --inputPath /protos --outputPath /output --namespaceName proto
```

### GUI 도구 (ProtoExporterGUI)

ProtoExport CLI를 래핑하는 크로스 플랫폼 Avalonia GUI로, 명령줄 인수를 직접 입력할 필요가 없습니다.

#### 기능

- 7가지 내보내기 모드(Server / Unity / Godot / TypeScript / C++ / Lua / Go) 드롭다운 전환
- 모든 CLI 매개변수를 시각적으로 편집(네임스페이스, using/import, 주석 검증 수준, 에러 코드, Description, 서버 모드)
- 폴더 피커로 경로를 탐색하여 선택(직접 입력 불필요)
- 중국어 / 영어 UI를 실시간으로 전환
- 설정 영속화(모드별로 프로그램 디렉토리의 `Setting.json`에 저장, 기본값과 깊이 병합하여 무손실 업그레이드)
- 실시간 로그 출력 패널

#### 빌드 및 게시

```bash
dotnet publish ProtoExporterGUI/ProtoExporterGUI.csproj -c Release -r win-x64 --no-self-contained
# 기타 RID: osx-arm64 / osx-x64 / linux-x64 / linux-arm64
```

산출물은 단일 파일 실행 파일입니다(런타임을 포함하지 않으므로 대상 머신에 .NET 10 런타임 사전 설치가 필요합니다).

#### 사용 방법

1. 앱을 실행합니다
2. 내보내기 모드를 선택합니다
3. 입력/출력 경로를 입력하거나 탐색하여 선택합니다
4. 내보내기를 클릭합니다
5. 로그 패널을 확인합니다

GUI는 CLI와 기능적으로 동등합니다. CI/자동화 시나리오에서는 여전히 CLI 또는 Docker 사용을 권장합니다.

### 빠른 내보내기 스크립트

`Protobuf/` 디렉토리에 사전 설정된 내보내기 스크립트가 제공됩니다:

| 스크립트 | 설명 |
|---------|------|
| `Proto2CsExport_Server.sh/.bat` | C# 서버용 코드 내보내기 |
| `Proto2CsExport_Client.sh/.bat` | C# Unity 클라이언트용 코드 내보내기 |
| `Proto2TsExport.sh/.bat` | TypeScript 코드 내보내기 |
| `Proto2CppExport.sh/.bat` | C++ 코드 내보내기 |
| `Proto2LuaExport.sh/.bat` | Lua 코드 내보내기 |
| `Proto2GoExport.sh/.bat` | Go 코드 내보내기 |

## API 참조

### Proto 프로토콜 사양

이 도구는 `.proto` 파일 형식에 특정 요구사항이 있습니다. 올바른 코드 생성을 위해 다음 규칙을 따르세요.

#### 파일 형식 요구사항

```protobuf
syntax = "proto3";     // 필수: proto3만 지원
package Basic;
option module = 10;    // 모듈 ID: 파일 이름 접두사(_0010_Name.proto) 또는 이 선언 중 하나. 둘 다 있으면 일치 필수

// 하트비트 요청
message ReqHeartBeat
{
    int64 Timestamp = 1; // 타임스탬프
}
```

#### 메시지 명명 규칙

- **요청 메시지**: `Req`로 시작해야 합니다(예: `ReqLogin`, `ReqHeartBeat`)
- **응답 메시지**: `Resp`로 시작해야 합니다(예: `RespLogin`)
- **알림 메시지**: `Notify`로 시작해야 합니다(예: `NotifyBagInfoChanged`)
- 메시지 이름, 필드 이름, enum 이름, enum 값은 모두 **UpperCamelCase**를 사용해야 합니다

#### 모듈 ID 규칙

모듈 ID는 두 가지 방식으로 선언할 수 있으며, **파일 이름 접두사가 우선**합니다:

1. **파일 이름 접두사**: `_<모듈ID>_<이름>.proto`(예: `_0010_Basic.proto` → 모듈 10, `_-0120_Inner_Social.proto` → 모듈 -120. 구분자로 `-`도 허용, 예: `_0010-Basic.proto`)
2. **option 선언**: `option module = <id>;`

규칙:

- 둘 다 존재하면 반드시 일치해야 하며, 불일치 시 빌드 오류
- `_` + 숫자로 시작하지만 두 번째 구분자가 없는 파일 이름(예: `_0010Basic`)은 명명 오류로 간주되어 빌드 오류
- 둘 다 없으면 빌드 오류

| ID 범위 | 용도 |
|---------|------|
| `0` ~ `32767` | 클라이언트-서버 통신 |
| `-32768` ~ `-1` | 서버-서버 통신 |

#### 필드 번호 규칙

- 메시지 필드 번호는 **800 미만**이어야 합니다(800 이상은 시스템 예약이며 파싱 오류를 발생시킵니다)
- `ErrorCode`는 응답 메시지의 예약된 필드 이름입니다. 수동으로 정의하지 마세요. `Resp` 메시지는 자동으로 `ErrorCode` 필드를 생성합니다

#### 제한 사항

- **중첩 타입 미지원**: message 내부에 `message`, `enum` 또는 다른 커스텀 타입을 중첩하는 것은 지원되지 않습니다
- **RPC 정의 미지원**: proto 파일 내의 RPC 서비스 정의는 지원되지 않습니다
- **proto3만 지원**: `syntax = "proto3";` 선언이 필수입니다. proto2는 지원되지 않습니다

#### 주석 규칙

- message 및 enum 정의 **위에** 주석을 추가:

```protobuf
// 하트비트 요청
message ReqHeartBeat
{
    int64 Timestamp = 1;
}
```

- 필드 줄 **끝에** 인라인 주석을 추가:

```protobuf
// 플레이어 정보
message PlayerInfo
{
    int64 Id = 1;         // 플레이어 ID
    string Name = 2;      // 플레이어 이름
    uint32 Level = 3;     // 플레이어 레벨
    int32 State = 4;      // 플레이어 상태
}
```

#### 샘플 Proto 파일

[TestProtos/](TestProtos/) 디렉토리에는 주요 패턴을 포괄하는 샘플 proto 파일이 있습니다:

| 파일 | 패턴 | 모듈 ID |
|------|------|---------|
| `heartbeat.proto` | 기본 Req/Resp | `1`(클라이언트-서버) |
| `player.proto` | Req/Resp/Notify + enum + map | `2`(클라이언트-서버) |
| `bag.proto` | enum + repeated + map + Notify | `3`(클라이언트-서버) |
| `admin-s.proto` | 서버 전용 프로토콜(`-s` 접미사) | `99`(클라이언트-서버) |
| `server-internal-s.proto` | 서버 간 통신(음수 모듈 ID) | `-1`(서버-서버) |

### 매개변수 설명

#### 핵심 매개변수

| 매개변수 | 필수 | 기본값 | 설명 |
|---------|------|--------|------|
| `--mode` | 예 | - | 언어 모드: `csharp`, `typescript`, `cpp`, `lua`, `go` |
| `--inputPath` | 예 | - | `.proto` 파일 디렉토리 경로 |
| `--outputPath` | 예 | - | 생성 파일의 출력 경로 |
| `--namespaceName` | 아니오 | `""` | 생성 코드의 네임스페이스(C#만 적용, TypeScript는 무시) |
| `--isGenerateErrorCode` | 아니오 | `true` | 응답 메시지에 `ErrorCode` 필드를 자동 생성할지 여부 |
| `--requireComments` | 아니오 | `none` | 주석 검증 수준: `none`(검증 없음), `container`(message/enum에 주석 필수), `member`(필드/enum 멤버에 주석 필수), `all`(모두) |

#### C# 전용 매개변수

| 매개변수 | 필수 | 기본값 | 설명 |
|---------|------|--------|------|
| `--usingStatements` | 아니오 | `""` | using 문을 `\|`로 구분하여 지정(예: `"using System\|using ProtoBuf\|using System.Collections.Generic"`) |
| `--isGenerateDescription` | 아니오 | `false` | `[System.ComponentModel.Description]` 속성을 생성할지 여부 |
| `--isServer` | 아니오 | `false` | 서버 전용 proto 파일(`-s` 또는 `_s`로 끝나는 파일)을 포함할지 여부 |

> 주의: 불리언 파라미터(`--isGenerateErrorCode`, `--isGenerateDescription`, `--isServer` 등)는 플래그 방식입니다: **지정하면 켜지며** `--xxx false` 로는 끌 수 없습니다(`false` 리터럴은 무시됨). 표의 "기본값"은 파라미터 생략 시의 값입니다(예: `--isGenerateErrorCode` 는 생략 시에도 켜짐).

#### TypeScript 전용 매개변수

| 매개변수 | 필수 | 기본값 | 설명 |
|---------|------|--------|------|
| `--importPath` | 아니오 | `"../network/"` | 생성되는 import 문의 경로 접두사 |
| `--isGenerateDescription` | 아니오 | `false` | JSDoc 스타일 주석을 생성할지 여부 |

#### 기타 매개변수

| 매개변수 | 필수 | 기본값 | 설명 |
|---------|------|--------|------|
| `--isGenerateErrorCodeExcelFile` | 아니오 | `true` | 에러 코드 Excel 파일을 생성할지 여부 |
| `--errorCodeExcelFilePath` | 아니오 | `""` | 에러 코드 Excel 파일의 사용자 지정 경로 |

### 서브 ID 안정성 (lock 파일)

#### 문제: 행 순서 자동 증가가 프로토콜을 깨뜨림

이전 버전의 `MessageIdHandler`는 proto 파일 내 **행 순서**대로 자동 증가하며 SubId를 할당했습니다(각 모듈은 10부터 시작). 따라서 proto 파일에 가해지는 구조적 편집은 해당 모듈의 SubId 전체를 일괄적으로 밀어냅니다:

| 작업 | 결과 |
|------|------|
| 중간에 메시지 하나 삽입 | 이후 모든 메시지의 SubId +1 |
| message 정의 순서 조정 | 해당 메시지들의 SubId가 서로 맞바뀜 |
| 메시지 하나 삭제 | 이후 모든 메시지의 SubId -1 |

SubId는 런타임 네트워크 프로토콜의 어드레싱 키입니다(`MessageID = (Module << 16) | SubId`). 한번 드리프트되면 C# / C++ / Go / Lua / TypeScript 각 끝단에서 생성된 레지스트리가 온라인의 기존 클라이언트와 **전체적으로 어긋나**, 이전 패키지가 실은 과거 Opcode가 다른 메시지로 해석됩니다. 행 순서는 텍스트 속성일 뿐이므로 프로토콜 의미론에 관여해서는 안 됩니다.

#### lock 메커니즘

활성화하면 `MessageIdCoordinator`가 `(Module, 메시지 이름) → SubId` 매핑을 JSON lock 파일(`proto-message-ids.lock.json`) 하나에 영속화하며, 할당 결정은 이름만 보고 행 순서는 보지 않습니다:

| 시나리오 | 할당 결정 |
|------|----------|
| 메시지 이름이 이미 lock에 있음 | 기존 SubId를 **그대로 사용**, 절대 재작성하지 않음 |
| 메시지 이름이 lock에 없음 | **max(이미 사용된 번호) + 1**, 빈 번호를 메우지 않음 |
| lock에는 있으나 이번 proto에 없음(삭제/이름 변경) | `retired` 섹션으로 이동, **절대 회수하지 않음** |

핵심 규칙:

- `schemaVersion`: lock 파일에 schema 버전 번호가 붙어 있으며, 버전이 호환되지 않으면 내보내기 도구가 즉시 오류를 냅니다. 절대 조용히 재정렬하지 않습니다
- 각 모듈(`option module = <id>`)은 **독립적으로 카운트**되며, 모듈 간 ID 공간은 서로 통하지 않습니다
- SubId의 유효 범위는 `1..65535`(16비트), 모듈 내 시작점은 `10`입니다
- 직렬화는 key 사전순으로 출력되어 diff에는 실제 변경만 담기므로 PR review에 유리합니다
- lock 파일 쓰기는 원자적 연산입니다(먼저 `.tmp`에 쓴 뒤 rename)

#### 마이그레이션 절차

```bash
# 1. 일회성 시드 생성: "지금 이 시점"의 Opcode를 lock 시작점으로 동결
bash tools/migrate-message-id-lock.sh
#    동등한 CLI:
#    dotnet ProtoExport.dll --inputPath <protos> --outputPath <out> \
#      --mode csharp --messageIdLockPath ./proto-message-ids.lock.json --regenerate-lock

# 2. lock 파일을 확인하고 git에 커밋
git add proto-message-ids.lock.json && git commit -m "chore(proto): freeze message id lock"

# 3. 이후 모든 내보내기(로컬 + CI + Docker)에는 반드시 lock 경로를 전달
dotnet ProtoExport.dll ... --messageIdLockPath ./proto-message-ids.lock.json
```

> **참고**: 마이그레이션은 **과거의 드리프트를 롤백하지 않습니다**. 이전 버전의 행 순서 자동 증가로 인한 어긋남은 이미 발생했으며, 시드는 "지금 이 시점을 동결"하여 이후 할당을 안정화할 뿐입니다. 이미 온라인에 드리프트 문제가 있다면 별도의 프로토콜 정렬 작업이 필요합니다.

관련 CLI 매개변수:

| 매개변수 | 설명 |
|------|------|
| `--messageIdLockPath` | lock 파일 경로. **비워 두면 lock 모드가 비활성화**되고 이전의 행 순서 자동 증가 동작으로 되돌아갑니다(하위 호환) |
| `--print-lock` | lock 파일 내용을 읽어 출력만 하고 내보내기를 트리거하지 않습니다(review용) |
| `--regenerate-lock` | 현재 파싱된 Opcode를 lock 시드로 직렬화합니다(일회성 마이그레이션용) |

#### merge conflict 처리

두 사람이 같은 모듈에 동시에 메시지를 추가하면 각자 동일한 `max+1`을 계산하므로, 병합 시 lock 파일은 반드시 충돌합니다. 처리 방법:

1. 어느 한쪽의 lock 내용을 취합니다(충돌 양쪽의 내용은 동등하며 단지 항목 정렬만 다릅니다)
2. 내보내기 도구를(`--messageIdLockPath`와 함께) 다시 실행하여, 나중에 합쳐진 새 메시지가 다음 사용 가능한 번호를 자동으로 재할당받게 합니다
3. lock 파일을 다시 커밋합니다

**이것은 결함이 아니라 의도된 특성입니다**: 충돌을 명시적으로 드러내는 것이 "하나의 번호에 두 주인이 없음"을 보장합니다. 조용한 자동 병합은 오히려 두 메시지가 같은 SubId를 공유하게 만들어 런타임 파싱 어긋남을 일으킬 수 있습니다. `retired` 섹션도 마찬가지입니다: 삭제/이름 변경된 이전 번호는 영구히 점유되며, 새 메시지는 `max+1`만 받을 수 있습니다.

### Docker 경로 매핑

Docker 사용 시 경로 매핑 규칙은 다음과 같습니다:

- `-v <호스트 경로>:<컨테이너 경로>` 로 호스트 디렉토리를 컨테이너에 마운트합니다
- `--inputPath` 와 `--outputPath` 에는 **컨테이너 내부 경로**(예: `/protos`, `/output`)를 지정해야 합니다(호스트 경로가 아님)

```bash
# 예시: 호스트 ./my-protos -> 컨테이너 /protos
docker run --rm \
  -v $(pwd)/my-protos:/protos \
  -v $(pwd)/my-output:/output \
  gameframex/gameframex-tools:latest \
  --mode csharp --isServer true \
  --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" \
  --isGenerateDescription true \
  --inputPath /protos --outputPath /output --namespaceName GameFrameX.Proto.Proto
```

## 의존성

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — 빌드 및 CLI 실행
- [Avalonia](https://avaloniaui.net/) — GUI 도구(ProtoExporterGUI)의 크로스 플랫폼 UI 프레임워크
- [Docker](https://www.docker.com/)(선택) — 컨테이너 방식으로 내보내기 도구 실행

## 문서 및 자료

- [공식 문서](https://gameframex.doc.alianblank.com)
- [통신 프로토콜 사양](https://gameframex.doc.alianblank.com/zh-CN/protobuf/require.html)
- [주의사항](https://gameframex.doc.alianblank.com/zh-CN/protobuf/note.html)

## 커뮤니티 및 지원

[![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[<img src="https://cdn.jsdelivr.net/npm/devicon@2/icons/linkedin/linkedin-original.svg" height="28" alt="LinkedIn" />](https://www.linkedin.com/in/alianblank)
[![Reddit](https://img.shields.io/badge/Reddit-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/GameFrameX/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/alian_blank)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
[![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?style=for-the-badge&logo=bluesky&logoColor=white)](https://bsky.app/profile/alianblank.bsky.social)
[![Bilibili](https://img.shields.io/badge/Bilibili-00A1D6?style=for-the-badge&logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?style=for-the-badge&logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)
![QQ](https://img.shields.io/badge/QQ-467608841%2F233840761-EB1923?style=for-the-badge&logo=qq&logoColor=white)

## 변경 로그

자세한 내용은 [CHANGELOG.md](CHANGELOG.md)을 참조하세요.

## 라이선스

자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.

<!--
EN: See [LICENSE.md](LICENSE.md) for license information.
zh-CN: 详见 [LICENSE.md](LICENSE.md) 文件。
zh-TW: 詳見 [LICENSE.md](LICENSE.md) 檔案。
ja: 詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
ko: 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
-->
