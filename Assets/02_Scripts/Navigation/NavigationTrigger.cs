using UnityEngine;

public class NavigationTrigger : MonoBehaviour
{
    [SerializeField] private NavigationUI navigationUI;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        navigationUI.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        navigationUI.Hide();
    }
}