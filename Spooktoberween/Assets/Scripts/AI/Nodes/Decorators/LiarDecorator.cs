
public class LiarDecorator : DecoratorNode
{
    public ENodeStatus Result;

    protected override ENodeStatus Tick(float DeltaSeconds)
    {
        base.Tick(DeltaSeconds);

        return Result;
    }
}
