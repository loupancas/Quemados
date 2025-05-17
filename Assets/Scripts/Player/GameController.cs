using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;


	public class GameController : NetworkBehaviour ,IPlayerJoined
	{
		enum GamePhase
		{
			Starting,
			Running,
			Ending
		}

		[SerializeField] private float _startDelay = 60.0f;
		[SerializeField] private float _endDelay = 60.0f;
		[SerializeField] private float _gameSessionLength = 180.0f;
		
		[SerializeField] private TextMeshProUGUI _startEndDisplay;
		[SerializeField] private TextMeshProUGUI _ingameTimerDisplay;
		[SerializeField] private PlayerOverviewPanel _playerOverview;

		[Networked] private TickTimer Timer { get; set; }
		[Networked] private GamePhase Phase { get; set; }
		[Networked] private NetworkBehaviourId Winner { get; set; }
        [Networked] private NetworkBehaviourId Loser { get; set; }
    public bool GameIsRunning => Phase == GamePhase.Running;


		private TickTimer _dontCheckforWinTimer;
		
		public List<NetworkBehaviourId> _playerDataNetworkedIds = new List<NetworkBehaviourId>();
		
		private static GameController _singleton;

		public static GameController Singleton
		{
			get => _singleton;
			private set
			{
				if (_singleton != null)
				{
					throw new InvalidOperationException();
				}
				_singleton = value;
			}
		}

		private void Awake()
		{
			GetComponent<NetworkObject>().Flags |= NetworkObjectFlags.MasterClientObject;
			Singleton = this;
		}

		private void OnDestroy()
		{
			if (Singleton == this)
			{
				_singleton = null;
			}
			else
			{
				throw new InvalidOperationException();
			}

		}

		public override void Spawned()
		{
			_startEndDisplay.gameObject.SetActive(false);
			_ingameTimerDisplay.gameObject.SetActive(false);
			_playerOverview.Clear();

			
			if (Object.HasStateAuthority)
			{
      

            Phase = GamePhase.Starting;
            Timer = TickTimer.CreateFromSeconds(Runner, _startDelay);
			}
		}

		public void ActivateStartEndDisplay()
		{
			_startEndDisplay.gameObject.SetActive(true);
		}

    public override void Render()
		{
       // Debug.Log($"Client {Runner.LocalPlayer}: Phase = {Phase}");
        switch (Phase)
			{
				case GamePhase.Starting:
					UpdateStartingDisplay();
					break;
				case GamePhase.Running:
					UpdateRunningDisplay();
					if (HasStateAuthority)
					{
						CheckIfGameHasEnded();
					}
					break;
				case GamePhase.Ending:
					UpdateEndingDisplay();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void UpdateStartingDisplay()
		{
        // --- All clients
        // Display the remaining time until the game starts in seconds (rounded down to the closest full second)

        _startEndDisplay.text = $"Game Starts In {Mathf.RoundToInt(Timer.RemainingTime(Runner) ?? 0)}";
        _startEndDisplay.gameObject.SetActive(true);
        if (!Object.HasStateAuthority) 
				return;

			if (!Timer.Expired(Runner)) 
				return;

			// --- Master client
			// Starts the Spaceship and Asteroids spawners once the game start delay has expired
			FindObjectOfType<RoomM>().StartRoom(this);
        Debug.Log("Game is now running!");
        // Switches to the Running GameState and sets the time to the length of a game session
        Phase = GamePhase.Running;
			Timer = TickTimer.CreateFromSeconds(Runner, _gameSessionLength);
			_dontCheckforWinTimer = TickTimer.CreateFromSeconds(Runner, 175);
		}

		private void UpdateRunningDisplay()
		{
			// --- All clients
			// Display the remaining time until the game ends in seconds (rounded down to the closest full second)
			_startEndDisplay.gameObject.SetActive(false);
			_ingameTimerDisplay.gameObject.SetActive(true);
			_ingameTimerDisplay.text = $"{Mathf.RoundToInt(Timer.RemainingTime(Runner) ?? 0).ToString("000")} seconds left... Player RTT: {(int)(1000*Runner.GetPlayerRtt(Runner.LocalPlayer))}ms";
		//Debug.Log("juego en proceso");

		}

		private void UpdateEndingDisplay()
		{
			// --- All clients
			// Display the results and
			// the remaining time until the current game session is shutdown
			Debug.Log("udpate ending display!");
        if (Runner.TryFindBehaviour(Winner, out PlayerDataNetworked playerData) == false) return;

			_startEndDisplay.gameObject.SetActive(true);
			_ingameTimerDisplay.gameObject.SetActive(false);
			_startEndDisplay.text = $"{playerData.NickName} won with {playerData.Score} points. Disconnecting in {Mathf.RoundToInt(Timer.RemainingTime(Runner) ?? 0)}";
        // Recuperar el color del jugador desde PlayerPrefs
        string playerName = playerData.NickName;
        string colorString = PlayerPrefs.GetString(playerName + "_Color", "#FFFFFF");
        if (ColorUtility.TryParseHtmlString("#" + colorString, out Color playerColor))
        {
            _startEndDisplay.color = playerColor;
        }

        if (Timer.Expired(Runner))
				Runner.Shutdown();

        if (Object.HasStateAuthority)
        {
            if (Runner.TryFindBehaviour(Winner, out PlayerDataNetworked player))
            {
                var playerRef = GetPlayerRef(Winner);
                RoomM.Instance.RPC_PlayerWin(playerRef);
            }

            if (Runner.TryFindBehaviour(Loser, out PlayerDataNetworked playerLoser))
            {
                var playerRef = GetPlayerRef(Loser);
                RoomM.Instance.RPC_PlayerLose(playerRef);
            }
        }


    }

   
    public void CheckIfGameHasEnded()
		{
			// --- Master client
			
			if (Timer.ExpiredOrNotRunning(Runner))
			{
			//si vencio el tiempo del juego
				GameHasEnded();
				return;
			}

			if (_dontCheckforWinTimer.Expired(Runner) == false)
			{
			//no terminar el juego tan pronton
				return;
			}
			

			int playersAlive = 0;
			//contabiliza jugadores vivos
			for (int i = 0; i < _playerDataNetworkedIds.Count; i++)
			{
				if (Runner.TryFindBehaviour(_playerDataNetworkedIds[i],
					    out PlayerDataNetworked playerDataNetworkedComponent) == false)
				{
					_playerDataNetworkedIds.RemoveAt(i);
					i--;
					continue;
				}

				if (playerDataNetworkedComponent.Lives > 0) playersAlive++;
			}
			
			
			//continuar o finalizar
			if (playersAlive > 1 || (Runner.ActivePlayers.Count() == 1 && playersAlive == 1)) return;

			foreach (var playerDataNetworkedId in _playerDataNetworkedIds)
			{
				if (Runner.TryFindBehaviour(playerDataNetworkedId,
					    out PlayerDataNetworked playerDataNetworkedComponent) ==
				    false) continue;

				if (playerDataNetworkedComponent.Lives > 0 == true) 
			    {
                  Winner = playerDataNetworkedId;
                }
				else
			    {
                   Loser = playerDataNetworkedId;
                }
				
			}
			//ganador
			if (Winner == default && _playerDataNetworkedIds.Count > 0) 
			{
				Winner = _playerDataNetworkedIds[0];
			}

			GameHasEnded();
		}
    
    private void GameHasEnded()
		{
			Timer = TickTimer.CreateFromSeconds(Runner, _endDelay);
			Phase = GamePhase.Ending;
       
    }
    private Dictionary<NetworkBehaviourId, PlayerRef> _playerRefMapping = new Dictionary<NetworkBehaviourId, PlayerRef>();

    public void TrackNewPlayer(NetworkBehaviourId playerDataNetworkedId, PlayerRef playerRef)

        {
			_playerDataNetworkedIds.Add(playerDataNetworkedId);
        _playerRefMapping[playerDataNetworkedId] = playerRef;
    }

    private PlayerRef GetPlayerRef(NetworkBehaviourId id)
    {
        return _playerRefMapping.TryGetValue(id, out var playerRef) ? playerRef : default;
    }

    public void PlayerJoined(PlayerRef player)
		{
        Debug.Log($"Player {player} joined. Current Phase: {Phase}");
        _dontCheckforWinTimer = TickTimer.CreateFromSeconds(Runner, 5);
		}
	}
