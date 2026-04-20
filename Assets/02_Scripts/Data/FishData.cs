using UnityEngine;

[CreateAssetMenu(fileName = "FishData", menuName = "JustFishing/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("기본 정보")]
    public string fishName;
    public Sprite fishSprite;

    [Header("체력")]
    public float fishHp = 100f;

    [Header("미니게임 난이도")]
    public float noteSpeed = 200f;  // 노트 이동 속도
    public float notesPerSecond = 1f;    // 초당 노트 생성 수
    public float captureMax = 100f;  // 포획 게이지 최대값 (물고기 체력)
    public int rhythmPattern = 0;     // 리듬 패턴 타입 (나중에 확장)

    [Header("판정 범위")]
    public float perfectRange = 20f;
    public float goodRange = 40f;
}