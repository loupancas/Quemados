using Fusion;
using Unity.VisualScripting;
using UnityEngine;


public class BallPickUp : NetworkBehaviour
{

    public float Radius = 1f;
    public GameObject ActiveObject;
    public GameObject InactiveObject;


    [Networked]
   
    public bool IsPickedUp { get; set; }

    private static Collider[] _colliders = new Collider[4];

    public override void Spawned()
    {
        IsPickedUp = false;
        UpdateBallState();
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
        IsPickedUp = true;
        //player._hasBall = true;
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


