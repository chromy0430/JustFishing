using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public enum SlotMode { MainMenu, InGame }

    [Header("데이터 있을 때")]
    [SerializeField] private GameObject      dataPanel;
    [SerializeField] private TextMeshProUGUI saveTimeTxt;
    [SerializeField] private TextMeshProUGUI playTimeTxt;
    [SerializeField] private TextMeshProUGUI goldTxt;
    [SerializeField] private TextMeshProUGUI boatLevelTxt;
    [SerializeField] private TextMeshProUGUI fishCountTxt;

    [Header("데이터 없을 때")]
    [SerializeField] private GameObject emptyPanel;

    // 슬롯 전체를 버튼으로
    [SerializeField] private Button slotButton;

    private int _slotIndex;

    // 메인메뉴용 (로드만)
    public void InitForLoad(int slotIndex, SaveData data, Action<int> onLoad)
    {
        _slotIndex = slotIndex;

        slotButton.onClick.RemoveAllListeners();

        if (data == null)
        {
            dataPanel.SetActive(false);
            emptyPanel.SetActive(true);
            // 빈 슬롯은 로드 불가
            slotButton.interactable = false;
            return;
        }

        slotButton.interactable = true;
        slotButton.onClick.AddListener(() => onLoad(slotIndex));
        RefreshDataUI(data);
    }

    // 인게임용 (저장 - 빈 슬롯도 클릭 가능)
    public void InitForSave(int slotIndex, SaveData data, Action<int> onSave)
    {
        _slotIndex = slotIndex;

        slotButton.interactable = true;
        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() => onSave(slotIndex));

        if (data == null)
        {
            dataPanel.SetActive(false);
            emptyPanel.SetActive(true);
            return;
        }

        RefreshDataUI(data);
    }

    private void RefreshDataUI(SaveData data)
    {
        dataPanel.SetActive(true);
        emptyPanel.SetActive(false);

        saveTimeTxt.text  = $"저장: {data.saveTime}";
        playTimeTxt.text  = $"플레이: {SaveSystem.Instance.FormatPlayTime(data.playTime)}";
        goldTxt.text      = $"골드: {data.gold}G";
        boatLevelTxt.text = $"보트: {GetBoatName(data.upgradeBoat)}";
        fishCountTxt.text = $"물고기: {data.inventory.Count}마리";
    }

    private string GetBoatName(int level) => level switch
    {
        0 => "나무 보트",
        1 => "철 보트",
        2 => "마력엔진 보트",
        _ => "나무 보트"
    };
}