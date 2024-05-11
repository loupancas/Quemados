using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Player : NetworkBehaviour
{
    public static Player LocalPlayer { get; private set; }

    [Header("Stats")]
    [SerializeField] private float _speed = 3;
    [SerializeField] private float _jumpForce = 5;
    [SerializeField] private float _shootDamage = 25f;
    [SerializeField] private LayerMask _shootLayer;

    public Rigidbody _rgbd;
    
    private float _xAxi;
    private float _yAxi;
    private bool _jumpPressed;
    private bool _shootPressed;

    #region Networked Color Change
    
    [Networked, OnChangedRender(nameof(OnNetColorChanged))]
    Color NetworkedColor { get; set; }

    void OnNetColorChanged() => GetComponentInChildren<Renderer>().material.color = NetworkedColor;
    
    // void OnNetColorChanged()
    // {
    //     GetComponentInChildren<Renderer>().material.color = NetworkedColor;
    // }
    
    #endregion

    #region Networked Health Change

    [Networked, OnChangedRender(nameof(OnNetHealthChanged))]
    private float NetworkedHealth { get; set; } = 100;
    void OnNetHealthChanged() => Debug.Log($"Life = {NetworkedHealth}");
    
    // void OnNetHealthChanged()
    // {
    //     Debug.Log($"Life = {NetworkedHealth}");
    // }

    #endregion
    
    //public event Action<float> OnMovement = delegate {  };
    //public event Action OnShooting = delegate {  };

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            NetworkedColor = GetComponentInChildren<Renderer>().material.color;
            
            Camera.main.GetComponent<CameraFollow>()?.SetTarget(transform);
            _rgbd = GetComponent<Rigidbody>();
        }
        else
        {
            SynchronizeProperties();
        }
    }

    void SynchronizeProperties()
    {
        OnNetColorChanged();
    }
    
    void Update()
    {
        if (!HasStateAuthority) return;
        
        _xAxi = Input.GetAxis("Horizontal");
        _yAxi = Input.GetAxis("Vertical");

        

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //_shootPressed = true;
            _jumpPressed = true;
        }
        if (Input.GetMouseButtonDown(1))
        {
            _shootPressed = true;
            
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            NetworkedColor = Color.red;
        }
    }

    
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        
        //transform.position += Vector3.right * (_xAxi * _speed * Runner.DeltaTime);
        
        //_rgbd.MovePosition(_rgbd.position + Vector3.right * (_xAxi * _speed * Runner.DeltaTime));
              

        Movement();
        
        
    }

    void Movement()
    {
        //SALTO
        if (_jumpPressed)
        {
            Jump();
            _jumpPressed = false;
        }

        if (_shootPressed)
        {
            RaycastShoot();

            _shootPressed = false;
        }

        //MOVIMIENTO
        if (_xAxi != 0)
        {
            transform.forward = Vector3.right * Mathf.Sign((_xAxi));

            _rgbd.velocity += Vector3.right * (_xAxi * _speed * 10 * Runner.DeltaTime);

            if (Mathf.Abs(_rgbd.velocity.x) > _speed)
            {
                var velocity = Vector3.ClampMagnitude(_rgbd.velocity, _speed);
                velocity.y = _rgbd.velocity.y;

                _rgbd.velocity = velocity;
            }
        }
        else if (_yAxi != 0)
        {
            transform.forward = Vector3.forward * Mathf.Sign((_yAxi));

            _rgbd.velocity += Vector3.forward * (_yAxi * _speed * 10 * Runner.DeltaTime);

            if (Mathf.Abs(_rgbd.velocity.z) > _speed)
            {
                var velocity = Vector3.ClampMagnitude(_rgbd.velocity, _speed);
                velocity.y = _rgbd.velocity.y;

                _rgbd.velocity = velocity;
            }
        }
        else
        {
            var velocity = _rgbd.velocity;
            velocity.x = 0;
            _rgbd.velocity = velocity;
        }

       
    }

    void Jump()
    {
        _rgbd.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
    }

    void RaycastShoot()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.red, 1f);
        
        if (Runner.GetPhysicsScene().Raycast(transform.position, transform.forward, out var raycastHit,100, _shootLayer ))
        {
            Debug.Log(raycastHit.transform.name);
            
            var enemy = raycastHit.transform.GetComponent<Player>();
            
            enemy.RPC_TakeDamage(_shootDamage);
        }

        //OnShooting();
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(float dmg)
    {
        Local_TakeDamage(dmg);
    }

    public void Local_TakeDamage(float dmg)
    {
        NetworkedHealth -= dmg;

        if (NetworkedHealth <= 0)
        {
            Dead();
        }
    }

    void Dead()
    {
        Runner.Despawn(Object);
    }

    private void SetVictoryScreenRPC(Player player)
    {
        UIManager.instance.SetVictoryScreen(player);
    }

}
