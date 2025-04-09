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

    //[SerializeField] private GameObject playerEntryPrefab;
    //    [SerializeField] private Transform entriesParent;

        private Dictionary<PlayerRef, PlayerOverviewEntry> _playerEntries = new Dictionary<PlayerRef, PlayerOverviewEntry>();



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

        var entry = Instantiate(_playerOverviewEntryPrefab, this.transform);
        entry.transform.localScale = Vector3.one;
        entry.color = Player.GetColor(playerRef.PlayerId);



        _playerListEntries.Add(playerDataNetworked, entry);

        UpdateEntry(playerDataNetworked);

        //if (!_playerEntries.ContainsKey(playerRef))
        //    {
        //        // Instancia una nueva entrada de jugador
        //        GameObject entryObject = Instantiate(playerEntryPrefab, entriesParent);
        //        PlayerOverviewEntry entry = entryObject.GetComponent<PlayerOverviewEntry>();

        //        // Actualiza la entrada con los datos del jugador
        //        entry.UpdateEntry(playerDataNetworked);

        //        // Agrega la entrada al diccionario
        //        _playerEntries.Add(playerRef, entry);
        //    }

    }

        public void UpdateEntry(PlayerDataNetworked playerData)
        {
        if (_playerListEntries.TryGetValue(playerData, out TextMeshProUGUI entry))
        {
            entry.text = $"{playerData.NickName}\nScore: {playerData.Score}\nLives: {playerData.Lives}";
        }

        //if (_playerEntries.TryGetValue(playerData.Object.InputAuthority, out PlayerOverviewEntry entry))
        //    {
        //        // Actualiza la entrada con los datos del jugador
        //        entry.UpdateEntry(playerData);
        //    }
    }


        public void RemoveEntry(PlayerDataNetworked playerData)
        {
        if (_playerListEntries.TryGetValue(playerData, out var entry) == false) return;

        if (entry != null)
        {
            Destroy(entry.gameObject);
        }

        _playerListEntries.Remove(playerData);
        //if (_playerEntries.TryGetValue(playerData.Object.InputAuthority, out PlayerOverviewEntry entry))
        //{
        //    // Elimina la entrada del diccionario y destruye el objeto de la UI
        //    _playerEntries.Remove(playerData.Object.InputAuthority);
        //    Destroy(entry.gameObject);
        //}

    }

  

}
