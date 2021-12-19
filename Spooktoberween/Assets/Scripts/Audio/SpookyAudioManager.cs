using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpookyAudioManager : MonoBehaviour
{
    public AudioClip huntSound;

    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if(audioSource)
        SpookManager.spookManager.onHuntStatusChanged += OnHuntChanged;
    }

    void OnHuntChanged(bool bNewHunt)
    {
        if(bNewHunt)
        {
            audioSource.clip = huntSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }
}
