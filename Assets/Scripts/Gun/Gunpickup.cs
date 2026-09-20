/*
using UnityEngine;

public class GunPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private Gun gunPrefab;

    public void Interact()
    {
        Debug.Log($"[GunPickup] Interact() called on {gameObject.name}.");

        Player player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            Debug.LogWarning("[GunPickup] No Player found in the scene - is your Player component actually on the player object?");
            return;
        }

        if (player.WeaponSocket == null)
        {
            Debug.LogWarning("[GunPickup] Player's Weapon Socket is not assigned in the Inspector - this is why nothing equips. Assign it on the Player component.");
            return;
        }

        if (gunPrefab == null)
        {
            Debug.LogWarning($"[GunPickup] {gameObject.name}'s Gun Prefab field is not assigned - nothing to equip.");
            return;
        }

        Transform socket = player.WeaponSocket;
        Debug.Log($"[GunPickup] Equipping onto socket: {socket.name}");

        foreach (Transform child in socket)
        {
            Debug.Log($"[GunPickup] Removing previously equipped gun: {child.name}");
            Destroy(child.gameObject);
        }

        Gun spawnedGun = Instantiate(gunPrefab, socket.position, socket.rotation, socket);

        if (spawnedGun.TryGetComponent(out PickupableObject spawnedPickupable))
            DestroyImmediate(spawnedPickupable);

        if (spawnedGun.TryGetComponent(out GunPickup spawnedGunPickup))
            DestroyImmediate(spawnedGunPickup);

        if (spawnedGun.TryGetComponent(out Rigidbody spawnedRigidbody))
        {
            Debug.Log($"[GunPickup] Removing Rigidbody from equipped clone.");
            DestroyImmediate(spawnedRigidbody);
        }

        if (spawnedGun.TryGetComponent(out Collider spawnedCollider))
        {
            Debug.Log($"[GunPickup] Removing Collider from equipped clone.");
            DestroyImmediate(spawnedCollider);
        }

        player.EquipGun(spawnedGun);
        Debug.Log($"[GunPickup] Equipped {gunPrefab.name} onto {player.name}.");

        gameObject.SetActive(false); // picked up - remove from the world
        Debug.Log($"[GunPickup] {gameObject.name} deactivated after pickup.");
    }
}
*/

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