using UnityEngine;

[RequireComponent (typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerInputData inputData;

    [Header("Move Setting")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;    
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jump Setting")]
    [SerializeField] private float JumpForce = 6f;
    [SerializeField] private float gravity = -20f;

    [Header("Ground Check")]
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0f, -0.9f, 0f); // 발바닥 바로 밑
    [SerializeField] private Vector3 groundCheckSize = new Vector3(0.4f, 0.05f, 0.4f); // 박스 크기
    [SerializeField] private LayerMask groundLayer;

    // 카메라
    private static readonly Vector3 WorldForward = new Vector3(0, 0, 1).normalized;
    private static readonly Vector3 WorldRight = new Vector3(1, 0, 0).normalized;

    private CharacterController _controller;
    private float _verticalVelocity;
    private bool _isGrounded;

    private void Awake()
    {
        TryGetComponent<CharacterController>(out _controller);
    }

    private void Update()
    {
        CheckGround();
        HandleJump();
        ApplyGravity();
        HandleMove();
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + groundCheckOffset;

        _isGrounded = Physics.CheckBox(center: origin, halfExtents: groundCheckSize * .5f, orientation: Quaternion.identity, layerMask: groundLayer);
    }

    private void HandleJump()
    {
        if (inputData.JumpPressed)
        {
            if (!_isGrounded)
            {
                inputData.ConsumeJump();
                return;
            }

            _verticalVelocity = JumpForce;
            inputData.ConsumeJump();
        }
    }

    private void HandleMove()
    {
        Vector2 input = inputData.MoveInput;
        if (input == Vector2.zero)        
            return;
        
        Vector3 moveDir = (WorldForward * input.y + WorldRight * input.x).normalized;
        float speed = inputData.IsRunning ? runSpeed : walkSpeed;

        _controller.Move(moveDir *  (speed * Time.deltaTime));

        RotateToward(moveDir);
    }

    private void RotateToward(Vector3 direction)
    {
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += gravity * Time.deltaTime;
        _controller.Move(new Vector3(0f, _verticalVelocity * Time.deltaTime, 0f));
    }

    // CheckBox 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + groundCheckOffset, groundCheckSize);
    }
}
