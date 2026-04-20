using UnityEngine;

// 섬 근처 빈 오브젝트에 부착
// Sphere Collider → Is Trigger 체크
public class ReturnIslandTrigger : MonoBehaviour
{
    [SerializeField] private ReturnIslandUI returnUI;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        returnUI.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        returnUI.Hide();
    }
}