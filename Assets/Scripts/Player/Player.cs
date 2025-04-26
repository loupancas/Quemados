using System;
using System.Collections;
using UnityEngine;
using Fusion;
using UnityEngine.Serialization;

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

    [FormerlySerializedAs("_ball")] [Header("Ball")] [SerializeField]
    private BallBehaviour _inGameBall;

    [SerializeField] private BallBehaviour _prefabBall;
    [Networked] public bool HasBall { get; set; }

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

    void OnNetHealthChanged() => Debug.Log($"Life = {NetworkedHealth}");

    #endregion

    public event Action<float> OnMovement = delegate { };
    public event Action OnShooting = delegate { };

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

            //_ball = GetComponentInChildren<BallBehaviour>();
            if (!Object.HasInputAuthority)
            {
                Object.AssignInputAuthority(Runner.LocalPlayer);
            }

            var playerRef = Object.InputAuthority;
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
                ballPickUp.PickUp(this);

                break;
            }
        }
    }


    private void Fire()
    {
        if (!Object.IsValid)
            return;

        var ball = Runner.Spawn(_prefabBall, ballSpawnPoint.position, _rgbd.rotation, Object.InputAuthority);
        var ballBehaviour = ball.GetComponent<BallBehaviour>();
        if (ballBehaviour != null)
        {
            ballBehaviour.SetThrowingPlayer(this);
            ballBehaviour.SetReady();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        //MOVIMIENTO
        Movement();


        if (IsAlive && HasHitBall() && GameController.Singleton.GameIsRunning)
            ApplyDamage(_inGameBall.ThrowingPlayer);
    }

    private bool HasHitBall()
    {
        var count = Runner.GetPhysicsScene().OverlapSphere(_rgbd.position, _playerDamageRadius, _hits,
            _ballCollisionLayer, QueryTriggerInteraction.UseGlobal);

        if (count <= 0)
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

        //if (_ball.ThrowingPlayerId == Object.Id)
        //{
        //    Debug.Log("Player hit their own ball, no damage applied.");
        //    return false;
        //}
        //else
        //{

        Debug.Log("Player hit by an enemy ball!");
        //Avisarle a la pelota que choco

        return true;
    }

    private void ApplyDamage(Player throwingPlayer)
    {
        if (!HasStateAuthority || throwingPlayer == null)
            return;

        var playerIds = GameController.Singleton._playerDataNetworkedIds;

        if (playerIds.Contains(throwingPlayer.Id))
        {
            Debug.Log("Damage already applied by this player.");
            return;
        }

        // Agregar el jugador a la lista
        playerIds.Add(throwingPlayer.Id);

        // Aplicar da�o y l�gica adicional
        _playerDataNetworked.SubtractLife();
        throwingPlayer._playerDataNetworked.AddToScore(1);

        if (_playerDataNetworked.Lives <= 0)
        {
            IsAlive = false;
            Debug.Log("Player has been eliminated.");
            SetLoseScreenRPC();
            UIManager.instance.SetLoseScreen();
            RoomM.Instance.RPC_PlayerWin(Runner.LocalPlayer);
        }

        // Verificar si el jugador ha muerto
        //if (_playerDataNetworked.Lives <= 0)
        //{
        //    IsAlive = false;
        //    Debug.Log("Player has been eliminated.");
        //    SetLoseScreenRPC();
        //    UIManager.instance.SetLoseScreen();
        //    RoomM.Instance.RPC_PlayerWin(Runner.LocalPlayer);
        //    // Aqu� puedes manejar la l�gica de eliminaci�n del jugador
        //}
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
}