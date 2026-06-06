using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [Header("퀘스트 정보")]
    [SerializeField] private TextMeshProUGUI questTitleTxt;
    [SerializeField] private TextMeshProUGUI questDescTxt;
    [SerializeField] private TextMeshProUGUI questConditionTxt;
    [SerializeField] private TextMeshProUGUI progressTxt;      // 2/5 형식
    [SerializeField] private TextMeshProUGUI rewardTxt;

    
    [Header("진행도")]
    [SerializeField] private Slider          progressSlider;   // Fill_Durability

    [Header("버튼")]
    [SerializeField] private Button          questButton;      // Btn_Quest
    [SerializeField] private TextMeshProUGUI questButtonTxt;   // Btn_Quest의 Text(TMP)

    private void Awake()
    {
        // Slider는 인터랙션 불필요
        progressSlider.interactable = false;

        questButton.onClick.AddListener(OnQuestButtonClick);
    }

    private void OnEnable()
    {
        if (QuestSystem.Instance == null) return;
        QuestSystem.Instance.OnQuestChanged    += RefreshUI;
        QuestSystem.Instance.OnProgressChanged += RefreshProgress;
        RefreshUI();
    }

    private void OnDisable()
    {
        if (QuestSystem.Instance == null) return;
        QuestSystem.Instance.OnQuestChanged    -= RefreshUI;
        QuestSystem.Instance.OnProgressChanged -= RefreshProgress;
    }

    private void RefreshUI()
    {
        QuestSystem quest = QuestSystem.Instance;
        if (quest?.CurrentQuest == null) return;

        questTitleTxt.text     = quest.CurrentQuest.questTitle;
        questDescTxt.text      = quest.CurrentQuest.questDescription;
        questConditionTxt.text = GetConditionText(quest.CurrentQuest);
        rewardTxt.text = $"<color=#FFD700>보상 : {quest.CurrentQuest.rewardGold.ToString()} 골드</color>";

        
        RefreshProgress();
        RefreshButton();
    }

    private void RefreshProgress()
    {
        QuestSystem quest = QuestSystem.Instance;
        if (quest?.CurrentQuest == null) return;

        int   current = quest.CurrentProgress;
        int   target  = quest.CurrentQuest.targetCount;
        float ratio   = target > 0 ? (float)current / target : 0f;

        progressSlider.value = ratio;
        progressTxt.text     = $"{current}/{target}";

        RefreshButton();
    }

    private void RefreshButton()
    {
        QuestSystem.QuestState state = QuestSystem.Instance.State;

        switch (state)
        {
            case QuestSystem.QuestState.Available:
                questButton.interactable = true;
                questButtonTxt.text      = "수락하기";
                break;

            case QuestSystem.QuestState.Active:
                questButton.interactable = false;
                questButtonTxt.text      = "진행 중";
                break;

            case QuestSystem.QuestState.Completed:
                questButton.interactable = true;
                questButtonTxt.text      = "완료";
                break;
        }
    }

    private void OnQuestButtonClick()
    {
        QuestSystem quest = QuestSystem.Instance;

        switch (quest.State)
        {
            case QuestSystem.QuestState.Available:
                quest.AcceptQuest();
                break;

            case QuestSystem.QuestState.Completed:
                quest.CompleteQuest();
                break;
        }
    }

    private string GetConditionText(QuestData data)
    {
        return data.questType switch
        {
            QuestType.CatchAnyFish =>
                $"물고기 {data.targetCount}마리 잡기",

            QuestType.CatchSpecificFish =>
                $"{data.targetFish?.fishName} {data.targetCount}마리 잡기",

            QuestType.CatchByWeight =>
                $"{data.targetWeight}kg 이상 물고기 {data.targetCount}마리 잡기",

            QuestType.CatchByZone =>
                $"{GetZoneName(data.targetZone)} 물고기 {data.targetCount}마리 잡기",

            _ => ""
        };
    }

    private string GetZoneName(int zone)
    {
        return zone switch
        {
            1 => "연안",
            2 => "심해",
            3 => "마력해역",
            _ => ""
        };
    }
}