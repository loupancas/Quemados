using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner _runnerPrefab;
    [SerializeField] private MainMenuHandler _mainMenuHandler;
    NetworkRunner _currentRunner;

    public event Action OnJoinedLobby = delegate { };
    
    public event Action<List<SessionInfo>> OnSessionListUpdate = delegate { };
    private Dictionary<PlayerRef, string> _playerNames = new Dictionary<PlayerRef, string>();

    #region LOBBY

    public void JoinLobby()
    {
        if (_currentRunner) Destroy(_currentRunner.gameObject);

        _currentRunner = Instantiate(_runnerPrefab);
        
        _currentRunner.AddCallbacks(this);

        JoinLobbyAsync();
    }

    async void JoinLobbyAsync()
    {
        var result = await _currentRunner.JoinSessionLobby(SessionLobby.Custom, "Normal Lobby");

        if (!result.Ok)
        {
            Debug.LogError("[Custom error] Unable to Join Lobby");
        }
        else
        {
            Debug.Log("[Custom Msg] Joined Lobby");

            OnJoinedLobby();
        }
    }
    
    #endregion

    #region Join / Create Game

    public async void CreateGame(string sessionName, string sceneName)
    {
        await InitializeGame(GameMode.Shared, sessionName, SceneUtility.GetBuildIndexByScenePath($"Scenes/{sceneName}"));
    }
    
    public async void JoinGame(SessionInfo sessionInfo)
    {
        await InitializeGame(GameMode.Shared, sessionInfo.Name, SceneManager.GetActiveScene().buildIndex);
      
    }
    
    async Task InitializeGame(GameMode gameMode, string sessionName, int sceneIndex)
    {
        _currentRunner.ProvideInput = gameMode == GameMode.Shared;

        var result = await _currentRunner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            Scene =  SceneRef.FromIndex(sceneIndex),
            SessionName = sessionName
        });
        
        if (!result.Ok)
        {
            Debug.LogError("[Custom error] Unable to start game");
        }
        else
        {
           
            Debug.Log("[Custom Msg] Game started");
            _mainMenuHandler?.OnStartGame();
        }
    }

    #endregion

    #region Used Runner Callbacks

  


    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        //if (sessionList.Count == 0) return;

        //var session = sessionList[0];

        //Debug.Log($"[Custom Msg] Joining {session.Name}");

        //JoinGame(session);

        OnSessionListUpdate(sessionList);
    }

    #endregion


    private void Start()
    {
       // var ClientTask = ISupportInitializeNotification
    }

    #region Unused Runner Callbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)    
    {
        

    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
  
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }

    public void OnConnectedToServer(NetworkRunner runner)
    {       
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    #endregion
}
