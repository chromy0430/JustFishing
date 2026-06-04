using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSellUI : MonoBehaviour
{
    [Header("판매 슬롯 (왼쪽)")]
    [SerializeField] private Transform  sellSlotParent;   // ScrollRect Content
    [SerializeField] private GameObject sellSlotPrefab;

    [Header("인벤토리 슬롯 (오른쪽)")]
    [SerializeField] private Transform  invSlotParent;    // ScrollRect Content
    [SerializeField] private GameObject invSlotPrefab;

    [Header("하단 UI")]
    [SerializeField] private TextMeshProUGUI totalPriceTxt;
    [SerializeField] private Button          sellButton;

    private List<ShopSellSlot> _sellSlots = new List<ShopSellSlot>();
    private List<ShopSellSlot> _invSlots  = new List<ShopSellSlot>();

    // 판매 목록 (왼쪽으로 옮겨진 물고기)
    private List<FishInstance> _pendingSell = new List<FishInstance>();
    private int                _totalPrice  = 0;

    private void OnEnable()
    {
        // null 체크 추가
        if (InventorySystem.Instance == null) return;

        RefreshAll();
        InventorySystem.Instance.OnInventoryChanged += RefreshInventorySlots;
    }

    private void OnDisable()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= RefreshInventorySlots;
    }

    private void Start()
    {
        sellButton.onClick.AddListener(OnSellClick);
    }

    // ==================== 전체 갱신 ====================

    private void RefreshAll()
    {
        _pendingSell.Clear();
        _totalPrice = 0;
        UpdateTotalPriceUI();

        RefreshSellSlots();
        RefreshInventorySlots();
    }

    // ==================== 판매 슬롯 (왼쪽) ====================

    private void RefreshSellSlots()
    {
        // 기존 슬롯 정리
        foreach (ShopSellSlot slot in _sellSlots)
            Destroy(slot.gameObject);
        _sellSlots.Clear();

        // pendingSell 기준으로 슬롯 생성
        foreach (FishInstance fish in _pendingSell)
        {
            ShopSellSlot slot = CreateSellSlot();
            slot.Init(fish, OnSellSlotClick);
        }

        // 최소 9칸 유지 (빈 슬롯)
        int emptyCount = Mathf.Max(0, 9 - _sellSlots.Count);
        for (int i = 0; i < emptyCount; i++)
        {
            ShopSellSlot slot = CreateSellSlot();
            slot.SetEmpty();
        }
    }

    private ShopSellSlot CreateSellSlot()
    {
        GameObject    obj  = Instantiate(sellSlotPrefab, sellSlotParent);
        ShopSellSlot  slot = obj.GetComponent<ShopSellSlot>();
        _sellSlots.Add(slot);
        return slot;
    }

    // 왼쪽 슬롯 클릭 → 다시 인벤토리로 반환
    private void OnSellSlotClick(ShopSellSlot slot)
    {
        FishInstance fish = slot.FishInstance;
        _pendingSell.Remove(fish);
        _totalPrice -= fish.price;
        UpdateTotalPriceUI();

        // 인벤토리에 다시 추가
        InventorySystem.Instance.AddFish(fish);

        RefreshSellSlots();
    }

    // ==================== 인벤토리 슬롯 (오른쪽) ====================

    private void RefreshInventorySlots()
    {
        // null 체크
        if (InventorySystem.Instance == null) return;

        foreach (ShopSellSlot slot in _invSlots)
            if (slot != null) Destroy(slot.gameObject);
        _invSlots.Clear();

        // 인벤토리가 비어있어도 정상 동작
        foreach (FishInstance fish in InventorySystem.Instance.Items)
        {
            GameObject   obj  = Instantiate(invSlotPrefab, invSlotParent);
            ShopSellSlot slot = obj.GetComponent<ShopSellSlot>();
            slot.Init(fish, OnInvSlotClick);
            _invSlots.Add(slot);
        }

        // 최소 9칸 유지
        int current    = _invSlots.Count;
        int emptyCount = Mathf.Max(0, 9 - current);
        for (int i = 0; i < emptyCount; i++)
        {
            GameObject   obj  = Instantiate(invSlotPrefab, invSlotParent);
            ShopSellSlot slot = obj.GetComponent<ShopSellSlot>();
            slot.SetEmpty();
            _invSlots.Add(slot);
        }
    }

    // 오른쪽 슬롯 클릭 → 판매 목록으로 이동
    private void OnInvSlotClick(ShopSellSlot slot)
    {
        FishInstance fish = slot.FishInstance;

        // 인벤토리에서 제거
        InventorySystem.Instance.DiscardFish(fish);

        // 판매 목록에 추가
        _pendingSell.Add(fish);
        _totalPrice += fish.price;
        UpdateTotalPriceUI();

        RefreshSellSlots();
    }

    // ==================== 판매 ====================

    private void OnSellClick()
    {
        if (_pendingSell.Count == 0) return;

        // 골드 지급
        PlayerWallet.Instance?.AddGold(_totalPrice);

        Debug.Log($"판매 완료: {_pendingSell.Count}마리 / {_totalPrice}G 획득");

        // 판매 목록 초기화
        _pendingSell.Clear();
        _totalPrice = 0;
        UpdateTotalPriceUI();

        RefreshSellSlots();

        // SFX
        AudioManager.Instance?.PlayPurchase();
    }

    private void UpdateTotalPriceUI()
    {
        totalPriceTxt.text = $"{_totalPrice} G";
    }
}