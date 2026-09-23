using System.Collections.Generic;
using UnityEngine;

public class WeaponInventoryUI : MonoBehaviour
{
    [SerializeField]
    private WeaponInventorySlotUI[] slots;

    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].SetSlotNumber(i + 1);
        }
    }

    public void Refresh(
        List<WeaponItem> weapons,
        int selectedIndex)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            WeaponItem item =
                i < weapons.Count
                    ? weapons[i]
                    : null;

            bool selected =
                i == selectedIndex &&
                item != null;

            slots[i].SetWeapon(
                item,
                selected
            );
        }
    }
}