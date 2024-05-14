using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] public List<Transform> spawnPoints;
   
    // [SerializeField] private GameObject[] _powerUpsPrefabs;
    // [SerializeField] private int _numberOfPowerUps;
    // [SerializeField] private float _minDistanceBetweenPowerUps;
    private List<Vector3> _powerUpSpawnPositions = new List<Vector3>();
    private int _nextSpawnPointIndex = 0;

    private void Start()
    {
        // GeneratePowerUpPositions();
    }
    // Se ejecuta CADA VEZ que se conecta un cliente
    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Vector3 spawnPosition = GetNextSpawnPoint();

            Runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity);

            //// Spawn power-ups for the new player
            //foreach (Vector3 powerUpSpawnPosition in _powerUpSpawnPositions)
            //{
            //    foreach (GameObject powerUpPrefab in _powerUpsPrefabs)
            //    {
            //        Runner.Spawn(powerUpPrefab, powerUpSpawnPosition, Quaternion.identity);
            //    }
            //}

            // for (int i = 0; i < _powerUpSpawnPositions.Count; i++)
            // {
            //     // Ensure that there are enough spawn positions for both types of power-ups
            //     if (i < _powerUpsPrefabs.Length)
            //     {
            //         Runner.Spawn(_powerUpsPrefabs[i], _powerUpSpawnPositions[i], Quaternion.identity);
            //     }
            //     else
            //     {
            //         Debug.LogError("Not enough power-up prefabs to match spawn positions.");
            //         break;
            //     }
            // }
        }
    }

    private Vector3 GetNextSpawnPoint()
    {
        // Ensure we don't exceed the available spawn points
        if (_nextSpawnPointIndex >= spawnPoints.Count)
        {
            _nextSpawnPointIndex = 0;
        }

        Vector3 spawnPosition = spawnPoints[_nextSpawnPointIndex].position;
        _nextSpawnPointIndex++;
        return spawnPosition;
    }

    // private void GeneratePowerUpPositions()
    // {
    //     for (int i = 0; i < _numberOfPowerUps; i++)
    //     {
    //         // Generate random spawn position for power-ups
    //         Vector3 spawnPosition = GetRandomSpawnPoint(_minDistanceBetweenPowerUps, Vector3.zero);
    //         _powerUpSpawnPositions.Add(spawnPosition);
    //     }
    // }

    // private Vector3 GetRandomSpawnPoint(float minDistance, Vector3 center)
    // {
    //     ObjectsSpawner objectsSpawner = ObjectsSpawner.instance;
    //     Vector3 randomPoint = objectsSpawner.GetRandomSpawnPoint(minDistance, center);
    //     return randomPoint;
    // }

}
