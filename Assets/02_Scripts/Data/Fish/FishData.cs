using UnityEngine;

[CreateAssetMenu(fileName = "FishData", menuName = "JustFishing/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("기본 정보")]
    public string fishName;
    [TextArea] public string fishDescription;
    public Sprite fishSprite;

    [Header("체력")]
    public float fishHp = 100f;

    [Header("미니게임 난이도")]
    public float noteSpeed = 200f;  // 노트 이동 속도
    public float notesPerSecond = 1f;    // 초당 노트 생성 수
    public int   rhythmPattern  = 0;
    public float perfectRange   = 20f;
    public float goodRange      = 40f;

    [Header("가격/크기")]
    public int   basePrice   = 100;   // 기본 가격
    public float maxLength   = 100f;  // 최대 길이 (cm)
    public float maxWeight   = 10f;   // 최대 무게 (kg)
    
    [Header("보스 설정")]
    public bool  isBoss          = false;
    public BossNoteData bossNoteData;
    public int   bossPhaseCount  = 3;       // 페이즈 수
    public float phase2HpRatio   = 0.66f;  // 2페이즈 전환 HP 비율
    public float phase3HpRatio   = 0.33f;  // 3페이즈 전환 HP 비율

    // 페이즈별 노트 속도
    public float phase2NoteSpeed = 0f;     // 0이면 fishData.noteSpeed * 1.5 자동 계산
    public float phase3NoteSpeed = 0f;     // 0이면 fishData.noteSpeed * 2.0 자동 계산
    
    // 랜덤 길이 생성 (짧을수록 확률 높음)
    public float GetRandomLength()
    {
        // 제곱근 분포: 짧은 길이에 확률 집중
        float rand = Random.value;
        return Mathf.Pow(rand, 2f) * maxLength;
    }

    // 랜덤 무게 생성 (가벼울수록 확률 높음)
    public float GetRandomWeight()
    {
        float rand = Random.value + 0.1f;
        return Mathf.Pow(rand, 2f) * maxWeight;
    }

    // 최종 가격 계산
    public int CalculatePrice(float length, float weight)
    {
        // 길이와 무게 비율로 보정 (최대 2배)
        float lengthRatio  = length / maxLength;
        float weightRatio  = weight / maxWeight;
        float bonusMultiplier = 1f + (lengthRatio + weightRatio) * 0.5f;

        return Mathf.RoundToInt(basePrice * bonusMultiplier);
    }
}