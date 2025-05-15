using System;
using System.Collections;
using UnityEngine;
using Fusion;


public class Player : NetworkBehaviour
{
    public static Player LocalPlayer { get; set; }
    public Inventario inventario;
    public PlayerDataNetworked _playerDataNetworked = null;
    [SerializeField] private float _respawnDelay = 2.0f;
    private ChangeDetector _changeDetector;
    private int _localPlayerId;
    [Header("Stats")] [SerializeField] public float _speed = 3;
    [SerializeField] public float _jumpForce = 5;
    [SerializeField] private float _playerDamageRadius = 3f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _shootLayer;
    [SerializeField] private LayerMask _ballCollisionLayer;
    [Networked] private NetworkBool IsAlive { get; set; }
    [Networked] private TickTimer RespawnTimer { get; set; }
    private Rigidbody _rgbd;
    public Animator _animator;
    [Header("Inputs")] public float _xAxi;
    public float _yAxi;
    public bool _jumpPressed;
    private bool _shootPressed;
    public float _defaultSpeed;
    public float _defaultJump;
    public Camera Camera;
    [SerializeField] public string _name;
    //[FormerlySerializedAs("_ball")] [Header("Ball")] [SerializeField]

   [SerializeField] private BallBehaviour _inGameBall;

    [SerializeField] private BallBehaviour _prefabBall;

    [SerializeField] private BallPickUp _ballPickUp;
    [Networked] public bool HasBall { get; set; }


    [Networked] public bool victim { get; set; }

    [Networked] public bool sniper { get; set; }

    [SerializeField] private bool _dmgApplied;

    public delegate void HasBallChangeHandler(bool hasBall);

    public delegate void BallThrownHandler();

    [SerializeField] private Transform ballSpawnPoint;
    private Collider[] _hits = new Collider[1];
    private PlayerManager _playerManager;
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;

    #region Networked Color Change

    [Networked, OnChangedRender(nameof(OnNetColorChanged))]
    Color NetworkedColor { get; set; }


    void OnNetColorChanged() => GetComponentInChildren<Renderer>().sharedMaterial.color = NetworkedColor;

    #endregion

    #region Networked Health Change

    [Networked, OnChangedRender(nameof(OnNetHealthChanged))]
    private float NetworkedHealth { get; set; } = 3;
    public PlayerRef PlayerRef { get; internal set; }

    void OnNetHealthChanged() => Debug.Log($"Life = {NetworkedHealth}");

    #endregion

    public event Action<float> OnMovement = delegate { };
    public event Action OnShooting = delegate { };

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            LocalPlayer = this;
            _name=PlayerSpawner.Instance._playerName;
            Debug.Log("Nombre: " + _name);
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

            //_ball = GetComponentInChildren<BallBehaviour>();
            if (!Object.HasInputAuthority)
            {
                Object.AssignInputAuthority(Runner.LocalPlayer);
            }

