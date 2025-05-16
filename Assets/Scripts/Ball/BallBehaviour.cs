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

    [Networked] public bool Activar { get; set; }
    public int Points => _points;

    private Rigidbody _rb;
    private NetworkRigidbody3D _nrb;
    private Collider _collider;
    public string sniperName;
    private bool _hitPlayer = false;
   

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _nrb = GetComponent<NetworkRigidbody3D>();
        _collider = GetComponent<Collider>();

        
    }

    

    public void SetThrowingPlayer(Player player)
    {
        
       
        ThrowingPlayer = player;
        this.ThrowingPlayerId = player.Object.Id;
        Debug.Log($"Throwing player set to {player.Object.Id}");
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
        //else
        //{
        //    Debug.Log("BallBehaviour spawned");
        //}

        Activar = true;
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

   


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateBallVisual()
    {
        if (_isVisualActive == false)
        {
            _isVisualActive = true;
        }
    }

   
    public void ResetBall()
    {
        IsThrown = false;
        IsReady = false;

        _visual.SetActive(false);
        _collider.enabled = false;
        Activar = true;
       
        //Debug.Log("Ball has been reset and deactivated.");
    }

    


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_NotifyCollision()
    {
       
        Debug.Log($"Ball collided with player");
    
        _hitPlayer = true;
        if (ThrowingPlayer != null)
        {
            // Si el ThrowingPlayer es el sniper, otorga puntos
            if (ThrowingPlayer.IsSniper)
            {
                ThrowingPlayer.AddScore(); 
                Debug.Log($"Sniper scored!");
            }
        }
        ResetBall();
    }

    

    public void NotifyCollision(Player hitPlayer)
    {
       
       
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