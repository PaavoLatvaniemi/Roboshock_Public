using MLAPI;
using MLAPI.Messaging;
using MLAPI.Prototyping;
using MLAPI.Serialization;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Pelaaja on liikkumisen authority, mutta
/// ampuminen on t‰ysin serveripohjaista cheattaamisen v‰ltt‰miseksi
/// Pelaajalta tarvitsee tallentaa freimitietoa, jotta osuminen
/// olisi huomattavasti tarkempaa clienttien v‰lill‰, etenkin
/// ottaen huomioon serverin eri viiveet. (Pakettien l‰hetyksess‰ ym. kest‰‰ liian kauan ett‰ nopeatempoinen peli tuntuisi normaalisti hyv‰lt‰)
/// </summary>
public class TickRateSimulatedObject : NetworkBehaviour
{
    Dictionary<int, FrameData> frameData = new Dictionary<int, FrameData>();
    public List<int> FrameKeys = new List<int>();
    //Tallennetaan, jotta voidaan interpoloida pelaajan sijainti oikein ampujan tietojen perusteella.
    ulong playerID;
    FrameData saveData = new FrameData();
    NetworkTransform netTransform;


    private void Start()
    {
        playerID = GetComponent<ConnectPlayer>().OwnerClientId;
        addServerRpc();
        netTransform = GetComponent<NetworkTransform>();
        if (!IsLocalPlayer) return;
    }

    private void FixedUpdate()
    {
        if (!IsLocalPlayer) return;


    }



    [ServerRpc]
    private void addServerRpc()
    {
        if (ServerFrameSimulator.Singleton.SimulatedFrameObjects.ContainsKey(playerID)) return;
        ServerFrameSimulator.Singleton.SimulatedFrameObjects.Add(playerID, this);

    }

    private void OnDisable()
    {
        if (!ServerFrameSimulator.Singleton.SimulatedFrameObjects.ContainsKey(playerID)) return;
        ServerFrameSimulator.Singleton.SimulatedFrameObjects.Remove(playerID);
    }


    public void AddFrameData(int time)
    {

        if (FrameKeys.Count >= ServerFrameSimulator.Singleton.FrameHistoryCount)
        {
            int key = FrameKeys[0];
            FrameKeys.RemoveAt(0);
            frameData.Remove(key);
        }
        if (!frameData.ContainsKey(time))
        {
            frameData.Add(time, new FrameData(transform.position, transform.rotation.eulerAngles));
            FrameKeys.Add(time);
        }





    }

    public void SetTransform(int frameNumber, float subFrame)
    {


        saveData.Position = transform.position;
        saveData.Rotation = transform.rotation.eulerAngles;
        //Modattu MLAPI l‰hdekoodi. Property network transformista, jos aiheuttaa compile erroreita, kirjasto on 
        //viel‰ se alkuper‰inen. (Tarvitaan currentLerpTime m_lerpTimesta). Tietoa k‰ytet‰‰n laskemaan "subframe" interpolaatiosta, mik‰ nettransformissa on

        if (frameNumber > FrameKeys[FrameKeys.Count-1])
        {
            frameNumber = FrameKeys[FrameKeys.Count - 1];
        }
        float lerpTime = netTransform.currentLerpTime;
        FrameData newFrame = new FrameData();
        FrameData oldFrame = new FrameData();
        try
        {
            newFrame = frameData[frameNumber];
            oldFrame = frameData[frameNumber - 1];
        }
        catch (KeyNotFoundException)
        {
            newFrame = frameData[FrameKeys.Count - 2];
            oldFrame = newFrame;
        }
        transform.position = Vector3.Lerp(oldFrame.Position, newFrame.Position, 1/subFrame);
        transform.rotation = Quaternion.Euler(newFrame.Rotation);
    }

    public void ResetTransform()
    {

        transform.position = saveData.Position;
        transform.rotation = Quaternion.Euler(saveData.Rotation);
    }


}
struct FrameData : INetworkSerializable
{
    Vector3 pos;
    Vector3 rot;

    public Vector3 Position { get => pos; set => pos = value; }
    public Vector3 Rotation { get => rot; set => rot = value; }

    public FrameData(Vector3 position, Vector3 rotation)
    {
        pos = Vector3.zero;
        rot = Vector3.zero;

        Position = position;
        Rotation = rotation;
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref pos);
        serializer.Serialize(ref rot);
    }
}