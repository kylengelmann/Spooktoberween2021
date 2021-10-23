using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpookPossessComponent : MonoBehaviour
{
    SpookyThing thingPossessing;
    Coroutine PassiveUpdateRoutine;
    float teleportChance = .2f;
    float teleportRadius = 1f;

    private void Awake()
    {
        thingPossessing = GetComponent<SpookyThing>();
        thingPossessing.onVisibleChanged += OnVisibleChanged;
    }

    void OnVisibleChanged(bool bIsVisible)
    {
        if(!gameObject.activeInHierarchy) return;

        if(bIsVisible)
        {
            if (PassiveUpdateRoutine != null)
            {
                StopCoroutine(PassiveUpdateRoutine);
                PassiveUpdateRoutine = null;
            }
        }
        else
        {
            if (PassiveUpdateRoutine == null)
            {
                PassiveUpdateRoutine = StartCoroutine(PassiveUpdate());
            }
        }
    }

    IEnumerator PassiveUpdate()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        while(true)
        {
            if(Random.value < teleportChance)
            {
                Vector2 randomTeleportLocation = Random.insideUnitCircle*teleportRadius;
                //NavMesh.SamplePosition()
                transform.position = new Vector3(randomTeleportLocation.x, 0f, randomTeleportLocation.y);
                
            }
            yield return wait;
        }
    }
}
