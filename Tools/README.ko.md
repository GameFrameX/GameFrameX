<div align="center">

![GameFrameX Logo](https://download.alianblank.com/gameframex/gameframex_logo_320.png)

# GameFrameX.Tools

[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Tools?label=version&color=green)](https://github.com/GameFrameX/GameFrameX.Tools/releases)
[![License](https://img.shields.io/badge/license-MIT+Apache%202.0-orange.svg)](LICENSE)
[![Documentation](https://img.shields.io/badge/docs-gameframex-brightgreen.svg)](https://gameframex.doc.alianblank.com)

**인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현**

[📖 문서](https://gameframex.doc.alianblank.com) • [💬 QQ 그룹: 467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)

---

🌐 **언어**: [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

---

</div>

# ProtoExport 도구

언어별 내보내기 Proto 프로토콜 코드 생성 도구입니다. C#, TypeScript를 지원하며 C++과 Lua는 개발 중입니다.

# 빌드

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


# Docker

`linux/amd64` 및 `linux/arm64` 용 Docker 이미지가 제공됩니다.

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

# Proto 프로토콜 사양

이 도구는 `.proto` 파일 형식에 특정 요구사항이 있습니다. 올바른 코드 생성을 위해 다음 규칙을 따르세요.

## 파일 형식 요구사항

```protobuf
syntax = "proto3";     // 필수: proto3만 지원
package Basic;
option module = 10;    // 필수: 모듈 ID 정의

// 하트비트 요청
message ReqHeartBeat
{
    int64 Timestamp = 1; // 타임스탬프
}
```

## 메시지 명명 규칙

- **요청 메시지**: `Req`로 시작해야 합니다 (예: `ReqLogin`, `ReqHeartBeat`)
- **응답 메시지**: `Resp`로 시작해야 합니다 (예: `RespLogin`)
- **알림 메시지**: `Notify`로 시작해야 합니다 (예: `NotifyBagInfoChanged`)
- 메시지 이름, 필드 이름, enum 이름, enum 값은 모두 **UpperCamelCase**를 사용해야 합니다

## 모듈 ID 규칙

`option module = <id>;`로 모듈 ID를 정의합니다:

| ID 범위 | 용도 |
|---------|------|
| `0` ~ `32767` | 클라이언트-서버 통신 |
| `-32768` ~ `-1` | 서버-서버 통신 |

## 필드 번호 규칙

- 메시지 필드 번호는 **800 미만**이어야 합니다 (800 이상은 시스템 예약이며 파싱 오류를 발생시킵니다)
- `ErrorCode`는 응답 메시지의 예약된 필드 이름입니다. 수동으로 정의하지 마세요. `Resp` 메시지는 자동으로 `ErrorCode` 필드를 생성합니다

## 제한 사항

- **중첩 타입 미지원**: message 내부에 `message`, `enum` 또는 다른 커스텀 타입을 중첩하는 것은 지원되지 않습니다
- **RPC 정의 미지원**: proto 파일 내의 RPC 서비스 정의는 지원되지 않습니다
- **proto3만 지원**: `syntax = "proto3";` 선언이 필수입니다. proto2는 지원되지 않습니다

## 주석 규칙

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

전체 프로토콜 사양은 [통신 프로토콜 사양](https://gameframex.doc.alianblank.com/zh-CN/protobuf/require.html) 및 [주의사항](https://gameframex.doc.alianblank.com/zh-CN/protobuf/note.html) 문서를 참조하세요.

## 샘플 Proto 파일

[TestProtos/](TestProtos/) 디렉토리에는 주요 패턴을 포괄하는 샘플 proto 파일이 있습니다:

| 파일 | 패턴 | 모듈 ID |
|------|------|---------|
| `heartbeat.proto` | 기본 Req/Resp | `1` (클라이언트-서버) |
| `player.proto` | Req/Resp/Notify + enum + map | `2` (클라이언트-서버) |
| `bag.proto` | enum + repeated + map + Notify | `3` (클라이언트-서버) |
| `admin-s.proto` | 서버 전용 프로토콜 (`-s` 접미사) | `99` (클라이언트-서버) |
| `server-internal-s.proto` | 서버 간 통신 (음수 모듈 ID) | `-1` (서버-서버) |

---

# 매개변수 설명

## 핵심 매개변수

| 매개변수 | 필수 | 기본값 | 설명 |
|---------|------|--------|------|
| `--mode` | 예 | - | 언어 모드: `csharp`, `typescript`, `cpp`, `lua` |
| `--inputPath` | 예 | - | `.proto` 파일 디렉토리 경로 |
| `--outputPath` | 예 | - | 생성 파일의 출력 경로 |
| `--namespaceName` | 아니오 | `""` | 생성 코드의 네임스페이스 (C#만 적용, TypeScript는 무시) |
| `--isGenerateErrorCode` | 아니오 | `true` | 응답 메시지에 `ErrorCode` 필드를 자동 생성할지 여부 |
| `--requireComments` | 아니오 | `none` | 주석 검증 수준: `none` (검증 없음), `container` (message/enum에 주석 필수), `member` (필드/enum 멤버에 주석 필수), `all` (모두) |

## C# 전용 매개변수

| 매개변수 | 필수 | 기본값 | 설명 |
|---------|------|--------|------|
| `--usingStatements` | 아니오 | `""` | using 문을 `\|`로 구분하여 지정 (예: `"using System\|using ProtoBuf\|using System.Collections.Generic"`) |
| `--isGenerateDescription` | 아니오 | `false` | `[System.ComponentModel.Description]` 속성을 생성할지 여부 |
| `--isServer` | 아니오 | `false` | 서버 전용 proto 파일 (`-s` 또는 `_s`로 끝나는 파일)을 포함할지 여부 |

## TypeScript 전용 매개변수

| 매개변수 | 필수 | 기본값 | 설명 |
|---------|------|--------|------|
| `--importPath` | 아니오 | `"../network/"` | 생성되는 import 문의 경로 접두사 |
| `--isGenerateDescription` | 아니오 | `false` | JSDoc 스타일 주석을 생성할지 여부 |

## 기타 매개변수

| 매개변수 | 필수 | 기본값 | 설명 |
|---------|------|--------|------|
| `--isGenerateErrorCodeExcelFile` | 아니오 | `true` | 에러 코드 Excel 파일을 생성할지 여부 |
| `--errorCodeExcelFilePath` | 아니오 | `""` | 에러 코드 Excel 파일의 사용자 지정 경로 |

---

# 모드 상세 및 예시

| 모드 | 출력 언어 | 파일 확장자 | 설명 |
|------|----------|------------|------|
| `csharp` | C# | `.cs` | Server, Unity, Godot, Stride, Flax 등에 적용 |
| `typescript` | TypeScript | `.ts` | LayaAir, Cocos Creator, Phaser 등에 적용 |
| `cpp` | C++ | `.h` | Unreal Engine 등에 적용 |
| `lua` | Lua | `.lua` | Defold, Solar2D, Dora SSR 등에 적용 |
| `go` | Go | `.go` | Go 게임 서버 등에 적용 |

## C# 모드

`[ProtoContract]` / `[ProtoMember]` 속성이 포함된 C# 코드를 생성합니다. 모든 동작은 CLI 매개변수로 제어되며, 엔진별 하드코딩이 없습니다.

### 서버 내보내기

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

### Unity 내보내기

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

### Godot 내보내기

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

## TypeScript 모드

`export namespace`, `export class`, `export enum`이 포함된 `.ts` 파일과 집계 파일 `ProtoMessageRegister.ts`를 생성합니다. 서버 전용 proto 파일은 자동으로 건너뜁니다.

### 기본 import 경로

```bash
dotnet ProtoExport.dll \
  --mode typescript \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Laya/src/gameframex/protobuf \
  --isGenerateErrorCode true
```

### 사용자 지정 import 경로

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

## C++ 모드

`#pragma once`, 네임스페이스, `enum class`, 클래스 정의가 포함된 C++ 헤더 파일을 생성합니다. `MessageObject`를 상속하는 클래스에는 `MESSAGE_ID`와 `Clear()` 메서드가 포함됩니다.

```bash
dotnet ProtoExport.dll \
  --mode cpp \
  --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" \
  --inputPath ./../../../../../Protobuf \
  --outputPath ./../../../../../Unreal/Source/Proto \
  --namespaceName GameFrameX.Proto
```

## Lua 모드

LuaDoc (EmmyLua) 스타일 타입 어노테이션과 모듈 기반 메시지 정의가 포함된 `.lua` 파일을 생성합니다. 집계 파일 `ProtoMessageRegister.lua`도 함께 생성됩니다.

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

## Go 모드

protobuf 태그가 포함된 Go struct 정의, enum 타입 정의, 집계 파일 `message_register.go`를 생성합니다. `--namespaceName`은 Go 패키지 이름으로 사용됩니다 (점으로 구분된 경우 마지막 세그먼트).

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

---

# GUI 도구 (ProtoExporterGUI)

ProtoExport CLI를 래핑하는 크로스 플랫폼 Avalonia GUI로, 명령줄 인수를 직접 입력할 필요가 없습니다.

## 기능

- 7가지 내보내기 모드 (Server / Unity / Godot / TypeScript / C++ / Lua / Go) 드롭다운 전환
- 모든 CLI 매개변수를 시각적으로 편집 (네임스페이스, using/import, 주석 검증 수준, 에러 코드, Description, 서버 모드)
- 폴더 피커로 경로를 탐색하여 선택 (직접 입력 불필요)
- 중국어 / 영어 UI를 실시간으로 전환
- 설정 영속화 (모드별로 프로그램 디렉토리의 `Setting.json`에 저장, 기본값과 깊이 병합하여 무손실 업그레이드)
- 실시간 로그 출력 패널

## 빌드 및 게시

```bash
dotnet publish ProtoExporterGUI/ProtoExporterGUI.csproj -c Release -r win-x64 --no-self-contained
# 기타 RID: osx-arm64 / osx-x64 / linux-x64 / linux-arm64
```

산출물은 단일 파일 실행 파일입니다 (런타임을 포함하지 않으므로 대상 머신에 .NET 10 런타임 사전 설치가 필요합니다).

## 사용 방법

1. 앱을 실행합니다
2. 내보내기 모드를 선택합니다
3. 입력/출력 경로를 입력하거나 탐색하여 선택합니다
4. 내보내기를 클릭합니다
5. 로그 패널을 확인합니다

GUI는 CLI와 기능적으로 동등합니다. CI/자동화 시나리오에서는 여전히 CLI 또는 Docker 사용을 권장합니다.

---

# 빠른 내보내기 스크립트

`Protobuf/` 디렉토리에 사전 설정된 내보내기 스크립트가 제공됩니다:

| 스크립트 | 설명 |
|---------|------|
| `Proto2CsExport_Server.sh/.bat` | C# 서버용 코드 내보내기 |
| `Proto2CsExport_Client.sh/.bat` | C# Unity 클라이언트용 코드 내보내기 |
| `Proto2TsExport.sh/.bat` | TypeScript 코드 내보내기 |
| `Proto2CppExport.sh/.bat` | C++ 코드 내보내기 |
| `Proto2LuaExport.sh/.bat` | Lua 코드 내보내기 |
| `Proto2GoExport.sh/.bat` | Go 코드 내보내기 |

---

# Docker 경로 매핑

Docker 사용 시 경로 매핑 규칙은 다음과 같습니다:

- `-v <호스트 경로>:<컨테이너 경로>` 로 호스트 디렉토리를 컨테이너에 마운트합니다
- `--inputPath` 와 `--outputPath` 에는 **컨테이너 내부 경로** (예: `/protos`, `/output`)를 지정해야 합니다 (호스트 경로가 아님)

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
