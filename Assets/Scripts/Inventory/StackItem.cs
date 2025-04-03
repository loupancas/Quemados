using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]

public class StackItem 
{
    public ItemData itemData;

    private int stackSize; //encapsulado
    public int StackSize { get => stackSize; set => stackSize = value; }

    public StackItem(ItemData item)  // constructor
    {
        itemData = item;
        addToStack();
    }

   

    public void addToStack()
    {
        stackSize++;
    }

    public void RemoveFromStack()
    {
        stackSize--;
    }



}
