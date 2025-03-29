using Fusion;
using Unity.VisualScripting;
using UnityEngine;


public class BallPickup : NetworkBehaviour
{

    public float Radius = 1f;
    public LayerMask LayerMask;
    public GameObject ActiveObject;
    public GameObject InactiveObject;


    [Networked]
  
    public bool IsPickedUp { get; set; }

    private static Collider[] _colliders = new Collider[4];

    public override void Spawned()
    {
        UpdateBallState();
    }


    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!other.TryGetComponent(out Player p)) return;

    //    if (!p.HasStateAuthority) return;

    //    GameManager.Instance.RPC_PlayerWin(p.Runner.LocalPlayer);

    //    this.gameObject.SetActive(false);
    //}


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PickUp(Player player)
    {
        if (IsPickedUp)
        {
            return;
        }

        PickUp(player);
    }

    private void PickUp(Player player)
    {
        IsPickedUp = true;
        player._hasBall = true;
    }

    public void Drop()
    {
        IsPickedUp = false;
    }

    private void UpdateBallState()
    {
        ActiveObject.SetActive(!IsPickedUp);
        InactiveObject.SetActive(IsPickedUp);
    }

    public override void Render()
    {
        UpdateBallState();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}

public class BallPickUp : NetworkBehaviour
{
    [Networked] public bool IsPickedUp { get; set; }

    private void UpdateBallState()
    {
        // Actualiza el estado visual de la pelota
        GetComponent<Renderer>().enabled = !IsPickedUp;
    }

    public override void Spawned()
    {
        UpdateBallState();
    }

    public override void Render()
    {
        UpdateBallState();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                var player = hitCollider.GetComponent<NetworkObject>();
                if (player != null && player.HasInputAuthority)
                {
                    RPC_PickUp();
                    break;
                }
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PickUp()
    {
        IsPickedUp = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

