using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour, IGun
{
    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 30f;

    [Header("Shooting")]
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private int bulletsPerShot = 1;
    [SerializeField] private float spreadAngle = 0f;

    [Header("Burst")]
    [SerializeField] private int burstCount = 1;
    [SerializeField] private float burstInterval = 0.1f;

    [Header("Magazine")]
    [SerializeField] private int magazineSize = 10;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("Aiming")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float aimRange = 100f;
    [SerializeField] private LayerMask aimLayerMask = ~0;

    [Header("Projectile Spawn")]
    [Tooltip("Moves the projectile slightly forward from the FirePoint so it does not hit the gun/player immediately.")]
    [SerializeField] private float spawnForwardOffset = 0.5f;

    private int currentAmmo;
    private bool isReloading;
    private bool isBursting;
    private float nextFireTime;

    public int CurrentAmmo => currentAmmo;
    public int MagazineSize => magazineSize;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        currentAmmo = magazineSize;
    }

    // CAMERA
    public void SetCamera(Camera newCamera)
    {
        playerCamera = newCamera;

        Debug.Log(
            $"[Gun] {name} camera assigned: " +
            $"{(playerCamera != null ? playerCamera.name : "NULL")}"
        );
    }

    // SHOOT
    public void Shoot()
    {
        if (isReloading)
            return;

        if (isBursting)
            return;

        if (Time.time < nextFireTime)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        StartCoroutine(FireBurst());
    }

    private IEnumerator FireBurst()
    {
        isBursting = true;

        int bursts = Mathf.Max(1, burstCount);

        for (int i = 0; i < bursts; i++)
        {
            if (currentAmmo <= 0)
                break;

            FireSingleShot();

            currentAmmo--;

            Debug.Log(
                $"[Gun] {name} fired. Ammo: " +
                $"{currentAmmo}/{magazineSize}"
            );

            if (i < bursts - 1)
            {
                yield return new WaitForSeconds(
                    burstInterval
                );
            }
        }

        nextFireTime = Time.time + cooldown;

        isBursting = false;
    }

    // FIRE PROJECTILE
    private void FireSingleShot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning(
                $"[Gun] {name} has no Bullet Prefab assigned."
            );

            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning(
                $"[Gun] {name} has no Fire Point assigned."
            );

            return;
        }

        int bulletAmount = Mathf.Max(
            1,
            bulletsPerShot
        );

        for (int i = 0; i < bulletAmount; i++)
        {
            Vector3 direction = GetAimDirection();

            direction = ApplySpread(direction);

            // IMPORTANT:
            // Spawn slightly forward in the direction
            // the projectile is actually travelling.
            Vector3 spawnPosition =
                firePoint.position +
                direction * spawnForwardOffset;

            Quaternion spawnRotation =
                Quaternion.LookRotation(
                    direction,
                    Vector3.up
                );

            GameObject projectile =
                Instantiate(
                    bulletPrefab,
                    spawnPosition,
                    spawnRotation
                );

            Rigidbody projectileRb =
                projectile.GetComponent<Rigidbody>();

            if (projectileRb != null)
            {
                projectileRb.linearVelocity =
                    direction * bulletSpeed;
            }
            else
            {
                Debug.LogWarning(
                    $"[Gun] {projectile.name} has no Rigidbody."
                );
            }

            // Ignore collisions between projectile
            // and the gun that fired it.
            IgnoreGunCollisions(projectile);

            Debug.Log(
                $"[Gun] {name} spawned projectile: " +
                $"{projectile.name} from prefab " +
                $"{bulletPrefab.name}"
            );

            Debug.Log(
                $"[Gun] Projectile direction: {direction} | " +
                $"FirePoint WORLD position: {firePoint.position} | " +
                $"Spawn WORLD position: {spawnPosition}"
            );
        }
    }

    // IGNORE GUN COLLISION
    private void IgnoreGunCollisions(GameObject projectile)
    {
        Collider[] projectileColliders =
            projectile.GetComponentsInChildren<Collider>();

        Collider[] gunColliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider projectileCollider
                 in projectileColliders)
        {
            foreach (Collider gunCollider
                     in gunColliders)
            {
                if (projectileCollider != null &&
                    gunCollider != null)
                {
                    Physics.IgnoreCollision(
                        projectileCollider,
                        gunCollider,
                        true
                    );
                }
            }
        }
    }

    // AIMING
    private Vector3 GetAimDirection()
    {
        if (firePoint == null)
            return transform.forward;

        // If there is no camera, use muzzle direction.
        if (playerCamera == null)
        {
            Debug.LogWarning(
                $"[Gun] {name} has no Player Camera. " +
                $"Using FirePoint.forward."
            );

            return firePoint.forward;
        }

        // Ray through centre of screen / crosshair.
        Ray aimRay =
            playerCamera.ViewportPointToRay(
                new Vector3(
                    0.5f,
                    0.5f,
                    0f
                )
            );

        Vector3 targetPoint =
            aimRay.origin +
            aimRay.direction * aimRange;

        if (Physics.Raycast(
            aimRay,
            out RaycastHit hit,
            aimRange,
            aimLayerMask,
            QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;

            Debug.Log(
                $"[Gun] Crosshair hit: {hit.collider.name} " +
                $"at {hit.point}"
            );
        }

        Vector3 direction =
            targetPoint -
            firePoint.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return aimRay.direction.normalized;
        }

        return direction.normalized;
    }

    // SPREAD
    private Vector3 ApplySpread(Vector3 direction)
    {
        if (spreadAngle <= 0f)
            return direction;

        Quaternion spread =
            Quaternion.Euler(
                Random.Range(
                    -spreadAngle,
                    spreadAngle
                ),
                Random.Range(
                    -spreadAngle,
                    spreadAngle
                ),
                0f
            );

        return (spread * direction).normalized;
    }

    // RELOAD
    private IEnumerator Reload()
    {
        if (isReloading)
            yield break;

        isReloading = true;

        Debug.Log(
            $"[Gun] {name} reloading..."
        );

        yield return new WaitForSeconds(
            reloadTime
        );

        currentAmmo = magazineSize;

        isReloading = false;

        Debug.Log(
            $"[Gun] {name} reloaded. Ammo: " +
            $"{currentAmmo}/{magazineSize}"
        );
    }
}