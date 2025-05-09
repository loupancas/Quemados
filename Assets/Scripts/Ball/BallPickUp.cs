using Fusion;
using UnityEngine;
using System.Collections;

public class BallPickUp : NetworkBehaviour
{
    public float Radius = 1f;
    public GameObject ActiveObject;
    public GameObject InactiveObject;

    [Networked] public bool IsPickedUp { get; set; }

  


    private bool previousIsPickedUp;
    public override void Spawned()
    {
        Debug.Log("BallPickUp spawned");
        base.Spawned();
        UpdateBallState();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PickUp(NetworkObject player)
    {
        if (IsPickedUp) return;
        //var playerComponent = player.GetComponent<Player>();
        //if (playerComponent == null)
        //{
        //    Debug.LogError("El objeto proporcionado no es un jugador válido.");
        //    return;
        //}
        IsPickedUp = true;
        player.GetComponent<Player>().HasBall = true;
       
        RPC_UpdateBallState();
    }

    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    //public void RPC_Drop(NetworkObject player)
    //{
    //    if (!IsPickedUp) return;
        
    //    IsPickedUp = false;
    //    player.GetComponent<Player>().HasBall = false;
    //    RPC_UpdateBallState();
    //}

 


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateBallState()
    {
        UpdateBallState();

        foreach (var player in PlayerSpawner.Instance.Players)
        {
            var playerComponent = player.GetComponent<Player>();

            if (playerComponent != null)
            {
                if (playerComponent.HasBall)
                {

                    playerComponent.RpcRequestSetSniper(true);
                    playerComponent.RpcRequestSetVictim(false);
                }
                else
                {
                    playerComponent.RpcRequestSetSniper(false);
                    playerComponent.RpcRequestSetVictim(true);
                }
            }
        }

    }
    private void UpdateBallState()
    {
        if (IsPickedUp)
        {
            ActiveObject.SetActive(false);
            InactiveObject.SetActive(true);
        }
        else if(GameController.Singleton.GameIsRunning && !IsPickedUp)
        {
            RPC_Respawn(); // Ajusta el tiempo de retraso según sea necesario
        }

        Debug.Log($"Ball state updated: IsPickedUp = {IsPickedUp}");


    }
    public override void FixedUpdateNetwork()
    {
        if (previousIsPickedUp != IsPickedUp)
        {
            UpdateBallState();
            previousIsPickedUp = IsPickedUp;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Respawn()
    {
        foreach (var player in PlayerSpawner.Instance.Players)
        {
            var playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {

                playerComponent.sniper = false;
                playerComponent.victim = false;


            }
        }
        StartCoroutine(ActivateWithDelay(3f));
       
    }

    private IEnumerator ActivateWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ActiveObject.SetActive(true);
        InactiveObject.SetActive(false);
    }

    public void PickUp(Player player)
    {
        Debug.Log("PickUp method called");
        if (IsPickedUp) return;
        if (player.Object.HasInputAuthority)
        {
            Debug.Log("Player has input authority, attempting to pick up the ball.");

            // Assign input authority to the ball if it doesn't already have it
            if (!Object.HasInputAuthority)
            {
                Object.AssignInputAuthority(player.Object.InputAuthority);
                Debug.Log("Input authority assigned to player");
            }

            // Call the RPC to pick up the ball
            RPC_PickUp(player.Object);
        }
        else
        {
            Debug.LogError("Player does not have input authority to pick up the ball.");
        }


    }

    //public void Drop(Player player)
    //{
    //    //if (!IsPickedUp) return;

    //    if (Object.HasInputAuthority)
    //    {
    //        Debug.Log("Drop");
    //        RPC_Drop(player.Object);
    //    }
      
    //}
    
}
