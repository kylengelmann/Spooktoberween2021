using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpookShadow : Character
{
    float pathUpdateRatePerUnitDistance = .1f;
    float maxPathUpdateTime = 4f;

    NavMeshAgent navAgent;

    float chaseSuccessDistance = .5f;
    float maxSight = 5f;

    bool bChaseSucceeded = false;
    float timeToLosePlayer = .5f;

    bool bCanSeePlayer = false;

    Vector3 LastPlayerLocation;
    Vector3 LastPlayerMoveDirection;

    float MaxSearchDistance = 10f;
    int GoalNumValidSearchLocations = 15;
    int MaxTotalSearchLocationAttempts = 100;
    int MaxSearchLocationAttemptsPerFrame = 15;

    float playerDirDotWeight = 2.5f;
    float playerDistanceWeigth = 1f;

    struct PlayerSearchQueryData
    {
        public Coroutine queryCoroutine;
        public List<PlayerSearchLocationData> searchLocations;
        public bool bIsComplete;
        public bool bSucceeded;

        public bool IsRunning() {return queryCoroutine != null;}
        public bool IsComplete() {return bIsComplete;}

        public Vector3 GetNextSearchLocation()
        {
            return Vector3.zero;
        }
    }

    PlayerSearchQueryData currentSearchData;

    struct PlayerSearchLocationData
    {
        public Vector3 location;
        public float score;

        public void SetScore(float newScore)
        {
            score = newScore;
        }
    }

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if(!navAgent) return;

        navAgent.updateRotation = false;
        //StartCoroutine(BehaviorUpdate());
    }

    IEnumerator BehaviorUpdate()
    {
        WaitForSeconds wait = new WaitForSeconds(.2f);

        StartCoroutine(UpdateCanSeePlayer());

        while(true)
        {
            yield return wait;

            if(bCanSeePlayer)
            {
                yield return StartCoroutine(ChasePlayer());
            }
            else
            {

            }
        }
    }

    IEnumerator ChasePlayer()
    {
        bool bChaseSucceeded = false;
        GameObject player = SpookyGameManager.gameManager.player.gameObject;
        if(!player) yield break;

        navAgent.isStopped = false;

        WaitForSeconds wait = new WaitForSeconds(.2f);

        float LastPathUpdateTime = 0f;

        float distSquaredToTarget = 0f;
        float chaseSuccessDistSqr = chaseSuccessDistance * chaseSuccessDistance;

        float timeLostSight = -1f;

        while(!navAgent.isStopped && bCanSeePlayer)
        {
            if (!player) yield break;

            distSquaredToTarget = Vector3.ProjectOnPlane(navAgent.transform.position - player.transform.position, Vector3.up).sqrMagnitude;

            if(distSquaredToTarget <= chaseSuccessDistSqr)
            {
                bChaseSucceeded = true;
                break;
            }

            if(!bCanSeePlayer)
            {
                bChaseSucceeded = false;
                break;
            }

            if (Time.time >= LastPathUpdateTime + Mathf.Min(maxPathUpdateTime, Mathf.Sqrt(distSquaredToTarget) * pathUpdateRatePerUnitDistance))
            {
                navAgent.destination = player.transform.position;
                LastPathUpdateTime = Time.time;
            }

            yield return wait;

        }

        navAgent.isStopped = true;
    }

    IEnumerator SearchForPlayer()
    {
        WaitForSeconds wait = new WaitForSeconds(.2f);
        while (!navAgent.isStopped && !bCanSeePlayer)
        {
            yield return wait;
        }
    }

    IEnumerator PickSearchLocation()
    {
        for(int i = 0; i < MaxTotalSearchLocationAttempts && currentSearchData.searchLocations.Count < GoalNumValidSearchLocations; )
        {
            for(int j = 0; j < MaxSearchLocationAttemptsPerFrame && i < MaxTotalSearchLocationAttempts && currentSearchData.searchLocations.Count < GoalNumValidSearchLocations; ++j, ++i)
            {
                Vector2 nextLocation2D = Random.insideUnitCircle * MaxSearchDistance;
                Vector3 nextLocation3D = new Vector3(transform.position.x + nextLocation2D.x, transform.position.y, transform.position.z + nextLocation2D.y);
                NavMeshHit navMeshHit;
                if(NavMesh.SamplePosition(nextLocation3D, out navMeshHit, 10f, -1))
                {
                    PlayerSearchLocationData searchLocationData;
                    searchLocationData.location = navMeshHit.position;

                }
            }

            yield return null;
        }

        yield return null;

        for(int i = 0; i < currentSearchData.searchLocations.Count; ++i)
        {
            Vector3 location = currentSearchData.searchLocations[i].location;
            NavMeshPath path = new NavMeshPath();
            if(NavMesh.CalculatePath(LastPlayerLocation, location, -1, path))
            {
                if(path.corners.Length > 0)
                {
                    float pathLength = 0;
                    Vector3 pathStartDir = path.corners[1] - LastPlayerLocation;
                    float pathDotPlayer = Vector2.Dot(new Vector2(pathStartDir.x, pathStartDir.z), new Vector2(LastPlayerMoveDirection.x, LastPlayerMoveDirection.z));

                    for(int p = 0; p < path.corners.Length - 1; ++p)
                    {
                        pathLength = Vector3.ProjectOnPlane((path.corners[p + 1] - path.corners[p]), Vector3.up).magnitude;   
                    }

                    if(pathLength > MaxSearchDistance)
                    {
                        continue;   
                    }

                    float distScore = (pathLength / Mathf.Max(MaxSearchDistance, Mathf.Epsilon)) * playerDistanceWeigth;
                    float dotScore = Mathf.Max(pathDotPlayer, 0f) * playerDirDotWeight;

                    currentSearchData.searchLocations[i].SetScore(distScore + dotScore);
                }
            }

            yield return null;
        }

        currentSearchData.searchLocations.Sort(
            (PlayerSearchLocationData locationDataA, PlayerSearchLocationData locationDataB) =>
            { 
                return Mathf.RoundToInt(Mathf.Sign(locationDataA.score - locationDataB.score));
            });

        currentSearchData.bIsComplete = true;
    }

    IEnumerator MoveToLocation(Vector3 location, float tolerance)
    {
        navAgent.destination = location;
        navAgent.isStopped = false;

        WaitForSeconds wait = new WaitForSeconds(.3f);

        while(!navAgent.isStopped && Vector3.ProjectOnPlane(navAgent.transform.position - location, Vector3.up).sqrMagnitude > (tolerance * tolerance))
        {
            yield return wait;
        }

        navAgent.isStopped = true;
    }

    IEnumerator UpdateCanSeePlayer()
    {
        SpookyPlayer player = SpookyGameManager.gameManager.player;
        if (!player) yield break;

        WaitForSeconds wait = new WaitForSeconds(.2f);
        int visibilityLayerMask = 1 << 6;
        float timeLostSight = -1f;
        while (true)
        {
            if (!player) yield break;

            Vector3 visCheckOrigin = navAgent.transform.position;
            visCheckOrigin.y = 2f;

            RaycastHit hitInfo;

            Vector3 navToPlayer = Vector3.ProjectOnPlane(player.transform.position - navAgent.transform.position, Vector3.up);
            if (Physics.Raycast(visCheckOrigin, navToPlayer, out hitInfo, Mathf.Min(maxSight, navToPlayer.magnitude), visibilityLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (timeLostSight > 0f)
                {
                    if (Time.time - timeLostSight > timeToLosePlayer)
                    {
                        bCanSeePlayer = false;
                    }
                }
                else
                {
                    timeLostSight = Time.time;
                }
            }
            else
            {
                Debug.DrawLine(visCheckOrigin, visCheckOrigin + Vector3.ProjectOnPlane(player.transform.position - navAgent.transform.position, Vector3.up), Color.green, 5f);

                bCanSeePlayer = true;
                timeLostSight = -1f;
                LastPlayerLocation = player.transform.position;
                LastPlayerMoveDirection = player.GetVelocity().normalized;
            }

            yield return wait;
        }
    }
}
