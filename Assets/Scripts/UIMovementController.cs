using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum StateOrderMode
{
    Sequential,  // 순서대로 (N -> D -> C)
    Random       // 랜덤하게
}

public class UIMovementController : MonoBehaviour
{
    [Header("State 순서 설정")]
    public StateOrderMode stateOrderMode = StateOrderMode.Sequential;  // State 순서 모드
    
    [Header("이동 설정")]
    public float moveSpeed = 200f;           // 이동 속도 (픽셀/초)
    public Vector2 moveDirection = Vector2.right;  // 이동 방향 (오른쪽)
    public float destroyDistance = 800f;     // 파괴 거리 (백업)
    public float destroyTime = 5f;           // 제거 시간 (초)
    public bool useDistanceDestroy = false;  // 거리 기반 제거 사용 여부
    
    [Header("생성 설정")]
    public GameObject uiPrefab;              // 생성할 UI 프리팹
    public float spawnInterval = 0.5f;       // 생성 간격 (초)
    
    [Header("텍스트 설정")]
    public string nameText = "이름";
    public string contentText = "내용입니다.";
    
    [Header("JsonLoader 연동 설정")]
    public List<MessageInfo> assignedMessages = new List<MessageInfo>(); // 할당된 메시지 목록
    public int currentMessageIndex = 0; // 현재 사용할 메시지 인덱스
    public bool useAssignedMessages = false; // 할당된 메시지 사용 여부
    public bool isManagedByJsonLoader = false; // JsonLoader에서 관리 중인지 여부
    
    [Header("현재 상태")]
    public bool isSpawning = false;          // 생성 중인지 여부
    
    private float lastSpawnTime;
    private int spawnedCount = 0;
    private int currentStateIndex = 0;       // 현재 State 인덱스 (0=N, 1=D, 2=C)
    private State[] stateSequence = { State.N, State.D, State.C }; // State 순서
    private List<State> usedStates = new List<State>(); // 랜덤 모드에서 사용된 State들 추적
    
    void Start()
    {
        lastSpawnTime = Time.time;
        
        // 현재 설정 출력
        Debug.Log($"현재 설정 - 제거시간: {destroyTime}초, 이동속도: {moveSpeed}, 이동방향: {moveDirection}");
    }
    
    /// <summary>
    /// Inspector에서 값이 변경될 때 호출
    /// </summary>
    void OnValidate()
    {
        Debug.Log($"설정 변경됨 - 제거시간: {destroyTime}초, 이동속도: {moveSpeed}, 이동방향: {moveDirection}");
    }
    
    void Update()
    {
        // JsonLoader에서 관리 중이 아닐 때만 스페이스바 입력 처리
        if (!isManagedByJsonLoader)
        {
            // 스페이스바 입력 감지
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpawnUIObject();
                Debug.Log("스페이스바 입력: UI 객체 생성");
            }
        }
        
