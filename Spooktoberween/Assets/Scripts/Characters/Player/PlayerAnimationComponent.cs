using UnityEngine;

public class PlayerAnimationComponent : MonoBehaviour
{
    SpookyPlayer player;
    Animator animator;

    int faceDirectionID = Animator.StringToHash("FaceDirection");
    int isWalkingID = Animator.StringToHash("IsWalking");

    private void Awake()
    {
        player = GetComponent<SpookyPlayer>();
        if(!player)
        {
            Debug.LogError("PlayerAnimationComponent: No player found");
            enabled = false;
            return;
        }

        animator = GetComponent<Animator>();
        if(!animator)
        {
            Debug.LogError("PlayerAnimationComponent: No animator found");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        animator.SetInteger(faceDirectionID, (int)player.currentFaceDirection);
        animator.SetBool(isWalkingID, player.GetVelocity().sqrMagnitude > .05f);
    }
}
