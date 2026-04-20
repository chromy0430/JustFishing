using UnityEngine;
using StylizedWater3;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [SerializeField] private BoatData boatData;
    [SerializeField] private PlayerInputData inputData;

    private Rigidbody _rb;
    private AlignToWater _alignToWater;
    private bool _isDriving = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _alignToWater = GetComponent<AlignToWater>();
    }

    public void Init(Transform player) { }

    private void Update()
    {
        HandleModeToggle();
    }

    private void FixedUpdate()
    {
        if (_isDriving) HandleDrive();
    }

    private void HandleModeToggle()
    {
        if (!inputData.ModeTogglePressed) return;
        inputData.ConsumeModeToggle();
        _isDriving = !_isDriving;
        Debug.Log(_isDriving ? "운전 모드" : "낚시 모드");
    }

    private void HandleDrive()
    {
        float moveInput = inputData.MoveInput.y;
        float turnInput = inputData.MoveInput.x;

        _rb.angularVelocity = Vector3.zero;

        // W/S : 보트 forward 방향으로 이동
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            _rb.linearVelocity = transform.forward * (moveInput * boatData.moveSpeed);
        }
        else
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularDamping = 50f;
        }

        // A/D : AlignToWater의 rotation 값 조절로 Y축 회전
        if (Mathf.Abs(turnInput) > 0.01f && _alignToWater != null)
        {
            _alignToWater.rotation += turnInput * boatData.rotateSpeed * Time.fixedDeltaTime;
            _alignToWater.rotation %= 360f; // 0~360 범위 유지
        }
    }
}