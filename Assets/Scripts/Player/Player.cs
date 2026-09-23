using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 6f;

    [Header("Weapon")]
    [Tooltip("Leave this empty. The player can only shoot after equipping a gun.")]
    [SerializeField] protected Gun gun;

    [Tooltip("Where picked-up guns get parented. This should normally be a child of the camera so aiming follows the player's full look rotation.")]
    [SerializeField] private Transform weaponSocket;

    public Transform WeaponSocket => weaponSocket;

    protected Rigidbody rb;

    private IGun gunInterface;
    private Vector3 moveInput;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Start with NO gun.
        // The player must pick up gun first.
        gun = null;
        gunInterface = null;
    }

    protected virtual void Update()
    {
        moveInput = GetMoveInput();

        // The player can only shoot if a gun has been equipped.
        if (gunInterface != null && WantsToShoot())
        {
            gunInterface.Shoot();
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
        {
            Debug.LogWarning("[Player] EquipGun called with a null gun.");
            return;
        }

        gun = newGun;
        gunInterface = newGun;

        Camera playerCamera = GetPlayerCamera();

        if (playerCamera != null)
        {
            newGun.SetCamera(playerCamera);
        }
        else
        {
            Debug.LogWarning(
                "[Player] Could not find a Camera in the player's children."
            );
        }

        Debug.Log($"[Player] Now equipped with {newGun.name}.");
    }

    public void UnequipGun()
    {
        gun = null;
        gunInterface = null;

        Debug.Log("[Player] Gun unequipped.");
    }
}

