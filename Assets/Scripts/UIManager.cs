using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] TextMeshProUGUI _victoryMesh;
    [SerializeField] private TextMeshProUGUI countdownText; 
    [SerializeField] private TextMeshProUGUI infoText; 
    private GameObject _victoryTextObject;
    [SerializeField] NetworkRunner runner;
    private PlayerRef player;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _victoryTextObject = _victoryMesh.gameObject;
        _victoryTextObject.SetActive(false);
        countdownText.gameObject.SetActive(false);
        infoText.gameObject.SetActive(true);
    }

    public void SetPlayerRef(PlayerRef newPlayer)
    {
        player = newPlayer;
    }

    public void SetLoseScreen()
    {
        _victoryTextObject.SetActive(true);
        _victoryMesh.text = "You Lose!";
    }

    public void SetVictoryScreen()
    {
        _victoryTextObject.SetActive(true);
        _victoryMesh.text = "You Win!";
    }

    public IEnumerator StartCountdown(int duration)
    {
        infoText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);
        while (duration > 0)
        {
            countdownText.text = duration.ToString();
            yield return new WaitForSeconds(1);
            duration--;
        }
        countdownText.gameObject.SetActive(false);
        Player.EnablePlayerControls();
    }

    public void SetReady()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RpcOnPlayerConfirm(player);
            //readyButton.SetActive(false);
            //readyText.SetActive(true);
            
        }
    }

    public void StartGame()
    {
        //readyText.SetActive(false);
    }

}
