using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpookManager : MonoBehaviour
{
    public static SpookManager spookManager;

    [System.Serializable]
    public struct TeleportData
    {
        public float teleportProbability;
        public float teleportRadius;
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

    public uint numHunting {get; private set;}
    HashSet<SpookPossessComponent> hunters = new HashSet<SpookPossessComponent>();

    public delegate void OnHuntStatusChanged(bool bIsHunting);
    public OnHuntStatusChanged onHuntStatusChanged;

    public delegate void OnHuntProgressed();
    public OnHuntProgressed onHuntProgressed;

    private void Awake()
    {
        spookManager = this;
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

    public void RemoveHunt(SpookPossessComponent hunter)
    {
        if(hunter && hunters.Contains(hunter))
        {
            hunters.Remove(hunter);
            numHunting = (uint)hunters.Count;

            if(numHunting == 0 && onHuntStatusChanged != null) onHuntStatusChanged(false);
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
}
