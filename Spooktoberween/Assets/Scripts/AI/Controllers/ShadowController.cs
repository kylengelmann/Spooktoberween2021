using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowController : AIController
{
    protected override void InitBehavior()
    {
        BehaviorProperty<GameObject> PlayerProp = new BehaviorProperty<GameObject>("Player", null);
        BehaviorProperty<bool> SeeProp = new BehaviorProperty<bool>("CanSeePlayer", false);
        BehaviorProperty<Vector3> LastPosProp = new BehaviorProperty<Vector3>("LastPlayerPos", Vector3.zero);
        BehaviorProperty<Vector3> LastDirProp = new BehaviorProperty<Vector3>("LastPlayerMoveDir", Vector3.zero);
        BehaviorProperty<Vector3> SearchPosProp = new BehaviorProperty<Vector3>("CurrentPlayerSearchLocation", Vector3.zero);
        BehaviorProperty<float> LostPlayerTimeProp = new BehaviorProperty<float>("LostPlayerTime", -1f);

        CanSeePlayerService canSeePlayerServ = new CanSeePlayerService();
        canSeePlayerServ.CanSeePlayerProp = SeeProp;
        canSeePlayerServ.LastPlayerPosProp = LastPosProp;
        canSeePlayerServ.LastPlayerMoveDirProp = LastDirProp;
        canSeePlayerServ.LostPlayerTime = LostPlayerTimeProp;

        SelectorNode RootSelector = new SelectorNode();
        canSeePlayerServ.SetChild(RootSelector);

        // Selector Disperse condition
        ConditionalDecorator<float> shouldDisperse = new ConditionalDecorator<float>();
        shouldDisperse.Property = LostPlayerTimeProp;
        shouldDisperse.ReferenceValue = 10f;
        shouldDisperse.ComparisionOperation = (in float TimeLostPlayer, in float TimeUntilDisperse) => { return TimeLostPlayer > 0f && Time.time - TimeLostPlayer > TimeUntilDisperse; };

        RootSelector.AddChild(shouldDisperse);

        Disperse disperse = new Disperse();

        shouldDisperse.SetChild(disperse);

        // Selector left branch
        ConditionalDecorator<bool> canSeePlayer = new ConditionalDecorator<bool>();
        canSeePlayer.Property = SeeProp;
        canSeePlayer.ReferenceValue = true;
        canSeePlayer.ComparisionOperation = (in bool A, in bool B) => { return A == B; };
        RootSelector.AddChild(canSeePlayer);

        SequenceNode moveSequence = new SequenceNode();
        canSeePlayer.SetChild(moveSequence);

        MoveTo moveToPlayer = new MoveTo();
        moveToPlayer.GoalObjectProperty = PlayerProp;
        moveSequence.AddChild(moveToPlayer);

        Attack attack = new Attack();
        attack.Target = PlayerProp;
        moveSequence.AddChild(attack);

        moveSequence.AddChild(disperse);

        // Selector right branch
        SequenceNode searchSequence = new SequenceNode();
        RootSelector.AddChild(searchSequence);

        FindPlayerSearchLocationService searchLocationFirstServ = new FindPlayerSearchLocationService();
        searchLocationFirstServ.ExecuteInterval = 1e-4f;
        searchLocationFirstServ.lastPlayerDirProp = LastDirProp;
        searchLocationFirstServ.lastPlayerPosProp = LastPosProp;
        searchLocationFirstServ.currentSearchLocationProp = SearchPosProp;
        searchLocationFirstServ.searchCenterProp = LastPosProp;
        searchSequence.AddChild(searchLocationFirstServ);

        MoveTo moveToLastPlayerPos = new MoveTo();
        moveToLastPlayerPos.GoalVectorProperty = LastPosProp;
        searchLocationFirstServ.SetChild(moveToLastPlayerPos);

        // Search sequence left branch
        LiarDecorator searchLoopDecorator = new LiarDecorator();
        searchLoopDecorator.Result = BehaviorNode.ENodeStatus.Running;
        searchSequence.AddChild(searchLoopDecorator);

        FindPlayerSearchLocationService searchLocationServ = new FindPlayerSearchLocationService();
        searchLocationServ.ExecuteInterval = 1e-4f;
        searchLocationServ.lastPlayerDirProp = LastDirProp;
        searchLocationServ.lastPlayerPosProp = LastPosProp;
        searchLocationServ.currentSearchLocationProp = SearchPosProp;
        searchLoopDecorator.SetChild(searchLocationServ);

        SequenceNode searchLoopSequence = new SequenceNode();
        searchLocationServ.SetChild(searchLoopSequence);

        MoveTo moveToSearchLocation = new MoveTo();
        moveToSearchLocation.GoalVectorProperty = SearchPosProp;
        searchLoopSequence.AddChild(moveToSearchLocation);

        // Search sequence right branch
        Wait wait = new Wait();
        wait.WaitDuration = 1f;
        searchLoopSequence.AddChild(wait);

        Behavior = new BehaviorTree();
        List<BehaviorPropertyBase> Props = new List<BehaviorPropertyBase>();
        Props.Add(PlayerProp);
        Props.Add(SeeProp);
        Behavior.Init(this, canSeePlayerServ, Props);
    }

    public override void Update()
    {
        if(SpookyGameManager.gameManager.player)
        {
            BehaviorPropertyBase playerProp;
            if (Behavior.TryGetProperty("Player", out playerProp))
            {
                if (playerProp is BehaviorProperty<GameObject>)
                {
                    ((BehaviorProperty<GameObject>)playerProp).Value = SpookyGameManager.gameManager.player.gameObject;
                }
            }
        }

        base.Update();
    }

    public void Attack()
    {
        CharacterAnimationComponent animComponent = GetComponent<CharacterAnimationComponent>();
        animComponent.onAttackEnded += AttackEnded;
        GetComponent<CharacterAnimationComponent>().Attack();
    }

    public void AttackEnded()
    {
        if(onAttackEnded != null)
        {
            onAttackEnded();
        }

        GetComponent<CharacterAnimationComponent>().onAttackEnded -= AttackEnded;
    }

    public delegate void OnAttackEnded();

    public OnAttackEnded onAttackEnded;
}
