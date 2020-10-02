using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CharacterControl : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [SerializeField] PlayerInput input;

    const float SPEED = 5f;
    readonly Vector3 GRAVITY = Physics.gravity;
    readonly Quaternion ISOMETRIC_ROTATION = Quaternion.AngleAxis(45f, Vector3.up);

    InputAction moveAction;
    

    void Start()
    {
        moveAction = input.actions["move"];
    }

    void Update()
    {
        var velocity = new Vector3(0f, GRAVITY.y, 0f);

        var moveInput = moveAction.ReadValue<Vector2>();
        var moveVelocity = ISOMETRIC_ROTATION * new Vector3(moveInput.x, 0f, moveInput.y).normalized * SPEED;
        velocity += moveVelocity;

        controller.Move(velocity * Time.deltaTime);
    }
}
