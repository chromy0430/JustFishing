using UnityEngine;

// 섬 근처 빈 오브젝트에 부착
// Sphere Collider → Is Trigger 체크
public class ReturnIslandTrigger : MonoBehaviour
{
    [SerializeField] private ReturnIslandUI returnUI;

    private bool _triggered = false; // 중복 방지 플래그

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        returnUI.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _triggered = false;
        returnUI.Hide();
    }
}