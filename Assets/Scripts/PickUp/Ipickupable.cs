using UnityEngine;

public interface IPickupable
{
    Transform PickupTransform { get; }
    void OnPickedUp();
    void OnDropped(Vector3 dropPosition);
}