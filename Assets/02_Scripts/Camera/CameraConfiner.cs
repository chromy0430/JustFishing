using Unity.Cinemachine;
using UnityEngine;

public class CameraConfiner : MonoBehaviour
{
    [Header("카메라 이동 범위")]
    [SerializeField] private float minX = -0.8f;
    [SerializeField] private float maxX = 1.4f;
    [SerializeField] private float minY = 8f;
    [SerializeField] private float maxY = 15f;

    private CinemachineCamera _vcam;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineCamera>();
    }

    private void LateUpdate()
    {
        if (_vcam == null) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}