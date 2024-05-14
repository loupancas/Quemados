using Fusion;
using UnityEngine;

public class RaycastAttack : NetworkBehaviour
{
    public float Damage = 10;

    // public PlayerMovement PlayerMovement;
    public Player PlayerMovement;

    void Update()
    {
        if (HasStateAuthority == false)
        {
            return;
        }

        Ray ray = PlayerMovement.Camera.ScreenPointToRay(Input.mousePosition);
        ray.origin += PlayerMovement.Camera.transform.forward;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.red, 1f);

            if (Runner.GetPhysicsScene().Raycast(ray.origin,ray.direction, out var hit))
            {
                if (hit.transform.TryGetComponent<Health2>(out var health))
                {
                    health.DealDamageRpc(Damage);
                }
            }
        }
    }
}