using UnityEngine;
using StylizedWater3;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [SerializeField] private PlayerInputData inputData;

    private Rigidbody _rb;
    private AlignToWater _alignToWater;
    private bool _isDriving = true;
    private BoatLevelData _levelData;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _alignToWater = GetComponent<AlignToWater>();
    }
    
    // BoatSpawner에서 레벨 데이터 주입
    public void SetLevelData(BoatLevelData levelData)
    {
        _levelData = levelData;
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
    }

    private void HandleDrive()
    {
        if (_levelData == null) return;

        float moveInput = inputData.MoveInput.y;
        float turnInput = inputData.MoveInput.x;

        _rb.angularVelocity = Vector3.zero;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            _rb.linearVelocity = transform.forward * (moveInput * _levelData.moveSpeed);
            AudioManager.Instance?.StartShipMoving();
        }
        else
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularDamping = 50f;
            AudioManager.Instance?.StopShipMoving();
        }

        if (Mathf.Abs(turnInput) > 0.01f && _alignToWater != null)
        {
            _alignToWater.rotation += turnInput * _levelData.rotateSpeed * Time.fixedDeltaTime;
            _alignToWater.rotation %= 360f;
        }
    }

    private void OnDisable()
    {
        if (_rb != null)
        {
            _rb.linearVelocity  = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        AudioManager.Instance?.ForceStopShipMoving();
    }
}