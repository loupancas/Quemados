using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyhandedEquipment : Equipment<EmptyhandedInteraction>
{
    public override bool RequiresInteractable => true;
}
