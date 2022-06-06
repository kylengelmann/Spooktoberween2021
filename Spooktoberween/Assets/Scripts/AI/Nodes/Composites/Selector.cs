
public class SelectorNode : CompositeNode
{
    int RunningChild = -1;

    protected override ENodeStatus Start()
    {
        RunningChild = -1;
        return base.Start();
    }

    protected override ENodeStatus Tick(float DeltaSeconds)
    {
        ENodeStatus Result = ENodeStatus.NotRunning;
        for(int i = 0; i < Children.Count; ++i)
        {
            Result = Children[i].Update(DeltaSeconds);
                
            if(Result != ENodeStatus.NotRunning)
            {
                if(i != RunningChild && RunningChild >= 0 && RunningChild < Children.Count)
                {
                    Children[RunningChild].Cancel();
                }

                RunningChild = i;
                break;
            }
        }

        return Result;
    }

    protected override void End(bool bDidSucceed)
    {
        if(RunningChild >= 0 && RunningChild < Children.Count)
        {
            Children[RunningChild].Cancel();
        }

        RunningChild = -1;
    }
}