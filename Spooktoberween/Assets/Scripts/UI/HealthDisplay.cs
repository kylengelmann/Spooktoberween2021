using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
    public GameObject HealthPipPrefab;

    SpookyPlayer player;

    GameObject[] healthPips;

    private void Start()
    {
        player = Object.FindObjectOfType<SpookyPlayer>();

        healthPips = new GameObject[player.maxHP];
        for(int i = 0; i < player.maxHP; ++i)
        {
            healthPips[i] = Object.Instantiate(HealthPipPrefab, transform);
        }

        OnPlayerHPChanged(player.currentHP, player.currentHP);
        player.onHPChanged += OnPlayerHPChanged;
    }

    void OnPlayerHPChanged(int newHP, int oldHP)
    {
        for(int i = 0; i < healthPips.Length; ++i)
        {
            healthPips[i].SetActive(i < newHP);
        }
    }
}
