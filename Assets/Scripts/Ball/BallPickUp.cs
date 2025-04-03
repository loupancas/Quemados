using Fusion;
using UnityEngine;

public class BallPickUp : NetworkBehaviour
{
    public float Radius = 1f;
    public GameObject ActiveObject;
    public GameObject InactiveObject;

    [Networked] public bool IsPickedUp { get; set; }

    

    public override void Spawned()
    {
        base.Spawned();
        UpdateBallState();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_PickUp(NetworkObject player)
    {
        if (IsPickedUp) return;

        IsPickedUp = true;
        player.GetComponent<Player>().HasBall = true;
        UpdateBallState();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Drop(NetworkObject player)
    {
        if (!IsPickedUp) return;

        IsPickedUp = false;
        player.GetComponent<Player>().HasBall = false;
        UpdateBallState();
    }

    private void UpdateBallState()
    {
        ActiveObject.SetActive(!IsPickedUp);
        InactiveObject.SetActive(IsPickedUp);
    }

    public void PickUp(Player player)
    {
        if (IsPickedUp) return;

        IsPickedUp = true;
        player.HasBall = true;
        UpdateBallState();
    }

    public void Drop(Player player)
    {
        if (!IsPickedUp) return;

        IsPickedUp = false;
        player.HasBall = false;
        UpdateBallState();
    }
}
