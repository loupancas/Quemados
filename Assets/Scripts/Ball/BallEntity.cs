using Fusion;
using UnityEngine;

public class BallEntity : Entity
{
    [field: SerializeField] public InventoryItem Stone { get; private set; }

    public void Hit()
    {
        Rpc_Hit();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    protected void Rpc_Hit()
    {
        if (PhysicsUtil.GetAvailablePosition(transform.position, out Vector3 pos))
        {
            Spawn(Stone.EntityPrefab, pos);
        }
    }
}