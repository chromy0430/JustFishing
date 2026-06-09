using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FishingMinigame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private Image captureGaugeFill;
    [SerializeField] private TextMeshProUGUI judgementText;
    [SerializeField] private RectTransform centerZone;    // 시각적 중심 (CenterZone)
    [SerializeField] private RectTransform perfectZone;   // 판정 기준점 (PerfectZone)
    [SerializeField] private RectTransform goodZone;      // 판정 기준점 (GoodZone)
    [SerializeField] private RectTransform noteSpawnArea;
    
    public PlayerInputData InputData => inputData;
    

    
    [Header("강화 효과")]
    [SerializeField] private UpgradeEffectData upgradeEffectData;
    private float _gaugeBonus;
    private float _noteSpeed;
    private float _missPenalty;
    private float _badPenalty;
    
    [Header("이미지 설정")]
    [SerializeField] private Image        centerZoneImage;  // Img_CenterZone의 Image
    [SerializeField] private List<Sprite> noteSprites;      // 노트 이미지 리스트

    [Header("Note")]
    [SerializeField] private GameObject notePrefab;

    [Header("Data")]
    [SerializeField] private PlayerInputData inputData;

    [Header("Pulse")]
    [SerializeField] private CenterZonePulse centerZonePulse; // Inspector 연결
    
    private static readonly Vector2[] SpawnDirections = new Vector2[]
    {
        new Vector2(   0f,  300f),
        new Vector2(   0f, -300f),
        new Vector2(-300f,    0f),
        new Vector2( 300f,    0f),
    };

    private Action<bool> _onResult;
    private FishingData _fishingData;
    private FishData _fishData;
    private float _captureGauge;
    private List<FishingNote> _activeNotes = new List<FishingNote>();
    private Coroutine _spawnRoutine;
    private bool _isPlaying = false;
    private Coroutine _judgementCoroutine;

    // 게이지 관련 상수
    private const float GAUGE_PERFECT = 12f;
    private const float GAUGE_GOOD = 8f;
    private const float GAUGE_BAD = 3f;
    private const float GAUGE_MISS = -7f;

    private void Awake()
    {
        minigamePanel.SetActive(false);
    }

    public void StartMinigame(FishingData fishingData, FishData fishData, Action<bool> onResult)
    {
        _fishingData  = fishingData;
        _fishData     = fishData;
        _onResult     = onResult;
        _captureGauge = CalculateStartGauge();

        // 강화 레벨
        int rodLevel  = PlayerPrefs.GetInt("Upgrade_낚싯대", 0);
        int reelLevel = PlayerPrefs.GetInt("Upgrade_릴",     0);
        int lineLevel = PlayerPrefs.GetInt("Upgrade_낚싯줄", 0);

        _gaugeBonus  = upgradeEffectData != null
            ? rodLevel * upgradeEffectData.rodGaugeBonus : rodLevel * 2f;
        _noteSpeed   = Mathf.Max(50f, fishData.noteSpeed
                                      - (reelLevel * (upgradeEffectData?.reelSpeedReduction ?? 20f)));
        _missPenalty = Mathf.Min(-1f, -7f
                                      + lineLevel * (upgradeEffectData?.lineMissPenaltyReduction ?? 2f));
        _badPenalty  = Mathf.Max(0f, 3f
                                     - lineLevel * (upgradeEffectData?.lineMissPenaltyReduction ?? 2f) * 0.5f);

        // 중심원 이미지를 물고기 아이콘으로 변경
        if (centerZoneImage != null && fishData.fishSprite != null)
            centerZoneImage.sprite = fishData.fishSprite;

        // 판정 존 크기 갱신
        perfectZone.sizeDelta = Vector2.one * (_fishData.perfectRange * 2f);
        goodZone.sizeDelta    = Vector2.one * (_fishData.goodRange    * 2f);

        CircleCollider2D perfectCol = perfectZone.GetComponent<CircleCollider2D>();
        CircleCollider2D goodCol    = goodZone.GetComponent<CircleCollider2D>();
        if (perfectCol != null) perfectCol.radius = _fishData.perfectRange;
        if (goodCol    != null) goodCol.radius    = _fishData.goodRange;

        _isPlaying = true;
        _activeNotes.Clear();

        UpdateGaugeUI();
        minigamePanel.SetActive(true);

        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(NoteSpawnRoutine());
    }

    // SpawnNote - perfectZone 기준으로 목표 지점 설정
    private void SpawnNote()
    {
        if (notePrefab == null) return;

        float   angle     = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float   radius    = 280f;
        Vector2 spawnPos  = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        Vector2 centerPos = perfectZone.anchoredPosition;

        GameObject  noteObj = Instantiate(notePrefab, noteSpawnArea);
        FishingNote note    = noteObj.GetComponent<FishingNote>();
        if (note == null) return;

        if (noteSprites != null && noteSprites.Count > 0)
            note.SetSprite(noteSprites[UnityEngine.Random.Range(0, noteSprites.Count)]);

        // _noteSpeed 사용 (릴 강화 반영)
        note.Init(spawnPos, centerPos, _noteSpeed, this);
        _activeNotes.Add(note);
    }

    private void Update()
    {
        if (!_isPlaying) return;

        if (inputData.JumpPressed)
        {
            inputData.ConsumeJump();
            JudgeClosestNote();
        }
    }

    private void JudgeClosestNote()
    {
        if (_activeNotes.Count == 0) return;

        FishingNote closest = null;
        float minDistance = float.MaxValue;

        foreach (FishingNote note in _activeNotes)
        {
            if (note == null) continue;
            float dist = note.GetDistanceToCenter();
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = note;
            }
        }

        if (closest == null) return;

        // 판정 범위 밖이면 완전히 무시
        if (minDistance > _fishData.goodRange)
        {
            _activeNotes.Remove(closest);
            closest.Judge(); // 노트 제거
            OnNoteJudged(NoteJudgement.Miss);
            return;
        }

        NoteJudgement judgement;
        if (minDistance <= _fishData.perfectRange) judgement = NoteJudgement.Perfect;
        else if (minDistance <= _fishData.goodRange) judgement = NoteJudgement.Good;
        else judgement = NoteJudgement.Bad;

        _activeNotes.Remove(closest);
        closest.Judge();
        OnNoteJudged(judgement);
    }

    public void OnNoteJudged(NoteJudgement judgement)
    {
        AudioManager.Instance?.PlayJudgement(judgement);
        
        switch (judgement)
        {
            case NoteJudgement.Perfect:
                _captureGauge += 12f + _gaugeBonus;
                ShowJudgement(judgement, 12f + _gaugeBonus);
                break;
            case NoteJudgement.Good:
                _captureGauge += 8f + _gaugeBonus;
                ShowJudgement(judgement, 8f + _gaugeBonus);
                break;
            case NoteJudgement.Bad:
                _captureGauge -= _badPenalty;
                ShowJudgement(judgement, -_badPenalty);
                break;
            case NoteJudgement.Miss:
                _captureGauge += _missPenalty;
                ShowJudgement(judgement, _missPenalty);
                break;
        }

        // Miss 제외 Pulse 효과 ← 누락됐던 부분
        if (judgement != NoteJudgement.Miss)
            centerZonePulse?.Pulse(judgement);

        _captureGauge = Mathf.Clamp(_captureGauge, 0f, _fishData.fishHp);
        UpdateGaugeUI();

        if      (_captureGauge >= _fishData.fishHp) EndMinigame(true);
        else if (_captureGauge <= 0f)               EndMinigame(false);
    }

    private IEnumerator NoteSpawnRoutine()
    {
        float interval = 1f / _fishData.notesPerSecond;

        while (_isPlaying)
        {
            SpawnNote();
            yield return new WaitForSeconds(interval);
        }
    }

    private void ShowJudgement(NoteJudgement judgement, float gaugeChange)
    {
        string text  = judgement.ToString();
        Color  color = judgement switch
        {
            NoteJudgement.Perfect => Color.yellow,
            NoteJudgement.Good    => Color.green,
            NoteJudgement.Bad     => Color.white,
            NoteJudgement.Miss    => Color.red,
            _                     => Color.white
        };

        if (_judgementCoroutine != null)
            StopCoroutine(_judgementCoroutine);
        _judgementCoroutine = StartCoroutine(JudgementFadeRoutine(text, color));
    }

    private IEnumerator JudgementFadeRoutine(string text, Color color)
    {
        judgementText.text  = text;
        judgementText.color = color;
        judgementText.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        judgementText.gameObject.SetActive(false);
        _judgementCoroutine = null;
    }

    private void UpdateGaugeUI()
    {
        captureGaugeFill.fillAmount = _captureGauge / _fishData.fishHp;
    }

    private void EndMinigame(bool success)
    {
        _isPlaying = false;
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);

        foreach (FishingNote note in _activeNotes)
            if (note != null) Destroy(note.gameObject);
        _activeNotes.Clear();

        centerZonePulse?.ClearImpacts();

        minigamePanel.SetActive(false);
        _onResult?.Invoke(success);
    }

    public void RemoveNote(FishingNote note)
    {
        _activeNotes.Remove(note);
    }

    private float CalculateStartGauge()
    {
        // 물고기 체력의 10%
        return _fishData.fishHp * 0.1f;
    }
    
    public void HideCaptureGauge()
    {
        if (captureGaugeFill != null)
            captureGaugeFill.transform.parent.gameObject.SetActive(false);
    }

}