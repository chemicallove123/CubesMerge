using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 6f;

    [Header("Weapon")]
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private WeaponInventory weaponInventory;

    public Transform WeaponSocket => weaponSocket;

    protected Rigidbody rb;

    private Gun equippedGun;
    private Vector3 moveInput;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        equippedGun = null;
    }

    protected virtual void Update()
    {
        moveInput = GetMoveInput();

        if (WantsToShoot())
        {
            if (weaponInventory != null)
            {
                weaponInventory.ShootAllWeapons();
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        Vector3 worldMove =
            transform.right * moveInput.x +
            transform.forward * moveInput.z;

        Vector3 velocity = worldMove * moveSpeed;

        rb.linearVelocity = new Vector3(
            velocity.x,
            rb.linearVelocity.y,
            velocity.z
        );
    }

    protected abstract Vector3 GetMoveInput();

    protected abstract bool WantsToShoot();

    public Camera GetPlayerCamera()
    {
        return GetComponentInChildren<Camera>();
    }

    public void EquipGun(Gun newGun)
    {
        if (newGun == null)
            return;

        equippedGun = newGun;

        Camera playerCamera = GetPlayerCamera();

        if (playerCamera != null)
        {
            equippedGun.SetCamera(playerCamera);
        }

        Debug.Log(
            $"[Player] Equipped gun: {equippedGun.name}"
        );
    }

    public void UnequipGun()
    {
        equippedGun = null;

        Debug.Log("[Player] Gun unequipped.");
    }
}