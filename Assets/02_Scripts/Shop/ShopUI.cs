using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _promptUI;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Canvas _shopCanvas;

    private bool _isOpen = false;

    private void Awake()
    {
        _shopCanvas.enabled = false;
        _shopPanel.SetActive(false);
        _promptUI.SetActive(false);
        _closeButton.onClick.AddListener(Close);
    }

    private void Update()
    {
        if (_isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    public void Open()
    {
        _isOpen = true;
        
        _shopPanel?.SetActive(true);
        _promptUI?.SetActive(false);
    }

    public void Close()
    {
        _isOpen = false;
        _shopCanvas.enabled = false;
        _shopPanel.SetActive(false);
    }

    public void ShowPrompt(bool show)
    {
        if (_isOpen) return;
        _shopCanvas.enabled = show;
        _promptUI?.SetActive(show);
    }
}
