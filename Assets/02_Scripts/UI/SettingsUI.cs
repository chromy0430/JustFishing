using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get;  private set; }
    
    [Header("Graphics - 해상도")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;

    [Header("Graphics - 체크박스")]
    [SerializeField] private UICheckBox _fullscreenCheckBox;
    [SerializeField] private UICheckBox _vsyncCheckBox;

    [Header("Audio - 슬라이더")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("버튼")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _closeButton;

    // PlayerPrefs 키
    private const string KEY_RESOLUTION  = "Resolution";
    private const string KEY_FULLSCREEN  = "Fullscreen";
    private const string KEY_VSYNC       = "VSync";
    private const string KEY_BGM_VOLUME  = "BGMVolume";
    private const string KEY_SFX_VOLUME  = "SFXVolume";

    // 지원 해상도 목록
    private readonly Resolution[] _supportedResolutions = new Resolution[]
    {
        new Resolution { width = 1280, height = 720  },
        new Resolution { width = 1920, height = 1080 },
        new Resolution { width = 2560, height = 1440 },
        new Resolution { width = 3840, height = 2160 },
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        InitResolutionDropdown();
        LoadSettings();
        AddListeners();
    }

    // ==================== 초기화 ====================

    private void InitResolutionDropdown()
    {
        _resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        foreach (Resolution res in _supportedResolutions)
            options.Add($"{res.width} x {res.height}");

        _resolutionDropdown.AddOptions(options);
    }

    private void AddListeners()
    {
        _saveButton.onClick.AddListener(SaveSettings);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        // 슬라이더 실시간 적용
        _bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        // 전체화면 실시간 적용
        _fullscreenCheckBox.checkBtn.onClick.AddListener(OnFullscreenToggled);

        // 수직동기화 실시간 적용
        _vsyncCheckBox.checkBtn.onClick.AddListener(OnVSyncToggled);
    }

    // ==================== 불러오기 ====================

    private void LoadSettings()
    {
        // 해상도
        int resIndex = PlayerPrefs.GetInt(KEY_RESOLUTION, 1); // 기본 1920x1080
        resIndex = Mathf.Clamp(resIndex, 0, _supportedResolutions.Length - 1);
        _resolutionDropdown.value = resIndex;
        _resolutionDropdown.RefreshShownValue();
        ApplyResolution(resIndex);

        // 전체화면
        bool isFullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
        if (isFullscreen) _fullscreenCheckBox.OnCheckBox();
        else              _fullscreenCheckBox.OffCheckBox();
        Screen.fullScreen = isFullscreen;

        // 수직동기화
        bool isVSync = PlayerPrefs.GetInt(KEY_VSYNC, 0) == 1;
        if (isVSync) _vsyncCheckBox.OnCheckBox();
        else         _vsyncCheckBox.OffCheckBox();
        QualitySettings.vSyncCount = isVSync ? 1 : 0;

        // BGM 볼륨
        float bgmVolume = PlayerPrefs.GetFloat(KEY_BGM_VOLUME, 0.5f);
        _bgmSlider.value = bgmVolume;
        AudioManager.Instance?.SetBGMVolume(bgmVolume);

        // SFX 볼륨
        float sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1.0f);
        _sfxSlider.value = sfxVolume;
        AudioManager.Instance?.SetSFXVolume(sfxVolume);
    }

    // ==================== 저장 ====================

    private void SaveSettings()
    {
        PlayerPrefs.SetInt(KEY_RESOLUTION, _resolutionDropdown.value);
        PlayerPrefs.SetInt(KEY_FULLSCREEN, _fullscreenCheckBox.isOn ? 1 : 0);
        PlayerPrefs.SetInt(KEY_VSYNC,      _vsyncCheckBox.isOn      ? 1 : 0);
        PlayerPrefs.SetFloat(KEY_BGM_VOLUME, _bgmSlider.value);
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, _sfxSlider.value);

        PlayerPrefs.Save();
    }

    // ==================== 실시간 적용 ====================

    private void OnBGMSliderChanged(float value)
    {
        AudioManager.Instance?.SetBGMVolume(value);
        PlayerPrefs.SetFloat(KEY_BGM_VOLUME, value);
        PlayerPrefs.Save();
    }

    private void OnSFXSliderChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, value);
        PlayerPrefs.Save();
    }

    private void OnFullscreenToggled()
    {
        // UICheckBox의 isOn은 클릭 후 갱신되므로 한 프레임 뒤에 읽어야 정확
        // 대신 현재 상태 반전으로 처리
        Screen.fullScreen = _fullscreenCheckBox.isOn;
    }

    private void OnVSyncToggled()
    {
        QualitySettings.vSyncCount = _vsyncCheckBox.isOn ? 1 : 0;
    }

    private void ApplyResolution(int index)
    {
        Resolution res = _supportedResolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
    
    
}