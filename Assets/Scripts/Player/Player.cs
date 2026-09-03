using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Player : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 6f;
    [SerializeField] protected Gun gun; 
    [Tooltip("Where picked-up guns get parented - should be a child of the camera so aim follows full look rotation.")]
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
        gun = newGun;
        gunInterface = newGun;
    }
}