using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerInputData inputData;

    private Animator _animator;
    private static readonly int MoveHash       = Animator.StringToHash("Move");
    private static readonly int IsFishingHash  = Animator.StringToHash("IsFishing");
    private static readonly int IsBitingHash   = Animator.StringToHash("IsBiting");
    private static readonly int FishingEndHash = Animator.StringToHash("FishingEnd");

    private bool _isFishingMode = false;
    private bool _isOnBoat      = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_isOnBoat || _isFishingMode)
        {
            _animator.SetBool(MoveHash, false);
            return;
        }
        _animator.SetBool(MoveHash, inputData.MoveInput != Vector2.zero);
    }

    public void SetOnBoat(bool value) => _isOnBoat = value;

    // 낚시 모드 진입 (애니메이션은 아직 실행 안 함)
    public void EnterFishingMode()
    {
        _isFishingMode = true;
        // IsFishing은 여기서 true로 안 함
    }

    // 찌 던지는 순간 호출 → 낚시대 던지는 애니메이션 시작
    public void OnCast()
    {
        _animator.SetBool(IsFishingHash, true);
    }

    // 낚시 모드 종료
    public void ExitFishingMode()
    {
        _isFishingMode = false;
        _animator.SetBool(IsBitingHash, false);
        
        _animator.ResetTrigger(IsFishingHash);
        _animator.Play("Idle");
    }

    public void SetBiting(bool value)
    {
        _animator.SetBool(IsBitingHash, value);
    }
}