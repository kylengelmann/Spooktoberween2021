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
        public float possessProbability;
        public float possessNearPlayerProbability;
        public float nearPlayerPossessRadius;
    }

    [System.Serializable]
    public struct TeleportData
    {
        public float teleportProbability;
        public float teleportRadius;
        public float unpossessProbability;
    }

    [System.Serializable]
    public struct HuntData
    {
        public float[] huntProbabilities;
        public float huntRadius;
        public int huntTeleports;
        public float minHuntTeleportTime;
        public float maxHuntTeleportTime;
    }

    SpookBehavior spookBehavior;

    List<SpookyThing> things = new List<SpookyThing>();
    List<SpookyThing> thingsNearPlayer = new List<SpookyThing>();

    public uint numIdle { get; private set; }

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
        numIdle = 5;
    }

    private void Start()
    {
        StartCoroutine(PossessUpdate());
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
        }
    }

    public void ProgressHunt()
    {
        if(onHuntProgressed != null) onHuntProgressed();
    }

    public float GetHuntProbability()
    {
        if(numHunting >= spookBehavior.huntData.huntProbabilities.Length) return 0;

        return spookBehavior.huntData.huntProbabilities[numHunting];
    }

    public void OnThingNearPlayer(SpookyThing thing)
    {
        thingsNearPlayer.Add(thing);
    }

    public void OnThingNotNearPlayer(SpookyThing thing)
    {
        thingsNearPlayer.Remove(thing);
    }

    IEnumerator PossessUpdate()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);

        while(true)
        {
            if(numIdle > 0 && Random.value < spookBehavior.possessData.possessProbability)
            {
                SpookyThing thingToPossess = null;
                if(thingsNearPlayer.Count > 0 && Random.value < spookBehavior.possessData.possessNearPlayerProbability)
                {
                    thingToPossess = thingsNearPlayer[Random.Range(0, thingsNearPlayer.Count)];
                }
                else
                {
                    thingToPossess = things[Random.Range(0, things.Count)];
                }

                if(thingToPossess.GetComponent<SpookPossessComponent>() == null)
                {
                    thingToPossess.gameObject.AddComponent<SpookPossessComponent>();
                    --numIdle;
                }
            }

            yield return wait;
        }
    }

    public void OnUnpossess(SpookPossessComponent possessComponent)
    {
        ++numIdle;
    }

    public void OnShadowDispersed()
    {
        ++numIdle;
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
