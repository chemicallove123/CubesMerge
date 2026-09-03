/*
using UnityEngine;

public class BoxBSwappedColorState : IState
{
    private readonly BoxBColorChange context;

    public BoxBSwappedColorState(BoxBColorChange context)
    {
        this.context = context;
    }

    public void Enter()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        context.ApplyColor(randomColor);
    }

    public void Update() { }
    public void Exit() { }
}
*/