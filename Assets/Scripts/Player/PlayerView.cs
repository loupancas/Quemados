using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    //[SerializeField] private ParticleSystem _shootingParticles;

    private NetworkMecanimAnimator _mecanim;
    Player player;
    private Vector3 _previousPosition;




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
        else
        {
            // Verifica si la animación de salto ha terminado
            if (_mecanim.Animator.GetCurrentAnimatorStateInfo(0).IsName("Jumping") &&
                _mecanim.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                _mecanim.Animator.SetBool("Jumping", false);
            }
        }

        Vector3 velocity = (player.transform.position - _previousPosition) / Time.fixedDeltaTime;
        _previousPosition = player.transform.position;

        if (velocity.sqrMagnitude < 0.01f)
        {
            _mecanim.Animator.SetFloat("Speed", 0f);
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
        _previousPosition = player.transform.position;
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_TriggerShootingParticles()
    {
        //_shootingParticles.Play();
    }
}
