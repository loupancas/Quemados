using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class PowerUp : NetworkBehaviour
{
    public float jump = 10f;
    public float Radius = 1f;
    public float Cooldown = 25f;
    public LayerMask LayerMask;
    public GameObject IsActive;
    public GameObject Desactivated;

    //public bool IsActive => _activationTimer.ExpiredOrNotRunning(Runner);

    private TickTimer _activationTimer { get; set; }

    private static Collider[] _colliders = new Collider[8];
    public override void Spawned()
    {
        IsActive.SetActive(IsActive);
        Desactivated.SetActive(IsActive == false);

    }

    public override void FixedUpdateNetwork()
    {
        if (IsActive == false) return;

        //se ejecutara el power up
        // Get all colliders around pickup within Radius.
        int collisions = Runner.GetPhysicsScene().OverlapSphere(transform.position + Vector3.up, Radius, _colliders, LayerMask, QueryTriggerInteraction.Ignore);
        //for (int i = 0; i < collisions; i++)
        //{
            //// Check for power component on collider game object or any parent.
            //var pickUp = _colliders[i].GetComponentInParent<Power>();
            //if (pickUp != null && pickUp.AddPower(Power))
            //{
            //    // Pickup was successful, activating timer.
            //    _activationTimer = TickTimer.CreateFromSeconds(Runner, Cooldown);
            //    break;
            //}
    }

        public override void Render()
        {
        IsActive.SetActive(IsActive);
        Desactivated.SetActive(IsActive == false);
        }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + Vector3.up, Radius);
    }

}
