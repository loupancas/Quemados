using System.Collections;
using System.Collections.Generic;
using System.Resources;
using Fusion;
using UnityEngine;
using UnityEngine.TextCore.Text;


public interface IItemInteraction
{
    public string ActionName { get; }
    public bool CanPerformInteraction(Inventory inventory, InventoryItem item);
    public void PerformInteraction(Inventory inventory, InventoryItem item);
}


public class DropItemInteraction : IItemInteraction
{
    public string ActionName => "Drop";

    public bool CanPerformInteraction(Inventory inventory, InventoryItem item) => true;

    public void PerformInteraction(Inventory inventory, InventoryItem item)
    {
        inventory.Remove(item);
        Entity.Spawn(inventory.Runner, item.EntityPrefab, inventory.transform.position);
    }
}




public class DropItemAmountInteraction : IItemInteraction
{
    public int Amount = 1;
    public string ActionName => $"Drop {Amount}";

    public bool CanPerformInteraction(Inventory inventory, InventoryItem item) => inventory.Contains(item, Amount);

    public void PerformInteraction(Inventory inventory, InventoryItem item)
    {
        inventory.Remove(item, Amount);
        Entity.Spawn(inventory.Runner, item.EntityPrefab, inventory.transform.position, BeforeSpawned);

        void BeforeSpawned(NetworkRunner runner, NetworkObject obj)
        {
            obj.GetBehaviour<CollectableEntity>().Count = Amount;
        }
    }
}




