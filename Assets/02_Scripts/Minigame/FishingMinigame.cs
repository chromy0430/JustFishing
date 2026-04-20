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

    [Header("Note")]
    [SerializeField] private GameObject notePrefab;

    [Header("Data")]
    [SerializeField] private PlayerInputData inputData;

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
        _fishingData = fishingData;
        _fishData = fishData;
        _onResult = onResult;
        _captureGauge = CalculateStartGauge();

        // 판정 존 크기를 FishData 기준으로 런타임 설정
        perfectZone.sizeDelta = Vector2.one * (_fishData.perfectRange * 2f);
        goodZone.sizeDelta = Vector2.one * (_fishData.goodRange * 2f);

        // PerfectZone의 CircleCollider2D 반지름도 업데이트
        CircleCollider2D perfectCol = perfectZone.GetComponent<CircleCollider2D>();
        CircleCollider2D goodCol = goodZone.GetComponent<CircleCollider2D>();
        if (perfectCol != null) perfectCol.radius = _fishData.perfectRange;
        if (goodCol != null) goodCol.radius = _fishData.goodRange;

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
        if (notePrefab == null) { Debug.LogError("notePrefab null"); return; }

        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = 280f;
        Vector2 spawnPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        Vector2 centerPos = perfectZone.anchoredPosition; // PerfectZone 중심이 목표

        GameObject noteObj = Instantiate(notePrefab, noteSpawnArea);
        FishingNote note = noteObj.GetComponent<FishingNote>();
        if (note == null) { Debug.LogError("FishingNote 없음"); return; }

        note.Init(spawnPos, centerPos, _fishData.noteSpeed, this);
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
        float gaugeChange = judgement switch
        {
            NoteJudgement.Perfect => GAUGE_PERFECT,
            NoteJudgement.Good => GAUGE_GOOD,
            NoteJudgement.Bad => GAUGE_BAD,
            NoteJudgement.Miss => GAUGE_MISS,
            _ => 0f
        };

        ShowJudgement(judgement, gaugeChange);

        // 게이지는 물고기 체력 기준으로 환산
        // 예: 체력 200, +12 → FillAmount로 환산 = 12/200 = 0.06
        _captureGauge += gaugeChange;
        _captureGauge = Mathf.Clamp(_captureGauge, 0f, _fishData.fishHp);

        UpdateGaugeUI();

        if (_captureGauge >= _fishData.fishHp) EndMinigame(true);
        else if (_captureGauge <= 0f) EndMinigame(false);
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
        string text = judgement.ToString();
        Color color = judgement switch
        {
            NoteJudgement.Perfect => Color.yellow,
            NoteJudgement.Good => Color.green,
            NoteJudgement.Bad => Color.white,
            NoteJudgement.Miss => Color.red,
            _ => Color.white
        };

        // nameof 대신 Coroutine 레퍼런스로 관리
        if (_judgementCoroutine != null)
            StopCoroutine(_judgementCoroutine);
        _judgementCoroutine = StartCoroutine(JudgementFadeRoutine(text, color));
    }

    private IEnumerator JudgementFadeRoutine(string text, Color color)
    {
        judgementText.text = text;
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

}