using UnityEngine;

public class Gun : MonoBehaviour, IGun
{
    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("The point where the bullet physically comes out of the gun.")]
    [SerializeField] private Transform firePoint;

    [SerializeField] private float bulletSpeed = 20f;

    [Tooltip("Time between shots in seconds.")]
    [SerializeField] private float fireRate = 0.3f;

    [Header("Aiming")]
    [Tooltip("Camera used to determine where the crosshair is aiming.")]
    [SerializeField] private Camera playerCamera;

    [Tooltip("Maximum distance the gun can aim.")]
    [SerializeField] private float aimRange = 100f;

    [Tooltip("Layers that the aiming ray can hit.")]
    [SerializeField] private LayerMask aimLayerMask = ~0;

    private float nextFireTime;

    /// Gives this gun the player's camera when the player equips the gun.
    public void SetCamera(Camera camera)
    {
        playerCamera = camera;
    }

    public void Shoot()
    {
        // Prevent shooting faster than the fire rate.
        if (Time.time < nextFireTime)
            return;

        if (bulletPrefab == null)
        {
            Debug.LogWarning(
                $"[Gun] {gameObject.name} is missing a Bullet Prefab."
            );
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning(
                $"[Gun] {gameObject.name} is missing a Fire Point."
            );
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogWarning(
                $"[Gun] {gameObject.name} has no Player Camera assigned."
            );
            return;
        }

        // Set the next allowed shooting time.
        nextFireTime = Time.time + fireRate;

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

        GameObject bulletObj = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        Rigidbody bulletRigidbody =
            bulletObj.GetComponent<Rigidbody>();

        if (bulletRigidbody != null)
        {
            bulletRigidbody.linearVelocity =
                shootDirection * bulletSpeed;
        }
        else
        {
            Debug.LogWarning(
                $"[Gun] Bullet prefab '{bulletPrefab.name}' " +
                "does not have a Rigidbody."
            );
        }
    }
}
