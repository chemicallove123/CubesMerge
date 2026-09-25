using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;

    [Tooltip("Layer used for readable/interactable objects such as signs.")]
    [SerializeField] private LayerMask readLayerMask;

    private InputAction readAction;

    private void Awake()
    {
        readAction = new InputAction(
            "Read",
            InputActionType.Button,
            "<Keyboard>/e"
        );
    }

    private void OnEnable()
    {
        readAction.Enable();
    }

    private void OnDisable()
    {
        readAction.Disable();
    }

    private void OnDestroy()
    {
        readAction.Dispose();
    }

    private void Update()
    {
        if (readAction.WasPressedThisFrame())
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning(
                "[Interactor] Player Camera is not assigned."
            );
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactRange,
            readLayerMask,
            QueryTriggerInteraction.Collide))
        {
            IInteractable interactable =
                hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}