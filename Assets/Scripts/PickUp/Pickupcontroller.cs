using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [Tooltip("Where picked-up items stack - should be a child of the camera, positioned in front of/below it.")]
    [SerializeField] private Transform carryPoint;

    [Header("Pickup")]
    [SerializeField] private float pickupRange = 5f;
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private int maxCarried = 5;
    [SerializeField] private float flyDuration = 0.4f;
    [SerializeField] private float stackSpacing = 0.3f;

    [Header("Drop")]
    [SerializeField] private float dropInterval = 0.3f; 

    private InputAction pickupAction;
    private InputAction dropAction;

    private readonly List<IPickupable> carriedItems = new List<IPickupable>(); 
    private int flightsInProgress = 0; 
    private float lastDropTime = -999f;

    private void Awake()
    {
        pickupAction = new InputAction("Pickup", InputActionType.Button, "<Keyboard>/f");
        dropAction = new InputAction("Drop", InputActionType.Button, "<Keyboard>/g");
    }

    private void OnEnable()
    {
        pickupAction?.Enable();
        dropAction?.Enable();
    }

    private void OnDisable()
    {
        pickupAction?.Disable();
        dropAction?.Disable();
    }

    private void OnDestroy()
    {
        pickupAction?.Dispose();
        dropAction?.Dispose();
    }

    private void Update()
    {
        if (pickupAction.IsPressed())
            TryPickupInRange();

        if (dropAction.IsPressed() && Time.time - lastDropTime >= dropInterval)
            TryDropOne();
    }

    private void TryPickupInRange()
    {
        if (carriedItems.Count >= maxCarried) return;

        Vector3 checkCenter = playerCamera.transform.position + playerCamera.transform.forward * (pickupRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(checkCenter, pickupRadius);

        foreach (Collider hit in hits)
        {
            if (carriedItems.Count >= maxCarried) break;
            if (!hit.TryGetComponent(out IPickupable pickupable)) continue;
            if (carriedItems.Contains(pickupable)) continue; 

            StartCoroutine(FlyToCarryPoint(pickupable));
        }
    }

    private IEnumerator FlyToCarryPoint(IPickupable pickupable)
    {
        flightsInProgress++;
        pickupable.OnPickedUp();

        int slotIndex = carriedItems.Count;
        carriedItems.Add(pickupable); 

        Transform item = pickupable.PickupTransform;
        Vector3 startPos = item.position;
        Quaternion startRot = item.rotation;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;

            Vector3 targetPos = carryPoint.position + carryPoint.up * (slotIndex * stackSpacing);
            item.position = Vector3.Lerp(startPos, targetPos, t);
            item.rotation = Quaternion.Slerp(startRot, carryPoint.rotation, t);

            yield return null;
        }

        item.SetParent(carryPoint);
        item.localPosition = Vector3.up * (slotIndex * stackSpacing);
        item.localRotation = Quaternion.identity;

        flightsInProgress--;
    }

    private void TryDropOne()
    {
        if (flightsInProgress > 0) return; 
        if (carriedItems.Count == 0) return;

        int lastIndex = carriedItems.Count - 1;
        IPickupable item = carriedItems[lastIndex];
        carriedItems.RemoveAt(lastIndex);

        Vector3 dropPosition = carryPoint.position - carryPoint.up * 0.5f; 
        item.OnDropped(dropPosition);

        lastDropTime = Time.time;
    }
}