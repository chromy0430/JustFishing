using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject promptUI;
    [SerializeField] private Button closeButton;

    private bool _isOpen = false;

    private void Awake()
    {
        shopPanel.SetActive(false);
        promptUI.SetActive(false);
        closeButton.onClick.AddListener(Close);
    }

    private void Update()
    {
        if (_isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    public void Open()
    {
        _isOpen = true;
        shopPanel?.SetActive(true);
        promptUI?.SetActive(false);
    }

    public void Close()
    {
        _isOpen = false;
        shopPanel.SetActive(false);
    }

    public void ShowPrompt(bool show)
    {
        if (_isOpen) return;
        promptUI?.SetActive(show);
    }
}
