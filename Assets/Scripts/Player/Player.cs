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

// Abstract base for anything that moves around, can turn, and can fire
// a gun. Movement is now relative to the player's own facing direction
// (transform.right / transform.forward) instead of fixed world axes -
// that only made sense back when the player never rotated. Now that
// PlayerLook turns the body with the mouse, movement needs to turn with it.
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

    public void UnequipGun()
    {
        gun = null;
        gunInterface = null;
        Debug.Log("[Player] Gun unequipped.");
    }
}