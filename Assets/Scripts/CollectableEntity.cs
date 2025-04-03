using System.Collections;
using System.Collections.Generic;
using System.Resources;
using Fusion;
using UnityEngine;

public class CollectableEntity : Entity
{
    [field: SerializeField] public InventoryItem Item { get; private set; }
    [Networked] public int Count { get; set; } = 1;

    public override void Spawned()
    {
        //if (Count > 1)
        //{
        //    Instantiate(
        //        ResourcesManager.instance.worldUITextPrefab,
        //        InterfaceManager.instance.worldCanvas.transform)
        //        .Init($"x{Count}", transform, Vector3.up);
        //}
    }

    // called by Interaction UnityEvents in the inspector
    public void Collect()
    {
        //Debug.Log("Collect " + Item.DisplayName);
        Rpc_Collect();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    protected void Rpc_Collect(RpcInfo info = default)
    {
        var collector = Runner.GetPlayerObject(info.Source).GetBehaviour<Inventory>();
        collector.Rpc_AddOrDrop(InventoryItem.GetKey(Item), Count);
        Runner.Despawn(Object);
    }
}