        if (isSpawning)
        {
            // 생성 간격 체크
            if (Time.time - lastSpawnTime >= spawnInterval)
            {
                SpawnUIObject();
                lastSpawnTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// UI 객체 생성
    /// </summary>
    public void SpawnUIObject()
    {
        if (uiPrefab == null)
        {
            Debug.LogWarning("UI 프리팹이 할당되지 않았습니다!");
            return;
        }
        
        // 생성 위치는 이 컨트롤러의 위치로 고정합니다.
        Vector3 spawnPosition = transform.position;
        
        // UI 객체 생성 (부모를 이 스크립트가 있는 오브젝트로 설정)
        GameObject newUIObject = Instantiate(uiPrefab, spawnPosition, Quaternion.identity, transform);
        
        // 이동 컴포넌트 추가
        UIMover mover = newUIObject.GetComponent<UIMover>();
        if (mover == null)
        {
            mover = newUIObject.AddComponent<UIMover>();
        }
        
        // 이동 설정
        mover.Initialize(moveSpeed, moveDirection, destroyDistance, destroyTime, useDistanceDestroy);
        
        // TextController 설정
        SetupTextController(newUIObject);
        
        spawnedCount++;
        Debug.Log($"UI 객체 생성 완료: {spawnedCount}번째 (위치: {transform.position})");
    }
    
    /// <summary>
    /// TextController 설정
    /// </summary>
    private void SetupTextController(GameObject uiObject)
    {
        // 생성된 프리팹(uiObject)에서 TextController 컴포넌트를 직접 찾습니다.
        TextController textController = uiObject.GetComponent<TextController>();
        
        if (textController != null)
        {
            // 1. 선택된 모드에 따라 다음 State를 결정합니다.
            State nextState;
            
            switch (stateOrderMode)
            {
                case StateOrderMode.Sequential:
                    // 순서대로 (N -> D -> C)
                    nextState = stateSequence[currentStateIndex];
                    currentStateIndex = (currentStateIndex + 1) % stateSequence.Length;
                    break;
                    
                case StateOrderMode.Random:
                    // 랜덤하게
                    nextState = GetRandomState();
                    break;
                    
                default:
                    nextState = State.N;
                    break;
            }
            
            // 2. TextController의 State를 먼저 설정합니다.
            //    SetText가 이 State를 사용해서 올바른 자식 오브젝트를 활성화합니다.
            textController.currentState = nextState;

            // 3. 메시지 텍스트 결정
            string finalNameText = nameText;
            string finalContentText = contentText;
            
            if (useAssignedMessages && assignedMessages != null && assignedMessages.Count > 0)
            {
                // 할당된 메시지 리스트의 첫 번째 메시지를 사용합니다.
                MessageInfo selectedMessage = assignedMessages[0];
                finalNameText = selectedMessage.name;
                finalContentText = selectedMessage.content;
                
                Debug.Log($"할당된 메시지 사용: '{selectedMessage.name}' - '{selectedMessage.content}'");
            }

            // 4. 이름과 내용을 설정합니다. 이 메서드는 글자 수에 맞는 모드를 자동으로 설정하고,
            //    State에 맞는 화면을 갱신하는 것까지 처리합니다.
            textController.SetText(finalNameText, finalContentText);
            
            Debug.Log($"TextController 설정 완료: '{uiObject.name}'에 이름='{finalNameText}', 내용='{finalContentText}', State='{nextState}' (모드: {stateOrderMode})를 설정했습니다.");
        }
        else
        {
            // 이 로그는 프리팹에 TextController가 없을 경우에만 나타납니다.
            Debug.LogWarning($"MESSAGE_PREFAB에 TextController 컴포넌트가 없습니다: {uiObject.name}");
        }
    }
    
    /// <summary>
    /// 생성 시작
    /// </summary>
    [ContextMenu("생성 시작")]
    public void StartSpawning()
    {
        isSpawning = true;
        Debug.Log("UI 객체 생성 시작");
    }
    
    /// <summary>
    /// 생성 중지
    /// </summary>
    [ContextMenu("생성 중지")]
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("UI 객체 생성 중지");
    }
    
    /// <summary>
    /// 즉시 하나 생성
    /// </summary>
    [ContextMenu("즉시 생성")]
    public void SpawnNow()
    {
        SpawnUIObject();
    }
    
    /// <summary>
    /// 모든 생성된 객체 파괴
    /// </summary>
    [ContextMenu("모든 객체 파괴")]
    public void DestroyAllObjects()
    {
        UIMover[] movers = FindObjectsOfType<UIMover>();
        foreach (UIMover mover in movers)
        {
            if (mover != null)
            {
                DestroyImmediate(mover.gameObject);
            }
        }
        spawnedCount = 0;
        Debug.Log("모든 UI 객체 파괴 완료");
    }
    
    /// <summary>
    /// 기존 오브젝트들의 설정 업데이트
    /// </summary>
    [ContextMenu("기존 오브젝트 설정 업데이트")]
    public void UpdateExistingObjects()
    {
        UIMover[] movers = FindObjectsOfType<UIMover>();
        int updatedCount = 0;
        
        foreach (UIMover mover in movers)
        {
            if (mover != null)
            {
                mover.UpdateSettings(moveSpeed, moveDirection, destroyDistance, destroyTime, useDistanceDestroy);
                updatedCount++;
            }
        }
        
        Debug.Log($"{updatedCount}개의 기존 오브젝트 설정 업데이트 완료");
    }
    
    /// <summary>
    /// 설정 초기화
    /// </summary>
    [ContextMenu("설정 초기화")]
    public void ResetSettings()
    {
        spawnInterval = 0.5f;
        moveSpeed = 200f;
        moveDirection = Vector2.right;
        destroyDistance = 800f;
        destroyTime = 5f;
        Debug.Log("설정 초기화 완료");
    }
    
    /// <summary>
    /// 스페이스바 테스트 안내
    /// </summary>
    [ContextMenu("스페이스바 테스트 안내")]
    public void ShowSpacebarTestInfo()
    {
        Debug.Log("=== 스페이스바 테스트 안내 ===");
        Debug.Log("1. 게임을 실행하세요");
        Debug.Log("2. 스페이스바를 누르면 UI 객체가 생성됩니다");
        Debug.Log("3. 생성된 객체는 오른쪽으로 이동합니다");
        Debug.Log($"4. {destroyTime}초 후 자동으로 제거됩니다");
        Debug.Log("5. UI Prefab이 할당되어 있는지 확인하세요");
        Debug.Log("6. Console에서 제거 로그를 확인할 수 있습니다");
        Debug.Log("================================");
    }
    
    /// <summary>
    /// 현재 설정 확인
    /// </summary>
    [ContextMenu("현재 설정 확인")]
    public void ShowCurrentSettings()
    {
        Debug.Log("=== 현재 설정 ===");
        Debug.Log($"제거 시간: {destroyTime}초");
        Debug.Log($"이동 속도: {moveSpeed}픽셀/초");
        Debug.Log($"이동 방향: {moveDirection}");
        Debug.Log($"파괴 거리: {destroyDistance}픽셀");
        Debug.Log($"생성 간격: {spawnInterval}초");
        Debug.Log($"UI 프리팹: {(uiPrefab != null ? uiPrefab.name : "할당되지 않음")}");
        
        // State 정보를 안전하게 가져오기
        string stateInfo = "N/A";
        if (stateSequence != null && stateSequence.Length > 0)
        {
            stateInfo = stateSequence[currentStateIndex % stateSequence.Length].ToString();
        }
        Debug.Log($"현재 State: {stateInfo} (모드: {stateOrderMode})");

        Debug.Log($"할당된 메시지 사용: {(useAssignedMessages ? "활성화" : "비활성화")}");
        Debug.Log($"JsonLoader 관리: {(isManagedByJsonLoader ? "활성화" : "비활성화")}");
        Debug.Log("==================");
    }
    
    /// <summary>
    /// State 인덱스 리셋
    /// </summary>
    [ContextMenu("State 인덱스 리셋")]
    public void ResetStateIndex()
    {
        currentStateIndex = 0;
        usedStates.Clear(); // 랜덤 모드 리스트도 초기화
        
        string nextStateInfo = stateSequence.Length > 0 ? stateSequence[0].ToString() : "N/A";
        Debug.Log($"State 인덱스 리셋 완료. 다음 순차 State는 '{nextStateInfo}'이며, 랜덤 모드 리스트도 초기화되었습니다.");
    }
    
    /// <summary>
    /// 할당된 메시지 정보 확인
    /// </summary>
    [ContextMenu("할당된 메시지 정보 확인")]
    public void ShowAssignedMessagesInfo()
    {
        Debug.Log($"=== 할당된 메시지 정보 ===");
        Debug.Log($"할당된 메시지 사용: {useAssignedMessages}");
        Debug.Log($"할당된 메시지 개수: {assignedMessages?.Count ?? 0}");
        Debug.Log($"현재 메시지 인덱스: {currentMessageIndex}");
        
        if (assignedMessages != null && assignedMessages.Count > 0)
        {
            for (int i = 0; i < assignedMessages.Count; i++)
            {
                var message = assignedMessages[i];
                Debug.Log($"메시지 {i}: '{message.name}' - '{message.content}'");
            }
        }
        else
        {
            Debug.Log("할당된 메시지가 없습니다.");
        }
        Debug.Log("==========================");
    }
    
    private State GetRandomState()
    {
        // 모든 State를 사용했으면 리스트 초기화
        if (usedStates.Count >= stateSequence.Length)
        {
            usedStates.Clear();
            Debug.Log("모든 State를 사용했으므로 리스트를 초기화합니다.");
        }
        
        State randomState;
        do
        {
            randomState = stateSequence[Random.Range(0, stateSequence.Length)];
        } while (usedStates.Contains(randomState));
        
        usedStates.Add(randomState);
        return randomState;
    }
}

/// <summary>
/// UI 객체 이동을 담당하는 컴포넌트
/// </summary>
public class UIMover : MonoBehaviour
{
    private float speed;
    private Vector2 direction;
    private float destroyDistance;
    private float destroyTime;           // 제거 시간
    private bool useDistanceDestroy;     // 거리 기반 제거 사용 여부
    private Vector2 startPosition;
    private RectTransform rectTransform;
    private float spawnTime;             // 생성 시간
    
