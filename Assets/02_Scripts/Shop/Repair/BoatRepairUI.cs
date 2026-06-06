using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoatRepairUI : MonoBehaviour
{
    [Header("보트 정보")]
    [SerializeField] private Image           boatIcon;
    [SerializeField] private TextMeshProUGUI boatNameTxt;
    [SerializeField] private Image           durabilityFill;
    [SerializeField] private TextMeshProUGUI durabilityTxt;

    [Header("수리 정보")]
    [SerializeField] private TextMeshProUGUI repairPercentTxt;
    [SerializeField] private TextMeshProUGUI repairCostTxt;
    [SerializeField] private Button          repairButton;

    [Header("Data")]
    [SerializeField] private BoatData boatData;

    private const float REPAIR_COST_PER_DURABILITY = 10f;

    private BoatLevelData  _currentLevelData;
    private float          _savedDurability;
    private float          _maxDurability;

    private void OnEnable()
    {
        LoadBoatData();
        RefreshUI();
    }

    private void Start()
    {
        repairButton.onClick.AddListener(OnRepairClick);
    }

    private void LoadBoatData()
    {
        // PlayerPrefs에서 현재 보트 레벨 읽기
        int level = PlayerPrefs.GetInt("Upgrade_보트", 0);
        level = Mathf.Clamp(level, 0, boatData.levels.Length - 1);


        boatIcon.sprite = boatData.levels[level].boatIcon;
        _currentLevelData = boatData.levels[level];
        _maxDurability    = _currentLevelData.maxDurability;
        _savedDurability  = PlayerPrefs.GetFloat("BoatDurability", _maxDurability);

        boatNameTxt.text  = _currentLevelData.boatName;
    }

    private void RefreshUI()
    {
        float ratio = _savedDurability / _maxDurability;

        durabilityFill.fillAmount = ratio;
        durabilityTxt.text        = $"{Mathf.RoundToInt(_savedDurability)}" +
                                    $"/{Mathf.RoundToInt(_maxDurability)}";

        float missingDurability = _maxDurability - _savedDurability;
        float repairPercent     = (1f - ratio) * 100f;
        int   repairCost        = Mathf.RoundToInt(
            missingDurability * REPAIR_COST_PER_DURABILITY);

        repairPercentTxt.text = $"{repairPercent:F0}%";
        repairCostTxt.text    = $"{repairCost} G";

        repairButton.interactable = _savedDurability < _maxDurability;
    }

    private void OnRepairClick()
    {
        float missingDurability = _maxDurability - _savedDurability;
        int   repairCost        = Mathf.RoundToInt(
            missingDurability * REPAIR_COST_PER_DURABILITY);

        if (!PlayerWallet.Instance.SpendGold(repairCost))
        {
            NotificationManager.Instance?.ShowMessage("골드가 부족합니다.");
            return;
        }

        // 내구도 복구 후 저장
        _savedDurability = _maxDurability;
        PlayerPrefs.SetFloat("BoatDurability", _savedDurability);
        PlayerPrefs.Save();

        RefreshUI();
        NotificationManager.Instance?.ShowMessage(
            $"{_currentLevelData.boatName} 수리 완료!");
    }
}