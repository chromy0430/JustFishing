using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryReplaceUI : MonoBehaviour
{
    public static InventoryReplaceUI Instance { get; private set; }

    [SerializeField] private GameObject      replacePanel;
    [SerializeField] private TextMeshProUGUI newFishTxt;
    [SerializeField] private TextMeshProUGUI reasonTxt;
    [SerializeField] private Transform       replaceSlotParent;
    [SerializeField] private GameObject      replaceSlotPrefab;
    [SerializeField] private Button          cancelButton;

    private FishInstance                  _newFish;
    private List<ReplaceSlot>             _replaceSlots = new List<ReplaceSlot>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        replacePanel.SetActive(false);
        cancelButton.onClick.AddListener(Hide);
    }

    public void Show(FishInstance newFish,
        InventorySystem.AddResult reason)
    {
        _newFish = newFish;

        newFishTxt.text = $"새 물고기: {newFish.fishData.fishName}\n" +
                          $"무게: {newFish.weight:F1}kg  " +
                          $"가격: {newFish.price}G";

        reasonTxt.text = reason switch
        {
            InventorySystem.AddResult.SlotFull   => "슬롯이 가득 찼습니다.",
            InventorySystem.AddResult.WeightFull => "무게 한도를 초과했습니다.",
            InventorySystem.AddResult.BothFull   => "슬롯과 무게 한도가 가득 찼습니다.",
            _                                    => ""
        };

        // 기존 슬롯 정리
        foreach (ReplaceSlot rs in _replaceSlots)
            Destroy(rs.gameObject);
        _replaceSlots.Clear();

        // 교체 가능한 슬롯 생성
        foreach (FishInstance fish in InventorySystem.Instance.Items)
        {
            GameObject  obj  = Instantiate(replaceSlotPrefab, replaceSlotParent);
            ReplaceSlot slot = obj.GetComponent<ReplaceSlot>();
            slot.Init(fish, OnReplaceSelected);
            _replaceSlots.Add(slot);
        }

        replacePanel.SetActive(true);
    }

    private void OnReplaceSelected(FishInstance oldFish)
    {
        InventorySystem.Instance.ReplaceFish(oldFish, _newFish);
        Hide();
    }

    public void Hide()
    {
        replacePanel.SetActive(false);
    }
}