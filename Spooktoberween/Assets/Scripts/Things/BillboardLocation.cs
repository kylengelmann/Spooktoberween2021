using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardLocation : MonoBehaviour
{
    static readonly float OriginZHeight = -10f;
    static readonly float FloorAngle = -60f;

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, OriginZHeight + Mathf.Tan(FloorAngle * Mathf.Deg2Rad) * transform.position.z, transform.position.z);
    }

    public static Vector3 GetWorldLocation(Vector2 location)
    {
        return new Vector3(location.x, OriginZHeight + Mathf.Tan(FloorAngle * Mathf.Deg2Rad) * location.y, location.y);
    }
}
