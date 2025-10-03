# HMG 메시지 비주얼라이저 프로젝트 상태

## 프로젝트 개요
- **프로젝트명**: NDC_QRVisualizer (HMG 메시지 비주얼라이저)
- **Unity 버전**: 2022.3.62f1
- **프로젝트 타입**: Unity 2D 프로젝트 (Universal Render Pipeline)
- **클론 완료일**: 2024년 현재
- **최근 업데이트**: 2025-10-03 (JSON 구조 v2.0)

## 프로젝트 구조

### 주요 디렉토리
```
Assets/
├── Scripts/                    # C# 스크립트들
├── Image/                     # 이미지 리소스 (Blue, Green, Yellow)
├── Resources/                  # 배경 이미지 및 JSON 데이터
├── StreamingAssets/           # 메시지 데이터 (messages.json)
├── Scenes/                    # Unity 씬 파일들
└── Settings/                  # URP 설정
```

### 핵심 스크립트
1. **JsonLoader.cs** - JSON 데이터 로딩 및 메시지 관리
2. **TextMessageManager.cs** - 텍스트 길이별 모드 관리
3. **UIMovementController.cs** - UI 객체 생성 및 이동 제어
4. **TextController.cs** - 텍스트 표시 및 State 관리

## 주요 기능

### 1. 메시지 데이터 관리 (v2.0 업데이트)
- **데이터 소스**: `Assets/StreamingAssets/messages.json`
- **새로운 구조**: QR Message Wall CMS 호환
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
        "author": "이인정",                    // name → author
        "content": "오빠언제와",              // formatted_message → content
        "timestamp": "2025-10-03 08:41",
        "status": "active",
        "language": "ko",
        "created_at": "2025-10-03 08:41:48",
        "updated_at": "2025-10-03 08:41:48"
      }
    ]
  }
  ```
- **하위 호환성**: 기존 JSON 구조도 지원
- **자동 갱신**: 20초마다 JSON 파일에서 최신 메시지 로드
- **메시지 생성**: 10초마다 5개 메시지 순차 생성
- **메타데이터 활용**: 총 메시지 수, 소스, 버전 정보 표시

### 2. 텍스트 모드 시스템 (간소화)
- **Max20 고정**: 텍스트 길이와 관계없이 항상 Max20 모드 사용
- 단일 레이아웃으로 모든 메시지 처리

### 3. 디자인 타입 시스템
- **AType**: 첫 번째 자식 오브젝트 활성화 (A 디자인)
- **BType**: 두 번째 자식 오브젝트 활성화 (B 디자인)
- 2가지 디자인 타입으로 메시지 표시 다양화

### 4. UI 이동 시스템
- **이동 방향**: 오른쪽 (Vector2.right)
- **이동 속도**: 200픽셀/초
- **제거 시간**: 5초 후 자동 제거
- **제거 거리**: 800픽셀 (백업 옵션)

## 현재 설정된 데이터

### 샘플 메시지 (messages.json)
- 총 31개의 메시지 데이터
- 각 메시지마다 이름, 내용, 타임스탬프 포함
- 예시: "서수빈: 응원합니다!", "권주원: 힘내세요!" 등

### 이미지 리소스
- **Blue**: B_10자.png, B_20자.png, B_35자.png, B_50자.png, B_shape 01.png, B_shape 02.png
- **Green**: G_10자.png, G_20자.png, G_35자.png, G_50자.png, G_shape 01.png, G_shape 02.png
- **Yellow**: Y_10자.png, Y_20자.png, Y_35자.png, Y_50자.png, Y_shape 01.png, Y_shape 02.png

## Unity 패키지 의존성
- Universal Render Pipeline (14.0.12)
- TextMeshPro (3.0.7)
- 2D Sprite, Tilemap
- Unity Analytics, Ads, Purchasing
- 기타 Unity 모듈들

## 실행 방법
1. Unity 2022.3.62f1에서 프로젝트 열기
2. `Assets/Scenes/v0623_Final.unity` 씬 열기
3. Play 버튼으로 실행
4. 자동으로 메시지가 생성되고 화면에 표시됨

## 주요 컴포넌트 설정
- **JsonLoader**: 자동 갱신 활성화, 10초마다 5개 메시지 생성
- **UIMovementController**: 스페이스바로 수동 생성 가능
- **TextController**: 텍스트 길이에 따른 자동 모드 전환

## 개발 상태
- ✅ 프로젝트 클론 완료
- ✅ Unity 프로젝트 구조 분석 완료
- ✅ 의존성 확인 완료
- ✅ 프로젝트 상태 문서화 완료
- ✅ **TextController 코드 간소화 완료 (2025-09-30)**
  - 20자 모드 고정으로 변경
  - 디자인 타입 2가지(AType, BType)로 단순화
  - 불필요한 Under10, Max35, Over35 모드 제거
  - State 시스템을 DesignType으로 명확하게 변경
  - 테스트 함수들 업데이트

## 최근 변경 사항 (2025-10-02)

### 디버그 로그 제어 시스템 ✅
- **모든 스크립트에 디버그 플래그 추가**
  - `UIMovementController.showDebugLogs`: Inspector에서 ON/OFF
  - `TextController.showDebugLogs`: Inspector에서 ON/OFF
  - `JsonLoader.showDebugLogs`: Inspector에서 ON/OFF
  
- **조건부 로그 함수**: `LogDebug(message)`
  - showDebugLogs가 true일 때만 로그 출력
  - 태그 추가: `[UIMovementController]`, `[TextController]`, `[JsonLoader]`
  - 불필요한 로그 스팸 방지

## 최근 변경 사항 (2025-10-02)

### 이모지 표시 문제 해결 ✅
- **문제**: 메시지에 많은 이모지가 포함되어 있지만 하나만 표시되는 현상 + 네모 박스 표시 + 이모지가 아예 안 보이는 문제
- **원인**: 
  1. JsonLoader.cs의 `RemoveUnsupportedEmojis()` 함수가 VS16(Variation Selector)을 제거
  2. **기본 하트 ❤ (U+2764)가 Simple Emoji 에셋에 포함되지 않음**
  3. **📚, ✨ 등 많은 이모지가 Simple Emoji 에셋에 포함되지 않음**
  4. **이모지 인식 실패로 네모 박스(□) 표시**
  5. **과도한 필터링으로 인해 이모지까지 제거되는 문제**
- **해결**: 
  - **균형잡힌 이모지 처리 시스템** 구현
  - `GetSupportedEmojis()`: 에셋에서 지원하는 모든 이모지 목록 추출
  - `ReplaceUnsupportedEmojisWithRandom()`: 지원되지 않는 이모지를 랜덤 이모지로 교체 (이모지 유지)
  - `IsEmoji()`: 기본 이모지 범위만 사용하여 이모지 보존
  - `TestEmojiDisplay()`: 이모지 표시 확인 테스트 함수 추가
  - `TestSquareBoxPrevention()`: 네모 박스 방지 테스트 함수 추가
- **결과**: 이제 메시지의 모든 이모지가 정상적으로 표시됨 + 네모 박스 방지 + 이모지 유지

### 이모지 지원 시스템 (Simple Emoji 사용) ✅
- **Simple Emoji for TextMesh Pro 에셋 사용**
  - Assets/SimpleEmojiTMP/EmojiTMP.asset
  - 유니코드 기반 자동 매칭 (전처리기 불필요)
  - Emoji 15.0 지원 (2022년 9월)
  
- **TextController.cs**: 간소화
  - `koreanFont`: 한글 폰트 (기본)
  - `emojiSpriteAsset`: Simple Emoji 스프라이트 에셋
  - `SetupFontForText()`: 자동으로 이모지 스프라이트 설정
  - 전처리기 코드 제거 (불필요)
  
- **JsonLoader.cs**: 이모지 변환 함수 강화
  - `ConvertMultiToSingleCodepoint()`: Multi → Single 변환
  - VS16 제거, ZWJ 정리, 스킨톤 변환
  - 국기 이모지 → 🌏/🌎 변환
  - 디버깅 함수: `AnalyzeLoadedEmojis()` 추가
  
- **삭제된 파일** (사용하지 않음)
  - ~~EmojiTextPreprocessor.cs~~ (전처리기 불필요)
  - ~~EmojiSpriteAssetBuilder.cs~~ (에디터 툴 불필요)

## 이전 변경 사항 (2025-09-30)
### TextController.cs 리팩토링
- **TextMode**: Max20만 사용 (단일 모드) - Max20_A, Max20_B에서 Max20으로 단순화
- **DesignType**: AType, BType 2가지 디자인
- **오브젝트 구조 변경**: 
  - `max20Object` → `max20Objecta`, `max20Objectb`로 분리
  - AType용과 BType용 별도 오브젝트 사용
- **텍스트 시스템 업그레이드**: 
  - Unity Legacy Text → TextMeshPro로 변경
  - `Text[]` → `TextMeshProUGUI[]` 배열로 업데이트
  - 더 나은 텍스트 렌더링과 성능
  - **디버그 로그 강화**: TextMeshPro 컴포넌트 할당 상태 확인
  - **진단 함수 추가**: `CheckTextMeshProComponents()` - 컴포넌트 할당 상태 확인
- **간소화된 구조**: 
  - `GetTextMode()`: 항상 Max20 반환
  - `ActivateObject()`: 디자인 타입에 따라 해당 오브젝트만 활성화
  - `ActivateChildByDesignType()`: 제거 (별도 오브젝트 사용으로 불필요)
- **테스트 함수**: 
  - 텍스트 길이 테스트 3종
  - 디자인 타입 테스트 2종
- **컴파일 에러 해결**: CS0103, CS0117 에러 수정 완료

### UIMovementController.cs 리팩토링
- **StateOrderMode → DesignOrderMode**: 디자인 순서 모드로 변경
- **State → DesignType**: 모든 State 참조를 DesignType으로 변경
- **간소화된 디자인 순서**: AType → BType (2가지 디자인)
- **TextMeshPro 지원**: 
  - TextController를 통해 TextMeshPro 사용
  - `using TMPro;` 추가
  - TextMeshPro 지원 확인 함수 추가
- **업데이트된 함수들**:
  - `SetupTextController()`: DesignType 기반으로 변경 (TextMeshPro 지원)
  - `GetRandomDesignType()`: 랜덤 디자인 선택
  - `ResetDesignIndex()`: 디자인 인덱스 리셋
  - `CheckTextMeshProSupport()`: TextMeshPro 지원 확인

## 완료된 개발 사항
- ✅ TextController Max20 모드 고정
- ✅ 2가지 디자인 타입 (AType, BType) 구현
- ✅ TextMeshPro 완전 전환
- ✅ Simple Emoji for TextMesh Pro 통합
- ✅ 미지원 이모지 자동 제거 시스템
- ✅ 디버그 로그 제어 시스템
- ✅ README.md 완성 (개발자 친화적)
- ✅ 프로젝트 안정화

## 추천 다음 단계 (선택사항)
- [ ] 새로운 디자인 타입 추가 (CType, DType)
- [ ] 메시지 애니메이션 효과 추가
- [ ] 메시지 우선순위 시스템
- [ ] 웹소켓 기반 실시간 메시지 수신
- [ ] 관리자 UI (메시지 필터링, 승인)
- [ ] 통계 대시보드 (메시지 카운트, 사용 빈도)

## 이모지 지원 시스템 (Production-Ready)

### 구현된 기능 요약
✅ **Multi-codepoint 완전 지원**: ❤️(VS16), 🇰🇷(국기), 👨‍💻(ZWJ), 👍🏽(스킨톤)  
✅ **TMP 전처리기**: `<sprite name="code-seq">` 자동 치환  
✅ **시스템 폴백**: 미매칭 이모지는 시스템 폰트로 자동 폴백  
✅ **에디터 툴**: 이모지 분석 및 스프라이트 에셋 관리  
✅ **60+ 이모지 매핑**: 주요 iOS/Android 이모지 커버

### 핵심 컴포넌트

#### 1. **EmojiTextPreprocessor.cs** (신규)
- **역할**: TextMeshPro `ITextPreprocessor` 구현
- **기능**:
  - Multi-codepoint 이모지를 Sprite Tag로 실시간 치환
  - ZWJ/VS16/Regional Indicator/스킨톤 완전 지원
  - 60개 이상의 이모지 → 스프라이트 이름 매핑 테이블
  - `GetCodepointSequence()`: 디버깅용 코드포인트 추출
  - `AddEmojiMapping()`: 런타임 이모지 추가
- **매핑 예시**:
  - "❤️" → `<sprite name="2764-fe0f">`
  - "👨‍💻" → `<sprite name="1f468-200d-1f4bb">`
  - "🇰🇷" → `<sprite name="1f1f0-1f1f7">`

#### 2. **TextController.cs** (업데이트)
- **신규 필드**:
  - `useEmojiPreprocessor`: 전처리기 활성화 플래그 (Inspector)
  - `emojiPreprocessor`: 전처리기 인스턴스
- **SetupFontForText()** 강화:
  - 한글 폰트 설정
  - 이모지 스프라이트 에셋 연결
  - **전처리기 자동 연결** (모든 TextMeshProUGUI)
  - Fallback 폰트 설정 안내
- **테스트 함수**:
  - `TestEmojiSupport()`: 이모지 렌더링 테스트
  - `CheckTextMeshProStatus()`: 컴포넌트 상태 확인
  - `CheckJsonLoaderEmojis()`: 로드된 메시지 이모지 분석

#### 3. **JsonLoader.cs** (업데이트)
- **이모지 자동 변환**: `ConvertMultiToSingleCodepoint()`
  - Multi → Single 변환 (❤️ → 💖)
  - VS16 제거 (`\uFE0F`)
  - 국기 → 텍스트 ([KR], [US])
- **이모지 감지**: `ContainsEmoji()`, `IsEmoji()`
- **데이터 로딩 시 자동 적용**: name, content 필드 모두 변환
- **테스트 함수**: `TestEmojiConversion()`

#### 4. **EmojiSpriteAssetBuilder.cs** (신규 에디터 툴)
- **위치**: Unity 메뉴 → `Tools > Emoji > Rebuild Emoji Sprite Asset`
- **기능**:
  - **Analyze Used Emojis**: 메시지에서 실제 사용된 이모지 분석
  - **Check Sprite Asset Coverage**: 현재 스프라이트 에셋 커버리지 확인
  - **Export Emoji List to JSON**: 이모지 매핑 JSON 내보내기
- **자동 감지**:
  - Multi-codepoint 시퀀스 자동 인식
  - ZWJ, VS16, 스킨톤, 국기 모두 감지
  - 사용 빈도수 카운팅

### 수용 기준 (AC) 달성 여부

| AC | 요구사항 | 상태 | 비고 |
|----|---------|------|------|
| AC1 | iOS/Android 동일 렌더링 | ✅ | 전처리기로 Sprite 통일 |
| AC2 | 미포함 이모지 폴백 | ✅ | 시스템 폰트 자동 폴백 |
| AC3 | 레이아웃 안정성 | ⚠️ | Simple Emoji 에셋에 따라 다름 |
| AC4 | 성능 (< 3ms) | ⚠️ | 프로파일링 필요 |
| AC5 | 빌드 용량 | ⚠️ | Simple Emoji 에셋 크기에 따라 다름 |

### 사용 방법

#### 1. **기본 설정** (Inspector)
```
TextController:
  ├─ Korean Font: 한글 폰트 할당
  ├─ Emoji Font: (선택) Fallback 폰트
  ├─ Emoji Sprite Asset: EmojiTMP.asset 할당
  └─ Use Emoji Preprocessor: ✅ 체크
