using Unity.VisualScripting;
using UnityEngine;

public class CameraStabilizer : MonoBehaviour
{
    [SerializeField] private Transform targetBoat;

    // BoatSpawner에서 보트 생성 후 자동 연결
    public void SetTarget(Transform boat) => targetBoat = boat;

    private void LateUpdate()
    {
        if (targetBoat == null) return;

        // X, Z만 보트 따라가고 Y는 0으로 고정 → 파도 흔들림 차단
        transform.position = new Vector3(targetBoat.position.x, 0f, targetBoat.position.z);
        transform.rotation = Quaternion.Euler(35, targetBoat.eulerAngles.y, 0);
    }
}