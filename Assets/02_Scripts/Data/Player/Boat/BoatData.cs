using UnityEngine;

[CreateAssetMenu(fileName = "BoatData", menuName = "JustFishing/Boat Data")]
public class BoatData : ScriptableObject
{
    [Header("레벨별 보트 데이터")]
    public BoatLevelData[] levels; // [0]=나무, [1]=철, [2]=마력
}

[System.Serializable]
public class BoatLevelData
{
    public string     boatName;
    public Sprite     boatIcon;
    public GameObject islandPrefab; // Island 선착장용 (메쉬만)
    public GameObject oceanPrefab;  // Ocean 씬용 (BoatController 포함)

    [Header("이동")]
    public float moveSpeed   = 5f;
    public float rotateSpeed = 90f;

    [Header("내구도")]
    public float maxDurability       = 150f;
    public float zone1DamagePerSec   = 0f;
    public float zone2DamagePerSec   = 0f;
    public float zone3DamagePerSec   = 0f;
}