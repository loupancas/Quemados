using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class RoomM : NetworkBehaviour
{
    public static RoomM Instance;
    private GameController _gameStateController = null;
    //Dictionary<int, Player> _activePlayers;
    private Dictionary<PlayerRef, bool> _playerStates = new Dictionary<PlayerRef, bool>();
    [Networked] public bool isGameStart { get; set; } = false;
    private void Awake()
    {


        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Debug.Log("RoomM awake");
        //_activePlayers = new Dictionary<int, Player>();
    }

    public void StartRoom(GameController gameController)
    {
        isGameStart = true;
        _gameStateController = gameController;
    }

    public override void Spawned()
    {
        RpcAddPlayer(Runner.LocalPlayer);
        UIManager.instance.SetPlayerRef(Runner.LocalPlayer);
        Debug.Log("RoomM spawned");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcAddPlayer(PlayerRef player)
    {
        _playerStates.TryAdd(player, false);

        foreach (var state in _playerStates)
        {
            Debug.Log($"Player ref {state.Key}: {state.Value}");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcOnPlayerConfirm(PlayerRef playerRef)
    {
        if (!_playerStates.ContainsKey(playerRef)) return;

        _playerStates[playerRef] = true;

        if (Runner.ActivePlayers.Count() < 2) return;

        var everyoneIsReady = true;

        foreach (var state in _playerStates)
        {
            if (state.Value == false)
                everyoneIsReady = false;

            Debug.Log($"Player ref {state.Key}: {state.Value}");
        }

        if (everyoneIsReady)
        {
            Debug.Log("RoomM confirmed");
            isGameStart = true;
            RpcStartGame();
            foreach (var actualPlayerRef in _playerStates.Keys)
            {
                RpcSpawnPlayer(actualPlayerRef);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcSpawnPlayer(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            PlayerSpawner.Instance.SpawnPlayerWithDelay(player, 1f);
        UIManager.instance.StartGame();

        }
       // UIManager.instance.StartGame();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcStartGame()
    {
        UIManager.instance.StartGame();
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
