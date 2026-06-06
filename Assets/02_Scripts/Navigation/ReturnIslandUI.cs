using UnityEngine;
using UnityEngine.UI;

public class ReturnIslandUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    private bool _isShowing = false;

    private void Awake()
    {
        panel.SetActive(false);
        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(OnNo);
    }

    public void Show()
    {
        if (_isShowing) return; // 이미 표시 중이면 무시
        _isShowing = true;
        panel.SetActive(true);
    }

    public void Hide()
    {
        _isShowing = false;
        panel.SetActive(false);
    }

    private void OnNo() => Hide();

    private void OnYes()
    {
        _isShowing = false;
        Hide();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            /*if (player.TryGetComponent<CharacterController>(out var cc))
                cc.enabled = true;
            if (player.TryGetComponent<PlayerMove>(out var pm))
                pm.enabled = true;

            player.transform.SetParent(null);*/

            if (player.TryGetComponent<PlayerModeController>(out var modeCtrl))
                modeCtrl.OnLeaveBoat();

            PlayerAnimator anim = player.GetComponentInChildren<PlayerAnimator>();
            anim?.SetOnBoat(false);
        }

        BoatDurability durability = FindFirstObjectByType<BoatDurability>();
        durability?.SaveDurability();

        SceneTransition.Instance.TransitionToScene("MainScene");
    }
}