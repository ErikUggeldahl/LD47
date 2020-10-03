using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dagger : MonoBehaviour
{
    [SerializeField] Camera camera = null;
    [SerializeField] new Rigidbody rigidbody = null;
    [SerializeField] LineRenderer rope = null;
    [SerializeField] Transform attachPoint = null;
    [SerializeField] Transform rotationParent = null;

    const float TRAVEL_SPEED = 30f;
    const float RETRACT_SPEED = 10f;
    const float TRAVEL_ROTATION = 360f;

    Transform originalParent;
    Quaternion originalRotation;

    Vector3 target;
    float embedDistance = 0f;
    const float EMBED_EXTRA_SLACK_DISTANCE = 1f;
    const float PICKUP_RADIUS = 2f;

    enum State
    {
        Holstered,
        Firing,
        Embedded,
        Retracting,
    }
    State state = State.Holstered;

    void Start()
    {
        camera = Camera.main;

        originalParent = transform.parent;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        if (state != State.Holstered)
        {
            RenderRope();
        }

        if (state == State.Embedded)
        {
            var distance = Vector3.Distance(originalParent.position, transform.position);
            if (distance >= embedDistance || distance <= PICKUP_RADIUS)
            {
                StartCoroutine(Retract());
            }
        }
    }

    public void Action()
    {
        if (state == State.Holstered)
        {
            StartCoroutine(Fire());
        }
        else if (state == State.Embedded)
        {
            StartCoroutine(Retract());
        }
    }

    IEnumerator Fire()
    {
        var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit)) yield break;

        state = State.Firing;

        transform.parent = null;
        target = hit.point;

        rope.enabled = true;

        while (!MoveToTarget(target, TRAVEL_SPEED))
        {
            rotationParent.Rotate(Vector3.forward, TRAVEL_ROTATION * Time.deltaTime, Space.Self);

            yield return null;
        }

        state = State.Embedded;

        embedDistance = Vector3.Distance(originalParent.position, target) + EMBED_EXTRA_SLACK_DISTANCE;
    }

    IEnumerator Retract()
    {
        state = State.Retracting;

        rigidbody.isKinematic = false;
        var retractionForce = (Vector3.up + transform.forward * -1f).normalized * 0.5f;
        rigidbody.AddForce(retractionForce, ForceMode.Impulse);

        yield return new WaitForSeconds(0.5f);

        rotationParent.localRotation = Quaternion.AngleAxis(180f, Vector3.up);

        rigidbody.isKinematic = true;

        while (!MoveToTarget(originalParent.position, RETRACT_SPEED))
        {
            yield return null;
        }

        state = State.Holstered;

        transform.parent = originalParent;
        transform.localPosition = Vector3.zero;
        transform.localRotation = originalRotation;

        rotationParent.localRotation = Quaternion.identity;

        rope.enabled = false;
    }

    bool MoveToTarget(Vector3 target, float speed)
    {
        transform.LookAt(target);

        var distanceToTarget = Vector3.Distance(transform.position, target);
        if (distanceToTarget <= 0.1f)
        {
            transform.position = target;
            return true;
        }
        else
        {
            var movement = Vector3.ClampMagnitude(transform.forward * speed * Time.deltaTime, distanceToTarget);
            transform.position += movement;
            return false;
        }
    }

    void RenderRope()
    {
        rope.SetPosition(0, originalParent.position);
        rope.SetPosition(1, attachPoint.position);

        var slack = 1f - Vector3.Distance(transform.position, originalParent.position) / embedDistance;
        rope.startColor = Color.white * slack;
    }
}
