using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    //[SerializeField] private ParticleSystem _shootingParticles;

    private NetworkMecanimAnimator _mecanim;
    Player player;
   




    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        _mecanim = GetComponentInChildren<NetworkMecanimAnimator>();   



        if (_mecanim == null) return;
     


    }

    public override void FixedUpdateNetwork()
    {
      
        if (IsProxy == true)
            return;

        if (Runner.IsForward == false)
            return;

        if (player._jumpPressed == true)
        {
            _mecanim.Animator.SetBool("Jumping", true);
        }


        if (player._rgbd.velocity.sqrMagnitude < 0.01f)
        {
            _mecanim.Animator.SetFloat("Speed",0f);
        }
        else
        {
            _mecanim.Animator.SetFloat("Speed", 1f);

        }

       




    }

    private void Awake()
    {
        player = GetComponentInChildren<Player>();
        _mecanim = GetComponentInChildren<NetworkMecanimAnimator>();
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_TriggerShootingParticles()
    {
        //_shootingParticles.Play();
    }
}