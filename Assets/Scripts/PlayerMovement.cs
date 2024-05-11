using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerMovement : NetworkBehaviour
{
    private CharacterController _controller;
    //private Animator _animator;

    public float PlayerSpeed = 2f;

    public float JumpForce = 5f;
    public float GravityValue = -9.81f;

    private Vector3 _velocity;
    private bool _jumpPressed;
    private Vector3 move;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Camera.main.GetComponent<CameraFollow>()?.SetTarget(transform);
        }
    }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        //_animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horizontalInput, 0, verticalInput) * PlayerSpeed * Runner.DeltaTime;

        //_animator.SetFloat("Speed", move.magnitude);

        _jumpPressed = Input.GetButtonDown("Jump");
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false)
        {
            return;
        }
        if (_controller.isGrounded)
        {
            _velocity = new Vector3(0, -1, 0);
        }

        _velocity.y += GravityValue * Runner.DeltaTime;

        if (_jumpPressed && _controller.isGrounded)
        {
            _velocity.y += JumpForce;
            //_animator.SetTrigger("Jump");
        }

        _controller.Move(move + _velocity * Runner.DeltaTime);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        // Reset jump input
        _jumpPressed = false;
    }
}
