using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spooktober/SpookBehavior")]
public class SpookBehavior : ScriptableObject
{
    public SpookManager.PossessData possessData = new SpookManager.PossessData() {possessProbability=.05f, possessNearPlayerProbability=.2f, nearPlayerPossessRadius=3f};

    public SpookManager.TeleportData teleportData = new SpookManager.TeleportData() {teleportProbability=.2f, teleportRadius=1f, unpossessProbability=.1f};

    public SpookManager.HuntData huntData = new SpookManager.HuntData() {huntProbabilities = new float[3]{ .2f, .1f, .5f},  huntRadius = .5f, huntTeleports = 3, minHuntTeleportTime = 3f, maxHuntTeleportTime = 5f};

    public GameObject shadowPrefab;
}
