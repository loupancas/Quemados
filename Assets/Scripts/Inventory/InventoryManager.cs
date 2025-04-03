using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public List<InventorySlots> inventorySlots = new List<InventorySlots>(4);

    private void OnEnable() //suscribir
    {
        Inventario.OnInventoryChange += DrawInventory;
        Inventario.OnInventoryUpdate += UpdateInventory;
    }
    private void OnDisabled() //desuscribir
    {
        Inventario.OnInventoryChange -= DrawInventory;
        Inventario.OnInventoryUpdate -= UpdateInventory;
    }



    void resetInvetory()
    {
        foreach(Transform childTransform in transform)
        {
            Destroy(childTransform.gameObject);
        }
        inventorySlots = new List<InventorySlots>(4);
    }

    void DrawInventory(List<StackItem>inventory)
    {
        resetInvetory();
        for (int i = 0; i < inventorySlots.Capacity; i++)
        {
            createInventorySlot();
        }

        for (int i = 0; i < inventory.Count; i++)
        {
            inventorySlots[i].DrawSlot(inventory[i]);
        }


    }



    void UpdateInventory(List<StackItem> inventory)
    {

        for (int i = 0; i < inventory.Count; i++)
        {
            inventorySlots[i].DrawSlot(inventory[i]);
        }


    }

    void createInventorySlot()
    {

       // if (slotPrefab != null)
        {
            GameObject newSlot = Instantiate(slotPrefab);
            newSlot.transform.SetParent(transform, false);

            InventorySlots newSlotComponent = newSlot.GetComponent<InventorySlots>();
            newSlotComponent.ClearSlot();

            inventorySlots.Add(newSlotComponent);

        }


    }

}
