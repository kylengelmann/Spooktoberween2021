using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spooktober/SpawnRates")]
public class SpookyThingSpawnRatesAsset : ScriptableObject
{
    [NamedListEntry("Thing")]
    public List<SpookyThingSpawnRatesEntry> Things;

    public float[] RaritySpawnWeights;

    Dictionary<EThingType, SpawnRatesDictionaryStruct> entries = new Dictionary<EThingType, SpawnRatesDictionaryStruct>();

    public void PreCacheEntries()
    {
        entries = new Dictionary<EThingType, SpawnRatesDictionaryStruct>();
        foreach(SpookyThingSpawnRatesEntry thingEntry in Things)
        {
            AddEntryToDictionary(thingEntry);
        }
    }

    public GameObject GetRandomThingPrefab(EThingType type)
    {
        SpawnRatesDictionaryStruct dictEntry = new SpawnRatesDictionaryStruct();
        if (!entries.TryGetValue(type, out dictEntry))
        {
            EThingType baseType = EThingType.None;
            foreach(EThingType recordedType in entries.Keys)
            {
                if((type & recordedType) != EThingType.None)
                {
                    baseType = recordedType;
                    break;
                }
            }

            if(baseType == EThingType.None)
            {
                return null;
            }

            dictEntry.entries = new List<SpookyThingSpawnRatesEntry>();

            foreach(SpookyThingSpawnRatesEntry ratesEntry in dictEntry.entries)
            {
                if ((ratesEntry.Thing.GetComponent<SpookyThing>().ThingType & type) == type)
                {
                    dictEntry.entries.Add(ratesEntry);
                    dictEntry.TotalWeight += RaritySpawnWeights[ratesEntry.Rarity];
                }
            }

            entries.Add(type, dictEntry);
        }

        if(dictEntry.entries.Count == 0)
        {
            return null;
        }

        float rand = Random.value * dictEntry.TotalWeight;
        float currentWeight = 0f;
        for(int i = 0; i < dictEntry.entries.Count; ++i)
        {
            currentWeight += RaritySpawnWeights[dictEntry.entries[i].Rarity];
            if(currentWeight >= rand)
            {
                return dictEntry.entries[i].Thing;
            }
        }

        Debug.Assert(false);
        return null;
    }

    private struct SpawnRatesDictionaryStruct
    {
        public List<SpookyThingSpawnRatesEntry> entries;
        public float TotalWeight;
    }

    void AddEntryToDictionary(in SpookyThingSpawnRatesEntry entry)
    {
        if(entry.Rarity < 0 || entry.Rarity >= RaritySpawnWeights.Length)
        {
            Debug.Assert(false);
            return;
        }

        SpookyThing spookyThing = null;
        if(!entry.Thing || !(spookyThing = entry.Thing.GetComponent<SpookyThing>()))
        {
            Debug.Assert(entry.Thing);
            Debug.Assert(spookyThing);
            return;
        }

        if(spookyThing.ThingType == EThingType.None)
        {
            Debug.Assert(false);
            return;
        }

        EThingType typesLeft = spookyThing.ThingType;
        for(int i = 0; typesLeft != EThingType.None; ++i)
        {
            EThingType currentType = (EThingType)(1 << i);
            if ((currentType & spookyThing.ThingType) != 0)
            {
                typesLeft &= ~currentType;

                if(!entries.ContainsKey(currentType))
                {
                    SpawnRatesDictionaryStruct dictEntry = new SpawnRatesDictionaryStruct();
                    dictEntry.entries = new List<SpookyThingSpawnRatesEntry>();
                    entries.Add(currentType, dictEntry);
                }

                // yes all these entry var names suck, deal with it
                SpawnRatesDictionaryStruct entryAddingTo = entries[currentType];

                entryAddingTo.entries.Add(entry);
                entryAddingTo.TotalWeight += RaritySpawnWeights[entry.Rarity];

                entries[currentType] = entryAddingTo;
            }
        }
    }
}

[System.Serializable]
public struct SpookyThingSpawnRatesEntry
{
    public GameObject Thing;

    public int Rarity;
}
