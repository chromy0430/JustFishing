using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopSellSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image           fishIcon;

    public FishInstance FishInstance { get; private set; }
    public bool         IsEmpty      => FishInstance == null;

    private System.Action<ShopSellSlot> _onClick;

    public void Init(FishInstance fish, System.Action<ShopSellSlot> onClick)
    {
        FishInstance       = fish;
        _onClick           = onClick;

        fishIcon.sprite    = fish.fishData.fishSprite;
        fishIcon.gameObject.SetActive(true);

    }

    public void SetEmpty()
    {
        FishInstance       = null;
        fishIcon.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsEmpty) return;
        _onClick?.Invoke(this);
    }
}