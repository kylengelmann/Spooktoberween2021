using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : BehaviorNode
{
    public BehaviorProperty<GameObject> Target;

    bool bAttackEnded = false;

    SpookShadow owningCharacter;

    protected override ENodeStatus Start()
    {
        bAttackEnded = false;
        ShadowController owningShadowController = GetOwningController() as ShadowController;
        if(!owningShadowController)
        {
            return ENodeStatus.NotRunning;
        }

        owningShadowController.onAttackEnded += OnAttackEnded;
        owningShadowController.Attack();

        owningCharacter = GetOwningController().ControlledCharacter as SpookShadow;
    
        if(!owningCharacter)
        {
            return ENodeStatus.NotRunning;
        }

        SpookManager.spookManager.onScream();

        return ENodeStatus.Running;
    }

    protected override ENodeStatus Tick(float DeltaSeconds)
    {
        if(!owningCharacter)
        {
            return ENodeStatus.NotRunning;
        }

        Vector3 toTarget = Target.Value.transform.position - owningCharacter.transform.position;
        EFaceDirection faceDirection = SpookyUtilities.VectorToFaceDirection(new Vector2(toTarget.x, toTarget.z));

        owningCharacter.SetOverrideFaceDirection(faceDirection);

        return bAttackEnded ? ENodeStatus.Success : ENodeStatus.Running;
    }

    protected override void End(bool bDidSucceed)
    {
        if(owningCharacter)
        {
            owningCharacter.SetOverrideFaceDirection(EFaceDirection.None);
        }
    }

    void OnAttackEnded()
    {
        bAttackEnded = true;
    }
}
