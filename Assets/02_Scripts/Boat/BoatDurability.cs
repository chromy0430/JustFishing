using System;
using UnityEngine;

public class BoatDurability : MonoBehaviour
{
    [SerializeField] private WaterZoneController waterZone;

    private BoatLevelData _levelData;
    private const string  KEY_DURABILITY = "BoatDurability";

    public float MaxDurability     => _levelData?.maxDurability ?? 150f;
    public float CurrentDurability { get; private set; }
    public float DurabilityRatio   => CurrentDurability / MaxDurability;

    public event Action<float, float> OnDurabilityChanged;
    public event Action               OnDurabilityEmpty;

    public void SetLevelData(BoatLevelData levelData)
    {
        _levelData        = levelData;
        CurrentDurability = PlayerPrefs.GetFloat(KEY_DURABILITY, MaxDurability);
        OnDurabilityChanged?.Invoke(CurrentDurability, MaxDurability);
    }

    private void Update()
    {
        if (_levelData == null || CurrentDurability <= 0f) return;

        int   zone       = GetCurrentZone();
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
            : Camera.main?.transform;

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
        if (_levelData == null) return 0f;
        return zone switch
        {
            1 => _levelData.zone1DamagePerSec,
            2 => _levelData.zone2DamagePerSec,
            _ => _levelData.zone3DamagePerSec,
        };
    }

    public void FullRepair()
    {
        CurrentDurability = MaxDurability;
        OnDurabilityChanged?.Invoke(CurrentDurability, MaxDurability);
        SaveDurability();
    }

    public void SaveDurability()
    {
        PlayerPrefs.SetFloat(KEY_DURABILITY, CurrentDurability);
        PlayerPrefs.Save();
    }

    public void GetWaterZone(WaterZoneController waterZone)
    {
        this.waterZone = waterZone;
    }
}