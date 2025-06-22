using UnityEngine;
using UnityEngine.UI;

public enum TextLengthMode
{
    Under10,        // 10글자 미만
    Max20,          // 10글자~20글자
    Max35,          // 21글자~35글자
    Over35          // 35글자 초과
}

public class TextMessageManager : MonoBehaviour
{
    [Header("이름 텍스트 컴포넌트 (12개)")]
    public Text[] nameTexts = new Text[12];       // 이름을 표시할 Text 컴포넌트들 (12개)
    
    [Header("내용 텍스트 컴포넌트 (12개)")]
    public Text[] contentTexts = new Text[12];    // 내용을 표시할 Text 컴포넌트들 (12개)
    
    [Header("텍스트 길이 모드별 게임오브젝트 (4개)")]
    public GameObject under10ModeObject;          // Under10 모드일 때 활성화할 오브젝트
    public GameObject max20ModeObject;            // Max20 모드일 때 활성화할 오브젝트
    public GameObject max35ModeObject;            // Max35 모드일 때 활성화할 오브젝트
    public GameObject over35ModeObject;           // Over35 모드일 때 활성화할 오브젝트
    
    [Header("현재 상태")]
    public TextLengthMode currentMode;
    public string currentName;
    public string currentContent;
    
    /// <summary>
    /// 외부에서 호출하는 텍스트 설정 함수
    /// </summary>
    /// <param name="name">이름 텍스트</param>
    /// <param name="content">내용 텍스트</param>
    public void SetTextMessage(string name, string content)
    {
        // 입력값 저장
        currentName = name;
        currentContent = content;
        
        // 내용 텍스트 길이에 따라 모드 결정
        currentMode = DetermineTextLengthMode(content);
        
        // UI 업데이트
        UpdateUI();
        
        // 디버그 로그
        Debug.Log($"텍스트 설정 완료 - 이름: {name}, 내용: {content} ({content.Length}글자), 모드: {currentMode}");
    }
    
    /// <summary>
    /// 텍스트 길이에 따라 모드를 결정하는 함수
    /// </summary>
    /// <param name="content">내용 텍스트</param>
    /// <returns>텍스트 길이 모드</returns>
    private TextLengthMode DetermineTextLengthMode(string content)
    {
        if (string.IsNullOrEmpty(content))
            return TextLengthMode.Under10;
            
        int length = content.Length;
        
        if (length < 10)
            return TextLengthMode.Under10;
        else if (length >= 10 && length <= 20)
            return TextLengthMode.Max20;
        else if (length >= 21 && length <= 35)
            return TextLengthMode.Max35;
        else // 35글자 초과
            return TextLengthMode.Over35;
    }
    
    /// <summary>
    /// UI를 업데이트하는 함수
    /// </summary>
    private void UpdateUI()
    {
        // 모든 이름 텍스트 업데이트
        for (int i = 0; i < nameTexts.Length; i++)
        {
            if (nameTexts[i] != null)
            {
                nameTexts[i].text = currentName;
            }
        }
        
        // 모든 내용 텍스트 업데이트
        for (int i = 0; i < contentTexts.Length; i++)
        {
            if (contentTexts[i] != null)
            {
                contentTexts[i].text = currentContent;
            }
        }
        
        // 모드에 따라 폰트 크기 조정
        AdjustFontSizeByMode();
        
        // 모드에 따라 오브젝트 활성화/비활성화
        ActivateModeObject();
    }
    
    /// <summary>
    /// 모드에 따라 폰트 크기를 조정하는 함수
    /// </summary>
    private void AdjustFontSizeByMode()
    {
        int fontSize = GetFontSizeByMode();
        
        // 모든 내용 텍스트의 폰트 크기 조정
        for (int i = 0; i < contentTexts.Length; i++)
        {
            if (contentTexts[i] != null)
            {
                contentTexts[i].fontSize = fontSize;
            }
        }
    }
    
    /// <summary>
    /// 현재 모드에 따른 폰트 크기 반환
    /// </summary>
    /// <returns>폰트 크기</returns>
    private int GetFontSizeByMode()
    {
        switch (currentMode)
        {
            case TextLengthMode.Under10:
                return 50;
            case TextLengthMode.Max20:
                return 45;
            case TextLengthMode.Max35:
                return 40;
            case TextLengthMode.Over35:
                return 35;
            default:
                return 50;
        }
    }
    
