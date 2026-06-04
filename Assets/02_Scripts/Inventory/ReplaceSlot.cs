using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReplaceSlot : MonoBehaviour
{
    [SerializeField] private Image          fishIcon;
    [SerializeField] private TextMeshProUGUI infoTxt;
    [SerializeField] private Button          selectButton;

    public void Init(FishInstance fish, Action<FishInstance> onSelect)
    {
        fishIcon.sprite = fish.fishData.fishSprite;
        infoTxt.text    = $"{fish.fishData.fishName}\n" +
                          $"{fish.weight:F1}kg  {fish.price}G";

        selectButton.onClick.AddListener(() => onSelect(fish));
    }
}