using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    [SerializeField] GuardCap cap = null;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Die()
    {
        cap.SetState(GuardCap.State.Dead);
    }

    public void Explode()
    {
        var rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = false;
        }
    }
}
