using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

/// <summary>
/// Data for any object which can be stored by an <see cref="Inventory"/>
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Inventory Item")]
public class InventoryItem : ScriptableObject
{
    /// <summary>
    /// Icon to display on the inventory screen
    /// </summary>
    [field: SerializeField] public Sprite Icon { get; private set; }
    /// <summary>
    /// Name to display on the inventory screen
    /// </summary>
    [field: SerializeField] public string DisplayName { get; private set; }
    /// <summary>
    /// How many items can occupy a single inventory slot
    /// </summary>
    [field: SerializeField] public byte MaxStackSize { get; private set; }

    /// <summary>
    /// The entity representation of the item, used by drop and spawn methods.
    /// </summary>
    [field: SerializeField] public CollectableEntity EntityPrefab { get; private set; }
    /// <summary>
    /// Collection of all performable actions for this item via the inventory screen.
    /// </summary>
    //[field: SerializeReference, SubclassSelector] public IItemInteraction[] Interactions { get; private set; }

    public static InventoryItem GetItem(short key) => ResourceManager.instance.moneyItem;
    public static short GetKey(InventoryItem item) => 1;
}
