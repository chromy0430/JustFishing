using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject      inventoryPanel;
    [SerializeField] private Transform       slotParent;
    [SerializeField] private GameObject      slotPrefab;
    [SerializeField] private InventoryTooltip tooltip;

    [Header("무게 게이지")]
    [SerializeField] private Image           weightFill;
    [SerializeField] private TextMeshProUGUI weightTxt;
    [SerializeField] private TextMeshProUGUI slotTxt;
    [SerializeField] private TextMeshProUGUI bucketLevelTxt;

    private List<InventorySlot> _slots      = new List<InventorySlot>();
    private bool                _isOpen     = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(transform.root.gameObject); return; }
        Instance = this;
        
        DontDestroyOnLoad(transform.root.gameObject);

        inventoryPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (InventorySystem.Instance == null) return;
        // 중복 방지를 위해 먼저 제거 후 추가
        InventorySystem.Instance.OnInventoryChanged -= RefreshUI;
        InventorySystem.Instance.OnInventoryChanged += RefreshUI;
    }

    private void OnDisable()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= RefreshUI;
    }

    private void Update()
    {
        // I키 토글
        if (Keyboard.current.iKey.wasPressedThisFrame)
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        _isOpen = !_isOpen;
        inventoryPanel.SetActive(_isOpen);

        if (_isOpen) RefreshUI();
    }

    private void RefreshUI()
    {
        InventorySystem inv = InventorySystem.Instance;

        // 슬롯 생성/갱신
        int maxSlots = inv.MaxSlots;

        // 슬롯 수 조정
        while (_slots.Count < maxSlots)
        {
            GameObject obj  = Instantiate(slotPrefab, slotParent);
            InventorySlot slot = obj.GetComponent<InventorySlot>();
            _slots.Add(slot);
        }

        // 슬롯 내용 갱신
        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < inv.Items.Count)
                _slots[i].Init(inv.Items[i], tooltip);
            else
                _slots[i].SetEmpty();

            // 현재 레벨 최대 슬롯 초과분 비활성화
            _slots[i].gameObject.SetActive(i < maxSlots);
        }

        if (weightFill == null) return;

        // 무게 게이지
        weightFill.fillAmount = inv.WeightRatio;
        weightTxt.text        = $"{inv.CurrentWeight:F1} / {inv.MaxWeight}kg";
        slotTxt.text          = $"{inv.Items.Count} / {inv.MaxSlots}";
        bucketLevelTxt.text   = $"양동이 Lv.{inv.BucketLevel}";
    }
}