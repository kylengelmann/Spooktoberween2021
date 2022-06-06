
using UnityEngine;

public class CanSeePlayerService : ServiceNode
{
    public BehaviorProperty<bool> CanSeePlayerProp;
    public BehaviorProperty<Vector3> LastPlayerPosProp;
    public BehaviorProperty<Vector3> LastPlayerMoveDirProp;
    public BehaviorProperty<Vector3> CurrentPlayerSearchLocationProp;

    public float TimeToLosePlayer = .5f;
    public float MaxSightDistance = 5f;

    readonly int visibilityLayerMask = 1 << 6;
    float timeLostSight = -1f;
    SpookyPlayer player;
    Character controlledCharacter;

    protected override ENodeStatus Start()
    {
        player = SpookyGameManager.gameManager.player;
        timeLostSight = -1f;
        AIController controller = GetOwningController();
        if(controller)
        {
            controlledCharacter = controller.ControlledCharacter;
        }

        return base.Start();
    }

    protected override void Execute()
    {
        if(controlledCharacter && player && CanSeePlayerProp != null)
        {
            Vector3 visCheckOrigin = controlledCharacter.transform.position;
            visCheckOrigin.y = 2f;

            RaycastHit hitInfo;

            Vector3 navToPlayer = Vector3.ProjectOnPlane(player.transform.position - controlledCharacter.transform.position, Vector3.up);
            if (Physics.Raycast(visCheckOrigin, navToPlayer, out hitInfo, Mathf.Min(MaxSightDistance, navToPlayer.magnitude), visibilityLayerMask, QueryTriggerInteraction.Ignore))
            {
                //Debug.DrawLine(visCheckOrigin, visCheckOrigin + Vector3.ProjectOnPlane(hitInfo.point - controlledCharacter.transform.position, Vector3.up), Color.green, 5f);
                //Debug.DrawLine(hitInfo.point, hitInfo.point + Vector3.ProjectOnPlane(player.transform.position - hitInfo.point, Vector3.up), Color.red, 5f);

                if (timeLostSight > 0f)
                {
                    if (Time.time - timeLostSight > TimeToLosePlayer)
                    {
                        CanSeePlayerProp.Value = false;
                    }
                }
                else
                {
                    timeLostSight = Time.time;
                }
            }
            else
            {
                //Debug.DrawLine(visCheckOrigin, visCheckOrigin + Vector3.ProjectOnPlane(player.transform.position - controlledCharacter.transform.position, Vector3.up), Color.green, 5f);

                CanSeePlayerProp.Value = true;
                timeLostSight = -1f;

                if(LastPlayerPosProp != null)
                {
                    LastPlayerPosProp.Value = player.transform.position;
                    if(CurrentPlayerSearchLocationProp != null)
                    {
                        CurrentPlayerSearchLocationProp.Value = player.transform.position;
                    }
                }
                if(LastPlayerMoveDirProp != null)
                {
                    LastPlayerMoveDirProp.Value = player.GetVelocity().normalized;
                }
            }
        }
    }
}
