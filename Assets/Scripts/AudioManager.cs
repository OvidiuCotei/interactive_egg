using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioClip crackSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayCrackSound()
    {
        if (crackSound != null)
        {
            AudioSource.PlayClipAtPoint(crackSound, transform.position, 1f);
        }
    }
}
