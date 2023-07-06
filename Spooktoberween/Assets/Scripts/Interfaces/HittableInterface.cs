using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHittableInterface
{
    public abstract void OnHit(MonoBehaviour objectHitting);
}
