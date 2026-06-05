using UnityEngine;

[CreateAssetMenu(fileName = "BucketData", menuName = "JustFishing/Bucket Data")]
public class BucketData : ScriptableObject
{
    [Header("레벨별 설정")]
    public BucketLevel[] levels;
}

[System.Serializable]
public class BucketLevel
{
    public int   level;
    public int   maxSlots;
    public float maxWeight; // kg
}