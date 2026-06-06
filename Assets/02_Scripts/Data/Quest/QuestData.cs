using UnityEngine;

public enum QuestType
{
    CatchAnyFish,       // 아무 물고기 잡기
    CatchSpecificFish,  // 특정 물고기 잡기
    CatchByWeight,      // 특정 무게 이상 물고기 잡기
    CatchByZone         // 특정 지역 물고기 잡기
}

[CreateAssetMenu(fileName = "QuestData", menuName = "JustFishing/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("퀘스트 정보")]
    public string    questTitle;
    [TextArea]
    public string    questDescription;

    [Header("퀘스트 조건")]
    public QuestType questType;
    public FishData  targetFish;        // CatchSpecificFish
    public float     targetWeight;      // CatchByWeight (kg 이상)
    public int       targetZone;        // CatchByZone (1/2/3)
    public int       targetCount;       // 목표 수량

    [Header("보상")]
    public int       rewardGold;
    [TextArea]
    public string    rewardDescription;
}