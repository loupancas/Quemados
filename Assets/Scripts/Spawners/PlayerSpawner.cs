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
    [SerializeField] private GameObject _ballPickUpPrefab;
    //[SerializeField] private BallBehaviour _ballPrefab;
    [SerializeField] private PlayerOverviewPanel _playerOverviewPanel;
    public static PlayerSpawner Instance;
    private List<Transform> availableSpawnPoints;
    public event System.Action<PlayerRef> OnPlayerJoinedEvent;

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

   
    public void SpawnPlayer()
    {
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("No available spawn points!");
            return;
        }

        int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
        Transform spawnPoint = availableSpawnPoints[spawnIndex];

        Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation);
        availableSpawnPoints.RemoveAt(spawnIndex);
       // OnPlayerJoinedEvent?.Invoke(Runner.LocalPlayer);
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
            BallBehaviour ballBehaviour = ballObject.GetComponent<BallBehaviour>();
            //ballBehaviour.Initialize( Player.LocalPlayer   , _playerPrefab);

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
