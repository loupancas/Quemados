using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    private PlayerSpawner playerSpawner;
    private Dictionary<int, Color> playerColors = new Dictionary<int, Color>();
    [SerializeField] private PlayerOverviewPanel _playerOverviewPanelPrefab;


    private void Start()
    {
        playerSpawner = PlayerSpawner.Instance;
        if (playerSpawner != null)
        {
            playerSpawner.OnPlayerJoinedEvent += OnPlayerJoined;
        }

    }

    private void OnDestroy()
    {
        if (playerSpawner != null)
        {
            playerSpawner.OnPlayerJoinedEvent -= OnPlayerJoined;
        }
    }


    public void OnPlayerJoined(PlayerRef player)
    {
        NetworkRunner runner = playerSpawner.Runner;
        if (runner.LocalPlayer == player)
        {
            int playerId = runner.LocalPlayer.PlayerId;
            Debug.Log("Player ID: " + playerId);

            Color playerColor = GetColor(playerId);
            playerColors[playerId] = playerColor;
            
        }

       
    }

    public static Color GetColor(int player)
    {
        switch (player % 8)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.grey;
            case 6: return Color.magenta;
            case 7: return Color.white;
        }

        return Color.black;
    }

    public Color GetPlayerColor(int playerId)
    {
        if (playerColors.TryGetValue(playerId, out Color color))
        {
            return color;
        }
        return Color.black; // Default color if not found
    }


}
