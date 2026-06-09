using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MainMenuUI : MonoBehaviour
{
    [Header("메인 버튼")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("패널")]
    [SerializeField] private UIPanelAnim loadPanelAnim;
    [SerializeField] private UIPanelAnim settingsPanelAnim;

    [Header("로드 슬롯")]
    [SerializeField] private SaveSlotUI[] saveSlots;

    private UIPanelAnim _openPanel; // 현재 열린 패널

    private void Start()
    {
        newGameButton.onClick.AddListener(OnNewGame);
        loadGameButton.onClick.AddListener(ToggleLoadPanel);
        settingsButton.onClick.AddListener(ToggleSettingsPanel);
        quitButton.onClick.AddListener(OnQuit);

        bool hasSave = false;
        for (int i = 0; i < SaveSystem.SLOT_COUNT; i++)
            if (SaveSystem.Instance.HasSaveData(i)) { hasSave = true; break; }
        loadGameButton.interactable = hasSave;
    }

    private void Update()
    {
        // ESC로 열린 패널 닫기
        if (Keyboard.current.escapeKey.wasPressedThisFrame && _openPanel != null)
            CloseCurrentPanel();
    }

    private void ToggleLoadPanel()
    {
        if (_openPanel == loadPanelAnim)
        {
            CloseCurrentPanel();
            return;
        }

        CloseCurrentPanel();
        RefreshLoadSlots();
        loadPanelAnim.Open();
        _openPanel = loadPanelAnim;
    }

    private void ToggleSettingsPanel()
    {
        if (_openPanel == settingsPanelAnim)
        {
            CloseCurrentPanel();
            return;
        }

        CloseCurrentPanel();
        settingsPanelAnim.Open();
        _openPanel = settingsPanelAnim;
    }

    private void CloseCurrentPanel()
    {
        if (_openPanel == null) return;
        _openPanel.Close();
        _openPanel = null;
    }

    private void RefreshLoadSlots()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            SaveData data = SaveSystem.Instance.GetSlotData(i);
            saveSlots[i].InitForLoad(i, data, (idx) =>
            {
                SaveSystem.Instance.Load(idx);
                SceneTransition.Instance.TransitionToScene("Island");
            });
        }
    }

    private void OnNewGame()
    {
        SaveSystem.Instance.NewGame();
        SceneTransition.Instance.TransitionToScene("Island");
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}