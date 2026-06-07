using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("메인 버튼")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("패널")]
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("로드 슬롯")]
    [SerializeField] private SaveSlotUI[] saveSlots; // 3개

    private void Start()
    {
        loadPanel.SetActive(false);
        settingsPanel.SetActive(false);

        newGameButton.onClick.AddListener(OnNewGame);
        loadGameButton.onClick.AddListener(OnLoadGame);
        settingsButton.onClick.AddListener(OnSettings);
        quitButton.onClick.AddListener(OnQuit);

        // 저장 데이터 없으면 불러오기 버튼 비활성화
        bool hasSave = false;
        for (int i = 0; i < SaveSystem.SLOT_COUNT; i++)
            if (SaveSystem.Instance.HasSaveData(i)) { hasSave = true; break; }
        loadGameButton.interactable = hasSave;
    }

    private void OnNewGame()
    {
        SaveSystem.Instance.NewGame();
        SceneTransition.Instance.TransitionToScene("MainScene");
    }

    private void OnLoadGame()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            SaveData data = SaveSystem.Instance.GetSlotData(i);
            saveSlots[i].InitForLoad(i, data, (idx) =>
            {
                SaveSystem.Instance.Load(idx);
                SceneTransition.Instance.TransitionToScene("MainScene");
            });
        }
        loadPanel.SetActive(true);
    }

    private void OnSlotSelected(int slotIndex)
    {
        if (SaveSystem.Instance.Load(slotIndex))
            SceneTransition.Instance.TransitionToScene("MainScene");
    }

    private void OnSettings()
    {
        settingsPanel.SetActive(true);
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