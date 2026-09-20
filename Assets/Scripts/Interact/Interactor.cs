using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayerMask; // set this to the "Pickup" layer so the ray can't hit the player's own body

    private InputAction interactAction;

    private void Awake()
    {
        // was bound to "e" - switched to "f" to match what's expected
        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/f");
    }

    private void OnEnable() => interactAction?.Enable();
    private void OnDisable() => interactAction?.Disable();
    private void OnDestroy() => interactAction?.Dispose();

    private void Update()
    {
        if (!interactAction.WasPressedThisFrame()) return;

        if (playerCamera == null)
        {
            Debug.LogWarning("[Interactor] Player Camera is not assigned - can't raycast.");
            return;
        }

        Debug.Log("[Interactor] F pressed - casting interact ray.");

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayerMask))
        {
            Debug.Log($"[Interactor] Ray hit: {hit.collider.name}");

            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                Debug.Log($"[Interactor] Found IInteractable on {hit.collider.name} - calling Interact().");
                interactable.Interact();
            }
            else
            {
                Debug.Log($"[Interactor] {hit.collider.name} does not implement IInteractable - nothing to interact with.");
            }
        }
        else
        {
            Debug.Log("[Interactor] Ray hit nothing within range - not close enough or not aimed at it.");
        }
    }
}