
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.ServerList
{

    public class Test : MonoBehaviour
    {
        ServerConnection advertConnection;
        // Use this for initialization
        void Start()
        {
            advertConnection = new ServerConnection();
            // Connect
            advertConnection.Connect("207.180.194.138", 9423).AsyncWaitHandle.WaitOne();
            // Create server data
            Dictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "Players", (int)10 },
                        { "Name", "Random servunimi " + UnityEngine.Random.Range(0,50).ToString() }
                    };
            // Register server
            advertConnection.StartAdvertisment(data);
        }
        private void OnDisable()
        {
            if (advertConnection != null)
            {
                advertConnection.StopAdvertising();
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                using (ServerConnection queryConnection = new ServerConnection())
                {
                    // Connect
                    queryConnection.Connect("207.180.194.138", 9423).AsyncWaitHandle.WaitOne();

                    // Send query
                    List<ServerModel> models = queryConnection.SendQuery();
                    Debug.Log("Servereitä löytyi " + models.Count + " kpl \n");
                    Debug.Log(string.Format("| {0,5} | {1,5} | {2,5} | {3,5 }", "UUID", "Serverinimi", "Pelaajamäärä", "IP-Osoite (v4) (Host)"));
                    Debug.Log(string.Join(Environment.NewLine, models.Select(x => string.Format("| {0,5} | {1,5} | {2,5} | {3,5} |", x.Id, x.ServerData["Name"], x.ServerData["Players"], x.Address.MapToIPv4()))));
                }
            }
        }

    }
}