using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOcclusionRevealer : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Occluder")
        {
            other.GetComponent<Revealer>().Reveal();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Occluder")
        {
            other.GetComponent<Revealer>().Solidify();
        }
    }
}
