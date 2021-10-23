using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleArea : MonoBehaviour
{
    public static VisibleArea visibleArea {get; private set;}

    static readonly Collider[] overlappingColliders = new Collider[1];

    const int visibilityLM = 1 << 31;
    public static bool IsInVisibleArea(Vector2 position, Vector2 boxHalfSize)
    {
        Vector3 pos3D = new Vector3(position.x, visibleArea.collision.bounds.center.y, position.y) ;
        
        return Physics.OverlapBoxNonAlloc(pos3D, new Vector3(boxHalfSize.x, visibleArea.collision.bounds.extents.y, boxHalfSize.y), overlappingColliders, Quaternion.identity, visibilityLM) > 0;
    }

    Collider collision;

    private void Awake()
    {
        visibleArea = this;
        collision = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        SpookyThing thing = other.transform.GetComponent<SpookyThing>();
        if (thing)
        {
            thing.SetInVisibleArea(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SpookyThing thing = other.transform.GetComponent<SpookyThing>();
        if (thing)
        {
            thing.SetInVisibleArea(false);
        }
    }
}
