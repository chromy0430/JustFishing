using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerInputData inputData;

    private Animator _animator;
    private static readonly int MoveHash = Animator.StringToHash("Move"); // 문자열 캐싱

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // MoveInput이 zero이면 false, 아니면 true
        bool isMoving = inputData.MoveInput != Vector2.zero;
        _animator.SetBool(MoveHash, isMoving);
    }
}