/*
using UnityEngine;

public class HitDialogue : MonoBehaviour
{
    [Tooltip("Drag in any object whose script implements IReadable.")]
    [SerializeField] private MonoBehaviour targetObject;

    [SerializeField] private string firstHitMessage;
    [SerializeField] private string secondHitMessage;

    private IReadable readable;
    private bool showFirstMessage = true; 

    private void Awake()
    {
        readable = targetObject as IReadable;

        if (readable == null)
            Debug.LogWarning($"{name}: Target Object doesn't implement IReadable.");
    }

    private void OnEnable()
    {
        if (readable != null)
            readable.OnDataChanged += ShowNextMessage;
    }

    private void OnDisable()
    {
        if (readable != null)
            readable.OnDataChanged -= ShowNextMessage;
    }

    private void ShowNextMessage()
    {
        string message = showFirstMessage ? firstHitMessage : secondHitMessage;
        showFirstMessage = !showFirstMessage;

        if (ChatBox.Instance != null)
            ChatBox.Instance.ShowMessage(message);
    }
}

*/