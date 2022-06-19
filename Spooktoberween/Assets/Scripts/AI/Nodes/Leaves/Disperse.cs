using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disperse : BehaviorNode
{
    protected override ENodeStatus Start()
    {
        SpookManager.spookManager.OnShadowDispersed();
        Object.Destroy(GetOwningController().ControlledCharacter.gameObject);

        return ENodeStatus.Success;
    }
}
