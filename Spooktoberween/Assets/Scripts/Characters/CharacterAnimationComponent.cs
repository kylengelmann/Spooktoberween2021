using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationComponent : MonoBehaviour
{
    Character character;
    public Animator animator;

    int faceDirectionID = Animator.StringToHash("FaceDirection");
    int isWalkingID = Animator.StringToHash("IsWalking");
    int attackID = Animator.StringToHash("Attack");

    private void Awake()
    {
        character = GetComponent<Character>();
        if (!character)
        {
            Debug.LogError("CharacterAnimationComponent: No character found");
            enabled = false;
            return;
        }

        //animator = GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("CharacterAnimationComponent: No animator found");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        animator.SetFloat(faceDirectionID, (float)character.currentFaceDirection);
        animator.SetBool(isWalkingID, character.GetVelocity().sqrMagnitude > .005f);
    }

    public void Attack()
    {
        animator.ResetTrigger(attackID);
        animator.SetTrigger(attackID);
    }

    public delegate void OnAttackEnded();

    public OnAttackEnded onAttackEnded;

    public void EndAttack()
    {
        if(onAttackEnded != null)
        {
            onAttackEnded();
        }
    }
}
