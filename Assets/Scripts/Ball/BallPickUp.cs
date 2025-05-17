using Fusion;
using UnityEngine;
using System.Collections;

public class BallPickUp : NetworkBehaviour
{
    public float Radius = 1f;
    public GameObject ActiveObject;
    public GameObject InactiveObject;
    private NetworkObject playerInTriggerZone;
    [Networked] public bool IsPickedUp { get; set; }

    [SerializeField] BallBehaviour ballBehaviour;


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
      
        IsPickedUp = true;
       

        RPC_UpdateBallState();
    }

   

   

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Drop(NetworkObject player)
    {
        if (!IsPickedUp) return;
        player.GetComponent<Player>().HasBall = false;
        IsPickedUp = false;


       RPC_Respawn();
      
    }

   

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateBallState()
    {
        UpdateBallState();

        if (!IsPickedUp) return;
       
        foreach (var player in PlayerSpawner.Instance.Players)
        {
            var playerComponent = player.GetComponent<Player>();

            if (playerComponent != null)
            {
                if (playerComponent.HasBall==true)
                {

                    playerComponent.RpcRequestSetSniper(true);
                    playerComponent.RpcRequestSetVictim(false);
                }
                else 
                {
                    playerComponent.RpcRequestSetSniper(false);
                    playerComponent.RpcRequestSetVictim(true);
                }
                //Debug.Log("Player sniper" + playerComponent.sniper);
                //Debug.Log("Player victim" + playerComponent.victim);
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
        else
        {
            IsPickedUp = false;
            ActiveObject.SetActive(true);
            InactiveObject.SetActive(false);
        }

        Debug.Log("Ball state updated: " + IsPickedUp);

    }
    public override void FixedUpdateNetwork()
    {
      
        

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Respawn()
    {
        Debug.Log("Ball respawned///////////");
        foreach (var player in PlayerSpawner.Instance.Players)
        {
            var playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {

                playerComponent.RpcRequestSetSniper(false);
                playerComponent.RpcRequestSetVictim(false);
                Debug.Log("Player sniper" + playerComponent.sniper+ "Player victim" + playerComponent.victim);
               

            }
        }
        //if(ballBehaviour.activar)
        StartCoroutine(ActivateWithDelay(3f));
       
    }

    private IEnumerator ActivateWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
       UpdateBallState();
        
    }

   

   
    
}
