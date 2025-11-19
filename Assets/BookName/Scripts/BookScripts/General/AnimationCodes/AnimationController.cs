using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator anim;
    public string animBool;
    private float animationDuration;
    public Transform startPos;
    public bool isRepeatable;

    public string animationSoundName;
    public bool isSoundLoop;

    private bool isPlayingSound = false;

    private void OnDisable()
    {
        anim.SetBool(animBool, false);
        GetComponent<BoxCollider2D>().enabled = true;

        if (startPos != null)
            transform.position = startPos.position;

        StopAnimationSound();
    }

    private void Start()
    {
        if (startPos != null)
            transform.position = startPos.position;

        AnimationClip clip = anim.runtimeAnimatorController.animationClips[0];
        animationDuration = clip.length;
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(AnimationRoutine());
        }
    }

    IEnumerator AnimationRoutine()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        anim.SetBool(animBool, true);

        PlayAnimationSound();

        if (isRepeatable)
        {
            yield return new WaitForSeconds(animationDuration);
            anim.SetBool(animBool, false);
        }
        else
        {
            while (anim.GetBool(animBool))
            {
                yield return null;
            }
        }

        GetComponent<BoxCollider2D>().enabled = true;

        StopAnimationSound();
    }

    void PlayAnimationSound()
    {
        if (string.IsNullOrEmpty(animationSoundName) || AudioManager.instance == null)
            return;

        // 🔇 Efekt kapalıysa sesi hiç oynatma
        if (PlayerPrefs.GetInt("SoundData") == 1)
            return;

        if (!isPlayingSound)
        {
            if (isSoundLoop)
                AudioManager.instance.PlayClip(animationSoundName, true);
            else
                AudioManager.instance.PlayClip(animationSoundName);

            isPlayingSound = true;
        }
    }


    void StopAnimationSound()
    {
        if (string.IsNullOrEmpty(animationSoundName) || AudioManager.instance == null)
            return;

        if (isPlayingSound)
        {
            AudioManager.instance.Stop(animationSoundName);
            isPlayingSound = false;
        }
    }
}