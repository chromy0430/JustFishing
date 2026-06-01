using System.Runtime.CompilerServices;
using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [SerializeField] private PlayerInputData inputData;
    [SerializeField] private ShopUI shopUI;

    private bool _playerInRange = false;

    void Update()
    {
        if (!_playerInRange) return;

        if (inputData.InteractPressed)
        {       
            inputData.ConsumeInteract();
            shopUI.Open();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = true;
        shopUI.ShowPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;
        shopUI.ShowPrompt(false);
        shopUI.Close();
    }
    
    private void OnPurchase()
    {
        AudioManager.Instance?.PlayPurchase();
    }
    
    private void OnEnhance()
    {
        AudioManager.Instance?.PlayEnhance();
    }
}
