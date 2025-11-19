using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Slot : MonoBehaviour
{
    public bool isFilled = false;
    public int slotId;
    private void OnDisable()
    {
        isFilled = false;
    }
}

