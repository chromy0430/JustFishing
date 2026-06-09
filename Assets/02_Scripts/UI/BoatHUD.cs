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
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnGoldChanged += UpdateGoldUI;

        UpdateGoldUI(PlayerWallet.Instance?.Gold ?? 0);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeDurability();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드 후 BoatDurability 재탐색
        UnsubscribeDurability();
        StartCoroutine(FindDurabilityNextFrame());
    }
    
    private System.Collections.IEnumerator FindDurabilityNextFrame()
    {
        // BoatSpawner가 보트를 생성할 때까지 한 프레임 대기
        yield return null;
        yield return null;

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
            // Island 씬 등 보트 없는 씬 - 저장된 값 표시
            float saved = PlayerPrefs.GetFloat("BoatDurability", 150f);
            int   level = PlayerPrefs.GetInt("Upgrade_보트", 0);
            float maxDur = level == 0 ? 150f : level == 1 ? 300f : 1000f;
            UpdateDurabilityUI(saved, maxDur);
        }
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
        float ratio = current / max;
        durabilityFill.fillAmount = ratio;
        durabilityTxt.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";

        Color barColor;
        if      (ratio > 0.5f)  barColor = Color.Lerp(midColor,  highColor, (ratio - 0.5f) * 2f);
        else if (ratio > 0.25f) barColor = Color.Lerp(lowColor,  midColor,  (ratio - 0.25f) * 4f);
        else                    barColor = lowColor;

        durabilityBarColor.color = barColor;
    }

    private void UpdateGoldUI(int gold)
    {
        if (goldTxt != null)
            goldTxt.text = $"{gold} G";
    }
}