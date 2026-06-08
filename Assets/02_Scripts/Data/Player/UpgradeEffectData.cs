using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeEffectData", 
    menuName = "JustFishing/Upgrade Effect Data")]
public class UpgradeEffectData : ScriptableObject
{
    [Header("낚싯대 - 포획 게이지 증가량 보너스 (레벨당)")]
    public float rodGaugeBonus = 2f;

    [Header("릴 - 노트 속도 감소 (레벨당)")]
    public float reelSpeedReduction = 20f;

    [Header("낚싯줄 - Miss 패널티 감소 (레벨당)")]
    public float lineMissPenaltyReduction = 2f;
}