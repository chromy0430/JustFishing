using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "JustFishing/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string       toolName;      // 낚싯대, 릴, 낚싯줄, 양동이, 보트
    public UpgradeLevel[] levels;      // 3단계
}

[System.Serializable]
public class UpgradeLevel
{
    public string  toolTipTitle;
    [TextArea]
    public string  toolTipDesc;
    public string  toolTipStat;    // 성능 설명 (예: "캐스팅 거리 +2m")
    public Sprite  icon;

    [Header("비용")]
    public int     goldCost;
    public FishData requiredFish;
    public int     requiredFishCount;
}