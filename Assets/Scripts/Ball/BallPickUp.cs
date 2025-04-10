using Fusion;
using UnityEngine;
using System.Collections;

public class BallPickUp : NetworkBehaviour
{
    public float Radius = 1f;
    public GameObject ActiveObject;
    public GameObject InactiveObject;

    [Networked] public bool IsPickedUp { get; set; }

    

    public override void Spawned()
    {
        Debug.Log("BallPickUp spawned");
        base.Spawned();
        UpdateBallState();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_PickUp(NetworkObject player)
    {
        if (IsPickedUp) return;

        IsPickedUp = true;
        player.GetComponent<Player>().HasBall = true;
        // Asignar la autoridad de entrada al jugador
        Object.AssignInputAuthority(player.InputAuthority);
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
        if (IsPickedUp)
        {
            ActiveObject.SetActive(false);
            InactiveObject.SetActive(true);
        }
        else
        {
            StartCoroutine(ActivateWithDelay(3f)); // Ajusta el tiempo de retraso según sea necesario
        }
    }

    private IEnumerator ActivateWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ActiveObject.SetActive(true);
        InactiveObject.SetActive(false);
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
