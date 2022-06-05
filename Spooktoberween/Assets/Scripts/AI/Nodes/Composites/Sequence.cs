
public class Sequence : CompositeNode
{
    int RunningChild = 0;

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
                RunningChild = 0;
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
