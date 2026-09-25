using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInventory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private Transform pickupTarget;
    [SerializeField] private WeaponInventoryUI inventoryUI;

    [Header("Weapon Formation")]
    [SerializeField] private WeaponOrbit weaponOrbit;

    [Header("Inventory")]
    [SerializeField] private int maxWeapons = 12;

    [Header("Pickup")]
    [SerializeField] private float pickupRange = 6f;
    [SerializeField] private LayerMask pickupLayerMask;

    [Header("Drop")]
    [SerializeField] private float dropInterval = 0.3f;
    [SerializeField] private float dropForwardDistance = 1.5f;

    private readonly List<WeaponItem> weapons =
        new List<WeaponItem>();

    private readonly List<Gun> activeGuns =
        new List<Gun>();

    private int selectedIndex = -1;

    private InputAction pickupAction;
    private InputAction dropAction;

    private InputAction rotateLeftAction;
    private InputAction rotateRightAction;

    private float nextDropTime;

    private int incomingWeapons = 0;
    private bool pickupLocked = false;

    public int WeaponCount => weapons.Count;
    public int SelectedIndex => selectedIndex;

    private void Awake()
    {
        // F = Pick up weapon
        pickupAction = new InputAction(
            "PickUpWeapon",
            InputActionType.Button,
            "<Keyboard>/f"
        );

        // G = Drop weapon
        dropAction = new InputAction(
            "DropWeapon",
            InputActionType.Button,
            "<Keyboard>/g"
        );

        // Q = Rotate weapons left
        rotateLeftAction = new InputAction(
            "RotateWeaponsLeft",
            InputActionType.Button,
            "<Keyboard>/q"
        );

        // E = Rotate weapons right
        rotateRightAction = new InputAction(
            "RotateWeaponsRight",
            InputActionType.Button,
            "<Keyboard>/e"
        );
    }

    private void OnEnable()
    {
        pickupAction?.Enable();
        dropAction?.Enable();

        rotateLeftAction?.Enable();
        rotateRightAction?.Enable();
    }

    private void OnDisable()
    {
        pickupAction?.Disable();
        dropAction?.Disable();

        rotateLeftAction?.Disable();
        rotateRightAction?.Disable();
    }

    private void OnDestroy()
    {
        pickupAction?.Dispose();
        dropAction?.Dispose();

        rotateLeftAction?.Dispose();
        rotateRightAction?.Dispose();
    }

    private void Start()
    {
        RefreshUI();
        RefreshOrbit();
    }

    private void Update()
    {
        HandlePickup();
        HandleDrop();
        HandleWeaponRotation();
    }

    // PICKUP
    private void HandlePickup()
    {
        if (!pickupAction.WasPressedThisFrame())
            return;

        if (pickupLocked)
            return;

        if (incomingWeapons > 0)
            return;

        TryPickupCrosshairWeapon();
    }

    private void TryPickupCrosshairWeapon()
    {
        if (!CanAcceptWeapon())
        {
            Debug.Log(
                $"[WeaponInventory] Inventory full: " +
                $"{weapons.Count}/{maxWeapons}"
            );

            return;
        }

        if (player == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Player reference missing."
            );

            return;
        }

        if (pickupTarget == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Pickup Target missing."
            );

            return;
        }

        Camera playerCamera =
            player.GetPlayerCamera();

        if (playerCamera == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Player camera missing."
            );

            return;
        }

        Ray ray =
            playerCamera.ViewportPointToRay(
                new Vector3(
                    0.5f,
                    0.5f,
                    0f
                )
            );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            pickupRange,
            pickupLayerMask,
            QueryTriggerInteraction.Collide))
        {
            Debug.Log(
                "[WeaponInventory] No weapon under crosshair."
            );

            return;
        }

        GunPickup pickup =
            hit.collider.GetComponentInParent<GunPickup>();

        if (pickup == null)
        {
            Debug.Log(
                "[WeaponInventory] Object under crosshair " +
                "does not have GunPickup."
            );

            return;
        }

        if (pickup.IsBeingCollected)
            return;

        bool started =
            pickup.BeginPickup(
                this,
                pickupTarget
            );

        if (!started)
            return;

        incomingWeapons++;
        pickupLocked = true;
    }

    public bool CanAcceptWeapon()
    {
        return
            weapons.Count + incomingWeapons
            < maxWeapons;
    }

    public void CompletePickup(WeaponItem item)
    {
        incomingWeapons =
            Mathf.Max(
                0,
                incomingWeapons - 1
            );

        pickupLocked = false;

        if (item == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] WeaponItem was null."
            );

            return;
        }

        if (weapons.Count >= maxWeapons)
        {
            Debug.LogWarning(
                "[WeaponInventory] Inventory is full."
            );

            return;
        }

        weapons.Add(item);

        // Create the actual visible floating gun.
        CreateActiveGun(item);

        if (selectedIndex < 0)
        {
            selectedIndex = 0;
        }

        RefreshOrbit();
        RefreshUI();

        Debug.Log(
            $"[WeaponInventory] Added {item.weaponName}. " +
            $"Inventory: {weapons.Count}/{maxWeapons}"
        );
    }

    // ACTIVE / FLOATING GUNS
    private void CreateActiveGun(WeaponItem item)
    {
        if (item == null)
            return;

        if (item.equippedPrefab == null)
        {
            Debug.LogWarning(
                $"[WeaponInventory] {item.weaponName} " +
                "has no Equipped Prefab."
            );

            return;
        }

        if (weaponOrbit == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Weapon Orbit is not assigned."
            );

            return;
        }

        Gun newGun =
            Instantiate(
                item.equippedPrefab,
                weaponOrbit.transform
            );

        newGun.transform.localPosition =
            Vector3.zero;

        newGun.transform.localRotation =
            Quaternion.identity;

        PrepareActiveGun(
            newGun.gameObject
        );

        Camera playerCamera =
            player != null
                ? player.GetPlayerCamera()
                : null;

        if (playerCamera != null)
        {
            newGun.SetCamera(playerCamera);
        }

        activeGuns.Add(newGun);

        Debug.Log(
            $"[WeaponInventory] Created floating gun: " +
            $"{newGun.name}"
        );
    }

    private void PrepareActiveGun(GameObject gunObject)
    {
        if (gunObject == null)
            return;

        // Prevent floating guns from being picked up again.
        GunPickup[] pickups =
            gunObject.GetComponentsInChildren<GunPickup>(
                true
            );

        foreach (GunPickup pickup in pickups)
        {
            pickup.enabled = false;
        }

        // Disable the old world spinning/twirl behaviour.
        WeaponTwirl[] twirls =
            gunObject.GetComponentsInChildren<WeaponTwirl>(
                true
            );

        foreach (WeaponTwirl twirl in twirls)
        {
            twirl.enabled = false;
        }

        // Floating guns should not fall.
        Rigidbody[] rigidbodies =
            gunObject.GetComponentsInChildren<Rigidbody>(
                true
            );

        foreach (Rigidbody body in rigidbodies)
        {
            body.useGravity = false;
            body.isKinematic = true;
        }

        // Floating guns do not need physical collision.
        Collider[] colliders =
            gunObject.GetComponentsInChildren<Collider>(
                true
            );

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    private void RefreshOrbit()
    {
        if (weaponOrbit == null)
            return;

        weaponOrbit.SetGuns(activeGuns);
    }

    // Q / E ROTATION
    private void HandleWeaponRotation()
    {
        if (activeGuns.Count == 0)
            return;

        // Q = rotate left
        if (rotateLeftAction.WasPressedThisFrame())
        {
            weaponOrbit.RotateLeft();

            selectedIndex--;

            if (selectedIndex < 0)
            {
                selectedIndex =
                    weapons.Count - 1;
            }

            RefreshUI();
        }

        // E = rotate right
        if (rotateRightAction.WasPressedThisFrame())
        {
            weaponOrbit.RotateRight();

            selectedIndex++;

            if (selectedIndex >= weapons.Count)
            {
                selectedIndex = 0;
            }

            RefreshUI();
        }
    }

    // SHOOT ALL WEAPONS
    public void ShootAllWeapons()
    {
        if (activeGuns.Count == 0)
            return;

        foreach (Gun gun in activeGuns)
        {
            if (gun != null)
            {
                gun.Shoot();
            }
        }
    }

    // DROP
    private void HandleDrop()
    {
        if (!dropAction.IsPressed())
            return;

        if (Time.time < nextDropTime)
            return;

        if (pickupLocked)
            return;

        if (incomingWeapons > 0)
            return;

        nextDropTime =
            Time.time + dropInterval;

        DropLastWeapon();
    }

    private void DropLastWeapon()
    {
        if (weapons.Count == 0)
            return;

        int dropIndex =
            weapons.Count - 1;

        WeaponItem item =
            weapons[dropIndex];

        weapons.RemoveAt(dropIndex);

        // Remove the corresponding floating gun.
        if (dropIndex < activeGuns.Count)
        {
            Gun gun =
                activeGuns[dropIndex];

            activeGuns.RemoveAt(dropIndex);

            if (gun != null)
            {
                Destroy(gun.gameObject);
            }
        }

        // Put the gun back into the world.
        SpawnDroppedWeapon(item);

        if (weapons.Count == 0)
        {
            selectedIndex = -1;
        }
        else
        {
            selectedIndex =
                Mathf.Clamp(
                    selectedIndex,
                    0,
                    weapons.Count - 1
                );
        }

        RefreshOrbit();
        RefreshUI();

        if (item != null)
        {
            Debug.Log(
                $"[WeaponInventory] Dropped {item.weaponName}. " +
                $"Inventory: {weapons.Count}/{maxWeapons}"
            );
        }
    }

    private void SpawnDroppedWeapon(
        WeaponItem item)
    {
        if (item == null)
            return;

        if (item.worldPrefab == null)
        {
            Debug.LogWarning(
                $"[WeaponInventory] {item.weaponName} " +
                "has no World Prefab."
            );

            return;
        }

        Camera playerCamera =
            player != null
                ? player.GetPlayerCamera()
                : null;

        Vector3 spawnPosition =
            transform.position +
            transform.forward *
            dropForwardDistance;

        Quaternion spawnRotation =
            Quaternion.identity;

        if (playerCamera != null)
        {
            spawnPosition =
                playerCamera.transform.position +
                playerCamera.transform.forward *
                dropForwardDistance;

            spawnRotation =
                Quaternion.Euler(
                    0f,
                    playerCamera.transform.eulerAngles.y,
                    0f
                );
        }

        Instantiate(
            item.worldPrefab,
            spawnPosition,
            spawnRotation
        );
    }

    // UI
    private void RefreshUI()
    {
        if (inventoryUI == null)
            return;

        inventoryUI.Refresh(
            weapons,
            selectedIndex
        );
    }
}