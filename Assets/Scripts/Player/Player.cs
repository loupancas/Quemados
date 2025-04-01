using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Unity.VisualScripting;
using System.Linq;

public class Player : NetworkBehaviour
{
    public static Player LocalPlayer { get; set; }
    //public static bool ControlsEnabled = false;
    [Header("Stats")]
    [SerializeField] public float _speed = 3;
    [SerializeField] public float _jumpForce = 5;
    [SerializeField] private float _groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _shootDamage = 25f;
    [SerializeField] private LayerMask _shootLayer;
    private Rigidbody _rgbd;
    public Animator _animator;
    private bool _isGrounded = true;
    //private int fadeTime = 5;
    public float _xAxi;
    public float _yAxi;
    public bool _jumpPressed;
    private bool _shootPressed;
    public float _defaultSpeed;
    public float _defaultJump; 
    public Camera Camera;
    [Networked]
    public bool HasBall { get; set; }
    [Networked, OnChangedRender(nameof(ChangeColor))] public Color _teamColor { get; set; }
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;
    private bool hasTeam = false;
    //[Networked] public NetworkObject HeldBall { get; set; }
    #region Networked Color Change

    [Networked, OnChangedRender(nameof(OnNetColorChanged))]
    Color NetworkedColor { get; set; }


    void OnNetColorChanged() => GetComponentInChildren<Renderer>().material.color = NetworkedColor;
   
    #endregion

    #region Networked Health Change

    [Networked, OnChangedRender(nameof(OnNetHealthChanged))]
    private float NetworkedHealth { get; set; } = 3;
    void OnNetHealthChanged() => Debug.Log($"Life = {NetworkedHealth}");

    #endregion

    public event Action<float> OnMovement = delegate {  };
    public event Action OnShooting = delegate {  };

    [SerializeField] private Transform _ballSpawnTransform;
    [SerializeField] WeaponHandler _weaponHandler;
   
