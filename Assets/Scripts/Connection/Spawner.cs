using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;


public class Spawner : MonoBehaviour
{
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private List<Transform> _recentSpawnPoints = new List<Transform>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        var spawnPoint = PlayerSpawnPoint();
        if (runner.IsServer)
        {
            runner.Spawn(_playerPrefab, spawnPoint.position, null, player);
        }
    }

    private Player _characterInputHandler;

   

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        runner.Shutdown();
    }

    private Transform PlayerSpawnPoint()
    {
        Transform spawnPoint = null;
        var spawnPoints = FindObjectsOfType<SpawnPoints>();

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found!");
            return null;
        }

        int offset = UnityEngine.Random.Range(0, spawnPoints.Length);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoint = spawnPoints[(offset + i) % spawnPoints.Length].transform;

            if (!_recentSpawnPoints.Contains(spawnPoint))
                break;
        }

        _recentSpawnPoints.Add(spawnPoint);

        if (_recentSpawnPoints.Count > 2)
        {
            _recentSpawnPoints.RemoveAt(0);
        }

        return spawnPoint;



    }







    #region Unused callbacks

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

 

    #endregion
}
