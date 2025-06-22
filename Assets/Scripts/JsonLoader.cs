using UnityEngine;
using System; // For [Serializable]
using System.IO; // For File operations
using System.Collections.Generic; // For List<>
using System.Linq; // For Skip()
using System.Collections;

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

    // 내부 변수
    private int currentMessageIndex = 0; // 다음에 가져올 메시지 시작 인덱스
    
    void Start()
    {
        // 디버그: Start 메서드가 호출되었는지, autoRefresh의 상태가 어떤지 확인합니다.
        Debug.Log($"JsonLoader.Start() 호출됨. 자동 갱신: {autoRefresh}, 자동 생성: {autoSpawnEnabled}");

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
                        name = messageData.name,
                        content = messageData.story
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