    /// <summary>
    /// 이동 설정 초기화
    /// </summary>
    public void Initialize(float moveSpeed, Vector2 moveDirection, float destroyDist, float destroyTimeParam, bool useDistanceDestroyParam)
    {
        Debug.Log($"Initialize 호출됨 - 파라미터: moveSpeed={moveSpeed}, destroyTimeParam={destroyTimeParam}, useDistanceDestroy={useDistanceDestroyParam}");
        
        speed = moveSpeed;
        direction = moveDirection.normalized;
        destroyDistance = destroyDist;
        destroyTime = destroyTimeParam;      // 파라미터 값을 올바르게 할당
        useDistanceDestroy = useDistanceDestroyParam;
        startPosition = GetComponent<RectTransform>().anchoredPosition;
        rectTransform = GetComponent<RectTransform>();
        spawnTime = Time.time;               // 생성 시간 기록
        
        Debug.Log($"UIMover 초기화 완료: 속도={speed}, 방향={direction}, 제거시간={destroyTime}초, 거리제거={useDistanceDestroy}, 시작시간={spawnTime}");
    }
    
    /// <summary>
    /// 설정만 업데이트 (생성 시간은 유지)
    /// </summary>
    public void UpdateSettings(float moveSpeed, Vector2 moveDirection, float destroyDist, float destroyTimeParam, bool useDistanceDestroyParam)
    {
        Debug.Log($"UpdateSettings 호출됨 - 파라미터: moveSpeed={moveSpeed}, destroyTimeParam={destroyTimeParam}, useDistanceDestroy={useDistanceDestroyParam}");
        
        speed = moveSpeed;
        direction = moveDirection.normalized;
        destroyDistance = destroyDist;
        destroyTime = destroyTimeParam;
        useDistanceDestroy = useDistanceDestroyParam;
        
        Debug.Log($"UIMover 설정 업데이트 완료: 속도={speed}, 방향={direction}, 제거시간={destroyTime}초, 거리제거={useDistanceDestroy}");
    }
    
