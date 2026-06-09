using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    public static UpgradeSystem Instance { get; private set; }
    
    [SerializeField] private BoatData boatData;

    [SerializeField] private List<UpgradeData> upgradeDataList;
    // UpgradeSystem.cs - UpgradeDataList 공개
    public List<UpgradeData> UpgradeDataList => upgradeDataList;

    private Dictionary<string, int> _levels = new Dictionary<string, int>();
    private const string KEY_PREFIX = "Upgrade_";

    public event Action OnUpgradeChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        LoadUpgrades();
    }

    private void LoadUpgrades()
    {
        foreach (UpgradeData data in upgradeDataList)
        {
            int level = PlayerPrefs.GetInt(KEY_PREFIX + data.toolName, 0);
            _levels[data.toolName] = level;
        }
    }

    public int GetCurrentLevel(string toolName)
    {
        return _levels.TryGetValue(toolName, out int level) ? level : 0;
    }

    public enum UpgradeResult
    {
        Success,
        AlreadyMaxLevel,
        NotEnoughGold,
        NotEnoughFish
    }

    public UpgradeResult TryUpgrade(UpgradeData data)
    {
        if (PlayerWallet.Instance == null)
        {
            Debug.LogError("PlayerWallet.Instance null");
            return UpgradeResult.AlreadyMaxLevel;
        }

        int currentLevel = GetCurrentLevel(data.toolName);

        if (currentLevel >= data.levels.Length - 1)
            return UpgradeResult.AlreadyMaxLevel;

        UpgradeLevel next = data.levels[currentLevel + 1];

        if (PlayerWallet.Instance.Gold < next.goldCost)
            return UpgradeResult.NotEnoughGold;

        if (next.requiredFish != null)
        {
            if (InventorySystem.Instance == null)
            {
                Debug.LogError("InventorySystem.Instance null");
                return UpgradeResult.NotEnoughFish;
            }

            if (CountFish(next.requiredFish) < next.requiredFishCount)
                return UpgradeResult.NotEnoughFish;

            ConsumeFish(next.requiredFish, next.requiredFishCount);
        }

        PlayerWallet.Instance.SpendGold(next.goldCost);

        _levels[data.toolName] = currentLevel + 1;
        PlayerPrefs.SetInt(KEY_PREFIX + data.toolName, currentLevel + 1);
        PlayerPrefs.Save();

        if (data.toolName == "양동이")
            InventorySystem.Instance?.UpgradeBucket();
        
        if (data.toolName == "보트")
        {
            int newLevel = _levels[data.toolName];
            if (boatData != null && newLevel < boatData.levels.Length)
            {
                float newMaxDur = boatData.levels[newLevel].maxDurability;
                PlayerPrefs.SetFloat("BoatDurability", newMaxDur);
                PlayerPrefs.Save();
                Debug.Log($"보트 업그레이드 → 내구도 {newMaxDur}으로 초기화");
            }
        }

        AudioManager.Instance?.PlayEnhance();
        OnUpgradeChanged?.Invoke();

        return UpgradeResult.Success;
    }

    private int CountFish(FishData fishData)
    {
        int count = 0;
        foreach (FishInstance fish in InventorySystem.Instance.Items)
            if (fish.fishData == fishData) count++;
        return count;
    }

    private void ConsumeFish(FishData fishData, int count)
    {
        int consumed = 0;
        var toRemove = new List<FishInstance>();
        foreach (FishInstance fish in InventorySystem.Instance.Items)
        {
            if (fish.fishData == fishData && consumed < count)
            {
                toRemove.Add(fish);
                consumed++;
            }
        }
        foreach (FishInstance fish in toRemove)
            InventorySystem.Instance.DiscardFish(fish);
    }
}