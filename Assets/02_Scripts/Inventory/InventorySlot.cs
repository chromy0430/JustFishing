using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image          fishIcon;
    [SerializeField] private GameObject     emptySlot;
    [SerializeField] private Button         discardButton;  // 버리기 버튼
    [SerializeField] private GameObject     discardPanel;   // 버리기 확인 패널
    [SerializeField] private Button         confirmDiscard;
    [SerializeField] private Button         cancelDiscard;

    private FishInstance     _fishInstance;
    private InventoryTooltip _tooltip;

    public void Init(FishInstance fish, InventoryTooltip tooltip)
    {
        _fishInstance = fish;
        _tooltip      = tooltip;

        fishIcon.sprite = fish.fishData.fishSprite;
        fishIcon.gameObject.SetActive(true);
        emptySlot.SetActive(false);
        discardPanel.SetActive(false);

        discardButton.onClick.AddListener(OnDiscardClick);
        confirmDiscard.onClick.AddListener(OnConfirmDiscard);
        cancelDiscard.onClick.AddListener(OnCancelDiscard);
    }

    public void SetEmpty()
    {
        _fishInstance = null;
        fishIcon.gameObject.SetActive(false);
        emptySlot.SetActive(true);
        discardPanel.SetActive(false);
    }

    // 툴팁 표시
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_fishInstance == null) return;
        _tooltip?.Show(_fishInstance);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip?.Hide();
    }

    // 버리기
    private void OnDiscardClick()
    {
        if (_fishInstance == null) return;
        discardPanel.SetActive(true);
    }

    private void OnConfirmDiscard()
    {
        InventorySystem.Instance.DiscardFish(_fishInstance);
        discardPanel.SetActive(false);
    }

    private void OnCancelDiscard()
    {
        discardPanel.SetActive(false);
    }
}