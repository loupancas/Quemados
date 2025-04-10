using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using System.Resources;
using UnityEditor;
using Fusion.Addons.Physics;
public class Player : NetworkBehaviour
{
    public static Player LocalPlayer { get; set; }
    public PlayerDataNetworked _playerDataNetworked = null;
    [SerializeField] private float _respawnDelay = 2.0f;
    private ChangeDetector _changeDetector;
    //GameController _gameController;
    //public static bool ControlsEnabled = false;
    [Header("Stats")]
    [SerializeField] public float _speed = 3;
    [SerializeField] public float _jumpForce = 5;
    [SerializeField] private float _playerDamageRadius = 3f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _shootLayer;
    [SerializeField] private LayerMask _ballCollisionLayer;
    [Networked] private NetworkBool IsAlive { get; set; }
    [Networked] private TickTimer RespawnTimer { get; set; }
    private Rigidbody _rgbd;
    public Animator _animator;
    [Header("Inputs")]
    public float _xAxi;
    public float _yAxi;
    public bool _jumpPressed;
    private bool _shootPressed;
    public float _defaultSpeed;
    public float _defaultJump; 
    public Camera Camera;
    [Header("Ball")]
    [SerializeField] private Ball2 _currentBall;
    [SerializeField] private BallBehaviour _ball;
    [Networked] public bool HasBall { get; set; }
    [SerializeField] private Transform ballSpawnPoint;
    private Collider[] _hits = new Collider[1];
 
    [Networked, OnChangedRender(nameof(ChangeColor))] public Color _teamColor { get; set; }
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;
    #region Networked Color Change

    [Networked, OnChangedRender(nameof(OnNetColorChanged))]
    Color NetworkedColor { get; set; }


    void OnNetColorChanged() => GetComponentInChildren<Renderer>().sharedMaterial.color = NetworkedColor;
   
    #endregion

    #region Networked Health Change

    [Networked, OnChangedRender(nameof(OnNetHealthChanged))]
    private float NetworkedHealth { get; set; } = 3;
    void OnNetHealthChanged() => Debug.Log($"Life = {NetworkedHealth}");

    #endregion

