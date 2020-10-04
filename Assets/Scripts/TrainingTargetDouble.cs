using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingTargetDouble : MonoBehaviour
{
    [SerializeField] TrainingTargetDouble otherTarget = null;
    [SerializeField] GameObject bridge = null;

    public bool approved = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Dagger>())
        {
            GetComponentInParent<TrainingTarget>().Approve();
            approved = true;

            if (otherTarget.approved)
            {
                bridge.SetActive(true);
                enabled = false;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Dagger>())
        {
            if (!otherTarget.approved)
            {
                approved = false;
                GetComponentInParent<TrainingTarget>().Deny();
            }
            else
            {
                enabled = false;
            }
        }
    }
}
