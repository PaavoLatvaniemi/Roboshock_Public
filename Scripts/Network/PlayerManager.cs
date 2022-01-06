using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerManager : NetworkBehaviour
{

    public static PlayerManager _singleton;
    public static PlayerManager Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindObjectOfType<PlayerManager>();
            }
            return _singleton;
        }
    }
    //Tarvitaan tietoa mm. Serverlistille sekä Pelaajan uille lobbyssa ja tab menussa.
    public delegate void PlayerInfoChange(List<PlayerInfo> info);
    public static event PlayerInfoChange onPlayersChange;
    public delegate void PlayerDisconnection(ulong objectID, ulong networkID);
    public static event PlayerDisconnection onPlayerDisconnection;
    static Dictionary<ulong, PlayerInfo> connectedPlayersByPlayerObject = new Dictionary<ulong, PlayerInfo>();
    static Dictionary<ulong, ulong> connectedPlayerObjectIDsByNetworkIDs = new Dictionary<ulong, ulong>();
    public List<PlayerInfo> AllPlayerInfos => connectedPlayersByPlayerObject.Values.ToList();

    public List<ulong> AllNetworkIds => connectedPlayersByPlayerObject.Keys.ToList();
    private void OnEnable()
    {
        //Varmistetaan että pelaajia ei ole valmiiksi lisätty, esim kun avaa uuden lobbyn.
        connectedPlayersByPlayerObject.Clear();
        connectedPlayerObjectIDsByNetworkIDs.Clear();
        NetworkManager.Singleton.OnServerStarted += addHostToGame;
        NetworkManager.Singleton.OnClientConnectedCallback += AddConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemoveDisconnected;

    }



    private void OnDisable()
    {
        NetworkManager.Singleton.OnServerStarted -= addHostToGame;
        NetworkManager.Singleton.OnClientConnectedCallback -= AddConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= RemoveDisconnected;
    }

    private void addHostToGame()
    {
        if (!IsServer) return;
        //Hostin network id = 0
        StartCoroutine(TaskDelayer.createDelayedTask(() => AddConnectionServerRpc(0), 0.1f));
    }
    private void RemoveDisconnected(ulong obj)
    {
        RemoveConnectionServerRpc(obj);
        //StartCoroutine(TaskDelayer.createDelayedTask(() => UpdateConnectedPlayersServerRpc(), 1));
    }
    private void AddConnected(ulong obj)
    {
        StartCoroutine(TaskDelayer.createDelayedTask(() => AddConnectionServerRpc(obj) , 0.5f));
        

    }
    [ServerRpc]
    void RemoveConnectionServerRpc(ulong obj)
    {
        if (!IsServer) return;

        //Etsitään hashattu tieto objektista, jolle ID kuuluu
        ulong objectID = connectedPlayerObjectIDsByNetworkIDs[obj];
        //Tätä tietoa pitää ensiksi käsitellä muutamassa muussa paikassa, joten invoke event
        onPlayerDisconnection?.Invoke(objectID, obj);
        //Poistetaan tieto ekana objekti-ID -> Objektikomponentti sanakirjasta
        connectedPlayersByPlayerObject.Remove(objectID);
        //Sitten poistetaan alkuperäisestä hashtablesta tieto.
        connectedPlayerObjectIDsByNetworkIDs.Remove(obj);
        //Poistetaan tiedot kaikilta muiltakin
        RemoveConnectionClientRpc(objectID, obj);

    }
    [ClientRpc]
    private void RemoveConnectionClientRpc(ulong objectID, ulong obj)
    {
        if (IsServer) return;
        onPlayerDisconnection?.Invoke(objectID, obj);
        connectedPlayersByPlayerObject.Remove(objectID);
        connectedPlayerObjectIDsByNetworkIDs.Remove(obj);
    }

    [ServerRpc]
    private void AddConnectionServerRpc(ulong obj)
    {
        if (!IsServer) return;

        PlayerInfo playerInfo = NetworkManager.Singleton.ConnectedClients[obj].PlayerObject.GetComponent<PlayerInfo>();
        connectedPlayersByPlayerObject.Add(playerInfo.NetworkObjectId, playerInfo);
        connectedPlayerObjectIDsByNetworkIDs.Add(obj, playerInfo.NetworkObjectId);
        onPlayersChange?.Invoke(connectedPlayersByPlayerObject.Values.ToList());
        StartCoroutine(TaskDelayer.createDelayedTask(() => AddConnectionClientRpc(connectedPlayersByPlayerObject.Keys.ToArray(),
                                                                                  connectedPlayerObjectIDsByNetworkIDs.Keys.ToArray()), 0.5f));

    }

    [ClientRpc]
    public void AddConnectionClientRpc(ulong[] playerObjects, ulong[] playerObjectsByNetworkID)
    {
        if (IsServer) return;
        PlayerInfo[] playerInfos = new PlayerInfo[playerObjects.Length];
        for (int i = 0; i < playerObjects.Length; i++)
        {
            playerInfos[i] = GetNetworkObject(playerObjects[i]).GetComponent<PlayerInfo>();
        }
        for (int i = 0; i < playerInfos.Length; i++)
        {
            connectedPlayersByPlayerObject.Add(playerObjects[i], playerInfos[i]);
            connectedPlayerObjectIDsByNetworkIDs.Add(playerObjectsByNetworkID[i], playerObjects[i]);
        }
        onPlayersChange?.Invoke(playerInfos.ToList());
    }


    public ulong GetPlayerNetworkId(PlayerInfo info)
    {
        return connectedPlayersByPlayerObject.FirstOrDefault(x => x.Value == info).Key;
    }

    public PlayerInfo GetPlayerInfoByNetworkObject(ulong? networkId)
    {
        if (networkId == null) return null;
        PlayerInfo info;
        if (connectedPlayersByPlayerObject.TryGetValue((ulong)networkId, out info)) return info;

        return null;
    }
    public PlayerInfo GetPlayerInfoByNetworkObject(ulong networkId)
    {
        PlayerInfo info;
        if (connectedPlayersByPlayerObject.TryGetValue(networkId, out info)) return info;

        return null;
    }
    public PlayerInfo getNthPlayerByNetworkObject(int n)
    {
        if (AllPlayerInfos.Count - 1 < n)
        {
            return null;
        }
        return AllPlayerInfos.OrderByDescending(player => player.PlayerKills).ToList()[n];
    }
}
