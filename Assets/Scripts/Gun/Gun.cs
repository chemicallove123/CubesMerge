/*
using UnityEngine;

public class Gun : MonoBehaviour, IGun
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.3f; // seconds between shots

    private float nextFireTime;

    public void Shoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Gun is missing a bullet prefab or fire point.");
            return;
        }

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bulletObj.GetComponent<Rigidbody>();

        if (rb != null)
            rb.linearVelocity = firePoint.forward * bulletSpeed;
    }
}
*/


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

    /// <summary>
    /// Gives this gun the player's camera.
    /// Called automatically when the player equips the gun.
    /// </summary>
    public void SetCamera(Camera camera)
    {
        playerCamera = camera;
    }

    public void Shoot()
    {
        // Prevent shooting faster than the fire rate.
        if (Time.time < nextFireTime)
            return;

        // Check required references.
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

        // ---------------------------------------------------------
        // 1. Create a ray from the exact centre of the camera.
        //
        // 0.5 = horizontal centre
        // 0.5 = vertical centre
        //
        // This is where the crosshair is located.
        // ---------------------------------------------------------

        Ray cameraRay = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint;

        // ---------------------------------------------------------
        // 2. Find what the crosshair is aiming at.
        // ---------------------------------------------------------

        if (Physics.Raycast(
            cameraRay,
            out RaycastHit hit,
            aimRange,
            aimLayerMask,
            QueryTriggerInteraction.Ignore))
        {
            // The crosshair is aiming at an object.
            targetPoint = hit.point;
        }
        else
        {
            // Nothing was hit.
            // Aim toward a point far away.
            targetPoint =
                cameraRay.origin +
                cameraRay.direction * aimRange;
        }

        // ---------------------------------------------------------
        // 3. Calculate the direction from the gun muzzle
        //    toward the crosshair target.
        // ---------------------------------------------------------

        Vector3 shootDirection =
            (targetPoint - firePoint.position).normalized;

        // ---------------------------------------------------------
        // 4. Spawn the bullet at the gun's FirePoint.
        // ---------------------------------------------------------

        GameObject bulletObj = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        // ---------------------------------------------------------
        // 5. Give the bullet its velocity.
        // ---------------------------------------------------------

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
