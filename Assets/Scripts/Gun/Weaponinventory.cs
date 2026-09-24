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

    [Header("Inventory")]
    [SerializeField] private int maxWeapons = 10;

    [Header("Pickup")]
    [SerializeField] private float pickupRange = 6f;
    [SerializeField] private LayerMask pickupLayerMask;
    [SerializeField] private float pickupInterval = 0.15f;

    [Header("Drop")]
    [SerializeField] private float dropInterval = 0.3f;
    [SerializeField] private float dropForwardDistance = 1.5f;

    private readonly List<WeaponItem> weapons =
        new List<WeaponItem>();

    // Every collected weapon gets a runtime gun.
    // Only the selected gun is visible.
    private readonly List<Gun> runtimeGuns =
        new List<Gun>();

    private int selectedIndex = -1;

    private InputAction pickupAction;
    private InputAction dropAction;
    private InputAction scrollAction;

    private float nextPickupTime;
    private float nextDropTime;

    private int incomingWeapons;

    public int WeaponCount => weapons.Count;
    public int SelectedIndex => selectedIndex;

    private void Awake()
    {
        pickupAction = new InputAction(
            "PickUpWeapons",
            InputActionType.Button,
            "<Keyboard>/f"
        );

        dropAction = new InputAction(
            "DropWeapon",
            InputActionType.Button,
            "<Keyboard>/g"
        );

        scrollAction = new InputAction(
            "ScrollWeapons",
            InputActionType.Value,
            "<Mouse>/scroll"
        );
    }

    private void OnEnable()
    {
        pickupAction.Enable();
        dropAction.Enable();
        scrollAction.Enable();
    }

    private void OnDisable()
    {
        pickupAction.Disable();
        dropAction.Disable();
        scrollAction.Disable();
    }

    private void OnDestroy()
    {
        pickupAction.Dispose();
        dropAction.Dispose();
        scrollAction.Dispose();
    }

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        HandlePickup();
        HandleDrop();
        HandleScrolling();
    }

    // PICKUP
    private void HandlePickup()
    {
        if (!pickupAction.IsPressed())
            return;

        if (Time.time < nextPickupTime)
            return;

        nextPickupTime =
            Time.time + pickupInterval;

        TryPickupNearestWeapon();
    }

    private void TryPickupNearestWeapon()
    {
        if (!CanAcceptWeapon())
            return;

        Vector3 searchPosition =
            pickupTarget != null
                ? pickupTarget.position
                : transform.position;

        Collider[] colliders =
            Physics.OverlapSphere(
                searchPosition,
                pickupRange,
                pickupLayerMask,
                QueryTriggerInteraction.Collide
            );

        GunPickup nearestPickup = null;

        float nearestDistance =
            Mathf.Infinity;

        foreach (Collider hit in colliders)
        {
            GunPickup pickup =
                hit.GetComponentInParent<GunPickup>();

            if (pickup == null)
                continue;

            if (pickup.IsBeingCollected)
                continue;

            float distance =
                Vector3.Distance(
                    searchPosition,
                    pickup.transform.position
                );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPickup = pickup;
            }
        }

        if (nearestPickup == null)
            return;

        if (nearestPickup.BeginPickup(
            this,
            pickupTarget))
        {
            incomingWeapons++;
        }
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

        if (item == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Tried to add a null WeaponItem."
            );

            return;
        }

        if (weapons.Count >= maxWeapons)
            return;

        if (item.equippedPrefab == null)
        {
            Debug.LogWarning(
                $"[WeaponInventory] {item.weaponName} " +
                "has no Equipped Prefab assigned."
            );

            return;
        }

        weapons.Add(item);

        CreateRuntimeGun(item);

        // First weapon automatically becomes selected.
        if (selectedIndex < 0)
        {
            selectedIndex = 0;
        }

        UpdateVisibleWeapon();
        RefreshUI();

        Debug.Log(
            $"[WeaponInventory] Added {item.weaponName}. " +
            $"Inventory: {weapons.Count}/{maxWeapons}"
        );
    }

    // =====================================================
    // CREATE INVENTORY GUN
    // =====================================================

    private void CreateRuntimeGun(WeaponItem item)
    {
        if (weaponSocket == null)
        {
            Debug.LogError(
                "[WeaponInventory] Weapon Socket is not assigned!"
            );

            return;
        }

        Gun gun =
            Instantiate(
                item.equippedPrefab,
                weaponSocket
            );

        gun.name =
            item.weaponName + "_InventoryGun";

        // Put gun directly onto WeaponSocket.
        gun.transform.localPosition =
            Vector3.zero;

        gun.transform.localRotation =
            Quaternion.identity;

        gun.transform.localScale =
            Vector3.one;

        PrepareInventoryGun(gun);

        Camera camera =
            player != null
                ? player.GetPlayerCamera()
                : null;

        if (camera != null)
        {
            gun.SetCamera(camera);
        }
        else
        {
            Debug.LogWarning(
                $"[WeaponInventory] Could not assign camera to {gun.name}."
            );
        }

        runtimeGuns.Add(gun);
    }

    private void PrepareInventoryGun(Gun gun)
    {
        if (gun == null)
            return;

        // Disable physics because the gun is now
        // being held by the player.
        Rigidbody[] rigidbodies =
            gun.GetComponentsInChildren<Rigidbody>(
                true
            );

        foreach (Rigidbody body in rigidbodies)
        {
            body.isKinematic = true;
            body.useGravity = false;
        }

        // Disable colliders while the gun is held.
        Collider[] colliders =
            gun.GetComponentsInChildren<Collider>(
                true
            );

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // A held gun should not be collectible again.
        GunPickup[] pickups =
            gun.GetComponentsInChildren<GunPickup>(
                true
            );

        foreach (GunPickup pickup in pickups)
        {
            pickup.enabled = false;
        }
    }

    // SELECTED / VISIBLE WEAPON

    private void UpdateVisibleWeapon()
    {
        for (int i = 0; i < runtimeGuns.Count; i++)
        {
            Gun gun =
                runtimeGuns[i];

            if (gun == null)
                continue;

            bool shouldBeVisible =
                i == selectedIndex;

            SetGunVisible(
                gun,
                shouldBeVisible
            );
        }
    }

    private void SetGunVisible(
        Gun gun,
        bool visible)
    {
        Renderer[] renderers =
            gun.GetComponentsInChildren<Renderer>(
                true
            );

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }

    // SCROLLING

    private void HandleScrolling()
    {
        if (weapons.Count <= 1)
            return;

        Vector2 scroll =
            scrollAction.ReadValue<Vector2>();

        if (scroll.y > 0.01f)
        {
            MoveSelection(-1);
        }
        else if (scroll.y < -0.01f)
        {
            MoveSelection(1);
        }
    }

    private void MoveSelection(int direction)
    {
        if (weapons.Count == 0)
            return;

        selectedIndex += direction;

        if (selectedIndex < 0)
        {
            selectedIndex =
                weapons.Count - 1;
        }

        if (selectedIndex >= weapons.Count)
        {
            selectedIndex = 0;
        }

        UpdateVisibleWeapon();
        RefreshUI();
    }

    // SHOOT ALL COLLECTED WEAPONS

    public void ShootAllWeapons()
    {
        Debug.Log(
            $"[INVENTORY] ShootAllWeapons called. " +
            $"Runtime guns: {runtimeGuns.Count}"
        );

        if (runtimeGuns.Count == 0)
        {
            Debug.LogWarning(
                "[INVENTORY] There are no runtime guns to shoot."
            );

            return;
        }

        for (int i = 0; i < runtimeGuns.Count; i++)
        {
            Gun gun =
                runtimeGuns[i];

            if (gun == null)
                continue;

            Debug.Log(
                $"[INVENTORY] Shooting gun {i}: {gun.name}"
            );

            gun.Shoot();
        }
    }

    // DROP

    private void HandleDrop()
    {
        if (!dropAction.IsPressed())
            return;

        if (Time.time < nextDropTime)
            return;

        // Do not allow dropping while guns
        // are still flying toward the player.
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

        // LIFO:
        // Last weapon collected is dropped first.
        int dropIndex =
            weapons.Count - 1;

        WeaponItem item =
            weapons[dropIndex];

        weapons.RemoveAt(dropIndex);

        // Remove its hidden/held runtime gun.
        if (dropIndex < runtimeGuns.Count)
        {
            Gun runtimeGun =
                runtimeGuns[dropIndex];

            runtimeGuns.RemoveAt(
                dropIndex
            );

            if (runtimeGun != null)
            {
                Destroy(
                    runtimeGun.gameObject
                );
            }
        }

        // Put the world version back into the scene.
        SpawnDroppedWeapon(item);

        if (weapons.Count == 0)
        {
            selectedIndex = -1;
        }
        else
        {
            if (selectedIndex >= weapons.Count)
            {
                selectedIndex =
                    weapons.Count - 1;
            }
        }

        UpdateVisibleWeapon();
        RefreshUI();

        Debug.Log(
            $"[WeaponInventory] Dropped {item.weaponName}. " +
            $"Inventory: {weapons.Count}/{maxWeapons}"
        );
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
                "has no World Prefab assigned."
            );

            return;
        }

        Camera camera =
            player != null
                ? player.GetPlayerCamera()
                : null;

        Vector3 spawnPosition =
            transform.position +
            transform.forward;

        Quaternion spawnRotation =
            Quaternion.identity;

        if (camera != null)
        {
            spawnPosition =
                camera.transform.position +
                camera.transform.forward *
                dropForwardDistance;

            spawnRotation =
                Quaternion.Euler(
                    0f,
                    camera.transform.eulerAngles.y,
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