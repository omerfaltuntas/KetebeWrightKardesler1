using UnityEngine;

[System.Serializable]
public class MyAudioClip
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f; // Ses seviyesi eklendi
}

