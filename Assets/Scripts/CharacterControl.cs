using System.Collections;
using System.Collections.Generic;
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
    readonly Vector3 GRAVITY = Physics.gravity;
    readonly Quaternion ISOMETRIC_ROTATION = Quaternion.AngleAxis(45f, Vector3.up);

    InputAction moveAction;

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
    }

    void Update()
    {
        var velocity = new Vector3(0f, GRAVITY.y, 0f);

        var moveInput = moveAction.ReadValue<Vector2>();
        var moveVelocity = ISOMETRIC_ROTATION * new Vector3(moveInput.x, 0f, moveInput.y).normalized * SPEED;
        velocity += moveVelocity;

        controller.Move(velocity * Time.deltaTime);

        if (moveVelocity.sqrMagnitude > 0)
        {
            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(moveVelocity), 5f * Time.deltaTime);
        }

        if (controller.velocity.sqrMagnitude <= 0.3f)
        {
            animator.SetInteger("State", (int)AnimationState.Idle);
        }
        else if (controller.velocity.sqrMagnitude > 0.3f)
        {
            animator.SetInteger("State", (int)AnimationState.Running);
        }
    }
}
