//using Fusion;
//using Fusion.Addons.Physics;
//using UnityEngine;

//public class Ball2 : NetworkBehaviour
//{
//    [SerializeField] private float moveForce = 7f;
//    [SerializeField] private float lifeTime = 5f;
//    [SerializeField] private LayerMask _playerLayer;
//    private NetworkRigidbody3D networkRb;
//    private TickTimer lifeTimer { get; set; }
//    public bool IsPickedUp { get; set; } = false;
//    private void Awake()
//    {
//        //networkRb = GetComponent<NetworkRigidbody3D>();
//    }

//    public override void Spawned()
//    {
//        if (Object.HasStateAuthority == false) return;
//        lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
//        //base.Spawned();
//    }

//    public override void FixedUpdateNetwork()
//    {
//        if (HasHitPlayer() == false)
//        {
//            transform.Translate(transform.forward * moveForce * Runner.DeltaTime, Space.World);
//        }
//        else
//        {
//            Runner.Despawn(Object);
//            return;
//        }

//        CheckLifetime();
//    }


//    private void CheckLifetime()
//    {
//        if (lifeTimer.Expired(Runner) == false) return;

//        Runner.Despawn(Object);
//    }

//    public bool HasHitPlayer()
//    {
//        if (Runner.GetPhysicsScene().Raycast(transform.position, transform.forward, out var hit, moveForce * Runner.DeltaTime * 1.25f, _playerLayer))
//        {
//            var ballBehaviour = hit.collider.GetComponent<BallBehaviour>();
//            if (ballBehaviour)
//            {
//                var hasHit = ballBehaviour.OnBallHit();
//                if (!hasHit) return false;

//                if (Runner.TryGetPlayerObject(Object.InputAuthority, out var playerNetworkObject))
//                {
//                    playerNetworkObject.GetComponent<PlayerDataNetworked>().AddToScore(ballBehaviour.Points);
//                }
//                return true;
//            }
//        }
//        return false;
//    }

//    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
//    //public void RpcThrow(Vector3 direction)
//    //{
//    //    if (!HasStateAuthority)
//    //    {
//    //        Debug.LogError("Local simulation is not allowed to send this RPC on " + gameObject);
//    //        return;
//    //    }
//    //    networkRb.Rigidbody.velocity = direction * moveForce;
//    //}

//    //private void OnTriggerEnter(Collider other)
//    //{
//    //    if (!HasStateAuthority) return;

//    //    if (other.gameObject.CompareTag("Player"))
//    //    {
//    //        Debug.Log("Hit player: " + other.gameObject.name);
//    //        other.GetComponent<Player>().RPC_TakeDamage(1);
//    //    }

//    //    Runner.Despawn(Object);
//    //}
//}
