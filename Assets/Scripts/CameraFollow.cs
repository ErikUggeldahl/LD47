using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target = null;

    readonly Vector3 OFFSET = new Vector3(-3f, 5f, -3f);
    const float FOLLOW_FACTOR = 5f;

    void Update()
    {
        var idealPosition = target.position + OFFSET;
        if (Application.isPlaying)
        {
            transform.position = Vector3.Lerp(transform.position, idealPosition, FOLLOW_FACTOR * Time.deltaTime);
        }
        else
        {
            transform.position = idealPosition;
        }
    }
}
