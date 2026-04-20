using UnityEngine;
using UnityEngine.UI;

public class ReturnIslandUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private void Awake()
    {
        panel.SetActive(false);
        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(OnNo);
    }

    public void Show() => panel.SetActive(true);
    public void Hide() => panel.SetActive(false);

    private void OnNo() => Hide();

    private void OnYes()
    {
        Hide();

        // 보트 떠나기 처리
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && player.TryGetComponent<PlayerModeController>(out var modeCtrl))
            modeCtrl.OnLeaveBoat();

        SceneTransition.Instance.TransitionToScene("MainScene");
    }
}