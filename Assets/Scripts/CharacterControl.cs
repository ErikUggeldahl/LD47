using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(PlayerInput))]
public class CharacterControl : MonoBehaviour
{
    [SerializeField] CharacterController controller = null;
    [SerializeField] PlayerInput input = null;
    [SerializeField] Animator animator = null;
    [SerializeField] Transform model = null;
    [SerializeField] Dagger daggerLeft = null;
    [SerializeField] Dagger daggerRight = null;
    [SerializeField] Transform leftHand = null;
    [SerializeField] Transform rightHand = null;
    [SerializeField] CharacterAnimationEventReceiver animationEvents = null;

    const float SPEED = 5f;
    const float ROTATION_SLERP_SPEED = 5f;
    const float MIN_SPEED_TO_ANIMATE = 0.3f;
    readonly Vector3 GRAVITY = Physics.gravity;
    readonly Quaternion ISOMETRIC_ROTATION = Quaternion.AngleAxis(45f, Vector3.up);

    InputAction moveAction;

    const float POP_FORCE = 20f;
    const float GROUND_DRAG = 5f;
    const float AIR_DRAG = 1f;
    Vector3 popVelocity = Vector3.zero;

    enum AnimationState
    {
        Idle = 0,
        Running = 1,
        DoubleThrow = 2,
        ThrowRight = 3,
        ThrowLeft = 4,
    }

    void Start()
    {
        moveAction = input.actions["Move"];

        input.actions["FireLeft"].performed += (InputAction.CallbackContext ctx) => FireOne(daggerLeft, AnimationState.ThrowLeft);
        input.actions["FireRight"].performed += (InputAction.CallbackContext ctx) => FireOne(daggerRight, AnimationState.ThrowRight);
        input.actions["FireBoth"].performed += FireBoth;

        animationEvents.GrabDaggers += GrabDaggers;
        animationEvents.ReleaseDaggers += ReleaseDaggers;
        animationEvents.ThrowComplete += ThrowComplete;
    }

    void Update()
    {
        var velocity = new Vector3(0f, GRAVITY.y, 0f) + popVelocity;

        var moveInput = moveAction.ReadValue<Vector2>();
        var moveVelocity = ISOMETRIC_ROTATION * new Vector3(moveInput.x, 0f, moveInput.y).normalized * SPEED;
        velocity += moveVelocity;

        controller.Move(velocity * Time.deltaTime);

        if (moveVelocity.sqrMagnitude > 0)
        {
            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(moveVelocity), ROTATION_SLERP_SPEED * Time.deltaTime);
        }

        if (animator.GetInteger("State") == (int)AnimationState.Running && controller.velocity.sqrMagnitude <= MIN_SPEED_TO_ANIMATE)
        {
            animator.SetInteger("State", (int)AnimationState.Idle);
        }
        else if (animator.GetInteger("State") == (int)AnimationState.Idle && controller.velocity.sqrMagnitude > MIN_SPEED_TO_ANIMATE)
        {
            animator.SetInteger("State", (int)AnimationState.Running);
        }
    }

    void FireOne(Dagger dagger, AnimationState animation)
    {
        if (dagger.State == Dagger.DaggerState.Holstered)
        {
            animator.SetInteger("State", (int)animation);
            dagger.WillGrab();
        }
        else
        {
            dagger.Action();
        }
    }

    void FireBoth(InputAction.CallbackContext ctx)
    {
        if (daggerLeft.State == Dagger.DaggerState.Embedded && daggerRight.State == Dagger.DaggerState.Embedded)
        {
            animator.SetTrigger("Popped");

            var toLeft = daggerLeft.transform.position - transform.position;
            var toRight = daggerRight.transform.position - transform.position;
            popVelocity = ((toLeft + toRight).normalized + Vector3.up).normalized * POP_FORCE;
            StartCoroutine(DragPopVelocity());

            daggerLeft.ActionBoth(daggerRight);
            daggerRight.ActionBoth(daggerLeft);
        }
        else if (daggerLeft.State == Dagger.DaggerState.Holstered && daggerRight.State == Dagger.DaggerState.Holstered)
        {
            animator.SetInteger("State", (int)AnimationState.DoubleThrow);

            daggerLeft.WillGrab();
            daggerRight.WillGrab();
        }
        else
        {
            daggerLeft.ActionBoth(daggerRight);
            daggerRight.ActionBoth(daggerLeft);
        }
    }

    void GrabDaggers(CharacterAnimationEventReceiver.WhichDagger which)
    {
        switch (which)
        {
            case CharacterAnimationEventReceiver.WhichDagger.Left: daggerLeft.Grab(rightHand); break;
            case CharacterAnimationEventReceiver.WhichDagger.Right: daggerRight.Grab(leftHand); break;
            case CharacterAnimationEventReceiver.WhichDagger.Both:
                daggerLeft.Grab(leftHand);
                daggerRight.Grab(rightHand);
                break;
        }
    }

    void ReleaseDaggers(CharacterAnimationEventReceiver.WhichDagger which)
    {
        switch (which)
        {
            case CharacterAnimationEventReceiver.WhichDagger.Left: daggerLeft.Action(); break;
            case CharacterAnimationEventReceiver.WhichDagger.Right: daggerRight.Action(); break;
            case CharacterAnimationEventReceiver.WhichDagger.Both:
                daggerLeft.ActionBoth(daggerRight);
                daggerRight.ActionBoth(daggerLeft);
                break;
        }
    }

    void ThrowComplete()
    {
        animator.SetInteger("State", (int)AnimationState.Idle);
    }

    IEnumerator DragPopVelocity()
    {
        while (popVelocity.sqrMagnitude > 0.1f)
        {
            var dragForce = controller.isGrounded ? GROUND_DRAG : AIR_DRAG;
            popVelocity = Vector3.Lerp(popVelocity, Vector3.zero, dragForce * Time.deltaTime);
            yield return null;
        }

        popVelocity = Vector3.zero;
    }
}
