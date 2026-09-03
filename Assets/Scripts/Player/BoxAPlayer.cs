using UnityEngine;
using UnityEngine.InputSystem;

public class BoxAPlayer : Player
{
    private InputAction moveAction;
    private InputAction shootAction;

    protected override void Awake()
    {
        base.Awake();

        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");

        shootAction = new InputAction("Shoot", InputActionType.Button, "<Mouse>/leftButton");
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        shootAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        shootAction?.Disable();
    }

    private void OnDestroy()
    {
        moveAction?.Dispose();
        shootAction?.Dispose();
    }

    protected override Vector3 GetMoveInput()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        return new Vector3(input.x, 0f, input.y);
    }

    protected override bool WantsToShoot()
    {
        return shootAction.WasPressedThisFrame();
    }
}