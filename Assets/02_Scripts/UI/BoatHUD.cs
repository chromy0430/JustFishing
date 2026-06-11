using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BoatHUD : MonoBehaviour
{
    public static BoatHUD Instance { get; private set; }
    
    [Header("내구도")]
    [SerializeField] private Image           durabilityFill;
    [SerializeField] private TextMeshProUGUI durabilityTxt;
    [SerializeField] private Image           durabilityBarColor; // 내구도에 따라 색상 변경

    [Header("골드")]
    [SerializeField] private TextMeshProUGUI goldTxt;

    [Header("색상")]
    [SerializeField] private Color highColor   = Color.green;
    [SerializeField] private Color midColor    = Color.yellow;
    [SerializeField] private Color lowColor    = Color.red;

    private BoatDurability _boatDurability;
    
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Start에서 구독 시도 (이미 있으면 한 번 더 연결 시도)
        SubscribeWallet();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeWallet();
        UnsubscribeDurability();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드 후 PlayerWallet 재구독 (씬 전환 시 Instance가 바뀔 수 있음)
        UnsubscribeWallet();
        SubscribeWallet();

        // BoatDurability 재탐색
        UnsubscribeDurability();
        StartCoroutine(FindDurabilityNextFrame());
    }
    
    private System.Collections.IEnumerator FindDurabilityNextFrame()
    {
        yield return null;
        yield return null;

        UnsubscribeDurability();
        _boatDurability = FindFirstObjectByType<BoatDurability>();

        if (_boatDurability != null)
        {
            _boatDurability.OnDurabilityChanged += UpdateDurabilityUI;
            UpdateDurabilityUI(
                _boatDurability.CurrentDurability,
                _boatDurability.MaxDurability);
        }
        else
        {
            // Island 씬 등 보트 없는 씬 - PlayerPrefs 저장값 표시
            float saved  = PlayerPrefs.GetFloat("BoatDurability", 150f);
            int   level  = PlayerPrefs.GetInt("Upgrade_보트", 0);
            float maxDur = level == 0 ? 150f : level == 1 ? 300f : 1000f;
            UpdateDurabilityUI(saved, maxDur);
        }
    }
    
    // ── PlayerWallet 구독 ──────────────────────────────────

    private void SubscribeWallet()
    {
        if (PlayerWallet.Instance == null)
        {
            // PlayerWallet이 아직 없으면 다음 프레임에 재시도
            StartCoroutine(SubscribeWalletNextFrame());
            return;
        }

        PlayerWallet.Instance.OnGoldChanged += UpdateGoldUI;

        // 현재 골드 즉시 반영
        UpdateGoldUI(PlayerWallet.Instance.Gold);
    }

    private System.Collections.IEnumerator SubscribeWalletNextFrame()
    {
        yield return null;
        yield return null;

        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(PlayerWallet.Instance.Gold);
        }
        else
        {
            Debug.LogWarning("BoatHUD: PlayerWallet.Instance가 null");
        }
    }

    private void UnsubscribeWallet()
    {
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnGoldChanged -= UpdateGoldUI;
    }
    
    private void UnsubscribeDurability()
    {
        if (_boatDurability != null)
        {
            _boatDurability.OnDurabilityChanged -= UpdateDurabilityUI;
            _boatDurability = null;
        }
    }

    private void UpdateDurabilityUI(float current, float max)
    {
        float ratio = max > 0f ? current / max : 0f;
        if (durabilityFill != null) durabilityFill.fillAmount = ratio;

        if (durabilityTxt != null)
            durabilityTxt.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";

        if (durabilityBarColor != null)
        {
            Color barColor;
            if      (ratio > 0.5f)  barColor = Color.Lerp(midColor, highColor, (ratio - 0.5f) * 2f);
            else if (ratio > 0.25f) barColor = Color.Lerp(lowColor, midColor,  (ratio - 0.25f) * 4f);
            else                    barColor = lowColor;
            durabilityBarColor.color = barColor;
        }
    }

    private void UpdateGoldUI(int gold)
    {
        if (goldTxt != null)
            goldTxt.text = $"{gold} G";
    }
}