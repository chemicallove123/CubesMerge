using System.Collections.Generic;
using UnityEngine;

public class WeaponInventoryUI : MonoBehaviour
{
    [Header("Inventory Slots")]
    [SerializeField] private WeaponInventorySlotUI[] slots;

    private void Awake()
    {
        SetupSlotNumbers();
    }

    private void Start()
    {
        SetupSlotNumbers();
    }

    private void SetupSlotNumbers()
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].SetSlotNumber(i + 1);
            }
        }
    }

    public void Refresh(
        List<WeaponItem> weapons,
        int selectedIndex)
    {
        if (slots == null)
        {
            Debug.LogWarning(
                "[WeaponInventoryUI] Slots array is null."
            );

            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            WeaponInventorySlotUI slot = slots[i];

            if (slot == null)
            {
                Debug.LogWarning(
                    $"[WeaponInventoryUI] Slot {i + 1} is not assigned."
                );

                continue;
            }

            WeaponItem item = null;

            if (weapons != null &&
                i >= 0 &&
                i < weapons.Count)
            {
                item = weapons[i];
            }

            bool selected =
                item != null &&
                i == selectedIndex;

            slot.SetWeapon(
                item,
                selected
            );
        }

        Debug.Log(
            $"[WeaponInventoryUI] Refreshed UI. " +
            $"Weapons: {(weapons != null ? weapons.Count : 0)}, " +
            $"Slots: {slots.Length}, " +
            $"Selected: {selectedIndex}"
        );
    }
}