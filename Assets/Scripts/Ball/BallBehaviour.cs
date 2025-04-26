

using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;


public class BallBehaviour : NetworkBehaviour
    {
        [Tooltip("Points awarded when this asteroid is destroyed")]
        [SerializeField] private int _points = 1;

        [Tooltip("Optional new prefab to spawn when this asteroid is destroyed")]
        [SerializeField] private BallBehaviour _ball;
    [SerializeField] private LayerMask _playerLayer;

        public GameObject _player;
    public Player Player => _player.GetComponent<Player>();
    [Networked] private bool IsReady { get; set; } = false;

    [Tooltip("The visual model of the asteroid")]
        [SerializeField] private GameObject _visual;
    private bool _isVisualActive;
    [Networked] public Player ThrowingPlayer { get; set; }
    [Networked] private TickTimer DespawnTimer { get; set; }
    [Networked] public bool IsThrown { get; set; }

    [Networked]
    public NetworkId ThrowingPlayerId { get; private set; }

    public bool IsAlive => _visual.activeSelf;
        public int Points => _points;

        private Rigidbody _rb;
        private NetworkRigidbody3D _nrb;
        private Collider _collider;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _nrb = GetComponent<NetworkRigidbody3D>();
            _collider = GetComponent<Collider>();
    

}

    public void SetThrowingPlayerID(Player player)
    {
        ThrowingPlayerId = player.Object.Id; // Guarda el ID del jugador
    }

    public void SetThrowingPlayer(Player player)
    {
        ThrowingPlayer = player;
    }

    public override void Spawned()
    {
        if(IsReady == true)
        {
            if (GameController.Singleton.GameIsRunning)
            {
                RPC_UpdateBallVisual();
            }
        }
        else
        {
            Debug.Log("BallBehaviour spawned");
          

        }


        //_collider.enabled = true;
        DespawnTimer = TickTimer.CreateFromSeconds(Runner, 10);
        //StartCoroutine(SetReadyAfterDelay(0.5f));
    }
    public void SetReady()
    {
        //IsReady = true;
        IsThrown = true;
    }

    public void ReseteState()
    {
        IsThrown = false; // Se establece en false cuando la pelota es recogida
    }

   

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateBallVisual()
    {
        if(_isVisualActive==false)
        {
            _isVisualActive = true;

        }
       
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ResetBall()
    {
        ResetBall();
    }

    public void ResetBall()
    {
        IsThrown = false;
        IsReady = false;

        _visual.SetActive(false);
        _collider.enabled = false;

      

        Debug.Log("Ball has been reset and deactivated.");
    }

    public void ActivateBall()
    {
        _visual.SetActive(true);
        _collider.enabled = true;

        IsReady = true;

        Debug.Log("Ball has been activated and is ready for use.");
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_BallHitEvent(Vector3 p)
    {
            if (!Object.IsValid)
            {
                Debug.LogWarning("RPC_BallHitEvent called on an invalid object.");
                return;
            }

        if (!IsReady)
        {
            Debug.LogWarning("La pelota no está lista para interactuar.");
            return;
        }
      
        _collider.enabled = false;

           
            if (HasStateAuthority && IsReady)
            {
                
                    if (_player != null)
                    {
                        PlayerDataNetworked playerData = _player.GetComponent<PlayerDataNetworked>();
                        if (playerData != null)
                        {
                            playerData.AddToScore(_points);
                            Debug.Log("add to score.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("ThrowingPlayer is null or the same as _player.");
                    }
          


            }


    }

 
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_OnBallHit()
    {
        if (IsThrown)
        {
            IsThrown = false; 
            Debug.Log("Ball hit detected and state reset.");
        }
    }

    public bool OnBallHit()
    {
        if (Object.HasStateAuthority)
        {
            if (IsThrown)
            {
                IsThrown = false;
                return true;
            }
        }
        else
        {
            RPC_OnBallHit();
        }
        return false;
    }

    

    public override void FixedUpdateNetwork()
    {
        //if (HasHitPlayer() == false)
        //{
        //    transform.Translate(transform.forward * moveForce * Runner.DeltaTime, Space.World);
        //}
        //else
        //{
        //    Runner.Despawn(Object);
        //    return;
        //}

        CheckLifetime();



    }

    private void CheckLifetime()
    {
        if (DespawnTimer.Expired(Runner) == false) return;
        RPC_ResetBall();
    }

}

