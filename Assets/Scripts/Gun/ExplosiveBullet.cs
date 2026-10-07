using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ExplosiveBullet : MonoBehaviour
{
    [Header("Direct Hit Damage")]
    [Tooltip("Damage dealt directly to the enemy that the rocket hits.")]
    [SerializeField] private float directHitDamage = 30f;

    [Header("Explosion")]
    [Tooltip("Maximum damage dealt by the explosion near its center.")]
    [SerializeField] private float explosiveDamage = 60f;

    [Tooltip("Radius of the explosion.")]
    [SerializeField] private float explosionRadius = 5f;

    [Tooltip("Which layers can be affected by the explosion.")]
    [SerializeField] private LayerMask explosionLayerMask = ~0;

    [Header("Damage Falloff")]
    [Tooltip(
        "X = normalized distance from explosion. " +
        "Y = damage multiplier."
    )]
    [SerializeField]
    private AnimationCurve damageFalloff =
        AnimationCurve.Linear(
            0f,
            1f,
            1f,
            0f
        );

    [Header("Lifetime")]
    [Tooltip(
        "Maximum time the rocket can exist before being destroyed."
    )]
    [SerializeField] private float lifeTime = 8f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;

    private bool hasHit = false;

    // START
    private void Start()
    {
        Debug.Log(
            $"[ExplosiveBullet] Rocket spawned: {name} " +
            $"at position {transform.position}"
        );

        Destroy(
            gameObject,
            lifeTime
        );
    }

    // TRIGGER
    private void OnTriggerEnter(
        Collider other)
    {
        if (hasHit)
            return;

        Debug.Log(
            $"[ExplosiveBullet] Trigger hit: " +
            $"{other.name}"
        );

        HandleHit(
            other,
            transform.position
        );
    }

    // COLLISION
    private void OnCollisionEnter(
        Collision collision)
    {
        if (hasHit)
            return;

        Vector3 hitPosition =
            transform.position;

        if (collision.contactCount > 0)
        {
            hitPosition =
                collision.GetContact(0).point;
        }

        Debug.Log(
            $"[ExplosiveBullet] Collision hit: " +
            $"{collision.collider.name}"
        );

        HandleHit(
            collision.collider,
            hitPosition
        );
    }

    // HIT
    private void HandleHit(
        Collider hitCollider,
        Vector3 hitPosition)
    {
        if (hasHit)
            return;

        hasHit = true;

        // DIRECT HIT DAMAGE
        EnemyType directlyHitEnemy =
            hitCollider.GetComponentInParent<EnemyType>();

        if (directlyHitEnemy != null)
        {
            directlyHitEnemy.TakeDamage(
                directHitDamage
            );

            Debug.Log(
                $"[ExplosiveBullet] Direct hit on " +
                $"{directlyHitEnemy.name}. " +
                $"Damage: {directHitDamage}"
            );
        }

        // EXPLOSION
        Explode(
            hitPosition
        );

        // IMPACT EFFECT
        if (hitEffectPrefab != null)
        {
            Instantiate(
                hitEffectPrefab,
                hitPosition,
                Quaternion.identity
            );
        }

        // Destroy rocket after impact.
        Destroy(gameObject);
    }

    // EXPLOSION DAMAGE
    private void Explode(
        Vector3 explosionPosition)
    {
        Debug.Log(
            $"[ExplosiveBullet] Explosion at: " +
            $"{explosionPosition}"
        );

        Collider[] hitColliders =
            Physics.OverlapSphere(
                explosionPosition,
                explosionRadius,
                explosionLayerMask,
                QueryTriggerInteraction.Collide
            );

        // Prevent an enemy with several colliders
        // from taking explosion damage several times.
        HashSet<EnemyType> damagedEnemies =
            new HashSet<EnemyType>();

        foreach (Collider hitCollider in hitColliders)
        {
            EnemyType enemy =
                hitCollider.GetComponentInParent<EnemyType>();

            if (enemy == null)
                continue;

            if (damagedEnemies.Contains(enemy))
                continue;

            damagedEnemies.Add(enemy);

            Vector3 closestPoint =
                hitCollider.ClosestPoint(
                    explosionPosition
                );

            float distance =
                Vector3.Distance(
                    explosionPosition,
                    closestPoint
                );

            float normalizedDistance =
                Mathf.Clamp01(
                    distance /
                    explosionRadius
                );

            float damageMultiplier =
                damageFalloff.Evaluate(
                    normalizedDistance
                );

            float finalDamage =
                explosiveDamage *
                damageMultiplier;

            enemy.TakeDamage(
                finalDamage
            );

            Debug.Log(
                $"[ExplosiveBullet] Explosion hit " +
                $"{enemy.name}. " +
                $"Distance: {distance:F2}, " +
                $"Damage: {finalDamage:F2}"
            );
        }

        // EXPLOSION EFFECT

        if (explosionEffectPrefab != null)
        {
            Instantiate(
                explosionEffectPrefab,
                explosionPosition,
                Quaternion.identity
            );
        }
    }

    // GIZMOS
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}