using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    private const string KEY_GOLD = "PlayerGold";

    public int Gold { get; private set; }

    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Gold = PlayerPrefs.GetInt(KEY_GOLD, 0);
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        PlayerPrefs.SetInt(KEY_GOLD, Gold);
        PlayerPrefs.Save();
        OnGoldChanged?.Invoke(Gold);
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        PlayerPrefs.SetInt(KEY_GOLD, Gold);
        PlayerPrefs.Save();
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
}