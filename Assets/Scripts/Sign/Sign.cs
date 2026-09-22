using UnityEngine;

public class Sign : MonoBehaviour, IInteractable
{
    [TextArea]
    [SerializeField] private string message = "Shoot the cats!";

    public void Interact()
    {
        Debug.Log($"[Sign] Interact() called on {gameObject.name}.");

        if (ChatBox.Instance == null)
        {
            Debug.LogWarning("[Sign] No ChatBox found in the scene.");
            return;
        }

        ChatBox.Instance.ShowMessage(message);
    }
}
