using UnityEngine;

[System.Serializable]
public class BossNoteData
{
    [Header("랜덤 속도")]
    public float minSpeed     = 150f;
    public float maxSpeed     = 350f;

    [Header("페이드 효과")]
    public bool  fadeEffect   = true;

    [Header("연타 노트")]
    public bool  comboNote    = true;
    public int   comboCount   = 5;
    public float comboDuration = 4f;
}