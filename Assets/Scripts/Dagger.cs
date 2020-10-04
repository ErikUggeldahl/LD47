using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;

public class Dagger : MonoBehaviour
{
    [SerializeField] new Rigidbody rigidbody = null;
    [SerializeField] LineRenderer rope = null;
    [SerializeField] Transform attachPoint = null;
    [SerializeField] Transform rotationParent = null;
    [SerializeField] Collider interceptTrigger = null;

    const float TRAVEL_SPEED = 30f;
    const float RETRACT_SPEED = 10f;
    const float TRAVEL_ROTATION = 360f;
    const float RETRACT_POP_FORCE = 0.5f;
    const float RETRACT_PULL_FORCE = 5f;

    Transform originalParent;
    Quaternion originalRotation;

    new Camera camera = null;

    int mask;
    Vector3 target;

    public Rigidbody TargetRigidbody { get; private set; }
    float embedDistance = 0f;
    const float EMBED_EXTRA_SLACK_DISTANCE = 1f;
    const float PICKUP_RADIUS = 2f;

    public enum DaggerState
    {
        Holstered,
        WillGrab,
        Grabbed,
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

        mask = LayerMask.GetMask("Default", "World");
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
        if (State == DaggerState.Holstered || State == DaggerState.Grabbed)
        {
            StartCoroutine(Fire());
        }
        else if (State == DaggerState.Embedded)
        {
            StartCoroutine(Retract());
        }
    }

    public void WillGrab()
    {
        if (State != DaggerState.Holstered) return;
            
        State = DaggerState.WillGrab;
    }

    public void Grab(Transform hand)
    {
        if (State != DaggerState.WillGrab) return;

        State = DaggerState.Grabbed;

        transform.parent = hand;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void ActionBoth(Dagger other)
    {
        if (TargetRigidbody != null && other.TargetRigidbody == TargetRigidbody)
        {
            if (TargetRigidbody.tag == "Enemy")
            {
                TargetRigidbody.GetComponentInParent<Guard>().Explode();
            }

            var retractionForce = (Vector3.up + transform.forward * -1f).normalized * RETRACT_PULL_FORCE;
            TargetRigidbody.AddForce(retractionForce, ForceMode.Impulse);
        }

        Action();
    }

    IEnumerator Fire()
    {
        var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, Mathf.Infinity, mask) || Vector3.Distance(hit.point, transform.position) <= PICKUP_RADIUS)
        {
            State = DaggerState.Holstered;
            yield break;
        }

        State = DaggerState.Firing;

        transform.parent = null;
        target = hit.point;

        rope.enabled = true;

        interceptTrigger.enabled = true;

        while (interceptTrigger.enabled && !MoveToTarget(target, TRAVEL_SPEED))
        {
            rotationParent.Rotate(Vector3.forward, TRAVEL_ROTATION * Time.deltaTime, Space.Self);

            yield return null;
        }

        State = DaggerState.Embedded;

        interceptTrigger.enabled = false;

        if (TargetRigidbody == null)
        {
            TargetRigidbody = hit.rigidbody;
        }

        if (TargetRigidbody && TargetRigidbody.tag == "Enemy")
        {
            TargetRigidbody.GetComponentInParent<Guard>().Die();
        }

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

    void OnTriggerEnter(Collider other)
    {
        interceptTrigger.enabled = false;

        TargetRigidbody = other.attachedRigidbody;
    }
}
