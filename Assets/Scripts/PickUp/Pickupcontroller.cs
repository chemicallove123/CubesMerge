using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupController : MonoBehaviour
{

    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform stackHolderTransform;
    [SerializeField] private float pickupRange = 6f;
    [SerializeField] private LayerMask pickupLayerMask;
    [SerializeField] private int maxCarryCount = 5;
    [SerializeField] private Vector3 stackSlotOffset = new Vector3(0f, 0.15f, 0f);
    [SerializeField] private float dropInterval = 0.3f;

    private readonly List<IPickupable> carriedList = new List<IPickupable>();
    private float dropTimer;

    private InputAction pickupAction;
    private InputAction dropAction;

    private void Awake()
    {
        pickupAction = new InputAction("PickUp", InputActionType.Button, "<Mouse>/leftButton");
        dropAction = new InputAction("Drop", InputActionType.Button, "<Mouse>/rightButton");
    }

    private void OnEnable()
    {
        pickupAction.Enable();
        dropAction.Enable();
    }

    private void OnDisable()
    {
        pickupAction.Disable();
        dropAction.Disable();
    }

    private void OnDestroy()
    {
        pickupAction.Dispose();
        dropAction.Dispose();
    }

    private void Update()
    {
        if (pickupAction.IsPressed()) TryPickUpNearbyObjects();
        if (dropAction.IsPressed()) TryDropLastObject();
    }

    private void TryPickUpNearbyObjects()
    {
        if (carriedList.Count >= maxCarryCount) return;

        Collider[] hitColliders = Physics.OverlapSphere(playerCameraTransform.position, pickupRange, pickupLayerMask);
        foreach (Collider hitCollider in hitColliders)
        {
            if (carriedList.Count >= maxCarryCount) break;
            if (!hitCollider.TryGetComponent(out IPickupable pickupable)) continue;
            if (carriedList.Contains(pickupable)) continue;

            Vector3 slotOffset = stackSlotOffset * carriedList.Count;
            pickupable.PickUp(stackHolderTransform, slotOffset);
            carriedList.Add(pickupable);
        }
    }

    private void TryDropLastObject()
    {
        if (carriedList.Count == 0) return;
        if (AnyItemFlying()) return; 

        dropTimer -= Time.deltaTime;
        if (dropTimer > 0f) return;
        dropTimer = dropInterval;

        int lastIndex = carriedList.Count - 1;
        IPickupable pickupable = carriedList[lastIndex];
        carriedList.RemoveAt(lastIndex);
        pickupable.Drop();
    }

    private bool AnyItemFlying()
    {
        for (int i = 0; i < carriedList.Count; i++)
        {
            if (carriedList[i].IsFlying) return true;
        }
        return false;
    }

}