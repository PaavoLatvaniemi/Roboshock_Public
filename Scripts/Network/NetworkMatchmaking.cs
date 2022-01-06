using Assets.Scripts.ServerList;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Puncher.Client;
using MLAPI.Transports.UNET;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Network
{

    public class NetworkMatchmaking : MonoBehaviour
    {

        public delegate void ServersFound(List<ServerModel> models);
        public static event ServersFound onServersFound;
        string playerServerName;
        //Matchmakingin queryn hakemat servut
        List<ServerModel> servers = new List<ServerModel>();
        //Lobbyn teon mainostama yhteys serveriin.
        static ServerConnection serverAdvertisementConnection;
        public delegate void ServerHosted();
        public static event ServerHosted onServerHosted;
        static NetworkMatchmaking singleton;
        public static NetworkMatchmaking Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = FindObjectOfType<NetworkMatchmaking>();
                }
                return singleton;
            }
        }

        public void RelayAndHostServer(string serverName)
        {
            playerServerName = serverName;
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect("192.168.1.1", 1234);
                    Debug.Log("Port open");
                }
                catch (Exception)
                {
                    Debug.Log("Port closed");
                }
            }
            Task listenTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    using (PuncherClient listenPeer = new PuncherClient("207.180.194.138", 6776))
                    {

                        listenPeer.ListenForPunches(new IPEndPoint(IPAddress.Any, 1234));

                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                }

            });
            HostServer();
            AdvertiseNewServer();
        }
        public void HostServer(bool relayOn = true)
        {
            if (!relayOn)
            {
                NetworkManager.Singleton.GetComponent<UNetTransport>().UseMLAPIRelay = false;
            }
            NetworkManager.Singleton.StartHost();
            onServerHosted?.Invoke();
        }
        public void DisconnectFromServer()
        {
            NetworkManager.Singleton.StopClient();
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.StopHost();
            }
        }
        public void QueryMatchmatchMakingServer()
        {
            using (ServerConnection queryConnection = new ServerConnection())
            {
                // Connect
                queryConnection.Connect("207.180.194.138", 9423).AsyncWaitHandle.WaitOne();

                // Send query
                List<ServerModel> models = queryConnection.SendQuery();

                servers.Clear();
                servers.AddRange(models);
                onServersFound?.Invoke(servers);


            }
        }
        public void DirectConnectToIPv4(string ip, bool noRelay = false)
        {
            if (noRelay)
            {
                NetworkManager.Singleton.GetComponent<UNetTransport>().UseMLAPIRelay = false;
            }
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ip;
            ConnectPlayer();
        }

        private static void ConnectPlayer()
        {
            NetworkManager.Singleton.StartClient();

        }

        public void ConnectToServerIndex(int i)
        {
            if (!TryNatPunchingFirst(servers[i].Address.MapToIPv4().ToString()))
            {
                NetworkManager.Singleton.GetComponent<UNetTransport>().UseMLAPIRelay = true;
                NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = servers[i].Address.MapToIPv4().ToString();
            }


            ConnectPlayer();
        }
        private bool TryNatPunchingFirst(string ipAddress)
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().UseMLAPIRelay = false;
            // Get listener public IP address by means of a matchmaker or otherwise.
            using (PuncherClient connector = new PuncherClient("207.180.194.138", 6776))
            {
                // Punches and returns the result
                if (connector.TryPunch(IPAddress.Parse(ipAddress), out IPEndPoint remoteEndPoint))
                {
                    // NAT Punchthrough was successful. It can now be connected to using your normal connection logic.
                    NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = remoteEndPoint.ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        private void AdvertiseNewServer()
        {
            serverAdvertisementConnection = new ServerConnection();
            //VPS serveri joka hallitsee serverilistausta ottaa talteen lobbyn. PORT 9423 TCP
            serverAdvertisementConnection.Connect("207.180.194.138", 9423).AsyncWaitHandle.WaitOne();
            // Luo server data
            Dictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "Players", 1 },
                        { "Name", playerServerName }
                    };
            // Rekisteröi serveri annetuilla tiedoilla.
            serverAdvertisementConnection.StartAdvertisment(data);
            onServerHosted?.Invoke();
        }
        public void StopAdvertising()
        {
            if (serverAdvertisementConnection != null)
            {
                serverAdvertisementConnection.StopAdvertising();
            }

        }
        private void OnDisable()
        {
            if (serverAdvertisementConnection != null)
            {
                serverAdvertisementConnection.StopAdvertising();
            }
        }
        public bool IsHost()
        {
            return NetworkManager.Singleton.IsHost;
        }
        [ServerRpc]
        public void StartGameModeServerRpc()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                GameModeManager.Singleton.StartGameMode();
            }

        }
        [ServerRpc]
        public void EndGameModeServerRpc()
        {
            if (IsHost())
                GameModeManager.Singleton.EndGameMode();
        }
    }
}
