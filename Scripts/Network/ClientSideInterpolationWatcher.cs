using MLAPI;
using MLAPI.Prototyping;
using MLAPI.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientSideInterpolationWatcher : NetworkBehaviour
{
    bool isEnabled = false;
    [SerializeField] List<InterpolationState> interpolationStates = new List<InterpolationState>();
    private void OnEnable()
    {
        GameModeManager.onGameModeStart += toggleEnabled;
        GameModeManager.onGameModeEnd += toggleEnabled;
        GameModeManager.onGameModeStart += addPlayers;
        GameModeManager.onNewHotJoin += addNewPlayer;
        PlayerManager.onPlayerDisconnection += removeDisconnectedPlayer;
    }

    private void removeDisconnectedPlayer(ulong objectID, ulong networkID)
    {
        interpolationStates.Remove(interpolationStates.Find(match => match.OwnerID == networkID));
    }

    private void addNewPlayer(ulong ID)
    {
        if (!IsLocalPlayer) return;

        ConnectPlayer player = PlayerManager.Singleton.GetPlayerInfoByNetworkObject(ID).GetComponent<ConnectPlayer>();
        //Älä lisää itseäsi uudelleen!
        if (player.IsLocalPlayer) return;
        InterpolationState interpolationState = new InterpolationState()
        {
            NetworkTransform = player.GetComponent<NetworkTransform>(),
            OwnerID = player.OwnerClientId,
            InterpolationAmount = 0
        };
        interpolationStates.Add(interpolationState);
    }

    private void addPlayers()
    {
        if (!IsLocalPlayer) return;
        ConnectPlayer[] players = FindObjectsOfType<ConnectPlayer>();
        for (int i = 0; i < players.Length; i++)
        {
            InterpolationState interpolationState = new InterpolationState()
            {
                NetworkTransform = players[i].GetComponent<NetworkTransform>(),
                OwnerID = players[i].OwnerClientId,
                InterpolationAmount = 0
            };
            interpolationStates.Add(interpolationState);
        }
    }

    private void OnDisable()
    {
        GameModeManager.onGameModeEnd -= toggleEnabled;
        GameModeManager.onGameModeEnd -= toggleEnabled;
        GameModeManager.onGameModeStart -= addPlayers;
        PlayerManager.onPlayerDisconnection -= removeDisconnectedPlayer;
    }

    private void toggleEnabled()
    {
        isEnabled = !isEnabled;

    }
    private void Update()
    {
        if (!IsLocalPlayer) return;
        if (isEnabled)
        {
            for (int i = 0; i < interpolationStates.Count; i++)
            {
                interpolationStates[i].InterpolationAmount = interpolationStates[i].NetworkTransform.currentLerpTime;
            }
        }
    }
    public InterpolationStateInfo[] getWrappedPlayersAndInterpolations()
    {
        InterpolationStateInfo[] tempPlayers = new InterpolationStateInfo[interpolationStates.Count];
        for (int i = 0; i < interpolationStates.Count; i++)
        {
            tempPlayers[i] = new InterpolationStateInfo()
            {
                OwnerID = interpolationStates[i].OwnerID,
                InterpolationAmount = interpolationStates[i].InterpolationAmount
            };
        }
        return tempPlayers;
    }


}
[System.Serializable]
class InterpolationState
{
    NetworkTransform networkTransform;
    [SerializeField]
    ulong ownerID;
    [SerializeField]
    float interpolationAmount;


    public NetworkTransform NetworkTransform { get => networkTransform; set => networkTransform = value; }
    public ulong OwnerID { get => ownerID; set => ownerID = value; }
    public float InterpolationAmount { get => interpolationAmount; set => interpolationAmount = value; }
}
public struct InterpolationStateInfo : INetworkSerializable
{
    ulong ownerID;
    float interpolationAmount;

    public float InterpolationAmount { get => interpolationAmount; set => interpolationAmount = value; }
    public ulong OwnerID { get => ownerID; set => ownerID = value; }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref ownerID);
        serializer.Serialize(ref interpolationAmount);
    }
}