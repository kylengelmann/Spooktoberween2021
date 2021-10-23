using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpookManager : MonoBehaviour
{

    public static SpookManager spookManager;

    private void Awake()
    {
        spookManager = this;
    }

    List<SpookyThing> things = new List<SpookyThing>();

    public void RegisterThing(SpookyThing thing)
    {
        things.Add(thing);
    }


}
