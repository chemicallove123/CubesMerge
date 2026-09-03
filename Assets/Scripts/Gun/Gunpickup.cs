using UnityEngine;

public class GunPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private Gun gunPrefab;

    public void Interact()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player == null || player.WeaponSocket == null)
        {
            Debug.LogWarning("No player/weapon socket found to equip the gun onto.");
            return;
        }

        Transform socket = player.WeaponSocket;

        foreach (Transform child in socket)
            Destroy(child.gameObject);

        Gun spawnedGun = Instantiate(gunPrefab, socket.position, socket.rotation, socket);
        player.EquipGun(spawnedGun);

        gameObject.SetActive(false); // picked up - remove 
    }
}