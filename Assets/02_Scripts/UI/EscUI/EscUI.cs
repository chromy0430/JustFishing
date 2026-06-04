using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EscUI : MonoBehaviour
{
    [SerializeField] private GameObject escPanel;
    [SerializeField] private Button     settingsButton;
    [SerializeField] private Button     saveButton;
    [SerializeField] private Button     quitButton;
    [SerializeField] private GameObject settingsPanel; // SettingsUI 패널

    private bool _isOpen = false;

    private void Awake()
    {
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
        // 나중에 구현
        Debug.Log("저장 (미구현)");
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