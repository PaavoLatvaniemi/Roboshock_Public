using Assets.Scripts.Network;
using Assets.Scripts.ServerList;
using MLAPI;
using MLAPI.Messaging;
using System.Collections.Generic;
using UnityEngine;
//TODO:
//SIIRRÄ UI JA FUNKTIONAALISET ASIAT ERIKSEEN JOSSAIN VÄLISSÄ
public class NetworkHUD : MonoBehaviour
{
    public static string nameText = "Player";
    public static string serverName = nameText + "'s Server";
    List<ServerModel> uiServers = new List<ServerModel>();

    NetworkMatchmaking networkMatchmaking;
    private void OnEnable()
    {
        networkMatchmaking = GetComponent<NetworkMatchmaking>();
    }
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
            GUILayout.Label("Name");
            nameText = GUILayout.TextField(nameText, 15, "textfield");
        }
        else
        {
            if (NetworkManager.Singleton.IsHost)
                HostView();
            else
                GeneralView();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        GUILayout.Label("Lobby name");
        if (GUILayout.Button("Start lobby"))
        {
            networkMatchmaking.RelayAndHostServer(serverName);
        }
        if (GUILayout.Button("Find matchmaking"))
        {
            networkMatchmaking.QueryMatchmatchMakingServer();
        }
        for (int i = 0; i < uiServers.Count; i++)
        {
            if (GUILayout.Button(uiServers[i].ServerData["Name"] + "," + uiServers[i].ServerData["Players"] + "Players"))
            {
                networkMatchmaking.ConnectToServerIndex(i);
            }
        }
    }



    public void updateServerUI(List<ServerModel> servers)
    {
        uiServers = servers;
    }
    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
    static void GeneralView()
    {
        if (!GameModeManager.Singleton.GameStarted)
        {
            GUILayout.Label("Current Game Mode: " + GameModeManager.Singleton.currentGameMode.GameModeName);
            ShowConnectedPlayers();
            GUILayout.Label("Waiting For Host... ");
        }
        else
        {
            StatusLabels();
        }
    }
    static void HostView()
    {
        if (!GameModeManager.Singleton.GameStarted)
        {
            GUILayout.Label("Current Game Mode: " + GameModeManager.Singleton.currentGameMode.GameModeName);
            if (GUILayout.Button("Start Game")) StartGameModeServerRpc();
            ShowConnectedPlayers();
        }
        else
        {
            StatusLabels();
            if (GUILayout.Button("End Game")) EndGameModeServerRpc();
        }
    }
    static void ShowConnectedPlayers()
    {
        string connectedPlayers = string.Empty;
        foreach (PlayerInfo info in PlayerManager.Singleton.AllPlayerInfos)
        {
            if (info.PlayerIsHost)
            {
                connectedPlayers += "\n" + info.PlayerName + " (Host)";
            }
            else
            {
                connectedPlayers += "\n" + info.PlayerName;
            }
        }
        GUILayout.Label("Connected Players:" + connectedPlayers);
    }
    [ServerRpc]
    static void StartGameModeServerRpc()
    {
        if (NetworkManager.Singleton.IsHost)
            GameModeManager.Singleton.StartGameMode();
    }
    [ServerRpc]
    static void EndGameModeServerRpc()
    {
        if (NetworkManager.Singleton.IsHost)
            GameModeManager.Singleton.EndGameMode();
    }
}