using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private RoomM _RoomManager;
    [SerializeField] private GameObject _readyButton;
    [SerializeField] private BallPickUp _ballPickUpPrefab;
    //[SerializeField] private Ball2 _ballPrefab;
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
                SpawnBallPickUp();
            }
           

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

    private void SpawnBallPickUp()
    {
        if (_ballPickUpPrefab != null)
        {
            // Selecciona un punto de aparición aleatorio de los disponibles
            int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[spawnIndex];

            // Spawnea el BallPickUp en el punto seleccionado
            Runner.Spawn(_ballPickUpPrefab, new Vector3(0,1,0), spawnPoint.rotation);

            // Elimina el punto de aparición de la lista de disponibles
            //availableSpawnPoints.RemoveAt(spawnIndex);

            //Ball2 ball = Runner.Spawn(_ballPickUpPrefab, new Vector3(0, 1, 0), spawnPoint.rotation).GetComponent<Ball2>();

            // Inicializa la pelota si es necesario
            //if (_ballPrefab != null)
            //{
            //   Runner.Spawn(_ballPrefab, new Vector3(0, 0, 0), spawnPoint.rotation);
            //}



        }
        else
        {
            Debug.LogError("BallPickUp prefab is not assigned!");
        }
    }
}
