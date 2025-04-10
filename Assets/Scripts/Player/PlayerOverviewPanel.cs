using System;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

    // A purely utilitarian class which manages the display of player information (Nickname, Lives and Score)
    public class PlayerOverviewPanel : MonoBehaviour
    {
    [SerializeField] private TextMeshProUGUI _playerOverviewEntryPrefab = null;

        private Dictionary<PlayerDataNetworked, TextMeshProUGUI> _playerListEntries =
        new Dictionary<PlayerDataNetworked, TextMeshProUGUI>();

   

        private Dictionary<PlayerRef, PlayerOverviewEntry> _playerEntries = new Dictionary<PlayerRef, PlayerOverviewEntry>();

    private PlayerRef _localPlayerRef;
    private NetworkRunner _networkRunner;
    private void Start()
    {
        _networkRunner = FindObjectOfType<NetworkRunner>();
        // Asume que el jugador local es el que tiene la autoridad de entrada
        _localPlayerRef = _networkRunner.LocalPlayer;
    }

        public void Clear()
        {
            foreach (var tmp in _playerEntries.Values)
            {
                Destroy(tmp);
            }

            _playerEntries.Clear();
        }

        public void AddEntry(PlayerRef playerRef, PlayerDataNetworked playerDataNetworked)
        {
        if (_playerListEntries.ContainsKey(playerDataNetworked)) return;
        if (playerDataNetworked == null) return;
        // Solo agregar la entrada si es el jugador local
        if (playerRef != _localPlayerRef) return;
        var entry = Instantiate(_playerOverviewEntryPrefab, this.transform);
        entry.transform.localScale = Vector3.one;
        string playerName = playerDataNetworked.NickName;
        entry.color = PlayerSpawner.GetColor(playerName);



        _playerListEntries.Add(playerDataNetworked, entry);

        UpdateEntry(playerDataNetworked);

       

        }

        public void UpdateEntry(PlayerDataNetworked playerData)
        {
        if (_playerListEntries.TryGetValue(playerData, out TextMeshProUGUI entry))
        {
            entry.text = $"{playerData.NickName}\nScore: {playerData.Score}\nLives: {playerData.Lives}";
        }

       
        }


        public void RemoveEntry(PlayerDataNetworked playerData)
        {
        if (_playerListEntries.TryGetValue(playerData, out var entry) == false) return;

        if (entry != null)
        {
            Destroy(entry.gameObject);
        }

        _playerListEntries.Remove(playerData);
       

         }

  

    }
