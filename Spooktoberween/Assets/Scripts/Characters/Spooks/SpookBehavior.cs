using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spooktober/SpookBehavior")]
public class SpookBehavior : ScriptableObject
{
    public SpookManager.PossessData possessData = new SpookManager.PossessData() {
        possessNearPlayerProbability = .3f,
        nearPlayerPossessRadius = 2f
    };

    public SpookManager.TeleportData teleportData = new SpookManager.TeleportData() {
        teleportCooldown = new SpookyRandRange() { min=4f, max=10f }, 
        teleportRadius = 1f, 
        switchObjectChance = .4f, 
        huntStartTimerLength = new SpookyRandRange() { min=5f, max=7f },
        huntStartPlayerRadius = 2f
    };

    public SpookManager.HuntData huntData = new SpookManager.HuntData() {
        huntPlayerRadius = 3f,
        huntTeleportCooldown = new SpookyRandRange() { min=3f, max=5f },
        huntDuration = new SpookyRandRange() { min = 10f, max = 15f },
        maxHuntDurationExtension = 4f
    };

    public GameObject shadowPrefab;
}
