
using UnityEngine;

public class Wait : BehaviorNode
{
    public BehaviorProperty<float> WaitDurationProp;
    public float WaitDuration;

    float timeWaitDone;

    protected override ENodeStatus Start()
    {
        float waitTime = WaitDurationProp == null ? WaitDuration : WaitDurationProp.Value;

        if(waitTime <= 0)
        {
            return ENodeStatus.Success;
        }

        timeWaitDone = Time.time + waitTime;

        return ENodeStatus.Running;
    }

    protected override ENodeStatus Tick(float DeltaSeconds)
    {
        if(Time.time >= timeWaitDone)
        {
            return ENodeStatus.Success;
        }

        return ENodeStatus.Running;
    }
}
