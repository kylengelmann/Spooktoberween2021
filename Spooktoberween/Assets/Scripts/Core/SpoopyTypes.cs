using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpookyRandRange
{
    public float min;
    public float max;

    public float Get()
    {
        return Random.Range(min, max);
    }
}