    /// <summary>
    /// 현재 모드에 따라 해당 오브젝트만 활성화하는 함수
    /// </summary>
    private void ActivateModeObject()
    {
        // 모든 모드 오브젝트 비활성화
        DeactivateAllModeObjects();
        
        // 현재 모드에 해당하는 오브젝트만 활성화
        switch (currentMode)
        {
            case TextLengthMode.Under10:
                if (under10ModeObject != null)
                    under10ModeObject.SetActive(true);
                break;
            case TextLengthMode.Max20:
                if (max20ModeObject != null)
                    max20ModeObject.SetActive(true);
                break;
            case TextLengthMode.Max35:
                if (max35ModeObject != null)
                    max35ModeObject.SetActive(true);
                break;
            case TextLengthMode.Over35:
                if (over35ModeObject != null)
                    over35ModeObject.SetActive(true);
                break;
        }
        
        Debug.Log($"모드 오브젝트 활성화: {currentMode}");
    }
    
    /// <summary>
    /// 모든 모드 오브젝트를 비활성화하는 함수
    /// </summary>
    private void DeactivateAllModeObjects()
    {
        if (under10ModeObject != null) under10ModeObject.SetActive(false);
        if (max20ModeObject != null) max20ModeObject.SetActive(false);
        if (max35ModeObject != null) max35ModeObject.SetActive(false);
        if (over35ModeObject != null) over35ModeObject.SetActive(false);
    }
    

    
    /// <summary>
    /// 현재 모드 정보를 문자열로 반환
    /// </summary>
    /// <returns>모드 설명 문자열</returns>
    public string GetCurrentModeDescription()
    {
        switch (currentMode)
        {
            case TextLengthMode.Under10:
                return "짧은 텍스트 (10글자 미만)";
            case TextLengthMode.Max20:
                return "중간 텍스트 (10-20글자)";
            case TextLengthMode.Max35:
                return "긴 텍스트 (21-35글자)";
            case TextLengthMode.Over35:
                return "매우 긴 텍스트 (35글자 초과)";
            default:
                return "알 수 없음";
        }
    }
    

    
    /// <summary>
    /// 할당된 텍스트 컴포넌트 개수 확인
    /// </summary>
    [ContextMenu("할당된 컴포넌트 개수 확인")]
    public void CheckAssignedComponents()
    {
        int assignedNameTexts = 0;
        int assignedContentTexts = 0;
        
        for (int i = 0; i < nameTexts.Length; i++)
        {
            if (nameTexts[i] != null) assignedNameTexts++;
        }
        
        for (int i = 0; i < contentTexts.Length; i++)
        {
            if (contentTexts[i] != null) assignedContentTexts++;
        }
        
        Debug.Log($"할당된 이름 텍스트: {assignedNameTexts}/{nameTexts.Length}");
        Debug.Log($"할당된 내용 텍스트: {assignedContentTexts}/{contentTexts.Length}");
        
        // 모드 오브젝트 할당 상태도 확인
        Debug.Log($"Under10 모드 오브젝트: {(under10ModeObject != null ? "할당됨" : "미할당")}");
        Debug.Log($"Max20 모드 오브젝트: {(max20ModeObject != null ? "할당됨" : "미할당")}");
        Debug.Log($"Max35 모드 오브젝트: {(max35ModeObject != null ? "할당됨" : "미할당")}");
        Debug.Log($"Over35 모드 오브젝트: {(over35ModeObject != null ? "할당됨" : "미할당")}");
        
    }
    
    /// <summary>
    /// OnValidate 함수 - Inspector에서 값이 변경될 때마다 호출되어 테스트 가능
    /// </summary>
    void OnValidate()
    {
        // 게임이 실행 중일 때만 작동
        if (Application.isPlaying)
        {
            ActivateModeObject();
            Debug.Log($"OnValidate: 현재 모드 {currentMode}에 맞는 오브젝트 활성화");
        }
    }
    
    /// <summary>
    /// 테스트용 함수 - 인스펙터에서 테스트 가능
    /// </summary>
    [ContextMenu("테스트 - Under10 모드")]
    public void TestUnder10Text()
    {
        SetTextMessage("김성은", "안녕하세요");
    }
    
    [ContextMenu("테스트 - Max20 모드")]
    public void TestMax20Text()
    {
        SetTextMessage("김성은", "안녕하세요 반갑습니다 좋은 하루 되세요");
    }
    
    [ContextMenu("테스트 - Max35 모드")]
    public void TestMax35Text()
    {
        SetTextMessage("김성은", "마바사아자차가타파하나다라마바사아자차가타파하나다라");
    }
    
    [ContextMenu("테스트 - Over35 모드")]
    public void TestOver35Text()
    {
        SetTextMessage("김성은", "마바사아자차가타파하나다라마바사아자차가타파하나다라마바사아자차가타파하나다라");
    }
} 