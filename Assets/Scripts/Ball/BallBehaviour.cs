

using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using System.Collections;


public class BallBehaviour : NetworkBehaviour
    {
        [Tooltip("Points awarded when this asteroid is destroyed")]
        [SerializeField] private int _points = 1;

        [Tooltip("Optional new prefab to spawn when this asteroid is destroyed")]
        [SerializeField] private BallBehaviour _ball;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float moveForce = 7f;
    //[Tooltip("Destruction FX spawned when this asteroid is destroyed")]
    //[SerializeField] private ParticleFx _impactFx;
    //public Player ThrowingPlayer { get; private set; }
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

    private IEnumerator SetReadyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsReady = true;
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
        // Reinicia el estado de la pelota
        IsThrown = false;
        IsReady = false;

        // Desactiva el objeto visual y el collider
        _visual.SetActive(false);
        _collider.enabled = false;

        // Detén cualquier movimiento
        //if (_rb != null)
        //{
        //    _rb.velocity = Vector3.zero;
        //    _rb.angularVelocity = Vector3.zero;
        //}

        Debug.Log("Ball has been reset and deactivated.");
    }

    public void ActivateBall()
    {
        // Reactiva el objeto visual y el collider
        _visual.SetActive(true);
        _collider.enabled = true;

        // Marca la pelota como lista para ser usada
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
        // Spawn an effect
        // Instantiate(_impactFx, p, _impactFx.transform.rotation);

        // Hide the asteroid and disable its collider so we don't hit it again
        //_visual.SetActive(false);
        _collider.enabled = false;

            // If we're the SA of this asteroid, it's our job to despawn it,
            // but we delay it a few ticks so rendering gets a chance to catch up on all peers.
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
            //if (ThrowingPlayer != null && ThrowingPlayer != _player) // Asegurarse de que no sea el mismo jugador
            //{
            //    PlayerDataNetworked hitPlayerData = ThrowingPlayer.GetComponent<PlayerDataNetworked>();
            //    if (hitPlayerData != null)
            //    {
            //        hitPlayerData.SubtractLife(); // Reducir 1 vida al jugador golpeado
            //        Debug.Log("subtract.");
            //    }
            //}


            }


    }

    // Initialize the networked state of a new Asteroid. This is separate from spawned so we can pass a parameter to it.
    // Only called on StateAuthority.
    //public void InitState(Vector3 force)
    //{

    //// Apply some random rotation to the asteroid
    //Vector3 torque = Random.insideUnitSphere * Random.Range(500.0f, 1500.0f);
    //    _rb.AddForce(force);
    //    _rb.AddTorque(torque);
    //}

    // When the asteroid gets hit by a player or a bullet, this method is called on the peer that detected the collision (only).
    // This is probably not the same peer that owns the Asteroid so it can't simply destroy the Asteroid.

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_OnBallHit()
    {
        if (IsThrown)
        {
            // Lógica de colisión cuando la pelota ha sido lanzada
            IsThrown = false; // Reiniciar el estado después de un impacto
            Debug.Log("Ball hit detected and state reset.");
        }
    }

    public bool OnBallHit()
    {
        if (Object.HasStateAuthority)
        {
            // Si este objeto tiene autoridad, actualiza directamente
            if (IsThrown)
            {
                IsThrown = false;
                return true;
            }
        }
        else
        {
            // Si no tiene autoridad, llama al RPC para que el State Authority lo maneje
            RPC_OnBallHit();
        }
        return false;
    }

    //public bool OnBallHit()
    //{
    //    if (Object == null)
    //    {
    //        Debug.LogWarning("Object is null.");
    //        return false;
    //    }
    //    if (IsAlive == false)
    //    {
    //        Debug.LogWarning("Object is not alive.");
    //        return false;
    //    }


    //    if (HasStateAuthority)
    //    {
    //        // Check if the player is assigned
    //        if (_player != null)
    //        {
    //            // Check if the object is valid before sending the RPC
    //            if (Object.IsValid)
    //            {
    //                RPC_BallHitEvent(transform.position);
    //                return true;
    //            }
    //            else
    //            {
    //                Debug.LogWarning("Object is not valid, cannot send RPC.");
    //                return false;
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogWarning("Player is not assigned.");
    //            return false;
    //        }
    //    }
    //    return false;
    //}

    //public bool HasHitPlayer()
    //{
    //    if (Runner.GetPhysicsScene().Raycast(transform.position, transform.forward, out var hit, moveForce * Runner.DeltaTime * 1.25f, _playerLayer))
    //    {
    //        var ballBehaviour = hit.collider.GetComponent<BallBehaviour>();
    //        if (ballBehaviour)
    //        {
    //            var hasHit = ballBehaviour.OnBallHit();
    //            if (!hasHit) return false;

    //            if (Runner.TryGetPlayerObject(Object.InputAuthority, out var playerNetworkObject))
    //            {
    //                playerNetworkObject.GetComponent<PlayerDataNetworked>().AddToScore(ballBehaviour.Points);
    //            }
    //            return true;
    //        }
    //    }
    //    return false;
    //}

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
        //Runner.Despawn(Object);
    }

}

