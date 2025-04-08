using UnityEngine;
using Fusion;


public class PlayerDataNetworked : NetworkBehaviour
    {
        // Global static setting
        private const int STARTING_LIVES = 3;

    // Local Runtime references
    [SerializeField]
        private PlayerOverviewPanel _overviewPanel = null;

        private ChangeDetector _changeDetector;

        //[Networked, OnChangedRender (nameof(OnColorChanged))]
    public Color PlayerColor { get; private set; }
    // Game Session SPECIFIC Settings are used in the UI.
    // The method passed to the OnChanged attribute is called everytime the [Networked] parameter is changed.
        [HideInInspector]
        [Networked]
        public NetworkString<_16> NickName { get; private set; }

        [HideInInspector]
        [Networked]
        public int Lives { get; private set; }

        [HideInInspector]
        [Networked]
        public int Score { get; private set; }

        public override void Spawned()
        {

            // --- StateAuthority
            // Initialized game specific settings
            if (Object.HasStateAuthority)
            {
                Lives = STARTING_LIVES;
                Score = 0;
            //NickName = LocalPlayerData.NickName;
               string playerNickName = PlayerPrefs.GetString("PlayerNickName", "DefaultNickName");
               NickName = playerNickName;
            }

            // --- All Clients
            // Set the local runtime references.
            GameController.Instance.TrackNewPlayer(this);
            //FindObjectOfType<GameController>().TrackNewPlayer(this);
           // _overviewPanel = FindObjectOfType<PlayerOverviewPanel>();
        _overviewPanel = PlayerOverviewPanel.Instance;
        if (_overviewPanel != null)
            {
            // Add an entry to the local Overview panel with the information of this spaceship
            _overviewPanel.AddEntry(Object.InputAuthority, this);

            // Refresh panel visuals in Spawned to set to initial values.
            _overviewPanel.UpdateEntry(this);
            }
            else
            {
            Debug.LogError("PlayerOverviewPanel not found in the scene.");
            }
        //var controller = FindObjectOfType<GameController>();
        //var controller = GameController.Instance;
        //if (controller != null)
        //{
        //    controller.AssignPlayerColor(Object.InputAuthority);
        //}
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }
        
        // Remove the entry in the local Overview panel for this spaceship
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
        if (_overviewPanel != null)
        {
            _overviewPanel.RemoveEntry(this);
        }
        }

    public override void Render()
    {
        if (_overviewPanel == null)
        {
            _overviewPanel = PlayerOverviewPanel.Instance;
            if (_overviewPanel == null)
            {
                Debug.LogWarning("Render called but _overviewPanel is still null.");
                return;
            }
        }

        foreach (var change in _changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            _overviewPanel.UpdateEntry(this);
            break;
        }
    }

    // Increase the score by X amount of points
        public void AddToScore(int points)
        {
            Score += points;
        }

        // Decrease the current Lives by 1
        public void SubtractLife()
        {
            Lives--;
        }

        // RPC used to send player information to the Host
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        private void RpcSetNickName(string nickName)
        {
            if (string.IsNullOrEmpty(nickName)) return;
            NickName = nickName;
        }

    public void SetPlayerColor(Color color)
    {
        if (Object.HasStateAuthority)
        {
            PlayerColor = color;
        }
    }

    private static void OnColorChanged(PlayerDataNetworked changed)
    {
        changed.ApplyColor();
    }

    private void ApplyColor()
    {
        var player = GetComponent<Player>();
        if (player != null)
        {
            player.SetColor(PlayerColor);
        }
    }
}
