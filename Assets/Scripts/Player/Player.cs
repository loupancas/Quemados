using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using System.Resources;
using UnityEditor;
using Fusion.Addons.Physics;
using Unity.VisualScripting;
public class Player : NetworkBehaviour
{
    public static Player LocalPlayer { get; set; }
    public Inventario inventario;
    public PlayerDataNetworked _playerDataNetworked = null;
    [SerializeField] private float _respawnDelay = 2.0f;
    private ChangeDetector _changeDetector;
    private int _localPlayerId;
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
    [SerializeField] private BallBehaviour _ball;
    [Networked] public bool HasBall { get; set; }
    public delegate void HasBallChangeHandler(bool hasBall);
    public event HasBallChangeHandler OnHasBallChange;
    public delegate void BallThrownHandler();
    public event BallThrownHandler OnBallThrown;
    [SerializeField] private Transform ballSpawnPoint;
    private Collider[] _hits = new Collider[1];
    private PlayerManager _playerManager;
    //[Networked, OnChangedRender(nameof(ChangeColor))] public Color _teamColor { get; set; }
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

            _ball = GetComponentInChildren<BallBehaviour>();
            if (!Object.HasInputAuthority)
            {
                Object.AssignInputAuthority(Runner.LocalPlayer);
            }
            var playerRef = Object.InputAuthority;
           // StartCoroutine(InitializePlayerColor());

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

    //private IEnumerator InitializePlayerColor()
    //{
    //    // Esperar hasta que el objeto esté confirmado
    //    while (!Object.IsValid)
    //    {
    //        yield return null;
    //    }

    //    // Obtener el nombre del jugador desde PlayerPrefs
    //    string playerName = PlayerPrefs.GetString("PlayerNickName", "DefaultPlayer");

    //    // Llamar al método GetColor de PlayerSpawner
    //    Color playerColor = PlayerSpawner.GetColor(playerName);

    //    // Establecer el color del jugador
    //    RPC_SetPlayerColor(playerColor);
    //}


    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    //private void RPC_SetPlayerColor(Color color)
    //{
    //    NetworkedColor = color;
    //}

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
           _ball.ActivateBall();

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

    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    //private void RPC_PickUpBall(NetworkObject ballObject)
    //{
    //    Debug.Log("RPC_PickUpBall called");
    //    BallPickUp ballPickUp = ballObject.GetComponent<BallPickUp>();
    //    if (ballPickUp != null)
    //    {
    //        ballPickUp.PickUp(this);
    //        HasBall = true;
    //        OnHasBallChange?.Invoke(HasBall);
    //        MeshRenderer ballRenderer = ballObject.GetComponent<MeshRenderer>();
    //        if (ballRenderer != null)
    //        {
    //            ballRenderer.enabled = false;
    //        }
           

    //    }
    //}

 


    private void Fire()
    {
        if (!Object.IsValid)
            return;

        var ball = Runner.Spawn(_ball, ballSpawnPoint.position, _rgbd.rotation, Object.InputAuthority);
        var ballBehaviour = ball.GetComponent<BallBehaviour>();
        if (ballBehaviour != null)
        {
            ballBehaviour.SetThrowingPlayer(this);
            ballBehaviour.SetReady();
        }
        //if (ball != null)
        //{
        //    var ballBehaviour = ball.GetComponent<BallBehaviour>();
        //    if (ballBehaviour != null)
        //    {
        //        ballBehaviour.Initialize(LocalPlayer, gameObject);
        //    }
        //}


        //var ball = Runner.Spawn(_ball, ballSpawnPoint.position, _rgbd.rotation, Object.InputAuthority);
        //ball.GetComponent<BallBehaviour>().Initialize( LocalPlayer, gameObject);
        //BallPickUp ballPickUp = ball.GetComponent<BallPickUp>();
        //if (ballPickUp != null)
        //{
        //    //ballPickUp.Drop(this );
        //    ball.InitState(_rgbd.velocity);
        //    //HasBall = false;
        //    //OnHasBallChange?.Invoke(HasBall);
        //    //MeshRenderer ballRenderer = Object.GetComponent<MeshRenderer>();
        //    //if (ballRenderer != null)
        //    //{
        //    //    ballRenderer.enabled = true;
        //    //}

        //}
        //OnBallThrown?.Invoke();
    }

    public override void FixedUpdateNetwork()
    {
       
        if (!HasStateAuthority) return;

        //MOVIMIENTO
        Movement();



        if (IsAlive && HasHitBall() && GameController.Singleton.GameIsRunning)
            ApplyDamage(_ball.ThrowingPlayer);






    }

