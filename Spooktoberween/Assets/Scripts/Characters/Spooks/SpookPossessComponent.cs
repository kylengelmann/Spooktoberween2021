using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpookPossessComponent : MonoBehaviour
{
    SpookyThing thingPossessing;
    Coroutine PassiveUpdateRoutine;

    bool bIsTeleporting = false;
    Coroutine teleportCoroutine;

    bool bIsHunting = false;
    Coroutine huntCoroutine;

    SpookManager spookManager;
    SpookManager.HuntData huntData;
    SpookManager.TeleportData teleportData;

    const int MAX_TELEPORT_LOCATION_SEARCH_ATTEMPTS = 15;

    private void Start()
    {
        spookManager = SpookManager.spookManager;
        if(!spookManager)
        {
            Destroy(this);
            return;
        }

        thingPossessing = GetComponent<SpookyThing>();
        if(!thingPossessing)
        {
            Destroy(this);
            return;
        }

        teleportData = spookManager.GetTeleportData();
        huntData = spookManager.GetHuntData();

        PassiveUpdateRoutine = StartCoroutine(PassiveUpdate());
    }

    IEnumerator PassiveUpdate()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        while(true)
        {
            yield return wait;

            if (SpookyGameManager.gameManager.player)
            {
                float distanceSqrFromPlayer = (SpookyGameManager.gameManager.player.transform.position - transform.position).sqrMagnitude;
                float huntRadius = huntData.huntRadius;
                if(distanceSqrFromPlayer < huntRadius*huntRadius && Random.value < spookManager.GetHuntProbability())
                {
                    Debug.Log(gameObject.name + ": ima gechu~~");
                    yield return huntCoroutine = StartCoroutine(HuntUpdate());
                    huntCoroutine = null;

                    continue;
                }
            }
            
            if(Random.value < teleportData.teleportProbability)
            {
                Debug.Log(gameObject.name + ": blink!");
                yield return teleportCoroutine = StartCoroutine(TeleportUpdate());
                teleportCoroutine = null;

                continue;
            }
        }
    }

    IEnumerator TeleportUpdate()
    {
        bIsTeleporting = true;

        if(thingPossessing.bIsVisible)
        {
            yield return new WaitForVisibilityChange(thingPossessing);
        }

        Vector3 teleportLocation;
        if (GetTeleportLocationInCircle(transform.position, teleportData.teleportRadius, out teleportLocation)) transform.position = teleportLocation;
        else Debug.LogFormat("%s failed to teleport. thing location: %s", gameObject.name, transform.position);

        bIsTeleporting = false;
    }

    IEnumerator HuntUpdate()
    {
        spookManager.AddHunt(this);
        bIsHunting = true;
        int numTeleportsLeft = huntData.huntTeleports;
        while(numTeleportsLeft > 0 & bIsHunting)
        {
            float currentTeleportTime = Random.Range(huntData.minHuntTeleportTime, huntData.maxHuntTeleportTime);
            yield return new WaitForSeconds(currentTeleportTime);

            bool bTeleported = false;
            WaitForSeconds smallWait = new WaitForSeconds(.1f);
            while(!bTeleported)
            {
                if (thingPossessing.bIsVisible)
                {
                    yield return new WaitForVisibilityChange(thingPossessing);
                }

                Vector3 teleportLocation;
                if(!GetTeleportLocationInCircle(SpookyGameManager.gameManager.player.transform.position, huntData.huntRadius, out teleportLocation))
                {
                    Debug.LogFormat("%s failed to find teleport location around player. Player position: %s", gameObject.name, SpookyGameManager.gameManager.player.transform.position);
                    yield return smallWait;
                    continue;
                }

                bTeleported = true;
                transform.position = teleportLocation;
            }

            --numTeleportsLeft;

            spookManager.ProgressHunt();
        }

        float teleportTime = Random.Range(huntData.minHuntTeleportTime, huntData.maxHuntTeleportTime);
        yield return new WaitForSeconds(teleportTime);

        Debug.Log("BOO!");

        bIsHunting = false;
        spookManager.RemoveHunt(this);
    }

    bool GetTeleportLocationInCircle(Vector3 center, float radius, out Vector3 teleportLocation)
    {
        bool bLocationFound = false;
        int numAttempts = 0;
        teleportLocation = Vector3.zero;
        while (!bLocationFound && numAttempts < MAX_TELEPORT_LOCATION_SEARCH_ATTEMPTS)
        {
            Vector2 randomTeleportLocation = Random.insideUnitCircle * radius;
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(center + new Vector3(randomTeleportLocation.x, SpookyCollider.CollisionYValue, randomTeleportLocation.y), out navMeshHit, .5f, NavMesh.AllAreas))
            {
                Bounds bounds = thingPossessing.spriteRenderer.bounds;
                Vector3 boundsOffset = bounds.center - gameObject.transform.position;
                Vector3 newBoundsLocation = navMeshHit.position + boundsOffset;
                Vector3 boundsSize = thingPossessing.spriteRenderer.bounds.extents;
                if (!VisibleArea.IsInVisibleArea(new Vector2(newBoundsLocation.x, newBoundsLocation.z), new Vector2(boundsSize.x, boundsSize.z)))
                {
                    bLocationFound = true;
                    teleportLocation = navMeshHit.position;
                }
            }

            ++numAttempts;
        }

        return bLocationFound;
    }
}
