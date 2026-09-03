/*
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class DataDisplay : MonoBehaviour
{
    [Tooltip("Drag in any object whose script implements IReadable.")]
    [SerializeField] private MonoBehaviour targetObject;

    private IReadable readable;
    private TextMeshPro textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();

        readable = targetObject as IReadable;

        if (readable == null)
            Debug.LogWarning($"{name}: Target Object doesn't implement IReadable.");
    }

    private void OnEnable()
    {
        if (readable == null) return;

        readable.OnDataChanged += UpdateDisplay;
        UpdateDisplay(); // show the current value right away
    }

    private void OnDisable()
    {
        if (readable == null) return;
        readable.OnDataChanged -= UpdateDisplay;
    }

    private void UpdateDisplay()
    {
        textMesh.text = readable.ReadData();
    }
}
*/