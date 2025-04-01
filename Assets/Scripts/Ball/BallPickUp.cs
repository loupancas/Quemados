using Fusion;
using Unity.VisualScripting;
using UnityEngine;


public class BallPickUp : NetworkBehaviour
{

    public float Radius = 1f;
    public GameObject ActiveObject;
    public GameObject InactiveObject;
    public static BallPickUp Instance;

    [Networked]
   
    public bool IsPickedUp { get; set; }

    private static Collider[] _colliders = new Collider[4];

    public override void Spawned()
    {
        Instance = this;
        //IsPickedUp = false;
        ActiveObject.SetActive(true);
        InactiveObject.SetActive(false);
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PickUp(Player player)
    {
        if (IsPickedUp)
        {
            return;
        }

        PickUp(player);
       Render();
    }

    private void PickUp(Player player)
    {
        if (Object.HasStateAuthority)
        {
            IsPickedUp = true;
           player.HasBall = true; // Actualiza el estado del jugador
        }
    }

    public void Drop(Player player)
    {
        if (Object.HasStateAuthority)
        {
            IsPickedUp = false;
            player.HasBall = false; // Actualiza el estado del jugador
        }
    }

    private void UpdateBallState()
    {
        if (ActiveObject != null && InactiveObject != null)
        {
            ActiveObject.SetActive(!IsPickedUp);
            InactiveObject.SetActive(IsPickedUp);
        }
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


