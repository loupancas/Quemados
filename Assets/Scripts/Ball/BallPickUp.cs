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

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_PickUp(NetworkObject player)
    {
        if (IsPickedUp) return;

        IsPickedUp = true;
        player.GetComponent<Player>().HasBall = true;
        // Asignar la autoridad de entrada al jugador
        // Object.AssignInputAuthority(player.InputAuthority);
        RPC_UpdateBallState();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Drop(NetworkObject player)
    {
        if (!IsPickedUp) return;
        
        IsPickedUp = false;
        player.GetComponent<Player>().HasBall = false;
        RPC_UpdateBallState();
    }

    //private static void OnIsPickedUpChanged(BallPickUp ballPickUp, bool oldValue, bool newValue)
    //{
    //    ballPickUp.UpdateBallState();
    //}


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateBallState()
    {
        UpdateBallState();
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
        Debug.Log($"Object.HasInputAuthority: {Object.HasInputAuthority}");
        if (Object.HasInputAuthority)
        {
            Debug.Log("PickUp");
            RPC_PickUp(player.Object);
        }
        else
        {
          
                Object.AssignInputAuthority(player.Object.InputAuthority);
                Debug.Log("Input authority assigned to player");
                RPC_PickUp(player.Object);
            
        }

        //IsPickedUp = true;
        //player.HasBall = true;
        //UpdateBallState();
    }

    public void Drop(Player player)
    {
        //if (!IsPickedUp) return;

        if (Object.HasInputAuthority)
        {
            Debug.Log("Drop");
            RPC_Drop(player.Object);
        }
        //IsPickedUp = false;
        //player.HasBall = false;
        //UpdateBallState();
    }
    
}
