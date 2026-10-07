using System.Collections.Generic;
using UnityEngine;

public class WeaponOrbit : MonoBehaviour
{
    [Header("Follow")]
    [Tooltip("Usually the Main Camera.")]
    [SerializeField] private Transform followTarget;

    [Tooltip("Local offset from the camera/player.")]
    [SerializeField] private Vector3 followOffset = Vector3.zero;

    [Header("Orbit")]
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float height = -0.35f;

    [Header("Rotation")]
    [SerializeField] private float rotationStep = 30f;
    [SerializeField] private float rotationSpeed = 8f;

    private readonly List<Transform> guns =
        new List<Transform>();

    private readonly List<WeaponItem> weaponItems =
        new List<WeaponItem>();

    private float targetRotation = 0f;
    private float currentRotation = 0f;

    public int GunCount => guns.Count;

    private void LateUpdate()
    {
        FollowTarget();

        currentRotation = Mathf.LerpAngle(
            currentRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        UpdateGunPositions();
    }

    // FOLLOW CAMERA / PLAYER
    private void FollowTarget()
    {
        if (followTarget == null)
            return;

        transform.position =
            followTarget.TransformPoint(followOffset);

        transform.rotation =
            followTarget.rotation;
    }

    // SET GUNS
    public void SetGuns(
        List<Gun> gunList,
        List<WeaponItem> itemList)
    {
        guns.Clear();
        weaponItems.Clear();

        if (gunList != null)
        {
            foreach (Gun gun in gunList)
            {
                if (gun != null)
                {
                    guns.Add(gun.transform);
                }
            }
        }

        if (itemList != null)
        {
            foreach (WeaponItem item in itemList)
            {
                weaponItems.Add(item);
            }
        }

        UpdateGunPositions();
    }

    // ROTATION INPUT
    public void RotateLeft()
    {
        if (guns.Count == 0)
            return;

        targetRotation -= GetRotationAmount();
    }

    public void RotateRight()
    {
        if (guns.Count == 0)
            return;

        targetRotation += GetRotationAmount();
    }

    private float GetRotationAmount()
    {
        if (guns.Count > 0)
        {
            return 360f / guns.Count;
        }

        return rotationStep;
    }

    // GUN POSITIONS
    private void UpdateGunPositions()
    {
        if (guns.Count == 0)
            return;

        float angleBetweenGuns =
            360f / guns.Count;

        for (int i = 0; i < guns.Count; i++)
        {
            Transform gun = guns[i];

            if (gun == null)
                continue;

            float angle =
                (i * angleBetweenGuns) +
                currentRotation;

            float radians =
                angle * Mathf.Deg2Rad;

            Vector3 localPosition =
                new Vector3(
                    Mathf.Sin(radians) * radius,
                    height,
                    Mathf.Cos(radians) * radius
                );

            gun.localPosition = localPosition;

            // Make weapon point forward with the camera.
            Quaternion baseRotation =
                Quaternion.identity;

            // Apply individual weapon rotation offset.
            // This allows the Rocket Launcher to use Y = 180
            // without rotating every other gun.
            Vector3 rotationOffset =
                Vector3.zero;

            if (i < weaponItems.Count &&
                weaponItems[i] != null)
            {
                rotationOffset =
                    weaponItems[i]
                        .equippedRotationOffset;
            }

            gun.localRotation =
                baseRotation *
                Quaternion.Euler(rotationOffset);
        }
    }

    // DEBUG
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            radius
        );
    }
}