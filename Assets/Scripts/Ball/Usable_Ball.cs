using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Usable_Ball : Usable
{
    public Transform pointToDrop;
    //public _ball ballModel;
    public override void Use()
    {
        print("drop bomb");
        //_ball ball = Instantiate(bombModel);

        //ball.throwBomb(pointToDrop.position, pointToDrop.forward);
    }
}
