
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class menu : MonoBehaviour
{
    

    [Header("Buttons")]
    [SerializeField] private Button _play;
    [SerializeField] private Button _quit;
   




    void Start()
    {
        //Debug.Log("Start");
        _play.onClick.AddListener(Btn_Play);
        _quit.onClick.AddListener(Btn_Quit);
        //Debug.Log("Join Lobby");



    }

    public void Btn_Play()
    {
        SceneManager.LoadScene("Main Menu");
        //Debug.Log("Join Lobby");
    }

    public void Btn_Quit()
    {
        Application.Quit();
        //Debug.Log("Join Lobby");
    }










}


