using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Room _RoomManager;
    [SerializeField] private GameObject _readyButton;
    public static PlayerSpawner Instance;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        // GeneratePowerUpPositions();
    }
    // Se ejecuta CADA VEZ que se conecta un cliente
    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            _readyButton.SetActive(true);
            if (Runner.ActivePlayers.Count() == 1)
            {
                Runner.Spawn(_RoomManager);
            }
            int currentPlayer = 0;
            foreach (var item in Runner.ActivePlayers)
            {
                if (item == player) break; //No funciona
                currentPlayer++;
            }

            Vector3 spawnPosition = currentPlayer < spawnPoints.Length ? spawnPoints[currentPlayer].position : Vector3.zero;
            Debug.Log($"Player {player} joined, spawning at {spawnPosition}");
            Runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity);

        }
    }

    public void SpawnPlayer()
    {
        Runner.Spawn(_playerPrefab, spawnPoints[0].position, spawnPoints[0].rotation);
    }


}
