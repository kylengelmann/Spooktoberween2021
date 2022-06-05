using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowController : AIController
{
    protected override void InitBehavior()
    {
        BehaviorProperty<GameObject> PlayerProp = new BehaviorProperty<GameObject>("Player", SpookyGameManager.gameManager.player.gameObject);
        BehaviorProperty<bool> SeeProp = new BehaviorProperty<bool>("CanSeePlayer", false);
        BehaviorProperty<Vector3> LastPosProp = new BehaviorProperty<Vector3>("LastPlayerPos", Vector3.zero);
        BehaviorProperty<Vector3> LastDirProp = new BehaviorProperty<Vector3>("LastPlayerMoveDir", Vector3.zero);

        MoveTo moveToPlayer = new MoveTo();
        moveToPlayer.GoalObjectProperty = PlayerProp;

        ConditionalDecorator<bool> canSeePlayer = new ConditionalDecorator<bool>();
        canSeePlayer.SetChild(moveToPlayer);
        canSeePlayer.Property = SeeProp;
        canSeePlayer.ReferenceValue = true;
        canSeePlayer.ComparisionOperation = (in bool A, in bool B) => { return A == B; };

        CanSeePlayerService canSeePlayerServ = new CanSeePlayerService();
        canSeePlayerServ.SetChild(canSeePlayer);
        canSeePlayerServ.CanSeePlayerProp = SeeProp;
        canSeePlayerServ.LastPlayerPosProp = LastPosProp;
        canSeePlayerServ.LastPlayerMoveDirProp = LastDirProp;

        Behavior = new BehaviorTree();
        List<BehaviorPropertyBase> Props = new List<BehaviorPropertyBase>();
        Props.Add(PlayerProp);
        Props.Add(SeeProp);
        Behavior.Init(this, canSeePlayerServ, Props);
    }
}
