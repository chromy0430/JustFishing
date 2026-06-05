using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoatHUD : MonoBehaviour
{
    [Header("내구도")]
    [SerializeField] private Image           durabilityFill;
    [SerializeField] private TextMeshProUGUI durabilityTxt;
    [SerializeField] private Image           durabilityBarColor; // 내구도에 따라 색상 변경

    [Header("골드")]
    [SerializeField] private TextMeshProUGUI goldTxt;

    [Header("색상")]
    [SerializeField] private Color highColor   = Color.green;
    [SerializeField] private Color midColor    = Color.yellow;
    [SerializeField] private Color lowColor    = Color.red;

    private BoatDurability _boatDurability;

    private void Start()
    {
        _boatDurability = FindFirstObjectByType<BoatDurability>();

        if (_boatDurability != null)
            _boatDurability.OnDurabilityChanged += UpdateDurabilityUI;

        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnGoldChanged += UpdateGoldUI;

        // 초기값 설정
        UpdateGoldUI(PlayerWallet.Instance?.Gold ?? 0);

        if (_boatDurability != null)
            UpdateDurabilityUI(
                _boatDurability.CurrentDurability,
                _boatDurability.MaxDurability);
    }

    private void OnDestroy()
    {
        if (_boatDurability != null)
            _boatDurability.OnDurabilityChanged -= UpdateDurabilityUI;

        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnGoldChanged -= UpdateGoldUI;
    }

    private void UpdateDurabilityUI(float current, float max)
    {
        float ratio               = current / max;
        durabilityFill.fillAmount = ratio;
        durabilityTxt.text        = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";

        // 내구도에 따라 색상 변경
        Color barColor;
        if      (ratio > 0.5f) barColor = Color.Lerp(midColor,  highColor, (ratio - 0.5f) * 2f);
        else if (ratio > 0.25f) barColor = Color.Lerp(lowColor, midColor,  (ratio - 0.25f) * 4f);
        else                    barColor = lowColor;

        durabilityBarColor.color = barColor;
    }

    private void UpdateGoldUI(int gold)
    {
        goldTxt.text = $"{gold} G";
    }
}