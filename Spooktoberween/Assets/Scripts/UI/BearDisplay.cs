using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BearDisplay : MonoBehaviour
{
    [System.Serializable]
    struct ItemTypeToUIMapping
    {
        public CollectableType type;
        public GameObject icon;
    }

    [SerializeField]
    ItemTypeToUIMapping[] BearPartIcons;

    Dictionary<CollectableType, GameObject> BearParts;

    SpookyPlayer player;

    private void Start()
    {
        BearParts = new Dictionary<CollectableType, GameObject>();
        foreach(ItemTypeToUIMapping BearPart in BearPartIcons)
        {
            BearParts.Add(BearPart.type, BearPart.icon);
            BearPart.icon.SetActive(false);
        }

        player = Object.FindObjectOfType<SpookyPlayer>();
        player.onItemCollected += OnItemCollected;
    }

    private void OnDestroy()
    {
        if(player)
        {
            player.onItemCollected -= OnItemCollected;
        }
    }

    void OnItemCollected(Collectable collectable)
    {
        if(BearParts.ContainsKey(collectable.type))
        {
            BearParts[collectable.type].SetActive(true);
        }
    }
}
