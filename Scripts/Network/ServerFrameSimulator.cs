using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerFrameSimulator : NetworkBehaviour
{


    int _frameHistoryCount = 64 * 3;
    /*Tick toimii tick frequencyn avulla. Kokonaisluku kuvastaa yhtä tickiä.
      Hauska huomio on että käytetään tilanteessa time.unscaledtimea,
      joka tarkoittaa time menettää tarkkuutensa 139 810 minuutin päästä, mutta tuskin meillä on
      niin pitkää peliä :^) (floatin merkitseviä lukuja on 23.) 
      */
    int _timeTickValue = 0;
    /// <summary>
    ///  Merkitsee sitä, että kuinka monta kertaa sekunnissa tulee tick. 0.015625s * 64 ticks = 1s => 64tick/s
    /// </summary>
    float _tickFrequency = 0.015625f;

    Dictionary<ulong, TickRateSimulatedObject> simulatedFrameObjects = new Dictionary<ulong, TickRateSimulatedObject>();
    NetworkVariable<int> frameTime = new NetworkVariable<int>();
    /// <summary>
    /// Serveri tallentaa näin monta tickiä pelaajien tietoja (64tick*3s) 
    /// </summary>
    public int FrameHistoryCount { get => _frameHistoryCount; set => _frameHistoryCount = value; }
    public NetworkVariable<int> FrameTime { get => frameTime; set => frameTime = value; }
    /// <summary>
    /// Sisältää kaikki networktransformit, jotka ovat pelaajia. (eli kaikki tickattavat objektit)
    /// </summary>
    public Dictionary<ulong, TickRateSimulatedObject> SimulatedFrameObjects { get => simulatedFrameObjects; set => simulatedFrameObjects = value; }

    static ServerFrameSimulator singleton;
    public static ServerFrameSimulator Singleton
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindObjectOfType<ServerFrameSimulator>();
            }
            return singleton;
        }
    }
    //Seuraava event tapahtuu vain serverillä, joten ei tarvita playermanagerin tietoja.
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += removeDisconnectedPlayer;
    }
    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= removeDisconnectedPlayer;
    }
    private void removeDisconnectedPlayer(ulong obj)
    {
        SimulatedFrameObjects.Remove(obj);
    }

    public void Simulate(int frameID, InterpolationStateInfo[] allIDs, Action action)
    {

        for (int i = 0; i < allIDs.Length; i++)
        {
            if (simulatedFrameObjects.ContainsKey(allIDs[i].OwnerID)) SimulatedFrameObjects[allIDs[i].OwnerID].SetTransform(frameID, allIDs[i].InterpolationAmount);
        }
        Physics.SyncTransforms();
        action.Invoke();
        for (int i = 0; i < allIDs.Length; i++)
        {
            if (simulatedFrameObjects.ContainsKey(allIDs[i].OwnerID)) SimulatedFrameObjects[allIDs[i].OwnerID].ResetTransform();
        }
        Physics.SyncTransforms();
    }
    /// <summary>
    /// Server only, tallentaa pelaajien sijainnit tickkiedolla.
    /// </summary>
    /// <param name="timeStamp"></param>
    [ServerRpc]
    private void updatePlayersServerRpc(int timeStamp)
    {

        foreach (var item in simulatedFrameObjects)
        {
            item.Value.AddFrameData(timeStamp);
        }

    }

    public override void NetworkStart()
    {
        base.NetworkStart();
        if (!IsOwner) return;
        StartCoroutine(TickThread());


    }

    private IEnumerator TickThread()
    {
        while (true)
        {
            _timeTickValue++;
            FrameTime.Value++;
            updatePlayersServerRpc(_timeTickValue);
            yield return new WaitForSecondsRealtime(_tickFrequency);
        }


    }

}
