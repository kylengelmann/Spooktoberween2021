using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider)), RequireComponent(typeof(SpookyCollider))]
public class SpookyThingSpawnLocation : MonoBehaviour
{
    public EThingType TypesAllowed;
    public EThingType TypesBlocked;
    public float ThingSpriteYOffset = 0f;

    private void Start()
    {
        foreach(SpookyRoom room in FindObjectsOfType<SpookyRoom>())
        {
            if(room.ContainsLocation(transform.position))
            {
                room.RegisterSpawner(this);
            }
        }
    }

    public bool CanThingSpawn(SpookyThing spookyThing)
    {
        return !thingOccupying && (spookyThing.ThingType & TypesAllowed) != 0 && (spookyThing.ThingType & TypesBlocked) == 0;
    }

    public bool CanThingSpawn(EThingType ThingType)
    {
        return !thingOccupying && (ThingType & TypesAllowed) != 0 && (ThingType & TypesBlocked) == 0;
    }

    public void OnThingSpawned(SpookyThing spookyThing)
    {
        Debug.Assert(thingOccupying == null);
        if (thingOccupying == null)
        {
            BillboardLocation billboard = spookyThing.GetComponentInChildren<BillboardLocation>();
            if(billboard)
            {
                billboard.SetYOffset(ThingSpriteYOffset);
            }

            SpookyCollider collider = spookyThing.GetComponent<SpookyCollider>();
            if(collider)
            {
                collider.enabled = true;
            }

            spookyThing.OnTeleported += OnThingLeft;
            thingOccupying = spookyThing;
        }
    }

    void OnThingLeft(SpookyThing spookyThing)
    {
        Debug.Assert(spookyThing == thingOccupying);
        if(thingOccupying == spookyThing)
        {
            BillboardLocation billboard = spookyThing.GetComponentInChildren<BillboardLocation>();
            if (billboard)
            {
                billboard.SetYOffset(0f);
            }

            spookyThing.OnTeleported -= OnThingLeft;
            thingOccupying = null;
        }
    }

    protected SpookyThing thingOccupying;

    private void Reset()
    {
        GetComponent<SpookyCollider>().bShouldTick = false;
    }
}
