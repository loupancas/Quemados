using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using UnityEngine.TextCore.Text;

[DefaultExecutionOrder(-200)]
public class GameManager : NetworkBehaviour, IStateAuthorityChanged, IPlayerLeft
{
    [field: SerializeField] public Transform EntityHolder { get; private set; }

    #region Singleton

    public static GameManager instance;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("Instance already exists!");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    #endregion

    public override void Spawned()
    {
        //InterfaceManager.instance.roomNameText.text = Runner.SessionInfo.Name;
    }

    public void StateAuthorityChanged()
    {
        if (Runner.IsSharedModeMasterClient)
        {
            CleanupLeftPlayers();
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsSharedModeMasterClient)
        {
            CleanupLeftPlayers();
        }
    }

    void CleanupLeftPlayers()
    {
        //var allAuths = Runner.GetAllBehaviours<AuthorityHandler>();

        //Character[] objs = FindObjectsOfType<Character>()
        //    .Where(c => !Runner.ActivePlayers.Contains(c.Object.StateAuthority))
        //    .ToArray();

        //foreach (Character c in objs)
        //{
        //    foreach (var auth in allAuths.Where(a => a.Object.StateAuthority == c.Object.StateAuthority))
        //    {
        //        if (auth.Object == c.Object) auth.RequestAuthority(() => Runner.Despawn(auth.Object));
        //        else auth.RequestAuthority();
        //    }
        //}
    }
}