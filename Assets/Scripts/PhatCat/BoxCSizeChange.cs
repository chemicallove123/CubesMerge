using System;
using System.Collections;
using UnityEngine;

public class BoxCSizeChange : MonoBehaviour, IHittable, IReadable
{
    [Header("Size Settings")]
    [Tooltip("Multiplier applied to the original scale when grown.")]
    [SerializeField] private float growScaleFactor = 2f;
    [Tooltip("How long (seconds) the grow/shrink transition takes.")]
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Hit Detection")]
    [Tooltip("Minimum time (seconds) between registered hits.")]
    [SerializeField] private float hitCooldown = 0.75f;

    public float GrowScaleFactor => growScaleFactor;

    private Vector3 originalScale;
    private float lastHitTime = -999f;
    private float currentScaleFactor = 1f;

    private Coroutine scaleRoutine;
    private Renderer[] renderers; 
    private float groundY;     

    private StateMachine stateMachine;
    private IState normalState;
    private IState enlargedState;

    public event Action OnDataChanged;

    private void Awake()
    {
        originalScale = transform.localScale;
        renderers = GetComponentsInChildren<Renderer>(true);
        groundY = GetLowestPointY();

        stateMachine = new StateMachine();
        normalState = new BoxCNormalSizeState(this);
        enlargedState = new BoxCEnlargedState(this);

        stateMachine.ChangeState(normalState); 
    }

    private void Update()
    {
        stateMachine.Update();
    }

    public void Hit()
    {
        if (Time.time - lastHitTime < hitCooldown)
            return;

        lastHitTime = Time.time;

        IState next = stateMachine.CurrentState == normalState ? enlargedState : normalState;
        stateMachine.ChangeState(next);
    }

    public void TransitionToScaleFactor(float scaleFactor)
    {
        currentScaleFactor = scaleFactor;
        Vector3 targetScale = originalScale * scaleFactor;

        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleCoroutine(transform.localScale, targetScale));

        OnDataChanged?.Invoke();
    }

    public string ReadData()
    {
        return $"Scale: {currentScaleFactor:0.00}x";
    }

    private IEnumerator ScaleCoroutine(Vector3 startScale, Vector3 endScale)
    {
        float elapsedTime = 0f;
        float transitionPercentage = 0f;

        while (transitionPercentage < 1f)
        {
            elapsedTime += Time.deltaTime;
            transitionPercentage = elapsedTime / transitionDuration;
            transform.localScale = Vector3.Lerp(startScale, endScale, transitionPercentage);

            SnapToGround();

            yield return null;
        }

        transform.localScale = endScale; 
        SnapToGround();
        scaleRoutine = null;
    }

    private void SnapToGround()
    {
        float currentLowestY = GetLowestPointY();
        float offset = groundY - currentLowestY;
        transform.position += new Vector3(0f, offset, 0f);
    }

    private float GetLowestPointY()
    {
        if (renderers == null || renderers.Length == 0)
            return transform.position.y;

        float lowest = float.MaxValue;
        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            if (r.bounds.min.y < lowest)
                lowest = r.bounds.min.y;
        }
        return lowest;
    }
}