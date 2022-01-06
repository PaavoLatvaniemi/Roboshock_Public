using System;
using System.Collections.Generic;
using System.Net;

namespace Assets.Scripts.ServerList
{
    public class ServerModel
    {
        public Guid Id;
        public IPAddress Address { get; set; } = new IPAddress(0);
        public Dictionary<string, object> ServerData { get; set; } = new Dictionary<string, object>();
        public DateTime LastPingTime;
    }
}
