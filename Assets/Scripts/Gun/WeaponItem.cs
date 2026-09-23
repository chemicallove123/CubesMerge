using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponItem",
    menuName = "CubesMerge/Weapon Item"
)]
public class WeaponItem : ScriptableObject
{
    [Header("Information")]
    public string weaponName;

    [TextArea]
    public string description;

    [Header("Inventory")]
    public Sprite icon;

    [Header("Prefabs")]
    [Tooltip("Prefab displayed in the player's hand.")]
    public Gun equippedPrefab;

    [Tooltip("Prefab spawned back into the world when dropped.")]
    public GameObject worldPrefab;
}