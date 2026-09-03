using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private InputAction lookAction;
    private float pitch;

    private void Awake()
    {
        lookAction = new InputAction("Look", InputActionType.Value, expectedControlType: "Vector2");
        lookAction.AddBinding("<Mouse>/delta");
    }

    private void OnEnable()
    {
        lookAction?.Enable();
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        lookAction?.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy() => lookAction?.Dispose();

    private void Update()
    {
        Vector2 delta = lookAction.ReadValue<Vector2>();

        float yaw = delta.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * yaw); 

        pitch -= delta.y * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f); 
    }
}