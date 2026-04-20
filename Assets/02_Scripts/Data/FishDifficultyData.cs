// FishDifficultyData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "JustFishing/Fish Difficulty Data")]
public class FishDifficultyData : ScriptableObject
{
    public float noteSpeed;        // 노트 이동 속도
    public float notesPerSecond;   // 초당 노트 생성 수
    public int rhythmPattern;    // 리듬 구조 타입
    public float captureMax;       // 포획 게이지 최대값 (체력 개념)
}