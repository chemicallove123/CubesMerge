using UnityEngine;

public class WeaponTwirl : MonoBehaviour
{
    [Header("Twirl")]
    [SerializeField] private float rotationSpeed = 60f;

    private void Update()
    {
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );
    }
}