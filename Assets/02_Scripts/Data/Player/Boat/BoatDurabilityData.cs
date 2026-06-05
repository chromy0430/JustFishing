using UnityEngine;

[CreateAssetMenu(fileName = "BoatDurabilityData", menuName = "JustFishing/Boat Durability Data")]
public class BoatDurabilityData : ScriptableObject
{
    public string boatName;
    public float  maxDurability;

    [Header("지역별 초당 내구도 감소량")]
    public float zone1DamagePerSec = 0f;  // 연안 - 전부 0
    public float zone2DamagePerSec = 0f;  // 심해 - 보트별로 다름
    public float zone3DamagePerSec = 0f;  // 마력해역 - 보트별로 다름
}