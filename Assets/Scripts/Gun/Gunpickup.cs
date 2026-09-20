using UnityEngine;

public class GunPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private Gun gunPrefab;

    public void Interact()
    {
        Debug.Log($"[GunPickup] Interact() called on {gameObject.name}.");

        WeaponInventory inventory = FindFirstObjectByType<WeaponInventory>();
        if (inventory == null)
        {
            Debug.LogWarning("[GunPickup] No WeaponInventory found in the scene.");
            return;
        }

        if (gunPrefab == null)
        {
            Debug.LogWarning($"[GunPickup] {gameObject.name}'s Gun Prefab field is not assigned.");
            return;
        }

        inventory.TryEquip(gunPrefab, gameObject);
    }
}