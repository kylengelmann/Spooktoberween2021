using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTree
{
    public AIController OwningController {get; protected set;}

    BehaviorNode RootNode;

    Dictionary<string, BehaviorProperty> Properties;

    public void Init(AIController Controller, BehaviorNode Root, in List<BehaviorProperty> DefaultProperties)
    {
        OwningController = Controller;
        RootNode = Root;
        
        foreach (BehaviorProperty Prop in DefaultProperties)
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

    public bool TryGetProperty(string PropName, out BehaviorProperty Prop)
    {
        return Properties.TryGetValue(PropName, out Prop);
    }

    public bool HasProperty(string PropName)
    {
        return Properties.ContainsKey(PropName);
    }

    public bool AddProperty(BehaviorProperty Prop)
    {
        if(Properties.ContainsKey(Prop.Name))
        {
            return false;
        }

        Properties.Add(Prop.Name, Prop);
        return true;
    }
}

public class BehaviorProperty
{
    public string Name {get; private set;}
    
    public object Value;

    public BehaviorProperty(string name, object DefaultValue)
    {
        Name = name;
        Value = DefaultValue;
    }
}