using UnityEngine;
using UnityEngine.AI;

public class MoveTo : BehaviorNode
{
    public BehaviorProperty<GameObject> GoalObjectProperty;
    public GameObject Goal;
    public BehaviorProperty<Vector3> GoalVectorProperty;
    public Vector3 GoalLocation;

    NavMeshAgent Agent;

    void UpdateGoal()
    {
        if(GoalObjectProperty != null)
        {
            Goal = GoalObjectProperty.Value;
        }
        if(Goal)
        {
            GoalLocation = Goal.transform.position;
            return;
        }
        if (GoalVectorProperty != null)
        {
            GoalLocation = GoalVectorProperty.Value;
        }
    }

    protected override ENodeStatus Start()
    {
        Agent = null;

        AIController Controller = GetOwningController();
        if(Controller)
        {
            Character character = Controller.ControlledCharacter;
            if (character)
            {
                Agent = character.GetComponent<NavMeshAgent>();
            }
        }

        if(!Agent)
        {
            return ENodeStatus.NotRunning;
        }

        UpdateGoal();

        Agent.SetDestination(GoalLocation);
        Agent.isStopped = false;

        return ENodeStatus.Running;
    }

    protected override ENodeStatus Tick(float DeltaSeconds)
    {
        if(GoalObjectProperty != null || Goal)
        {
            UpdateGoal();
        }

        Agent.isStopped = false;
        Agent.SetDestination(GoalLocation);

        Vector3 ToEnd = GoalLocation - Agent.transform.position;
        if(ToEnd.sqrMagnitude <= .05f)
        {
            return ENodeStatus.Success;
        }

        return ENodeStatus.Running;
    }

    protected override void End(bool bDidSucceed)
    {
        Agent.isStopped = true;
        Agent = null;
    }
}
