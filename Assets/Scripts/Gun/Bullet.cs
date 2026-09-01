using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private GameObject hitEffectPrefab; // particle burst, plays on impact

    private bool hasHit = false; // guards against reacting more than once

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.TryGetComponent(out IHittable hittable))
        {
            hittable.Hit();
            hasHit = true;

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}