using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private RoomM _RoomManager;
    [SerializeField] private GameObject _readyButton;
    [SerializeField] private BallPickUp _ballPickUpPrefab;
    [SerializeField] private Ball2 _ballPrefab;
    public static PlayerSpawner Instance;
    private List<Transform> availableSpawnPoints;
    private int playerCount = 0;
    public List<Ball2> SpawnedBalls { get; private set; } = new List<Ball2>();
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
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("NetworkRunner not found in the scene.");
            return;
        }

    }
    // Se ejecuta CADA VEZ que se conecta un cliente
    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            _readyButton.SetActive(true);

            if (Runner.ActivePlayers.Count() == 1)
            {
                if (_RoomManager != null)
                {
                    Runner.Spawn(_RoomManager);
                    SpawnBallPickUp();

                }
                else
                {
                    Debug.LogError("RoomManager is not assigned!");
                }
            }

            //var gameController = FindObjectOfType<GameController>();
            //if (gameController != null)
            //{
                //gameController.AssignPlayerColor(player);
            //}
            //else
            //{
                //Debug.LogError("GameController not found in the scene.");
           // }
        }
    }

   

    public void SpawnPlayer()
    {
        if (_playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned!");
            return;
        }
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("No available spawn points!");
            return;
        }

        // Selecciona un punto de aparición aleatorio de los disponibles
        int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
        Transform spawnPoint = availableSpawnPoints[spawnIndex];

        // Spawnea el jugador en el punto seleccionado
        var spawnedPlayer = Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation);
        if (spawnedPlayer == null)
        {
            Debug.LogError("Failed to spawn player!");
            return;
        }

        // Elimina el punto de aparición de la lista de disponibles
        availableSpawnPoints.RemoveAt(spawnIndex);
    }

    private void SpawnBallPickUp()
    {
        if (_ballPickUpPrefab != null)
        {
            // Selecciona un punto de aparición aleatorio de los disponibles
            int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[spawnIndex];

            // Spawnea el BallPickUp en el punto seleccionado
            var spawnedBallPickUp = Runner.Spawn(_ballPickUpPrefab, new Vector3(0, 1, 0), spawnPoint.rotation);
            if (spawnedBallPickUp == null)
            {
                Debug.LogError("Failed to spawn BallPickUp!");
                return;
            }


        }
        //else if(_ballPrefab != null)
        //{

        //    var spawnedBall = Runner.Spawn(_ballPrefab, null, null);
        //    if (spawnedBall == null)
        //    {
        //        Debug.LogError("Failed to spawn Ball2!");
        //        return;
        //    }

        //    Ball2 newBall = spawnedBall.GetComponent<Ball2>();
        //    if (newBall != null)
        //    {
        //        SpawnedBalls.Add(newBall);
        //        Debug.Log("Ball2 added");
        //    }
        //    else
        //    {
        //        Debug.LogError("Failed to get Ball2 component!");
        //    }
        //}
        else
        {
            Debug.LogError("BallPickUp prefab is not assigned!");
        }
    }
}
