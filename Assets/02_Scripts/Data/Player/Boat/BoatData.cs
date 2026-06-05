using UnityEngine;

[CreateAssetMenu(fileName = "BoatData", menuName = "JustFishing/Boat Data")]
public class BoatData : ScriptableObject
{
    public GameObject boatPrefab;
    public float moveSpeed = 5f;   // linearVelocity 직접 설정이라 낮은 값
    public float rotateSpeed = 100f; // deg/sec 기준
}