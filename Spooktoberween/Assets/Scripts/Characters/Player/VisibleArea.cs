using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        SpookyThing thing = other.gameObject.GetComponent<SpookyThing>();
        if (thing)
        {
            thing.SetInVisibleArea(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SpookyThing thing = other.gameObject.GetComponent<SpookyThing>();
        if (thing)
        {
            thing.SetInVisibleArea(false);
        }
    }
}
