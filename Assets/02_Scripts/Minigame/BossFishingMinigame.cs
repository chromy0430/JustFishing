using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BossFishingMinigame : MonoBehaviour
{
    [Header("기본 UI (일반 미니게임과 공유)")]
    [SerializeField] private FishingMinigame baseMiniGame;

    [Header("보스 전용 UI")]
    [SerializeField] private GameObject      bossPanel;
    [SerializeField] private Image           bossHpFill;
    [SerializeField] private TextMeshProUGUI bossNameTxt;
    [SerializeField] private TextMeshProUGUI phaseTxt;
    [SerializeField] private GameObject      warningObj;
    [SerializeField] private TextMeshProUGUI warningTxt;

    [Header("연타 UI")]
    [SerializeField] private GameObject      comboPanel;
    [SerializeField] private TextMeshProUGUI comboCountTxt;

    private FishData      _fishData;
    private float         _bossHp;
    private float         _currentHp;
    private int           _currentPhase = 1;
    private bool          _phaseChanging = false;
    private bool          _comboActive   = false;
    private int           _comboRemaining;
    private float         _comboTimer;

    private Sequence _warningSeq;

    public void StartBossMinigame(FishData fishData, Action<bool> onResult)
    {
        _fishData     = fishData;
        _bossHp       = fishData.fishHp;
        _currentHp    = _bossHp;
        _currentPhase = 1;

        bossPanel.SetActive(true);
        bossNameTxt.text = fishData.fishName;
        UpdateBossHpUI();
        UpdatePhaseUI();

        // 기존 미니게임 게이지 숨기기
        // (baseMiniGame의 captureGaugeFill 비활성화)
        baseMiniGame.HideCaptureGauge();

        // 미니게임 시작은 baseMiniGame이 처리
        // 단 OnNoteJudged를 보스 버전으로 override
    }

    public void OnBossNoteJudged(NoteJudgement judgement, float gaugeChange)
    {
        // HP 감소 (포획 게이지 역할)
        _currentHp -= gaugeChange;
        _currentHp  = Mathf.Clamp(_currentHp, 0f, _bossHp);

        UpdateBossHpUI();
        CheckPhaseChange();
    }

    private void CheckPhaseChange()
    {
        if (_phaseChanging) return;

        float progress = 1f - (_currentHp / _bossHp);

        int newPhase = 1;
        if (progress >= 1f - _fishData.phase2HpRatio) newPhase = 2;
        if (progress >= 1f - _fishData.phase3HpRatio) newPhase = 3;

        if (newPhase > _currentPhase)
            StartCoroutine(PhaseChangeRoutine(newPhase));
    }

    private IEnumerator PhaseChangeRoutine(int newPhase)
    {
        _phaseChanging = true;

        // 경고 연출 (DOTween 깜빡임)
        warningObj.SetActive(true);
        warningTxt.text = $"PHASE {newPhase}!";

        _warningSeq?.Kill();
        _warningSeq = DOTween.Sequence();
        for (int i = 0; i < 3; i++)
        {
            _warningSeq
                .Append(warningObj.GetComponent<CanvasGroup>()
                    .DOFade(1f, 0.2f))
                .Append(warningObj.GetComponent<CanvasGroup>()
                    .DOFade(0f, 0.2f));
        }

        yield return new WaitForSeconds(1.5f);
        warningObj.SetActive(false);

        _currentPhase = newPhase;
        UpdatePhaseUI();

        // 페이즈 3에서 연타 노트 예약
        if (newPhase == 3 && _fishData.bossNoteData.comboNote)
            StartCoroutine(ComboNoteRoutine());

        _phaseChanging = false;
    }

    // 연타 노트
    private IEnumerator ComboNoteRoutine()
    {
        yield return new WaitForSeconds(2f);

        _comboActive    = true;
        _comboRemaining = _fishData.bossNoteData.comboCount;
        _comboTimer     = _fishData.bossNoteData.comboDuration;

        comboPanel.SetActive(true);
        comboCountTxt.text = $"{_comboRemaining}";

        // 연타 판정은 Update에서 스페이스바 입력으로 처리
    }

    private void Update()
    {
        if (!_comboActive) return;

        _comboTimer -= Time.deltaTime;
        if (_comboTimer <= 0f)
        {
            // 시간 초과 - 실패
            _comboActive = false;
            comboPanel.SetActive(false);
            baseMiniGame.OnNoteJudged(NoteJudgement.Miss);
            return;
        }

        // 스페이스바 입력
        if (baseMiniGame.InputData.JumpPressed)
        {
            baseMiniGame.InputData.ConsumeJump();
            _comboRemaining--;
            comboCountTxt.text = $"{_comboRemaining}";

            if (_comboRemaining <= 0)
            {
                _comboActive = false;
                comboPanel.SetActive(false);
                baseMiniGame.OnNoteJudged(NoteJudgement.Perfect);
            }
        }
    }

    private void UpdateBossHpUI()
    {
        bossHpFill.fillAmount = _currentHp / _bossHp;
        // HP 낮을수록 색상 변화
        float ratio = _currentHp / _bossHp;
        bossHpFill.color = Color.Lerp(Color.red, new Color(1f, 0.5f, 0f), ratio);
    }

    private void UpdatePhaseUI()
    {
        phaseTxt.text = $"PHASE {_currentPhase}";
    }
}