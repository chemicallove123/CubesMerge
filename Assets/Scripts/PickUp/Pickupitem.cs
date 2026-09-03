using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour, IPickupable
{
    private Rigidbody rb;
    private Collider col;

    public Transform PickupTransform => transform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void OnPickedUp()
    {
        rb.isKinematic = true; 
        col.enabled = false;  
    }

    public void OnDropped(Vector3 dropPosition)
    {
        transform.SetParent(null);
        transform.position = dropPosition;
        rb.isKinematic = false;
        col.enabled = true;
    }
}