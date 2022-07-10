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

    float TimeFocused = -1f;
    public float PossessionDisplayDelay = 3f;
    public float TimeUnpossess = 5f;

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

        Debug.Log("Mine");

        thingPossessing.onFocusedChanged += OnFocusChanged;
        if (thingPossessing.bIsFocused)
        {
            OnFocusChanged(true);
        }

        teleportData = spookManager.GetTeleportData();
        huntData = spookManager.GetHuntData();

        PassiveUpdateRoutine = StartCoroutine(PassiveUpdate());
    }

    private void Update()
    {
        if(TimeFocused <= 0f)
        {
            return;
        }

        float TimeSinceFocused = Time.timeSinceLevelLoad - TimeFocused;
        if(TimeSinceFocused > PossessionDisplayDelay)
        {
            thingPossessing.SetDisplayPossession(true);
        }

        if(TimeSinceFocused > TimeUnpossess)
        {
            spookManager.OnUnpossess(this);
            Destroy(this);
        }
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
                if (distanceSqrFromPlayer < huntRadius * huntRadius && Random.value < spookManager.GetHuntProbability() && CanSeePlayer())
                {
                    Debug.Log(gameObject.name + ": ima gechu~~");
                    yield return huntCoroutine = StartCoroutine(HuntUpdate());
                    huntCoroutine = null;

                    break;
                }
            }

            float rand = Random.value;

            if (rand < teleportData.teleportProbability)
            {
                Debug.Log(gameObject.name + ": blink!");
                yield return teleportCoroutine = StartCoroutine(TeleportUpdate());
                teleportCoroutine = null;

                continue;
            }
            else if((rand - teleportData.teleportProbability) < teleportData.unpossessProbability)
            {
                spookManager.OnUnpossess(this);
                break;
            }
        }

        Destroy(this);
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
        else Debug.LogFormat("{0} failed to teleport. thing location: {1}", gameObject.name, transform.position);

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

                TeleportCheck canSeePlayer = (in Vector3 teleportLocation, GameObject teleportingObject) =>
                {
                    if(!thingPossessing) return false;

                    Bounds bounds = thingPossessing.spriteRenderer.bounds;
                    Vector3 boundsOffset = bounds.center - gameObject.transform.position;
                    Vector3 newBoundsLocation = teleportLocation + boundsOffset;
                    Vector3 boundsSize = thingPossessing.spriteRenderer.bounds.extents;
                    return !VisibleArea.IsObscured( new Vector2(newBoundsLocation.x, newBoundsLocation.z), new Vector2(boundsSize.x, boundsSize.z));
                };

                Vector3 teleportLocation;
                if(!GetTeleportLocationInCircle(SpookyGameManager.gameManager.player.transform.position, huntData.huntRadius, out teleportLocation, new TeleportCheck[]{ canSeePlayer }))
                {
                    Debug.LogFormat("{0} failed to find teleport location around player. Player position: {1}", gameObject.name, SpookyGameManager.gameManager.player.transform.position);
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
        spookManager.RemoveHunt(this, true);
    }

    bool CanSeePlayer()
    {
        Bounds bounds = thingPossessing.spriteRenderer.bounds;
        Vector3 boundsOffset = bounds.center - transform.position;
        Vector3 newBoundsLocation = transform.position + boundsOffset;
        Vector3 boundsSize = thingPossessing.spriteRenderer.bounds.extents;
        return !VisibleArea.IsObscured(new Vector2(newBoundsLocation.x, newBoundsLocation.z), new Vector2(boundsSize.x, boundsSize.z));
    }

    delegate bool TeleportCheck(in Vector3 teleportLocation, GameObject teleportingObject);

    bool GetTeleportLocationInCircle(Vector3 center, float radius, out Vector3 teleportLocation, TeleportCheck[] additionalChecks = null)
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
                    if (additionalChecks != null)
                    {
                        foreach(TeleportCheck additionalCheck in additionalChecks)
                        {
                            if(!additionalCheck(navMeshHit.position, gameObject))
                            {
                                bLocationFound = false;
                                break;
                            }
                        }

                        if(!bLocationFound) continue;
                    }

                    bLocationFound = true;
                    teleportLocation = navMeshHit.position;
                }
            }

            ++numAttempts;
        }

        return bLocationFound;
    }

    void OnFocusChanged(bool newFocus)
    {
        TimeFocused = newFocus ? Time.timeSinceLevelLoad : -1f;

        if(!newFocus)
        {
            thingPossessing.SetDisplayPossession(false);
        }
    }

    private void OnDestroy()
    {
        if(thingPossessing)
        {
            thingPossessing.SetDisplayPossession(false);
        }
    }
}
