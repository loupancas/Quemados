using Fusion;
using UnityEngine;
using UnityEngine.Events;

public class RaycastAttack : NetworkBehaviour
{
    public float Damage = 10;

    // public PlayerMovement PlayerMovement;
    public Player PlayerMovement;

    public UnityEvent OnThrow;
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
            OnThrow?.Invoke();
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