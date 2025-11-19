using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Bu kod sayfa üzerinde ses caldirma kodudur.
public class AudioPlay : MonoBehaviour
{
    public string musicName;
    private void OnEnable()
    {
        AudioManager.instance.PlayClip(musicName);
    }

    private void OnDisable()
    {
        AudioManager.instance.Stop(musicName);
    }
}
