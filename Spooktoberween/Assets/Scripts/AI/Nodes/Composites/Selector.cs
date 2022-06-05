
public class Selector : CompositeNode
{
    int RunningChild = -1;

    protected override ENodeStatus Tick(float DeltaSeconds)
    {
        ENodeStatus Result = ENodeStatus.NotRunning;
        if(RunningChild <= 0 && RunningChild < Children.Count)
        {
            Result = Children[RunningChild].Update(DeltaSeconds);

            if(Result != ENodeStatus.Running)
            {
                RunningChild = -1;
            }
        }
        else
        {
            for(int i = 0; i < Children.Count; ++i)
            {
                Result = Children[i].Update(DeltaSeconds);
                
                if(Result != ENodeStatus.NotRunning)
                {
                    RunningChild = i;
                    break;
                }
            }
        }

        return Result;
    }
}