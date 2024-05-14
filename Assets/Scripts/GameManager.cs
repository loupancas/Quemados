using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    Dictionary<int, Player> _activePlayers;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _activePlayers = new Dictionary<int, Player>();
    }

    public void AddNewPlayer(int i, Player p)
    {
        _activePlayers.TryAdd(i, p);
        Debug.Log("Agregado");
    }

    [Rpc]
    public void RPC_PlayerWin(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Debug.Log("Win");
            UIManager.instance.SetVictoryScreen();
            return;
        }

        Debug.Log("Defeat");
        UIManager.instance.SetLoseScreen();
    }
}
