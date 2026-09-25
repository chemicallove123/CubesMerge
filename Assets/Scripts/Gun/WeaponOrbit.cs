using System.Collections.Generic;
using UnityEngine;

public class WeaponOrbit : MonoBehaviour
{
    [Header("Orbit")]
    [SerializeField] private float radius = 2.2f;
    [SerializeField] private float height = -0.3f;

    [Header("Rotation")]
    [SerializeField] private float rotationStep = 30f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Gun Orientation")]
    [Tooltip("Extra rotation applied to every gun.")]
    [SerializeField] private Vector3 gunRotationOffset = Vector3.zero;

    private readonly List<Transform> guns =
        new List<Transform>();

    private float targetRotation = 0f;
    private float currentRotation = 0f;

    public int GunCount => guns.Count;

    private void Update()
    {
        currentRotation = Mathf.LerpAngle(
            currentRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        UpdateGunPositions();
    }

    public void SetGuns(List<Gun> gunList)
    {
        guns.Clear();

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

        UpdateGunPositions();
    }

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

            Vector3 outwardDirection =
                new Vector3(
                    localPosition.x,
                    0f,
                    localPosition.z
                );

            if (outwardDirection.sqrMagnitude > 0.001f)
            {
                Quaternion outwardRotation =
                    Quaternion.LookRotation(
                        outwardDirection.normalized,
                        Vector3.up
                    );

                gun.localRotation =
                    outwardRotation *
                    Quaternion.Euler(gunRotationOffset);
            }
        }
    }
}