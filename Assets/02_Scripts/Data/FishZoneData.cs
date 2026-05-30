using UnityEngine;

[CreateAssetMenu(fileName = "FishZoneData", menuName = "JustFishing/Fish Zone Data")]
public class FishZoneData : ScriptableObject
{
    [Header("Zone 1 (섬 근처)")]
    public GameObject[] zone1FishPrefabs;

    [Header("Zone 2 (중간 거리)")]
    public GameObject[] zone2FishPrefabs;

    [Header("Zone 3 (먼 바다)")]
    public GameObject[] zone3FishPrefabs;
}