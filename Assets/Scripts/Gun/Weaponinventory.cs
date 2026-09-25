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

    private Gun displayedGun;

    private int selectedIndex = -1;

    private InputAction pickupAction;
    private InputAction dropAction;
    private InputAction scrollAction;

    private float nextDropTime;

    private int incomingWeapons = 0;
    private bool pickupLocked = false;

    public int WeaponCount => weapons.Count;
    public int SelectedIndex => selectedIndex;

    private void Awake()
    {
        Debug.Log(
            $"[WeaponInventory] AWAKE on '{gameObject.name}' " +
            $"InstanceID={GetInstanceID()}"
        );

        pickupAction = new InputAction(
            "PickUpWeapon",
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
        pickupAction?.Enable();
        dropAction?.Enable();
        scrollAction?.Enable();
    }

    private void OnDisable()
    {
        pickupAction?.Disable();
        dropAction?.Disable();
        scrollAction?.Disable();
    }

    private void OnDestroy()
    {
        pickupAction?.Dispose();
        dropAction?.Dispose();
        scrollAction?.Dispose();
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
        // Check whether there is room BEFORE starting pickup.
        if (!CanAcceptWeapon())
        {
            Debug.Log(
                $"[WeaponInventory] Inventory is full. " +
                $"{weapons.Count}/{maxWeapons}"
            );

            return;
        }

        if (player == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Player reference is missing."
            );

            return;
        }

        if (pickupTarget == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Pickup Target is missing."
            );

            return;
        }

        Camera playerCamera =
            player.GetPlayerCamera();

        if (playerCamera == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Player camera could not be found."
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
                $"[WeaponInventory] '{hit.collider.name}' " +
                "does not contain a GunPickup."
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
        {
            Debug.LogWarning(
                $"[WeaponInventory] BeginPickup failed for " +
                $"'{pickup.gameObject.name}'."
            );

            return;
        }

        // Reserve the slot only AFTER BeginPickup succeeds.
        incomingWeapons++;
        pickupLocked = true;

        Debug.Log(
            $"[WeaponInventory] Started pickup: " +
            $"{pickup.gameObject.name}. " +
            $"Current={weapons.Count}, " +
            $"Incoming={incomingWeapons}, " +
            $"Max={maxWeapons}"
        );
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
                "[WeaponInventory] Completed pickup " +
                "had no WeaponItem."
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

        Debug.Log(
            $"[WeaponInventory] INSTANCE {GetInstanceID()} " +
            $"added {item.weaponName}. " +
            $"Count={weapons.Count}/{maxWeapons}"
        );

        // First weapon automatically becomes selected.
        if (selectedIndex < 0)
        {
            selectedIndex = 0;
            DisplaySelectedWeapon();
        }

        RefreshUI();

        Debug.Log(
            $"[WeaponInventory] Added WeaponItem " +
            $"asset='{item.name}', " +
            $"WeaponName='{item.weaponName}', " +
            $"Icon='{(item.icon != null ? item.icon.name : "NULL")}', " +
            $"Inventory: {weapons.Count}/{maxWeapons}"
        );
    }

    // DISPLAY / EQUIP
    private void DisplaySelectedWeapon()
    {
        if (displayedGun != null)
        {
            Destroy(displayedGun.gameObject);
            displayedGun = null;
        }

        if (weapons.Count == 0)
        {
            selectedIndex = -1;

            if (player != null)
            {
                player.UnequipGun();
            }

            RefreshUI();
            return;
        }

        selectedIndex =
            Mathf.Clamp(
                selectedIndex,
                0,
                weapons.Count - 1
            );

        WeaponItem item =
            weapons[selectedIndex];

        if (item == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Selected WeaponItem is null."
            );

            return;
        }

        if (item.equippedPrefab == null)
        {
            Debug.LogWarning(
                $"[WeaponInventory] {item.weaponName} " +
                "has no equipped prefab."
            );

            return;
        }

        if (weaponSocket == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Weapon Socket is missing."
            );

            return;
        }

        displayedGun =
            Instantiate(
                item.equippedPrefab,
                weaponSocket
            );

        displayedGun.transform.localPosition =
            Vector3.zero;

        displayedGun.transform.localRotation =
            Quaternion.identity;

        PrepareEquippedWeapon(
            displayedGun.gameObject
        );

        if (player != null)
        {
            player.EquipGun(displayedGun);
        }

        RefreshUI();
    }

    private void PrepareEquippedWeapon(
        GameObject gunObject)
    {
        if (gunObject == null)
            return;

        GunPickup[] pickups =
            gunObject.GetComponentsInChildren<GunPickup>(
                true
            );

        foreach (GunPickup pickup in pickups)
        {
            pickup.enabled = false;
        }

        WeaponTwirl[] twirls =
            gunObject.GetComponentsInChildren<WeaponTwirl>(
                true
            );

        foreach (WeaponTwirl twirl in twirls)
        {
            twirl.enabled = false;
        }

        Rigidbody[] rigidbodies =
            gunObject.GetComponentsInChildren<Rigidbody>(
                true
            );

        foreach (Rigidbody body in rigidbodies)
        {
            body.useGravity = false;
            body.isKinematic = true;
        }

        Collider[] colliders =
            gunObject.GetComponentsInChildren<Collider>(
                true
            );

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        WeaponInventory[] accidentalInventories =
            gunObject.GetComponentsInChildren<WeaponInventory>(
                true
            );

        foreach (WeaponInventory accidentalInventory
                 in accidentalInventories)
        {
            if (accidentalInventory == this)
                continue;

            Debug.LogWarning(
                "[WeaponInventory] Found an accidental " +
                "WeaponInventory on equipped gun clone: " +
                accidentalInventory.gameObject.name +
                ". Disabling it."
            );

            accidentalInventory.enabled = false;
        }
    }

    // SCROLL / WEAPON SELECTION
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

        DisplaySelectedWeapon();
    }

    // SHOOT ALL WEAPONS
    public void ShootAllWeapons()
    {
        if (weapons.Count == 0)
            return;

        StartCoroutine(
            ShootAllRoutine()
        );
    }

    private IEnumerator ShootAllRoutine()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponItem item =
                weapons[i];

            if (item == null)
                continue;

            if (item.equippedPrefab == null)
                continue;

            // Selected gun already exists.
            if (i == selectedIndex &&
                displayedGun != null)
            {
                displayedGun.Shoot();
                continue;
            }

            Gun temporaryGun =
                Instantiate(
                    item.equippedPrefab,
                    weaponSocket
                );

            temporaryGun.transform.localPosition =
                Vector3.zero;

            temporaryGun.transform.localRotation =
                Quaternion.identity;

            Camera playerCamera =
                player != null
                    ? player.GetPlayerCamera()
                    : null;

            if (playerCamera != null)
            {
                temporaryGun.SetCamera(
                    playerCamera
                );
            }

            PrepareTemporaryGun(
                temporaryGun.gameObject
            );

            temporaryGun.Shoot();

            yield return null;

            Destroy(
                temporaryGun.gameObject
            );
        }
    }

    private void PrepareTemporaryGun(
        GameObject gunObject)
    {
        if (gunObject == null)
            return;

        WeaponTwirl[] twirls =
            gunObject.GetComponentsInChildren<WeaponTwirl>(
                true
            );

        foreach (WeaponTwirl twirl in twirls)
        {
            twirl.enabled = false;
        }

        GunPickup[] pickups =
            gunObject.GetComponentsInChildren<GunPickup>(
                true
            );

        foreach (GunPickup pickup in pickups)
        {
            pickup.enabled = false;
        }

        Collider[] colliders =
            gunObject.GetComponentsInChildren<Collider>(
                true
            );

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Rigidbody[] bodies =
            gunObject.GetComponentsInChildren<Rigidbody>(
                true
            );

        foreach (Rigidbody body in bodies)
        {
            body.useGravity = false;
            body.isKinematic = true;
        }

        Renderer[] renderers =
            gunObject.GetComponentsInChildren<Renderer>(
                true
            );

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        WeaponInventory[] accidentalInventories =
            gunObject.GetComponentsInChildren<WeaponInventory>(
                true
            );

        foreach (WeaponInventory accidentalInventory
                 in accidentalInventories)
        {
            if (accidentalInventory != this)
            {
                accidentalInventory.enabled = false;
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

        weapons.RemoveAt(
            dropIndex
        );

        SpawnDroppedWeapon(item);

        if (weapons.Count == 0)
        {
            selectedIndex = -1;

            if (displayedGun != null)
            {
                Destroy(
                    displayedGun.gameObject
                );

                displayedGun = null;
            }

            if (player != null)
            {
                player.UnequipGun();
            }
        }
        else
        {
            if (selectedIndex >= weapons.Count)
            {
                selectedIndex =
                    weapons.Count - 1;
            }

            DisplaySelectedWeapon();
        }

        RefreshUI();

        if (item != null)
        {
            Debug.Log(
                $"[WeaponInventory] Dropped " +
                $"{item.weaponName}."
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
                "has no world prefab."
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