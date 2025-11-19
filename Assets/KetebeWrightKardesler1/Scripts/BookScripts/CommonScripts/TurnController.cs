using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TurnController : MonoBehaviour
{
    public float turnSpeed = 30f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * turnSpeed * Time.deltaTime);
    }
}

