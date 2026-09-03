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