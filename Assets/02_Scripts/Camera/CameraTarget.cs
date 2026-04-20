using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Transform boat; // 부모 보트
    [SerializeField] private float fixedY = 0f; // 고정할 Y값

    private void LateUpdate()
    {
        // X, Z는 보트를 따라가되 Y는 고정
        transform.position = new Vector3(
            boat.position.x,
            fixedY,
            boat.position.z
        );
    }
}
