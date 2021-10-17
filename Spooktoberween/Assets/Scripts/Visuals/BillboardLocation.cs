using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardLocation : MonoBehaviour
{
    float OriginZHeight = -10f;
    float FloorAngle = -60f;

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, OriginZHeight + Mathf.Tan(FloorAngle * Mathf.Deg2Rad) * transform.position.z, transform.position.z);
    }
}
