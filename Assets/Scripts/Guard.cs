using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour
{
    [SerializeField] GuardCap cap = null;
    [SerializeField] NavMeshAgent agent = null;
    [SerializeField] Transform patrolPath = null;

    const float PATROL_ARRIVED_DISTANCE = 0.5f;
    Transform[] patrolPoints;
    int currentPatrolPoint = 0;

    void Start()
    {
        agent.autoBraking = false;

        patrolPoints = patrolPath.GetComponentsInChildren<Transform>().Where(t => t != patrolPath).ToArray();
        PatrolToCurrentPoint();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < PATROL_ARRIVED_DISTANCE)
        {
            currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Length;
            PatrolToCurrentPoint();
        }
    }

    void PatrolToCurrentPoint()
    {
        agent.destination = patrolPoints[currentPatrolPoint].position;
    }

    public void Die()
    {
        cap.SetState(GuardCap.State.Dead);

        agent.enabled = false;
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
