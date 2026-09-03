/*
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;

    private InputAction interactAction;

    private void Awake()
    {
        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
    }

    private void OnEnable() => interactAction?.Enable();
    private void OnDisable() => interactAction?.Disable();
    private void OnDestroy() => interactAction?.Dispose();

    private void Update()
    {
        if (!interactAction.WasPressedThisFrame()) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
                interactable.Interact();
        }
    }
}
*/