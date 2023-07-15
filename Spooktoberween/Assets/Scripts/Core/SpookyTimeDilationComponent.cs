using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpookyTimeDilationComponent : MonoBehaviour
{
    [System.NonSerialized, HideInInspector]
    public float timeDilation = 1f;

    public float GetDeltaTime()
    {
        return timeDilation * Time.deltaTime;
    }
}
