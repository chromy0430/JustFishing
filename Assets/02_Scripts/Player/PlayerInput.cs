using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private PlayerInputData inputData;
    [SerializeField] private InputActionAsset actionAsset;

    private InputAction _moveAction;
    private InputAction _sprintAction;
    private InputAction _jumpAction;
    private InputAction _interactAction;

    private void Awake()
    {
        // Action Map에서 각 Action 가져오기
        var playerMap = actionAsset.FindActionMap("Player", throwIfNotFound: true);
        _moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
        _sprintAction = playerMap.FindAction("Sprint", throwIfNotFound: true);
        _jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);
        _interactAction = playerMap.FindAction("Interact", throwIfNotFound: true);
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _sprintAction.Enable();
        _jumpAction.Enable();
        _interactAction.Enable();

        // Move
        _moveAction.performed += OnMove;
        _moveAction.canceled += OnMove;

        // Sprint
        _sprintAction.performed += OnSprintPerformed;
        _sprintAction.canceled += OnSprintCanceled;

        // Jump
        _jumpAction.performed += OnJump;

        // Interact
        _interactAction.performed += OnInteract;
    }

    private void OnDisable()
    {
        _moveAction.performed -= OnMove;
        _moveAction.canceled -= OnMove;

        _sprintAction.performed -= OnSprintPerformed;
        _sprintAction.canceled -= OnSprintCanceled;

        _jumpAction.performed -= OnJump;
        _interactAction.performed -= OnInteract;

        _moveAction.Disable();
        _sprintAction.Disable();
        _jumpAction.Disable();
        _interactAction.Disable();

        inputData.Reset();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        inputData.SetMoveInput(ctx.ReadValue<Vector2>());
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        inputData.SetRunning(true);
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        inputData.SetRunning(false);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        inputData.SetJumpPressed(true);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        inputData.SetInteractPressed(true);
    }
}
