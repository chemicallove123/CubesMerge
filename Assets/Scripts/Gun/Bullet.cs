using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("Damage dealt directly to an EnemyType that this bullet hits.")]
    [SerializeField] private float directHitDamage = 20f;

    [Header("Lifetime")]
    [Tooltip("Maximum amount of time the bullet can exist before being destroyed.")]
    [SerializeField] private float lifeTime = 5f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;

    private bool hasHit = false;

    private void Start()
    {
        // Destroy bullets that never hit anything.
        Destroy(
            gameObject,
            lifeTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(
            other,
            transform.position
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 hitPosition =
            transform.position;

        if (collision.contactCount > 0)
        {
            hitPosition =
                collision.GetContact(0).point;
        }

        HandleHit(
            collision.collider,
            hitPosition
        );
    }

    private void HandleHit(
        Collider hitCollider,
        Vector3 hitPosition)
    {
        // Prevent one bullet from registering multiple collisions.
        if (hasHit)
            return;

        hasHit = true;

        // Look for EnemyType on the object or one of its parents.
        EnemyType enemy =
            hitCollider.GetComponentInParent<EnemyType>();

        if (enemy != null)
        {
            enemy.TakeDamage(
                directHitDamage
            );

            Debug.Log(
                $"[Bullet] Hit enemy: {enemy.name}. " +
                $"Direct damage: {directHitDamage}"
            );
        }

        // Optional impact effect.
        if (hitEffectPrefab != null)
        {
            Instantiate(
                hitEffectPrefab,
                hitPosition,
                Quaternion.identity
            );
        }

        // Bullets destroyed when they hit ANYTHING.
        Destroy(gameObject);
    }
}