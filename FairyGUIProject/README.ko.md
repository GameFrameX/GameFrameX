<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX FairyGUI Project

[![License](https://img.shields.io/github/license/GameFrameX/GameFrameX.FairyGUIProject)](LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.FairyGUIProject)](https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases)
[![Documentation](https://img.shields.io/badge/Documentation-doc.alianblank.com-blue)](https://gameframex.doc.alianblank.com)

인디 게임 프론트엔드/백엔드 통합 솔루션 · 인디 게임 개발자의 꿈을 이뤄주는 동반자

<br />

[문서](https://gameframex.doc.alianblank.com) · [빠른 시작](#빠른-시작) · QQ 그룹: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>

## 이 프로젝트는 무엇인가요?

쉽게 말해: **이것은 GameFrameX의 모든 게임 UI "디자인 소스 파일"입니다.**

Figma 파일을 상상해 보세요. 다만 웹 디자인 시안이 아니라 게임 안의 UI(로그인 화면, 메인 화면, 인벤토리, 로딩 화면...)를 담고 있습니다. **FairyGUI 에디터**라는 무료 도구로 열어서 드래그 앤 드롭으로 UI를 그린 뒤, 버튼 하나만 누르면 Unity에서 바로 쓸 수 있는 형태로 내보낼 수 있습니다.

내보내면 두 가지를 얻게 됩니다:

- **아트 리소스 패키지**(`.bytes` 파일): UI에 쓰인 이미지, 애니메이션으로, 게임 실행 중 Unity가 로드합니다.
- **C# 바인딩 코드**: UI 안의 모든 버튼, 리스트, 슬라이더에 타입이 있는 프로퍼티를 생성합니다. 덕분에 프로그래머는 `loginPanel.btn_start.onClick = ...`처럼 쓸 수 있고, 문자열로 컨트롤을 찾을 필요가 없습니다.

C# 코드는 직접 작성할 필요가 없습니다. 플러그인이 자동으로 생성해 줍니다.

## 빠른 시작

전체 흐름을 한마디로 요약하면 4단계입니다:

1. FairyGUI 에디터로 `Game.fairy`를 **엽니다**.
2. 어느 화면을 **편집합니다**. 예를 들어 로그인 버튼의 텍스트를 바꿔 봅니다.
3. **퍼블리시를 클릭합니다**. 같은 레벨의 Unity 프로젝트에 두 가지가 나타납니다:
   - `../Unity/Assets/Bundles/UI/*.bytes` —— 아트 리소스
   - `../Unity/Assets/Hotfix/UI/FairyGUI/.../*.cs` —— 바인딩 코드
4. **Unity에서 사용합니다**: `UILoginPanel.CreateInstance()`를 호출하면 이 화면을 표시할 수 있습니다.

아래에서 각 단계마다 구체적인 예를 듭니다. 먼저 준비물부터 살펴봅시다.

## 준비물(필요한 것)

| 도구 | 용도 | 어디서 구하나요 |
|------|------|----------------|
| FairyGUI 에디터 ≥ 5.0 | 이 프로젝트를 열고 편집하는 디자인 도구 | https://www.fairygui.com/ |
| Unity 프로젝트 하나 | 내보낸 리소스 패키지와 코드를 받을 대상 | 이 저장소와 같은 레벨의 디렉터리에 배치 |

> 이것은 Unity 플러그인 패키지가 아니며, Unity Package Manager로 설치할 수 없습니다. 저장소를 클론해서 당신의 Unity 프로젝트와 같은 부모 디렉터리에 두기만 하면 됩니다:
> ```
> git clone git@github.com:GameFrameX/GameFrameX.FairyGUIProject.git
> ```
> 디렉터리 구조는 이렇게 됩니다:
> ```
> <workspace>/
> ├── GameFrameX.FairyGUIProject/   ← 이 저장소(여기서 Game.fairy 열기)
> └── Unity/                         ← 당신의 Unity 게임(내보내기 결과 수신)
> ```

## 1단계: 프로젝트 열기

1. FairyGUI 에디터(5.0 이상)를 설치합니다.
2. 이 저장소 안의 **`Game.fairy`** 를 더블 클릭합니다.
3. 에디터가 열린 뒤, 왼쪽 패널에서 **9개의 UI 패키지**를 볼 수 있습니다.

> **예시:** `UILogin`을 클릭하면 로그인 화면의 디자인이 보입니다: 배경 이미지 하나, 아이디 입력란, 비밀번호 입력란, 그리고 "로그인" 버튼.

프로젝트는 이미 다음과 같이 미리 설정되어 있습니다 (보통은 바꾸지 않아도 됩니다):

- 해상도 1080 × 2160(세로 휴대폰), 스케일 모드 `MatchWidthOrHeight`.
- 통일된 글꼴, 색 팔레트, 스크롤바로, `settings/Common.json`에 집중 작성되어 전역에서 공유합니다.
- 아틀라스 설정: 2048 상한, 페이징, 2의 거듭제곱, 회전 허용, 이미지 트리밍(`settings/Publish.json`), 모바일 최적화.
- 퍼블리시할 때 `UI` / `Res` / `Def` 세 개의 패키지 그룹으로 나뉩니다(`settings/PackageGroup.json`).

## 2단계: UI 패키지 이해하기

**패키지(Package)** 는 폴더처럼, 서로 관련된 화면과 그 안에서 쓰이는 아트 리소스를 하나로 묶어 둡니다. 이 프로젝트에는 9개의 패키지가 있습니다:

| 패키지 | 무엇인가요 | 안의 대표적인 화면 |
|--------|-----------|-------------------|
| `UILauncher` | 시작 스플래시 | 게임 시작 시의 Logo |
| `UILoading` | 로딩 화면 | 리소스 로딩 중의 진행률 바 |
| `UILogin` | 로그인 화면 | 아이디 / 비밀번호 / 로그인 버튼 |
| `UIMain` | 메인 화면 HUD | 로그인 후의 상단 바와 메뉴 |
| `UIBag` | 인벤토리 | 아이템 그리드 |
| `UIRoom` | 룸 / 로비 | 방 목록, 준비 버튼 |
| `UIPlayer` | 플레이어 패널 | 아바타, 속성 |
| `UICommon` | 공용 컴포넌트 | 여기저기서 재사용되는 버튼 등 |
| `UICommonAvatar` | 공용 아바타 | 아바타 컨트롤 |

> **팁:** 이름이 모두 `UI`로 시작하는 것은 우연이 아닙니다. 이는 퍼블리시 규칙이 요구하는 사항입니다 (뒤의 "이름 규칙" 참조).

## 3단계: 화면 편집하기

> **예시: 로그인 버튼의 이름을 바꿔 봅니다.**
>
> 1. `UILogin` 패키지를 열고 → `UILoginPanel` 컴포넌트를 더블 클릭합니다.
> 2. 그 로그인 버튼을 선택하고, 오른쪽 속성 패널에서 텍스트를 `登录`에서 `Sign In`로 바꿉니다.
> 3. 저장(Ctrl+S). 완료되었습니다.

기억하세요: 여기서의 디자인 변경은 **퍼블리시하기 전까지는** 시각적인 것일 뿐, Unity 프로젝트에는 아직 영향을 주지 않습니다.

## 4단계: 퍼블리시(내보내기)

이제 기적을 목격할 단계입니다.

1. 에디터에서 **파일 → 퍼블리시**를 실행합니다 (또는 툴바의 퍼블리시 버튼을 누릅니다).
2. 퍼블리시 대화상자에서 **"코드 생성"** 이 체크되어 있는지 확인합니다.
3. 에디터가 같은 레벨의 Unity 프로젝트에 파일을 작성합니다:

```
../Unity/Assets/Bundles/UI/           ← *.bytes 아트 리소스 패키지
../Unity/Assets/Hotfix/UI/FairyGUI/   ← 생성된 C# 바인딩 코드
```

> **배경에서 플러그인이 하는 일:** 퍼블리시할 때 `plugins/gencode/` 아래의 코드 생성 플러그인이 실행됩니다. "내보내기"로 표시된 각 컴포넌트를 읽어, 컴포넌트마다 `.cs` 파일을 하나씩 생성하고, 추가로 `PackageXxx.cs`도 만들어냅니다.

> **주의:** 컴포넌트에 "내보내기" 표시가 안 되어 있거나, 퍼블리시할 때 "코드 생성"에 체크하지 않으면 C# 코드가 생성되지 않습니다. 신규 사용자가 가장 많이 겪는 함정입니다 (FAQ 참조).

## 5단계: 생성된 C# 코드의 모습

`UILogin`을 퍼블리시한 뒤, 아래와 비슷한 파일을 얻게 됩니다 (이해 관계 없는 부분은 생략하여 단순화했습니다):

```csharp
#if ENABLE_UI_FAIRYGUI
namespace Hotfix.UI
{
    public sealed partial class UILoginPanel : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UILoginPanel";

        public GButton btn_start { get; private set; }    // 자동 바인딩
        public GTextField txt_title { get; private set; } // 자동 바인딩

        public static UILoginPanel CreateInstance() { /* 인스턴스를 생성해 반환 */ }

        protected override void InitView()
        {
            btn_start  = (GButton)com.GetChild("btn_start");
            txt_title  = (GTextField)com.GetChild("txt_title");
        }
    }
}
#endif
```

그래서 Unity 안의 프로그래머는 이렇게 사용할 수 있습니다:

```csharp
var panel = UILoginPanel.CreateInstance();          // 로그인 화면 표시
panel.btn_start.onClick.Add(() => Debug.Log("로그인 클릭됨")); // 버튼 클릭 시 실행
```

문자열을 찾아볼 필요도 없고, 오타가 날 일도 없습니다. 이름이 부여된 모든 컨트롤이 자동으로 타입이 있는 프로퍼티가 됩니다.

## 이름과 크기 규칙(퍼블리시 시 강제 검사)

플러그인은 **퍼블리시할 때 패키지마다 검사**하며, 어느 하나라도 위반하면 퍼블리시를 멈추고 에러를 냅니다. 이 규칙들은 생성된 코드를 깔끔하고 통일되게 만들기 위함입니다.

아래의 각 규칙에는 "올바름 / 틀림" 대조를 함께 드리고, 에러 메시지가 어떻게 보이는지도 설명합니다.

### 규칙 1: 패키지 이름은 `UI`로 시작해야 하며, 영문자만 포함

| ✅ 올바름 | ❌ 틀림 | 어디가 틀렸는지 |
|----------|--------|----------------|
| `UILogin` | `Login` | `UI` 접두사가 없음 |
| `UIBag` | `UI_Login` | 밑줄을 쓸 수 없음 |
| `UIPlayer` | `UI1` | 숫자를 쓸 수 없음 |

위반 시 에러: `包名 'xxx' 必须以'UI'开头并且只能包含字母的大写驼峰命名`.

### 규칙 2: 컴포넌트 이름은 `UI`로 시작해야 하며, 영문자만 포함

| ✅ 올바름 | ❌ 틀림 | 어디가 틀렸는지 |
|----------|--------|----------------|
| `UILoginPanel` | `LoginPanel` | `UI` 접두사가 없음 |
| `UIBagItem` | `UILogin_Panel` | 밑줄을 쓸 수 없음 |

### 규칙 3: 컴포넌트 이름은 해당 패키지 이름으로 시작해야 함

컴포넌트는 어느 한 패키지 안에 들어 있으므로, 이름에 패키지 이름을 접두사로 붙여야 합니다.

| 속한 패키지 | ✅ 올바름 | ❌ 틀림 | 어디가 틀렸는지 |
|-------------|----------|--------|----------------|
| `UILogin` | `UILoginPanel` | `UIMainPanel` | 접두사는 `UILogin`이어야 함 |
| `UIBag` | `UIBagItem` | `UILoginItem` | 접두사는 `UIBag`이어야 함 |

### 규칙 4: 멤버 이름은 모두 소문자여야 함 (소문자 + 밑줄)

UI 안에서 각 컨트롤에 붙인 이름(변수명)은 모두 소문자여야 합니다. **예외**: Controller 는 제한 없음, 그리고 세 개의 예약어 `closeButton`, `dragArea`, `contentArea` 도 카멜 표기법을 쓸 수 있습니다.

| ✅ 올바름 | ❌ 틀림 | 어디가 틀렸는지 |
|----------|--------|----------------|
| `btn_start` | `BtnStart` | 대문자가 포함됨 |
| `txt_title` | `txtTitle` | 대문자가 포함됨 |
| `list_items` | `listItems` | 대문자가 포함됨 |

### 규칙 5: 너비와 높이는 모두 짝수여야 함

내보낸 각 컴포넌트, 그리고 아트 리소스를 가진 각 멤버는 너비와 높이가 모두 짝수여야 합니다.

| ✅ 올바름 | ❌ 틀림 | 어디가 틀렸는지 |
|----------|--------|----------------|
| 1080 × 1920 | 1081 × 1920 | 너비가 홀수 |
| 200 × 80 | 200 × 81 | 높이가 홀수 |

> **왜 짝수여야 하나요?** 모바일의 픽셀 중심 맞춤과 아틀라스 패킹이 정확히 정렬되도록 하여, 반 픽셀 흐려짐을 피하기 위해서입니다.

### 플러그인이 자동으로 하는 일(신경 쓰지 않아도 됨)

- **"내보내기"** 로 표시된 컴포넌트만 팩토리 메서드 `CreateInstance()` / `CreateInstanceAsync()`가 생성됩니다.
- 멤버는 타입에 따라 자동으로 바인딩됩니다: 일반 객체는 `GetChild`, Controller 는 `GetController`, Transition 은 `GetTransition`을 사용하며, 커스텀 컴포넌트라면 `Xxx.Create(...)`로 한 번 감싸집니다.
- 패키지를 가로지르는 커스텀 컴포넌트는 자동으로 원본 패키지의 실제 타입명으로 돌아갑니다.
- 타입명에 `Scene`이 들어간 멤버는 해제 시 자동으로 `Dispose()`가 호출됩니다.
- 생성 코드의 네임스페이스: 기본은 `Hotfix.UI`; 내보내기 경로에 `Unity/Assets/Scripts`가 포함되어 있으면 자동으로 `Unity.Startup`으로 바뀝니다.
- 모든 생성 코드는 `#if ENABLE_UI_FAIRYGUI`로 감싸져 있어, Unity에서 켜고 끄기 편합니다.

## 자주 묻는 질문(FAQ)

**Q: 퍼블리시할 때 "패키지명은 UI로 시작해야 합니다"라는 에러가 납니다.**
A: 패키지명을 `UI`로 시작하고 영문자만 쓰도록 바꾸세요. 예: `UIBoss`.

**Q: 퍼블리시할 때 "너비는 짝수여야 합니다"라는 에러가 납니다.**
A: 해당 컴포넌트를 열어 너비 / 높이를 모두 짝수로 설정하세요 (오른쪽 속성 패널 → 크기).

**Q: 퍼블리시해도 C# 코드가 생성되지 않습니다.**
A: 대부분 두 가지 원인 중 하나입니다: (1) 퍼블리시 대화상자에서 "코드 생성"에 체크하지 않았거나; (2) 에디터에서 이 컴포넌트를 "내보내기"로 표시하지 않았거나.

**Q: 분명히 컨트롤에 이름을 지었는데, 생성된 코드에 나타나지 않습니다.**
A: 이름에 대문자가 들어 있을 수 있습니다. 모두 소문자로 바꿔 보세요 (규칙 4 참조).

**Q: 새로운 화면을 추가하고 싶은데 어떻게 하나요?**
A: (1) `UI`로 시작하는 새 패키지를 만들거나 기존 패키지를 사용합니다; (2) 패키지 안에 새 컴포넌트를 만들고, 이름을 `UI` + 패키지명으로 시작하게 합니다; (3) 사용할 컨트롤에 모두 소문자 이름을 붙입니다; (4) 컴포넌트를 "내보내기"로 표시합니다; (5) 너비/높이를 짝수로 설정합니다; (6) 퍼블리시합니다.

## 의존성

- FairyGUI 에디터 ≥ 5.0 (디자인 도구).
- 같은 레벨의 Unity 프로젝트로, 내보낸 리소스 패키지와 코드를 받습니다.
- Unity 측에 필요: FairyGUI 런타임, UniTask, GameFrameX (`Entity.Runtime`, `UI.Runtime`, `UI.FairyGUI.Runtime`, `Runtime`).

## 문서 및 자료

- 공식 문서: https://gameframex.doc.alianblank.com
- GitHub Releases: https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases
- FairyGUI 공식 사이트: https://www.fairygui.com/

## 커뮤니티 및 지원

- QQ 그룹: 467608841 / 233840761

## 변경 로그

전체 변경 로그는 [GitHub Releases](https://github.com/GameFrameX/GameFrameX.FairyGUIProject/releases)에서 확인하세요.

첫 릴리스에는 FairyGUI 프로젝트 골격과 첫 번째 UI 자산 패키지가 포함되어 있습니다.

## 라이선스

자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
