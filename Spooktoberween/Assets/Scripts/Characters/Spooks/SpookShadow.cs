using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpookShadow : Character
{
    NavMeshAgent navAgent;

    public EFaceDirection overrideFaceDirection { get; protected set;}

    private void Start()
    {
        overrideFaceDirection = EFaceDirection.None;

        navAgent = GetComponent<NavMeshAgent>();
        if(!navAgent) return;

        navAgent.updateRotation = false;
    }

    public override Vector2 GetVelocity() { return new Vector2(navAgent.velocity.x, navAgent.velocity.z); }

    public void SetOverrideFaceDirection(EFaceDirection newOverride)
    {
        overrideFaceDirection = newOverride;

        if(newOverride != EFaceDirection.None)
        {
            currentFaceDirection = overrideFaceDirection;
        }
    }

    private void Update()
    {
        if(overrideFaceDirection == EFaceDirection.None)
        {
            EFaceDirection newFaceDir = SpookyUtilities.VectorToFaceDirection(GetVelocity());
            currentFaceDirection = newFaceDir == EFaceDirection.None ? currentFaceDirection : newFaceDir;
        }
    }
}