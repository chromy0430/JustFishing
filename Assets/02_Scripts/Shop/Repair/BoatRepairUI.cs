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
    [SerializeField] private TextMeshProUGUI repairPercentTxt;  // 수리 필요량 %
    [SerializeField] private TextMeshProUGUI repairCostTxt;     // 수리 비용
    [SerializeField] private Button          repairButton;

    [Header("Data")]
    [SerializeField] private BoatDurabilityData boatData;

    // 수리 비용 계산 (내구도 1당 골드)
    private const float REPAIR_COST_PER_DURABILITY = 10f;

    private BoatDurability _boatDurability;

    private void OnEnable()
    {
        // BoatDurability를 씬에서 찾기
        _boatDurability = FindFirstObjectByType<BoatDurability>();

        if (_boatDurability != null)
            _boatDurability.OnDurabilityChanged += RefreshUI;

        RefreshUI(
            _boatDurability?.CurrentDurability ?? boatData.maxDurability,
            boatData.maxDurability);
    }

    private void OnDisable()
    {
        if (_boatDurability != null)
            _boatDurability.OnDurabilityChanged -= RefreshUI;
    }

    private void Start()
    {
        repairButton.onClick.AddListener(OnRepairClick);
        boatNameTxt.text = boatData.boatName;
    }

    private void RefreshUI(float current, float max)
    {
        float ratio        = current / max;
        durabilityFill.fillAmount = ratio;
        durabilityTxt.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";

        // 수리 필요량
        float missingDurability = max - current;
        float repairPercent     = (1f - ratio) * 100f;
        int   repairCost        = Mathf.RoundToInt(
            missingDurability * REPAIR_COST_PER_DURABILITY);

        repairPercentTxt.text = $"{repairPercent:F0}%";
        repairCostTxt.text    = $"{repairCost} G";

        // 내구도 가득 차있으면 버튼 비활성화
        repairButton.interactable = current < max;
    }

    private void OnRepairClick()
    {
        if (_boatDurability == null) return;

        float missingDurability = boatData.maxDurability - _boatDurability.CurrentDurability;
        int   repairCost        = Mathf.RoundToInt(
            missingDurability * REPAIR_COST_PER_DURABILITY);

        if (!PlayerWallet.Instance.SpendGold(repairCost))
        {
            Debug.Log("골드 부족");
            return;
        }

        _boatDurability.FullRepair();
        Debug.Log($"수리 완료: {repairCost}G 소비");
    }
}