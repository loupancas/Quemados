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
    [SerializeField] private Ball2 _ballPrefab;
    [SerializeField] private PlayerOverviewPanel _playerOverviewPanel;
    public static PlayerSpawner Instance;
    private List<Transform> availableSpawnPoints;
    private int playerCount = 0;
    //public List<Ball2> SpawnedBalls { get; private set; } = new List<Ball2>();
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
        // GeneratePowerUpPositions();
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
        //ApplyPlayerData(_playerPrefab);
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

            
        }
        else if(_ballPrefab != null)
        {

            Ball2 newBall = Runner.Spawn(_ballPrefab, null, null).GetComponent<Ball2>();
            //SpawnedBalls.Add(newBall);

           Debug.Log("se añade ball2");
        }
        else
        {
            Debug.LogError("BallPickUp prefab is not assigned!");
        }
    }


    public void ApplyPlayerData()
    {
        string playerName = PlayerPrefs.GetString("PlayerNickName", "DefaultPlayer");
        string sharedSessionName = PlayerPrefs.GetString("SharedSessionName", "DefaultSession");
        string colorString = PlayerPrefs.GetString(playerName + "_Color", "#FFFFFF");

        //if (ColorUtility.TryParseHtmlString("#" + colorString, out Color playerColor))
        //{
        //    Player playerComponent = playerObject.GetComponent<Player>();
        //    if (playerComponent != null)
        //    {
        //        playerComponent.SetPlayerColor(playerColor);
        //    }
        //}

        //// Crear y agregar PlayerDataNetworked
        //    PlayerDataNetworked playerData = new PlayerDataNetworked
        //    {
        //        NickName = playerName,
        //        Score = 0, // Inicializa con el puntaje inicial
        //        Lives = 3 // Inicializa con el número de vidas inicial
        //    };

        //    // Agregar la entrada al PlayerOverviewPanel
        //    _playerOverviewPanel.AddEntry(Runner.LocalPlayer, playerData);
       
    }


}
