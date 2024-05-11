using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    //[SerializeField] private ParticleSystem _shootingParticles;

    //private NetworkMecanimAnimator _mecanim;
    Animator _animator;

    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        //_mecanim = GetComponentInChildren<NetworkMecanimAnimator>();
        _animator = GetComponent<Animator>();
        //var m = GetComponentInParent<PlayerMovement>();

        //if (!m || !_mecanim) return;

        //m.OnMovement += MoveAnimation;
        //m.OnShooting += RPC_TriggerShootingParticles;

    }

    public override void FixedUpdateNetwork()
    {
        //_animator.SetBool("isWalking", Player.LocalPlayer._rgbd.velocity.SetYZero().sqrMagnitude >0);
        //IsRunning = Player.LocalPlayer.RB.velocity.SetYZero().sqrMagnitude > 0;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_TriggerShootingParticles()
    {
        //_shootingParticles.Play();
    }
}
