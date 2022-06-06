
public class SequenceNode : CompositeNode
{
    int RunningChild = 0;

    protected override ENodeStatus Start()
    {
        RunningChild = 0;

        return Children.Count > 0 ? ENodeStatus.Running : ENodeStatus.NotRunning;
    }

    protected override ENodeStatus Tick(float DeltaSeconds)
    {
        ENodeStatus Result = ENodeStatus.NotRunning;

        if(RunningChild < 0 || RunningChild >= Children.Count) RunningChild = 0;

        Result = Children[RunningChild].Update(DeltaSeconds);

        if(Result == ENodeStatus.Success)
        {
            ++RunningChild;
            if(RunningChild >= Children.Count)
            {
                Result = ENodeStatus.Success;
            }
            else
            {
                Result = ENodeStatus.Running;
            }
        }
        else if(Result == ENodeStatus.NotRunning)
        {
            RunningChild = 0;
        }

        return Result;
    }
}
