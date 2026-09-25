using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponInventorySlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectedBorder;
    [SerializeField] private TMP_Text slotNumberText;

    [Header("Selected Border")]
    [SerializeField] private Color selectedColor = Color.yellow;

    private void Awake()
    {
        if (selectedBorder != null)
        {
            selectedBorder.color = selectedColor;
            selectedBorder.enabled = false;
        }
    }

    public void SetSlotNumber(int number)
    {
        if (slotNumberText != null)
        {
            slotNumberText.text = number.ToString();
        }
    }

    public void SetWeapon(WeaponItem item, bool selected)
    {
        // ICON
        if (iconImage != null)
        {
            if (item != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
                iconImage.color = Color.white;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
        }

        // SELECTED BORDER
        if (selectedBorder != null)
        {
            selectedBorder.color = selectedColor;
            selectedBorder.enabled =
                item != null && selected;
        }
    }
}