<div align="center">
    <a href="https://trendshift.io/repositories/7536" target="_blank"><img src="https://trendshift.io/api/badge/repositories/7536" alt="GameFrameX%2FGameFrameX | Trendshift" style="width: 250px; height: 55px;" width="250" height="55"/></a>
</div>

[简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [English](README.md) | [日本語](README.ja.md) | **한국어**

# 🎮 GameFrameX가 뭐예요?

한마디로: **게임을 「아이디어 → 만들기 → 출시 운영」까지 싹 다 해주는 오픈소스 툴박스예요.**

게임 만들 때 진짜 까다로운 건 「캐릭터 그리기, 스킬 만들기」가 아니라, 이런 조각들을 하나로 끌어모으는 일이에요:

- 플레이어 세이브 데이터는 어디에 저장하고, 어떻게 불러올까?
- 멀티플레이할 때 서버는 메시지를 어떻게 중계할까?
- 아이템, 스테이지, 레벨 같은 데이터는 누가 관리하고, 기획자가 고치면 어떡하지?
- 출시 이후에는 데이터를 어떻게 보고, 플레이어를 어떻게 관리하고, 새 버전은 어떻게 배포할까?

이런 「삽질」은 GameFrameX가 다 알아서 해주니까, 당신은 「내 게임이 재밌는가」에만 집중하면 돼요.

이런 메이저 엔진을 지원해요: **Unity, Cocos Creator, LayaAir(LayaBox), Godot** —— 어느 걸 쓰든 다 소화해요.

---

# 🧰 어떤 귀찮은 일들을 덜어줄까요?

| 원래 직접 다뤄야 했던 일 | GameFrameX가 알아서 준비해주는 것 |
|---|---|
| 멀티플레이 서버를 처음부터 짜기 | 바로 쓸 수 있는 고성능 서버(.NET으로 작성, 동시 접속자도 잘 버템) |
| 데이터를 어떻게 저장할지 | 플레이어 데이터는 MongoDB에(빠른 읽기/쓰기), 백오피스 데이터는 PostgreSQL에(안정적) |
| Excel 설정을 코드로 손 옮기기 | LuBan으로 클릭 한 번에 Excel을 코드와 데이터로 변환 |
| 클라이언트와 서버 「암호 맞추기」 | ProtoBuf로 프로토콜을 통일하고, 한 곳을 고치면 양 끝이 동기화 |
| 출시 후 아무것도 모르는 상태 | 관리 백오피스 웹 페이지가 기본 탑재, 데이터 조회 / 플레이어 관리 / 설정 배포 |
| 서버 배포가 머리 아픔 | Docker로 원클릭 패키징 배포, 속 편함 |

> 쉽게 말해: **한 사람도 소규모 팀처럼 온라인 게임 하나를 만들고, 운영까지 이어갈 수 있어요.**

---

# 👤 누가 쓰면 좋을까요?

- **온라인 / 멀티 게임**을 만들고 싶은데 「서버는 어떻게 하지」에 막힌 인디 개발자
- 아이디어를 검증할 **게임 프로토타입**을 빠르게 만들고 싶은 소규모 팀
- 「클라이언트 + 서버 + 백오피스」 전체 흐름을 처음부터 끝까지 배우고 싶은 학습자

---

# 🗺️ 이 저장소들은 다 뭐 하는 곳인가요? (저장소 맵)

GameFrameX는 「올인원 패키지」예요. 단, 패키지 안의 각 요리는 **각자 독립된 저장소**에 담겨 있어요(개별적으로 유지보수/업그레이드하기 쉽도록요). 먼저 이 표로 전체 윤곽을 잡아봐요:

| 저장소 | 쉽게 말하면… | 주소 |
|---|---|---|
| 🏠 **메인 저장소(여기)** | 「주방 배치도」 —— 모든 부품이 어느 폴더에 들어가야 하는지 알려줌 | https://github.com/GameFrameX/GameFrameX |
| 🌐 **서버** | 게임의 두뇌, 멀티플레이 · 세이브 · 전투 로직을 담당(GeekServer 기반으로 진화) | https://github.com/GameFrameX/GameFrameX.Server |
| 📊 **설정표(LuBan)** | Excel로 게임 데이터(아이템 / 스테이지 / 레벨…)를 채우고, 클릭 한 번에 코드 생성 | https://github.com/GameFrameX/GameFrameX.Config |
| 📡 **통신 프로토콜(ProtoBuf)** | 클라이언트와 서버가 「대화하는 규칙」, 양쪽이 주고받을 메시지를 정의 | https://github.com/GameFrameX/GameFrameX.Protobuf |
| 🎨 **UI 프로젝트(FairyGUI)** | FairyGUI 에디터로 게임 UI를 그리는 소스 프로젝트 | https://github.com/GameFrameX/GameFrameX.FairyGUIProject |
| 🛠️ **도구 모음** | 각종 보조 도구 | https://github.com/GameFrameX/GameFrameX.Tools |
| 💻 **관리 백오피스** | 출시 후 데이터 · 플레이어를 관리하는 웹(일부 소스코드는 비공개) | https://github.com/GameFrameX/GameFrameX.Admin |

백오피스 온라인 데모 👉 https://game.admin.web.vue.alianblank.com

## 🎮 클라이언트(넷 중 하나만 고르세요. 쓰는 걸 다운로드)

| 엔진 | 주소 |
|---|---|
| Unity | https://github.com/GameFrameX/GameFrameX.Unity |
| Cocos Creator | https://github.com/GameFrameX/GameFrameX.CocosCreator |
| LayaAir(LayaBox) | https://github.com/GameFrameX/GameFrameX.LayaBox |
| Godot | https://github.com/GameFrameX/GameFrameX.Godot |

---

# 📁 폴더를 마음대로 두면 안 되는 이유는?

> ⚠️ **중요**: 이 프레임워크는 **상대 경로**로 파일을 찾아요. 집의 콘센트 위치와 같아서 —— 서버를 `Server/`에서 `MyServer/`로 옮기면 전체 파이프라인이 길을 잃어버려요.

그러니 아래 구조대로, 각 저장소를 **제자리 폴더**에 두세요:

```
GameFrameX/                  # 프로젝트 루트 디렉토리(이름은 바꿔도 됨)
├── Config/                  # ← GameFrameX.Config를 여기에(Excel 설정 + LuBan 내보내기)
├── Protobuf/                # ← GameFrameX.Protobuf를 여기에(통신 프로토콜)
├── FairyGUIProject/         # ← GameFrameX.FairyGUIProject를 여기에(UI 편집 프로젝트)
├── Server/                  # ← GameFrameX.Server를 여기에(게임 서버)
├── Unity/                   # ← GameFrameX.Unity를 여기에(Unity 클라이언트, 필요하면 다른 엔진으로 교체)
│   ├── Assets/              #    Unity 리소스 디렉토리
│   ├── Packages/            #    Unity 패키지
│   ├── ProjectSettings/     #    Unity 프로젝트 설정
│   └── UserSettings/        #    Unity 사용자 설정
├── Tools/                   # ← GameFrameX.Tools를 여기에(보조 도구)
├── docker/                  # Docker 로컬 실행 환경(MongoDB / PostgreSQL)
├── Docs/                    # 문서(현재는 주로 GeekServer 원본 문서)
└── LICENSE.md               # 오픈소스 라이선스
```

> 다른 클라이언트 엔진으로 바꾸고 싶다고요? `Unity/`를 해당 이름으로 바꾸면 돼요(`Laya/`, `CocosCreator/`, `Godot/`), 규칙은 같아요.

---

# 🔧 먼저 환경을 준비해요

시작하기 전에 아래 도구들을 먼저 설치해요(링크를 누르면 공식 사이트로):

| 설치 항목 | 버전 | 용도 | 다운로드 |
|---|---|---|---|
| **Git** | 아무 최신 버전 | 각 저장소의 코드를 가져오기 | https://git-scm.com/ |
| **.NET SDK** | **10.0 이상** | 서버 컴파일/실행, LuBan 내보내기 도구 실행 | https://dotnet.microsoft.com/download |
| **Unity 에디터** | **2019.4.40f1**(2019.4+ 호환) | Unity 클라이언트를 열고 실행 | https://unity.com/download |
| **Docker**(선택이지만 권장) | 아무 최신 버전 | 원클릭으로 로컬 데이터베이스 MongoDB / PostgreSQL 실행 | https://www.docker.com/ |

> 💡 서버와 내보내기 도구 모두 **.NET 10.0**에 의존해요. 가장 중요한 버전 요구사항이니 반드시 맞춰 설치하세요.

---

# 🚀 제로부터 시작해요, 하나씩 따라 하기

**1단계**: 프로젝트를 넣을 새 폴더를 만들고 터미널을 열어요(Windows는 cmd / PowerShell, Mac / Linux는 터미널). 그리고 `cd`로 들어가요.

**2단계**: 「주방 배치도」를 내려받아요:

```shell
git clone https://github.com/GameFrameX/GameFrameX.git
```

그러면 `GameFrameX/` 폴더가 생기고, 안에 프로젝트 뼈대가 담겨 있어요.

**3단계**: 각 부품을 `GameFrameX/` 안의 **해당 폴더**에 넣어요(아래는 Unity 기준이에요. 다른 엔진을 쓰면 마지막 줄을 해당 주소로 바꾸세요):

```shell
git clone https://github.com/GameFrameX/GameFrameX.Server.git ./GameFrameX/Server
git clone https://github.com/GameFrameX/GameFrameX.Config.git ./GameFrameX/Config
git clone https://github.com/GameFrameX/GameFrameX.Protobuf.git ./GameFrameX/Protobuf
git clone https://github.com/GameFrameX/GameFrameX.FairyGUIProject.git ./GameFrameX/FairyGUIProject
git clone https://github.com/GameFrameX/GameFrameX.Tools.git ./GameFrameX/Tools
git clone https://github.com/GameFrameX/GameFrameX.Unity.git ./GameFrameX/Unity
```

> 이 명령들의 의미는 「XX 저장소의 내용을 XX 폴더에 내려받는다」예요. **폴더 이름은 절대 바꾸지 마세요.**

**4단계(로컬 데이터베이스 실행)**: Docker를 설치했다면 두 디렉토리에 각각 들어가서 MongoDB와 PostgreSQL을 띄워요(서버는 MongoDB, 백오피스는 PostgreSQL에 연결):

```shell
cd GameFrameX/docker/mongo && docker compose up -d
cd ../postgres && docker compose up -d
```

실행에 성공하면 이렇게 접속해요:
- MongoDB: `mongodb://admin:admin@localhost:27017`
- PostgreSQL: `localhost:5432`, 계정 `postgres` / 비밀번호 `postgres`, 초기 DB `gameframex`

> ⚠️ 위 계정/비밀번호는 로컬 개발 기본값이에요. `Server` / `Admin`의 연결 설정과 맞춰야 접속돼요.

**5단계(설정 코드 생성)**: `Config/` 디렉토리로 가서 LuBan 내보내기 스크립트를 실행해요. Excel을 클라이언트와 서버 모두에서 쓸 수 있는 코드와 데이터로 바꿔주는 거예요. 구체적인 명령은 👉 [`GameFrameX.Config`](https://github.com/GameFrameX/GameFrameX.Config) 설명을 참고하세요.

**6단계(프로토콜 코드 생성)**: `Protobuf/` 디렉토리로 가서 프로토콜 내보내기 스크립트를 실행해요. 각 엔드에서 메시지를 주고받을 때 쓰는 코드를 만들어주는 거예요. 구체적인 명령은 👉 [`GameFrameX.Protobuf`](https://github.com/GameFrameX/GameFrameX.Protobuf) 설명을 참고하세요.

**7단계(선택)**: 필요하면 `Tools/`를 열어 보조 도구를 컴파일해요. 👉 [`GameFrameX.Tools`](https://github.com/GameFrameX/GameFrameX.Tools) 설명을 참고하세요.

**8단계(드디어 실행!)**: Unity로 `Unity/` 프로젝트를 열고, `Server/`에 있는 서버를 띄우면 한번 돌려볼 수 있어요 🎉

---

# 💬 교류 & 피드백(제안, 요청, 버그)

QQ 그룹: **467608841**

# 📖 문서(진짜 쓰는 중이에요, 재촉 금지 😅)

> 모든 사이트 내용은 같아요. 열리는 아무 곳이나 쓰면 돼요.

- 메인 사이트: https://gameframex.doc.alianblank.com
- 백업 1: https://gameframex-docs.pages.dev
- 백업 2: https://gameframex.doc.cloudflare.alianblank.com
- 백업 3: https://gameframex.doc.vercel.alianblank.com

---

# ☕ 작성자에게 커피 한 잔 사주기

![wechat.jpg](Docs/imgs/wechat.jpg)

# 🎯 누가 GameFrameX를 쓰고 있을까?

| 게임 이름 | 출시 채널 | 출시 시기 |
|:---|:---|:---|
| 심야의 바베큐 가게(深夜的烧烤店) | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |
| 연속 흑백(连续黑白) | 더우인, 콰이쇼우, 알리페이, 홍몽, TapTap, iOS 등 | 2024-11 |

> GameFrameX로 출시작을 만들었나요? PR이나 issue로 위 표에 추가해 주세요. 리스트를 함께 키워가요 🙌

# 👥 기여자 명단

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

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=GameFrameX/GameFrameX,GameFrameX/GameFrameX.Unity,GameFrameX/GameFrameX.Server,GameFrameX/GameFrameX.Admin&type=Date)](https://star-history.com/embed?secret=Z2hwX0l1VlJVYlE0RUhIZE9hS2pVZ21ISVozNFNNSUdETDMycmZEWQ==#GameFrameX/GameFrameX&GameFrameX/GameFrameX.Unity&GameFrameX/GameFrameX.Server&GameFrameX/GameFrameX.Admin&Date)

# 📜 면책 조항

모든 플러그인은 인터넷에서 가져온 것으로, 사용하실 때는 직접 결제하세요. 저작권 침해가 있다면 email을 보내주시면 제거하겠습니다. 감사합니다.

이 프로젝트는 해당 지역 법률이 허용하지 않는 범위에서 사용해서는 안 됩니다. 기술 자체에는 죄가 없고, 잘못은 기술을 남용하는 사람에게 있어요.

# 💎 스폰서

[AITKPARTY](https://aitkparty.com/)는 AI 대형 모델 API 중계·통합 서비스예요. 오픈소스 프로젝트 New API 기반으로 구축되었고, 통합 인터페이스를 통해 개발자가 주요 대형 언어 모델에 편리하게 접근할 수 있도록 해줘요. 다수 모델 공급자를 각각 연동하는 수고를 덜어주는 거죠.
