using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] private GameObject      panel;
    [SerializeField] private TextMeshProUGUI statTxt;
    [SerializeField] private TextMeshProUGUI commandHintTxt; // 명령어 안내 텍스트

    [Header("Data")]
    [SerializeField] private BoatData          boatData;
    [SerializeField] private UpgradeEffectData upgradeEffectData;

    public static DebugPanel Instance { get; private set; }

    // 디버그 명령 플래그
    public bool InstantWinRequested     { get; private set; } = false; // F2
    public bool BossInstantWinRequested { get; private set; } = false; // F3

    public bool IsOpen => _isOpen; // 추가
    private bool _isOpen = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        panel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            _isOpen = !_isOpen;
            panel.SetActive(_isOpen);
            if (_isOpen) RefreshStats();
        }

        if (!_isOpen) return;

        // F2 - 일반 미니게임 즉시 성공 (1회, 미리 예약)
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            InstantWinRequested = true;
            ShowCommandFeedback("[F2] 다음 미니게임 즉시 성공 준비됨");
            RefreshStats();
        }

        // F4 - 골드 지급
        if (Keyboard.current.f4Key.wasPressedThisFrame)
        {
            PlayerWallet.Instance?.AddGold(1000);
            ShowCommandFeedback($"[F4] 골드 +1000 (현재: {PlayerWallet.Instance?.Gold}G)");
            RefreshStats();
        }
    }

    // 플래그 소비 (1회 사용 후 자동 초기화)
    public bool ConsumeInstantWin()
    {
        if (!InstantWinRequested) return false;
        InstantWinRequested = false;
        ShowCommandFeedback("[F2] 미니게임 즉시 성공 사용됨");
        return true;
    }

    public bool ConsumeBossInstantWin()
    {
        if (!BossInstantWinRequested) return false;
        BossInstantWinRequested = false;
        ShowCommandFeedback("[F3] 보스 즉시 성공 사용됨");
        return true;
    }

    private void ShowCommandFeedback(string message)
    {
        if (commandHintTxt != null)
            commandHintTxt.text = message;
        Debug.Log($"[Debug] {message}");
    }

    private void RefreshStats()
    {
        // 기존 스탯 표시 코드 유지...
        int rodLevel    = PlayerPrefs.GetInt("Upgrade_낚싯대", 0);
        int reelLevel   = PlayerPrefs.GetInt("Upgrade_릴",     0);
        int lineLevel   = PlayerPrefs.GetInt("Upgrade_낚싯줄", 0);
        int bucketLevel = PlayerPrefs.GetInt("Upgrade_양동이", 0);
        int boatLevel   = PlayerPrefs.GetInt("Upgrade_보트",   0);

        float gaugeBonus     = rodLevel  * (upgradeEffectData?.rodGaugeBonus            ?? 2f);
        float speedReduction = reelLevel * (upgradeEffectData?.reelSpeedReduction        ?? 20f);
        float missPenalty    = Mathf.Min(-1f, -7f
            + lineLevel * (upgradeEffectData?.lineMissPenaltyReduction ?? 2f));

        BoatLevelData boat = null;
        if (boatData != null && boatLevel < boatData.levels.Length)
            boat = boatData.levels[boatLevel];

        float currentWeight = InventorySystem.Instance?.CurrentWeight ?? 0f;
        float maxWeight     = InventorySystem.Instance?.MaxWeight     ?? 30f;
        int   itemCount     = InventorySystem.Instance?.Items.Count   ?? 0;
        int   maxSlots      = InventorySystem.Instance?.MaxSlots      ?? 10;
        int   gold          = PlayerWallet.Instance?.Gold ?? 0;

        BoatDurability durability = FindFirstObjectByType<BoatDurability>();
        float currentDur = durability?.CurrentDurability ?? 0f;
        float maxDur     = durability?.MaxDurability     ?? 150f;

        statTxt.text =
            $@"=== DEBUG PANEL ===

            [ 명령어 ]
            F2 : 일반 미니게임 즉시 성공 (1회)
            F3 : 보스 미니게임 즉시 성공 (1회)
            F4 : 골드 +1000 즉시 지급

            [ 강화 정보 ]
            낚싯대  Lv.{rodLevel}  |  릴  Lv.{reelLevel}  |  낚싯줄  Lv.{lineLevel}
            양동이  Lv.{bucketLevel}  |  보트  Lv.{boatLevel}

            [ 미니게임 스탯 ]
            게이지 보너스 : +{gaugeBonus:F1}
            노트 속도 감소 : -{speedReduction:F1}
            Miss 패널티   : {missPenalty:F1}

            [ 보트 스탯 ]
            이름        : {boat?.boatName ?? "없음"}
            이동 속도   : {boat?.moveSpeed ?? 0f}
            내구도      : {currentDur:F1} / {maxDur:F1}

            [ 인벤토리 ]
            슬롯 : {itemCount}/{maxSlots}  |  무게 : {currentWeight:F1}/{maxWeight:F1}kg

            [ 경제 ]
            골드 : {gold} G

            [ 디버그 플래그 ]
            F2 대기 중 : {(InstantWinRequested     ? "YES" : "NO")}
            F3 대기 중 : {(BossInstantWinRequested ? "YES" : "NO")}";
    }
}