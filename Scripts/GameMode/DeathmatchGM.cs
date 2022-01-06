using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections;
using UnityEngine;

public class DeathmatchGM : NetworkBehaviour, IGameMode
{
    [SerializeField]
    string gameModeName;
    public string GameModeName => gameModeName;
    public GameObject GameModeGO => gameObject;
    public delegate void OnGameEnd(ulong winner);
    public static event OnGameEnd onGameEnd;
    public int winKills;
    float _currentTime = 0;
    float _timeLeft = 0;
    Coroutine _timer;
    [Tooltip("Seconds")]
    public float timeLimit;
    [Tooltip("Seconds")]
    public float respawnTime;
    FragLeader _fragLeader;
    public void StartGame() // Server Side
    {
        if (!IsServer) return;
        PlayerInfo.onKillAdded += CheckKillCount;
        StartTimerClientRpc();
        NetworkManager.Singleton.OnClientConnectedCallback += addHotJoinerToTimer;
        PlayerRespawn playerRespawn = FindObjectOfType<PlayerRespawn>();
        if (playerRespawn != null) playerRespawn.RespawnAllPlayers();
    }

    private void OnDisable()
    {
        GameModeManager.onNewHotJoin -= addHotJoinerToTimer;
    }
    private void addHotJoinerToTimer(ulong ID)
    {
        if (GameModeManager.Singleton.GameStarted == true)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams()
            {
                Send = new ClientRpcSendParams()
                {
                    TargetClientIds = new ulong[] { ID }
                }
            };
            StartTimerClientRpc(_currentTime, clientRpcParams);
        }


    }

    [ClientRpc]
    private void StartTimerClientRpc(float hotjoinTime = 0, ClientRpcParams clientRpcParams = default)
    {
        _timer = StartCoroutine(Countdown(hotjoinTime));
    }

    private IEnumerator Countdown(float hotjoinTime = 0)
    {
        if (hotjoinTime != 0)
        {
            _currentTime += hotjoinTime;
        }
        while (_currentTime < timeLimit)
        {
            _currentTime += Time.deltaTime;
            _timeLeft = timeLimit - _currentTime;
            UIManager.Singleton.SetBindable("TimeLeft", UIManager.Singleton.FormatTimer(_timeLeft));
            if (_timeLeft < 0)
            {
                onGameEnd?.Invoke(PlayerManager.Singleton.getNthPlayerByNetworkObject(0).OwnerClientId);
            }
            yield return null;
        }
    }

    public void EndGame(PlayerInfo winner) // Server Side
    {
        if (!IsServer) return;
        PlayerInfo.onKillAdded -= CheckKillCount;
        GameEndClientRpc(winner.OwnerClientId);
    }
    [ClientRpc]
    private void GameEndClientRpc(ulong winner)
    {
        onGameEnd?.Invoke(winner);
    }

    void CheckKillCount(PlayerInfo playerInfo)
    {
        if (!IsServer) return;
        if (playerInfo.PlayerKills > _fragLeader.Kills)
        {
            _fragLeader.LeaderClientID = playerInfo.OwnerClientId;
            _fragLeader.Kills = playerInfo.PlayerKills;
        }

        if (playerInfo.PlayerKills >= winKills)
        {
            EndGame(playerInfo);
        }
    }

    private void OnEnable()
    {
        _timeLeft = timeLimit - _currentTime;

        UIManager.Singleton.AddBindable("TimeLeft", UIManager.Singleton.FormatTimer(_timeLeft));
        _fragLeader = new FragLeader()
        {
            LeaderClientID = 0,
            Kills = 0
        };
    }
}
struct FragLeader
{
    ulong leaderClientID;
    int kills;

    public ulong LeaderClientID { get => leaderClientID; set => leaderClientID = value; }
    public int Kills { get => kills; set => kills = value; }
}