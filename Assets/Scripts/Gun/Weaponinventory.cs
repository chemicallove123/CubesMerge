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

    private Gun displayedGun;

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

        nextPickupTime = Time.time + pickupInterval;

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

        Collider[] colliders = Physics.OverlapSphere(
            searchPosition,
            pickupRange,
            pickupLayerMask,
            QueryTriggerInteraction.Collide
        );

        GunPickup nearestPickup = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider hit in colliders)
        {
            GunPickup pickup =
                hit.GetComponentInParent<GunPickup>();

            if (pickup == null)
                continue;

            if (pickup.IsBeingCollected)
                continue;

            float distance = Vector3.Distance(
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

        if (nearestPickup.BeginPickup(this, pickupTarget))
        {
            incomingWeapons++;
        }
    }

    public bool CanAcceptWeapon()
    {
        return weapons.Count + incomingWeapons < maxWeapons;
    }

    public void CompletePickup(WeaponItem item)
    {
        incomingWeapons =
            Mathf.Max(0, incomingWeapons - 1);

        if (item == null)
            return;

        if (weapons.Count >= maxWeapons)
            return;

        weapons.Add(item);

        // First collected weapon becomes selected.
        if (selectedIndex < 0)
        {
            selectedIndex = 0;
            DisplaySelectedWeapon();
        }

        RefreshUI();

        Debug.Log(
            $"[WeaponInventory] Added {item.weaponName}. " +
            $"Inventory: {weapons.Count}/{maxWeapons}"
        );
    }

    // DISPLAYED WEAPON
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
                player.UnequipGun();

            RefreshUI();
            return;
        }

        selectedIndex = Mathf.Clamp(
            selectedIndex,
            0,
            weapons.Count - 1
        );

        WeaponItem item = weapons[selectedIndex];

        if (item == null || item.equippedPrefab == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Selected WeaponItem " +
                "has no equipped prefab."
            );

            return;
        }

        displayedGun = Instantiate(
            item.equippedPrefab,
            weaponSocket
        );

        displayedGun.transform.localPosition =
            Vector3.zero;

        displayedGun.transform.localRotation =
            Quaternion.identity;

        displayedGun.transform.localScale =
            Vector3.one;

        RemoveWorldPhysics(displayedGun.gameObject);

        if (player != null)
            player.EquipGun(displayedGun);

        RefreshUI();
    }

    private void RemoveWorldPhysics(GameObject gunObject)
    {
        Rigidbody[] rigidbodies =
            gunObject.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody body in rigidbodies)
        {
            body.isKinematic = true;
            body.useGravity = false;
        }

        Collider[] colliders =
            gunObject.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        GunPickup[] pickups =
            gunObject.GetComponentsInChildren<GunPickup>();

        foreach (GunPickup pickup in pickups)
        {
            pickup.enabled = false;
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
            selectedIndex = weapons.Count - 1;

        if (selectedIndex >= weapons.Count)
            selectedIndex = 0;

        DisplaySelectedWeapon();
    }

    // SHOOT ALL WEAPONS
    public void ShootAllWeapons()
    {
        if (weapons.Count == 0)
            return;

        StartCoroutine(ShootAllRoutine());
    }

    private IEnumerator ShootAllRoutine()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponItem item = weapons[i];

            if (item == null ||
                item.equippedPrefab == null)
            {
                continue;
            }

            // The selected gun already exists physically.
            if (i == selectedIndex &&
                displayedGun != null)
            {
                displayedGun.Shoot();
                continue;
            }

            Gun temporaryGun = Instantiate(
                item.equippedPrefab,
                weaponSocket
            );

            temporaryGun.transform.localPosition =
                Vector3.zero;

            temporaryGun.transform.localRotation =
                Quaternion.identity;

            temporaryGun.gameObject.SetActive(false);

            Camera playerCamera =
                player != null
                    ? player.GetPlayerCamera()
                    : null;

            if (playerCamera != null)
                temporaryGun.SetCamera(playerCamera);

            temporaryGun.gameObject.SetActive(true);

            // Hide the model but keep Gun functional.
            Renderer[] renderers =
                temporaryGun.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
                renderer.enabled = false;

            temporaryGun.Shoot();

            // Keep it alive briefly so Shoot() can spawn
            // whatever projectile this weapon uses.
            yield return null;

            Destroy(temporaryGun.gameObject);
        }
    }

    // DROP
   private void HandleDrop()
    {
        if (!dropAction.IsPressed())
            return;

        if (Time.time < nextDropTime)
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

        int dropIndex = weapons.Count - 1;

        WeaponItem item = weapons[dropIndex];

        weapons.RemoveAt(dropIndex);

        SpawnDroppedWeapon(item);

        if (weapons.Count == 0)
        {
            selectedIndex = -1;

            if (displayedGun != null)
            {
                Destroy(displayedGun.gameObject);
                displayedGun = null;
            }

            if (player != null)
                player.UnequipGun();
        }
        else
        {
            if (selectedIndex >= weapons.Count)
                selectedIndex = weapons.Count - 1;

            DisplaySelectedWeapon();
        }

        RefreshUI();

        Debug.Log(
            $"[WeaponInventory] Dropped {item.weaponName}."
        );
    }

    private void SpawnDroppedWeapon(WeaponItem item)
    {
        if (item == null || item.worldPrefab == null)
        {
            Debug.LogWarning(
                "[WeaponInventory] Weapon has no world prefab."
            );

            return;
        }

        Camera camera =
            player != null
                ? player.GetPlayerCamera()
                : null;

        Vector3 spawnPosition =
            transform.position + transform.forward;

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