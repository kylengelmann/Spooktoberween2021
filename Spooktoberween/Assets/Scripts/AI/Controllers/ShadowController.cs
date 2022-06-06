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
        BehaviorProperty<Vector3> SearchPosProp = new BehaviorProperty<Vector3>("CurrentPlayerSearchLocation", Vector3.zero);

        CanSeePlayerService canSeePlayerServ = new CanSeePlayerService();
        canSeePlayerServ.CanSeePlayerProp = SeeProp;
        canSeePlayerServ.LastPlayerPosProp = LastPosProp;
        canSeePlayerServ.LastPlayerMoveDirProp = LastDirProp;
        canSeePlayerServ.CurrentPlayerSearchLocationProp = SearchPosProp;

        SelectorNode RootSelector = new SelectorNode();
        canSeePlayerServ.SetChild(RootSelector);

        // Selector left branch
        ConditionalDecorator<bool> canSeePlayer = new ConditionalDecorator<bool>();
        canSeePlayer.Property = SeeProp;
        canSeePlayer.ReferenceValue = true;
        canSeePlayer.ComparisionOperation = (in bool A, in bool B) => { return A == B; };
        RootSelector.AddChild(canSeePlayer);

        MoveTo moveToPlayer = new MoveTo();
        moveToPlayer.GoalObjectProperty = PlayerProp;
        canSeePlayer.SetChild(moveToPlayer);

        // Selector right branch
        SequenceNode searchSequence = new SequenceNode();
        RootSelector.AddChild(searchSequence);

        // Search sequence left branch
        FindPlayerSearchLocationService searchLocationServ = new FindPlayerSearchLocationService();
        searchLocationServ.ExecuteInterval = 1e-4f;
        searchLocationServ.lastPlayerDirProp = LastDirProp;
        searchLocationServ.lastPlayerPosProp = LastPosProp;
        searchLocationServ.currentSearchLocationProp = SearchPosProp;
        searchSequence.AddChild(searchLocationServ);

        MoveTo moveToSearchLocation = new MoveTo();
        moveToSearchLocation.GoalVectorProperty = SearchPosProp;
        searchLocationServ.SetChild(moveToSearchLocation);

        // Search sequence right branch
        Wait wait = new Wait();
        wait.WaitDuration = 1f;
        searchSequence.AddChild(wait);

        Behavior = new BehaviorTree();
        List<BehaviorPropertyBase> Props = new List<BehaviorPropertyBase>();
        Props.Add(PlayerProp);
        Props.Add(SeeProp);
        Behavior.Init(this, canSeePlayerServ, Props);
    }
}