    [SerializeField] BallPickUp _ballpickup;
    //[SerializeField] private Ball2 _ballPrefab;
    public MeshRenderer ball;
    private void Awake()
    {
        //if (_ballPrefab != null)
        //{
        //    _ballPrefab.GetComponentInChildren<MeshRenderer>().enabled = false;
        //}

     

    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            LocalPlayer = this;
            NetworkedColor = GetComponentInChildren<Renderer>().material.color;
            Debug.Log("Local Player");
            Camera = Camera.main;
            Camera.GetComponent<ThirdPersonCamera>().Target = transform;
            _rgbd = GetComponent<Rigidbody>();
            _defaultJump = _jumpForce;
            _defaultSpeed = _speed;
            ball.GetComponent<MeshRenderer>().enabled = false;

            HasBall = false;
           

            _weaponHandler = GetComponent<WeaponHandler>();
          
           

            StartCoroutine(WaitInit());

        }
        else
        {
            Debug.Log("No State Authority");
            //SynchronizeProperties();
            //GameManager.Instance.AddNewPlayer(Runner.ActivePlayers.Count(), this);
            //Debug.Log("index" + Runner.ActivePlayers.Count());
            //if (GameManager.Instance.Runner.ActivePlayers.Count() == 2)
            //{
            //    Debug.Log("Start Countdown");
            //    //StartCoroutine(UIManager.instance.StartCountdown(3));

            //}
        }
    }

    void SynchronizeProperties()
    {
        OnNetColorChanged();
    }

    public static void EnablePlayerControls()
    {
        //ControlsEnabled = true;
    }
    
    void Update()
    {
        if (!HasStateAuthority ) return;

       //heckGrounded();
   
       _xAxi = Input.GetAxis("Horizontal");
        _yAxi = Input.GetAxis("Vertical");

       //Debug.Log(moveInputVector);

        if (Input.GetKeyDown(KeyCode.Space) )
        {
            _jumpPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            //TryPickupBall();


            _ballpickup.RPC_PickUp(this);
            //ball.GetComponent<NetworkObject>().AssignInputAuthority(Runner.LocalPlayer);
            ball.GetComponent<MeshRenderer>().enabled = true;

            // HasBall = true;

        }

        if (Input.GetMouseButtonDown(0) && HasBall)
        {
            RPC_FireAndDropBall();
            ball.GetComponent<MeshRenderer>().enabled = false;

        }
        else
        {
            Debug.Log("No Ball");
        }    
    }


    

    #region
    //private void TryPickupBall()
    //{
    //    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);
    //    foreach (var hitCollider in hitColliders)
    //    {
    //        if (hitCollider.CompareTag("Ball"))
    //        {
    //            NetworkObject ballNetworkObject = hitCollider.GetComponent<NetworkObject>();
    //            BallPickUp ballPickUp = hitCollider.GetComponent<BallPickUp>();
    //            if (ballNetworkObject && !ballNetworkObject.HasInputAuthority && ballPickUp)
    //            {
    //                ballPickUp.RPC_PickUp(this);
    //                ballNetworkObject.AssignInputAuthority(Runner.LocalPlayer);
    //                HeldBall = ballNetworkObject;
    //                //if (_ballPrefab != null && !_ballPrefab.GetComponentInChildren<MeshRenderer>().enabled)
    //                //{
    //                //    HasBall = true;
    //                //    _ballPrefab.GetComponentInChildren<MeshRenderer>().enabled = true;
    //                //}
    //                break;
    //            }
    //        }
    //    }
    //}

    //private void ThrowBall()
    //{
    //    if (HeldBall.IsValid)
    //    {
    //        Vector3 throwDirection = transform.forward;
    //        Ball2 ball = HeldBall.GetComponent<Ball2>();
    //        if (ball != null)
    //        {
    //            ball.RpcThrow(throwDirection);
    //            HeldBall.GetComponent<NetworkObject>().RemoveInputAuthority();
    //            HeldBall = default;
    //            //if (_ballPrefab != null && _ballPrefab.GetComponentInChildren<MeshRenderer>().enabled)
    //            //{
    //            //    HasBall = false;
    //            //    _ballPrefab.GetComponentInChildren<MeshRenderer>().enabled = false;
    //            //}
    //        }
    //    }
    //}
    #endregion




    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_FireAndDropBall()
    {
        if (!HasBall) return;
        _weaponHandler.Fire();
        DropBall();
    }


    private void DropBall()
    {
        HasBall = false;

        if (Runner.IsServer)
        {
            FindObjectOfType<BallPickUp>().Drop(this);

        }
        else
        {
            RPC_DropBallOnServer();
        }

    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_DropBallOnServer()
    {
        FindObjectOfType<BallPickUp>().Drop(this);
    }




    private void ChangeColor()
    {
        _meshRenderer.material.color = _teamColor;
    }



    public override void FixedUpdateNetwork()
    {
       
        if (!HasStateAuthority) return;

        //MOVIMIENTO
        Movement();


      

    }

   

    public void Movement()
    {
        Vector3 camForward = Camera.transform.forward;
        Vector3 camRight = Camera.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = (camForward * _yAxi + camRight * _xAxi).normalized;

        // SALTO
        if (_jumpPressed)
        {
            Jump();
            _jumpPressed = false;
        }

        // DISPARO
        //if (_shootPressed)
        //{
        //    //RaycastShoot();
        //    _shootPressed = false;
        //}

        // MOVIMIENTO
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
            _rgbd.velocity += direction * (_speed * 10 * Runner.DeltaTime);

            var velocity = Vector3.ClampMagnitude(_rgbd.velocity, _speed);
            velocity.y = _rgbd.velocity.y;
            _rgbd.velocity = velocity;
            OnMovement(_rgbd.velocity.magnitude);
        }
        else
        {
            var velocity = _rgbd.velocity;
            velocity.x = 0;
            velocity.z = 0;
            _rgbd.velocity = velocity;
        }
    }

    void Jump()
    {
        _rgbd.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        _animator.SetBool("Jumping", true);
        // playerView.isRunning(true);
    }

   
    #region POWER UP
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<JumpPower>())
        {
            Debug.Log("PowerUp Jump");
            JumpPower jumpPower = other.gameObject.GetComponent<JumpPower>();
            jumpPower.GetPower();
            _jumpForce = jumpPower.modifier;
            ChangeColorRecursively(transform, Color.red);
            StartCoroutine(NormalizeStats(jumpPower.fadeTime));
        }
        if (other.gameObject.GetComponent<SpeedPower>())
        {
            Debug.Log("PowerUp Speed");
            SpeedPower speedPower = other.gameObject.GetComponent<SpeedPower>();
            speedPower.GetPower();
            _speed = speedPower.modifier;
            ChangeColorRecursively(transform, Color.blue);
            StartCoroutine(NormalizeStats(speedPower.fadeTime));
        }
    }   

    IEnumerator NormalizeStats(float fadeTime)
    {
        yield return new WaitForSeconds(fadeTime);
        _speed = _defaultSpeed;
        _jumpForce = _defaultJump;
        ChangeColorRecursively(transform, Color.white);
    }
    #endregion
    void ChangeColorRecursively(Transform parent, Color color)
    {
        foreach (Transform child in parent)
        {
            SkinnedMeshRenderer renderer = child.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                foreach (var material in renderer.materials)
                {
                    material.color = color;
                }
            }
            
            ChangeColorRecursively(child, color);
        }
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
            //Dead();
            //SetLoseScreenRPC();
            //UIManager.instance.SetLoseScreen();
            RoomM.Instance.RPC_PlayerWin(Runner.LocalPlayer);
        }
        else
        {
            Debug.Log("Player Hit");
        }

    }

    void Dead()
    {
        Runner.Despawn(Object);
    }

    //private void SetLoseScreenRPC()
    //{
    //    UIManager.instance.SetLoseScreen();
    //}

    private IEnumerator WaitInit()
    {
        yield return new WaitForSeconds(1);

        switch (Runner.LocalPlayer.PlayerId)
        {
            case 1:
                _teamColor = Color.red;
                break;
            case 2:
                _teamColor = Color.blue;
                break;
            default:
                _teamColor = Color.yellow;
                break;
        }
    }
}
