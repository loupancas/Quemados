using Fusion;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void OnStartGame()
    {
        // Conectar al servidor y cargar la escena del Lobby
        //NetworkManager.Singleton.StartClient();
        //SceneManager.LoadScene("LobbyScene");
    }

    public void OnPlayerConnected(PlayerRef playerRef, string nickname)
    {
        PlayerData playerData = new PlayerData
        {
            PlayerRef = playerRef,
            Nickname = nickname,
            IsConnected = true
        };
        PlayerDataManager.Instance.AddPlayer(playerData);
    }

}
