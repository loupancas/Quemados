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
    private List<Transform> availableSpawnPoints;
    private int playerCount = 0;
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
        availableSpawnPoints = new List<Transform>(spawnPoints);
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
            //int currentPlayer = 0;
            //foreach (var item in Runner.ActivePlayers)
            //{
            //    if (item == player) break; //No funciona
            //    currentPlayer++;
            //}

            //Vector3 spawnPosition = currentPlayer < spawnPoints.Length ? spawnPoints[currentPlayer].position : Vector3.zero;
            //Debug.Log($"Player {player} joined, spawning at {spawnPosition}");
            //Runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity);

        }
    }

    public void SpawnPlayer()
    {
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("No available spawn points!");
            return;
        }

        // Selecciona un punto de aparición aleatorio de los disponibles
        int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
        Transform spawnPoint = availableSpawnPoints[spawnIndex];

        // Spawnea el jugador en el punto seleccionado
        Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation);

        // Elimina el punto de aparición de la lista de disponibles
        availableSpawnPoints.RemoveAt(spawnIndex);
    }


}
