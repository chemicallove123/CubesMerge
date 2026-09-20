using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupableObject : MonoBehaviour, IPickupable
{

    [SerializeField] private float flySpeed = 10f;

    private Rigidbody objectRigidbody;
    private Collider objectCollider;
    private Transform followTarget;
    private Vector3 followOffset;

    public bool IsFlying { get; private set; }

    private void Awake()
    {
        objectRigidbody = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
    }

    public void PickUp(Transform followTarget, Vector3 followOffset)
    {
        this.followTarget = followTarget;
        this.followOffset = followOffset;

        IsFlying = true;
        objectRigidbody.useGravity = false;
        objectCollider.enabled = false; // don't bump the player or other carried items mid-flight
    }

    public void Drop()
    {
        followTarget = null;
        IsFlying = false;

        objectRigidbody.useGravity = true;
        objectCollider.enabled = true;
    }

    private void FixedUpdate()
    {
        if (followTarget == null) return;

        Vector3 targetPosition = followTarget.position + followTarget.TransformDirection(followOffset);
        Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * flySpeed);
        objectRigidbody.MovePosition(newPosition);

        float arriveDistance = .05f;
        if (IsFlying && Vector3.Distance(transform.position, targetPosition) < arriveDistance)
        {
            IsFlying = false;
        }
    }

}