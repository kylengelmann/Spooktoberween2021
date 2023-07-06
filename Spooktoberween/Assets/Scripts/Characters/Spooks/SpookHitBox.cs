using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpookHitBox : MonoBehaviour
{
    HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    private void OnEnable()
    {
        hitObjects.Clear();
    }

    private void OnDisable()
    {
        hitObjects.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && !hitObjects.Contains(other.gameObject))
        {
            hitObjects.Add(other.gameObject);
            IHittableInterface[] hittables = other.gameObject.GetComponents<IHittableInterface>();
            foreach (IHittableInterface hittable in hittables)
            {
                hittable.OnHit(this);
            }
        }
    }
}
