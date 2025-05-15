using UnityEngine;
using Fusion;


    public class PlayerDataNetworked : NetworkBehaviour
    {

    Player _player;
    // Global static setting
    private const int STARTING_LIVES = 3;

        // Local Runtime references
       [SerializeField] private PlayerOverviewPanel _overviewPanel;

        private ChangeDetector _changeDetector;

    [HideInInspector]
        [Networked]
        public string NickName { get; set; }

        [HideInInspector]
        [Networked]
        public int Lives { get; set; }

        [HideInInspector]
        [Networked]
        public int Score { get; set; }

        public override void Spawned()
        {
             _player = GetComponent<Player>();
       
        // --- StateAuthority
        // Initialized game specific settings
        if (Object.HasStateAuthority)
            {
                Lives = STARTING_LIVES;
                Score = 0;
            //NickName = PlayerPrefs.GetString("PlayerNickName", "DefaultNickName");
            NickName = _player._name;
            //Debug.Log(NickName + " has joined the game!");
        }
       
        Debug.Log("PlayerDataNetworked object has been spawned!");
        
        // --- All Clients
        // Set the local runtime references.

        FindObjectOfType<GameController>().TrackNewPlayer(this);
           _overviewPanel = FindObjectOfType<PlayerOverviewPanel>();

        if (_overviewPanel != null)
        {
            _overviewPanel.AddEntry(Object.InputAuthority, this);

            _overviewPanel.UpdateEntry(this);
        }
        else
        {
            Debug.LogError("PlayerOverviewPanel not found in the scene.");
        }

        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }
        
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

    }

  


    public void AddToScore(int points)
        {

        //if (!Object.HasStateAuthority)
        //{
        //    Debug.LogError("Cannot modify Score. The object does not have state authority.");
        //    return;
        //}

        Score += points;
        if (_overviewPanel != null)
        {
            _overviewPanel.UpdateEntry(this);
        }
        else
        {
            Debug.LogWarning("OverviewPanel is null. Cannot update the score entry.");
        }
        //_overviewPanel?.UpdateEntry(this);
    }

        // Decrease the current Lives by 1
        public void SubtractLife()
        {
            Lives--;
        //_overviewPanel?.UpdateEntry(this);
    }

        //// RPC used to send player information to the Host
        //[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        //private void RpcSetNickName(string nickName)
        //{
        //    if (string.IsNullOrEmpty(nickName)) return;
        //    NickName = nickName;
        //}
    }
