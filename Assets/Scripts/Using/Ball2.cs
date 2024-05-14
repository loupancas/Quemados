using Fusion;
using UnityEngine;

public class Ball2 : NetworkBehaviour
{
    [SerializeField] private float _moveForce = 7f;
    [SerializeField] private float _damage = 25f;
    [SerializeField] private float _lifeTime = 3f;

    private TickTimer _lifeTimer;
    
    public override void Spawned()
    {
       
    }

    public void ShootBall()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * _moveForce, ForceMode.VelocityChange);
        _lifeTimer = TickTimer.CreateFromSeconds(Runner, _lifeTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        
        if (!_lifeTimer.Expired(Runner)) return;
        
        Runner.Despawn(Object);
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
}
