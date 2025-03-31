using Fusion;
using UnityEngine;

public class Ball2 : NetworkBehaviour
{
    [SerializeField] private float _moveForce = 7f;
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _lifeTime = 3f;
    MeshRenderer _meshRenderer;
    private TickTimer _lifeTimer;

    private void Awake()
    {
    }

    private void Start()
    {
       
    }
    public override void Spawned()
    {
        _meshRenderer = GetComponent<MeshRenderer>();

        _meshRenderer.enabled = false;
        _lifeTimer = TickTimer.CreateFromSeconds(Runner, _lifeTime);
    }

    public void ShootBall()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * _moveForce, ForceMode.VelocityChange);
        _lifeTimer = TickTimer.CreateFromSeconds(Runner, _lifeTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        
        if (!_lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;
        
        if (other.gameObject.layer == 3)
        {
            other.GetComponent<Player>().RPC_TakeDamage(_damage);
        }

        Runner.Despawn(Object);
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
