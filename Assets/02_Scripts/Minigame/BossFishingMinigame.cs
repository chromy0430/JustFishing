using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;

public class BossFishingMinigame : MonoBehaviour
{
    [Header("기본 미니게임 참조")]
    [SerializeField] private FishingMinigame baseMiniGame;

    [Header("보스 전용 UI")]
    [SerializeField] private GameObject      bossPanel;
    [SerializeField] private Image           bossHpFill;
    [SerializeField] private TextMeshProUGUI bossNameTxt;
    [SerializeField] private GameObject      warningObj;
    [SerializeField] private TextMeshProUGUI warningTxt;

    private FishData _fishData;
    private float    _bossMaxHp;
    private int      _currentPhase  = 1;
    private bool     _phaseChanging = false;
    private Sequence _warningSeq;

    // 페이즈별 속도 설정
    private float _phase1MinSpeed;
    private float _phase1MaxSpeed;
    private float _phase2MinSpeed;
    private float _phase2MaxSpeed;
    private float _phase3MinSpeed;
    private float _phase3MaxSpeed;

    private void Awake()
    {
        bossPanel.SetActive(false);
    }
    
    private void Update()
    {
        // F3: 보스 패널 활성 + 디버그 패널 열린 상태에서만 작동
        if (bossPanel != null
            && bossPanel.activeSelf
            && DebugPanel.Instance != null
            && DebugPanel.Instance.IsOpen
            && Keyboard.current.f3Key.wasPressedThisFrame)
        {
            Debug.Log("[F3] 보스 즉시 성공");
            // ForceEndMinigame이 EndMinigame(true)를 호출
            // → EndMinigame에서 _onResult(true) → EndBossMinigame 콜백 실행
            // → bossPanel.SetActive(false) 자동 처리됨
            baseMiniGame.ForceEndMinigame(true);
            return;
        }
    }

    public void StartBossMinigame(FishData fishData, Action<bool> onResult)
    {
        _fishData      = fishData;
        _bossMaxHp     = fishData.fishHp;
        _currentPhase  = 1;
        _phaseChanging = false;

        // 페이즈별 속도 범위 계산
        float baseSpeed = fishData.noteSpeed;
        _phase1MinSpeed = baseSpeed * 0.7f;
        _phase1MaxSpeed = baseSpeed * 1.3f;
        _phase2MinSpeed = baseSpeed * 0.9f;
        _phase2MaxSpeed = baseSpeed * 1.8f;
        _phase3MinSpeed = baseSpeed * 1.2f;
        _phase3MaxSpeed = baseSpeed * 2.5f;

        bossPanel.SetActive(true);

        if (bossNameTxt != null)
            bossNameTxt.text = "BOSS : " + fishData.fishName;

        UpdateBossHpUI(0f);

        baseMiniGame.HideCaptureGauge();
        baseMiniGame.OnGaugeChanged += OnGaugeUpdated;

        // 보스 전용 노트 스폰 방식으로 변경
        baseMiniGame.SetBossNoteSpawnMode(true, _phase1MinSpeed, _phase1MaxSpeed);

        baseMiniGame.StartMinigame(baseMiniGame.GetFishingData(), fishData, (success) =>
        {
            EndBossMinigame(success, onResult);
        });
    }

    private void EndBossMinigame(bool success, Action<bool> onResult)
    {
        baseMiniGame.OnGaugeChanged -= OnGaugeUpdated;
        baseMiniGame.SetBossNoteSpawnMode(false, 0f, 0f);

        _warningSeq?.Kill();
        StopAllCoroutines();

        _phaseChanging = false;

        if (bossPanel  != null) bossPanel.SetActive(false);
        if (warningObj != null)
        {
            warningObj.SetActive(false);
            CanvasGroup cg = warningObj.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;
        }

        baseMiniGame.ShowCaptureGauge();
        onResult?.Invoke(success);
    }

    private void OnGaugeUpdated(float captureGauge)
    {
        UpdateBossHpUI(captureGauge);
        
        if (DebugPanel.Instance != null && DebugPanel.Instance.ConsumeBossInstantWin())
        {
            // 게이지를 최대로 채워서 EndMinigame(true) 유도
            baseMiniGame.ForceEndMinigame(true);
            return;
        }

        if (_phaseChanging) return;

        float progress = captureGauge / _bossMaxHp;

        // 핵심: 현재 페이즈 기준으로 딱 하나만 체크
        if      (_currentPhase == 1 && progress >= _fishData.phase2HpRatio)
            StartCoroutine(PhaseChangeRoutine(2));
        else if (_currentPhase == 2 && progress >= _fishData.phase3HpRatio)
            StartCoroutine(PhaseChangeRoutine(3));
    }

    private IEnumerator PhaseChangeRoutine(int newPhase)
    {
        // 즉시 플래그 세팅 (같은 프레임에 중복 진입 차단)
        _phaseChanging = true;
        _currentPhase  = newPhase; // 페이즈 먼저 바꿔서 중복 체크 차단

        // 경고 연출
        if (warningObj != null && warningTxt != null)
        {
            warningTxt.text = newPhase switch
            {
                2 => "조금만 더 힘내세요!!",
                3 => "이제 곧 잡혀요! 영차영차!!",
                _ => $"Phase {newPhase}"
            };

            warningObj.SetActive(true);

            CanvasGroup cg = warningObj.GetComponent<CanvasGroup>();
            if (cg == null) cg = warningObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            // DOTween 깜빡임 (독립 Sequence로 생성)
            Sequence seq = DOTween.Sequence();
            for (int i = 0; i < 3; i++)
            {
                seq.Append(cg.DOFade(1f, 0.25f))
                   .AppendInterval(0.15f)
                   .Append(cg.DOFade(0f, 0.25f));
            }

            // _warningSeq에 보관 (EndBossMinigame에서 Kill 가능하도록)
            _warningSeq = seq;
            yield return seq.WaitForCompletion();

            warningObj.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(1.2f);
        }

        Debug.Log($"보스 페이즈 {newPhase} 시작");

        // 페이즈별 노트 속도 변경
        float minSpd = newPhase switch
        {
            2 => _phase2MinSpeed,
            3 => _phase3MinSpeed,
            _ => _phase1MinSpeed
        };
        float maxSpd = newPhase switch
        {
            2 => _phase2MaxSpeed,
            3 => _phase3MaxSpeed,
            _ => _phase1MaxSpeed
        };

        baseMiniGame.SetBossNoteSpawnMode(true, minSpd, maxSpd);

        _phaseChanging = false;
    }

    private void UpdateBossHpUI(float captureGauge)
    {
        if (bossHpFill == null) return;
        float ratio = captureGauge / _bossMaxHp;
        bossHpFill.fillAmount = ratio;
        bossHpFill.color = Color.Lerp(
            new Color(1f, 0.5f, 0f),
            new Color(0.2f, 0.8f, 0.2f),
            ratio);
    }
}