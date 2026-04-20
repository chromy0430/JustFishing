using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;  // 보트 또는 캐릭터
    [SerializeField] private Vector3 offset = new Vector3(-10f, 15f, -10f);
    [SerializeField] private float smoothSpeed = 8f;

    // BoatSpawner에서 보트 생성 후 타겟 교체 가능
    public void SetTarget(Transform newTarget) => target = newTarget;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        // 회전은 고정 (쿼터뷰 유지)
    }
}