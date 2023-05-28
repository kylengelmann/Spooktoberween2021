using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleArea : MonoBehaviour
{
    public static VisibleArea visibleArea {get; private set;}

    static readonly Collider[] overlappingColliders = new Collider[1];

    const int visibilityLM = 1 << 31;
    const int collisionLM = 1 << 6;

    public static VisibilityLight visLight;

    public static bool IsInVisibleArea(in Vector2 position, in Vector2 boxHalfSize)
    {
        float boundsCenterHeight = visibleAreaCollider.bounds.center.y;
        Vector3 pos3D = new Vector3(position.x, boundsCenterHeight, position.y) ;
        
        // Check if we're in the visible area
        if(Physics.OverlapBoxNonAlloc(pos3D, new Vector3(boxHalfSize.x, visibleAreaCollider.bounds.extents.y, boxHalfSize.y), overlappingColliders, Quaternion.identity, visibilityLM) > 0)
        {
            // Check if there's anything obscuring view from the player
            return IsObjectInVisLight(position, boxHalfSize) && !IsObscured(position, boxHalfSize);
        }

        return false;
    }

    public static bool IsObscured(in Vector2 position, in Vector2 boxHalfSize)
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
            bool bMinHit = Physics.Raycast(playerPos, new Vector3(toMinCorner.x, 0f, toMinCorner.y), minCornerDist, collisionLM, QueryTriggerInteraction.Ignore);

            if(bMinHit)
            {    
                float maxCornerDist = Vector2.Dot(toMaxCorner, corners[maxCorner] + fromPlayerVec);
                bool bMaxHit = Physics.Raycast(playerPos, new Vector3(toMaxCorner.x, 0f, toMaxCorner.y), maxCornerDist, collisionLM, QueryTriggerInteraction.Ignore);

                //Debug.DrawLine(playerPos, playerPos + new Vector3(toMinCorner.x, 0f, toMinCorner.y) * minCornerDist, bMinHit ? Color.red : Color.cyan);
                //Debug.DrawLine(playerPos, playerPos + new Vector3(toMaxCorner.x, 0f, toMaxCorner.y) * maxCornerDist, bMaxHit ? Color.red : Color.cyan);

                return bMaxHit && bMinHit;
            }
        }

        return false;
    }

    static bool IsObjectInVisLight(in Vector2 position, in Vector2 boxHalfSize)
    {
        Vector2[] corners = { new Vector2(-boxHalfSize.x, -boxHalfSize.y), new Vector2(boxHalfSize.x, -boxHalfSize.y), new Vector2(-boxHalfSize.x, boxHalfSize.y), new Vector2(boxHalfSize.x, boxHalfSize.y) };
        foreach(Vector2 corner in corners)
        {
            if(IsPointInVisLight(corner + position))
            {
                return true;
            }
        }

        return false;
    }

    static bool IsPointInVisLight(Vector2 Point)
    {
        if(!visLight)
        {
            return false;
        }

        Vector2 PointRelative = new Vector2(Point.x - visLight.transform.position.x, Point.y - visLight.transform.position.z);

        if (PointRelative.sqrMagnitude > (visLight.light.range * .5f) * (visLight.light.range * .5f))
        {
            return false;
        }

        if(PointRelative.sqrMagnitude < (visLight.VisibilityBoundsDistanceFalloff * visLight.VisibilityBoundsDistanceFalloff))
        {
            return true;
        }

        Vector2 LeftBounds2D = new Vector2(visLight.LeftBounds.x, visLight.LeftBounds.z);
        Vector2 LeftBoundsRightVec = new Vector2(visLight.LeftBounds.z, -visLight.LeftBounds.x);

        Vector2 RightBounds2D = new Vector2(visLight.RightBounds.x, visLight.RightBounds.z);
        Vector2 RightBoundsRightVec = new Vector2(visLight.RightBounds.z, -visLight.RightBounds.x);

        float PointDotLeft = Vector2.Dot(PointRelative, LeftBounds2D);
        Vector2 PointLeftPerp = PointRelative - LeftBounds2D * PointDotLeft;
        float LeftPerpDot = Vector2.Dot(PointLeftPerp, LeftBoundsRightVec);

        float PointDotRight = Vector2.Dot(PointRelative, RightBounds2D);
        Vector2 PointRightPerp = PointRelative - RightBounds2D * PointDotRight;
        float RightPerpDot = Vector2.Dot(PointRightPerp, -RightBoundsRightVec);

        bool bIsInLeftBounds = LeftPerpDot < 0 && PointDotLeft > 0;
        bool bIsInLeftTransition = LeftPerpDot > 0 && LeftPerpDot < visLight.VisibilityBoundsDistanceFalloff && PointDotLeft > 0;
        bool bIsInRightBounds = RightPerpDot < 0 && PointDotRight > 0;
        bool bIsInRightTransition = RightPerpDot > 0 && RightPerpDot < visLight.VisibilityBoundsDistanceFalloff && PointDotRight > 0;

        //Vector3 p3d = new Vector3(PointRelative.x + visLight.transform.position.x, visLight.transform.position.y, PointRelative.y + visLight.transform.position.z);
        //Vector3 plp = new Vector3(PointLeftPerp.x + visLight.transform.position.x, visLight.transform.position.y, PointLeftPerp.y + visLight.transform.position.z);
        //Vector3 prp = new Vector3(PointRightPerp.x + visLight.transform.position.x, visLight.transform.position.y, PointRightPerp.y + visLight.transform.position.z);
        //Vector3 rb = new Vector3(-RightBoundsRightVec.x, 0, -RightBoundsRightVec.y) * VisibilityBoundsBuffer + visLight.transform.position;
        //Vector3 lb = new Vector3(LeftBoundsRightVec.x, 0, LeftBoundsRightVec.y) * VisibilityBoundsBuffer + visLight.transform.position;

        //Debug.DrawLine(p3d, plp, Color.green);
        //Debug.DrawLine(visLight.transform.position, lb, Color.green);
        //Debug.DrawLine(visLight.transform.position, visLight.transform.position + visLight.LeftBounds * PointDotLeft, Color.green);

        //Debug.DrawLine(p3d, prp, Color.red);
        //Debug.DrawLine(visLight.transform.position, rb, Color.red);
        //Debug.DrawLine(visLight.transform.position, visLight.transform.position + visLight.RightBounds * PointDotRight, Color.red);

        //Debug.Log("LeftB: " + bIsInLeftBounds + " | LeftT: " + bIsInLeftTransition + " | RightB: " + bIsInRightBounds + " | RightT: " + bIsInRightTransition);

        return (bIsInLeftBounds && bIsInRightBounds) || bIsInLeftTransition ||  bIsInRightTransition;
    }

    static Collider visibleAreaCollider;

    HashSet<SpookyThing> inAreaThings = new HashSet<SpookyThing>();

    private void Awake()
    {
        visibleArea = this;

        foreach(Collider collider in GetComponents<Collider>())
        {
            if(collider.enabled)
            {
                visibleAreaCollider = collider;
                break;
            }
        }
        //visibleAreaCollider = GetComponent<Collider>();
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
            thing.SetInVisibleArea(IsInVisibleArea(new Vector2(thing.transform.position.x, thing.transform.position.z), new Vector2(thingExtents.x, thingExtents.z)));
        }
    }
}
