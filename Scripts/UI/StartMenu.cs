using Assets.Scripts.Config;
using Assets.Scripts.Network;
using Assets.Scripts.PlayerScript;
using Assets.Scripts.ServerList;
using Assets.Scripts.UI;
using MLAPI;
using MLAPI.Messaging;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : BaseMenu
{
    public static string nameText = PlayerGlobals.PlayerName;
    public static string serverName = PlayerGlobals.PlayerName + "'s Server";
    [SerializeField] string joinIP;
    NetworkMatchmaking networkMatchmaking;

    //UI
    [SerializeField] GameObject matchJoinButton;
    [SerializeField] Transform matchesContentScroller;
    [SerializeField] TextMeshProUGUI lobbyPlayersText;
    [SerializeField] Button lobbyStartButton;
    [SerializeField] Toggle localHostToggler;
    [SerializeField] TMP_InputField nameInputField;

    public delegate void StartingGameFromCanvas();
    public static event StartingGameFromCanvas onStartGameFromCanvas;
    public NetworkMatchmaking getMatchMaking()
    {
        return networkMatchmaking;
    }
    private void Awake()
    {
        networkMatchmaking = FindObjectOfType<NetworkMatchmaking>();
    }
    protected override void OnEnable()
    {

        base.OnEnable();

        NetworkMatchmaking.onServerHosted += beginLobby;
        NetworkMatchmaking.onServersFound += CreateServerListing;
        PlayerManager.onPlayersChange += ShowConnectedPlayers;
        GameModeManager.onGameModeStart += DisableUI;
        getPlayerName();
    }
    private void OnDisable()
    {
        NetworkMatchmaking.onServerHosted -= beginLobby;
        NetworkMatchmaking.onServersFound -= CreateServerListing;
        PlayerManager.onPlayersChange -= ShowConnectedPlayers;
        GameModeManager.onGameModeStart -= DisableUI;
    }
    private void CreateServerListing(List<ServerModel> models)
    {

        for (int i = 0; i < models.Count; i++)
        {
            GameObject go = Instantiate(matchJoinButton, matchesContentScroller);
            go.transform.gameObject.GetComponent<MatchJoinButtonExtender>().Initialize(i, models[i].ServerData["Name"].ToString());
        }
    }
    public void JoinLocalHostServer()
    {

        networkMatchmaking.DirectConnectToIPv4(joinIP, true);
    }
    public void populateLobbyView()
    {
        if (networkMatchmaking.IsHost())
        {
            EnableStartGameButton();
        }
    }

    private void EnableStartGameButton()
    {
        lobbyStartButton.interactable = true;
    }
    public void StartGame()
    {
        InvokeGameServerRpc();
        //networkMatchmaking.StartGameModeServerRpc();


    }
    [ServerRpc]
    private void InvokeGameServerRpc()
    {
        InvokeGameClientRpc();
    }
    [ClientRpc]
    private void InvokeGameClientRpc()
    {
        onStartGameFromCanvas?.Invoke();
    }

    public void StartServer()
    {
        if (localHostToggler.isOn)
        {

            networkMatchmaking.HostServer(false);
        }
        else
        {
            networkMatchmaking.RelayAndHostServer(serverName);
        }

    }

    private void beginLobby()
    {
        populateLobbyView();
    }

    public void FindMatchMaking()
    {
        networkMatchmaking.QueryMatchmatchMakingServer();
    }

    void ShowConnectedPlayers(List<PlayerInfo> playerInfos)
    {
        string connectedPlayers = string.Empty;
        foreach (PlayerInfo info in playerInfos)
        {
            if (info.PlayerIsHost)
            {
                connectedPlayers += info.PlayerName + " (Host) \n";
            }
            else
            {
                connectedPlayers += info.PlayerName + "\n";
            }
        }
        lobbyPlayersText.text = connectedPlayers;


    }

    public void DisconnectFromGame()
    {
        if (networkMatchmaking.IsHost())
        {
            NetworkManager.Singleton.StopHost();
            NetworkMatchmaking.Singleton.StopAdvertising();
            networkMatchmaking.EndGameModeServerRpc();
        }
        else
        {
            NetworkManager.Singleton.StopClient();
        }
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(2);


    }
    private void DisableUI()
    {
        gameObject.SetActive(false);
    }
    public void SavePlayerName(TextMeshProUGUI textMeshProUGUI)
    {
        PlayerGlobals.PlayerName = textMeshProUGUI.text;
        ConfigManager.saveConfigKeyValue("PlayerName", PlayerGlobals.PlayerName);
        nameText = PlayerGlobals.PlayerName;
        nameInputField.text = nameText;
        ConfigManager.serializeConfigValues();
    }
    void getPlayerName()
    {

        nameText = PlayerGlobals.PlayerName;
        nameInputField.SetTextWithoutNotify(nameText);
    }
    public void ExitGame()
    {
        Application.Quit();
    }


}
