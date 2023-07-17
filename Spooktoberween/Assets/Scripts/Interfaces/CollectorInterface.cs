using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollectorInterface
{
    public abstract bool Collect(Collectable collectable);
}
