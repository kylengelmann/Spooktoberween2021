using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpookyRoom : MonoBehaviour
{
    public GameObject[] Walls;
    public GameObject Floor;

    public SpookyThingSpawnSettings spawnSettings;

    Tilemap FloorTilemap;
    TilemapRenderer WallsRenderer;

    List<SpookyThingSpawnLocation> Spawners = new List<SpookyThingSpawnLocation>();
    List<SpookyThingSpawnLocation> SpawnersRemaining = new List<SpookyThingSpawnLocation>();

    const int SpawnsPerFrame = 10;

    private void Awake()
    {
        FloorTilemap = Floor.GetComponent<Tilemap>();
        FindObjectOfType<SpookyGameManager>().RegisterRoom(this);
    }

    public IEnumerator SpawnRoutine()
    {
        SpawnersRemaining.Clear();
        SpawnersRemaining.AddRange(Spawners);
        for(int spawnerIdx = 0; spawnerIdx < SpawnersRemaining.Count; ++spawnerIdx)
        {
            int rand = Random.Range(0, Spawners.Count);
            SpookyThingSpawnLocation temp = SpawnersRemaining[spawnerIdx];
            SpawnersRemaining[spawnerIdx] = SpawnersRemaining[rand];
            SpawnersRemaining[rand] = temp;
        }

        int numLeftOfType = Random.Range(spawnSettings.settings[0].minNumThings, spawnSettings.settings[0].maxNumThings + 1);
        int settingsIdx = 0;
        while(settingsIdx < spawnSettings.settings.Count)
        {
            int numSpawned = 0;
            while(numSpawned < SpawnsPerFrame)
            {
                if(numLeftOfType > 0)
                {
                    if(SpawnThing(spawnSettings.settings[settingsIdx].Type))
                    {
                        --numLeftOfType;
                        ++numSpawned;
                        continue;
                    }
                }

                ++settingsIdx;
                if (settingsIdx >= spawnSettings.settings.Count)
                {
                    break;
                }

                numLeftOfType = Random.Range(spawnSettings.settings[settingsIdx].minNumThings, spawnSettings.settings[settingsIdx].maxNumThings + 1);
            }

            yield return null;
        }
    }

    public bool SpawnThing(EThingType type)
    {
        for(int i = 0; i < SpawnersRemaining.Count; ++i)
        {
            SpookyThingSpawnLocation spawner = SpawnersRemaining[i];
            if(spawner.CanThingSpawn(type))
            {
                GameObject thingToSpawn = SpookyGameManager.gameManager.GetSpawnRates().GetRandomThingPrefab(type);
                GameObject thingSpawned = Instantiate<GameObject>(thingToSpawn);
                thingSpawned.transform.position = spawner.transform.position;
                spawner.OnThingSpawned(thingSpawned.GetComponent<SpookyThing>());
                SpawnersRemaining.RemoveAt(i);

                SpookManager.spookManager.RegisterThing(thingSpawned.GetComponent<SpookyThing>());
                return true;
            }
        }

        return false;
    }

    public bool ContainsLocation(in Vector3 Location)
    {
        return FloorTilemap.HasTile(FloorTilemap.WorldToCell(Location));
    }

    public void RegisterSpawner(SpookyThingSpawnLocation spawner)
    {
        Spawners.Add(spawner);
    }


}