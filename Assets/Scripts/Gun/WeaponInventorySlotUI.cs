using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponInventorySlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectedBorder;
    [SerializeField] private TMP_Text slotNumberText;

    public void SetSlotNumber(int number)
    {
        if (slotNumberText != null)
            slotNumberText.text = number.ToString();
    }

    public void SetWeapon(
        WeaponItem item,
        bool selected)
    {
        if (item != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (selectedBorder != null)
            selectedBorder.enabled = selected;
    }
}