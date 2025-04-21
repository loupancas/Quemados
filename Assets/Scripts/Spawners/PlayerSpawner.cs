using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;
    //[SerializeField] private Transform[] spawnPoints;
    [SerializeField] private RoomM _RoomManager;
    [SerializeField] private GameObject _readyButton;
    [SerializeField] private GameObject _ballPickUpPrefab;
    //[SerializeField] private BallBehaviour _ballPrefab;
    [SerializeField] private PlayerOverviewPanel _playerOverviewPanel;
    public static PlayerSpawner Instance;
    private List<Transform> availableSpawnPoints;
    public event System.Action<PlayerRef> OnPlayerJoinedEvent;
    [SerializeField] private List<Transform> spawnPoints; // Lista de puntos de spawn
    private int nextSpawnIndex = 0; // Índice del próximo punto de spawn


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

        Debug.Log($"Player {player} spawned at {spawnPoint.position}");
    }


    //public void SpawnPlayer(PlayerRef player)
    //{
    //    // Obtener el próximo punto de spawn
    //    Transform spawnPoint = GetNextSpawnPoint();

    //    // Instanciar al jugador en el punto de spawn
    //    NetworkObject playerObject = Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation, player);

    //    Debug.Log($"Player {player} spawned at {spawnPoint.position}");
    //}

    private Transform GetNextSpawnPoint()
    {
        // Obtener el punto de spawn actual y avanzar al siguiente
        //Transform spawnPoint = spawnPoints[nextSpawnIndex];
        //nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Count; // Ciclar entre los puntos
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        return spawnPoint;
    }


    //public void SpawnPlayer()
    //{
    //    if (availableSpawnPoints.Count == 0)
    //    {
    //        Debug.LogError("No available spawn points!");
    //        return;
    //    }

    //    int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
    //    Transform spawnPoint = availableSpawnPoints[spawnIndex];

    //    NetworkObject playerObject = Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation, Runner.LocalPlayer);

    //    if (playerObject == null)
    //    {
    //        Debug.LogError("Failed to spawn player object!");
    //        return;
    //    }

    //    // Get the BallBehaviour component
    //    //_ballPrefab = playerObject.GetComponentInChildren<BallBehaviour>();

    //    //if (_ballPrefab == null)
    //    //{
    //    //    Debug.LogError("BallBehaviour component not found in the spawned player prefab!");
    //    //    return;
    //    //}

    //    // Resolve the Player component from the spawned object
    //    //Player player = playerObject.GetComponent<Player>();

    //    //if (player != null)
    //    //{
    //    //    _ballPrefab.Initialize(player, _playerPrefab);
    //    //}
    //    //else
    //    //{
    //    //    Debug.LogError("Failed to resolve Player component from the spawned player object!");
    //    //}

    //    availableSpawnPoints.RemoveAt(spawnIndex);
    //   // OnPlayerJoinedEvent?.Invoke(Runner.LocalPlayer);
    //}

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
        //else if(_ballPrefab != null)
        //{

        //    BallBehaviour newBall = Runner.Spawn(_ballPrefab, null, null).GetComponent<BallBehaviour>();
        //    //SpawnedBalls.Add(newBall);

        //   Debug.Log("se añade ball2");
        //}
        else
        {
            Debug.LogError("BallPickUp prefab is not assigned!");
        }
    }

   



}
