using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly:CheatSystem.CheatClass(typeof(SpookyGameManager))]

public class SpookyGameManager : MonoBehaviour
{
    public static SpookyGameManager gameManager {get; private set;}

    public PlayerController playerController {get; private set;}

    public SpookyPlayer player {get; private set;}

    [SerializeField] SpookBehavior spookBehavior;
    [SerializeField] SpookyThingSpawnRatesAsset spawnRates;

    public SpookyThingSpawnRatesAsset GetSpawnRates() { return spawnRates; }

    SpookManager spookManager;

    List<SpookyRoom> rooms = new List<SpookyRoom>();

#if USING_CHEAT_SYSTEM
    Light debugDirectionalLight;
#endif

    private void Awake()
    {
        if(gameManager)
        {
            Debug.LogFormat("static gamemanager already set to {0}", gameManager.gameObject.name);
            Destroy(this);
            return;
        }

        gameManager = this;

        DontDestroyOnLoad(gameObject);

        spawnRates.PreCacheEntries();

        spookManager = gameObject.AddComponent<SpookManager>();
        spookManager.SetSpookBehavior(spookBehavior);

#if USING_CHEAT_SYSTEM
        gameObject.AddComponent<CheatSystem.CheatSystem>();

        GameObject debugDirLightGO = new GameObject();
        debugDirLightGO.transform.parent = transform;
        debugDirLightGO.transform.localPosition = Vector3.zero;
        debugDirLightGO.transform.localRotation = Quaternion.Euler(90, 0, 0);
        debugDirectionalLight = debugDirLightGO.AddComponent<Light>();
        debugDirectionalLight.type = LightType.Directional;
        debugDirectionalLight.enabled = false;
#endif
    }

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    public void SetPlayerController(PlayerController newPlayerController)
    {
        playerController = newPlayerController;
        player = playerController.player;
    }

    public void RegisterRoom(SpookyRoom room)
    {
        rooms.Add(room);
    }    

    IEnumerator SpawnLoop()
    {
        yield return null;

        foreach(SpookyRoom room in rooms)
        {
            yield return room.SpawnRoutine();
        }

        OnSpawningFinished();
    }

    void OnSpawningFinished()
    {
        spookManager.OnThingsSpawned();

        
    }

    [CheatSystem.Cheat(), System.Diagnostics.Conditional("USING_CHEAT_SYSTEM")]
    void ToggleDebugDirectionalLight()
    {
        debugDirectionalLight.enabled = !debugDirectionalLight.enabled;
    }
}
