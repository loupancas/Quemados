using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base component for equipment interactions.
/// Interactions are associated with equipment via <see cref="Equipment.InteractionType"/>
/// </summary>
public abstract class InteractionBase : MonoBehaviour
{
    //[field: SerializeField] public AnimationResponse AnimationResponse { get; private set; }
    public UnityEvent OnInteract;
    public UnityEvent<Player> OnInteractCharacter;
    
    public virtual void Interact(Player interactor)
    {
        Debug.Log(GetType().Name);
        OnInteract?.Invoke();
        OnInteractCharacter?.Invoke(interactor);
    }
}
