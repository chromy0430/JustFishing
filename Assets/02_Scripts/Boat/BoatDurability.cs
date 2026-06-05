using System;
using UnityEngine;

public class BoatDurability : MonoBehaviour
{
    [SerializeField] private BoatDurabilityData durabilityData;
    [SerializeField] private WaterZoneController waterZone;

    private const string KEY_DURABILITY = "BoatDurability";

    public float MaxDurability  => durabilityData.maxDurability;
    public float CurrentDurability { get; private set; }
    public float DurabilityRatio   => CurrentDurability / MaxDurability;

    public event Action<float, float> OnDurabilityChanged; // current, max
    public event Action               OnDurabilityEmpty;

    private void Start()
    {
        // 저장된 내구도 불러오기 (없으면 최대값)
        CurrentDurability = PlayerPrefs.GetFloat(
            KEY_DURABILITY + durabilityData.boatName,
            durabilityData.maxDurability);

        OnDurabilityChanged?.Invoke(CurrentDurability, MaxDurability);
    }

    private void Update()
    {
        if (CurrentDurability <= 0f) return;

        int   zone      = GetCurrentZone();
        float damageRate = GetDamageRate(zone);

        if (damageRate <= 0f) return;

        CurrentDurability -= damageRate * Time.deltaTime;
        CurrentDurability  = Mathf.Max(0f, CurrentDurability);

        OnDurabilityChanged?.Invoke(CurrentDurability, MaxDurability);

        if (CurrentDurability <= 0f)
            OnDurabilityEmpty?.Invoke();
    }

    private int GetCurrentZone()
    {
        if (waterZone == null) return 1;

        Transform target = waterZone.followTarget != null
            ? waterZone.followTarget
            : (Camera.main != null ? Camera.main.transform : null);

        if (target == null) return 1;

        float dist = Vector2.Distance(
            new Vector2(waterZone.island.position.x, waterZone.island.position.z),
            new Vector2(target.position.x, target.position.z)
        );

        if (dist < waterZone.zone1Distance) return 1;
        if (dist < waterZone.zone2Distance) return 2;
        return 3;
    }

    private float GetDamageRate(int zone)
    {
        return zone switch
        {
            1 => durabilityData.zone1DamagePerSec,
            2 => durabilityData.zone2DamagePerSec,
            _ => durabilityData.zone3DamagePerSec,
        };
    }

    // 수리 시스템에서 호출
    public void Repair(float amount)
    {
        CurrentDurability = Mathf.Min(CurrentDurability + amount, MaxDurability);
        OnDurabilityChanged?.Invoke(CurrentDurability, MaxDurability);
        SaveDurability();
    }

    public void FullRepair()
    {
        CurrentDurability = MaxDurability;
        OnDurabilityChanged?.Invoke(CurrentDurability, MaxDurability);
        SaveDurability();
    }

    public void SaveDurability()
    {
        PlayerPrefs.SetFloat(
            KEY_DURABILITY + durabilityData.boatName,
            CurrentDurability);
        PlayerPrefs.Save();
    }

    public void GetWaterZone(WaterZoneController waterZone)
    {
        this.waterZone = waterZone;
    }
}