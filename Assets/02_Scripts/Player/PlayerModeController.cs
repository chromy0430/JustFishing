using UnityEngine;

// 나중에 모드별 캐릭터 상태 확장용
public class PlayerModeController : MonoBehaviour
{
    public enum PlayerMode { Drive, Fish }

    [SerializeField] private PlayerInputData inputData;
    [SerializeField] private BoatController boatController;

    private FishingController _fishingController;
    private BoatController _boatController;
    private bool _isOnBoat = false;

    public PlayerMode CurrentMode { get; private set; } = PlayerMode.Drive;
    public event System.Action<PlayerMode> OnModeChanged;

    public void OnBoardBoat(BoatController bc, FishingController fc)
    {
        _boatController = bc;
        _fishingController = fc;
        _isOnBoat = true;
        CurrentMode = PlayerMode.Drive; // 탑승 시 기본 운전 모드
    }

    public void OnLeaveBoat()
    {
        _boatController = null;
        _fishingController = null;
        _isOnBoat = false;
    }

    private void Update()
    {
        // 보트 위에 있을 때만 모드 전환 허용
        if (!_isOnBoat) return;
        if (!inputData.ModeTogglePressed) return;
        inputData.ConsumeModeToggle();

        CurrentMode = CurrentMode == PlayerMode.Drive ? PlayerMode.Fish : PlayerMode.Drive;
        OnModeChanged?.Invoke(CurrentMode);

        if (CurrentMode == PlayerMode.Fish)
        {
            _boatController.enabled = false;
            
            if (_boatController.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity  = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            AudioManager.Instance?.ForceStopShipMoving();
            _fishingController?.EnterFishingMode();
        }
        else
        {
            _boatController.enabled = true;
            _fishingController?.ExitFishingMode();
        }
    }

    public void SetBoatController(BoatController bc) => boatController = bc;
}