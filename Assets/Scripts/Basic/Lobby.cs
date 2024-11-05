using Fusion;
using System.Collections.Generic;
using UnityEngine;


public class Lobby : NetworkBehaviour
{
    public List<PlayerData> players = new List<PlayerData>();

    public void OnPlayerConnected(PlayerRef playerRef)
    {
        // Añadir jugador a la lista
        players.Add(new PlayerData { PlayerRef = playerRef, IsConnected = true });
        UpdatePlayerListUI();
    }

    public void OnStartGame()
    {
        //if (IsServer)
        //{
        //    // Iniciar el juego y cargar la escena de Gameplay
        //    NetworkManager.Singleton.SceneManager.LoadScene("GameplayScene", LoadSceneMode.Single);
        //}
    }

    private void UpdatePlayerListUI()
    {
        foreach (var playerData in PlayerDataManager.Instance.Players)
        {
            // Actualizar la UI con la lista de jugadores
            Debug.Log($"Player: {playerData.Nickname}, Connected: {playerData.IsConnected}");
        }

    }
}
