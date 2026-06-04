using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryTooltip : MonoBehaviour
{
    [SerializeField] private GameObject      tooltipPanel;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI descTxt;
    [SerializeField] private TextMeshProUGUI sizeTxt;
    [SerializeField] private TextMeshProUGUI priceTxt;

    private void Awake()
    {
        tooltipPanel.SetActive(false);
    }

    public void Show(FishInstance fish)
    {
        nameTxt.text  = fish.fishData.fishName;
        descTxt.text  = fish.fishData.fishDescription;
        sizeTxt.text  = $"길이: {fish.length:F1}cm  무게: {fish.weight:F1}kg";
        priceTxt.text = $"가격: {fish.price}G";

        tooltipPanel.SetActive(true);
        UpdatePosition();
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
            UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector2       mousePos = Mouse.current.position.ReadValue();
        RectTransform rt       = tooltipPanel.GetComponent<RectTransform>();
        Vector2       offset   = new Vector2(200f, -50f);

        Vector2 targetPos = new Vector2(mousePos.x + offset.x, mousePos.y + offset.y);

        // 툴팁 크기
        float tooltipW = rt.rect.width;
        float tooltipH = rt.rect.height;

        // 화면 크기
        float screenW  = Screen.width;
        float screenH  = Screen.height;

        // 오른쪽 밖으로 나가면 왼쪽으로
        if (targetPos.x + tooltipW > screenW)
            targetPos.x = mousePos.x - tooltipW - 10f;

        // 위쪽 밖으로 나가면 아래로
        if (targetPos.y + tooltipH > screenH)
            targetPos.y = mousePos.y - tooltipH - 10f;

        // 왼쪽 밖으로 나가면 오른쪽으로
        if (targetPos.x < 0f)
            targetPos.x = 10f;

        // 아래쪽 밖으로 나가면 위로
        if (targetPos.y < 0f)
            targetPos.y = 10f;

        rt.position = new Vector3(targetPos.x, targetPos.y, 0f);
    }
}