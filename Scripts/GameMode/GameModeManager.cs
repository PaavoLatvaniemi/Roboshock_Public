using Assets.Scripts.PlayerScript;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : NetworkBehaviour
{
    static GameModeManager singleton;
    public static GameModeManager Singleton
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindObjectOfType<GameModeManager>();
            }
            return singleton;
        }
    }

    NetworkVariableBool gameStarted = new NetworkVariableBool();
    public bool GameStarted => gameStarted.Value;
    List<IGameMode> gameModes = new List<IGameMode>();
    public IGameMode currentGameMode { get; private set; }

    public delegate void GameModeStartServer();
    public static event GameModeStart onGameModeStartServer; // Server Side

    public delegate void GameModeStart();
    public static event GameModeStart onGameModeStart; // Client Side

    public delegate void GameModeEnd();
    public static event GameModeEnd onGameModeEnd; // Client Side

    public delegate void GameIsStarting();
    public static event GameIsStarting onGameIsStarting;
    bool matchStarted = false;

    public delegate void NewHotJoin(ulong ID);
    public static event NewHotJoin onNewHotJoin;

    private void Awake()
    {

        StartMenu.onStartGameFromCanvas += StartGameServerRpc;

        NetworkManager.OnClientConnectedCallback += addPlayerHotJoinServerRpc;
        // Haetaan kaikki GameModet childeista ja asetetaan ensimmäinen child currentGameMode
        foreach (Transform child in transform)
        {
            IGameMode gameMode = child.GetComponent<IGameMode>();
            if (gameMode != null) gameModes.Add(gameMode);
        }
        currentGameMode = gameModes[0];
    }
    /// <summary>
    /// Tämä tapahtuu, jos joku pelaaja liittyy kesken pelin.
    /// </summary>
    /// <param name="obj"></param>
    [ServerRpc]
    private void addPlayerHotJoinServerRpc(ulong obj)
    {
        if (IsServer)
        {
            if (gameStarted.Value == true)
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams()
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { obj }
                    }
                };
                StartCoroutine(TaskDelayer.createDelayedTask(() => HotJoinAdd(obj, clientRpcParams), 2f));
            }
        }

    }

    private void HotJoinAdd(ulong obj, ClientRpcParams clientRpcParams)
    {

        //Aloitetaan peli vain liittyvällä clientillä, joten parametrisoidaan tieto send kohteesta
        StartGameClientRpc(clientRpcParams);
        StartGameModeClientRpc(clientRpcParams);
        var id = NetworkManager.Singleton.ConnectedClients[obj].PlayerObject.NetworkObjectId;
        NotifyHotJoinClientRpc(id);

        PlayerRespawn.Singleton.RespawnServerRpc(id);
    }
    [ClientRpc]
    private void NotifyHotJoinClientRpc(ulong obj)
    {
        onNewHotJoin?.Invoke(obj);
    }

    private void OnDisable()
    {
        StartMenu.onStartGameFromCanvas -= StartGameServerRpc;
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= addPlayerHotJoinServerRpc;
        }
    }
    [ServerRpc]
    private void StartGameServerRpc()
    {
        matchStarted = true;
        StartGameClientRpc();
    }
    [ClientRpc]
    private void StartGameClientRpc(ClientRpcParams clientRpcParams = default)
    {
        matchStarted = true;
        onGameIsStarting?.Invoke();
        PlayerGlobals.canOpenMenus = true;

    }

    public void StartGameMode()
    {
        if (!IsServer) return;

        foreach (PlayerInfo player in PlayerManager.Singleton.AllPlayerInfos)
            player.ClearStats();

        gameStarted.Value = true;
        currentGameMode.StartGame();
        StartGameModeClientRpc();
        onGameModeStartServer?.Invoke();
    }

    public void EndGameMode()
    {
        if (!IsServer) return;

        gameStarted.Value = false;
        EndGameModeClientRpc();
    }

    [ServerRpc]
    public void SetCurrentGameModeServerRpc(int gameModeId) => SetCurrentGameModeClientRpc(gameModeId);

    [ClientRpc]
    void StartGameModeClientRpc(ClientRpcParams clientRpcParams = default)
    {
        currentGameMode.GameModeGO.SetActive(true);
        onGameModeStart?.Invoke();
    }

    [ClientRpc]
    void EndGameModeClientRpc()
    {
        currentGameMode.GameModeGO.SetActive(false);
        if (IsHost)
        {
            NetworkManager.Singleton.StopHost();
        }
        else
        {
            NetworkManager.Singleton.StopClient();
        }
        SceneManager.LoadScene(2);
    }

    [ClientRpc]
    void SetCurrentGameModeClientRpc(int gameModeId)
    {
        currentGameMode = gameModes[gameModeId];
    }
}
