using UnityEngine;
using UnityEngine.UI;

public enum TextMode
{
    Under10,    // 10글자 미만
    Max20,      // 10-20글자
    Max35,      // 21-35글자
    Over35      // 35글자 초과
}

public enum State
{
    N,          // 첫 번째 자식 오브젝트 활성화
    D,          // 두 번째 자식 오브젝트 활성화
    C           // 세 번째 자식 오브젝트 활성화
}

public class TextController : MonoBehaviour
{
    [Header("강제 모드 설정")]
    [SerializeField] private bool forceMax20Mode = false;  // 체크하면 무조건 10-20자용 모드 사용
    
    [Header("현재 상태")]
    public TextMode currentMode;
    public State currentState;
    public string currentName;
    public string currentContent;
    
    [Header("모드별 게임오브젝트")]
    public GameObject under10Object;    // 10글자 미만 오브젝트
    public GameObject max20Object;      // 10-20글자 오브젝트
    public GameObject max35Object;      // 21-35글자 오브젝트
    public GameObject over35Object;     // 35글자 초과 오브젝트

    [Header("텍스트 컴포넌트 배열")]
    public Text[] nameTexts;
    public Text[] contentTexts;
    
    /// <summary>
    /// 외부에서 호출하는 메인 함수
    /// </summary>
    public void SetText(string name, string content)
    {
        currentName = name;
        currentContent = content;
        
        // 이름과 내용 텍스트 배열을 업데이트합니다.
        UpdateAllTextComponents(name, content);

        // 텍스트 길이에 따라 모드 결정
        currentMode = GetTextMode(content);
        
        // 오브젝트 활성화
        ActivateObject();
        
        Debug.Log($"텍스트 설정: {name} - {content} ({content.Length}글자) - {currentMode}");
    }
    
    /// <summary>
    /// 모든 이름과 내용 Text 컴포넌트의 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateAllTextComponents(string name, string content)
    {
        Debug.Log("--- 텍스트 컴포넌트 업데이트 시작 ---");

        // 이름 텍스트들을 업데이트합니다.
        if (nameTexts == null || nameTexts.Length == 0)
        {
            Debug.LogWarning("`Name Texts` 배열이 비어있거나 할당되지 않았습니다. Inspector를 확인하세요.");
        }
        else
        {
            Debug.Log($"`Name Texts` 배열에 {nameTexts.Length}개의 컴포넌트가 있습니다. 업데이트를 시작합니다.");
            for (int i = 0; i < nameTexts.Length; i++)
            {
                Text nameText = nameTexts[i];
                if (nameText != null)
                {
                    nameText.text = name;
                    Debug.Log($"  - '{nameText.gameObject.name}'의 텍스트를 '{name}'(으)로 설정했습니다.");
                }
                else
                {
                    Debug.LogWarning($"  - `Name Texts` 배열의 {i}번 인덱스가 비어있습니다 (Null).");
                }
            }
        }

        // 내용 텍스트들을 업데이트합니다.
        if (contentTexts == null || contentTexts.Length == 0)
        {
            Debug.LogWarning("`Content Texts` 배열이 비어있거나 할당되지 않았습니다. Inspector를 확인하세요.");
        }
        else
        {
            Debug.Log($"`Content Texts` 배열에 {contentTexts.Length}개의 컴포넌트가 있습니다. 업데이트를 시작합니다.");
            for (int i = 0; i < contentTexts.Length; i++)
            {
                Text contentText = contentTexts[i];
                if (contentText != null)
                {
                    contentText.text = content;
                    Debug.Log($"  - '{contentText.gameObject.name}'의 텍스트를 '{content}'(으)로 설정했습니다.");
                }
                else
                {
                    Debug.LogWarning($"  - `Content Texts` 배열의 {i}번 인덱스가 비어있습니다 (Null).");
                }
            }
        }
        Debug.Log("--- 텍스트 컴포넌트 업데이트 완료 ---");
    }
    
    /// <summary>
    /// 텍스트 길이에 따른 모드 반환
    /// </summary>
    private TextMode GetTextMode(string text)
    {
        // 강제 Max20 모드가 활성화되어 있으면 무조건 Max20 모드 반환
        if (forceMax20Mode)
        {
            Debug.Log("강제 Max20 모드 활성화: 글자수와 관계없이 Max20 모드 사용");
            return TextMode.Max20;
        }
        
        if (string.IsNullOrEmpty(text)) return TextMode.Under10;
        
        int length = text.Length;
        
        if (length < 10) return TextMode.Under10;
        if (length <= 20) return TextMode.Max20;
        if (length <= 35) return TextMode.Max35;
        return TextMode.Over35;
    }
    
