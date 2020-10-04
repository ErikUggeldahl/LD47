using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingTargetSingle : MonoBehaviour
{
    [SerializeField] GameObject bridge = null;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Dagger>())
        {
            GetComponentInParent<TrainingTarget>().Approve();
            bridge.SetActive(true);
        }
    }
}
