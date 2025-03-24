using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] public List<Transform> spawnPoints;
    [SerializeField] private GameManager _gameManager;

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
            int currentPlayer = -1;
            foreach (var item in Runner.ActivePlayers)
            {
                //if (item == player) break; //No funciona
                currentPlayer++;
            }

            Vector3 spawnPosition = spawnPoints.Count - 1 <= currentPlayer ? Vector3.zero : spawnPoints[currentPlayer].position;
            Debug.Log($"Player {player} joined, spawning at {spawnPosition}");
            Runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity);
         
        }
    }

    public void SpawnPlayer()
    {
        Runner.Spawn(_playerPrefab, spawnPoints[0].position, spawnPoints[0].rotation);
    }


}
