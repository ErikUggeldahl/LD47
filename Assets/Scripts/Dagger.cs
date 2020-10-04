using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;

public class Dagger : MonoBehaviour
{
    [SerializeField] new Camera camera = null;
    [SerializeField] new Rigidbody rigidbody = null;
    [SerializeField] LineRenderer rope = null;
    [SerializeField] Transform attachPoint = null;
    [SerializeField] Transform rotationParent = null;

    const float TRAVEL_SPEED = 30f;
    const float RETRACT_SPEED = 10f;
    const float TRAVEL_ROTATION = 360f;
    const float RETRACT_POP_FORCE = 0.5f;
    const float RETRACT_PULL_FORCE = 5f;

    Transform originalParent;
    Quaternion originalRotation;

    Vector3 target;

    public Rigidbody TargetRigidbody { get; private set; }
    float embedDistance = 0f;
    const float EMBED_EXTRA_SLACK_DISTANCE = 1f;
    const float PICKUP_RADIUS = 2f;

    public enum DaggerState
    {
        Holstered,
        Firing,
        Embedded,
        Retracting,
    }
    public DaggerState State { get; private set; } = DaggerState.Holstered;

    void Start()
    {
        camera = Camera.main;

        originalParent = transform.parent;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        if (State != DaggerState.Holstered)
        {
            RenderRope();
        }

        if (State == DaggerState.Embedded)
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
        if (State == DaggerState.Holstered)
        {
            StartCoroutine(Fire());
        }
        else if (State == DaggerState.Embedded)
        {
            StartCoroutine(Retract());
        }
    }

    public void ActionBoth(Dagger other)
    {
        if (TargetRigidbody != null && other.TargetRigidbody == TargetRigidbody)
        {
            var retractionForce = (Vector3.up + transform.forward * -1f).normalized * RETRACT_PULL_FORCE;
            TargetRigidbody.AddForce(retractionForce, ForceMode.Impulse);
        }

        Action();
    }

    IEnumerator Fire()
    {
        var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit)) yield break;

        State = DaggerState.Firing;

        transform.parent = null;
        target = hit.point;

        rope.enabled = true;

        while (!MoveToTarget(target, TRAVEL_SPEED))
        {
            rotationParent.Rotate(Vector3.forward, TRAVEL_ROTATION * Time.deltaTime, Space.Self);

            yield return null;
        }

        State = DaggerState.Embedded;

        TargetRigidbody = hit.rigidbody;

        embedDistance = Vector3.Distance(originalParent.position, target) + EMBED_EXTRA_SLACK_DISTANCE;
    }

    IEnumerator Retract()
    {
        State = DaggerState.Retracting;

        target = Vector3.zero;
        TargetRigidbody = null;

        rigidbody.isKinematic = false;
        var retractionForce = (Vector3.up + transform.forward * -1f).normalized * RETRACT_POP_FORCE;
        rigidbody.AddForce(retractionForce, ForceMode.Impulse);

        yield return new WaitForSeconds(0.5f);

        rotationParent.localRotation = Quaternion.AngleAxis(180f, Vector3.up);

        rigidbody.isKinematic = true;

        while (!MoveToTarget(originalParent.position, RETRACT_SPEED))
        {
            yield return null;
        }

        State = DaggerState.Holstered;

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
