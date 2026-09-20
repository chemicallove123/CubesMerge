using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private Transform carryPoint;
    [SerializeField] private int maxCarry = 2;

    private readonly List<Gun> equippedClones = new List<Gun>();
    private readonly List<GameObject> sourceObjects = new List<GameObject>(); // original world objects, reactivated on drop

    private InputAction dropAction;

    private void Awake()
    {
        dropAction = new InputAction("DropGun", InputActionType.Button, "<Keyboard>/g");
    }

    private void OnEnable() => dropAction?.Enable();
    private void OnDisable() => dropAction?.Disable();
    private void OnDestroy() => dropAction?.Dispose();

    private void Update()
    {
        if (dropAction.WasPressedThisFrame())
            DropLastEquipped();
    }

    public bool TryEquip(Gun gunPrefab, GameObject sourceObject)
    {
        if (equippedClones.Count >= maxCarry)
        {
            Debug.Log("[WeaponInventory] Already carrying the max number of guns.");
            return false;
        }

        Transform slot = equippedClones.Count == 0 ? weaponSocket : carryPoint;

        Gun spawnedGun = Instantiate(gunPrefab, slot.position, slot.rotation, slot);
        StripVisualCloneComponents(spawnedGun);

        equippedClones.Add(spawnedGun);
        sourceObjects.Add(sourceObject);
        sourceObject.SetActive(false);

        if (slot == weaponSocket)
            player.EquipGun(spawnedGun);

        Debug.Log($"[WeaponInventory] Equipped {gunPrefab.name} into {(slot == weaponSocket ? "WeaponSocket" : "CarryPoint")}.");
        return true;
    }

    private void DropLastEquipped()
    {
        if (equippedClones.Count == 0) return;

        int lastIndex = equippedClones.Count - 1;
        Gun clone = equippedClones[lastIndex];
        GameObject source = sourceObjects[lastIndex];
        bool wasActiveWeapon = lastIndex == 0;

        equippedClones.RemoveAt(lastIndex);
        sourceObjects.RemoveAt(lastIndex);

        Transform dropSlot = wasActiveWeapon ? weaponSocket : carryPoint;
        Destroy(clone.gameObject);

        source.transform.position = dropSlot.position;
        source.transform.rotation = Quaternion.identity;
        source.SetActive(true);

        if (wasActiveWeapon)
            player.UnequipGun();

        Debug.Log($"[WeaponInventory] Dropped {source.name}.");
    }

    private void StripVisualCloneComponents(Gun spawnedGun)
    {
        if (spawnedGun.TryGetComponent(out PickupableObject spawnedPickupable))
            DestroyImmediate(spawnedPickupable);

        if (spawnedGun.TryGetComponent(out GunPickup spawnedGunPickup))
            DestroyImmediate(spawnedGunPickup);

        if (spawnedGun.TryGetComponent(out Rigidbody spawnedRigidbody))
            DestroyImmediate(spawnedRigidbody);

        if (spawnedGun.TryGetComponent(out Collider spawnedCollider))
            DestroyImmediate(spawnedCollider);
    }
}