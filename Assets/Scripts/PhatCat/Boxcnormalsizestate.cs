public class BoxCNormalSizeState : IState
{
    private readonly BoxCSizeChange context;

    public BoxCNormalSizeState(BoxCSizeChange context)
    {
        this.context = context;
    }

    public void Enter() => context.TransitionToScaleFactor(1f);
    public void Update() { }
    public void Exit() { }
}