using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool FlashlightPressed { get; private set; }

    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction flashlightAction;
    void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Player/Move");
        jumpAction = InputSystem.actions.FindAction("Player/Jump");
        sprintAction = InputSystem.actions.FindAction("Player/Sprint");
        flashlightAction = InputSystem.actions.FindAction("Player/Flashlight");
    }

    void Update()
    {
        MoveInput = moveAction.ReadValue<Vector2>();
        JumpPressed = jumpAction.WasPressedThisFrame();
        JumpHeld = jumpAction.IsPressed();
        SprintHeld = sprintAction.IsPressed();
        FlashlightPressed = flashlightAction.WasPressedThisFrame();
        Debug.Log(MoveInput);
    }
}