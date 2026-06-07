using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [SerializeField] private BucketData bucketData;

    private int              _currentLevel   = 0; // 0 = 1레벨
    private List<FishInstance> _items        = new List<FishInstance>();

    public int   MaxSlots  => bucketData.levels[_currentLevel].maxSlots;
    public float MaxWeight => bucketData.levels[_currentLevel].maxWeight;
    public float CurrentWeight
    {
        get
        {
            float total = 0f;
            foreach (FishInstance fish in _items)
                total += fish.weight;
            return total;
        }
    }

    public IReadOnlyList<FishInstance> Items => _items;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 물고기 추가 시도 - 성공/실패/교체필요 반환
    public enum AddResult { Success, SlotFull, WeightFull, BothFull }

    public AddResult TryAddFish(FishInstance fish)
    {
        bool slotFull   = _items.Count  >= MaxSlots;
        bool weightFull = CurrentWeight + fish.weight > MaxWeight;
        
        Debug.Log($"슬롯: {_items.Count}/{MaxSlots}, 무게: {CurrentWeight}/{MaxWeight}, 추가무게: {fish.weight}");

        if (slotFull || weightFull)
        {
            if (slotFull && weightFull) return AddResult.BothFull;
            if (slotFull)               return AddResult.SlotFull;
            return AddResult.WeightFull;
        }

        AddFish(fish);
        return AddResult.Success;
    }

    public void AddFish(FishInstance fish)
    {
        _items.Add(fish);
        OnInventoryChanged?.Invoke();
    }

    // 교체 (기존 슬롯 제거 후 새 물고기 추가)
    public void ReplaceFish(FishInstance oldFish, FishInstance newFish)
    {
        _items.Remove(oldFish);
        _items.Add(newFish);
        OnInventoryChanged?.Invoke();
    }

    // 버리기
    public void DiscardFish(FishInstance fish)
    {
        _items.Remove(fish);
        OnInventoryChanged?.Invoke();
    }

    // 양동이 레벨업
    public void UpgradeBucket()
    {
        if (_currentLevel < bucketData.levels.Length - 1)
        {
            _currentLevel++;
            OnInventoryChanged?.Invoke();
        }
    }
    
    public void ClearInventory()
    {
        _items.Clear();
        _currentLevel = 0;
        OnInventoryChanged?.Invoke();
    }

    public int   BucketLevel => _currentLevel + 1;
    public float WeightRatio => CurrentWeight / MaxWeight;
}