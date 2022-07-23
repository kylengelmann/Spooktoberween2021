using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowAnimation : MonoBehaviour
{

    public Animator animator;

    int faceDirectionID = Animator.StringToHash("FaceDirection");
    int isWalkingID = Animator.StringToHash("IsWalking");

    private void Awake()
    {/*
        player = GetComponent<ShadowAnimation>();
        if (!player)
        {
            Debug.LogError("PlayerAnimationComponent: No player found");
            enabled = false;
            return;
        }

        //animator = GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("PlayerAnimationComponent: No animator found");
            enabled = false;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat(faceDirectionID, (float)player.currentFaceDirection);
        animator.SetBool(isWalkingID, player.GetVelocity().sqrMagnitude > .05f);
    */}
}
