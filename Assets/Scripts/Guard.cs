using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour
{
    [SerializeField] GuardCap cap = null;
    [SerializeField] NavMeshAgent agent = null;
    [SerializeField] Transform patrolPath = null;
    [SerializeField] int currentPatrolPoint = 0;
    [SerializeField] GameObject bulletPrefab = null;
    [SerializeField] Transform bulletSpawnPoint = null;
    [SerializeField] new AudioSource audio = null;
    [SerializeField] Sounds sounds = null;

    [Serializable]
    class Sounds
    {
        public AudioClip alarm = null;
        public AudioClip shoot = null;
        public AudioClip die = null;
    }

    const float PATROL_ARRIVED_DISTANCE = 0.5f;
    Transform[] patrolPoints;

    const float SIGHT_DISTANCE = 20f;
    int sightMask;

    Transform target = null;
    const float ATTACK_ANGLE = 30f;
    const float ATTACK_COOLDOWN_MAX = 2f;
    float attackCooldown = ATTACK_COOLDOWN_MAX;

    enum State
    {
        Patrolling,
        Attacking,
        Dead,
    }
    State state = State.Patrolling;

    void Start()
    {
        agent.autoBraking = false;

        if (patrolPath)
        {
            patrolPoints = patrolPath.GetComponentsInChildren<Transform>().Where(t => t != patrolPath).ToArray();
            PatrolToCurrentPoint();
        }

        sightMask = LayerMask.GetMask("World", "Player");
    }

    void Update()
    {
        if (state == State.Patrolling)
        {
            Patrol();
        }
        else if (state == State.Attacking)
        {
            Attack();
        }
    }

    void Patrol()
    {
        if (patrolPoints != null && patrolPoints.Length > 0 && !agent.pathPending && agent.remainingDistance < PATROL_ARRIVED_DISTANCE)
        {
            currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Length;
            PatrolToCurrentPoint();
        }

        RaycastHit hit;
        if (Physics.SphereCast(transform.position + new Vector3(0f, 3f, 0f), 2f, transform.forward, out hit, SIGHT_DISTANCE, sightMask))
        {
            if (hit.transform.tag == "Player")
            {
                state = State.Attacking;

                cap.SetState(GuardCap.State.Alarm);

                target = hit.transform;

                audio.PlayOneShot(sounds.alarm);
            }
        }
    }

    void Attack()
    {
        agent.destination = target.position;

        var angle = Vector3.Angle(transform.forward, target.position - transform.position);

        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f && angle <= ATTACK_ANGLE)
        {
            attackCooldown = ATTACK_COOLDOWN_MAX;
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            bullet.GetComponent<Bullet>().Target = target.position + new Vector3(0f, 1.2f, 0f);

            audio.PlayOneShot(sounds.shoot);
        }
    }

    void PatrolToCurrentPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPatrolPoint].position;
    }

    public void Die()
    {
        if (state == State.Dead) return;

        state = State.Dead;

        cap.SetState(GuardCap.State.Dead);

        agent.enabled = false;

        audio.PlayOneShot(sounds.die);
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
