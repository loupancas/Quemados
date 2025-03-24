using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Fusion;
using System.Linq;


public class NetworkManager : MonoBehaviour
{
    public NetworkRunner runner;
    [SerializeField] GameObject readyButton;
    [SerializeField] GameObject readyText;

    public void StartGame()
    {
        Debug.Log("Start Countdown");

        if (GameManager.Instance.Runner.ActivePlayers.Count() == 2)
        {
            UIManager.instance.StartCountdown(3);
        }

        if (runner.SessionInfo.PlayerCount == 2)
        {
            StartCoroutine(UIManager.instance.StartCountdown(3));
        }
        else
        {
            Debug.Log("waiting for another player");
        }
    }



}
