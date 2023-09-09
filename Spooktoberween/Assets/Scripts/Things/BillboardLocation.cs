using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardLocation : MonoBehaviour
{
    static readonly float OriginZHeight = -10f;
    static readonly float FloorAngle = -60f;

    private float YOffset = 0f;
    private void Update()
    {
        transform.position = new Vector3(transform.position.x, YOffset + OriginZHeight + Mathf.Tan(FloorAngle * Mathf.Deg2Rad) * transform.position.z, transform.position.z);
    }

    public void SetYOffset(float Offset)
    {
        YOffset = Offset;
    }

    public static Vector3 GetWorldLocation(Vector2 location)
    {
        return new Vector3(location.x, OriginZHeight + Mathf.Tan(FloorAngle * Mathf.Deg2Rad) * location.y, location.y);
    }
}
