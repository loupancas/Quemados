using UnityEngine;
using Fusion;


    public class PlayerDataNetworked : NetworkBehaviour
    {
        // Global static setting
        private const int STARTING_LIVES = 3;

        // Local Runtime references
        private PlayerOverviewPanel _overviewPanel = null;

        private ChangeDetector _changeDetector;

    // Game Session SPECIFIC Settings are used in the UI.
    // The method passed to the OnChanged attribute is called everytime the [Networked] parameter is changed.
    [HideInInspector]
        [Networked]
        public string NickName { get; private set; }

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
            NickName = PlayerPrefs.GetString("PlayerNickName", "DefaultNickName");
        }

            // --- All Clients
            // Set the local runtime references.
            
            FindObjectOfType<GameController>().TrackNewPlayer(this);
           _overviewPanel = FindObjectOfType<PlayerOverviewPanel>();

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
        foreach (var change in _changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            _overviewPanel.UpdateEntry(this);
            break;
        }

        //_overviewPanel?.UpdateEntry(this);
    }

        // Increase the score by X amount of points
        public void AddToScore(int points)
        {
            Score += points;
        //_overviewPanel?.UpdateEntry(this);
    }

        // Decrease the current Lives by 1
        public void SubtractLife()
        {
            Lives--;
        //_overviewPanel?.UpdateEntry(this);
    }

        // RPC used to send player information to the Host
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        private void RpcSetNickName(string nickName)
        {
            if (string.IsNullOrEmpty(nickName)) return;
            NickName = nickName;
        }
    }
