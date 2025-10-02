using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TextMode
{
    Max20,      // 10-20글자 (단일 모드)
}

public enum DesignType
{
    AType,          // 첫 번째 자인 오브젝트 활성화 (A 디자인)
    BType           // 두 번째 자인 오브젝트 활성화 (B 디자인)
}

public class TextController : MonoBehaviour
{
    [Header("현재 상태")]
    public TextMode currentMode = TextMode.Max20;  // 항상 Max20 고정
    public DesignType currentDesignType;
    public string currentName;
    public string currentContent;
    
    [Header("모드별 게임오브젝트")]
    public GameObject max20Objecta;      // 10-20글자 오브젝트
    public GameObject max20Objectb;      // 10-20글자 오브젝트

    [Header("텍스트 컴포넌트 배열")]
    public TextMeshProUGUI[] nameTexts;
    public TextMeshProUGUI[] contentTexts;
    
    [Header("폰트 설정")]
    public TMP_FontAsset koreanFont; // 한글 폰트 (기본)
    public TMP_SpriteAsset emojiSpriteAsset; // 이모지 스프라이트 에셋 (Simple Emoji)
    
    [Header("디버그 설정")]
    public bool showDebugLogs = false; // 디버그 로그 표시 여부
    
    /// <summary>
    /// 조건부 디버그 로그
    /// </summary>
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[TextController] {message}");
        }
    }

    void Start()
    {
        // 이모지 스프라이트 에셋 자동 할당
        AutoAssignEmojiSpriteAsset();
    }

    /// <summary>
    /// 이모지 스프라이트 에셋을 자동으로 할당합니다.
    /// </summary>
    private void AutoAssignEmojiSpriteAsset()
    {
        if (emojiSpriteAsset == null)
        {
            // Simple Emoji 에셋을 자동으로 찾아서 할당
            TMP_SpriteAsset[] spriteAssets = Resources.FindObjectsOfTypeAll<TMP_SpriteAsset>();
            foreach (var asset in spriteAssets)
            {
                if (asset.name == "EmojiTMP")
                {
                    emojiSpriteAsset = asset;
                    LogDebug($"이모지 스프라이트 에셋 자동 할당: {asset.name}");
                    
                    // TMP Settings에도 기본 스프라이트 에셋으로 설정
                    SetTMPDefaultSpriteAsset(asset);
                    break;
                }
            }
            
            if (emojiSpriteAsset == null)
            {
                Debug.LogError("⚠️ EmojiTMP 스프라이트 에셋을 찾을 수 없습니다! Assets/SimpleEmojiTMP/EmojiTMP.asset을 확인하세요.");
            }
        }
    }

    /// <summary>
    /// TMP Settings에서 기본 스프라이트 에셋을 설정합니다.
    /// </summary>
    private void SetTMPDefaultSpriteAsset(TMP_SpriteAsset spriteAsset)
    {
        try
        {
            // TMP Settings는 static 프로퍼티를 사용합니다
            TMP_Settings.defaultSpriteAsset = spriteAsset;
            LogDebug($"TMP Settings 기본 스프라이트 에셋 설정: {spriteAsset.name}");
        }
        catch (System.Exception e)
        {
            LogDebug($"TMP Settings 설정 중 오류: {e.Message}");
        }
    }

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
        
        Debug.Log($"TextController.SetText 호출됨: 이름='{name}', 내용='{content}', 디자인='{currentDesignType}'");
    }

    private TextMode GetTextMode(string content)
    {
        // 항상 Max20 모드 사용
        return TextMode.Max20;
    }

    private void UpdateAllTextComponents(string name, string content)
    {
        Debug.Log($"--- TextMeshPro 컴포넌트 업데이트 시작 (이모지 지원) ---");
        Debug.Log($"업데이트할 데이터: 이름='{name}', 내용='{content}'");
        
        bool hasEmoji = JsonLoader.ContainsEmoji(name) || JsonLoader.ContainsEmoji(content);
        Debug.Log($"이모지가 포함된 텍스트를 처리합니다.");

        if (nameTexts == null || nameTexts.Length == 0) { /* 이름 텍스트 배열이 비어있음 */ }
        else
        {
            for (int i = 0; i < nameTexts.Length; i++)
            {
                TextMeshProUGUI nameText = nameTexts[i];
                if (nameText != null) 
                { 
                    nameText.text = name; 
                    // 폰트 설정 적용
                    SetupFontForText(nameText);
                    Debug.Log($"  - '{nameText.gameObject.name}'의 TextMeshPro 텍스트를 '{name}'(으)로 설정했습니다.");
                }
                else { /* 널 체크 실패 */ }
            }
        }
        
        if (contentTexts == null || contentTexts.Length == 0) { /* 내용 텍스트 배열이 비어있음 */ }
        else
        {
            for (int i = 0; i < contentTexts.Length; i++)
            {
                TextMeshProUGUI contentText = contentTexts[i];
                if (contentText != null) 
                { 
                    contentText.text = content; 
                    // 폰트 설정 적용
                    SetupFontForText(contentText);
                    Debug.Log($"  - '{contentText.gameObject.name}'의 TextMeshPro 텍스트를 '{content}'(으)로 설정했습니다.");
                }
                else { /* 널 체크 실패 */ }
            }
        }
    }

    private void ActivateObject()
    {
        if (max20Objecta != null) max20Objecta.SetActive(false);
        if (max20Objectb != null) max20Objectb.SetActive(false);
        
        GameObject activeObject = null;
        if (currentDesignType == DesignType.AType)
        {
            if (max20Objecta != null) { max20Objecta.SetActive(true); activeObject = max20Objecta; }
        }
        else if (currentDesignType == DesignType.BType)
        {
            if (max20Objectb != null) { max20Objectb.SetActive(true); activeObject = max20Objectb; }
        }
        
        Debug.Log($"오브젝트 활성화: 디자인='{currentDesignType}', 활성 오브젝트='{activeObject?.name ?? "None"}'");
    }

    void OnValidate()
    {
        currentMode = TextMode.Max20;
        if (max20Objecta != null || max20Objectb != null)
        {
            ActivateObject();
        }
        Debug.Log($"OnValidate: 모드='{currentMode}', 디자인='{currentDesignType}'");
    }

    /// <summary>
    /// TextMeshPro 컴포넌트에 폰트 및 이모지 스프라이트 설정을 적용합니다.
    /// </summary>
    private void SetupFontForText(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;
        
        // 기본 폰트 설정 (한글 지원)
        if (koreanFont != null)
        {
            textComponent.font = koreanFont;
        }
        
        // 이모지 스프라이트 에셋 설정 (Simple Emoji)
        if (emojiSpriteAsset != null)
        {
            textComponent.spriteAsset = emojiSpriteAsset;
            Debug.Log($"[{textComponent.gameObject.name}] 이모지 스프라이트 '{emojiSpriteAsset.name}' 설정 완료");
        }
        else
        {
            Debug.LogWarning($"[{textComponent.gameObject.name}] 이모지 스프라이트 에셋이 할당되지 않았습니다!");
        }
    }

    [ContextMenu("TextMeshPro 컴포넌트 상태 확인")]
    public void CheckTextMeshProStatus() 
    {
        Debug.Log("=== TextMeshPro 컴포넌트 상태 확인 ===");
        Debug.Log($"Name Texts 배열: {nameTexts?.Length ?? 0}개");
        Debug.Log($"Content Texts 배열: {contentTexts?.Length ?? 0}개");
        Debug.Log($"한글 폰트: {koreanFont?.name ?? "None"}");
        Debug.Log($"이모지 스프라이트 에셋: {emojiSpriteAsset?.name ?? "None"}");
        
        if (emojiSpriteAsset == null)
        {
            Debug.LogError("⚠️ 이모지 스프라이트 에셋이 없습니다! Assets/SimpleEmojiTMP/EmojiTMP.asset을 할당하세요!");
        }
    }

    [ContextMenu("JsonLoader 메시지 이모지 확인")]
    public void CheckJsonLoaderEmojis() 
    {
        Debug.Log("=== JsonLoader 메시지 이모지 확인 ===");
        JsonLoader jsonLoader = FindObjectOfType<JsonLoader>();
        if (jsonLoader != null)
        {
            var messages = jsonLoader.GetLoadedMessages();
            int emojiCount = 0;
            foreach (var msg in messages)
            {
                if (JsonLoader.ContainsEmoji(msg.name) || JsonLoader.ContainsEmoji(msg.content))
                {
                    emojiCount++;
                }
            }
            Debug.Log($"총 {messages.Count}개 메시지 중 {emojiCount}개에 이모지 포함");
        }
    }

    [ContextMenu("이모지 지원 테스트")]
    public void TestEmojiSupport() 
    {
        Debug.Log("=== 이모지 지원 테스트 ===");
        string[] testTexts = { "테스트 😊", "좋아요 👍", "하트 ❤️" };
        foreach (string testText in testTexts)
        {
            bool hasEmoji = JsonLoader.ContainsEmoji(testText);
            Debug.Log($"텍스트: '{testText}' - 이모지 포함: {hasEmoji}");
        }
        SetText("테스터", "이모지 테스트 😊👍❤️");
    }

    [ContextMenu("이모지 스프라이트 에셋 수동 할당")]
    public void ManualAssignEmojiSpriteAsset()
    {
        AutoAssignEmojiSpriteAsset();
        if (emojiSpriteAsset != null)
        {
            Debug.Log($"✅ 이모지 스프라이트 에셋 할당 완료: {emojiSpriteAsset.name}");
        }
        else
        {
            Debug.LogError("❌ 이모지 스프라이트 에셋 할당 실패");
        }
    }

    [ContextMenu("디버그 로그 활성화")]
    public void EnableDebugLogs()
    {
        showDebugLogs = true;
        Debug.Log("✅ TextController 디버그 로그가 활성화되었습니다.");
    }

    [ContextMenu("디버그 로그 비활성화")]
    public void DisableDebugLogs()
    {
        showDebugLogs = false;
        Debug.Log("❌ TextController 디버그 로그가 비활성화되었습니다.");
    }
}
