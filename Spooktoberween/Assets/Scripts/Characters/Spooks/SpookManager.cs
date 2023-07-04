using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly:CheatSystem.CheatClass(typeof(SpookManager))]
public class SpookManager : MonoBehaviour
{
    public static SpookManager spookManager;

    [System.Serializable]
    public struct PossessData
    {
        public float possessNearPlayerProbability;
        public float nearPlayerPossessRadius;
    }

    [System.Serializable]
    public struct TeleportData
    {
        public SpookyRandRange teleportCooldown;
        public float teleportRadius;
        public float switchObjectChance;
        public SpookyRandRange huntStartTimerLength;
        public float huntStartPlayerRadius;
    }

    [System.Serializable]
    public struct HuntData
    {
        public float huntPlayerRadius;
        public SpookyRandRange huntTeleportCooldown;
        public SpookyRandRange huntDuration;
        public float maxHuntDurationExtension;
    }

    SpookBehavior spookBehavior;

    List<SpookyThing> things = new List<SpookyThing>();
    List<SpookyThing> thingsNearPlayer = new List<SpookyThing>();

    public uint numHunting {get; private set;}
    HashSet<SpookPossessComponent> hunters = new HashSet<SpookPossessComponent>();

    public delegate void OnHuntStatusChanged(bool bIsHunting);
    public OnHuntStatusChanged onHuntStatusChanged;

    public delegate void OnHuntProgressed();
    public OnHuntProgressed onHuntProgressed;

    #if USING_CHEAT_SYSTEM
    bool bDebug_DisplayPossesTeleportAttempts = false;
    #endif // USING_CHEAT_SYSTEM

    private void Awake()
    {
        spookManager = this;
    }

    private void Start()
    {
        const int NumSpooks = 3;
        for(int i = 0; i < NumSpooks; ++i)
        {
            PossessThing();
        }
    }

    public void SetSpookBehavior(SpookBehavior behavior)
    {
        spookBehavior = behavior;
    }

    public TeleportData GetTeleportData()
    {
        if(spookBehavior)
        {
            return spookBehavior.teleportData;
        }

        return new TeleportData();
    }

    public HuntData GetHuntData()
    {
        if (spookBehavior)
        {
            return spookBehavior.huntData;
        }

        return new HuntData();
    }

    public void UnregisterThing(SpookyThing thing)
    {
        things.Remove(thing);
        thingsNearPlayer.Remove(thing);
    }

    public void RegisterThing(SpookyThing thing)
    {
        things.Add(thing);
    }

    public void AddHunt(SpookPossessComponent hunter)
    {
        if(hunter && !hunters.Contains(hunter))
        {
            hunters.Add(hunter);

            numHunting = (uint)hunters.Count;

            if (numHunting == 1 && onHuntStatusChanged != null) onHuntStatusChanged(true);

            ProgressHunt();
        }
    }

    public void RemoveHunt(SpookPossessComponent hunter, bool bWasSuccessful)
    {
        if(hunter && hunters.Contains(hunter))
        {
            hunters.Remove(hunter);
            numHunting = (uint)hunters.Count;

            if(numHunting == 0 && onHuntStatusChanged != null) onHuntStatusChanged(false);

            if(bWasSuccessful)
            {
                Instantiate(spookBehavior.shadowPrefab, hunter.transform.position, hunter.transform.rotation);
            }

            Unpossess(hunter);
        }
    }

    public void ProgressHunt()
    {
        if(onHuntProgressed != null) onHuntProgressed();
    }

    public void OnThingNearPlayer(SpookyThing thing)
    {
        thingsNearPlayer.Add(thing);
    }

    public void OnThingNotNearPlayer(SpookyThing thing)
    {
        thingsNearPlayer.Remove(thing);
    }

    public void PossessThing()
    {
        SpookyThing thingToPossess = null;
        if (thingsNearPlayer.Count > 0 && Random.value < spookBehavior.possessData.possessNearPlayerProbability)
        {
            thingToPossess = thingsNearPlayer[Random.Range(0, thingsNearPlayer.Count)];
        }
        
        if(thingToPossess == null)
        {
            thingToPossess = things[Random.Range(0, things.Count)];
        }

        if(thingToPossess)
        {
            thingToPossess.gameObject.AddComponent<SpookPossessComponent>();
            UnregisterThing(thingToPossess);
        }
    }

    public void SwitchObjectPossessing(SpookPossessComponent possessComponent)
    {
        Unpossess(possessComponent);
        PossessThing();
    }

    public void Unpossess(SpookPossessComponent possessComponent)
    {
        RegisterThing(possessComponent.thingPossessing);
        Destroy(possessComponent);
    }

    public void OnShadowDispersed()
    {
        PossessThing();
    }

#if USING_CHEAT_SYSTEM
    [CheatSystem.Cheat()]
    void DisplayDebugTeleportData(int newDisplay = -1)
    {
        if(newDisplay < 0)
        {
            bDebug_DisplayPossesTeleportAttempts = !bDebug_DisplayPossesTeleportAttempts;
        }
        else
        {
            bDebug_DisplayPossesTeleportAttempts = newDisplay > 0;
        }
    }


    public bool IsDisplayingDebugTeleportData()
    {
        return bDebug_DisplayPossesTeleportAttempts;
    }
#endif
}
