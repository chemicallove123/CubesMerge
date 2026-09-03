using System;
using UnityEngine;

public class BoxBColorChange : MonoBehaviour, IHittable, IReadable
{
    [Tooltip("Drag in only the renderer(s) that should be tinted.")]
    [SerializeField] private Renderer[] colorableRenderers;

    private Material[] materialInstances; 
    private Color currentColor = Color.white;

    private StateMachine stateMachine;
    private IState orangeState;
    private IState swappedState;
    private bool isInitializing = true; 

    public event Action OnDataChanged;

    private void Awake()
    {
        materialInstances = new Material[colorableRenderers.Length];
        for (int i = 0; i < colorableRenderers.Length; i++)
        {
            if (colorableRenderers[i] == null) continue;
            if (colorableRenderers[i] is SpriteRenderer) continue;
            materialInstances[i] = colorableRenderers[i].material; 
        }

        stateMachine = new StateMachine();
        orangeState = new BoxBOrangeState(this);
        swappedState = new BoxBSwappedColorState(this);

        stateMachine.ChangeState(orangeState); 
        isInitializing = false;
    }

    private void Update()
    {
        stateMachine.Update();
    }

    public void Hit()
    {
        IState next = stateMachine.CurrentState == orangeState ? swappedState : orangeState;
        stateMachine.ChangeState(next);
    }

    public void ApplyColor(Color color)
    {
        currentColor = color;

        for (int i = 0; i < colorableRenderers.Length; i++)
        {
            if (colorableRenderers[i] == null) continue;

            if (colorableRenderers[i] is SpriteRenderer sr)
                sr.color = color;
            else if (materialInstances[i] != null)
                materialInstances[i].color = color;
        }

        if (!isInitializing)
            OnDataChanged?.Invoke();
    }

    public string ReadData()
    {
        return "Color: #" + ColorUtility.ToHtmlStringRGB(currentColor);
    }
}