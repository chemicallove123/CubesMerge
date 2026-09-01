using System.Collections;
using TMPro;
using UnityEngine;

public class ChatBox : MonoBehaviour
{
    public static ChatBox Instance { get; private set; }
    [SerializeField] private TMP_Text textDisplay;
    [SerializeField] private float charactersPerSecond = 30f;
    [SerializeField] private string introMessage = "Shoot the cats";

    private Coroutine typingRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShowMessage(introMessage);
    }

    public void ShowMessage(string message)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

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
    }
}