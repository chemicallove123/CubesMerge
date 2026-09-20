/*
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public abstract class Player : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 6f;
    [SerializeField] protected Gun gun; // starting gun, if any - can be swapped at runtime via EquipGun
    [Tooltip("Where picked-up guns get parented - should be a child of the camera so aim follows full look rotation, not just body yaw.")]
    [SerializeField] private Transform weaponSocket;

    public Transform WeaponSocket => weaponSocket;

    protected Rigidbody rb;
    private IGun gunInterface;
    private Vector3 moveInput;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        gunInterface = gun;
    }

    protected virtual void Update()
    {
        moveInput = GetMoveInput();

        if (WantsToShoot())
            gunInterface?.Shoot();
    }

    protected virtual void FixedUpdate()
    {
        Vector3 worldMove = transform.right * moveInput.x + transform.forward * moveInput.z;
        Vector3 velocity = worldMove * moveSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    protected abstract Vector3 GetMoveInput();

    protected abstract bool WantsToShoot();

    public void EquipGun(Gun newGun)
    {
        if (newGun == null)
        {
            Debug.LogWarning("[Player] EquipGun called with a null gun.");
            return;
        }

        gun = newGun;
        gunInterface = newGun;
        Debug.Log($"[Player] Now equipped with {newGun.name}.");
    }
}
*/

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 6f;

    [Header("Starting Gun")]
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

        gunInterface = gun;

        // If the player starts with a gun,
        // automatically give it the player's camera.
        if (gun != null)
        {
            Camera playerCamera = GetPlayerCamera();

            if (playerCamera != null)
            {
                gun.SetCamera(playerCamera);
            }
        }
    }

    protected virtual void Update()
    {
        moveInput = GetMoveInput();

        if (WantsToShoot())
        {
            gunInterface?.Shoot();
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

    // Finds the camera belonging to this player.
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

        // Give the newly equipped gun the player's camera.
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

