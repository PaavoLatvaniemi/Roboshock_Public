using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    [SerializeField] NetworkVariableString playerName = new NetworkVariableString();
    [SerializeField] NetworkVariableInt playerKills = new NetworkVariableInt(0);
    NetworkVariableInt playerDeaths = new NetworkVariableInt(0);
    [SerializeField] NetworkVariableBool playerIsHost = new NetworkVariableBool();

    public string PlayerName => playerName.Value;
    public int PlayerKills => playerKills.Value;
    public int PlayerDeaths => playerDeaths.Value;
    public bool PlayerIsHost => playerIsHost.Value;



    public delegate void KillAdded(PlayerInfo playerInfo);
    public static event KillAdded onKillAdded;

    public delegate void DeathAdded(PlayerInfo playerInfo);
    public static event DeathAdded onDeathAdded;

    public delegate void NotifyKill(string killName, int killCount);
    public static event NotifyKill onKill;

    [ServerRpc]
    public void SetPlayerNameServerRpc(string name) => playerName.Value = name;
    [ServerRpc]
    public void SetPlayerIsHostServerRpc(bool state) => playerIsHost.Value = state;

    public void AddKill(string killed, ClientRpcParams clientRpcParams)
    {
        if (!IsServer) return;
        playerKills.Value++;
        onKillAdded?.Invoke(this);
        //Ei voida antaa tietoa ihan heti, annetaan rpc viestin eka pyöriä loppuun.
        StartCoroutine(TaskDelayer.createDelayedTask(() => NotifyNewKillToClientRpc(NetworkObjectId), 0.1f));

        //Annetaan tieto vaan yksittäiselle pelaajalle (mm "You killed x!" teksti.)
        OnNewKillClientRpc(killed, playerKills.Value, clientRpcParams);

    }
    [ClientRpc]
    private void NotifyNewKillToClientRpc(ulong networkObjectId)
    {
        if (IsServer) return;
        onKillAdded?.Invoke(GetNetworkObject(networkObjectId).GetComponent<PlayerInfo>());
    }

    [ClientRpc]
    private void OnNewKillClientRpc(string killed, int killCount, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        onKill?.Invoke(killed, killCount);

    }

    public void AddDeath()
    {
        if (!IsServer) return;
        playerDeaths.Value++;
        onDeathAdded?.Invoke(this);
    }

    public void ClearStats()
    {
        if (!IsServer) return;
        playerKills.Value = 0;
        playerDeaths.Value = 0;

    }



}
