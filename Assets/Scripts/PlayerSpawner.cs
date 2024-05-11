using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] public List<Transform> spawnPoints;
    //Se ejecuta CADA VEZ que se conecta un cliente
    public void PlayerJoined(PlayerRef player)
    {
        //Si el player que entro es el jugador local, entonces "Instanciamos" en red a su player
        if (player == Runner.LocalPlayer)
        {
            int currentPlayer = -1;
            foreach (var item in Runner.ActivePlayers)
            {
               
                currentPlayer++;
            }

            Vector3 spawnPosition = spawnPoints.Count - 1 <= currentPlayer ? Vector3.zero : spawnPoints[currentPlayer].position;

            Runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
