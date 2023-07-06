using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FindPlayerSearchLocationService : ServiceNode
{
    public BehaviorProperty<Vector3> lastPlayerPosProp;
    public BehaviorProperty<Vector3> lastPlayerDirProp;
    public BehaviorProperty<Vector3> currentSearchLocationProp;
    public BehaviorProperty<Vector3> searchCenterProp;

    public bool bHasFinished {get; protected set;}
    bool bHasChildFinished;
    ENodeStatus childStatus = ENodeStatus.NotRunning;

    int numLocationsChecked;
    public int numLocationsToCheck = 30;
    public float MaxSearchDist = 2;
    public float MaxPathDist = 4;

    public float PathDistanceScoreMult = .33f;
    public float PathDirScoreMult = 1f;

    public float MinScorePercentage = .8f;

    float bestScore;
    float worstScore;

    NavMeshAgent Agent;
    struct SearchLocationData
    {
        public Vector3 location;
        public float score;

        public SearchLocationData(Vector3 searchLocation, float locationScore)
        {
            location = searchLocation;
            score = locationScore;
        }
    }

    List<SearchLocationData> searchLocations = new List<SearchLocationData>();

    Vector3 searchCenter;

    float initialExecuteInterval;

    protected override ENodeStatus Start()
    {
        initialExecuteInterval = ExecuteInterval;

        numLocationsChecked = 0;
        bHasFinished = false;
        bHasChildFinished = false;
        searchCenter = searchCenterProp == null ? currentSearchLocationProp.Value : searchCenterProp.Value;

        bestScore = float.NegativeInfinity;
        worstScore = float.PositiveInfinity;

        searchLocations.Clear();

        AIController Controller = GetOwningController();
        if (Controller)
        {
            Character character = Controller.ControlledCharacter;
            if (character)
            {
                Agent = character.GetComponent<NavMeshAgent>();
            }
        }

        return base.Start();
    }

    protected override ENodeStatus Tick(float DeltaSeconds)
    {
        foreach (SearchLocationData data in searchLocations)
        {
            Debug.DrawRay(data.location, Vector3.forward, Color.HSVToRGB((1f - Mathf.Clamp01((data.score - worstScore) / (bestScore - worstScore + 1e-4f))) * .5f, 1, 1));
        }

        if (!bHasFinished)
        {
            Execute();
        }

        if (!bHasChildFinished)
        {
            childStatus = Child.Update(DeltaSeconds);
            if(childStatus != ENodeStatus.Running)
            {
                bHasChildFinished = true;
            }
        }

        return (bHasFinished && bHasChildFinished) ? childStatus : ENodeStatus.Running;
    }

    protected override void Execute()
    {
        if(bHasFinished || !Agent)
        {
            return;
        }

        if (numLocationsChecked >= numLocationsToCheck)
        {
            bHasFinished = true;
            ExecuteInterval = -1f;

            if (searchLocations.Count <= 0)
            {
                return;
            }

            Vector3 bestLocation = searchLocations[0].location;
            if(searchLocations.Count > 1)
            {
                float scoreRange = bestScore - worstScore;
                float minScoreToChoose = scoreRange * MinScorePercentage + worstScore;

                List<SearchLocationData> choices = searchLocations.FindAll((SearchLocationData data) => {return data.score >= minScoreToChoose;});

                Debug.Assert(choices.Count > 0);

                if(choices.Count > 0)
                {
                    bestLocation = choices[Random.Range(0, choices.Count)].location;
                }
            }

            currentSearchLocationProp.Value = bestLocation;

            return;
        }

        ++numLocationsChecked;

        Vector2 randLocation = Random.insideUnitCircle * MaxSearchDist;

        Vector3 nextLocation = new Vector3(randLocation.x + searchCenter.x, SpookyCollider.CollisionYValue, randLocation.y + searchCenter.z);
        
        NavMeshHit hit;
        if(!NavMesh.SamplePosition(nextLocation, out hit, .5f, NavMesh.AllAreas))
        {
            return;
        }

        NavMeshPath path = new NavMeshPath();
        if(!NavMesh.CalculatePath(lastPlayerPosProp.Value, nextLocation, NavMesh.AllAreas, path))
        {
            return;
        }

        if(!(path.corners.Length > 1))
        {
            return;
        }

        Vector3 pathStartDir = Vector3.ProjectOnPlane(path.corners[1] - path.corners[0], Vector3.up);
        pathStartDir.Normalize();

        path = new NavMeshPath();
        if (!NavMesh.CalculatePath(Agent.transform.position, nextLocation, NavMesh.AllAreas, path))
        {
            return;
        }

        float pathDist = pathStartDir.magnitude;

        for (int i = 1; i < path.corners.Length; ++i)
        {
            pathDist += Vector3.ProjectOnPlane(path.corners[i] - path.corners[i - 1], Vector3.up).magnitude;
        }

        if(pathDist > MaxPathDist)
        {
            return;
        }

        float score = Mathf.Clamp01(Vector3.Dot(pathStartDir, lastPlayerDirProp.Value)) * PathDirScoreMult + pathDist * PathDirScoreMult;

        if(score > bestScore)
        {
            bestScore = score;
        }

        if(score < worstScore)
        {
            worstScore = score;
        }

        searchLocations.Add(new SearchLocationData(nextLocation, score));
    }

    protected override void End(bool bDidSucceed)
    {
        ExecuteInterval = initialExecuteInterval;
        base.End(bDidSucceed);
    }
}