            var playerRef = Object.InputAuthority;
        }
    }

    public void SetVictim(bool value)
    {
        if (HasStateAuthority) // Ensure only the server modifies this
        {
            victim = value;
        }
        else
        {
            Debug.LogWarning("Only the server can modify the 'victim' variable.");
        }
    }

    public void SetSniper(bool value)
    {
        if (HasStateAuthority) // Ensure only the server modifies this
        {
            sniper = value;
        }
        else
        {
            Debug.LogWarning("Only the server can modify the 'sniper' variable.");
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcRequestSetVictim(bool value)
    {
        if (HasStateAuthority) // Server processes the request
        {
            SetVictim(value);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcRequestSetSniper(bool value)
    {
        if (HasStateAuthority) // Server processes the request
        {
            SetSniper(value);
        }
    }


    private void Start()
    {
        _playerManager = FindObjectOfType<PlayerManager>();
        SetPlayerColor(_playerManager.GetPlayerColor(_localPlayerId));
    }

    public void SetPlayerColor(Color color)
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.material.color = _playerManager.GetPlayerColor(_localPlayerId);
        }
    }


    void Update()
    {
        if (!HasStateAuthority) return;


        _xAxi = Input.GetAxis("Horizontal");
        _yAxi = Input.GetAxis("Vertical");


        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupBall();
            //_ball?.ActivateBall();
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
                ballPickUp.RPC_PickUp(Object);
                HasBall = true;
                
                break;
            }
        }
    }


    private void Fire()
    {
        if (!Object.IsValid)
            return;
        var ball = Runner.Spawn(_prefabBall, ballSpawnPoint.position, Quaternion.identity, Object.InputAuthority);
        ball.IsThrown = true;
        ball.SetThrowingPlayer(this);
        HasBall = false;
        BallPickUp ballPickUp = FindObjectOfType<BallPickUp>();
        Debug.Log("IsPickedUp before RPC_Drop: " + ballPickUp.IsPickedUp);

        if (Object != null && Object.IsValid)
        {
            ballPickUp.RPC_Drop(Object);
        }
        else
        {
            Debug.LogError("Invalid NetworkObject passed to RPC_Drop.");
        }


    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        //MOVIMIENTO
        Movement();
        Debug.Log("dmg" + _dmgApplied.ToString());

        if (IsAlive && HasHitBall() && GameController.Singleton.GameIsRunning)
        {
            Debug.Log("hit");
            ApplyDamage(_inGameBall.ThrowingPlayer);
            //_dmgApplied = true; 
        }
    }

    private bool HasHitBall()
    {
        var count = Runner.GetPhysicsScene().OverlapSphere(_rgbd.position, _playerDamageRadius, _hits,
            _ballCollisionLayer, QueryTriggerInteraction.UseGlobal);
        Debug.Log($"Number of colliders hit: {count}");
        if (count <= 0 || _dmgApplied)
            return false;


        _inGameBall = _hits[0]?.GetComponent<BallBehaviour>();
        if (_inGameBall == null || !_inGameBall.Object.IsValid)
        {
            Debug.LogWarning("Ball is not valid or not confirmed.");
            return false;
        }

        // Verificar si la pelota fue lanzada por otro jugador
        if (_inGameBall.ThrowingPlayer == this)
        {
            Debug.Log("Player hit their own ball, no damage applied.");
            return false; // El jugador no deber�a da�arse con su propia pelota
        }

       

        Debug.Log("Player hit by an enemy ball!");
        //Avisarle a la pelota que choco
        _inGameBall.NotifyCollision(this);
       
        return true;
    }

    private void ApplyDamage(Player throwingPlayer)
    {
        Debug.Log("ApplyDamage called-----------------");

        if (!HasStateAuthority || throwingPlayer == null)
            return;
       
        if (victim && !_dmgApplied)
        {
            _playerDataNetworked.SubtractLife();
            Debug.Log("se resto vida");
            _dmgApplied = true;
            StartCoroutine(ResetDamageFlag());
            if (_playerDataNetworked.Lives <= 0)
            {
                IsAlive = false;
                Debug.Log("Player has been eliminated.");
                SetLoseScreenRPC();
               
            }

        }

        if (sniper)
        {
            throwingPlayer._playerDataNetworked.AddToScore(1);
            if (_playerDataNetworked.Score >= 3)
            {
                Debug.Log("Player has won the game.");
                SetWinScreenRPC();

            }
        }


    }

    private IEnumerator ResetDamageFlag()
    {
        yield return new WaitForSeconds(5f); 
        Debug.Log("Resetting damage flag");
        _dmgApplied = false;
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
                foreach (var material in renderer.materials)
                {
                    material.color = color;
                }
            }

            ChangeColorRecursively(child, color);
        }
    }

    
    private void SetLoseScreenRPC()
    {
        UIManager.instance.SetLoseScreen();
    }

    private void SetWinScreenRPC()
    {
        UIManager.instance.SetVictoryScreen();
    }
}