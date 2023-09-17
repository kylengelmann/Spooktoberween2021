using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spooktober/ThingSpawnSettings")]
public class SpookyThingSpawnSettings : ScriptableObject
{
    [NamedListEntry("Type")]
    public List<ThingSpawnSettings> settings = new List<ThingSpawnSettings>();
}

[System.Serializable]
public struct ThingSpawnSettings
{
    public EThingType Type;

    public int minNumThings;
    public int maxNumThings;
}
