using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Bu kod tıkladimiz objenin ses calmasini istedgimiz koddur.

public class ClickVoice : MonoBehaviour
{
    public string soundName;
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayClip(soundName);

            }
        }

    }
    private void OnDisable()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Stop(soundName);
        }
    }

}
