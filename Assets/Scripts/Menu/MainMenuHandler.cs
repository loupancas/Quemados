using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private NetworkRunnerHandler _networkHandler;

    [Header("Panels")]
    [SerializeField] private GameObject _initialPanel;
    [SerializeField] private GameObject _sessionBrowserPanel;
    [SerializeField] private GameObject _shareGamePanel;
    [SerializeField] private GameObject _statusPanel;

    [Header("Buttons")]
    [SerializeField] private Button _joinLobbyBTN;
    [SerializeField] private Button _goToSharedPanelBTN;
    [SerializeField] private Button _sharedBTN;
    
    [Header("InputFields")]
    [SerializeField] private TMP_InputField _sharedSessionName;
    [SerializeField] private TMP_InputField _playerNickName;

    [Header("Texts")]
    [SerializeField] private TMP_Text _statusText;
    
    void Start()
    {
        _joinLobbyBTN.onClick.AddListener(Btn_JoinLobby);
        _goToSharedPanelBTN.onClick.AddListener(Btn_ShowHostPanel);
        _sharedBTN.onClick.AddListener(Btn_CreateGameSession);

        _networkHandler.OnJoinedLobby += () =>
        {
            _statusPanel.SetActive(false);
            _sessionBrowserPanel.SetActive(true);
        };
    }

    void Btn_JoinLobby()
    {
        _networkHandler.JoinLobby();

        PlayerPrefs.SetString("UserNickName", _playerNickName.text);

        _initialPanel.SetActive(false);
        _statusPanel.SetActive(true);

        _statusText.text = "Joining Lobby...";
    }
    
    void Btn_ShowHostPanel()
    {
        _sessionBrowserPanel.SetActive(false);
        
        _shareGamePanel.SetActive(true);
    }
    
    void Btn_CreateGameSession()
    {
        _sharedBTN.interactable = false;
        _networkHandler.CreateGame(_sharedSessionName.text, "Game");
    }
    
}
