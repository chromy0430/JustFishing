using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EscUI : MonoBehaviour
{
    public static EscUI Instance { get; private set; }
    [SerializeField] private GameObject escPanel;
    [SerializeField] private Button     settingsButton;
    [SerializeField] private Button     saveButton;
    [SerializeField] private Button     quitButton;
    [SerializeField] private GameObject settingsPanel; // SettingsUI 패널
    
    [Header("저장 슬롯")]
    [SerializeField] private GameObject    saveSlotPanel;
    [SerializeField] private SaveSlotUI[]  saveSlots;
    [SerializeField] private Button        closeSavePanel;

    private bool _isOpen = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject.transform.root);
        
        escPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        settingsButton.onClick.AddListener(OnSettingsClick);
        saveButton.onClick.AddListener(OnSaveClick);
        quitButton.onClick.AddListener(OnQuitClick);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // 설정창이 열려있으면 설정창만 닫기
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                return;
            }

            ToggleEscUI();
        }
    }

    private void ToggleEscUI()
    {
        _isOpen = !_isOpen;
        escPanel.SetActive(_isOpen);
    }

    private void OnSettingsClick()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void OnSaveClick()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            SaveData data     = SaveSystem.Instance.GetSlotData(i);
            saveSlots[i].InitForSave(i, data, (idx) =>
            {
                SaveSystem.Instance.Save(idx);
                saveSlotPanel.SetActive(false);
                NotificationManager.Instance?.ShowMessage(
                    $"슬롯 {idx + 1}에 저장되었습니다.");
            });
        }
        saveSlotPanel.SetActive(true);
    }

    private void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}