    public event Action<float> OnMovement = delegate {  };
    public event Action OnShooting = delegate {  };

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            LocalPlayer = this;
            
            
            NetworkedColor = GetComponentInChildren<Renderer>().sharedMaterial.color;
            Debug.Log("Player spawned");
            Camera = Camera.main;
            Camera.GetComponent<ThirdPersonCamera>().Target = transform;
            _rgbd = GetComponent<Rigidbody>();
            _defaultJump = _jumpForce;
            _defaultSpeed = _speed;
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            HasBall = false;
            IsAlive = true;
            if (!Object.HasInputAuthority)
            {
                Object.AssignInputAuthority(Runner.LocalPlayer);
            }
            var playerRef = Object.InputAuthority;
            _meshRenderer.sharedMaterial.color = GetColor(playerRef.PlayerId);

        }

    }

    public void SetPlayerColor(Color color)
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.sharedMaterial.color = color;
        }
    }

    void Update()
    {
        if (!HasStateAuthority ) return;

   
       _xAxi = Input.GetAxis("Horizontal");
        _yAxi = Input.GetAxis("Vertical");


        if (Input.GetKeyDown(KeyCode.Space) )
        {
            _jumpPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupBall();
        

        }

        if (Input.GetMouseButtonDown(0) && HasBall)
        {
            Fire();
           
        }
         
    }




    private void TryPickupBall()
    {
        Debug.Log("TryPickupBall called");
        if (HasBall)
        {
            Debug.Log("Player already has a ball");
            return;
        }

        if (!Object.HasInputAuthority)
        {
            Debug.Log("Player does not have input authority");
            return;
        }
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f, _ballCollisionLayer);
        Debug.Log($"Number of colliders found: {hitColliders.Length}");

        foreach (var hitCollider in hitColliders)
        {
            BallPickUp ballPickUp = hitCollider.GetComponent<BallPickUp>();
            if (ballPickUp != null)
            {
                Debug.Log("BallPickUp component found, calling RPC_PickUpBall");
                RPC_PickUpBall(ballPickUp.Object);
                break;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PickUpBall(NetworkObject ballObject)
    {
        Debug.Log("RPC_PickUpBall called");
        BallPickUp ballPickUp = ballObject.GetComponent<BallPickUp>();
        if (ballPickUp != null)
        {
            ballPickUp.PickUp(this);
            HasBall = true;
            MeshRenderer ballRenderer = ballObject.GetComponent<MeshRenderer>();
            if (ballRenderer != null)
            {
                ballRenderer.enabled = false;
            }
           

        }
    }

 


    private void Fire()
    {
        var ball = Runner.Spawn(_ball, ballSpawnPoint.position, _rgbd.rotation, Object.InputAuthority);
        ball.GetComponent<BallBehaviour>().Initialize(this);
        HasBall = false;
    }

    // Spawns a bullet which will be travelling in the direction the spaceship is facing
    private void SpawnBullet()
    {
        //if (_shootCooldown.ExpiredOrNotRunning(Runner) == false) return;

        Runner.Spawn(_ball, ballSpawnPoint.position, _rgbd.rotation, Object.InputAuthority);
        Debug.Log("Ball Spawned");

        //_shootCooldown = TickTimer.CreateFromSeconds(Runner, _delayBetweenShots);
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

        if (IsAlive && HasHitBall() && GameController.Singleton.GameIsRunning)
        {
            PlayerWasHit();
        }


    }

    private bool HasHitBall()
    {
        var count = Runner.GetPhysicsScene().OverlapSphere(_rgbd.position, _playerDamageRadius, _hits,
            _ballCollisionLayer.value, QueryTriggerInteraction.UseGlobal);

        if (count <= 0)
            return false;

        var ballBehaviour = _hits[0]?.GetComponent<BallBehaviour>();
        if (ballBehaviour == null)
            return false;
        return ballBehaviour.OnBallHit();
    }

    private void PlayerWasHit()
    {
        if (!HasStateAuthority) return;
        Debug.Log("Player was hit by a ball");
      

        if (_playerDataNetworked.Lives > 1)
        {
            RespawnTimer = TickTimer.CreateFromSeconds(Runner, _respawnDelay);
            _playerDataNetworked.SubtractLife();
            var ballBehaviour = _hits[0].GetComponent<BallBehaviour>();
            if (ballBehaviour != null && ballBehaviour.ThrowingPlayer != null)
            {
                ballBehaviour.ThrowingPlayer._playerDataNetworked.AddToScore(1);
            }

        }
        else
        {
            RespawnTimer = default;
            IsAlive = false;
            SetLoseScreenRPC();
            UIManager.instance.SetLoseScreen();
            RoomM.Instance.RPC_PlayerWin(Runner.LocalPlayer);
        }


    }


    #region movement
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
            _animator.SetFloat("Speed", _rgbd.velocity.magnitude);
        }
        else
        {
            var velocity = _rgbd.velocity;
            velocity.x = 0;
            velocity.z = 0;
            _rgbd.velocity = velocity;
            _animator.SetFloat("Speed", 0);
        }
    }

    void Jump()
    {
        _rgbd.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        _animator.SetBool("Jumping", true);
        // playerView.isRunning(true);
    }
    #endregion

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
                foreach (var material in renderer.sharedMaterials)
                {
                    material.color = color;
                }
            }
            
            ChangeColorRecursively(child, color);
        }
    }

    //[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    //public void RPC_TakeDamage(float dmg)
    //{
    //    Local_TakeDamage(dmg);
    //}

    //public void Local_TakeDamage(float dmg)
    //{
    //    NetworkedHealth -= dmg;

    //    if (NetworkedHealth <= 0)
    //    {
    //        //Dead();
    //        //SetLoseScreenRPC();
    //        //UIManager.instance.SetLoseScreen();
    //        RoomM.Instance.RPC_PlayerWin(Runner.LocalPlayer);
    //    }
    //    else
    //    {
    //        Debug.Log("Player Hit");
    //    }

    //}

    //void Dead()
    //{
    //    Runner.Despawn(Object);
    //}

    private void SetLoseScreenRPC()
    {
        UIManager.instance.SetLoseScreen();
    }


    public static Color GetColor(int player)
    {
        switch (player % 8)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.grey;
            case 6: return Color.magenta;
            case 7: return Color.white;
        }

        return Color.black;
    }

}
