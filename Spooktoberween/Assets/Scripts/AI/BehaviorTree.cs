using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTree
{
    public AIController OwningController {get; protected set;}

    BehaviorNode RootNode;

    Dictionary<string, BehaviorPropertyBase> Properties = new Dictionary<string, BehaviorPropertyBase>();

    public void Init(AIController Controller, BehaviorNode Root, in List<BehaviorPropertyBase> DefaultProperties)
    {
        OwningController = Controller;
        RootNode = Root;
        
        foreach (BehaviorPropertyBase Prop in DefaultProperties)
        {
            bool bSuccess = AddProperty(Prop);
            if(!bSuccess)
            {
                Debug.LogWarning("Default Property " + Prop.Name + " exists multiple times, only the first instance will be used");
            }
        }

        RootNode.Init(null, this);
    }

    public void Update(float DeltaSeconds)
    {
        if(RootNode != null)
        {
            RootNode.Update(DeltaSeconds);
        }
    }

    public bool TryGetProperty(string PropName, out BehaviorPropertyBase Prop)
    {
        return Properties.TryGetValue(PropName, out Prop);
    }

    public bool HasProperty(string PropName)
    {
        return Properties.ContainsKey(PropName);
    }

    public bool AddProperty(BehaviorPropertyBase Prop)
    {
        if(Properties.ContainsKey(Prop.Name))
        {
            return false;
        }

        Properties.Add(Prop.Name, Prop);
        return true;
    }
}

public abstract class BehaviorPropertyBase
{
    public string Name {get; private set;}

    protected BehaviorPropertyBase(string name)
    {
        Name = name;
    }
}

public class BehaviorProperty<T> : BehaviorPropertyBase
{
    public T Value;

    public BehaviorProperty(string name, T value) : base(name)
    {
        Value = value;
    }
}