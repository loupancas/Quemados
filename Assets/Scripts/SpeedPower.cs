using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPower : Pickup
{
    public float Modifier=1.8f;
    public string pickUpName = "Speed";
    public override void Activate()
    {
        Debug.Log("Speed");

       
    }

   
}