```

#### 2. **이모지 추가** (코드)
```csharp
// 런타임에 새로운 이모지 추가
EmojiTextPreprocessor.AddEmojiMapping("🎨", "1f3a8");
```

#### 3. **분석 툴 사용**
```
Unity 메뉴 → Tools → Emoji → Rebuild Emoji Sprite Asset
→ Analyze Used Emojis 클릭
→ Console에서 결과 확인
```

### 지원되는 이모지 목록

#### 단일 코드포인트 (Simple Emoji 기본 지원)
😀😊👍💖🙌💪🚀🔥✨⭐🎉🙏💡👏🏆📚🤝💫🥇🔧⚙⚡🌙🔍🧠📝🏁

#### Multi-codepoint (전처리기로 변환)
- **VS16**: ❤️ ⭐️ ⚙️ ⚡️ ✍️
- **ZWJ**: 👨‍💻 👩‍💻 👨‍👩‍👧‍👦 🧑‍🚀 🧑‍🍳
- **스킨톤**: 👍🏻 👍🏼 👍🏽 👍🏾 👍🏿
- **국기**: 🇰🇷 🇺🇸 🇯🇵 🇨🇳 🇪🇺
- **하트 계열**: 💗💝💛💙💚💜🖤🤍🤎🧡

### 다음 단계 (Phase 3 - 선택사항)

#### 자동 파이프라인 구축
- [ ] Twemoji SVG 자동 다운로드 스크립트
- [ ] PNG 변환 및 애틀라스 생성 자동화
- [ ] 메시지 로그에서 사용 이모지만 추출
- [ ] 스프라이트 에셋 증분 빌드

#### 성능 최적화
- [ ] 프레임 타임 프로파일링
- [ ] 전처리기 캐싱 최적화
- [ ] 애틀라스 분할 (512/1024px)
- [ ] 2x 스프라이트 준비 (고해상도)

#### 플랫폼 별 대응
- [ ] Android: `androidx.emoji2` 통합 (네이티브 입력)
- [ ] iOS: Apple Color Emoji 폴백 테스트
- [ ] WebGL: 이모지 입력 처리 (workaround)

## Sequential 모드 동작 방식
1. **UIMovementController**: `designOrderMode = Sequential` 설정
2. **디자인 시퀀스**: AType → BType → AType → BType...
3. **프리팹 생성 시**: 
   - 1번째: AType (max20Objecta 활성화)
   - 2번째: BType (max20Objectb 활성화)
   - 3번째: AType (max20Objecta 활성화)
   - 4번째: BType (max20Objectb 활성화)
   - 반복...

## 개발 히스토리

### JSON 구조 v2.0 업데이트 (2025-10-03)

#### 업데이트 배경
- QR Message Wall CMS와의 호환성을 위해 새로운 JSON 구조 지원 필요
- 기존 구조와의 하위 호환성 유지 필요

#### 주요 변경사항
1. **새로운 JSON 구조 지원**:
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
         "author": "이인정",                    // name → author
         "content": "오빠언제와",              // formatted_message → content
         "timestamp": "2025-10-03 08:41",
         "status": "active",
         "language": "ko",
         "created_at": "2025-10-03 08:41:48",
         "updated_at": "2025-10-03 08:41:48"
       }
     ]
   }
   ```

2. **코드 변경사항**:
   - `JsonRootData`, `JsonMetadata` 클래스 추가
   - `JsonMessageData` 클래스 필드 업데이트 (name→author, formatted_message→content)
   - JSON 파싱 로직 개선 (JsonHelper 제거)
   - 메타데이터 정보 활용 (총 메시지 수, 소스, 버전 표시)
   - `TestNewJsonStructure()` 테스트 함수 추가

3. **하위 호환성**:
   - 기존 JSON 배열 구조도 계속 지원
   - 자동 감지 및 적절한 파싱 방식 선택

#### 테스트 방법
1. Unity에서 JsonLoader 컴포넌트 선택
2. Context Menu → "새로운 JSON 구조 테스트" 실행
3. Context Menu → "StreamingAssets에서 JSON 파일 읽기" 실행

#### 장점
- QR Message Wall CMS와 완벽 호환
- 더 풍부한 메시지 정보 (ID, 상태, 언어 등)
- 메타데이터를 통한 데이터 품질 관리
- 표준화된 JSON 구조로 확장성 향상

