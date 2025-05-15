using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;


public class BallBehaviour : NetworkBehaviour
{
    [Tooltip("Points awarded when this asteroid is destroyed")] [SerializeField]
    private int _points = 1;

    [Tooltip("Optional new prefab to spawn when this asteroid is destroyed")] [SerializeField]
    private BallBehaviour _ball;

    [SerializeField] private LayerMask _playerLayer;

    public GameObject _player;
    public Player Player => _player.GetComponent<Player>();
    [Networked] private bool IsReady { get; set; } = false;

    [Tooltip("The visual model of the asteroid")] [SerializeField]
    private GameObject _visual;
    
    private bool _isVisualActive;
    [Networked] public Player ThrowingPlayer { get; set; }
    [Networked] private TickTimer DespawnTimer { get; set; }
    [Networked] public bool IsThrown { get; set; }

    [Networked] public NetworkId ThrowingPlayerId { get; private set; }
  
    public bool activar;
    public int Points => _points;

    private Rigidbody _rb;
    private NetworkRigidbody3D _nrb;
    private Collider _collider;
    public string sniperName;
    private bool _hitPlayer = false;

    [SerializeField] private PlayerDataNetworked _playerDataNetworked;
   
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _nrb = GetComponent<NetworkRigidbody3D>();
        _collider = GetComponent<Collider>();
        if (_playerDataNetworked == null)
        {
            _playerDataNetworked = FindObjectOfType<PlayerDataNetworked>();
            if (_playerDataNetworked == null)
            {
                Debug.LogError("PlayerDataNetworked is not assigned and could not be found in the scene.");
            }
        }


    }

    

    public void SetThrowingPlayer(Player player)
    {
        Debug.Log("Throwing player: " + player);
        sniperName = player._name;
        ThrowingPlayer = player;
        _visual.SetActive(true);
    }

  
    public override void Spawned()
    {
        if (IsThrown == true)
        {
            if (GameController.Singleton.GameIsRunning)
            {
                //RPC_UpdateBallVisual();
                SetReady();
            }
        }
        else
        {
            Debug.Log("BallBehaviour spawned");
        }


        //_collider.enabled = true;
        DespawnTimer = TickTimer.CreateFromSeconds(Runner, 5);
        //StartCoroutine(SetReadyAfterDelay(0.5f));
    }

    public void SetReady()
    {
        //IsReady = true;
        
        transform.Translate(transform.forward * 10 * Runner.DeltaTime, Space.World);
        IsThrown = false;
        //_hitPlayer = false;
    }

    public void ReseteState()
    {
        IsThrown = false; // Se establece en false cuando la pelota es recogida
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateBallVisual()
    {
        if (_isVisualActive == false)
        {
            _isVisualActive = true;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ResetBall()
    {
        ResetBall();
        //_visual.SetActive(false);
        //_ballPickUp = GetComponent<BallPickUp>();
        //if (_ballPickUp != null)
        //{
        //    _ballPickUp.IsPickedUp = false;
        //    _ballPickUp.RPC_UpdateBallState();

        //}
    }
    public void ResetBall()
    {
        IsThrown = false;
        IsReady = false;

        _visual.SetActive(false);
        _collider.enabled = false;
        activar = true;

        Debug.Log("Ball has been reset and deactivated.");
    }

    public void ActivateBall()
    {
        _visual.SetActive(true);
        _collider.enabled = true;

        IsReady = true;

        Debug.Log("Ball has been activated and is ready for use.");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_NotifyCollision()
    {
        //if (_playerDataNetworked == null || !_playerDataNetworked.Object.IsValid)
        //{
        //    Debug.LogError("PlayerDataNetworked is not initialized or not spawned yet.");
        //    return;
        //}
        Debug.Log($"Ball collided with player");
        // Aquí puedes manejar lógica adicional, como desactivar la pelota o registrar el impacto.
        _hitPlayer = true;
        //_playerDataNetworked.AddToScore(1);

        ResetBall();
    }

    public void NotifyCollision(Player hitPlayer)
    {
       
        // Aquí puedes manejar lógica adicional, como desactivar la pelota o registrar el impacto.
        //if (Object.HasStateAuthority)
        //{
        //    RPC_ResetBall();
        //}
        //else
        //{
        //    RPC_NotifyCollision();
        //}
        RPC_NotifyCollision();
    }




    public override void FixedUpdateNetwork()
    {
        if (_hitPlayer)
        {
           _visual.SetActive(false);
            //transform.Translate(transform.forward * 10 * Runner.DeltaTime, Space.World);
        }
        else
        {
            //Runner.Despawn(Object);
            transform.Translate(transform.forward * 10 * Runner.DeltaTime, Space.World);
            CheckLifetime();
            //transform.Translate(transform.forward * 10 * Runner.DeltaTime, Space.World);
            return;
        }

        CheckLifetime();
    }

    private void CheckLifetime()
    {
        if (DespawnTimer.Expired(Runner) == false) return;
        ResetBall();
    }
}