    /// <summary>
    /// 현재 모드에 맞는 오브젝트만 활성화
    /// </summary>
    private void ActivateObject()
    {
        // 모든 오브젝트 비활성화
        if (under10Object != null) under10Object.SetActive(false);
        if (max20Object != null) max20Object.SetActive(false);
        if (max35Object != null) max35Object.SetActive(false);
        if (over35Object != null) over35Object.SetActive(false);
        
        // 현재 모드 오브젝트만 활성화
        GameObject activeParentObject = null;
        
        switch (currentMode)
        {
            case TextMode.Under10:
                if (under10Object != null)
                {
                    under10Object.SetActive(true);
                    activeParentObject = under10Object;
                }
                break;
            case TextMode.Max20:
                if (max20Object != null)
                {
                    max20Object.SetActive(true);
                    activeParentObject = max20Object;
                }
                break;
            case TextMode.Max35:
                if (max35Object != null)
                {
                    max35Object.SetActive(true);
                    activeParentObject = max35Object;
                }
                break;
            case TextMode.Over35:
                if (over35Object != null)
                {
                    over35Object.SetActive(true);
                    activeParentObject = over35Object;
                }
                break;
        }
        
        // 활성화된 부모 오브젝트의 자식들을 State에 따라 제어
        if (activeParentObject != null)
        {
            ActivateChildByState(activeParentObject);
        }
    }
    
    /// <summary>
    /// State에 따라 부모 오브젝트의 자식들을 제어
    /// </summary>
    private void ActivateChildByState(GameObject parentObject)
    {
        // 모든 자식 오브젝트 비활성화
        for (int i = 0; i < parentObject.transform.childCount; i++)
        {
            parentObject.transform.GetChild(i).gameObject.SetActive(false);
        }
        
        // State에 따라 해당 자식만 활성화
        int childIndex = 0;
        switch (currentState)
        {
            case State.N:
                childIndex = 0; // 첫 번째 자식
                break;
            case State.D:
                childIndex = 1; // 두 번째 자식
                break;
            case State.C:
                childIndex = 2; // 세 번째 자식
                break;
        }
        
        // 해당 인덱스의 자식이 존재하면 활성화
        if (childIndex < parentObject.transform.childCount)
        {
            parentObject.transform.GetChild(childIndex).gameObject.SetActive(true);
            Debug.Log($"State {currentState}: {parentObject.name}의 {childIndex + 1}번째 자식 활성화");
        }
        else
        {
            Debug.LogWarning($"State {currentState}: {parentObject.name}에 {childIndex + 1}번째 자식이 없습니다!");
        }
    }
    
    /// <summary>
    /// 현재 State에 따라 화면을 즉시 갱신합니다.
    /// </summary>
    [ContextMenu("현재 State로 화면 갱신")]
    public void UpdateStateVisuals()
    {
        // State가 변경되었을 때, 해당하는 오브젝트와 자식 오브젝트의 활성화 상태를 갱신합니다.
        ActivateObject();
        Debug.Log($"화면 갱신 완료: 현재 모드({currentMode}), 현재 상태({currentState})");
    }
    
    /// <summary>
    /// Inspector에서 값 변경 시 자동 호출 (테스트용)
    /// </summary>
    void OnValidate()
    {
        // 에디터 모드에서도 작동하도록 수정
        if (!string.IsNullOrEmpty(currentContent))
        {
            // 텍스트 길이에 따라 모드 자동 결정
            currentMode = GetTextMode(currentContent);
        }
        
        // 오브젝트 활성화 (에디터 모드에서도 작동)
        ActivateObject();
        
        // 디버그 로그 (에디터 모드에서도 출력)
        if (!string.IsNullOrEmpty(currentContent))
        {
            Debug.Log($"OnValidate: {currentName} - {currentContent} ({currentContent.Length}글자) - {currentMode} - State: {currentState}");
        }
    }
    
    // ===== 테스트 함수들 =====
    
    [ContextMenu("테스트 - Under10 모드 (2글자)")]
    public void TestShort()
    {
        SetText("김성은", "안녕");
    }
    
    [ContextMenu("테스트 - Max20 모드 (12글자)")]
    public void TestMedium()
    {
        SetText("김성은", "안녕하세요 반갑습니다");
    }
    
    [ContextMenu("테스트 - Max35 모드 (25글자)")]
    public void TestLong()
    {
        SetText("김성은", "마바사아자차가타파하나다라마바사아자차가타파하나다라");
    }
    
    [ContextMenu("테스트 - Over35 모드 (50글자)")]
    public void TestVeryLong()
    {
        SetText("김성은", "마바사아자차가타파하나다라마바사아자차가타파하나다라마바사아자차가타파하나다라마바사아자차가타파하나다라");
    }
    
    [ContextMenu("모든 오브젝트 비활성화")]
    public void DeactivateAll()
    {
        if (under10Object != null) under10Object.SetActive(false);
        if (max20Object != null) max20Object.SetActive(false);
        if (max35Object != null) max35Object.SetActive(false);
        if (over35Object != null) over35Object.SetActive(false);
        
        Debug.Log("모든 오브젝트 비활성화 완료");
    }
    
    // ===== State 테스트 함수들 =====
    
    [ContextMenu("State 테스트 - N (첫 번째 자식)")]
    public void TestStateN()
    {
        currentState = State.N;
        ActivateObject();
        Debug.Log($"State 변경: {currentState} - 첫 번째 자식 활성화");
    }
    
    [ContextMenu("State 테스트 - D (두 번째 자식)")]
    public void TestStateD()
    {
        currentState = State.D;
        ActivateObject();
        Debug.Log($"State 변경: {currentState} - 두 번째 자식 활성화");
    }
    
    [ContextMenu("State 테스트 - C (세 번째 자식)")]
    public void TestStateC()
    {
        currentState = State.C;
        ActivateObject();
        Debug.Log($"State 변경: {currentState} - 세 번째 자식 활성화");
    }
} 