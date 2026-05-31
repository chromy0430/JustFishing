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
    
    [Header("Zone 1 - FishData")]
    public FishData[] zone1FishData;

    [Header("Zone 2 - FishData")]
    public FishData[] zone2FishData;

    [Header("Zone 3 - FishData")]
    public FishData[] zone3FishData;

    public FishData GetRandomFishData(int zone)
    {
        FishData[] dataArr = zone switch
        {
            1 => zone1FishData,
            2 => zone2FishData,
            _ => zone3FishData,
        };

        if (dataArr == null || dataArr.Length == 0)
        {
            Debug.LogError($"Zone {zone}의 FishData가 비어있습니다.");
            return null;
        }

        return dataArr[Random.Range(0, dataArr.Length)];
    }
}