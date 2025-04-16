

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
     
    //[Tooltip("Destruction FX spawned when this asteroid is destroyed")]
    //[SerializeField] private ParticleFx _impactFx;
        public Player ThrowingPlayer { get; private set; }
        public GameObject _player;
    public Player Player => _player.GetComponent<Player>();


    [Tooltip("The visual model of the asteroid")]
        [SerializeField] private GameObject _visual;
    private bool _isVisualActive = false;
    [Networked] private TickTimer DespawnTimer { get; set; }

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

    public void Initialize(Player throwingPlayer, GameObject player)
    {
        ThrowingPlayer = throwingPlayer;
        _player = player;
    }

    public override void Spawned()
        {
           // Activate the visual and the collider (in case this is a recycled Asteroid)
        if (GameController.Singleton.GameIsRunning)
        {
            RPC_UpdateBallVisual();
        }
        
        _collider.enabled = true;

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateBallVisual()
    {
        if(_isVisualActive==false)
        {
            _isVisualActive = true;
        }
    }

    private IEnumerator EnableColliderWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _collider.enabled = true;
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_BallHitEvent(Vector3 p)
        {
            // Spawn an effect
           // Instantiate(_impactFx, p, _impactFx.transform.rotation);

            // Hide the asteroid and disable its collider so we don't hit it again
            _visual.SetActive(false);
            _collider.enabled = false;

            // If we're the SA of this asteroid, it's our job to despawn it,
            // but we delay it a few ticks so rendering gets a chance to catch up on all peers.
            if (HasStateAuthority)
            {
                DespawnTimer = TickTimer.CreateFromTicks(Runner, 10);
               PlayerDataNetworked playerData = _player.GetComponent<PlayerDataNetworked>();
            if (playerData != null)
            {
                playerData.AddToScore(_points);
            }
  
        }



        }

        // Initialize the networked state of a new Asteroid. This is separate from spawned so we can pass a parameter to it.
        // Only called on StateAuthority.
        public void InitState(Vector3 force)
        {
           
        // Apply some random rotation to the asteroid
        Vector3 torque = Random.insideUnitSphere * Random.Range(500.0f, 1500.0f);
            _rb.AddForce(force);
            _rb.AddTorque(torque);
        }

    // When the asteroid gets hit by a player or a bullet, this method is called on the peer that detected the collision (only).
    // This is probably not the same peer that owns the Asteroid so it can't simply destroy the Asteroid.
    public bool OnBallHit()
    {
        if (Object == null)
        {
            Debug.LogWarning("Object is null.");
            return false;
        }
        if (IsAlive == false)
        {
            Debug.LogWarning("Object is not alive.");
            return false;
        }


        if (HasStateAuthority)
        {
            // Check if the player is assigned
            if (_player != null)
            {
                // Check if the object is valid before sending the RPC
                if (Object.IsValid)
                {
                    RPC_BallHitEvent(transform.position);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Object is not valid, cannot send RPC.");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("Player is not assigned.");
                return false;
            }
        }
        return false;
    }

    public override void FixedUpdateNetwork()
        {
            // Despawn asteroid if it left the game boundaries or it was destroyed.
            if (DespawnTimer.Expired(Runner))
                Runner.Despawn(Object);
        }

       
       
    }
