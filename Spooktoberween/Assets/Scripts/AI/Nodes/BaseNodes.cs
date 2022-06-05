using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviorNode
{
    public bool bHasBeenInitialized { get; private set; }

    public enum ENodeStatus
    {
        NotRunning,
        Running,
        Success,
    }

    public ENodeStatus CurrentStatus { get; private set; }

    public BehaviorNode ParentNode { get; private set; }
    public BehaviorTree OwningTree { get; private set; }

    public virtual void Init(BehaviorNode Parent, BehaviorTree Tree)
    {
        ParentNode = Parent;
        OwningTree = Tree;

        bHasBeenInitialized = true;
    }

    public ENodeStatus Update(float DeltaSeconds)
    {
        ENodeStatus result = ENodeStatus.NotRunning;
        if (CurrentStatus == ENodeStatus.NotRunning)
        {
            result = Start();
            if (result != ENodeStatus.Running)
            {
                return result;
            }
            else
            {
                CurrentStatus = ENodeStatus.Running;
            }
        }

        if (CurrentStatus == ENodeStatus.Running)
        {
            result = Tick(DeltaSeconds);

            if (result != ENodeStatus.Running)
            {
                End(result == ENodeStatus.Success);
                CurrentStatus = ENodeStatus.NotRunning;
            }
        }

        return result;
    }

    public void Cancel()
    {
        if (CurrentStatus == ENodeStatus.Running)
        {
            End(false);
            CurrentStatus = ENodeStatus.NotRunning;
        }
    }

    protected virtual ENodeStatus Start()
    {
        return ENodeStatus.Success;
    }

    protected virtual ENodeStatus Tick(float DeltaSeconds)
    {
        return ENodeStatus.Success;
    }

    protected virtual void End(bool bDidSucceed) { }

    protected bool TryGetProperty<T>(string PropName, out BehaviorProperty<T> Prop)
    {
        BehaviorPropertyBase BaseProp;
        bool bFound = TryGetProperty(PropName, out BaseProp);
        if(bFound && BaseProp is BehaviorProperty<T>)
        {
            Prop = (BehaviorProperty<T>)BaseProp;
            return true;
        }

        Prop = null;
        return false;
    }

    protected bool TryGetProperty(string PropName, out BehaviorPropertyBase Prop)
    {
        if (OwningTree == null)
        {
            Debug.LogError("Owning Tree is null");
            Prop = null;
            return false;
        }

        return OwningTree.TryGetProperty(PropName, out Prop);
    }

    protected bool HasProperty(string PropName)
    {
        if (OwningTree == null)
        {
            Debug.LogError("Owning Tree is null");
            return false;
        }

        return OwningTree.HasProperty(PropName);
    }

    protected bool AddProperty(BehaviorPropertyBase Prop)
    {
        if (OwningTree == null)
        {
            Debug.LogError("Owning Tree is null");
            return false;
        }

        return OwningTree.AddProperty(Prop);
    }

    protected bool GetOrAddProperty<T>(string PropName, out BehaviorProperty<T> Prop, T DefaultValue)
    {
        if(!TryGetProperty(PropName, out Prop))
        {
            Prop = new BehaviorProperty<T>(PropName, DefaultValue);
            if(!AddProperty(Prop))
            {
                Prop = null;
                return false;
            }
        }

        return true;
    }

    public AIController GetOwningController()
    {
        if(OwningTree != null)
        {
            return OwningTree.OwningController;
        }

        return null;
    }
}

public abstract class CompositeNode : BehaviorNode
{
    protected List<BehaviorNode> Children;
    public void AddChild(BehaviorNode Child)
    {
        if(Child == null)
        {
            return;
        }

        Children.Add(Child);

        if(bHasBeenInitialized && !Child.bHasBeenInitialized)
        {
            Child.Init(ParentNode, OwningTree);
        }
    }

    public void RemoveChild(BehaviorNode Child)
    {
        Children.Remove(Child);
    }

    public override void Init(BehaviorNode Parent, BehaviorTree Tree)
    {
        if(bHasBeenInitialized) return;

        base.Init(Parent, Tree);

        foreach (BehaviorNode Child in Children)
        {
            Child.Init(Parent, Tree);
        }
    }

    protected override ENodeStatus Start()
    {
        return ENodeStatus.Running;
    }

    protected override void End(bool bDidSucceed)
    {
        foreach (BehaviorNode Child in Children)
        {
            Child.Cancel();
        }
    }
}

public abstract class DecoratorNode : BehaviorNode
{
    protected BehaviorNode Child;

    public void SetChild(BehaviorNode child)
    {
        if (child == null)
        {
            return;
        }

        Child = child;

        if (bHasBeenInitialized && !Child.bHasBeenInitialized)
        {
            Child.Init(ParentNode, OwningTree);
        }
    }

    public override void Init(BehaviorNode Parent, BehaviorTree Tree)
    {
        if(bHasBeenInitialized) return;

        base.Init(Parent, Tree);

        if(Child == null)
        {
            Debug.Log("No child");
            return;
        }

        Child.Init(Parent, Tree);
    }

    protected override ENodeStatus Start()
    {
        return ENodeStatus.Running;
    }

    protected override ENodeStatus Tick(float DeltaSeconds)
    {
        return Child.Update(DeltaSeconds);
    }

    protected override void End(bool bDidSucceed)
    {
        Child.Cancel();
    }
}

public abstract class ServiceNode : DecoratorNode
{
    public float ExecuteInterval = .1f;

    float TimeSinceLastExecution;

    protected override ENodeStatus Start()
    {
        Execute();
        TimeSinceLastExecution = 0f;
        return base.Start();
    }

    protected override ENodeStatus Tick(float DeltaSeconds)
    {
        TimeSinceLastExecution += DeltaSeconds;
        if(TimeSinceLastExecution > ExecuteInterval)
        {
            TimeSinceLastExecution -= ExecuteInterval;
            Execute();
        }

        return base.Tick(DeltaSeconds);
    }

    protected abstract void Execute();
}