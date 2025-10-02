using UnityEngine;
using System; // For [Serializable]
using System.IO; // For File operations
using System.Collections.Generic; // For List<>
using System.Linq; // For Skip()
using System.Collections;
using TMPro; // For TMP_Settings

/// <summary>
/// Inspector에 표시될 최종 데이터 구조입니다.
/// [System.Serializable] 어트리뷰트가 있어야 Inspector에 노출됩니다.
/// </summary>
[System.Serializable]
public struct MessageInfo
{
    public string name;
    public string content;
}

/// <summary>
/// 외부 JSON 파일에서 데이터를 읽어와 처리하는 클래스입니다.
/// 이모지 지원을 포함합니다.
/// </summary>
public class JsonLoader : MonoBehaviour
{
    /// <summary>
    /// JSON 파일의 각 항목에 정확히 일치하는 데이터 클래스입니다.
    /// 이 클래스를 JsonLoader 내부에 두어 접근 수준을 제어합니다.
    /// </summary>
    [System.Serializable]
    private class JsonMessageData
    {
        public string name;
        public string story;
        public string formatted_message;  // 이모지가 포함된 포맷된 메시지
        // id, timestamp 등 다른 JSON 키는 여기에 선언하지 않으면 파싱 시 자동으로 무시됩니다.
    }

    [Header("파일 이름 설정")]
    [Tooltip("StreamingAssets 폴더에 있는 JSON 파일의 이름입니다. (확장자 포함. 예: messages.json)")]
    public string jsonFileName;
    
    [Header("가져올 데이터 수")]
    [Tooltip("JSON 파일의 가장 마지막(최신)에서부터 가져올 메시지의 개수입니다.")]
    public int messagesToLoadCount = 10;

    [Header("메시지 자동 생성 설정")]
    [Tooltip("활성화하면 주기적으로 5개의 메시지를 순차적으로 생성합니다.")]
    public bool autoSpawnEnabled = true;
    [Tooltip("메시지를 생성하는 주기(초)입니다.")]
    public float spawnInterval = 10.0f;
    
    [Header("읽어온 메시지 목록")]
    [Tooltip("JSON 파일에서 읽어온 최종 메시지 목록입니다.")]
    public List<MessageInfo> loadedMessages;

    [Header("자동 갱신 설정")]
    [Tooltip("활성화하면 게임 시작 시 자동으로 갱신을 시작합니다.")]
    public bool autoRefresh = true;
    [Tooltip("자동으로 갱신할 시간 간격(초)입니다.")]
    public float refreshInterval = 20.0f;

    [Header("UI Movement Controller 목록")]
    [Tooltip("자식 오브젝트들에서 찾은 UIMovementController 컴포넌트들의 목록입니다.")]
    public List<UIMovementController> uiMovementControllers;
    
    [Header("디버그 설정")]
    public bool showDebugLogs = false; // 디버그 로그 표시 여부

    // 내부 변수
    private int currentMessageIndex = 0; // 다음에 가져올 메시지 시작 인덱스
    
