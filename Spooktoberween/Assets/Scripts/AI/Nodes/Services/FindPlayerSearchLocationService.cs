using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FindPlayerSearchLocationService : ServiceNode
{
    public BehaviorProperty<Vector3> lastPlayerPosProp;
    public BehaviorProperty<Vector3> lastPlayerDirProp;
    public BehaviorProperty<Vector3> currentSearchLocationProp;

    public bool bHasFoundLocation {get; protected set;}

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

    Vector3 lastSearchLocation;

    float initialExecuteInterval;

    protected override ENodeStatus Start()
    {
        initialExecuteInterval = ExecuteInterval;

        numLocationsChecked = 0;
        bHasFoundLocation = false;
        lastSearchLocation = currentSearchLocationProp.Value;

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

    protected override void Execute()
    {
        //foreach(SearchLocationData data in searchLocations)
        //{
        //    Debug.DrawRay(data.location, Vector3.forward, Color.HSVToRGB(Mathf.Clamp01((data.score - worstScore) /(bestScore - worstScore + 1e-4f))*.5f, 1, 1));
        //}

        if(bHasFoundLocation || !Agent)
        {
            return;
        }

        if (numLocationsChecked >= numLocationsToCheck)
        {
            bHasFoundLocation = true;
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

        Vector3 nextLocation = new Vector3(randLocation.x + lastSearchLocation.x, SpookyCollider.CollisionYValue, randLocation.y + lastSearchLocation.z);
        
        NavMeshHit hit;
        if(!NavMesh.SamplePosition(nextLocation, out hit, .5f, NavMesh.AllAreas))
        {
            return;
        }

        NavMeshPath path = new NavMeshPath();
        if(!NavMesh.CalculatePath(Agent.transform.position, nextLocation, NavMesh.AllAreas, path))
        {
            return;
        }

        if(!(path.corners.Length > 1))
        {
            return;
        }

        Vector3 pathStartDir = Vector3.ProjectOnPlane(path.corners[1] - path.corners[0], Vector3.up);
        float pathDist = pathStartDir.magnitude;
        pathStartDir.Normalize();

        for(int i = 2; i < path.corners.Length; ++i)
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
