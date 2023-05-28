using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : BehaviorNode
{
    public BehaviorProperty<GameObject> Target;

    protected override ENodeStatus Start()
    {
        (Target.Value).GetComponent<SpookyPlayer>().Damage();
        return ENodeStatus.Success;
    }
}