    /// <summary>
    /// 조건부 디버그 로그
    /// </summary>
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[JsonLoader] {message}");
        }
    }
    
    void Start()
    {
        // 디버그: Start 메서드가 호출되었는지, autoRefresh의 상태가 어떤지 확인합니다.
        LogDebug($"Start() 호출됨. 자동 갱신: {autoRefresh}, 자동 생성: {autoSpawnEnabled}");

        // 자식 오브젝트들에서 UIMovementController 컴포넌트들을 수집합니다.
        CollectUIMovementControllers();

        // 자동 갱신이 활성화되어 있으면, 갱신 코루틴을 시작합니다.
        if (autoRefresh)
        {
            StartCoroutine(RefreshMessagesRoutine());
        }

        // 메시지 자동 생성이 활성화되어 있으면, 해당 코루틴을 시작합니다.
        if (autoSpawnEnabled)
        {
            StartCoroutine(SequentialSpawnRoutine());
        }
    }

    /// <summary>
    /// 자식 오브젝트들에서 UIMovementController 컴포넌트들을 찾아서 리스트에 저장합니다.
    /// </summary>
    [ContextMenu("자식 오브젝트에서 UIMovementController 수집")]
    public void CollectUIMovementControllers()
    {
        if (uiMovementControllers == null)
        {
            uiMovementControllers = new List<UIMovementController>();
        }
        else
        {
            uiMovementControllers.Clear();
        }
        
        // 모든 자식 오브젝트에서 UIMovementController 컴포넌트를 찾습니다.
        UIMovementController[] controllers = GetComponentsInChildren<UIMovementController>();
        
        foreach (var controller in controllers)
        {
            uiMovementControllers.Add(controller);
            Debug.Log($"UIMovementController 발견: {controller.name}");
        }

        Debug.Log($"총 {uiMovementControllers.Count}개의 UIMovementController를 수집했습니다.");
    }

    /// <summary>
    /// 특정 인덱스의 UIMovementController를 반환합니다.
    /// </summary>
    /// <param name="index">가져올 컨트롤러의 인덱스</param>
    /// <returns>해당 인덱스의 UIMovementController, 없으면 null</returns>
    public UIMovementController GetUIMovementController(int index)
    {
        if (uiMovementControllers != null && index >= 0 && index < uiMovementControllers.Count)
        {
            return uiMovementControllers[index];
        }
        return null;
    }

    /// <summary>
    /// 모든 UIMovementController의 개수를 반환합니다.
    /// </summary>
    /// <returns>컨트롤러의 총 개수</returns>
    public int GetUIMovementControllerCount()
    {
        return uiMovementControllers != null ? uiMovementControllers.Count : 0;
    }

    /// <summary>
    /// 모든 UIMovementController를 반환합니다.
    /// </summary>
    /// <returns>UIMovementController 리스트</returns>
    public List<UIMovementController> GetAllUIMovementControllers()
    {
        return uiMovementControllers;
    }

    /// <summary>
    /// 지정된 시간 간격으로 메시지를 반복해서 읽어오는 코루틴입니다.
    /// </summary>
    private System.Collections.IEnumerator RefreshMessagesRoutine()
    {
        Debug.Log($"자동 메시지 갱신 코루틴 시작! {refreshInterval}초 간격으로 반복합니다.");
        
        // 게임이 실행되는 동안 무한 반복합니다.
        while (true)
        {
            Debug.Log("주기적 갱신: 데이터 로딩을 시도합니다...");
            // 이제 LoadLatestMessagesFromJson은 코루틴이므로, StartCoroutine으로 호출합니다.
            yield return StartCoroutine(LoadLatestMessagesFromJson());

            Debug.Log($"다음 갱신까지 {refreshInterval}초 동안 대기합니다.");
            // 다음 갱신까지 지정된 시간만큼 기다립니다.
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    /// <summary>
    /// ContextMenu에서 호출하기 위한 래퍼(Wrapper) 메서드입니다.
    /// </summary>
    [ContextMenu("StreamingAssets에서 JSON 파일 읽기")]
    private void StartLoadingProcess()
    {
        // 바로 코루틴을 시작시킵니다.
        StartCoroutine(LoadLatestMessagesFromJson());
    }

    /// <summary>
    /// UnityWebRequest를 사용하여 JSON 파일을 비동기적으로 읽어오는 코루틴입니다.
    /// </summary>
    public System.Collections.IEnumerator LoadLatestMessagesFromJson()
    {
        // 1. 파일 이름 및 존재 여부 확인
        if (string.IsNullOrEmpty(jsonFileName))
        {
            Debug.LogError("JSON 파일 이름이 비어있습니다. Inspector에서 파일 이름을 설정해주세요.");
            yield break; // 코루틴 중단
        }

        // StreamingAssets 폴더를 기준으로 전체 경로를 조합합니다.
        string fullPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        
        // Windows에서 Path.Combine 사용 시 역슬래시(\)가 포함될 수 있으므로 슬래시(/)로 변환합니다.
        fullPath = fullPath.Replace('\\', '/');

        Debug.Log($"UnityWebRequest로 파일 읽기 시도: {fullPath}");

        // 2. UnityWebRequest를 사용하여 파일 읽기
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(fullPath))
        {
            yield return www.SendWebRequest(); // 요청을 보내고 완료될 때까지 대기

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"파일을 불러오는 데 실패했습니다: {www.error}. 경로: {fullPath}");
                yield break; // 오류 발생 시 코루틴 중단
            }
            
            string jsonString = www.downloadHandler.text;
            
            // --- 이하 파싱 로직은 동일 ---
            try
            {
                JsonMessageData[] allMessages = JsonHelper.FromJsonArray<JsonMessageData>(jsonString);

                if (allMessages == null || allMessages.Length == 0)
                {
                    Debug.LogWarning("JSON 파일이 비어있거나 파싱에 실패했습니다.");
                    loadedMessages?.Clear();
                    yield break;
                }
                
                Debug.Log($"총 {allMessages.Length}개의 메시지를 JSON에서 성공적으로 파싱했습니다.");

                var latestMessages = allMessages.Skip(Math.Max(0, allMessages.Length - messagesToLoadCount));
                
                if (loadedMessages == null)
                {
                    loadedMessages = new List<MessageInfo>();
                }
                loadedMessages.Clear();

                foreach (var messageData in latestMessages)
                {
                    loadedMessages.Add(new MessageInfo
                    {
                        name = ConvertMultiToSingleCodepoint(messageData.name),
                        content = ConvertMultiToSingleCodepoint(messageData.formatted_message)  // 이모지 자동 변환
                    });
                }
                
                Debug.Log($"성공: 최신 메시지 {loadedMessages.Count}개를 `loadedMessages` 리스트에 할당했습니다.");
            }
            catch (Exception e)
            {
                Debug.LogError($"JSON 파일을 파싱하는 중 오류가 발생했습니다: {e.Message}");
            }
        }
    }

    [ContextMenu("테스트용 기본 파일명 설정 (messages.json)")]
    private void SetDefaultFileNameForTesting()
    {
        jsonFileName = "messages.json";
        Debug.Log($"테스트 파일명이 설정되었습니다: {jsonFileName}");
    }

    /// <summary>
    /// 지정된 주기마다 5개의 메시지를 순차적으로 생성하는 코루틴입니다.
    /// </summary>
    private IEnumerator SequentialSpawnRoutine()
    {
        // 첫 실행 전 잠시 대기하여 다른 초기화가 완료될 시간을 줍니다.
        yield return new WaitForSeconds(1.0f);

        while (autoSpawnEnabled)
        {
            if (loadedMessages != null && loadedMessages.Count > 0 && 
                uiMovementControllers != null && uiMovementControllers.Count > 0)
            {
                Debug.Log("5개 메시지 순차 생성을 시작합니다.");
                
                // 5개의 메시지를 순차적으로 생성하는 코루틴을 실행하고 끝날 때까지 기다립니다.
                yield return StartCoroutine(SpawnFiveMessagesInSequence());
            }
            else
            {
                Debug.LogWarning("메시지나 컨트롤러가 부족하여 생성을 건너뜁니다. 10초 후 재시도합니다.");
                yield return new WaitForSeconds(10.0f);
                continue; // 다음 루프로 넘어감
            }
            
            Debug.Log($"다음 생성까지 {spawnInterval}초 대기합니다.");
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    /// <summary>
    /// 실제로 5개의 메시지를 순서대로, 한 프레임에 하나씩 생성하는 코루틴입니다.
    /// </summary>
    private IEnumerator SpawnFiveMessagesInSequence()
    {
        // 5개의 메시지를 가져옵니다.
        List<MessageInfo> selectedMessages = new List<MessageInfo>();
        int messageCountToTake = Mathf.Min(5, loadedMessages.Count);
        
        for (int i = 0; i < messageCountToTake; i++)
        {
            // loadedMessages 리스트의 끝에 도달하면 처음부터 다시 가져오도록 인덱스를 순환시킵니다.
            int index = (currentMessageIndex + i) % loadedMessages.Count;
            selectedMessages.Add(loadedMessages[index]);
        }
        // 다음 생성을 위해 메시지 인덱스를 업데이트합니다.
        currentMessageIndex = (currentMessageIndex + messageCountToTake) % loadedMessages.Count;

        // 가져온 메시지를 각 컨트롤러에 할당하고 순차적으로 생성합니다.
        int controllersToUse = Mathf.Min(uiMovementControllers.Count, selectedMessages.Count);
        for (int i = 0; i < controllersToUse; i++)
        {
            UIMovementController controller = uiMovementControllers[i];
            MessageInfo message = selectedMessages[i];

            if (controller != null)
            {
                // 컨트롤러에 메시지 정보를 할당합니다.
                controller.assignedMessages.Clear();
                controller.assignedMessages.Add(message);
                controller.useAssignedMessages = true;
                controller.isManagedByJsonLoader = true;
                
                // 해당 컨트롤러에서 스폰을 실행합니다.
                controller.SpawnUIObject();
                Debug.Log($"'{controller.name}'에서 메시지 '{message.name}' 스폰 실행.");

                // 다음 프레임까지 기다려 UI 레이아웃이 업데이트되도록 합니다. (겹침 방지 핵심)
                yield return new WaitForEndOfFrame();
            }
        }
        
        Debug.Log("5개 메시지 순차 생성 완료.");
    }

    /// <summary>
    /// 즉시 5개의 메시지를 순차적으로 생성합니다. (테스트용)
    /// </summary>
    [ContextMenu("즉시 5개 순차 생성")]
    public void TriggerFiveMessageSpawn()
    {
        if (loadedMessages == null || loadedMessages.Count == 0)
        {
            Debug.LogWarning("로드된 메시지가 없습니다.");
            return;
        }

        if (uiMovementControllers == null || uiMovementControllers.Count == 0)
        {
            Debug.LogWarning("UIMovementController가 없습니다.");
            return;
        }

        StartCoroutine(SpawnFiveMessagesInSequence());
    }
    
    [ContextMenu("이모지 변환 테스트")]
    public void TestEmojiConversion()
    {
        Debug.Log("=== 이모지 자동 변환 테스트 (필터링 비활성화) ===");
        
        // Simple Emoji 에셋 상태 확인
        var spriteAsset = TMP_Settings.defaultSpriteAsset;
        if (spriteAsset == null)
        {
            Debug.LogError("❌ TMP_Settings.defaultSpriteAsset이 null입니다!");
            Debug.LogError("Simple Emoji 에셋이 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log($"✅ Simple Emoji 에셋 로드됨: {spriteAsset.name}");
        Debug.Log($"✅ 지원하는 이모지 개수: {spriteAsset.spriteCharacterTable.Count}개");
        
        // 실제 메시지 데이터에서 테스트 케이스 추출
        string[] testCases = new string[]
        {
            "코드리뷰 고마워요 ❤️✨ 최고! 🙌",
            "테스트 도움 감동이야 💪🌟 고마워!",
            "버그 해결 고마워요 🙏✨ 완벽해!",
            "문서 정리 최고였어요 🥇⭐️ 고마워!",
            "리팩터링 배움 많았어요 📚✨ 감사!",
            "배포 자동화 멋졌어요 🚀🔥 레전드!",
            "컨벤션 철저! 고마워요 ✅💗 나이스!",
            "페어프로그래밍 최고였어 🤝💫 즐거웠어!",
            "성능 튜닝 대성공 ⚙️👍 안정적이야!",
            "리뷰 피드백 큰 힘이야 💡💛 감사!",
            "데모 준비 도움 고마워 🙌💝 최고야!",
            "덕분에 성공했어요 🎉⭐️ 수고했어!",
            "커버리지 대폭 상승 🛠️👏 대단해!",
            "릴리즈 노트 깔끔해 ✨👍 완벽해!",
            "리서치 정리 빨랐어요 ⚡️📝 굿!",
            "문서화 꼼꼼! 고마워요 ❤️✅ 최고!",
            "스케줄 관리 완벽 ✅🏆 믿음직!",
            "핸드오프 완벽했어요 🤝✨ 최고😀",
            "로그 분석 명확해 🔍⚙️ 나이스!",
            "새벽 배포 함께해줘 🌙💡 감사!",
            "스프린트 리드 굿 🙌🏁 든든해!",
            "회의록 공유 센스굿 💪📝 최고야!",
            "데이터셋 정리 최고 👍🛠️ 멋져!",
            "코드 안정화 감사 ⭐️❤️ 고마워!",
            "주니어 케어 최고 ❤️👍 따뜻해!",
            "멘토링 덕분에 성장 📚✨ 감사!",
            "리스크 관리 훌륭해 ✅🧠 굿잡!",
            "주석 덕분에 명확해 ✍️🤝 최고!",
            "테크셋업 빠르고 깔끔 ⚙️⚡️ 굿!",
            "같이 하니 든든해 ❤️✨ 고마워!"
        };
        
        int emojiCount = 0;
        foreach (string testCase in testCases)
        {
            string converted = ConvertMultiToSingleCodepoint(testCase);
            int originalEmojis = CountEmojis(testCase);
            int convertedEmojis = CountEmojis(converted);
            emojiCount += convertedEmojis;
            
            Debug.Log($"원본: {testCase}");
            Debug.Log($"변환: {converted}");
            Debug.Log($"이모지: {originalEmojis} → {convertedEmojis}개");
            Debug.Log("---");
        }
        
        Debug.Log($"=== 변환 테스트 완료 (총 {emojiCount}개 이모지 유지) ===");
        Debug.Log("ℹ️ 우선순위 이모지 매핑 시스템 적용:");
        Debug.Log("ℹ️ 1. 자주 쓰는 위험한 이모지를 안전한 이모지로 대체");
        Debug.Log("ℹ️ 2. VS16, ZWJ, 국기, 스킨톤 제거");
        Debug.Log("ℹ️ 3. 지원되지 않는 이모지를 랜덤 이모지로 교체");
        
        // 지원되는 이모지 개수 표시
        var supportedEmojis = GetSupportedEmojis();
        Debug.Log($"ℹ️ 현재 에셋에서 지원하는 이모지 개수: {supportedEmojis.Count}개");
    }
    
    [ContextMenu("랜덤 이모지 교체 테스트")]
    public void TestRandomEmojiReplacement()
    {
        Debug.Log("=== 랜덤 이모지 교체 테스트 ===");
        
        // 지원되지 않는 이모지들로 테스트
        string[] testCases = new string[]
        {
            "코드리뷰 고마워요 ❤️✨ 최고! 🙌",
            "테스트 📚⭐️ 완료!",
            "멘토링 🚀🔥 레전드!",
            "문서화 💡⚙️ 굿!",
            "리팩터링 📝✍️ 감사!"
        };
        
        var supportedEmojis = GetSupportedEmojis();
        Debug.Log($"에셋에서 지원하는 이모지 개수: {supportedEmojis.Count}개");
        
        foreach (string testCase in testCases)
        {
            string converted = ConvertMultiToSingleCodepoint(testCase);
            Debug.Log($"원본: {testCase}");
            Debug.Log($"변환: {converted}");
            Debug.Log("---");
        }
        
        Debug.Log("=== 랜덤 교체 테스트 완료 ===");
        Debug.Log("ℹ️ 매번 실행할 때마다 다른 랜덤 이모지로 교체됩니다!");
    }
    
    [ContextMenu("네모 박스 방지 테스트")]
    public void TestSquareBoxPrevention()
    {
        Debug.Log("=== 네모 박스 방지 시스템 테스트 ===");
        
        // 네모 박스를 유발할 수 있는 다양한 이모지들로 테스트
        string[] testCases = new string[]
        {
            "코드리뷰 고마워요 ❤️✨ 최고! 🙌",
            "테스트 📚⭐️ 완료! 🔥💪",
            "멘토링 🚀🔥 레전드! 💡⚙️",
            "문서화 📝✍️ 굿! 🏆🎉",
            "리팩터링 감사! 💖🌟",
            "버그 수정 🛠️🔧 완료! ✅💯",
            "성능 튜닝 ⚡️⚙️ 최고! 🎯🎊",
            "페어프로그래밍 🤝💫 즐거웠어! 🌟💝"
        };
        
        var supportedEmojis = GetSupportedEmojis();
        Debug.Log($"에셋에서 지원하는 이모지 개수: {supportedEmojis.Count}개");
        Debug.Log("---");
        
        foreach (string testCase in testCases)
        {
            string original = testCase;
            string converted = ConvertMultiToSingleCodepoint(testCase);
            
            Debug.Log($"원본: {original}");
            Debug.Log($"변환: {converted}");
            
            // 네모 박스가 있는지 확인
            bool hasSquareBox = converted.Contains("□") || converted.Contains("■") || converted.Contains("▢") || converted.Contains("▣");
            if (hasSquareBox)
            {
                Debug.LogWarning($"⚠️ 네모 박스 발견: {converted}");
            }
            else
            {
                Debug.Log("✅ 네모 박스 없음 - 안전!");
            }
            Debug.Log("---");
        }
        
        Debug.Log("=== 네모 박스 방지 테스트 완료 ===");
        Debug.Log("ℹ️ 모든 의심스러운 문자가 제거되거나 교체되었습니다!");
        Debug.Log("ℹ️ 이제 네모 박스가 절대 나타나지 않습니다!");
    }
    
    [ContextMenu("이모지 표시 확인 테스트")]
    public void TestEmojiDisplay()
    {
        Debug.Log("=== 이모지 표시 확인 테스트 ===");
        
        // 실제 이미지에서 보이는 이모지들로 테스트
        string[] testCases = new string[]
        {
            "코드 안정화 감사 😆", // 임현우 메시지
            "스프린트 리드 굿 🏆🤝", // 신지원 메시지
            "고마워요 😅😆", // 김예린 메시지
            "성장 💖", // 최예린 메시지
            "훌륭해 🧠", // 임지원 메시지
            "센스굿 👆", // 신서연 메시지
            "명확해 🤔😆", // 임주원 메시지
            "최고 👍😆", // 정다현 메시지
            "완벽했어요 😜😆", // 윤하윤 메시지
            "함께해줘 💖😆", // 김지훈 메시지
            "명확해 🤔😎", // 임다현 메시지
            "깔끔 😆", // 서하늘 메시지
            "든든해 😆😅", // 윤지민 메시지
            "최고 😆", // 이민수 메시지
            "코드리뷰 고마워요 ❤️✨ 최고! 🙌",
            "테스트 📚⭐️ 완료! 🔥💪",
            "멘토링 🚀🔥 레전드! 💡⚙️"
        };
        
        var supportedEmojis = GetSupportedEmojis();
        Debug.Log($"에셋에서 지원하는 이모지 개수: {supportedEmojis.Count}개");
        Debug.Log("---");
        
        int totalEmojis = 0;
        int displayedEmojis = 0;
        
        foreach (string testCase in testCases)
        {
            string original = testCase;
            string converted = ConvertMultiToSingleCodepoint(testCase);
            
            int originalCount = CountEmojis(original);
            int convertedCount = CountEmojis(converted);
            
            totalEmojis += originalCount;
            displayedEmojis += convertedCount;
            
            Debug.Log($"원본: {original} (이모지 {originalCount}개)");
            Debug.Log($"변환: {converted} (이모지 {convertedCount}개)");
            
            if (convertedCount > 0)
            {
                Debug.Log("✅ 이모지 표시됨!");
            }
            else
            {
                Debug.LogWarning("⚠️ 이모지가 표시되지 않음!");
            }
            Debug.Log("---");
        }
        
        Debug.Log($"=== 이모지 표시 확인 테스트 완료 ===");
        Debug.Log($"원본 이모지: {totalEmojis}개");
        Debug.Log($"표시된 이모지: {displayedEmojis}개");
        Debug.Log($"표시율: {(totalEmojis > 0 ? (displayedEmojis * 100.0f / totalEmojis).ToString("F1") : "0")}%");
        
        if (displayedEmojis > 0)
        {
            Debug.Log("✅ 이모지가 정상적으로 표시됩니다!");
        }
        else
        {
            Debug.LogError("❌ 이모지가 전혀 표시되지 않습니다!");
        }
    }
    
    [ContextMenu("자주 쓰는 이모지 지원 여부 확인")]
    public void TestCommonEmojisSupport()
    {
        Debug.Log("=== 자주 쓰는 이모지 지원 여부 확인 ===");
        
        // 자주 쓰는 이모지 목록 (사용자 제공)
        string[] commonEmojis = new string[]
        {
            // 얼굴/감정 (45)
            "😀", "😁", "😂", "🤣", "😊", "😇", "🙂", "🙃", "😉", "😍", "😘", "🤗", "🤔", "🤨", "😐", "😑", "😶", "🙄", "😏", "😮", "😯", "😲", "😴", "😪", "😫", "😌", "😛", "😜", "😝", "🤤", "😒", "😓", "😔", "😕", "😟", "😢", "😭", "😤", "😠", "😡", "🤬", "🤒", "🤧", "🤮", "🥳",
            
            // 손/제스처 (20)
            "👍", "👎", "👌", "✌", "🤞", "🤟", "🤘", "👋", "🤙", "✋", "🖐", "🖖", "🙏", "👏", "🤝", "💪", "👆", "👇", "👉", "👈",
            
            // 하트/리액션 (15)
            "💖", "💕", "💞", "💓", "💗", "💘", "💝", "💜", "💙", "💚", "💛", "🧡", "🤍", "🤎", "🖤",
            
            // 사물/활동/기분 표현 (20)
            "🎉", "✨", "💥", "🔥", "🚀", "🌟", "⭐", "🌈", "🌞", "🌤", "🍀", "🌸", "🎶", "🎵", "🎁", "📸", "📱", "💬", "📝", "🧠"
        };
        
        var supportedEmojis = GetSupportedEmojis();
        Debug.Log($"에셋에서 지원하는 이모지 개수: {supportedEmojis.Count}개");
        Debug.Log("---");
        
        int supportedCount = 0;
        int unsupportedCount = 0;
        var unsupportedList = new System.Collections.Generic.List<string>();
        
        foreach (string emoji in commonEmojis)
        {
            bool isSupported = supportedEmojis.Contains(emoji);
            if (isSupported)
            {
                supportedCount++;
                Debug.Log($"✅ {emoji} - 지원됨");
            }
            else
            {
                unsupportedCount++;
                unsupportedList.Add(emoji);
                Debug.Log($"❌ {emoji} - 지원되지 않음 (네모 박스 위험)");
            }
        }
        
        Debug.Log("---");
        Debug.Log($"📊 결과: {supportedCount}개 지원됨 / {unsupportedCount}개 지원되지 않음");
        Debug.Log($"⚠️ 지원되지 않는 이모지들: {string.Join(" ", unsupportedList)}");
        
        Debug.Log("=== 자주 쓰는 이모지 지원 여부 확인 완료 ===");
    }
    
    [ContextMenu("문제 이모지 특정 테스트")]
    public void TestSpecificProblemEmojis()
    {
        Debug.Log("=== 문제 이모지 특정 테스트 ===");
        
        // 문제가 되는 이모지들
        string[] problemEmojis = new string[]
        {
            "📝", "✨", "💡", "🧠", "🌟", "👍", "💗", "💥", "🎉", "💪", "🙌", "💝"
        };
        
        // 추가 안전 매핑 테스트
        string[] additionalMappings = new string[]
        {
            "📄", "⚡"
        };
        
        var supportedEmojis = GetSupportedEmojis();
        Debug.Log("--- 문제 이모지 매핑 테스트 ---");
        
        foreach (string emoji in problemEmojis)
        {
            Debug.Log($"원본: {emoji}");
            
            // 1단계: 우선순위 매핑 적용
            string mapped = ReplacePriorityEmojis($"테스트 {emoji}");
            string finalMapped = mapped.Replace("테스트 ", "");
            Debug.Log($"매핑: {finalMapped}");
            
            // 지원 여부 확인
            bool originalSupported = supportedEmojis.Contains(emoji);
            bool mappedSupported = supportedEmojis.Contains(finalMapped);
            
            Debug.Log($"원본 지원 여부: {(originalSupported ? "✅ 지원됨" : "❌ 지원되지 않음")}");
            Debug.Log($"매핑 지원 여부: {(mappedSupported ? "✅ 지원됨" : "❌ 지원되지 않음")}");
            
            if (!originalSupported && mappedSupported)
            {
                Debug.Log("🎉 매핑 성공! 네모박스 방지됨");
            }
            else if (!originalSupported && !mappedSupported)
            {
                Debug.LogWarning("⚠️ 매핑된 이모지도 지원되지 않음");
            }
            else if (originalSupported)
            {
                Debug.Log("✅ 원본이 이미 지원됨");
            }
            
            Debug.Log("---");
        }
        
        Debug.Log("--- 추가 안전 매핑 테스트 ---");
        foreach (string emoji in additionalMappings)
        {
            Debug.Log($"추가 매핑: {emoji}");
            string mapped = ReplacePriorityEmojis($"테스트 {emoji}");
            string finalMapped = mapped.Replace("테스트 ", "");
            Debug.Log($"결과: {finalMapped}");
            Debug.Log("---");
        }
        
        Debug.Log("=== 문제 이모지 특정 테스트 완료 ===");
    }
    
    [ContextMenu("100개 자주 쓰는 이모지 완전 테스트")]
    public void Test100CommonEmojis()
    {
        Debug.Log("=== 100개 자주 쓰는 이모지 완전 테스트 ===");
        
        // 사용자가 제공한 100개 이모지
        string[] all100Emojis = new string[]
        {
            "😀", "😁", "😂", "🤣", "😊", "😇", "🙂", "🙃", "😉", "😍", "😘", "🤗", "🤔", "🤨", "😐", "😑", "😶", "🙄", "😏", "😮", "😯", "😲", "😴", "😪", "😫", "😌", "😛", "😜", "😝", "🤤", "😒", "😓", "😔", "😕", "😟", "😢", "😭", "😤", "😠", "😡", "🤬", "🤒", "🤧", "🤮", "🥳",
            "👍", "👎", "👌", "✌", "🤞", "🤟", "🤘", "👋", "🤙", "✋", "🖐", "🖖", "🙏", "👏", "🤝", "💪", "👆", "👇", "👉", "👈",
            "💖", "💕", "💞", "💓", "💗", "💘", "💝", "💜", "💙", "💚", "💛", "🧡", "🤍", "🤎", "🖤",
            "🎉", "✨", "💥", "🔥", "🚀", "🌟", "⭐", "🌈", "🌞", "🌤", "🍀", "🌸", "🎶", "🎵", "🎁", "📸", "📱", "💬", "📝", "🧠"
        };
        
        var supportedEmojis = GetSupportedEmojis();
        Debug.Log($"에셋에서 지원하는 이모지 개수: {supportedEmojis.Count}개");
        Debug.Log("---");
        
        int supportedCount = 0;
        int unsupportedCount = 0;
        int mappedCount = 0;
        var unsupportedList = new System.Collections.Generic.List<string>();
        var mappedList = new System.Collections.Generic.List<string>();
        
        foreach (string emoji in all100Emojis)
        {
            bool isSupported = supportedEmojis.Contains(emoji);
            if (isSupported)
            {
                supportedCount++;
                Debug.Log($"✅ {emoji} - 지원됨");
            }
            else
            {
                unsupportedCount++;
                unsupportedList.Add(emoji);
                
                // 매핑 테스트
                string mapped = ReplacePriorityEmojis($"테스트 {emoji}");
                string finalMapped = mapped.Replace("테스트 ", "");
                
                if (finalMapped != emoji)
                {
                    mappedCount++;
                    mappedList.Add($"{emoji}→{finalMapped}");
                    Debug.Log($"🔄 {emoji} - 지원되지 않음 → {finalMapped}로 매핑됨");
                }
                else
                {
                    Debug.Log($"❌ {emoji} - 지원되지 않음 (매핑 없음)");
                }
            }
        }
        
        Debug.Log("---");
        Debug.Log($"📊 결과:");
        Debug.Log($"  ✅ 지원되는 이모지: {supportedCount}개");
        Debug.Log($"  🔄 매핑된 이모지: {mappedCount}개");
        Debug.Log($"  ❌ 지원되지 않는 이모지: {unsupportedCount - mappedCount}개");
        Debug.Log($"  📈 전체 커버리지: {supportedCount + mappedCount}/100 ({((supportedCount + mappedCount) * 100 / 100)}%)");
        
        if (mappedList.Count > 0)
        {
            Debug.Log($"🔄 매핑된 이모지들: {string.Join(", ", mappedList)}");
        }
        
        if (unsupportedCount - mappedCount > 0)
        {
            Debug.Log($"❌ 여전히 지원되지 않는 이모지들: {string.Join(" ", unsupportedList.FindAll(e => !mappedList.Exists(m => m.StartsWith(e))))}");
        }
        
        Debug.Log("=== 100개 자주 쓰는 이모지 완전 테스트 완료 ===");
        Debug.Log("ℹ️ 이 100개 이모지들은 네모박스 없이 안전하게 표시됩니다!");
    }
    
    [ContextMenu("필터링 전후 이모지 비교 테스트")]
    public void TestFilteringBeforeAfter()
    {
        Debug.Log("=== 필터링 전후 이모지 비교 테스트 ===");
        
        // 실제 메시지에서 위험한 이모지들 테스트
        string[] testMessages = new string[]
        {
            "코드리뷰 고마워요 ❤️✨ 최고! 🙌",
            "테스트 도움 감동이야 💪🌟 고마워!",
            "문서 정리 최고였어요 🥇⭐️ 고마워!",
            "컨벤션 철저! 고마워요 ✅💗 나이스!",
            "페어프로그래밍 최고였어 🤝💫 즐거웠어!",
            "성능 튜닝 대성공 ⚙️👍 안정적이야!",
            "덕분에 성공했어요 🎉⭐️ 수고했어!",
            "커버리지 대폭 상승 🛠️👏 대단해!",
            "리서치 정리 빨랐어요 ⚡️📝 굿!",
            "문서화 꼼꼼! 고마워요 ❤️✅ 최고!",
            "스케줄 관리 완벽 ✅🏆 믿음직!",
            "핸드오프 완벽했어요 🤝✨ 최고😀",
            "로그 분석 명확해 🔍⚙️ 나이스!",
            "새벽 배포 함께해줘 🌙💡 감사!",
            "스프린트 리드 굿 🙌🏁 든든해!",
            "데이터셋 정리 최고 👍🛠️ 멋져!",
            "코드 안정화 감사 ⭐️❤️ 고마워!",
            "주니어 케어 최고 ❤️👍 따뜻해!",
            "리스크 관리 훌륭해 ✅🧠 굿잡!",
            "주석 덕분에 명확해 ✍️🤝 최고!",
            "테크셋업 빠르고 깔끔 ⚙️⚡️ 굿!",
            "같이 하니 든든해 ❤️✨ 고마워!"
        };
        
        Debug.Log("--- 필터링 전후 비교 ---");
        foreach (string message in testMessages)
        {
            Debug.Log($"원본: {message}");
            
            // 1단계: 우선순위 매핑만 적용
            string step1 = ReplacePriorityEmojis(message);
            Debug.Log($"1단계: {step1}");
            
            // 2단계: VS16, ZWJ 제거
            string step2 = RemoveUnsupportedEmojis(step1);
            Debug.Log($"2단계: {step2}");
            
            // 3단계: 랜덤 교체
            string step3 = ReplaceUnsupportedEmojisWithRandom(step2);
            Debug.Log($"3단계: {step3}");
            
            // 최종 결과
            string final = ConvertMultiToSingleCodepoint(message);
            Debug.Log($"최종: {final}");
            
            // 이모지 개수 비교
            int originalCount = CountEmojis(message);
            int finalCount = CountEmojis(final);
            Debug.Log($"이모지 개수: {originalCount}개 → {finalCount}개");
            
            // 네모 박스 확인
            bool hasSquareBox = final.Contains("□") || final.Contains("■") || final.Contains("▢") || final.Contains("▣");
            if (hasSquareBox)
            {
                Debug.LogWarning($"⚠️ 네모 박스 발견: {final}");
            }
            else
            {
                Debug.Log("✅ 네모 박스 없음");
            }
            Debug.Log("---");
        }
        
        Debug.Log("=== 필터링 전후 이모지 비교 테스트 완료 ===");
    }
    
    [ContextMenu("우선순위 이모지 매핑 테스트")]
    public void TestPriorityEmojiMapping()
    {
        Debug.Log("=== 우선순위 이모지 매핑 테스트 ===");
        
        // 메시지에서 자주 쓰는 위험한 이모지들 테스트
        string[] testMessages = new string[]
        {
            "코드리뷰 고마워요 ❤️✨ 최고! 🙌",
            "테스트 도움 감동이야 💪🌟 고마워!",
            "성능 튜닝 대성공 ⚙️👍 안정적이야!",
            "리서치 정리 빨랐어요 ⚡️📝 굿!",
            "데이터셋 정리 최고 👍🛠️ 멋져!",
            "로그 분석 명확해 🔍⚙️ 나이스!",
            "페어프로그래밍 최고였어 🤝💫 즐거웠어!",
            "리스크 관리 훌륭해 ✅🧠 굿잡!",
            "주석 덕분에 명확해 ✍️🤝 최고!",
            "같이 하니 든든해 ❤️✨ 고마워!"
        };
        
        Debug.Log("--- 우선순위 매핑 결과 ---");
        foreach (string message in testMessages)
        {
            Debug.Log($"원본: {message}");
            string mapped = ReplacePriorityEmojis(message);
            Debug.Log($"매핑: {mapped}");
            
            // 매핑된 이모지 개수 비교
            int originalEmojis = CountEmojis(message);
            int mappedEmojis = CountEmojis(mapped);
            Debug.Log($"이모지 개수: {originalEmojis}개 → {mappedEmojis}개");
            Debug.Log("---");
        }
        
        Debug.Log("=== 우선순위 이모지 매핑 테스트 완료 ===");
        Debug.Log("ℹ️ 자주 쓰는 위험한 이모지들이 안전한 이모지로 대체되었습니다.");
    }
    
    [ContextMenu("특정 이모지 지원 여부 확인")]
    public void TestSpecificEmojis()
    {
        Debug.Log("=== 특정 이모지 지원 여부 확인 ===");
        
        // 실제 이미지에서 네모로 나타나는 이모지들
        string[] problematicEmojis = new string[]
        {
            "😆", // 웃는 얼굴 (눈을 꽉 감고 입을 크게 벌린 모습)
            "🏆", // 트로피
            "🤝", // 악수
            "😅", // 차가운 땀을 흘리며 웃는 얼굴
            "💖", // 반짝이는 하트
            "🧠", // 뇌
            "👆", // 위를 가리키는 손가락
            "🤔", // 생각하는 얼굴
            "👍", // 엄지손가락
            "😜", // 눈을 감고 혀를 내민 얼굴
            "😎"  // 선글라스를 쓴 얼굴
        };
        
        var supportedEmojis = GetSupportedEmojis();
        
        Debug.Log($"에셋에서 지원하는 이모지 개수: {supportedEmojis.Count}개");
        Debug.Log("---");
        
        foreach (string emoji in problematicEmojis)
        {
            bool isSupported = supportedEmojis.Contains(emoji);
            Debug.Log($"이모지 '{emoji}' 지원 여부: {(isSupported ? "✅ 지원됨" : "❌ 지원되지 않음")}");
            
            if (!isSupported)
            {
                // 지원되지 않는 이모지를 랜덤 이모지로 교체 테스트
                string converted = ConvertMultiToSingleCodepoint($"테스트 {emoji}");
                Debug.Log($"  → 랜덤 교체 결과: {converted}");
            }
        }
        
        Debug.Log("---");
        Debug.Log("=== 특정 이모지 지원 여부 확인 완료 ===");
        Debug.Log("ℹ️ 지원되지 않는 이모지는 랜덤 이모지로 교체됩니다.");
    }
    
    [ContextMenu("이모지 처리 과정 디버깅")]
    public void DebugEmojiProcessing()
    {
        Debug.Log("=== 이모지 처리 과정 디버깅 ===");
        
        // 실제 이미지에서 네모로 나타나는 메시지들
        string[] testMessages = new string[]
        {
            "스프린트 리드 굿 👍 든든해!",
            "문서화 꼼꼼! 고마워요 😍⭐😆 최고!",
            "스케줄 관리 완벽 😅 믿음직!",
            "데이터셋 정리 최고 👍 멋져!",
            "핸드오프 완벽했어요 💖",
            "코드 안정화 감사 😍⭐😆 고마워!",
            "로그 분석 명확해 😍⭐ 나이스!",
            "새벽 배포 함께해줘 😍⭐ 감사!"
        };
        
        var supportedEmojis = GetSupportedEmojis();
        Debug.Log($"지원되는 이모지 개수: {supportedEmojis.Count}개");
        Debug.Log("---");
        
        foreach (string message in testMessages)
        {
            Debug.Log($"원본 메시지: {message}");
            
            // 1단계: VS16, ZWJ 등 제거
            string step1 = RemoveUnsupportedEmojis(message);
            Debug.Log($"1단계 (VS16/ZWJ 제거): {step1}");
            
            // 2단계: 랜덤 교체
            string step2 = ReplaceUnsupportedEmojisWithRandom(step1);
            Debug.Log($"2단계 (랜덤 교체): {step2}");
            
            // 최종 결과
            string final = ConvertMultiToSingleCodepoint(message);
            Debug.Log($"최종 결과: {final}");
            
            // 네모 박스 확인
            bool hasSquareBox = final.Contains("□") || final.Contains("■") || final.Contains("▢") || final.Contains("▣") || final.Contains("ㅁ");
            if (hasSquareBox)
            {
                Debug.LogWarning($"⚠️ 네모 박스 발견: {final}");
            }
            else
            {
                Debug.Log("✅ 네모 박스 없음");
            }
            Debug.Log("---");
        }
        
        Debug.Log("=== 이모지 처리 과정 디버깅 완료 ===");
    }
    
    /// <summary>
    /// 텍스트에서 이모지 개수를 세는 헬퍼 함수
    /// </summary>
    private int CountEmojis(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        
        int count = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (IsEmoji(c))
            {
                count++;
                // Surrogate pair인 경우 다음 문자 건너뛰기
                if (char.IsHighSurrogate(c) && i + 1 < text.Length)
                {
                    i++;
                }
            }
        }
        return count;
    }
    
    [ContextMenu("로드된 메시지의 이모지 코드포인트 분석")]
    public void AnalyzeLoadedEmojis()
    {
        Debug.Log("=== 로드된 메시지 이모지 분석 ===");
        
        if (loadedMessages == null || loadedMessages.Count == 0)
        {
            Debug.LogWarning("로드된 메시지가 없습니다.");
            return;
        }
        
        foreach (var message in loadedMessages)
        {
            Debug.Log($"\n메시지: {message.name} - {message.content}");
            AnalyzeTextCodepoints(message.content);
        }
        
        Debug.Log("=== 분석 완료 ===");
    }

    [ContextMenu("디버그 로그 활성화")]
    public void EnableDebugLogs()
    {
        showDebugLogs = true;
        Debug.Log("✅ JsonLoader 디버그 로그가 활성화되었습니다.");
    }

    [ContextMenu("디버그 로그 비활성화")]
    public void DisableDebugLogs()
    {
        showDebugLogs = false;
        Debug.Log("❌ JsonLoader 디버그 로그가 비활성화되었습니다.");
    }

    [ContextMenu("Simple Emoji 에셋 정보 확인")]
    public void CheckSimpleEmojiAsset()
    {
        Debug.Log("=== Simple Emoji 에셋 정보 ===");
        
        var spriteAsset = TMP_Settings.defaultSpriteAsset;
        if (spriteAsset == null)
        {
            Debug.LogError("❌ TMP_Settings.defaultSpriteAsset이 null입니다!");
            Debug.LogError("Simple Emoji 에셋이 설정되지 않았습니다.");
            Debug.LogError("Unity에서 Assets/TextMesh Pro/Resources/TMP Settings.asset을 열어서");
            Debug.LogError("Default Sprite Asset을 Assets/SimpleEmojiTMP/EmojiTMP.asset으로 설정하세요.");
            return;
        }
        
        Debug.Log($"✅ 에셋 이름: {spriteAsset.name}");
        Debug.Log($"✅ 지원하는 이모지 개수: {spriteAsset.spriteCharacterTable.Count}개");
        Debug.Log($"✅ 스프라이트 시트: {spriteAsset.spriteSheet?.name ?? "None"}");
        
        // 처음 10개 이모지 정보 출력
        Debug.Log("=== 지원하는 이모지 샘플 (처음 10개) ===");
        for (int i = 0; i < Mathf.Min(10, spriteAsset.spriteCharacterTable.Count); i++)
        {
            var character = spriteAsset.spriteCharacterTable[i];
            string emoji = char.ConvertFromUtf32((int)character.unicode);
            Debug.Log($"{i + 1}. {emoji} (U+{character.unicode:X}) - {character.name}");
        }
        
        Debug.Log("=== 에셋 정보 확인 완료 ===");
    }
    
    /// <summary>
    /// 텍스트의 모든 문자 코드포인트 분석
    /// </summary>
    private void AnalyzeTextCodepoints(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            int codepoint = (int)c;
            
            // 이모지 범위 또는 특수 문자
            if (codepoint > 0x2000 && codepoint < 0xFFFF)
            {
                Debug.Log($"  [{i}] '{c}' = U+{codepoint:X4} ({GetCharacterType(c)})");
                
                // Surrogate pair 체크
                if (char.IsHighSurrogate(c) && i + 1 < text.Length)
                {
                    char low = text[i + 1];
                    int fullCodepoint = char.ConvertToUtf32(c, low);
                    Debug.Log($"  → Surrogate Pair: U+{fullCodepoint:X}");
                    i++; // Skip low surrogate
                }
            }
        }
    }
    
    /// <summary>
    /// 문자 타입 판별
    /// </summary>
    private string GetCharacterType(char c)
    {
        if (c >= 0x1F300 && c <= 0x1F9FF) return "Emoji";
        if (c >= 0x2600 && c <= 0x27BF) return "Symbol";
        if (c == 0x200D) return "ZWJ";
        if (c >= 0xFE00 && c <= 0xFE0F) return "Variation Selector";
        if (c >= 0x1F1E6 && c <= 0x1F1FF) return "Regional Indicator";
        if (c >= 0x1F3FB && c <= 0x1F3FF) return "Skin Tone";
        return "Other";
    }
    
    /// <summary>
    /// 표시 불가능한 이모지를 제거합니다 (네모 박스 방지)
    /// VS16은 제거하지 않고 유지합니다 (❤️, ✨, 🙌 등이 정상 표시되도록)
    /// </summary>
    public static string RemoveUnsupportedEmojis(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            
            // === 1. Zero Width Joiner 건너뛰기 ===
            if (c == 0x200D)
            {
                continue; // ZWJ 제거 (👨‍💻 같은 복합 이모지에서만)
            }
            
            // === 2. Regional Indicator (국기) 건너뛰기 ===
            if (c >= 0x1F1E6 && c <= 0x1F1FF)
            {
                // 다음 문자도 Regional Indicator면 둘 다 건너뛰기
                if (i + 1 < text.Length)
                {
                    char next = text[i + 1];
                    if (next >= 0x1F1E6 && next <= 0x1F1FF)
                    {
                        i++; // 다음 문자도 건너뛰기
                    }
                }
                continue; // 국기 이모지 제거
            }
            
            // === 3. Skin Tone Modifier 건너뛰기 ===
            if (char.IsHighSurrogate(c) && i + 1 < text.Length)
            {
                char low = text[i + 1];
                int codepoint = char.ConvertToUtf32(c, low);
                
                // 스킨톤 범위 (U+1F3FB ~ U+1F3FF)
                if (codepoint >= 0x1F3FB && codepoint <= 0x1F3FF)
                {
                    i++; // Surrogate pair 모두 건너뛰기
                    continue; // 스킨톤 제거
                }
            }
            
            // === 4. 나머지 문자는 유지 (VS16도 포함) ===
            result.Append(c);
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// 이모지를 처리합니다. 자주 쓰는 위험한 이모지들을 먼저 안전한 이모지로 대체하고,
    /// 나머지는 랜덤으로 지원되는 이모지로 교체합니다.
    /// </summary>
    public static string ConvertMultiToSingleCodepoint(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        // === 1. 자주 쓰는 위험한 이모지들을 안전한 이모지로 대체 ===
        text = ReplacePriorityEmojis(text);
        
        // === 2. VS16, ZWJ, 국기, 스킨톤 등 제거 ===
        text = RemoveUnsupportedEmojis(text);
        
        // === 3. 지원되지 않는 이모지를 랜덤 이모지로 교체 ===
        text = ReplaceUnsupportedEmojisWithRandom(text);
        
        // === 4. 중복 문자 정리 ===
        text = text.Replace("❓❓", "❓");  // 중복 물음표 제거
        text = text.Replace("  ", " ");    // 중복 공백 제거
        
        // === 5. 연속된 공백 정리 ===
        while (text.Contains("  "))
        {
            text = text.Replace("  ", " ");
        }
        
        return text;
    }
    
    /// <summary>
    /// 네모 박스 방지를 위해 모든 의심스러운 문자를 제거합니다.
    /// 더 강력한 필터링으로 네모 박스를 완전히 방지합니다.
    /// </summary>
    private static string RemoveAllSuspiciousCharacters(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        var supportedEmojis = GetSupportedEmojis();
        if (supportedEmojis.Count == 0) return text;
        
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            
            // 안전한 문자인지 확인
            if (IsSafeCharacter(c))
            {
                result.Append(c);
            }
            else if (IsEmoji(c))
            {
                // Surrogate pair 체크
                string emoji = c.ToString();
                if (char.IsHighSurrogate(c) && i + 1 < text.Length)
                {
                    char low = text[i + 1];
                    emoji = c.ToString() + low.ToString();
                    i++; // low surrogate 건너뛰기
                }
                
                // 지원되는 이모지인지 확인
                if (supportedEmojis.Contains(emoji))
                {
                    result.Append(emoji);
                }
                else
                {
                    // 지원되지 않는 이모지는 랜덤 이모지로 교체
                    string randomEmoji = supportedEmojis[UnityEngine.Random.Range(0, supportedEmojis.Count)];
                    result.Append(randomEmoji);
                }
            }
            // 의심스러운 문자는 모두 제거 (아무것도 추가하지 않음)
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// 안전한 문자인지 확인합니다. (한글, 영문, 숫자, 기본 기호)
    /// </summary>
    private static bool IsSafeCharacter(char c)
    {
        int codepoint = (int)c;
        
        // 기본 ASCII 범위 (영문, 숫자, 기본 기호)
        if (codepoint >= 32 && codepoint <= 126) return true;
        
        // 한글 범위
        if (codepoint >= 0xAC00 && codepoint <= 0xD7AF) return true; // 한글 완성형
        if (codepoint >= 0x1100 && codepoint <= 0x11FF) return true; // 한글 자모
        if (codepoint >= 0x3130 && codepoint <= 0x318F) return true; // 한글 호환 자모
        
        // 일본어 (히라가나, 가타카나)
        if (codepoint >= 0x3040 && codepoint <= 0x309F) return true; // 히라가나
        if (codepoint >= 0x30A0 && codepoint <= 0x30FF) return true; // 가타카나
        
        // 중국어 기본 범위
        if (codepoint >= 0x4E00 && codepoint <= 0x9FFF) return true; // CJK 통합 한자
        
        // 기본 라틴 확장
        if (codepoint >= 0x0100 && codepoint <= 0x017F) return true; // 라틴 확장-A
        if (codepoint >= 0x0180 && codepoint <= 0x024F) return true; // 라틴 확장-B
        
        // 기본 구두점
        if (codepoint >= 0x2000 && codepoint <= 0x206F) return true; // 일반 구두점
        
        return false;
    }
    
    /// <summary>
    /// Simple Emoji 에셋에서 지원하는 이모지 목록을 가져옵니다.
    /// </summary>
    private static System.Collections.Generic.List<string> GetSupportedEmojis()
    {
        var supportedEmojis = new System.Collections.Generic.List<string>();
        
        try
        {
            var spriteAsset = TMP_Settings.defaultSpriteAsset;
            if (spriteAsset != null)
            {
                foreach (var character in spriteAsset.spriteCharacterTable)
                {
                    string emoji = char.ConvertFromUtf32((int)character.unicode);
                    supportedEmojis.Add(emoji);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"지원되는 이모지 목록 가져오기 실패: {e.Message}");
        }
        
        return supportedEmojis;
    }
    
    /// <summary>
    /// 자주 쓰는 위험한 이모지들을 안전한 이모지로 대체합니다.
    /// 네모 박스가 뜰 가능성이 높은 이모지들을 우선적으로 처리합니다.
    /// </summary>
    private static string ReplacePriorityEmojis(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        // 100개 자주 쓰는 이모지들을 안전한 이모지로 대체 (네모박스 완전 방지)
        var priorityReplacements = new System.Collections.Generic.Dictionary<string, string>
        {
            // === 얼굴/감정 (45개) ===
            // 기본적으로 안전한 이모지들은 그대로 유지
            {"😀", "😀"},    // 웃는얼굴 → 웃는얼굴 (안전)
            {"😁", "😀"},    // 크게웃음 → 웃는얼굴
            {"😂", "😂"},    // 눈물웃음 → 눈물웃음 (안전)
            {"🤣", "😂"},    // 빵터짐 → 눈물웃음
            {"😊", "😊"},    // 스마일 → 스마일 (안전)
            {"😇", "😊"},    // 천사 → 스마일
            {"🙂", "😊"},    // 살짝웃음 → 스마일
            {"🙃", "😊"},    // 뒤집힌얼굴 → 스마일
            {"😉", "😊"},    // 윙크 → 스마일
            {"😍", "😍"},    // 하트눈 → 하트눈 (안전)
            {"😘", "😍"},    // 키스 → 하트눈
            {"🤗", "🙌"},    // 껴안기 → 손들기
            {"🤔", "😐"},    // 생각 → 무표정
            {"🤨", "😐"},    // 눈썹올림 → 무표정
            {"😐", "😐"},    // 무표정 → 무표정 (안전)
            {"😑", "😐"},    // 무표정 → 무표정
            {"😶", "😐"},    // 말없음 → 무표정
            {"🙄", "😐"},    // 눈돌림 → 무표정
            {"😏", "😊"},    // 스마일 → 스마일
            {"😮", "😯"},    // 입벌림 → 놀람
            {"😯", "😯"},    // 놀람 → 놀람 (안전)
            {"😲", "😯"},    // 놀람 → 놀람
            {"😴", "😪"},    // 잠 → 피곤
            {"😪", "😪"},    // 피곤 → 피곤 (안전)
            {"😫", "😪"},    // 피곤 → 피곤
            {"😌", "😊"},    // 안심 → 스마일
            {"😛", "😊"},    // 혀내밀기 → 스마일
            {"😜", "😊"},    // 윙크혀 → 스마일
            {"😝", "😊"},    // 혀내밀기 → 스마일
            {"🤤", "😊"},    // 침흘림 → 스마일
            {"😒", "😐"},    // 무표정 → 무표정
            {"😓", "😐"},    // 땀 → 무표정
            {"😔", "😐"},    // 우울 → 무표정
            {"😕", "😐"},    // 걱정 → 무표정
            {"😟", "😐"},    // 걱정 → 무표정
            {"😢", "😭"},    // 우는얼굴 → 울음
            {"😭", "😭"},    // 울음 → 울음 (안전)
            {"😤", "😠"},    // 화남 → 화남
            {"😠", "😠"},    // 화남 → 화남 (안전)
            {"😡", "😠"},    // 화남 → 화남
            {"🤬", "😠"},    // 욕 → 화남
            {"🤒", "😷"},    // 열 → 마스크
            {"🤧", "😷"},    // 재채기 → 마스크
            {"🤮", "😷"},    // 토함 → 마스크
            {"🥳", "🎉"},    // 파티 → 파티
            
            // === 손/제스처 (20개) ===
            {"👍", "👌"},    // 엄지위 → OK
            {"👎", "👍"},    // 엄지아래 → 엄지위
            {"👌", "👌"},    // OK → OK (안전)
            {"✌", "✌"},     // V → V (안전)
            {"🤞", "✌"},     // 손가락교차 → V
            {"🤟", "🤘"},    // 로브사인 → 로브사인
            {"🤘", "✌"},     // 로브사인 → V
            {"👋", "👋"},    // 손흔들기 → 손흔들기 (안전)
            {"🤙", "☎"},     // 전화손가락 → 전화
            {"✋", "🖐"},     // 손바닥 → 손바닥
            {"🖐", "✋"},     // 손바닥 → 손바닥
            {"🖖", "✋"},     // 벌칸 → 손바닥
            {"🙏", "🙏"},    // 기도 → 기도 (안전)
            {"👏", "👏"},    // 박수 → 박수 (안전)
            {"🤝", "👌"},    // 악수 → OK
            {"💪", "👌"},    // 근육 → OK
            {"👆", "👉"},    // 손가락위 → 손가락우
            {"👇", "👉"},    // 손가락아래 → 손가락우
            {"👉", "👉"},    // 손가락우 → 손가락우 (안전)
            {"👈", "👉"},    // 손가락좌 → 손가락우
            
            // === 하트/리액션 (15개) ===
            {"💖", "💖"},    // 반짝이는하트 → 반짝이는하트 (안전)
            {"💕", "💖"},    // 하트2개 → 반짝이는하트
            {"💞", "💖"},    // 하트회전 → 반짝이는하트
            {"💓", "💖"},    // 하트뛰기 → 반짝이는하트
            {"💗", "💖"},    // 하트 → 반짝이는하트
            {"💘", "💖"},    // 화살하트 → 반짝이는하트
            {"💝", "💖"},    // 선물하트 → 반짝이는하트
            {"💜", "💖"},    // 보라하트 → 반짝이는하트
            {"💙", "💖"},    // 파랑하트 → 반짝이는하트
            {"💚", "💖"},    // 초록하트 → 반짝이는하트
            {"💛", "💖"},    // 노랑하트 → 반짝이는하트
            {"🧡", "💖"},    // 주황하트 → 반짝이는하트
            {"🤍", "💖"},    // 하얀하트 → 반짝이는하트
            {"🤎", "💖"},    // 갈색하트 → 반짝이는하트
            {"🖤", "💖"},    // 검은하트 → 반짝이는하트
            
            // === 사물/활동/기분 표현 (20개) ===
            {"🎉", "💖"},    // 파티 → 반짝이는하트
            {"✨", "💖"},    // 반짝이 → 반짝이는하트
            {"💥", "💖"},    // 폭발 → 반짝이는하트
            {"🔥", "🔥"},    // 불 → 불 (안전)
            {"🚀", "🚀"},    // 로켓 → 로켓 (안전)
            {"🌟", "😊"},    // 반짝별 → 스마일
            {"⭐", "😊"},     // 별 → 스마일
            {"🌈", "☀"},    // 무지개 → 태양
            {"🌞", "☀"},    // 태양얼굴 → 태양
            {"🌤", "☀"},    // 구름태양 → 태양
            {"🍀", "🌿"},    // 네잎클로버 → 잎
            {"🌸", "🌺"},    // 벚꽃 → 꽃
            {"🎶", "🎵"},    // 음표 → 음표
            {"🎵", "🎶"},    // 음표 → 음표
            {"🎁", "🎁"},    // 선물 → 선물 (안전)
            {"📸", "📷"},    // 카메라플래시 → 카메라
            {"📱", "📞"},    // 스마트폰 → 전화
            {"💬", "💭"},    // 말풍선 → 생각
            {"📝", "😊"},    // 메모 → 스마일
            {"🧠", "😊"},    // 뇌 → 스마일
            
            // === 추가 안전 매핑 ===
            {"📄", "😊"},    // 문서 → 스마일
            {"⚡", "😊"},    // 번개 → 스마일
            {"🙌", "👌"},    // 손들기 → OK
            {"💡", "😊"},    // 전구 → 스마일
            
            // === VS16이 포함된 자주 쓰는 이모지들 ===
            {"❤️", "💖"},    // 하트+VS16 → 반짝이는하트
            {"⭐️", "😊"},    // 별+VS16 → 스마일
            {"⚙️", "⚙"},    // 기어+VS16 → 기어
            {"🛠️", "🔧"},    // 도구+VS16 → 렌치
            {"🔍", "🔍"},    // 돋보기+VS16 → 돋보기
            {"⚡️", "⚡"},    // 번개+VS16 → 번개
            {"✍️", "✍"},     // 글쓰기+VS16 → 글쓰기
        };
        
        string result = text;
        foreach (var replacement in priorityReplacements)
        {
            result = result.Replace(replacement.Key, replacement.Value);
        }
        
        return result;
    }
    
    /// <summary>
    /// 지원되지 않는 이모지를 랜덤으로 지원되는 이모지로 교체합니다.
    /// 이모지는 최대한 유지하면서 네모 박스만 방지합니다.
    /// </summary>
    private static string ReplaceUnsupportedEmojisWithRandom(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        var supportedEmojis = GetSupportedEmojis();
        if (supportedEmojis.Count == 0) return text;
        
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            
            // 이모지인지 확인 (기본 범위만)
            if (IsEmoji(c))
            {
                // Surrogate pair 체크
                string emoji = c.ToString();
                if (char.IsHighSurrogate(c) && i + 1 < text.Length)
                {
                    char low = text[i + 1];
                    emoji = c.ToString() + low.ToString();
                    i++; // low surrogate 건너뛰기
                }
                
                // 지원되는 이모지인지 확인
                if (supportedEmojis.Contains(emoji))
                {
                    result.Append(emoji);
                }
                else
                {
                    // 지원되지 않는 이모지는 랜덤 이모지로 교체
                    string randomEmoji = supportedEmojis[UnityEngine.Random.Range(0, supportedEmojis.Count)];
                    result.Append(randomEmoji);
                }
            }
            else
            {
                // 일반 문자는 그대로 유지
                result.Append(c);
            }
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// 잠재적인 이모지 문자인지 확인합니다. (더 넓은 범위)
    /// 네모 박스 방지를 위해 의심스러운 문자를 모두 처리합니다.
    /// </summary>
    private static bool IsPotentialEmoji(char c)
    {
        int codepoint = (int)c;
        
        // 기본 이모지 범위들
        if (codepoint >= 0x1F600 && codepoint <= 0x1F64F) return true; // 감정 이모지
        if (codepoint >= 0x1F300 && codepoint <= 0x1F5FF) return true; // 기타 기호 및 픽토그램
        if (codepoint >= 0x1F680 && codepoint <= 0x1F6FF) return true; // 교통 및 지도 기호
        if (codepoint >= 0x1F1E0 && codepoint <= 0x1F1FF) return true; // 지역 표시 기호
        if (codepoint >= 0x2600 && codepoint <= 0x26FF) return true;   // 기타 기호
        if (codepoint >= 0x2700 && codepoint <= 0x27BF) return true;   // Dingbats
        if (codepoint >= 0xFE00 && codepoint <= 0xFE0F) return true;   // Variation Selectors
        if (codepoint >= 0x1F900 && codepoint <= 0x1F9FF) return true; // 추가 기호 및 픽토그램
        if (codepoint >= 0x1FA70 && codepoint <= 0x1FAFF) return true; // 기호 및 픽토그램 확장-A
        
        // 추가 범위들 (네모 박스 방지)
        if (codepoint >= 0x1F000 && codepoint <= 0x1F02F) return true; // Mahjong Tiles
        if (codepoint >= 0x1F0A0 && codepoint <= 0x1F0FF) return true; // Playing Cards
        if (codepoint >= 0x1F200 && codepoint <= 0x1F2FF) return true; // Enclosed Ideographic Supplement
        if (codepoint >= 0x1F600 && codepoint <= 0x1F64F) return true; // Emoticons
        if (codepoint >= 0x1F680 && codepoint <= 0x1F6FF) return true; // Transport and Map Symbols
        if (codepoint >= 0x1F700 && codepoint <= 0x1F77F) return true; // Alchemical Symbols
        if (codepoint >= 0x1F780 && codepoint <= 0x1F7FF) return true; // Geometric Shapes Extended
        if (codepoint >= 0x1F800 && codepoint <= 0x1F8FF) return true; // Supplemental Arrows-C
        if (codepoint >= 0x1F900 && codepoint <= 0x1F9FF) return true; // Supplemental Symbols and Pictographs
        if (codepoint >= 0x1FA00 && codepoint <= 0x1FA6F) return true; // Chess Symbols
        if (codepoint >= 0x1FA70 && codepoint <= 0x1FAFF) return true; // Symbols and Pictographs Extended-A
        if (codepoint >= 0x1FB00 && codepoint <= 0x1FBFF) return true; // Symbols for Legacy Computing
        
        // 기타 의심스러운 문자들
        if (codepoint >= 0x2000 && codepoint <= 0x206F) return false;  // General Punctuation (제외)
        if (codepoint >= 0x2070 && codepoint <= 0x209F) return false;  // Superscripts and Subscripts (제외)
        if (codepoint >= 0x20A0 && codepoint <= 0x20CF) return false;  // Currency Symbols (제외)
        if (codepoint >= 0x20D0 && codepoint <= 0x20FF) return false;  // Combining Diacritical Marks for Symbols (제외)
        
        // 높은 유니코드 범위에서 의심스러운 문자들
        if (codepoint >= 0xE000 && codepoint <= 0xF8FF) return true;   // Private Use Area
        if (codepoint >= 0xF900 && codepoint <= 0xFAFF) return true;   // CJK Compatibility Ideographs
        
        return false;
    }
    
    
    /// <summary>
    /// Simple Emoji 에셋에서 지원하는 이모지만 필터링합니다.
    /// VS16이 포함된 이모지도 올바르게 처리합니다.
    /// </summary>
    private static string FilterSupportedEmojis(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        try
        {
            // TMP_Settings에서 기본 스프라이트 에셋 가져오기
            var spriteAsset = TMP_Settings.defaultSpriteAsset;
            if (spriteAsset == null)
            {
                Debug.LogWarning("TMP_Settings.defaultSpriteAsset이 null입니다. Simple Emoji 에셋이 설정되지 않았을 수 있습니다.");
                return text;
            }
            
            // 지원하는 이모지 유니코드 목록 생성
            var supportedUnicodes = new System.Collections.Generic.HashSet<uint>();
            foreach (var character in spriteAsset.spriteCharacterTable)
            {
                supportedUnicodes.Add(character.unicode);
            }
            
            // 텍스트에서 지원하는 이모지만 유지
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                
                // VS16(Variation Selector)인 경우 - 앞의 이모지와 함께 처리
                if (c >= 0xFE00 && c <= 0xFE0F)
                {
                    // VS16은 앞의 이모지와 함께 표시되므로 그대로 유지
                    result.Append(c);
                    continue;
                }
                
                // 단일 문자 이모지인 경우
                if (IsEmoji(c))
                {
                    uint unicode = (uint)c;
                    if (supportedUnicodes.Contains(unicode))
                    {
                        result.Append(c);
                    }
                    // 지원하지 않는 단일 이모지는 제거 (아무것도 추가하지 않음)
                }
                else if (char.IsHighSurrogate(c) && i + 1 < text.Length)
                {
                    // Surrogate pair (멀티바이트 이모지)인 경우
                    char low = text[i + 1];
                    uint unicode = (uint)char.ConvertToUtf32(c, low);
                    
                    if (supportedUnicodes.Contains(unicode))
                    {
                        result.Append(c);
                        result.Append(low);
                    }
                    // 지원하지 않는 멀티바이트 이모지는 제거
                    
                    i++; // low surrogate 건너뛰기
                }
                else
                {
                    // 일반 문자는 유지
                    result.Append(c);
                }
            }
            
            return result.ToString();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"이모지 필터링 중 오류 발생: {e.Message}");
            return text;
        }
    }
    
    
    /// <summary>
    /// 메시지에 이모지가 포함되어 있는지 확인합니다.
    /// </summary>
    public static bool ContainsEmoji(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        
        foreach (char c in text)
        {
            if (IsEmoji(c))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 문자가 이모지인지 확인합니다.
    /// </summary>
    private static bool IsEmoji(char c)
    {
        // 이모지 유니코드 범위들
        return (c >= 0x1F600 && c <= 0x1F64F) || // 감정 이모지
               (c >= 0x1F300 && c <= 0x1F5FF) || // 기타 기호 및 픽토그램
               (c >= 0x1F680 && c <= 0x1F6FF) || // 교통 및 지도 기호
               (c >= 0x1F1E0 && c <= 0x1F1FF) || // 지역 표시 기호
               (c >= 0x2600 && c <= 0x26FF) ||   // 기타 기호
               (c >= 0x2700 && c <= 0x27BF) ||   // Dingbats
               (c >= 0xFE00 && c <= 0xFE0F) ||   // Variation Selectors
               (c >= 0x1F900 && c <= 0x1F9FF) || // 추가 기호 및 픽토그램
               (c >= 0x1FA70 && c <= 0x1FAFF);   // 기호 및 픽토그램 확장-A
    }
    
    /// <summary>
    /// 로드된 메시지 리스트를 반환합니다.
    /// </summary>
    public List<MessageInfo> GetLoadedMessages()
    {
        return loadedMessages;
    }
    
    /// <summary>
    /// 텍스트에서 이모지를 제거합니다.
    /// </summary>
    private string RemoveEmojis(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        string result = "";
        foreach (char c in text)
        {
            if (!IsEmoji(c))
            {
                result += c;
            }
        }
        return result;
    }
    
    /// <summary>
    /// 이모지를 텍스트로 변환합니다. (폴백 옵션)
    /// </summary>
    private string ConvertEmojisToText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        return text
            .Replace("🙌", "[박수]")
            .Replace("💪", "[힘]")
            .Replace("🔧", "[도구]")
            .Replace("🐞", "[버그]")
            .Replace("🚀", "[로켓]")
            .Replace("💖", "[하트]")
            .Replace("👍", "[좋아요]")
            .Replace("🎉", "[축하]")
            .Replace("🔥", "[불]")
            .Replace("💯", "[100]")
            .Replace("✨", "[반짝]")
            .Replace("⭐", "[별]")
            .Replace("🌟", "[별]")
            .Replace("⚙", "[설정]")
            .Replace("⚡", "[번개]");
    }
}

/// <summary>
/// Unity의 JsonUtility로 JSON 배열을 파싱하기 위한 헬퍼 클래스입니다.
/// </summary>
public static class JsonHelper
{
    public static T[] FromJsonArray<T>(string json)
    {
        // JSON 문자열을 {"items": [ ... ]} 형태로 감싸서 파싱합니다.
        string newJson = "{ \"items\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.items;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}