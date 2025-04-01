using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Ball2 : NetworkBehaviour
{
    [SerializeField] private float _moveForce = 7f;
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _lifeTime = 3f;
    private NetworkRigidbody3D _networkRb;
    MeshRenderer _meshRenderer;
    private TickTimer _lifeTimer;
    TickTimer _lifeTimeTickTimer = TickTimer.None;
    private void Awake()
    {
        _networkRb = GetComponent<NetworkRigidbody3D>();
    }

    private void Start()
    {
       
    }
    public override void Spawned()
    {
        _networkRb.Rigidbody.AddForce(transform.forward * 10, ForceMode.VelocityChange);

        if (Object.HasStateAuthority)
        {
            _lifeTimeTickTimer = TickTimer.CreateFromSeconds(Runner, 2);
        }
    }

    public void ShootBall()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * _moveForce, ForceMode.VelocityChange);
        _lifeTimer = TickTimer.CreateFromSeconds(Runner, _lifeTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (_lifeTimeTickTimer.Expired(Runner))
            {
                DespawnObject();
            }
        }
    }

    void DespawnObject()
    {
        _lifeTimeTickTimer = TickTimer.None;

        Runner.Despawn(Object);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;
        
        if (other.gameObject.layer == 3)
        {
            other.GetComponent<Player>().RPC_TakeDamage(_damage);
        }

        DespawnObject();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcThrow(Vector3 direction)
    {
        // Implement the logic for throwing the ball
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * 10, ForceMode.VelocityChange);
        }
    }
}
