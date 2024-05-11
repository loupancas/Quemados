using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] TextMeshProUGUI _victoryMesh;
    private GameObject _victoryTextObject;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        _victoryTextObject = _victoryMesh.gameObject;
        _victoryTextObject.SetActive(false);
    }

    public void SetVictoryScreen(Player winPlayer)
    {
        _victoryTextObject.SetActive(true);
        _victoryMesh.text = winPlayer == Player.LocalPlayer ? "You Win!" : "You Lose!";

    }

    
}
