using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpookPossessComponent : MonoBehaviour
{
    public SpookyThing thingPossessing {get; private set;}
    Coroutine PassiveUpdateRoutine;

    bool bIsTeleporting = false;
    Coroutine teleportCoroutine;

    float timeLeftUntilHunt = 0f;

    bool bIsHunting = false;
    Coroutine huntCoroutine;
    float timeLeftToAddToHunt;

    float timeLeftInHunt;

    SpookManager spookManager;
    SpookManager.HuntData huntData;
    SpookManager.TeleportData teleportData;

    const int MAX_TELEPORT_LOCATION_SEARCH_ATTEMPTS = 15;
    const float SpawnLocationSearchRadius = 1f;

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

        timeLeftUntilHunt = teleportData.huntStartTimerLength.Get();

        PassiveUpdateRoutine = StartCoroutine(PassiveUpdate());
    }

    private void Update()
    {
        if(TimeFocused <= 0f)
        {
            return;
        }

        if(huntCoroutine != null && !Mathf.Approximately(timeLeftToAddToHunt, 0f))
        {
            float timeToAddToHunt = Mathf.Min(timeLeftToAddToHunt, Time.deltaTime);
            timeLeftToAddToHunt -= timeToAddToHunt;
            timeLeftInHunt += timeLeftToAddToHunt;
        }


        float TimeSinceFocused = Time.timeSinceLevelLoad - TimeFocused;
        if(TimeSinceFocused > PossessionDisplayDelay)
        {
            thingPossessing.SetDisplayPossession(true);
        }

        if(TimeSinceFocused > TimeUnpossess)
        {
            spookManager.SwitchObjectPossessing(this);
        }
    }

    IEnumerator PassiveUpdate()
    {
        const float updateWaitTime = .1f;
        WaitForSeconds wait = new WaitForSeconds(.1f);
        bool bFirstTeleport = true;

        while(true)
        {
            yield return wait;

            if (teleportCoroutine == null)
            {
                if(bFirstTeleport)
                {
                    bFirstTeleport = false;
                }
                else if(Random.value < teleportData.switchObjectChance)
                {
                    spookManager.SwitchObjectPossessing(this);
                    break;
                }

                teleportCoroutine = StartCoroutine(TeleportUpdate(teleportData.teleportCooldown.Get()));
            }

            if (SpookyGameManager.gameManager.player)
            {
                float distanceSqrFromPlayer = (SpookyGameManager.gameManager.player.transform.position - transform.position).sqrMagnitude;
                if (distanceSqrFromPlayer < teleportData.huntStartPlayerRadius * teleportData.huntStartPlayerRadius && CanSeePlayer())
                {
                    if(timeLeftUntilHunt <= 0f)
                    {
                        Debug.Log("Ima getchu");

                        CancelTeleport();

                        huntCoroutine = StartCoroutine(HuntUpdate());
                        break;
                    }
                    timeLeftUntilHunt -= updateWaitTime;
                }
            }
        }
    }

    IEnumerator TeleportUpdate(float minTimeUntilTeleport)
    {
        yield return new WaitForSeconds(minTimeUntilTeleport);

        if(thingPossessing.bIsVisible)
        {
            yield return new WaitForVisibilityChange(thingPossessing);
        }

        Vector3 teleportLocation;
        SpookyThingSpawnLocation spawnLocation;
        if (GetTeleportLocationInCircle(transform.position, teleportData.teleportRadius, out teleportLocation, out spawnLocation))
        {
            transform.position = teleportLocation;
            timeLeftUntilHunt = teleportData.huntStartTimerLength.Get();
            thingPossessing.NotifyTeleport();

            if(spawnLocation)
            {
                spawnLocation.OnThingSpawned(thingPossessing);
            }
        }
        else
        {
            Debug.LogFormat("{0} failed to teleport. thing location: {1}", gameObject.name, transform.position);
        }

        teleportCoroutine = null;
    }

    IEnumerator HuntUpdate()
    {
        spookManager.AddHunt(this);

        timeLeftInHunt = huntData.huntDuration.Get();

        float timeUntilNextTeleport = huntData.huntTeleportCooldown.Get();

        while(timeLeftInHunt > 0f)
        {
            const float updateTime = .1f;
            WaitForSeconds smallWait = new WaitForSeconds(updateTime);

            yield return smallWait;

            if(timeUntilNextTeleport <= 0f)
            {
                if (!thingPossessing.bIsVisible)
                {
                    TeleportCheck canSeePlayer = (in Vector3 teleportLocation, GameObject teleportingObject) =>
                    {
                        Bounds bounds = thingPossessing.spriteRenderer.bounds;
                        Vector3 boundsOffset = bounds.center - transform.position;
                        Vector3 newBoundsLocation = teleportLocation + boundsOffset;
                        Vector3 boundsSize = thingPossessing.spriteRenderer.bounds.extents;
                        return !VisibleArea.IsObscured(new Vector2(newBoundsLocation.x, newBoundsLocation.z), new Vector2(boundsSize.x, boundsSize.z));
                    };

                    Vector3 teleportLocation;
                    SpookyThingSpawnLocation spawnLocation;
                    if (GetTeleportLocationInCircle(SpookyGameManager.gameManager.player.transform.position, huntData.huntPlayerRadius, out teleportLocation, out spawnLocation, new TeleportCheck[] { canSeePlayer }))
                    {
                        transform.position = teleportLocation;
                        spookManager.ProgressHunt();
                        timeUntilNextTeleport = huntData.huntTeleportCooldown.Get();
                        thingPossessing.NotifyTeleport();

                        if(spawnLocation)
                        {
                            spawnLocation.OnThingSpawned(thingPossessing);
                        }
                    }
                    else
                    {
                        Debug.LogFormat("{0} failed to find teleport location around player. Player position: {1}", gameObject.name, SpookyGameManager.gameManager.player.transform.position);
                        yield return smallWait;
                        continue;
                    }
                }
            }
            else
            {
                timeUntilNextTeleport -= updateTime;
            }

            timeLeftInHunt -= updateTime;
        }

        Debug.Log("BOO!");

        spookManager.RemoveHunt(this, true);

        Destroy(this);
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

    bool GetTeleportLocationInCircle(Vector3 center, float radius, out Vector3 teleportLocation, out SpookyThingSpawnLocation spawnLocation, TeleportCheck[] additionalChecks = null)
    {
        spawnLocation = null;
        bool bLocationFound = false;
        int numAttempts = 0;
        teleportLocation = Vector3.zero;
        Collider[] OverlappedSpawners = new Collider[16];
        HashSet<Collider> CheckedColliders = new HashSet<Collider>();
        while (!bLocationFound && numAttempts < MAX_TELEPORT_LOCATION_SEARCH_ATTEMPTS)
        {
            Vector2 random2DTeleportLocation = Random.insideUnitCircle * radius;
            Vector3 randomTeleportLocation = center + new Vector3(random2DTeleportLocation.x, 0f, random2DTeleportLocation.y);
            randomTeleportLocation.y = SpookyCollider.CollisionYValue;
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(randomTeleportLocation, out navMeshHit, .5f, NavMesh.AllAreas))
            {
                bool CheckLocation(Vector3 location)
                {
                    Bounds bounds = thingPossessing.spriteRenderer.bounds;
                    Vector3 boundsOffset = bounds.center - gameObject.transform.position;
                    Vector3 newBoundsLocation = navMeshHit.position + boundsOffset;
                    Vector3 boundsSize = thingPossessing.spriteRenderer.bounds.extents;
                    if (!VisibleArea.IsInVisibleArea(new Vector2(newBoundsLocation.x, newBoundsLocation.z), new Vector2(boundsSize.x, boundsSize.z)))
                    {
                        if (additionalChecks != null)
                        {
                            foreach (TeleportCheck additionalCheck in additionalChecks)
                            {
                                if (!additionalCheck(navMeshHit.position, gameObject))
                                {
                                    return false;
                                }
                            }
                        }

                        return true;
                    }

                    return false;
                }

                int NumOverlaps = Physics.OverlapSphereNonAlloc(navMeshHit.position, SpawnLocationSearchRadius, OverlappedSpawners);
                for(int i = 0; i < NumOverlaps; ++i)
                {
                    if(CheckedColliders.Contains(OverlappedSpawners[i]))
                    {
                        continue;
                    }

                    spawnLocation = OverlappedSpawners[i].gameObject.GetComponent<SpookyThingSpawnLocation>();
                    if(spawnLocation && spawnLocation.CanThingSpawn(thingPossessing))
                    {
                        if(CheckLocation(spawnLocation.transform.position))
                        {
                            bLocationFound = true;
                            teleportLocation = spawnLocation.transform.position;
                            break;
                        }
                    }

                    CheckedColliders.Add(OverlappedSpawners[i]);
                }

                if(bLocationFound)
                {
                    break;
                }

                spawnLocation = null;

                if(CheckLocation(navMeshHit.position))
                {
                    bLocationFound = true;
                    teleportLocation = navMeshHit.position;
                }
                else
                {
                    DrawDebugTeleportData(randomTeleportLocation, Color.cyan);
                }
            }
            else
            {
                DrawDebugTeleportData(randomTeleportLocation, Color.red);
            }

            ++numAttempts;
        }

        return bLocationFound;
    }

    void CancelTeleport()
    {
        if(teleportCoroutine == null)
        {
            return;
        }

        StopCoroutine(teleportCoroutine);
    }

    [System.Diagnostics.Conditional("USING_CHEAT_SYSTEM")]
    void DrawDebugTeleportData(Vector3 location, Color color)
    {
        if (spookManager.IsDisplayingDebugTeleportData())
        {
            Debug.DrawRay(location, Vector3.forward*.2f, color, 10f);
        }
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
