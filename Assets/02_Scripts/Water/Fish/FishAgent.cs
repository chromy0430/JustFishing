using UnityEngine;

public class FishAgent : MonoBehaviour
{
    [HideInInspector] public Vector3 velocity;        // 현재 속도
    [HideInInspector] public Vector3 spawnCenter;     // 스폰 중심
    [HideInInspector] public int     zoneIndex;       // 소속 Zone

    [Header("개별 설정")]
    public float moveSpeed    = 1.5f;
    public float wanderRadius = 8f;   // 스폰 중심 기준 최대 배회 반경

    // FishManager에서 직접 접근
    [HideInInspector] public Vector3 acceleration;

    public void Init(Vector3 pos, int zone)
    {
        spawnCenter = pos;
        zoneIndex   = zone;

        // 랜덤 초기 방향
        Vector2 rand = Random.insideUnitCircle.normalized;
        velocity     = new Vector3(rand.x, 0f, rand.y) * moveSpeed;
    }
}