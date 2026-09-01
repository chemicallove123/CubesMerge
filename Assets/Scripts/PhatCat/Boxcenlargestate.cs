public class BoxCEnlargedState : IState
{
    private readonly BoxCSizeChange context;

    public BoxCEnlargedState(BoxCSizeChange context)
    {
        this.context = context;
    }

    public void Enter() => context.TransitionToScaleFactor(context.GrowScaleFactor);
    public void Update() { }
    public void Exit() { }
}