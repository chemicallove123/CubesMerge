using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private Transform carryPoint;
    [SerializeField] private int maxCarry = 2;
    [SerializeField] private float tweenDuration = 0.35f;

    private readonly List<Gun> equippedClones = new List<Gun>();
    private readonly List<GameObject> sourceObjects = new List<GameObject>(); 
    private readonly List<Vector3> pickupPositions = new List<Vector3>(); 

    private InputAction dropAction;

    private void Awake()
    {
        dropAction = new InputAction("DropGun", InputActionType.Button, "<Keyboard>/g");
    }

    private void OnEnable() => dropAction?.Enable();
    private void OnDisable() => dropAction?.Disable();
    private void OnDestroy() => dropAction?.Dispose();

    private void Update()
    {
        if (dropAction.WasPressedThisFrame())
            DropLastEquipped();
    }

    public bool TryEquip(Gun gunPrefab, GameObject sourceObject)
    {
        if (equippedClones.Count >= maxCarry)
        {
            Debug.Log("[WeaponInventory] Already carrying the max number of guns.");
            return false;
        }

        Transform slot = equippedClones.Count == 0 ? weaponSocket : carryPoint;
        Vector3 groundPosition = sourceObject.transform.position;

        Gun spawnedGun = Instantiate(gunPrefab, groundPosition, slot.rotation, slot);
        StripVisualCloneComponents(spawnedGun);

        equippedClones.Add(spawnedGun);
        sourceObjects.Add(sourceObject);
        pickupPositions.Add(groundPosition);
        sourceObject.SetActive(false);

        StartCoroutine(TweenLocalPosition(spawnedGun.transform, spawnedGun.transform.localPosition, Vector3.zero));

        if (slot == weaponSocket)
            player.EquipGun(spawnedGun);

        Debug.Log($"[WeaponInventory] Equipped {gunPrefab.name} into {(slot == weaponSocket ? "WeaponSocket" : "CarryPoint")}.");
        return true;
    }

    private void DropLastEquipped()
    {
        if (equippedClones.Count == 0) return;

        int lastIndex = equippedClones.Count - 1;
        Gun clone = equippedClones[lastIndex];
        GameObject source = sourceObjects[lastIndex];
        Vector3 groundPosition = pickupPositions[lastIndex];
        bool wasActiveWeapon = lastIndex == 0;

        equippedClones.RemoveAt(lastIndex);
        sourceObjects.RemoveAt(lastIndex);
        pickupPositions.RemoveAt(lastIndex);

        Vector3 equippedPosition = clone.transform.position; 
        Destroy(clone.gameObject);

        source.transform.position = equippedPosition; 
        source.transform.rotation = Quaternion.identity;
        source.SetActive(true);

        StartCoroutine(DropTween(source, groundPosition));

        if (wasActiveWeapon)
            player.UnequipGun();

        Debug.Log($"[WeaponInventory] Dropped {source.name}.");
    }

    private IEnumerator TweenLocalPosition(Transform target, Vector3 startLocalPosition, Vector3 endLocalPosition)
    {
        float elapsedTime = 0f;

        while (elapsedTime < tweenDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / tweenDuration;
            target.localPosition = Vector3.Lerp(startLocalPosition, endLocalPosition, t);
            yield return null;
        }

        target.localPosition = endLocalPosition;
    }

    private IEnumerator DropTween(GameObject source, Vector3 groundPosition)
    {
        Rigidbody sourceRigidbody = source.GetComponent<Rigidbody>();
        Collider sourceCollider = source.GetComponent<Collider>();

        bool hadGravity = sourceRigidbody != null && sourceRigidbody.useGravity;
        if (sourceRigidbody != null)
        {
            sourceRigidbody.isKinematic = true;
            sourceRigidbody.useGravity = false;
        }
        if (sourceCollider != null) sourceCollider.enabled = false;

        Vector3 startPosition = source.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < tweenDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / tweenDuration;
            Vector3 newPosition = Vector3.Lerp(startPosition, groundPosition, t);

            if (sourceRigidbody != null)
                sourceRigidbody.MovePosition(newPosition);
            else
                source.transform.position = newPosition;

            yield return null;
        }

        source.transform.position = groundPosition;

        if (sourceRigidbody != null)
        {
            sourceRigidbody.isKinematic = false;
            sourceRigidbody.useGravity = hadGravity;
        }
        if (sourceCollider != null) sourceCollider.enabled = true;
    }

    private void StripVisualCloneComponents(Gun spawnedGun)
    {
        if (spawnedGun.TryGetComponent(out PickupableObject spawnedPickupable))
            DestroyImmediate(spawnedPickupable);

        if (spawnedGun.TryGetComponent(out GunPickup spawnedGunPickup))
            DestroyImmediate(spawnedGunPickup);

        if (spawnedGun.TryGetComponent(out Rigidbody spawnedRigidbody))
            DestroyImmediate(spawnedRigidbody);

        if (spawnedGun.TryGetComponent(out Collider spawnedCollider))
            DestroyImmediate(spawnedCollider);
    }
}