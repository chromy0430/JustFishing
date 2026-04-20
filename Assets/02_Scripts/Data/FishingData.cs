using UnityEngine;

[CreateAssetMenu(fileName = "FishingData", menuName = "JustFishing/Fishing Data")]
public class FishingData : ScriptableObject
{
    [Header("인디케이터")]
    public float minCastRadius = 3f;   // 최소 반경 추가
    public float maxCastRadius = 10f;

    [Header("찌")]
    public float castDuration = 1f;    // 포물선 비행 시간
    public float arcHeight = 3f;    // 포물선 최고 높이
    public float waitMinTime = 1f;    // 입질 최소 대기 시간
    public float waitMaxTime = 5f;    // 입질 최대 대기 시간
    public float biteDepth = 0.5f;  // 찌가 내려가는 깊이
    public float biteDuration = 0.5f;  // 찌가 내려가는 시간
    public float biteTimeLimit = 3f;    // 입질 반응 제한 시간

    [Header("미니게임 공통")]
    public float captureGaugeStart = 0f; // 시작 게이지 (공통)
}