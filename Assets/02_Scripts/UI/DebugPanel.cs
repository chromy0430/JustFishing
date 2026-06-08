using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] private GameObject      panel;
    [SerializeField] private TextMeshProUGUI statTxt;

    [Header("Data")]
    [SerializeField] private BoatData        boatData;
    [SerializeField] private UpgradeEffectData upgradeEffectData;

    private bool _isOpen = false;

    private void Awake()
    {
        panel.SetActive(false);
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            _isOpen = !_isOpen;
            panel.SetActive(_isOpen);

            if (_isOpen) RefreshStats();
        }
    }

    private void RefreshStats()
    {
        int rodLevel    = PlayerPrefs.GetInt("Upgrade_낚싯대", 0);
        int reelLevel   = PlayerPrefs.GetInt("Upgrade_릴",     0);
        int lineLevel   = PlayerPrefs.GetInt("Upgrade_낚싯줄", 0);
        int bucketLevel = PlayerPrefs.GetInt("Upgrade_양동이", 0);
        int boatLevel   = PlayerPrefs.GetInt("Upgrade_보트",   0);

        // 강화 효과 계산
        float gaugeBonus      = rodLevel  * (upgradeEffectData?.rodGaugeBonus            ?? 2f);
        float speedReduction  = reelLevel * (upgradeEffectData?.reelSpeedReduction        ?? 20f);
        float missPenalty     = -7f + lineLevel * (upgradeEffectData?.lineMissPenaltyReduction ?? 2f);
        missPenalty           = Mathf.Min(-1f, missPenalty);

        // 보트 데이터
        BoatLevelData boat = null;
        if (boatData != null && boatLevel < boatData.levels.Length)
            boat = boatData.levels[boatLevel];

        // 인벤토리
        float currentWeight = InventorySystem.Instance?.CurrentWeight ?? 0f;
        float maxWeight     = InventorySystem.Instance?.MaxWeight     ?? 30f;
        int   itemCount     = InventorySystem.Instance?.Items.Count   ?? 0;
        int   maxSlots      = InventorySystem.Instance?.MaxSlots      ?? 10;

        // 골드
        int gold = PlayerWallet.Instance?.Gold ?? 0;

        // 내구도
        BoatDurability durability = FindFirstObjectByType<BoatDurability>();
        float currentDur = durability?.CurrentDurability ?? 0f;
        float maxDur     = durability?.MaxDurability     ?? 150f;

        statTxt.text = $@"=== DEBUG PANEL ===
        [강화 정보]
        낚싯대  : Lv.{rodLevel}  ({GetUpgradeName("낚싯대", rodLevel)})
        릴      : Lv.{reelLevel}  ({GetUpgradeName("릴", reelLevel)})
        낚싯줄  : Lv.{lineLevel}  ({GetUpgradeName("낚싯줄", lineLevel)})
        양동이  : Lv.{bucketLevel}  ({GetUpgradeName("양동이", bucketLevel)})
        보트    : Lv.{boatLevel}  ({GetUpgradeName("보트", boatLevel)})

        [미니게임 스탯]
        포획 게이지 보너스 : +{gaugeBonus:F1} (Perfect/Good)
        노트 속도 감소     : -{speedReduction:F1}
        Miss 패널티        : {missPenalty:F1}

        [보트 스탯]
        이름        : {boat?.boatName ?? "없음"}
        이동 속도   : {boat?.moveSpeed ?? 0f}
        회전 속도   : {boat?.rotateSpeed ?? 0f}
        최대 내구도 : {boat?.maxDurability ?? 0f}
        현재 내구도 : {currentDur:F1} / {maxDur:F1}
        Zone1 감소  : {boat?.zone1DamagePerSec ?? 0f}/초
        Zone2 감소  : {boat?.zone2DamagePerSec ?? 0f}/초
        Zone3 감소  : {boat?.zone3DamagePerSec ?? 0f}/초

        [인벤토리]
        슬롯  : {itemCount} / {maxSlots}
        무게  : {currentWeight:F1} / {maxWeight:F1} kg

        [경제]
        골드  : {gold} G

        [퀘스트]
        {GetQuestInfo()}

        [씬]
        현재 씬 : {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}
        플레이타임 : {(SaveSystem.Instance != null ? SaveSystem.Instance.FormatPlayTime(0f) : "N/A")}";
    }

    private string GetUpgradeName(string tool, int level)
    {
        return (tool, level) switch
        {
            ("낚싯대", 0) => "나무",
            ("낚싯대", 1) => "대나무",
            ("낚싯대", 2) => "마력섬유",
            ("릴",     0) => "청동",
            ("릴",     1) => "강철",
            ("릴",     2) => "마력코팅",
            ("낚싯줄", 0) => "일반",
            ("낚싯줄", 1) => "정령거미",
            ("낚싯줄", 2) => "마력줄",
            ("양동이", 0) => "나무",
            ("양동이", 1) => "철",
            ("양동이", 2) => "마력박스",
            ("보트",   0) => "나무",
            ("보트",   1) => "철",
            ("보트",   2) => "마력엔진",
            _             => "알 수 없음"
        };
    }

    private string GetQuestInfo()
    {
        if (QuestSystem.Instance?.CurrentQuest == null)
            return "퀘스트 없음";

        return $"제목  : {QuestSystem.Instance.CurrentQuest.questTitle}\n" +
               $"진행도 : {QuestSystem.Instance.CurrentProgress}" +
               $"/{QuestSystem.Instance.CurrentQuest.targetCount}\n" +
               $"상태  : {QuestSystem.Instance.State}";
    }
}