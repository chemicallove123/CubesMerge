using UnityEngine;

public abstract class Player : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 6f;
    [SerializeField] protected Gun gun; 

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
            gunInterface.Shoot();
    }

    protected virtual void FixedUpdate()
    {
        Vector3 velocity = moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    protected abstract Vector3 GetMoveInput();

    protected abstract bool WantsToShoot();
}