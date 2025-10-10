# 🎨 HMG 메시지 비주얼라이저

Unity 기반의 실시간 메시지 표시 시스템입니다. JSON 파일에서 메시지를 읽어와 화면에 흐르는 텍스트로 표시합니다.

[![Unity Version](https://img.shields.io/badge/Unity-2022.3.62f1-blue.svg)](https://unity.com/)
[![URP](https://img.shields.io/badge/URP-17.2.0-green.svg)](https://unity.com/urp)
[![TextMeshPro](https://img.shields.io/badge/TextMeshPro-3.0.7-orange.svg)](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest)

**📅 마지막 업데이트**: 2025-03-06 (JSON 구조 v2.0)

---

## 📋 목차

- [프로젝트 개요](#-프로젝트-개요)
- [빠른 시작](#-빠른-시작)
- [프로젝트 구조](#-프로젝트-구조)
- [핵심 시스템](#-핵심-시스템)
- [개발 가이드](#-개발-가이드)
- [트러블슈팅](#-트러블슈팅)
- [라이선스](#-라이선스)

---

## 🎯 프로젝트 개요

### 기능
- ✅ JSON 파일 기반 메시지 관리
- ✅ 실시간 메시지 자동 생성 및 이동
- ✅ 2가지 디자인 타입 (A-Type, B-Type)
- ✅ TextMeshPro 기반 고품질 텍스트 렌더링
- ✅ 이모지 지원 (Simple Emoji for TextMesh Pro)
- ✅ Inspector 기반 디버그 로그 제어

### 기술 스택
- **Unity**: 2022.3.62f1 LTS
- **렌더 파이프라인**: Universal Render Pipeline (URP) 17.2.0
- **텍스트 렌더링**: TextMeshPro 3.0.7
- **이모지 지원**: Simple Emoji for TextMesh Pro
- **데이터 포맷**: JSON

---

## ⚡ 빠른 시작

### 1. 프로젝트 열기
```bash
1. Unity Hub 실행
2. Unity 2022.3.62f1 LTS 설치
3. "Open" → 프로젝트 폴더 선택
```

### 2. 메인 씬 실행
```bash
1. Assets/v0623_Final.unity 씬 열기
2. Play 버튼 (▶️) 클릭
3. 자동으로 메시지가 생성되고 화면을 흐름
```

### 3. 메시지 수정
```bash
1. Assets/StreamingAssets/messages.json 편집
2. 게임 실행 중이면 20초마다 자동 갱신
3. 또는 게임 재시작
```

---

## 📁 프로젝트 구조

```
HMG-messagw-visualizer/
├── Assets/
│   ├── Scripts/                      # 🔧 C# 스크립트
│   │   ├── JsonLoader.cs            # JSON 데이터 로딩 및 메시지 관리
│   │   ├── TextController.cs        # 텍스트 표시 및 디자인 타입 제어
│   │   ├── UIMovementController.cs  # UI 생성 및 이동 제어
│   │   └── TextMessageManager.cs    # 메시지 매니저 (레거시)
│   │
│   ├── StreamingAssets/              # 📦 런타임 데이터
│   │   └── messages.json            # 메시지 데이터 (외부 편집 가능)
│   │
│   ├── SimpleEmojiTMP/               # 😀 이모지 에셋
│   │   └── EmojiTMP.asset           # Simple Emoji 스프라이트 에셋
│   │
│   ├── Image/                        # 🖼️ UI 이미지 리소스
│   ├── HMG_TV_Design/               # 🎨 폰트 및 디자인
│   ├── Scenes/                       # 🎬 Unity 씬
│   │   └── v0623_Final.unity        # 메인 씬
│   └── Settings/                     # ⚙️ URP 렌더 설정
│
├── ProjectSettings/                  # Unity 프로젝트 설정
├── Packages/                         # Unity 패키지 의존성
├── README.md                         # 📖 이 문서
└── Project_Status.md                # 📝 개발 히스토리 (상세)
```

---

## 🔧 핵심 시스템

### 1. 설정 시스템 (`config.json`) 🆕

**역할**: 모든 Inspector 설정을 외부 파일로 관리

#### Config 파일 구조
```json
{
  "version": "1.0",
  "lastModified": "2025-03-06",
  "jsonLoader": {
    "jsonFileName": "messages.json",
    "messagesToLoadCount": 10,
    "autoSpawnEnabled": true,
    "spawnInterval": 10.0,
    "autoRefresh": true,
    "refreshInterval": 20.0,
    "showDebugLogs": false
  },
  "uiMovement": {
    "moveSpeed": 280.0,
    "moveDirection": { "x": 1.0, "y": 0.0 },
    "destroyTime": 35.0,
    "designOrderMode": "Sequential"
  },
  "textController": {
    "showDebugLogs": false
  }
}
```

#### 장점
- ✅ **외부 파일 관리**: Unity를 열지 않고도 설정 변경 가능
- ✅ **자동 적용**: 게임 시작 시 자동으로 Inspector 값에 적용
- ✅ **버전 관리**: Config 파일도 Git으로 관리 가능
- ✅ **빌드 후 수정**: 빌드된 게임의 설정도 변경 가능

#### 사용 방법
1. `Assets/StreamingAssets/config.json` 편집
2. Unity 게임 실행 → 자동으로 설정 적용
3. Context Menu → "Config 파일 테스트"로 검증

---

### 2. 메시지 관리 시스템 (`JsonLoader.cs`)

**역할**: JSON 파일에서 메시지를 로드하고 UIMovementController에 전달

#### 주요 기능
```csharp
// 자동 갱신: 20초마다 JSON 파일 리로드
public float refreshInterval = 20.0f;

// 자동 생성: 10초마다 5개 메시지 순차 생성
public float spawnInterval = 10.0f;
```

#### Inspector 설정
| 필드 | 설명 | 기본값 |
|------|------|--------|
| `Json File Name` | 읽어올 JSON 파일명 | `messages.json` |
| `Messages To Load Count` | 로드할 메시지 개수 | `10` |
| `Auto Refresh` | 자동 갱신 활성화 | `✅` |
| `Refresh Interval` | 갱신 주기 (초) | `20` |
| `Auto Spawn Enabled` | 자동 생성 활성화 | `✅` |
| `Spawn Interval` | 생성 주기 (초) | `10` |
| `Show Debug Logs` | 디버그 로그 표시 | `☐` |

#### JSON 데이터 형식 (v2.0 - 2025-10-03 업데이트)

**새로운 구조 (QR Message Wall CMS 호환):**
```json
{
  "metadata": {
    "exportedAt": "2025-10-03T18:05:26.448034",
    "totalCount": 50,
    "source": "QR Message Wall CMS",
    "version": "1.0"
  },
  "messages": [
    {
      "id": "msg_1759480908302_6oqcw4qy3",
      "author": "이인정",                    // 기존 name → author
      "content": "오빠언제와",              // 기존 formatted_message → content
      "timestamp": "2025-10-03 08:41",
      "status": "active",
      "language": "ko",
      "created_at": "2025-10-03 08:41:48",
      "updated_at": "2025-10-03 08:41:48"
    },
    {
      "id": "msg_1758028410129_u1xpbbixa",
      "author": "익명의 사용자",
      "content": "최고최고 🍎🍎🍎🍎",
      "timestamp": "2025-09-16 13:13",
      "status": "active",
      "language": "ko",
      "created_at": "2025-09-16 13:13:30",
      "updated_at": "2025-09-16 13:13:30"
    }
  ]
}
```

**레거시 구조 (하위 호환):**
```json
[
  {
    "id": "2025-06-22T08:00:00.000000",
    "name": "서수빈",
    "story": "코드리뷰 고마워요",
    "timestamp": "2025-06-22T08:00:00.000000",
    "formatted_message": "코드리뷰 고마워요 💖✨ 최고! 🙌"
  }
]
```

**Note**: 
- **새 구조**: `author`와 `content` 필드 사용
- **레거시**: `name`과 `formatted_message` 필드 사용
- 두 구조 모두 지원되며 자동 감지됩니다

---

### 2. UI 이동 시스템 (`UIMovementController.cs`)

**역할**: UI 프리팹을 생성하고 화면을 가로질러 이동시킴

#### 주요 기능
```csharp
// 이동 설정
public float moveSpeed = 280f;           // 픽셀/초
public Vector2 moveDirection = Vector2.right;  // 오른쪽
public float destroyTime = 35f;          // 35초 후 제거

// 디자인 순서
public DesignOrderMode designOrderMode = Sequential;  // AType → BType
```

#### Inspector 설정
| 필드 | 설명 | 기본값 |
|------|------|--------|
| `Design Order Mode` | Sequential 또는 Random | `Sequential` |
| `Move Speed` | 이동 속도 (픽셀/초) | `280` |
| `Move Direction` | 이동 방향 벡터 | `(1, 0)` |
| `Destroy Time` | 제거 시간 (초) | `35` |
| `UI Prefab` | 생성할 프리팹 | `MESSAGE_PREFAB` |
| `Show Debug Logs` | 디버그 로그 표시 | `☐` |

#### 디자인 순서 모드
- **Sequential**: A → B → A → B... 순서대로
- **Random**: A, B 무작위 선택

---

### 3. 텍스트 제어 시스템 (`TextController.cs`)

**역할**: 텍스트 내용 설정 및 디자인 타입 전환

#### 디자인 타입
```csharp
public enum DesignType
{
    AType,  // 첫 번째 디자인
    BType   // 두 번째 디자인
}
```

#### Inspector 설정
| 필드 | 설명 |
|------|------|
| `Korean Font` | 한글 폰트 에셋 |
| `Emoji Sprite Asset` | Simple Emoji 스프라이트 에셋 |
| `Show Debug Logs` | 디버그 로그 표시 |

#### 프리팹 구조
```
MESSAGE_PREFAB
├── TextController (컴포넌트)
├── max20Objecta (AType 디자인)
│   ├── Name Text (TextMeshProUGUI)
│   └── Content Text (TextMeshProUGUI)
└── max20Objectb (BType 디자인)
    ├── Name Text (TextMeshProUGUI)
    └── Content Text (TextMeshProUGUI)
```

---

### 4. 이모지 지원 시스템

**기술**: Simple Emoji for TextMesh Pro

#### 작동 원리
```
JSON 파일 → ConvertMultiToSingleCodepoint()
            ↓
         미지원 이모지 제거 (VS16, ZWJ, 국기, 스킨톤)
            ↓
         지원 가능한 이모지로 변환
            ↓
         TextMeshPro + EmojiTMP.asset
            ↓
         화면에 표시 ✅
```

#### 지원되는 이모지
- ✅ 단일 코드포인트: 😀😊👍💖🙌💪🚀🔥✨⭐🎉
- ✅ 변환된 이모지: ❤️ → 💖
- ❌ 미지원 (자동 제거): 국기(🇰🇷), ZWJ 조합(👨‍💻), 스킨톤(👍🏽)

#### 설정 방법
```
1. Hierarchy → TextController 선택
2. Inspector → Emoji Sprite Asset
3. Assets/SimpleEmojiTMP/EmojiTMP 드래그
```

---

## 💻 개발 가이드

### 새로운 메시지 추가

#### 방법 1: JSON 파일 편집 (권장)
```json
// Assets/StreamingAssets/messages.json
{
  "id": "2025-10-02T10:00:00.000000",
  "name": "새로운 이름",
  "story": "짧은 내용",
  "timestamp": "2025-10-02T10:00:00.000000",
  "formatted_message": "실제 표시될 메시지 💖✨"
}
```

#### 방법 2: 런타임 할당
```csharp
// UIMovementController에 메시지 직접 할당
uiMovementController.assignedMessages.Add(new MessageInfo 
{
    name = "이름",
    content = "내용"
});
uiMovementController.useAssignedMessages = true;
```

---

### 새로운 디자인 타입 추가

#### 1. DesignType Enum 확장
```csharp
// Assets/Scripts/TextController.cs
public enum DesignType
{
    AType,
    BType,
    CType  // 새로 추가
}
```

#### 2. TextController에 오브젝트 추가
```csharp
[Header("모드별 게임오브젝트")]
public GameObject max20Objecta;  // AType
public GameObject max20Objectb;  // BType
public GameObject max20Objectc;  // CType (새로 추가)
```

#### 3. ActivateObject() 수정
```csharp
private void ActivateObject()
{
    // 모두 비활성화
    if (max20Objecta != null) max20Objecta.SetActive(false);
    if (max20Objectb != null) max20Objectb.SetActive(false);
    if (max20Objectc != null) max20Objectc.SetActive(false);
    
    // 현재 디자인만 활성화
    switch (currentDesignType)
    {
        case DesignType.AType:
            if (max20Objecta != null) max20Objecta.SetActive(true);
            break;
        case DesignType.BType:
            if (max20Objectb != null) max20Objectb.SetActive(true);
            break;
        case DesignType.CType:
            if (max20Objectc != null) max20Objectc.SetActive(true);
            break;
    }
}
```

#### 4. UIMovementController 시퀀스 업데이트
```csharp
// Assets/Scripts/UIMovementController.cs
private DesignType[] designSequence = { 
    DesignType.AType, 
    DesignType.BType,
    DesignType.CType  // 새로 추가
};
```

---

### 이동 속도/방향 변경

#### Inspector에서 변경 (런타임 중 가능)
```
UIMovementController:
├─ Move Speed: 280 → 500 (더 빠르게)
├─ Move Direction: (1, 0) → (1, 0.2) (오른쪽 + 위로)
└─ Destroy Time: 35 → 60 (더 오래 표시)
```

#### 코드에서 변경
```csharp
// 특정 오브젝트의 속도만 변경
UIMover mover = uiObject.GetComponent<UIMover>();
mover.UpdateSettings(
    newSpeed: 500f,
    newDirection: new Vector2(1, 0.2f),
    newDestroyTime: 60f
);
```

---

### 폰트 변경

#### 1. TextMeshPro 폰트 에셋 생성
```
1. Window → TextMeshPro → Font Asset Creator
2. Source Font File: 원하는 TTF/OTF 폰트 선택
3. Character Set: Unicode Range (한글)
4. Unicode Range: Add Range → AC00-D7AF (한글 완성형)
5. Generate Font Atlas
6. Save
```

#### 2. TextController에 할당
```
Hierarchy → TextController 선택
Inspector → Korean Font → 새로운 폰트 에셋 드래그
```

---

### 디버그 로그 활성화

#### 전체 시스템 디버깅
```
JsonLoader → Show Debug Logs: ✅
UIMovementController → Show Debug Logs: ✅
TextController → Show Debug Logs: ✅
```

#### 출력 예시
```
[JsonLoader] Start() 호출됨. 자동 갱신: True, 자동 생성: True
[UIMovementController] 설정 변경됨 - 제거시간: 35초, 이동속도: 280
[TextController] 이모지 스프라이트 'EmojiTMP' 설정 완료
```

---

## 🐛 트러블슈팅

### 문제: 메시지가 표시되지 않음

#### 원인 1: JSON 파일 경로 오류
```
해결: JsonLoader → Json File Name = "messages.json" 확인
```

#### 원인 2: UI 프리팹 미할당
```
해결: UIMovementController → UI Prefab에 MESSAGE_PREFAB 할당
```

#### 원인 3: TextMeshPro 컴포넌트 없음
```
해결: Prefab의 Text 컴포넌트를 TextMeshProUGUI로 교체
```

---

### 문제: 이모지가 네모 박스(□)로 표시됨

#### 원인 1: Emoji Sprite Asset 미할당
```
해결:
1. TextController 선택
2. Emoji Sprite Asset → Assets/SimpleEmojiTMP/EmojiTMP 할당
```

#### 원인 2: Simple Emoji가 지원하지 않는 이모지
```
해결: 자동으로 제거됩니다 (VS16, ZWJ, 국기, 스킨톤 등)
확인: JsonLoader → Context Menu → "이모지 변환 테스트"
```

---

### 문제: 메시지가 너무 빠르게/느리게 움직임

```
해결:
1. UIMovementController 선택
2. Move Speed 조절:
   - 기본: 280
   - 빠르게: 400~600
   - 느리게: 100~200
```

---

### 문제: 메시지가 너무 빨리/늦게 사라짐

```
해결:
1. UIMovementController 선택
2. Destroy Time 조절:
   - 기본: 35초
   - 오래 표시: 60~120초
   - 빨리 제거: 10~20초
```

---

### 문제: Console 창에 로그가 너무 많음

```
해결:
모든 스크립트의 Show Debug Logs 체크 해제:
├─ JsonLoader.showDebugLogs: ☐
├─ UIMovementController.showDebugLogs: ☐
└─ TextController.showDebugLogs: ☐
```

---

## 🔍 Context Menu 기능

### JsonLoader
```
우클릭 → "Config 파일 테스트"                // Config 시스템 (신규)
우클릭 → "StreamingAssets에서 JSON 파일 읽기"
우클릭 → "즉시 5개 순차 생성"
우클릭 → "새로운 JSON 구조 테스트"          // v2.0
우클릭 → "이모지 변환 테스트"
우클릭 → "로드된 메시지의 이모지 코드포인트 분석"
```

### UIMovementController
```
우클릭 → "현재 설정 확인"
우클릭 → "디자인 인덱스 리셋"
우클릭 → "할당된 메시지 정보 확인"
우클릭 → "Sequential 모드 5개 테스트"
```

### TextController
```
우클릭 → "TextMeshPro 컴포넌트 상태 확인"
우클릭 → "JsonLoader 메시지 이모지 확인"
우클릭 → "이모지 지원 테스트"
```

---

## 📊 성능 최적화 팁

### 1. 메시지 생성 빈도 조절
```csharp
// 부하를 줄이려면:
JsonLoader.spawnInterval = 15f;  // 10초 → 15초
```

### 2. 로드하는 메시지 개수 제한
```csharp
// 메모리 사용량 감소:
JsonLoader.messagesToLoadCount = 5;  // 10개 → 5개
```

### 3. 제거 시간 단축
```csharp
// 화면의 오브젝트 수 감소:
UIMovementController.destroyTime = 20f;  // 35초 → 20초
```

---

## 📝 개발 히스토리

자세한 개발 히스토리는 [Project_Status.md](Project_Status.md)를 참고하세요.

### 최근 주요 업데이트

#### ✅ JSON 구조 v2.0 업데이트 (2025-03-06)
- **QR Message Wall CMS 호환 구조** 지원
- 새로운 필드명: `name` → `author`, `formatted_message` → `content`
- 메타데이터 정보 활용 (총 메시지 수, 소스, 버전 등)
- 하위 호환성 유지 (기존 JSON 구조도 지원)
- `JsonRootData`, `JsonMetadata` 클래스 추가
- `TestNewJsonStructure()` 테스트 함수 추가

#### ✅ 디버그 로그 제어 시스템 (2025-10-02)
- 모든 스크립트에 `showDebugLogs` 플래그 추가
- Inspector에서 ON/OFF 제어 가능
- 태그 추가: `[스크립트명]`

#### ✅ 이모지 지원 시스템 (2025-10-02)
- Simple Emoji for TextMesh Pro 통합
- 미지원 이모지 자동 제거 (네모 박스 방지)
- `RemoveUnsupportedEmojis()` 함수 구현

#### ✅ 텍스트 시스템 간소화 (2025-10-02)
- Max20 모드 고정
- 2가지 디자인 타입 (AType, BType)
- TextMeshPro 완전 전환

---

## 🤝 기여 가이드

### 브랜치 전략
```
master         - 프로덕션 릴리즈
develop        - 개발 통합
feature/*      - 새로운 기능
bugfix/*       - 버그 수정
```

### 커밋 메시지 규칙
```
feat: 새로운 기능 추가
fix: 버그 수정
docs: 문서 수정
style: 코드 포맷팅
refactor: 코드 리팩토링
test: 테스트 추가
chore: 빌드/설정 변경
```

---

## 📦 빌드 가이드

### Windows 빌드
```
1. File → Build Settings
2. Platform: Windows
3. Architecture: x86_64
4. Build
```

### Android 빌드
```
1. File → Build Settings
2. Platform: Android
3. Minimum API Level: Android 7.0 (API 24)
4. Build
```

### WebGL 빌드
```
1. File → Build Settings
2. Platform: WebGL
3. Compression Format: Brotli
4. Build
```

---

## 🎓 학습 리소스

### Unity 공식 문서
- [TextMeshPro 가이드](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest)
- [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [JSON Serialization](https://docs.unity3d.com/Manual/JSONSerialization.html)

### 관련 에셋
- [Simple Emoji for TextMesh Pro](https://assetstore.unity.com/packages/tools/gui/simple-emoji-for-textmesh-pro-227992)

---

## 📄 라이선스

이 프로젝트는 내부 사용을 위한 것입니다.

---

## 👥 팀

**프로젝트명**: HMG 메시지 비주얼라이저  
**Unity 버전**: 2022.3.62f1 LTS  
**마지막 업데이트**: 2025-03-06 (JSON 구조 v2.0)

---

## 🆘 지원

문제가 발생하거나 질문이 있으시면:
1. [Project_Status.md](Project_Status.md) 확인
2. Context Menu 기능으로 디버깅
3. Show Debug Logs 활성화하여 로그 확인

---

**Happy Coding! 🚀**


