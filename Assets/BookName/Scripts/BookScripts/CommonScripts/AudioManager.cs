using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;

    public AudioSource _bgAudioSource;
    public AudioSource _storytellingSource;
    public AudioSource _singleAudioSource;
    public AudioSource buttonClick;

    public List<MyAudioClip> clips;

    private List<AudioSource> _audioSources = new List<AudioSource>();
    private Dictionary<string, AudioSource> activeLoopingSounds = new Dictionary<string, AudioSource>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        var allSources = FindObjectsOfType<AudioSource>(true);
        foreach (var source in allSources)
        {
            _audioSources.Add(source);
        }

        _audioSources.Remove(_bgAudioSource);
        _audioSources.Remove(_storytellingSource);
    }

    public void PlayClip(string name, bool loop = false)
    {
        var selectedClip = clips.Find(clip => clip.name == name);
        if (selectedClip == null) return;

        if (loop)
        {
            if (activeLoopingSounds.ContainsKey(name))
            {
                if (activeLoopingSounds[name] != null)
                    return;
            }

            GameObject tempObj = new GameObject("LoopingAudio_" + name);
            AudioSource tempSource = tempObj.AddComponent<AudioSource>();
            tempSource.clip = selectedClip.clip;
            tempSource.volume = selectedClip.volume;
            tempSource.loop = true;
            tempSource.Play();

            activeLoopingSounds[name] = tempSource;

            StartCoroutine(DestroyWhenClipStops(tempSource, tempObj, name));
        }
        else
        {
            _singleAudioSource.PlayOneShot(selectedClip.clip, selectedClip.volume);
        }
    }

    public void Stop(string name)
    {
        if (activeLoopingSounds.ContainsKey(name))
        {
            Destroy(activeLoopingSounds[name].gameObject);
            activeLoopingSounds.Remove(name);
        }
    }

    private IEnumerator DestroyWhenClipStops(AudioSource source, GameObject obj, string name)
    {
        while (source != null && source.isPlaying)
            yield return null;

        if (obj != null)
            Destroy(obj);

        if (activeLoopingSounds.ContainsKey(name))
            activeLoopingSounds.Remove(name);
    }

    public float GetClipLength(string name)
    {
        var selectedClip = clips.Find(clip => clip.name == name);
        return selectedClip != null ? selectedClip.clip.length : 0f;
    }

    public void MuteAllLoopingSounds(bool mute)
    {
        foreach (var pair in activeLoopingSounds)
        {
            if (pair.Value != null)
                pair.Value.mute = mute;
        }

        if (buttonClick != null)
            buttonClick.mute = mute;
    }

    public void Pause()
    {
        _audioSources.ForEach(source => source.Pause());
        _bgAudioSource?.Pause();
        _storytellingSource?.Pause();
    }

    public void UnPause()
    {
        _audioSources.ForEach(source => source.UnPause());
        _bgAudioSource?.UnPause();
        _storytellingSource?.UnPause();
    }
}

