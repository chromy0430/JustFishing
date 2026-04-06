using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInputData", menuName = "JustFishing/Player Input Data")]
public class PlayerInputData : ScriptableObject
{
    [field : SerializeField] public Vector2 MoveInput { get; private set; }
    [field: SerializeField] public bool IsRunning { get; private set; }
    [field: SerializeField] public bool JumpPressed { get; private set; }
    [field: SerializeField] public bool InteractPressed { get; private set; }

    public void SetJumpPressed(bool pressed) => JumpPressed = pressed;

    public void SetMoveInput(Vector2 input) => MoveInput = input;
    public void SetRunning(bool running) => IsRunning = running;
    public void SetInteractPressed(bool pressed) => InteractPressed = pressed;

    public void ConsumeJump() => JumpPressed = false;
    public void ConsumeInteract() => InteractPressed = false;

    public void Reset()
    {
        MoveInput = Vector2.zero;
        IsRunning = false;
        JumpPressed = false;
        InteractPressed = false;
    }
}
