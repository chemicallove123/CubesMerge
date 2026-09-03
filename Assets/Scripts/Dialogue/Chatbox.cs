using System.Collections;
using TMPro;
using UnityEngine;

public class ChatBox : MonoBehaviour
{
    public static ChatBox Instance { get; private set; }

    [Tooltip("The panel containing both the background box and the text - this whole object gets shown/hidden together.")]
    [SerializeField] private GameObject chatBoxRoot;
    [SerializeField] private TMP_Text textDisplay;
    [SerializeField] private float charactersPerSecond = 30f;
    [SerializeField] private float autoHideDelay = 5f;

    private Coroutine typingRoutine;
    private Coroutine hideRoutine;

    private void Awake()
    {
        Instance = this;

        if (chatBoxRoot != null)
            chatBoxRoot.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (chatBoxRoot != null)
            chatBoxRoot.SetActive(true);

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        if (hideRoutine != null) StopCoroutine(hideRoutine);

        typingRoutine = StartCoroutine(TypeMessage(message));
    }

    private IEnumerator TypeMessage(string message)
    {
        textDisplay.text = "";
        float delay = 1f / charactersPerSecond;

        foreach (char c in message)
        {
            textDisplay.text += c;
            yield return new WaitForSeconds(delay);
        }

        typingRoutine = null;
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);

        if (chatBoxRoot != null)
            chatBoxRoot.SetActive(false);

        hideRoutine = null;
    }
}