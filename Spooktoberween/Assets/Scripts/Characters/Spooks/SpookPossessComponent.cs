using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpookPossessComponent : MonoBehaviour
{
    SpookyThing thingPossessing;
    Coroutine PassiveUpdateRoutine;
    float teleportChance = .2f;
    float teleportRadius = 1f;

    const int MAX_TELEPORT_LOCATION_SEARCH_ATTEMPTS = 15;

    private void Awake()
    {
        thingPossessing = GetComponent<SpookyThing>();
        thingPossessing.onVisibleChanged += OnVisibleChanged;
    }

    void OnVisibleChanged(bool bIsVisible)
    {
        if(!gameObject.activeInHierarchy) return;

        if(bIsVisible)
        {
            if (PassiveUpdateRoutine != null)
            {
                StopCoroutine(PassiveUpdateRoutine);
                PassiveUpdateRoutine = null;
            }
        }
        else
        {
            if (PassiveUpdateRoutine == null)
            {
                PassiveUpdateRoutine = StartCoroutine(PassiveUpdate());
            }
        }
    }

    IEnumerator PassiveUpdate()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        while(true)
        {
            if(Random.value < teleportChance)
            {
                bool bLocationFound = false;
                int numAttempts = 0;
                Vector3 teleportLocation = Vector3.zero;
                while(!bLocationFound && numAttempts < MAX_TELEPORT_LOCATION_SEARCH_ATTEMPTS)
                {
                    Vector2 randomTeleportLocation = Random.insideUnitCircle*teleportRadius;
                    NavMeshHit navMeshHit;
                    if(NavMesh.SamplePosition(new Vector3(randomTeleportLocation.x, SpookyCollider.CollisionYValue, randomTeleportLocation.y), out navMeshHit, .5f, NavMesh.AllAreas))
                    {
                        Bounds bounds = thingPossessing.spriteRenderer.bounds;
                        Vector3 boundsOffset = bounds.center - gameObject.transform.position;
                        Vector3 newBoundsLocation = navMeshHit.position + boundsOffset;
                        Vector3 boundsSize = thingPossessing.spriteRenderer.bounds.extents;
                        if(!VisibleArea.IsInVisibleArea(new Vector2(newBoundsLocation.x, newBoundsLocation.z), new Vector2(boundsSize.x, boundsSize.z)))
                        {
                            bLocationFound = true;
                            teleportLocation = navMeshHit.position;
                        }
                    }

                    ++numAttempts;
                }
                
                if(bLocationFound) transform.position = teleportLocation;
            }
            yield return wait;
        }
    }
}
