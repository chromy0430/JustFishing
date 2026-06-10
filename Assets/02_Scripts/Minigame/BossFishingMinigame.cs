using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BossFishingMinigame : MonoBehaviour
{
    [Header("기본 미니게임 참조")]
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
    [SerializeField] private TextMeshProUGUI comboTimerTxt;

    private FishData _fishData;
    private float    _bossMaxHp;
    private int      _currentPhase  = 1;
    private bool     _phaseChanging = false;
    private bool     _comboActive   = false;
    private int      _comboRemaining;
    private float    _comboTimer;
    private Sequence _warningSeq;

    private void Awake()
    {
        bossPanel.SetActive(false);
        comboPanel.SetActive(false);
    }

    public void StartBossMinigame(FishData fishData, Action<bool> onResult)
    {
        _fishData      = fishData;
        _bossMaxHp     = fishData.fishHp;
        _currentPhase  = 1;
        _phaseChanging = false;
        _comboActive   = false;

        bossPanel.SetActive(true);
        comboPanel.SetActive(false);

        if (bossNameTxt != null) bossNameTxt.text = fishData.fishName;
        UpdatePhaseUI();
        UpdateBossHpUI(0f); // 시작 게이지 0

        // 기존 게이지 숨기기
        baseMiniGame.HideCaptureGauge();

        // 게이지 변화 구독
        baseMiniGame.OnGaugeChanged += OnGaugeUpdated;

        // 미니게임 시작
        baseMiniGame.StartMinigame(baseMiniGame.GetFishingData(), fishData, (success) =>
        {
            baseMiniGame.OnGaugeChanged -= OnGaugeUpdated;
            bossPanel.SetActive(false);
            comboPanel.SetActive(false);
            _warningSeq?.Kill();
            onResult?.Invoke(success);
        });
    }

    // 게이지 변화 시 호출됨
    private void OnGaugeUpdated(float captureGauge)
    {
        UpdateBossHpUI(captureGauge);

        if (_phaseChanging) return;

        // 게이지 진행도로 페이즈 체크
        float progress = captureGauge / _bossMaxHp;

        int newPhase = 1;
        if (progress >= _fishData.phase2HpRatio) newPhase = 2;
        if (progress >= _fishData.phase3HpRatio) newPhase = 3;

        if (newPhase > _currentPhase)
            StartCoroutine(PhaseChangeRoutine(newPhase));
    }

    private void UpdateBossHpUI(float captureGauge)
    {
        if (bossHpFill == null) return;
        float ratio = captureGauge / _bossMaxHp;
        bossHpFill.fillAmount = ratio;

        // 진행될수록 색상 변화 (주황 → 빨강)
        bossHpFill.color = Color.Lerp(
            new Color(1f, 0.5f, 0f),
            new Color(0.2f, 0.8f, 0.2f),
            ratio);
    }

    private IEnumerator PhaseChangeRoutine(int newPhase)
    {
        _phaseChanging = true;

        // 경고 텍스트
        if (warningObj != null && warningTxt != null)
        {
            warningTxt.text = $"PHASE {newPhase}!";
            warningObj.SetActive(true);

            CanvasGroup cg = warningObj.GetComponent<CanvasGroup>();
            if (cg == null) cg = warningObj.AddComponent<CanvasGroup>();

            _warningSeq?.Kill();
            _warningSeq = DOTween.Sequence();
            for (int i = 0; i < 3; i++)
            {
                _warningSeq
                    .Append(cg.DOFade(1f, 0.2f))
                    .Append(cg.DOFade(0f, 0.2f));
            }
            yield return _warningSeq.WaitForCompletion();
            warningObj.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(1.2f);
        }

        _currentPhase = newPhase;
        UpdatePhaseUI();

        Debug.Log($"보스 페이즈 {newPhase} 시작");

        // 페이즈 3에서 연타 노트 시작
        if (newPhase == 3 && _fishData.bossNoteData != null
            && _fishData.bossNoteData.comboNote)
        {
            StartCoroutine(ComboNoteRoutine());
        }

        _phaseChanging = false;
    }

    private IEnumerator ComboNoteRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (!bossPanel.activeSelf) yield break; // 미니게임 종료됐으면 중단

        _comboActive    = true;
        _comboRemaining = _fishData.bossNoteData.comboCount;
        _comboTimer     = _fishData.bossNoteData.comboDuration;

        comboPanel.SetActive(true);
        UpdateComboUI();

        Debug.Log($"연타 시작! {_comboRemaining}회 / {_comboTimer}초");
    }

    private void Update()
    {
        if (!_comboActive) return;

        _comboTimer -= Time.deltaTime;

        if (comboTimerTxt != null)
            comboTimerTxt.text = $"{_comboTimer:F1}";

        // 시간 초과
        if (_comboTimer <= 0f)
        {
            _comboActive = false;
            comboPanel.SetActive(false);
            baseMiniGame.OnNoteJudged(NoteJudgement.Miss);
            Debug.Log("연타 실패 - 시간 초과");
            return;
        }

        // 스페이스바 입력
        if (baseMiniGame.InputData != null && baseMiniGame.InputData.JumpPressed)
        {
            baseMiniGame.InputData.ConsumeJump();
            _comboRemaining--;
            UpdateComboUI();

            AudioManager.Instance?.PlayJudgement(NoteJudgement.Good);

            if (_comboRemaining <= 0)
            {
                _comboActive = false;
                comboPanel.SetActive(false);
                baseMiniGame.OnNoteJudged(NoteJudgement.Perfect);
                Debug.Log("연타 성공!");
            }
        }
    }

    private void UpdateComboUI()
    {
        if (comboCountTxt != null)
            comboCountTxt.text = $"{_comboRemaining}";
    }

    private void UpdatePhaseUI()
    {
        if (phaseTxt != null)
            phaseTxt.text = $"PHASE {_currentPhase}";
    }
}