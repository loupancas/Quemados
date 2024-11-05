using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public List<PlayerData> Players { get; private set; } = new List<PlayerData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPlayer(PlayerData playerData)
    {
        Players.Add(playerData);
    }

    public void RemovePlayer(PlayerRef playerRef)
    {
        Players.RemoveAll(p => p.PlayerRef == playerRef);
    }

    public PlayerData GetPlayerData(PlayerRef playerRef)
    {
        return Players.Find(p => p.PlayerRef == playerRef);
    }
}
