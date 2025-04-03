using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Pickeable : MonoBehaviour
{
   
   public void EVENT_Pick()
   {
        onPick();
   }

    protected abstract void onPick();

}
