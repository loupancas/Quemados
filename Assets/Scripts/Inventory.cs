using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Resources;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Stores and maintains a networked collection of items. Used by players and NPCs.
/// </summary>
public class Inventory : NetworkBehaviour
{
    [SerializeField] private SlotData[] initialItems;
    [field: SerializeField, Range(1, 32)] public byte Capacity { get; private set; }
    [Networked, Capacity(32)] public NetworkLinkedList<InventorySlot> Slots => default;


    public event System.Action<InventoryItem> OnItemAdded;
    public event System.Action<InventoryItem> OnItemRemoved;

    public override void Spawned()
    {
        if (HasStateAuthority) foreach (var data in initialItems) TryAdd(data.Item, data.Count, out _);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_AddOrDrop(short itemKey, int count)
    {
        InventoryItem item = InventoryItem.GetItem(itemKey);
        TryAdd(item, count, out int countNotAdded);
        if (countNotAdded > 0)
        {
            PhysicsUtil.GetAvailablePosition(transform.position, out Vector3 pos);
            Entity.Spawn(Runner, item.EntityPrefab, pos, BeforeSpawned);
        }

        void BeforeSpawned(NetworkRunner runner, NetworkObject obj)
        {
            obj.GetBehaviour<CollectableEntity>().Count = countNotAdded;
        }
    }

    public void TryAdd(InventoryItem item, int count, out int countNotAdded)
    {
        if (count <= 0)
        {
            countNotAdded = 0;
            return;
        }

        bool itemAdded = false;
        for (int i = 0; i < Slots.Count; i++)
        {
            var slot = Slots.Get(i);
            if (slot.IsItem(item))
            {
                if (slot.Count < item.MaxStackSize)
                {
                    byte amt = (byte)Mathf.Min(count, item.MaxStackSize - slot.Count);
                    count -= amt;
                    slot.Count += amt;

                    Slots.Set(i, slot);
                    itemAdded = true;

                    if (count == 0)
                    {
                        countNotAdded = 0;
                        OnItemAdded?.Invoke(item);
                        return;
                    }
                }
            }
        }

        while (count > 0 && Slots.Count < Capacity)
        {
            byte amt = (byte)Mathf.Min(count, item.MaxStackSize);
            count -= amt;
            Slots.Add(InventorySlot.Create(item, amt));
            itemAdded = true;
        }
        if (itemAdded) OnItemAdded?.Invoke(item);
        countNotAdded = count;
    }

    public void Remove(InventoryItem item, int amount = 1)
    {
        if (amount <= 0) return;

        Stack<int> emptyIndices = new();

        bool itemRemoved = false;
        for (int i = Slots.Count - 1; i >= 0; i--)
        {
            var slot = Slots.Get(i);
            if (slot.IsItem(item))
            {
                byte deduction = (byte)Mathf.Min(amount, slot.Count);
                amount -= deduction;
                slot.Count -= deduction;

                if (slot.Count == 0) emptyIndices.Push(i);
                else Slots.Set(i, slot);

                itemRemoved = true;

                if (amount == 0) break;
            }
        }

        while (emptyIndices.TryPop(out int i))
        {
            Slots.Remove(Slots.Get(i));
        }

        if (itemRemoved) OnItemRemoved?.Invoke(item);
    }

    public bool Contains(InventoryItem item)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            var slot = Slots.Get(i);
            if (slot.IsItem(item)) return true;
        }
        return false;
    }

    public bool Contains(InventoryItem item, int amount)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            var slot = Slots.Get(i);
            if (slot.IsItem(item))
            {
                amount -= slot.Count;
                if (amount <= 0) return true;
            }
        }
        return false;
    }

    public int Count(InventoryItem item)
    {
        int count = 0;
        for (int i = 0; i < Slots.Count; i++)
        {
            var slot = Slots.Get(i);
            if (slot.IsItem(item))
            {
                count += slot.Count;
            }
        }
        return count;
    }

    public IEnumerable<SlotData> EnumerateSlots()
    {
        var enumer = Slots.GetEnumerator();
        while (enumer.MoveNext())
        {
            yield return new SlotData(enumer.Current.GetItem(), enumer.Current.Count);
        }
    }

    [System.Serializable]
    public struct SlotData
    {
        [field: SerializeField] public InventoryItem Item { get; private set; }
        [field: SerializeField] public int Count { get; private set; }
        public SlotData(InventoryItem item, int count)
        {
            Item = item;
            Count = count;
        }
    }
}

public struct InventorySlot : INetworkStruct
{
    public short ResourceID;
    public byte Count;

    public readonly bool IsItem(InventoryItem item)
    {
        return InventoryItem.GetKey(item) == ResourceID;
    }

    public readonly InventoryItem GetItem()
    {
        return InventoryItem.GetItem(ResourceID);
    }

    public static InventorySlot Create(InventoryItem item, byte count = 1)
    {
        return new InventorySlot()
        {
            ResourceID = InventoryItem.GetKey(item),
            Count = count
        };
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Inventory inv = (Inventory)target;
        if (inv.Object && inv.Object.IsValid)
        {
            EditorGUILayout.BeginVertical();
            foreach (var slot in inv.Slots)
            {
                EditorGUILayout.LabelField($"{InventoryItem.GetItem(slot.ResourceID)}({slot.ResourceID}) x {slot.Count}");
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif