using UnityEngine;

public class FishingIndicator : MonoBehaviour
{
    private void Awake()
    {
        Hide();
    }

    public void Show(Vector3 position)
    {
        gameObject.SetActive(true);
        transform.position = position;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}