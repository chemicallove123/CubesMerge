using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWeaponItem",
    menuName = "Weapons/Weapon Item"
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
    public Gun equippedPrefab;
    public GameObject worldPrefab;

    [Header("Equipped Weapon Transform")]
    [Tooltip(
        "Extra rotation for this weapon while it is floating " +
        "around the player."
    )]
    public Vector3 equippedRotationOffset = Vector3.zero;
}