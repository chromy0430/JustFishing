using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FishSaveEntry
{
    public string fishName;
    public float  length;
    public float  weight;
    public int    price;
}

[Serializable]
public class SaveData
{
    // 메타 정보
    public int    slotIndex;
    public string saveTime;       // 저장 시간
    public float  playTime;       // 플레이 시간 (초)

    // 경제
    public int    gold;

    // 강화 레벨
    public int    upgradeRod;
    public int    upgradeReel;
    public int    upgradeLine;
    public int    upgradeBucket;
    public int    upgradeBoat;

    // 내구도
    public float  boatDurability;

    // 인벤토리
    public List<FishSaveEntry> inventory = new List<FishSaveEntry>();

    // 퀘스트
    public string questTitle;
    public int    questProgress;
    public int    questState;
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    public const int SLOT_COUNT = 3;

    // 현재 로드된 슬롯 인덱스 (-1 = 새 게임)
    public int CurrentSlot { get; private set; } = -1;

    // 플레이 시간 측정
    private float _playTime = 0f;
    private bool  _isTracking = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (_isTracking)
            _playTime += Time.deltaTime;
    }

    public void StartTracking(float initialPlayTime = 0f)
    {
        _playTime    = initialPlayTime;
        _isTracking  = true;
    }

    public void StopTracking() => _isTracking = false;

    // ==================== 저장 ====================

    public void Save(int slotIndex)
    {
        SaveData data = new SaveData();

        data.slotIndex      = slotIndex;
        data.saveTime       = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        data.playTime       = _playTime;
        data.gold           = PlayerWallet.Instance?.Gold ?? 0;

        // 강화 레벨
        data.upgradeRod     = PlayerPrefs.GetInt("Upgrade_낚싯대", 0);
        data.upgradeReel    = PlayerPrefs.GetInt("Upgrade_릴",     0);
        data.upgradeLine    = PlayerPrefs.GetInt("Upgrade_낚싯줄", 0);
        data.upgradeBucket  = PlayerPrefs.GetInt("Upgrade_양동이", 0);
        data.upgradeBoat    = PlayerPrefs.GetInt("Upgrade_보트",   0);

        // 내구도
        data.boatDurability = PlayerPrefs.GetFloat("BoatDurability",
            PlayerPrefs.GetFloat("Upgrade_보트", 0) == 0 ? 150f :
            PlayerPrefs.GetFloat("Upgrade_보트", 0) == 1 ? 300f : 1000f);

        // 인벤토리
        if (InventorySystem.Instance != null)
        {
            foreach (FishInstance fish in InventorySystem.Instance.Items)
            {
                data.inventory.Add(new FishSaveEntry
                {
                    fishName = fish.fishData.fishName,
                    length   = fish.length,
                    weight   = fish.weight,
                    price    = fish.price
                });
            }
        }

        // 퀘스트
        if (QuestSystem.Instance != null)
        {
            data.questTitle    = QuestSystem.Instance.CurrentQuest?.questTitle ?? "";
            data.questProgress = QuestSystem.Instance.CurrentProgress;
            data.questState    = (int)QuestSystem.Instance.State;
        }

        string json = JsonUtility.ToJson(data, true);
        string key  = GetSlotKey(slotIndex);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();

        Debug.Log($"슬롯 {slotIndex + 1} 저장 완료");
        NotificationManager.Instance?.ShowMessage($"슬롯 {slotIndex + 1}에 저장되었습니다.");
    }

    // ==================== 불러오기 ====================

    public bool Load(int slotIndex)
    {
        string json = PlayerPrefs.GetString(GetSlotKey(slotIndex), "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log($"슬롯 {slotIndex + 1}에 저장 데이터 없음");
            return false;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(json);
        CurrentSlot   = slotIndex;

        ApplyData(data);
        StartTracking(data.playTime);

        Debug.Log($"슬롯 {slotIndex + 1} 로드 완료");
        return true;
    }

    private void ApplyData(SaveData data)
    {
        // 골드
        PlayerPrefs.SetInt("PlayerGold", data.gold);

        // 강화 레벨
        PlayerPrefs.SetInt("Upgrade_낚싯대", data.upgradeRod);
        PlayerPrefs.SetInt("Upgrade_릴",     data.upgradeReel);
        PlayerPrefs.SetInt("Upgrade_낚싯줄", data.upgradeLine);
        PlayerPrefs.SetInt("Upgrade_양동이", data.upgradeBucket);
        PlayerPrefs.SetInt("Upgrade_보트",   data.upgradeBoat);

        // 내구도
        PlayerPrefs.SetFloat("BoatDurability", data.boatDurability);

        // 퀘스트
        PlayerPrefs.SetString("QuestID",    data.questTitle);
        PlayerPrefs.SetInt("QuestProgress", data.questProgress);
        PlayerPrefs.SetInt("QuestState",    data.questState);

        PlayerPrefs.Save();

        // 인벤토리는 씬 로드 후 적용 (LoadInventoryData로 별도 처리)
    }

    // 씬 로드 완료 후 InventorySystem이 준비된 시점에 호출
    public void ApplyInventoryData(int slotIndex, List<FishData> allFishData)
    {
        string json = PlayerPrefs.GetString(GetSlotKey(slotIndex), "");
        if (string.IsNullOrEmpty(json)) return;

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (InventorySystem.Instance == null) return;

        foreach (FishSaveEntry entry in data.inventory)
        {
            FishData fishData = allFishData.Find(f => f.fishName == entry.fishName);
            if (fishData == null) continue;

            FishInstance fish = new FishInstance(fishData);
            // 저장된 수치로 덮어씌우기
            fish.OverrideValues(entry.length, entry.weight, entry.price);
            InventorySystem.Instance.AddFish(fish);
        }
    }

    // ==================== 새 게임 ====================

    public void NewGame()
    {
        CurrentSlot = -1;
        _playTime   = 0f;

        // 런타임 데이터만 초기화 (PlayerPrefs는 건드리지 않음)
        PlayerWallet.Instance?.ResetGold();
        InventorySystem.Instance?.ClearInventory();
        QuestSystem.Instance?.GenerateNewQuest();

        StartTracking(0f);
    }

    // ==================== 슬롯 정보 ====================

    public SaveData GetSlotData(int slotIndex)
    {
        string json = PlayerPrefs.GetString(GetSlotKey(slotIndex), "");
        if (string.IsNullOrEmpty(json)) return null;
        return JsonUtility.FromJson<SaveData>(json);
    }

    public bool HasSaveData(int slotIndex)
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString(GetSlotKey(slotIndex), ""));
    }

    public string FormatPlayTime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)(seconds % 3600 / 60);
        int s = (int)(seconds % 60);
        return h > 0 ? $"{h}시간 {m}분" : $"{m}분 {s}초";
    }

    private string GetSlotKey(int index) => $"SaveSlot_{index}";
}