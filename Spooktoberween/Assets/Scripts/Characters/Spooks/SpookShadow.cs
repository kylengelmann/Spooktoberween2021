using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpookShadow : Character
{
    NavMeshAgent navAgent;

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if(!navAgent) return;

        navAgent.updateRotation = false;
    }

    private void Update()
    {
        EFaceDirection newFaceDir = SpookyUtilities.VectorToFaceDirection(navAgent.velocity);
        currentFaceDirection = newFaceDir == EFaceDirection.None ? currentFaceDirection : newFaceDir;
    }
}
