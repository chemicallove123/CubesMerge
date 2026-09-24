using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayerMask; // F - guns
    [SerializeField] private LayerMask readLayerMask;      // E - signs

    private InputAction interactAction;
    private InputAction readAction;

    private void Awake()
    {
        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/f");
        readAction = new InputAction("Read", InputActionType.Button, "<Keyboard>/e");
    }

    private void OnEnable()
    {
        interactAction?.Enable();
        readAction?.Enable();
    }

    private void OnDisable()
    {
        interactAction?.Disable();
        readAction?.Disable();
    }

    private void OnDestroy()
    {
        interactAction?.Dispose();
        readAction?.Dispose();
    }

    private void Update()
    {
        if (readAction.WasPressedThisFrame())
            TryInteract(readLayerMask, "E");
    }

    private void TryInteract(LayerMask layerMask, string keyLabel)
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("[Interactor] Player Camera is not assigned - can't raycast.");
            return;
        }

        Debug.Log($"[Interactor] {keyLabel} pressed - casting interact ray.");

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, layerMask))
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