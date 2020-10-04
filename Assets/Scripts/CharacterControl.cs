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
    }

    void Start()
    {
        moveAction = input.actions["Move"];

        input.actions["FireLeft"].performed += (InputAction.CallbackContext ctx) => daggerLeft.Action();
        input.actions["FireRight"].performed += (InputAction.CallbackContext ctx) => daggerRight.Action();
        input.actions["FireBoth"].performed += FireBoth;
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

        if (controller.velocity.sqrMagnitude <= MIN_SPEED_TO_ANIMATE)
        {
            animator.SetInteger("State", (int)AnimationState.Idle);
        }
        else if (controller.velocity.sqrMagnitude > MIN_SPEED_TO_ANIMATE)
        {
            animator.SetInteger("State", (int)AnimationState.Running);
        }
    }

    void FireBoth(InputAction.CallbackContext ctx)
    {
        if (daggerLeft.State == Dagger.DaggerState.Embedded && daggerRight.State == Dagger.DaggerState.Embedded)
        {
            var toLeft = daggerLeft.transform.position - transform.position;
            var toRight = daggerRight.transform.position - transform.position;
            popVelocity = ((toLeft + toRight).normalized + Vector3.up).normalized * POP_FORCE;
            StartCoroutine(DragPopVelocity());
        }

        daggerLeft.ActionBoth(daggerRight);
        daggerRight.ActionBoth(daggerLeft);
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
