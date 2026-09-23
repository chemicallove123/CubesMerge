using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GunPickup : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponItem weaponItem;

    [Header("Pickup Animation")]
    [SerializeField] private float flySpeed = 12f;
    [SerializeField] private float arriveDistance = 0.1f;

    private Rigidbody rb;
    private Collider objectCollider;

    private bool isBeingCollected;
    private Transform collectionTarget;
    private WeaponInventory inventory;

    public WeaponItem WeaponItem => weaponItem;
    public bool IsBeingCollected => isBeingCollected;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
    }

    public bool BeginPickup(
        WeaponInventory targetInventory,
        Transform target)
    {
        if (isBeingCollected)
            return false;

        if (weaponItem == null)
        {
            Debug.LogWarning(
                $"[GunPickup] {name} has no WeaponItem assigned."
            );

            return false;
        }

        if (targetInventory == null || target == null)
            return false;

        if (!targetInventory.CanAcceptWeapon())
            return false;

        inventory = targetInventory;
        collectionTarget = target;
        isBeingCollected = true;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (objectCollider != null)
            objectCollider.enabled = false;

        return true;
    }

    private void Update()
    {
        if (!isBeingCollected || collectionTarget == null)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            collectionTarget.position,
            flySpeed * Time.deltaTime
        );

        if (Vector3.Distance(
            transform.position,
            collectionTarget.position) <= arriveDistance)
        {
            FinishPickup();
        }
    }

    private void FinishPickup()
    {
        if (!isBeingCollected)
            return;

        isBeingCollected = false;

        if (inventory != null)
            inventory.CompletePickup(weaponItem);

        Destroy(gameObject);
    }
}