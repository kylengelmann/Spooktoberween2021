using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectableType
{
    BearHead = 1,
    BearTorso = 2,
    BearRightArm = 3,
    BearLeftArm = 4,
    BearRightLeg = 5,
    BearLeftLeg = 6,

    BEAR_START = 1,
    BEAR_END = 6,
}

public class Collectable : MonoBehaviour
{

    [SerializeField]
    private CollectableType _type;
    public CollectableType type {get { return _type; } private set {_type = value; } }

    private void OnTriggerEnter(Collider other)
    {
        ICollectorInterface collector = other.GetComponentInParent<ICollectorInterface>();
        if(collector != null)
        {
            if(collector.Collect(this))
            {
                OnCollected();
            }
        }
    }

    protected virtual void OnCollected()
    {
        Destroy(gameObject);
    }

    static public bool IsBearPart(CollectableType type)
    {
        return type >= CollectableType.BEAR_START && type <= CollectableType.BEAR_END;
    }
}
