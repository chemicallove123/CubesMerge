using UnityEngine;

public class Gun : MonoBehaviour, IGun
{
    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Point where the bullet comes out.")]
    [SerializeField] private Transform firePoint;

    [SerializeField] private float bulletSpeed = 20f;

    [Tooltip("Time between shots.")]
    [SerializeField] private float fireRate = 0.3f;

    [Header("Aiming")]
    [SerializeField] private Camera playerCamera;

    [SerializeField] private float aimRange = 100f;

    [SerializeField] private LayerMask aimLayerMask = ~0;

    private float nextFireTime;

    public void SetCamera(Camera camera)
    {
        playerCamera = camera;
    }

    public void Shoot()
    {
        if (Time.time < nextFireTime)
            return;

        if (bulletPrefab == null)
        {
            Debug.LogWarning(
                $"[Gun] {name}: Bullet Prefab is missing."
            );
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning(
                $"[Gun] {name}: Fire Point is missing."
            );
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogWarning(
                $"[Gun] {name}: Player Camera is missing."
            );
            return;
        }

        nextFireTime = Time.time + fireRate;

        // Aim from centre of screen.
        Ray cameraRay = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint;

        if (Physics.Raycast(
            cameraRay,
            out RaycastHit hit,
            aimRange,
            aimLayerMask,
            QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint =
                cameraRay.origin +
                cameraRay.direction * aimRange;
        }

        Vector3 shootDirection =
            (targetPoint - firePoint.position).normalized;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        // Prevent the bullet immediately colliding with
        // colliders belonging to this gun.
        Collider bulletCollider =
            bullet.GetComponent<Collider>();

        if (bulletCollider != null)
        {
            Collider[] gunColliders =
                GetComponentsInChildren<Collider>();

            foreach (Collider gunCollider in gunColliders)
            {
                if (gunCollider != null)
                {
                    Physics.IgnoreCollision(
                        bulletCollider,
                        gunCollider
                    );
                }
            }
        }

        Rigidbody bulletRb =
            bullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            bulletRb.linearVelocity =
                shootDirection * bulletSpeed;
        }
        else
        {
            Debug.LogWarning(
                $"[Gun] Bullet '{bullet.name}' has no Rigidbody."
            );
        }
    }
}