using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    const float TRAVEL_SPEED = 60f;

    const float LIFETIME = 5f;
    float aliveTime = 0f;

    Vector3 target;
    public Vector3 Target
    {
        get { return target; }
        set
        {
            target = value;
            transform.LookAt(target);
        }
    }

    void Update()
    {
        aliveTime += Time.deltaTime;
        if (aliveTime > LIFETIME)
        {
            Destroy(gameObject);
        }

        transform.position += transform.forward * TRAVEL_SPEED * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<CharacterControl>().Die();
        }

        Destroy(gameObject);
    }
}
