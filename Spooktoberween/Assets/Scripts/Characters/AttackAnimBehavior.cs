using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimBehavior : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CharacterAnimationComponent animComponent = animator.GetComponentInParent<CharacterAnimationComponent>();
        animComponent.EndAttack();
    }
}
