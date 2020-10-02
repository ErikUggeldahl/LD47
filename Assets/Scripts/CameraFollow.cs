using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;

    readonly Vector3 OFFSET = new Vector3(-5f, 7f, -5f);
    const float FOLLOW_FACTOR = 5f;

    void Update()
    {
        var idealPosition = target.position + OFFSET;
        transform.position = Vector3.Lerp(transform.position, idealPosition, FOLLOW_FACTOR * Time.deltaTime);
    }
}
