using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem Instance { get; private set; }

    [SerializeField] private List<QuestData> questPool;

    public QuestData  CurrentQuest    { get; private set; }
    public int        CurrentProgress { get; private set; }
    public QuestState State           { get; private set; } = QuestState.None;

    public enum QuestState { None, Available, Active, Completed }

    public event Action OnQuestChanged;
    public event Action OnProgressChanged;

    private const string KEY_QUEST_ID    = "QuestID";
    private const string KEY_QUEST_PROG  = "QuestProgress";
    private const string KEY_QUEST_STATE = "QuestState";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadQuest();
    }

    // ==================== 퀘스트 생성 ====================

    public void GenerateNewQuest()
    {
        if (questPool == null || questPool.Count == 0) return;

        QuestData newQuest = null;
        int attempts = 0;
        while (attempts < 10)
        {
            QuestData candidate = questPool[UnityEngine.Random.Range(0, questPool.Count)];
            if (candidate != CurrentQuest) { newQuest = candidate; break; }
            attempts++;
        }

        if (newQuest == null) newQuest = questPool[0];

        CurrentQuest    = newQuest;
        CurrentProgress = 0;
        State           = QuestState.Available;

        OnQuestChanged?.Invoke();
    }

    // ==================== 수락 ====================

    public void AcceptQuest()
    {
        if (State != QuestState.Available) return;
        State = QuestState.Active;
        SaveQuest();
        OnQuestChanged?.Invoke();
    }

    // ==================== 완료 ====================

    public void CompleteQuest()
    {
        if (State != QuestState.Completed) return;
        PlayerWallet.Instance?.AddGold(CurrentQuest.rewardGold);
        Debug.Log($"퀘스트 완료: {CurrentQuest.questTitle} / 보상: {CurrentQuest.rewardGold}G");
        GenerateNewQuest();
    }

    // ==================== 진행도 업데이트 ====================

    // FishingController에서 물고기 잡을 때 호출
    // fish: 잡은 물고기 인스턴스, zone: 잡은 지역
    public void OnFishCaught(FishInstance fish, int zone = 0)
    {
        if (State != QuestState.Active || CurrentQuest == null) return;

        bool isValid = CurrentQuest.questType switch
        {
            QuestType.CatchAnyFish =>
                true,

            QuestType.CatchSpecificFish =>
                fish.fishData == CurrentQuest.targetFish,

            QuestType.CatchByWeight =>
                fish.weight >= CurrentQuest.targetWeight,

            QuestType.CatchByZone =>
                zone == CurrentQuest.targetZone,

            _ => false
        };

        if (!isValid) return;

        CurrentProgress++;
        CurrentProgress = Mathf.Min(CurrentProgress, CurrentQuest.targetCount);

        if (CurrentProgress >= CurrentQuest.targetCount)
            State = QuestState.Completed;

        SaveQuest();
        OnProgressChanged?.Invoke();
    }

    // ==================== 저장/불러오기 ====================

    private void SaveQuest()
    {
        if (CurrentQuest == null) return;
        PlayerPrefs.SetString(KEY_QUEST_ID,    CurrentQuest.questTitle);
        PlayerPrefs.SetInt(KEY_QUEST_PROG,     CurrentProgress);
        PlayerPrefs.SetInt(KEY_QUEST_STATE,    (int)State);
        PlayerPrefs.Save();
    }

    private void LoadQuest()
    {
        if (!PlayerPrefs.HasKey(KEY_QUEST_STATE))
        {
            GenerateNewQuest();
            return;
        }

        State           = (QuestState)PlayerPrefs.GetInt(KEY_QUEST_STATE);
        CurrentProgress = PlayerPrefs.GetInt(KEY_QUEST_PROG, 0);

        string savedTitle = PlayerPrefs.GetString(KEY_QUEST_ID, "");
        CurrentQuest = questPool?.Find(q => q.questTitle == savedTitle);

        if (CurrentQuest == null) GenerateNewQuest();
        else                      OnQuestChanged?.Invoke();
    }
}