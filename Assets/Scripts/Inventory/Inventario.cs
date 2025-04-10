using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventario : MonoBehaviour
{
   
    public static event Action<List<StackItem>> OnInventoryChange;
    public static event Action<List<StackItem>> OnInventoryUpdate;
    public List<StackItem> inventory=new List<StackItem>();
    private Dictionary<ItemData, StackItem> itemDictionary=new Dictionary<ItemData, StackItem>(); // para chequear si existe o no en el inventario

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            TryUse(0);
        }
    }
    public List<StackItem> GetInventory()
    {
        return inventory;
    }

    public void TryUse(int index) //primer indice cero
    {
        if(inventory.Count>0 && inventory.Count>=index+1 )
        {
            if (inventory[index].StackSize > 0) //existe en el stack?
            {
                string nameUsable = inventory[index].itemData.UsableName;
                //UsableManager.instance.Use(nameUsable);
                inventory[index].RemoveFromStack();
                OnInventoryUpdate.Invoke(inventory);
            }
        }
    }



    public void Add(ItemData itemData)
    {
        for (int i = 0; i < inventory.Count; i++) 
        {
            if(inventory[i].itemData.displayName==itemData.displayName)
            {
                inventory[i].addToStack();
                Debug.Log($"{ itemData.displayName} La cantidad total ahora es {inventory.Count+1}");
                OnInventoryUpdate.Invoke(inventory);
                return;
            }
        }

        StackItem newItem = new StackItem(itemData);
        inventory.Add(newItem);
        Debug.Log($"Se añadió { itemData.displayName} por primera vez");

        OnInventoryChange.Invoke(inventory);
        

    }

    public void RemoveOne (ItemData itemData)
    {
        StackItem stackToRemove = null;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemData.displayName == itemData.displayName)
            {
                inventory[i].RemoveFromStack();
                if(inventory[i].StackSize<=0)
                {
                    stackToRemove = inventory[i];
                   
                }

                
            }
        }
        if(stackToRemove!=null)
        {
            inventory.Remove(stackToRemove);
        }

        OnInventoryChange.Invoke(inventory);
    }

    public void Remove(ItemData itemData)
    {
        StackItem stackToRemove = null;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemData.displayName == itemData.displayName)
            {
                stackToRemove = inventory[i];
            }
        }
        inventory.Remove(stackToRemove);
        OnInventoryChange.Invoke(inventory);
    }



}
