using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private RoomM _RoomManager;
    [SerializeField] private GameObject _readyButton;
    [SerializeField] private GameObject _ballPickUpPrefab;
    [SerializeField] private PlayerOverviewPanel _playerOverviewPanel;
    public static PlayerSpawner Instance;
    private List<Transform> availableSpawnPoints;
    public event System.Action<PlayerRef> OnPlayerJoinedEvent;
    [SerializeField] private List<Transform> spawnPoints; // Lista de puntos de spawn
    public List<NetworkObject> Players { get; private set; } = new List<NetworkObject>();


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
     
        Debug.Log("PlayerSpawner awake");
    }

   
    private void Start()
    {
        Debug.Log("PlayerSpawner started");
       
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
            string playerName = PlayerPrefs.GetString("PlayerNickName", "UnknownPlayer");
            
        }
       
        OnPlayerJoinedEvent?.Invoke(player);
    }


    public void SpawnPlayerWithDelay(PlayerRef player, float delay)
    {
        StartCoroutine(SpawnPlayerCoroutine(player, delay));
    }

    private IEnumerator SpawnPlayerCoroutine(PlayerRef player, float delay)
    {
        // Esperar el tiempo especificado
        yield return new WaitForSeconds(delay);

        // Obtener el próximo punto de spawn
        Transform spawnPoint = GetNextSpawnPoint();

        // Instanciar al jugador en el punto de spawn
        NetworkObject playerObject = Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation, player);
        Players.Add(playerObject);
        Debug.Log($"Player {player} spawned at {spawnPoint.position}");
    }


 

    private Transform GetNextSpawnPoint()
    {
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        return spawnPoint;
    }


   
    private void SpawnBallPickUp()
    {
        if (_ballPickUpPrefab != null)
        {
            // Selecciona un punto de aparición aleatorio de los disponibles
            int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[spawnIndex];

            // Spawnea el BallPickUp en el punto seleccionado
            NetworkObject ballObject = Runner.Spawn(_ballPickUpPrefab, new Vector3(0,1,0), spawnPoint.rotation);
           

        }
        else
        {
            Debug.LogError("BallPickUp prefab is not assigned!");
        }
    }

   



}
