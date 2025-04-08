using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;


	public class GameController : NetworkBehaviour ,IPlayerJoined
	{
		public enum GamePhase
		{
			Starting,
			Running,
			Ending
		}

		[SerializeField] private float _startDelay = 4.0f;
		[SerializeField] private float _endDelay = 4.0f;
		[SerializeField] private float _gameSessionLength = 180.0f;
		
		[SerializeField] private TextMeshProUGUI _startEndDisplay;
		[SerializeField] private TextMeshProUGUI _ingameTimerDisplay;
		[SerializeField] private PlayerOverviewPanel _playerOverview;

		[Networked] private TickTimer Timer { get; set; }
		[Networked] private GamePhase Phase { get; set; }
		[Networked] private NetworkBehaviourId Winner { get; set; }
		
	    public bool GameIsRunning => Phase == GamePhase.Running;

		public TickTimer GameStartTimer { get; private set; }
		public float GameStartDelay = 5.0f;

		private TickTimer _dontCheckforWinTimer;
		
		private List<NetworkBehaviourId> _playerDataNetworkedIds = new List<NetworkBehaviourId>();
        public static GameController Instance { get; private set; }
 

		private void Awake()
		{
			GetComponent<NetworkObject>().Flags |= NetworkObjectFlags.MasterClientObject;
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

		
		public override void Spawned()
		{
			_startEndDisplay.gameObject.SetActive(false);
			_ingameTimerDisplay.gameObject.SetActive(false);
			_playerOverview.Clear();

			
		}
		
		public void Starting()
		{

			if (Object.HasStateAuthority)
			{
				// Initialize the game state on the master client
				Phase = GamePhase.Starting;
				Timer = TickTimer.CreateFromSeconds(Runner, _startDelay);
			}
		}


		public override void Render()
		{
			// Update the game display with the information relevant to the current game phase
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

			if (!Object.HasStateAuthority) 
				return;

			if (!Timer.Expired(Runner)) 
				return;

		// --- Master client
		// Starts the Spaceship and Asteroids spawners once the game start delay has expired

		//FindObjectOfType<PlayerSpawner>().SpawnPlayer();
		//PlayerSpawner.Instance.SpawnPlayer();
		//Debug.Log("Player Spawned");
		if (!RoomM.Instance.isGameStart)
			return;
        // Switches to the Running GameState and sets the time to the length of a game session
            Phase = GamePhase.Running;
			Timer = TickTimer.CreateFromSeconds(Runner, _gameSessionLength);
			_dontCheckforWinTimer = TickTimer.CreateFromSeconds(Runner, 5);
	}

  //  public void StartRoom(RoomM room)
  //  {
  //      GameStartTimer = TickTimer.CreateFromSeconds(Runner, GameStartDelay);
		//_startEndDisplay.gameObject.SetActive(true);
  //      // Inicializa el estado del juego
  //      //Phase = GamePhase.Running;
  //      //Timer = TickTimer.CreateFromSeconds(Runner, _gameSessionLength);
  //      //_dontCheckforWinTimer = TickTimer.CreateFromSeconds(Runner, 5);
  //  }

    private void UpdateRunningDisplay()
		{
			// --- All clients
			// Display the remaining time until the game ends in seconds (rounded down to the closest full second)
			_startEndDisplay.gameObject.SetActive(false);
			_ingameTimerDisplay.gameObject.SetActive(true);
			_ingameTimerDisplay.text = $"{Mathf.RoundToInt(Timer.RemainingTime(Runner) ?? 0).ToString("000")} seconds left... Player RTT: {(int)(1000*Runner.GetPlayerRtt(Runner.LocalPlayer))}ms";
		}

		private void UpdateEndingDisplay()
		{
			// --- All clients
			// Display the results and
			// the remaining time until the current game session is shutdown
			
			if (Runner.TryFindBehaviour(Winner, out PlayerDataNetworked playerData) == false) return;

			_startEndDisplay.gameObject.SetActive(true);
			_ingameTimerDisplay.gameObject.SetActive(false);
			_startEndDisplay.text = $"{playerData.NickName} won with {playerData.Score} points. Disconnecting in {Mathf.RoundToInt(Timer.RemainingTime(Runner) ?? 0)}";
            _startEndDisplay.color = Player.GetColor(playerData.Object.InputAuthority.PlayerId);


        // Shutdowns the current game session.
        if (Timer.Expired(Runner))
				Runner.Shutdown();
		}
   
    public void CheckIfGameHasEnded()
		{
			// --- Master client
			
			if (Timer.ExpiredOrNotRunning(Runner))
			{
				GameHasEnded();
				return;
			}

			// Dont check for the first few seconds of the match or after a player joined for a winner to allow for players to join and spawn their spaceships
			if (_dontCheckforWinTimer.Expired(Runner) == false)
			{
				return;
			}
			

			int playersAlive = 0;
			
			for (int i = 0; i < _playerDataNetworkedIds.Count; i++)
			{
				if (Runner.TryFindBehaviour(_playerDataNetworkedIds[i],
					    out PlayerDataNetworked playerDataNetworkedComponent) == false)
				{
                _playerDataNetworkedIds.RemoveAll(id => !Runner.TryFindBehaviour(id, out _));
                //_playerDataNetworkedIds.RemoveAt(i);
					i--;
					continue;
				}

				if (playerDataNetworkedComponent.Lives > 0) playersAlive++;
			}
			
			
			// If more than 1 player is left alive, the game continues.
			// If only 1 player is left, the game ends immediately.
			if (playersAlive > 1 || (Runner.ActivePlayers.Count() == 1 && playersAlive == 1)) return;

			foreach (var playerDataNetworkedId in _playerDataNetworkedIds)
			{
				if (Runner.TryFindBehaviour(playerDataNetworkedId,
					    out PlayerDataNetworked playerDataNetworkedComponent) ==
				    false) continue;

				if (playerDataNetworkedComponent.Lives > 0 == false) continue;

				Winner = playerDataNetworkedId;
			}

			if (Winner == default && _playerDataNetworkedIds.Count > 0) // when playing alone in host mode this marks the own player as winner
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

		public void TrackNewPlayer(NetworkBehaviourId playerDataNetworkedId)
		{
			_playerDataNetworkedIds.Add(playerDataNetworkedId);
		}

		public void PlayerJoined(PlayerRef player)
		{
			_dontCheckforWinTimer = TickTimer.CreateFromSeconds(Runner, 5);
              AssignPlayerColor(player);
        }

    private Dictionary<PlayerRef, Color> playerColors = new Dictionary<PlayerRef, Color>();
    private Color[] availableColors = { Color.red, Color.blue, Color.green, Color.yellow };

    public void AssignPlayerColor(PlayerRef playerRef)
    {
        if (!playerColors.ContainsKey(playerRef))
        {
            Color assignedColor = availableColors[playerColors.Count % availableColors.Length];
            playerColors[playerRef] = assignedColor;

            // Find the PlayerDataNetworked component and set the color
            foreach (var playerDataId in _playerDataNetworkedIds)
            {
                if (Runner.TryFindBehaviour(playerDataId, out PlayerDataNetworked playerData) && playerData.Object.InputAuthority == playerRef)
                {
                    playerData.SetPlayerColor(assignedColor);
                    break;
                }
            }
        }
    }

    public Color GetPlayerColor(PlayerRef playerRef)
    {
        return playerColors.ContainsKey(playerRef) ? playerColors[playerRef] : Color.white;
    }
}