    private bool HasHitBall()
    {
        var count = Runner.GetPhysicsScene().OverlapSphere(_rgbd.position, _playerDamageRadius, _hits,
            _ballCollisionLayer.value, QueryTriggerInteraction.UseGlobal);

        if (count <= 0)
            return false;

        //var ballBehaviour = _hits[0]?.GetComponent<BallBehaviour>();

        _ball = _hits[0]?.GetComponent<BallBehaviour>();
        if (_ball == null || !_ball.Object.IsValid)
        {
            Debug.LogWarning("Ball is not valid or not confirmed.");
            return false;
        }

        // Verificar si la pelota fue lanzada por otro jugador
        if (_ball.ThrowingPlayer == this)
        {
            Debug.Log("Player hit their own ball, no damage applied.");
            return false; // El jugador no debería dañarse con su propia pelota
        }

        //if (_ball.ThrowingPlayerId == Object.Id)
        //{
        //    Debug.Log("Player hit their own ball, no damage applied.");
        //    return false;
        //}
        //else
        //{

            // Si la pelota es válida y fue lanzada por otro jugador, aplicar daño
            Debug.Log("Player hit by an enemy ball!");
          
        



        return true;
    }

    private void ApplyDamage(Player throwingPlayer)
    {
        //if (!HasStateAuthority)
        //    return;

        //// Reducir la vida del jugador
        //_playerDataNetworked.SubtractLife();

        //// Opcional: Incrementar la puntuación del jugador que lanzó la pelota
        //if (throwingPlayer != null)
        //{
        //    throwingPlayer._playerDataNetworked.AddToScore(1);
        //}
        if (!HasStateAuthority || throwingPlayer == null)
            return;

        // Acceder a _playerDataNetworkedIds desde GameController
        var playerIds = GameController.Singleton._playerDataNetworkedIds;

        // Verificar si el jugador ya ha causado daño
        if (playerIds.Contains(throwingPlayer.Id))
        {
            Debug.Log("Damage already applied by this player.");
            return;
        }

        // Agregar el jugador a la lista
        playerIds.Add(throwingPlayer.Id);

        // Aplicar daño y lógica adicional
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
        //    // Aquí puedes manejar la lógica de eliminación del jugador
        //}
    }

    //private void PlayerWasHit()
    //{
    //    if (!HasStateAuthority || !Object.IsValid)
    //    {
    //        Debug.LogWarning("Player object is not valid or does not have state authority.");
    //        return;
    //    }

    //    Debug.Log("Player was hit by a ball.");


    //    if (_playerDataNetworked.Lives > 1)
    //    {
    //        RespawnTimer = TickTimer.CreateFromSeconds(Runner, _respawnDelay);
    //        _playerDataNetworked.SubtractLife();
    //        var ballBehaviour = _hits[0].GetComponent<BallBehaviour>();
    //        if (ballBehaviour != null && ballBehaviour.Object.IsValid && ballBehaviour.ThrowingPlayer != null)
    //        {
    //            ballBehaviour.ThrowingPlayer._playerDataNetworked.AddToScore(1);
    //        }

    //    }
    //    else
    //    {
    //        RespawnTimer = default;
    //        IsAlive = false;
    //        SetLoseScreenRPC();
    //        UIManager.instance.SetLoseScreen();
    //        RoomM.Instance.RPC_PlayerWin(Runner.LocalPlayer);
    //    }


    //}


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

    //private IEnumerator WaitInit()
    //{
    //    yield return new WaitForSeconds(1);

    //    switch (Runner.LocalPlayer.PlayerId)
    //    {
    //        case 1:
    //            _teamColor = Color.red;
    //            break;
    //        case 2:
    //            _teamColor = Color.blue;
    //            break;
    //        default:
    //            _teamColor = Color.yellow;
    //            break;
    //    }
    //}

    //public static Color GetColor(int player)
    //{
    //    switch (player % 8)
    //    {
    //        case 0: return Color.red;
    //        case 1: return Color.green;
    //        case 2: return Color.blue;
    //        case 3: return Color.yellow;
    //        case 4: return Color.cyan;
    //        case 5: return Color.grey;
    //        case 6: return Color.magenta;
    //        case 7: return Color.white;
    //    }

    //    return Color.black;
    //}


    private void SetLoseScreenRPC()
    {
        UIManager.instance.SetLoseScreen();
    }



}
