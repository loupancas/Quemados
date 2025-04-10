using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private GameObject _victoryScreen;
    [SerializeField] private GameObject _loseScreen;
    [SerializeField] private GameObject _startGameScreen;
    [SerializeField] private GameObject _infoPlayer;
    [SerializeField] private GameObject _infoBall;
    [SerializeField] NetworkRunner runner;
    private PlayerRef player;
    [SerializeField] public GameObject readyButton;
    private void Awake()
    {
      instance = this;
     Debug.Log("UIManager awake");
        
    }

    void Start()
    {
        
    }

    public void SetPlayerRef(PlayerRef newPlayer)
    {
        player = newPlayer;
    }

    public void SetLoseScreen()
    {
      _loseScreen.SetActive(true);
        _victoryScreen.SetActive(false);
    }

    public void SetVictoryScreen()
    {
       _victoryScreen.SetActive(true);
        _loseScreen.SetActive(false);
    }

    public void SetReady()
    {
        if (RoomM.Instance != null)
        {
            RoomM.Instance.RpcOnPlayerConfirm(player);
            GameController.Singleton?.ActivateStartEndDisplay();

            readyButton.SetActive(false);
            _startGameScreen.SetActive(true);
            RoomM.Instance.RpcStartGame();
        }
    }

    public void StartGame()
    {
        //readyText.SetActive(false);
        _startGameScreen.SetActive(false);
        _infoBall.SetActive(true);
        _infoPlayer.SetActive(true);
    }

}
