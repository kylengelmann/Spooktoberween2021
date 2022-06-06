using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIController : MonoBehaviour
{
    public Character ControlledCharacter {get; protected set;}
    public BehaviorTree Behavior {get; protected set;}

    public void Start()
    {
        Character attachedCharacter = GetComponent<Character>();
        if(attachedCharacter)
        {
            Possess(attachedCharacter);
        }
    }

    public void Possess(Character character)
    {
        ControlledCharacter = character;

        InitBehavior();
    }

    protected abstract void InitBehavior();

    public virtual void Update()
    {
        if(Behavior != null)
        {
            Behavior.Update(Time.deltaTime);
        }
    }
}
