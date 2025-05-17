using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AgainBack : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _playAgain;
    [SerializeField] private Button _main;





    void Start()
    {
        //Debug.Log("Start");
        _main.onClick.AddListener(Btn_Main);
        _playAgain.onClick.AddListener(Btn_PlayAgain);
        //Debug.Log("Join Lobby");



    }

    public void Btn_Main()
    {
        SceneManager.LoadScene("Main");
        //Debug.Log("Join Lobby");
    }

    public void Btn_PlayAgain()
    {
        SceneManager.LoadScene("Main Menu");
        //Debug.Log("Join Lobby");
    }
}