    void Update()
    {
        if (rectTransform == null) return;
        
        // 오른쪽으로 이동
        Vector2 currentPosition = rectTransform.anchoredPosition;
        Vector2 newPosition = currentPosition + direction * speed * Time.deltaTime;
        rectTransform.anchoredPosition = newPosition;
        
        // 디버그: 이동 상태 확인 (1초마다)
        if (Time.frameCount % 60 == 0) // 60프레임마다 (약 1초마다)
        {
            Debug.Log($"이동 중: {gameObject.name} - 위치: {newPosition}, 경과시간: {Time.time - spawnTime:F1}초, 제거시간설정: {destroyTime}초");
        }
        
        // 시간 기반 제거 체크
        if (Time.time - spawnTime >= destroyTime)
        {
            Debug.Log($"시간 기반 제거: {gameObject.name} (생성 후 {destroyTime}초 경과, 실제경과: {Time.time - spawnTime:F1}초)");
            Destroy(gameObject);
            return;
        }
        
        // 거리 기반 제거 체크 (백업)
        if (useDistanceDestroy)
        {
            float distance = Vector2.Distance(startPosition, newPosition);
            if (distance >= destroyDistance)
            {
                Debug.Log($"거리 기반 제거: {gameObject.name} (이동 거리: {distance:F1}픽셀, 제한거리: {destroyDistance}픽셀)");
                Destroy(gameObject);
                return;
            }
            
            // 디버그: 거리 상태 확인 (1초마다)
            if (Time.frameCount % 60 == 0) // 60프레임마다 (약 1초마다)
            {
                Debug.Log($"거리 체크: {gameObject.name} - 현재거리: {distance:F1}픽셀, 제한거리: {destroyDistance}픽셀");
            }
        }
    }
} 