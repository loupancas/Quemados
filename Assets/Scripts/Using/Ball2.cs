using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Ball2 : NetworkBehaviour
{
    [SerializeField] private float moveForce = 7f;
    [SerializeField] private float lifeTime = 3f;
    private NetworkRigidbody3D networkRb;
    private TickTimer lifeTimer;

    private void Awake()
    {
        networkRb = GetComponent<NetworkRigidbody3D>();
    }

    public override void Spawned()
    {
        lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }

    public override void FixedUpdateNetwork()
    {
        //if (Object.HasStateAuthority && lifeTimer.Expired(Runner))
        //{
        //    Runner.Despawn(Object);
        //}
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcThrow(Vector3 direction)
    {
        if (!HasStateAuthority)
        {
            Debug.LogError("Local simulation is not allowed to send this RPC on " + gameObject);
            return;
        }
        networkRb.Rigidbody.velocity = direction * moveForce;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player: " + other.gameObject.name);
            other.GetComponent<Player>().RPC_TakeDamage(1);
        }

        Runner.Despawn(Object);
    }
}
