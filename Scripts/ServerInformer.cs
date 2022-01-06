using Assets.Scripts.Network;
using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerInformer : MonoBehaviour
{
    //[SerializeField] TextMeshProUGUI informText;
    //[SerializeField] Transform informerObject;
    //private void OnEnable()
    //{
    //    NetworkMatchmaking.onStartingServer += openInformer;
    //    NetworkMatchmaking.onNatFailed += natFailureInformation;
    //    NetworkMatchmaking.onListenerFail += serverFailureInformation;
    //    //NetworkManager.Singleton.OnClientConnectedCallback += tellNewConnection;
    //}

    //private void tellNewConnection(ulong obj)
    //{
    //    informText.text += "\nA player has connected, welcome," + NetworkManager.Singleton.ConnectedClients[obj].PlayerObject.GetComponent<PlayerInfo>().PlayerName;
    //}

    //private void serverFailureInformation()
    //{
    //    informText.text = "\n[ERROR] Server hosting failed!" + "\nPlease open port 7777 (UDP)"
    //        + "\n From your modem firewall.";
    //}

    //private void natFailureInformation()
    //{
    //    informText.text += "\n[WARNING] Failed opening port for direct connections!\n" +
    //        "The server will use a slower relay for connection instead. \n" +
    //        "To enable direct NAT connections, please open port 1234 (UDP) \n"
    //        + "to enable NAT-punchthrough connectivity.";
    //}

    //private void OnDisable()
    //{
    //    NetworkMatchmaking.onStartingServer -= openInformer;
    //    NetworkMatchmaking.onNatFailed -= natFailureInformation;
    //    NetworkMatchmaking.onListenerFail -= serverFailureInformation;
    //    NetworkManager.Singleton.OnClientConnectedCallback -= tellNewConnection;
    //}
    //private void openInformer()
    //{
    //    informerObject.gameObject.SetActive(true);
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
