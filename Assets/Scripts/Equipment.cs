using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Equipment provides a means of interacting with the game world.
/// Derive from <see cref="Equipment{T}"/> rather than directly from this class.
/// </summary>
public abstract class Equipment : NetworkBehaviour
{
   // [field: SerializeField] public EquipmentData Data { get; private set; }

    /// <summary>
    /// The type of interaction associated with the equipment.
    /// </summary>
    public abstract System.Type InteractionType { get; }

    /// <summary>
    /// If true, the equipment can be Used without the presence of an interactable.
    /// </summary>
    public abstract bool RequiresInteractable { get; }

    //AnimationResponse ResolveAnimationResponse(InteractionBase interaction) => (interaction && interaction.AnimationResponse) ? interaction.AnimationResponse : Data.AnimationResponse;

    //public void Use(Player User)
    //{
    //    //InteractionBase interaction = null;
    //    //var hits = Physics.OverlapSphere(User.InteractionPoint.position, User.InteractionRadius);
    //    //foreach (var hit in hits)
    //    //{
    //    //    // if (hit.transform.TryGetComponentInParent(InteractionType, out Component c))
    //    //    if (hit.transform.TryGetComponentInParent(InteractionType, out Component c))
    //    //    {
    //    //        interaction = (InteractionBase)c;
    //    //        break;
    //    //    }
    //    //}

    //    //AnimationResponse response = ResolveAnimationResponse(interaction);

    //    if (interaction || !RequiresInteractable)
    //    {
    //        //if (Runner.IsForward)
    //        //{
    //        //    if (response && !string.IsNullOrWhiteSpace(response.Animation)) User.SetAnimation(response.Animation);
    //        //}

    //        //Angle? angle = interaction == null ? null : Quaternion.LookRotation(interaction.transform.position - User.transform.position).eulerAngles.y;
    //        //if (response) User.LockInput(response.AnimationDuration, angle);

    //        StartCoroutine(ActionRoutine());
    //    }

    //    IEnumerator ActionRoutine()
    //    {
    //        yield return new WaitForSeconds(3);

    //        if (interaction)
    //        {
    //            interaction.Interact(User);
    //            OnUsed(interaction);
    //        }
    //        else if (!RequiresInteractable)
    //        {
    //            OnUsed(null);
    //        }
    //    }
    //}

    protected virtual void OnUsed(InteractionBase usedOn) { }
}

public abstract class Equipment<T> : Equipment where T : InteractionBase
{
    public sealed override System.Type InteractionType => typeof(T);
}
