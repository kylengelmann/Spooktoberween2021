using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleArea : MonoBehaviour
{
    public static VisibleArea visibleArea {get; private set;}

    static readonly Collider[] overlappingColliders = new Collider[1];

    const int visibilityLM = 1 << 31;
    const int collisionLM = 1 << 6;
    public static bool IsInVisibleArea(in Vector2 position, in Vector2 boxHalfSize)
    {
        float boundsCenterHeight = visibleAreaCollider.bounds.center.y;
        Vector3 pos3D = new Vector3(position.x, boundsCenterHeight, position.y) ;
        
        // Check if we're in the visible area
        if(Physics.OverlapBoxNonAlloc(pos3D, new Vector3(boxHalfSize.x, visibleAreaCollider.bounds.extents.y, boxHalfSize.y), overlappingColliders, Quaternion.identity, visibilityLM) > 0)
        {
            // Check if there's anything obscuring view from the player
            return !IsObscured(position, boxHalfSize);
        }

        return false;
    }

    static bool IsObscured(in Vector2 position, in Vector2 boxHalfSize)
    {
        float boundsCenterHeight = visibleAreaCollider.bounds.center.y;

        // Check if there's anything obscuring view from the player
        Vector2 fromPlayerVec = position - new Vector2(visibleArea.transform.position.x, visibleArea.transform.position.z);
        Vector2 perpVec = new Vector2(-fromPlayerVec.y, fromPlayerVec.x);

        float minCornerDot = float.PositiveInfinity;
        float maxCornerDot = float.NegativeInfinity;
        int minCorner = -1;
        int maxCorner = -1;
        Vector2 toMinCorner = Vector2.zero;
        Vector2 toMaxCorner = Vector2.zero;

        const int NUM_CORNERS = 4;
        Vector2[] corners = { new Vector2(-boxHalfSize.x, -boxHalfSize.y), new Vector2(boxHalfSize.x, -boxHalfSize.y), new Vector2(-boxHalfSize.x, boxHalfSize.y), new Vector2(boxHalfSize.x, boxHalfSize.y) };

        {
            Vector2 cornerDir = (corners[0] + fromPlayerVec).normalized;
            float dot = Vector2.Dot(cornerDir, perpVec);

            minCornerDot = dot;
            minCorner = 0;
            toMinCorner = cornerDir;

            maxCornerDot = dot;
            maxCorner = 0;
            toMaxCorner = cornerDir;
        }

        for (int i = 1; i < NUM_CORNERS; ++i)
        {
            Vector2 cornerDir = (corners[i] + fromPlayerVec).normalized;
            float dot = Vector2.Dot(cornerDir, perpVec);

            if (dot < minCornerDot)
            {
                minCornerDot = dot;
                minCorner = i;
                toMinCorner = cornerDir;
            }
            else if (dot > maxCornerDot)
            {
                maxCornerDot = dot;
                maxCorner = i;
                toMaxCorner = cornerDir;
            }
        }

        if (minCorner >= 0 && maxCorner >= 0)
        {
            Vector3 playerPos = new Vector3(visibleArea.transform.position.x, boundsCenterHeight, visibleArea.transform.position.z);

            float minCornerDist = Vector2.Dot(toMinCorner, corners[minCorner] + fromPlayerVec);
            bool bMinHit = Physics.Raycast(playerPos, new Vector3(toMinCorner.x, 0f, toMinCorner.y), minCornerDist, collisionLM, QueryTriggerInteraction.Collide);

            float maxCornerDist = Vector2.Dot(toMaxCorner, corners[maxCorner] + fromPlayerVec);
            bool bMaxHit = Physics.Raycast(playerPos, new Vector3(toMaxCorner.x, 0f, toMaxCorner.y), maxCornerDist, collisionLM, QueryTriggerInteraction.Ignore);

            Debug.DrawLine(playerPos, playerPos + new Vector3(toMinCorner.x, 0f, toMinCorner.y) * minCornerDist, bMinHit ? Color.red : Color.cyan);
            Debug.DrawLine(playerPos, playerPos + new Vector3(toMaxCorner.x, 0f, toMaxCorner.y) * maxCornerDist, bMaxHit ? Color.red : Color.cyan);

            return bMaxHit && bMinHit;
        }

        return false;
    }

    static Collider visibleAreaCollider;

    HashSet<SpookyThing> inAreaThings = new HashSet<SpookyThing>();

    private void Awake()
    {
        visibleArea = this;
        visibleAreaCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        SpookyThing thing = other.transform.GetComponent<SpookyThing>();
        if (thing)
        {
            inAreaThings.Add(thing);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SpookyThing thing = other.transform.GetComponent<SpookyThing>();
        if (thing)
        {
            inAreaThings.Remove(thing);
            thing.SetInVisibleArea(false);
        }
    }

    private void Update()
    {
        foreach(SpookyThing thing in inAreaThings)
        {
            Vector3 thingExtents = thing.spriteRenderer.bounds.extents;
            thing.SetInVisibleArea(!IsObscured(new Vector2(thing.transform.position.x, thing.transform.position.z), new Vector2(thingExtents.x, thingExtents.z)));
        }
    }
}
