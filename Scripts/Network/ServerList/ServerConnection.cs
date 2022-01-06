using ServerList.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Assets.Scripts.ServerList
{
    class ServerConnection : IDisposable
    {
        private readonly Dictionary<Guid, Action<List<ServerModel>>> queryCallbacks = new Dictionary<Guid, Action<List<ServerModel>>>();
        private Dictionary<Guid, Action<bool>> validationCallbacks = new Dictionary<Guid, Action<bool>>();
        private TcpClient client = new TcpClient();
        private bool isAdvertising;
        private Guid? registerGuid = null;
        private Dictionary<string, object> advertismentData = null;

        public List<ServerModel> SendQuery()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
                {
                    stream.Position = 2;

                    AutoResetEvent resetEvent = new AutoResetEvent(false);
                    List<ServerModel> serverModels = null;

                    Guid guid = Guid.NewGuid();

                    queryCallbacks.Add(guid, (models) =>
                    {
                        serverModels = models;
                        resetEvent.Set();
                    });

                    // Write packet
                    writer.Write((byte)MessageType.Query);
                    writer.Write(guid.ToString());
                    ;

                    stream.Position = 0;
                    for (byte i = 0; i < sizeof(ushort); i++) stream.WriteByte(((byte)(stream.Length >> (i * 8))));
                    client.Client.Send(stream.GetBuffer(), 0, (int)stream.Length, SocketFlags.None);

                    // Wait for response
                    resetEvent.WaitOne();

                    return serverModels;
                }
            }
        }
        public IAsyncResult Connect(string host, int port)
        {
            return client.BeginConnect(host, port, (args) =>
            {
                client.EndConnect(args);

                new Thread(() =>
                {
                    while (client.Connected)
                    {
                        try
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                // Buffer to copy between NetworkStream and MemoryStream
                                byte[] buffer = new byte[1024];

                                // Only do when there is data
                                while (client.GetStream().DataAvailable)
                                {
                                    // Read data from the NetworkStream in increments of 1024
                                    int count = client.GetStream().Read(buffer, 0, 1024);

                                    // Write the data to the MemoryStream
                                    stream.Write(buffer, 0, count);
                                }

                                // Set stream at start
                                stream.Position = 0;

                                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
                                {
                                    byte messageType = reader.ReadByte();

                                    if (messageType == (byte)MessageType.QueryResponse)
                                    {
                                        Guid callbackGuid = new Guid(reader.ReadString());

                                        int serverCount = reader.ReadInt32();

                                        List<ServerModel> models = new List<ServerModel>();

                                        for (int i = 0; i < serverCount; i++)
                                        {
                                            Guid serverGuid = new Guid(reader.ReadString());
                                            IPAddress address = new IPAddress(reader.ReadBytes(16));
                                            DateTime lastPing = DateTime.FromBinary(reader.ReadInt64());

                                            ServerModel model = new ServerModel()
                                            {
                                                Id = serverGuid,
                                                Address = address,
                                                LastPingTime = lastPing,
                                                ServerData = new Dictionary<string, object>()
                                            };

                                            int dataCount = reader.ReadInt32();

                                            for (int x = 0; x < dataCount; x++)
                                            {
                                                string name = reader.ReadString();
                                                ServerDataType type = (ServerDataType)reader.ReadByte();

                                                switch (type)
                                                {
                                                    case ServerDataType.Int8:
                                                        model.ServerData[name] = reader.ReadSByte();
                                                        break;
                                                    case ServerDataType.Int16:
                                                        model.ServerData[name] = reader.ReadInt16();
                                                        break;
                                                    case ServerDataType.Int32:
                                                        model.ServerData[name] = reader.ReadInt32();
                                                        break;
                                                    case ServerDataType.Int64:
                                                        model.ServerData[name] = reader.ReadInt64();
                                                        break;
                                                    case ServerDataType.UInt8:
                                                        model.ServerData[name] = reader.ReadByte();
                                                        break;
                                                    case ServerDataType.UInt16:
                                                        model.ServerData[name] = reader.ReadUInt16();
                                                        break;
                                                    case ServerDataType.UInt32:
                                                        model.ServerData[name] = reader.ReadUInt32();
                                                        break;
                                                    case ServerDataType.UInt64:
                                                        model.ServerData[name] = reader.ReadUInt64();
                                                        break;
                                                    case ServerDataType.String:
                                                        model.ServerData[name] = reader.ReadString();
                                                        break;
                                                    case ServerDataType.Buffer:
                                                        model.ServerData[name] = reader.ReadBytes(reader.ReadInt32());
                                                        break;
                                                    case ServerDataType.Guid:
                                                        model.ServerData[name] = new Guid(reader.ReadString());
                                                        break;
                                                }
                                            }

                                            models.Add(model);
                                        }

                                        if (queryCallbacks.TryGetValue(callbackGuid, out Action<List<ServerModel>> callback))
                                        {
                                            queryCallbacks.Remove(callbackGuid);
                                            callback(models);
                                        }
                                    }
                                    else if (messageType == (byte)MessageType.RegisterAck)
                                    {
                                        registerGuid = new Guid(reader.ReadString());
                                        bool success = reader.ReadBoolean();

                                        if (!success)
                                        {
                                            // TODO: Error
                                        }
                                    }
                                    else if (messageType == (byte)MessageType.DataResponse)
                                    {
                                        Guid callbackGuid = new Guid(reader.ReadString());
                                        bool success = reader.ReadBoolean();

                                        if (validationCallbacks.TryGetValue(callbackGuid, out Action<bool> callback))
                                        {
                                            validationCallbacks.Remove(callbackGuid);
                                            callback(success);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }).Start();
            }, null);
        }
        public void StopAdvertising()
        {
            if (!isAdvertising)
            {
                // TODO: Throw
                return;
            }

            isAdvertising = false;

            DateTime startTime = DateTime.Now;

            while (registerGuid == null && (DateTime.Now - startTime).TotalMilliseconds < 10_000)
            {
                Thread.Sleep(10);
            }

            if (registerGuid != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
                    {
                        stream.Position = 2;

                        writer.Write((byte)MessageType.RemoveServer);
                        writer.Write(registerGuid.Value.ToString());

                        stream.Position = 0;
                        for (byte i = 0; i < sizeof(ushort); i++) stream.WriteByte(((byte)(stream.Length >> (i * 8))));
                        client.Client.Send(stream.GetBuffer(), 0, (int)stream.Length, SocketFlags.None);
                    }
                }
            }
        }
        public void UpdateAdvertismentData(Dictionary<string, object> data)
        {
            if (!isAdvertising)
            {
                // TODO: Throw
                return;
            }

            DateTime startTime = DateTime.Now;

            while (registerGuid == null && (DateTime.Now - startTime).TotalMilliseconds < 10_000)
            {
                Thread.Sleep(10);
            }

            if (registerGuid != null)
            {
                Type[] acceptedTypes = new Type[]
                {
                    typeof(sbyte),
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(byte),
                    typeof(ushort),
                    typeof(uint),
                    typeof(ulong),
                    typeof(string),
                    typeof(byte[]),
                    typeof(Guid)
                };

                advertismentData = data.Where(x => acceptedTypes.Contains(x.Value.GetType())).ToDictionary(x => x.Key, x => x.Value);

                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
                    {
                        stream.Position = 2;

                        writer.Write((byte)MessageType.UpdateServer);
                        writer.Write(registerGuid.Value.ToString());

                        writer.Write(advertismentData.Count);

                        foreach (KeyValuePair<string, object> pair in advertismentData)
                        {
                            writer.Write(pair.Key.GetStableHash64());

                            if (pair.Value is sbyte)
                            {
                                writer.Write((byte)ServerDataType.Int8);
                                writer.Write((sbyte)pair.Value);
                            }
                            else if (pair.Value is short)
                            {
                                writer.Write((byte)ServerDataType.Int16);
                                writer.Write((short)pair.Value);
                            }
                            else if (pair.Value is int)
                            {
                                writer.Write((byte)ServerDataType.Int32);
                                writer.Write((int)pair.Value);
                            }
                            else if (pair.Value is long)
                            {
                                writer.Write((byte)ServerDataType.Int64);
                                writer.Write((long)pair.Value);
                            }
                            else if (pair.Value is byte)
                            {
                                writer.Write((byte)ServerDataType.UInt8);
                                writer.Write((byte)pair.Value);
                            }
                            else if (pair.Value is ushort)
                            {
                                writer.Write((byte)ServerDataType.UInt16);
                                writer.Write((ushort)pair.Value);
                            }
                            else if (pair.Value is uint)
                            {
                                writer.Write((byte)ServerDataType.UInt32);
                                writer.Write((uint)pair.Value);
                            }
                            else if (pair.Value is ulong)
                            {
                                writer.Write((byte)ServerDataType.UInt64);
                                writer.Write((ulong)pair.Value);
                            }
                            else if (pair.Value is byte[] bytes)
                            {
                                writer.Write((byte)ServerDataType.Buffer);
                                writer.Write(bytes.Length);
                                writer.Write(bytes);
                            }
                            else if (pair.Value is Guid guid)
                            {
                                writer.Write((byte)ServerDataType.Guid);
                                writer.Write(guid.ToString());
                            }
                            else if (pair.Value is string)
                            {
                                writer.Write((byte)ServerDataType.String);
                                writer.Write((string)pair.Value);
                            }
                        }

                        stream.Position = 0;
                        for (byte i = 0; i < sizeof(ushort); i++) stream.WriteByte(((byte)(stream.Length >> (i * 8))));
                        client.Client.Send(stream.GetBuffer(), 0, (int)stream.Length, SocketFlags.None);
                    }
                }
            }
        }
        public void StartAdvertisment(Dictionary<string, object> data, int delay = 10_000)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
                {
                    stream.Position = 2;

                    writer.Write((byte)MessageType.RegisterServer);

                    Type[] acceptedTypes = new Type[]
                    {
                    typeof(sbyte),
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(byte),
                    typeof(ushort),
                    typeof(uint),
                    typeof(ulong),
                    typeof(string),
                    typeof(byte[]),
                    typeof(Guid)
                    };

                    advertismentData = data.Where(x => acceptedTypes.Contains(x.Value.GetType())).ToDictionary(x => x.Key, x => x.Value);

                    writer.Write(advertismentData.Count);

                    foreach (KeyValuePair<string, object> pair in advertismentData)
                    {
                        writer.Write(pair.Key.GetStableHash64());

                        if (pair.Value is sbyte)
                        {
                            writer.Write((byte)ServerDataType.Int8);
                            writer.Write((sbyte)pair.Value);
                        }
                        else if (pair.Value is short)
                        {
                            writer.Write((byte)ServerDataType.Int16);
                            writer.Write((short)pair.Value);
                        }
                        else if (pair.Value is int)
                        {
                            writer.Write((byte)ServerDataType.Int32);
                            writer.Write((int)pair.Value);
                        }
                        else if (pair.Value is long)
                        {
                            writer.Write((byte)ServerDataType.Int64);
                            writer.Write((long)pair.Value);
                        }
                        else if (pair.Value is byte)
                        {
                            writer.Write((byte)ServerDataType.UInt8);
                            writer.Write((byte)pair.Value);
                        }
                        else if (pair.Value is ushort)
                        {
                            writer.Write((byte)ServerDataType.UInt16);
                            writer.Write((ushort)pair.Value);
                        }
                        else if (pair.Value is uint)
                        {
                            writer.Write((byte)ServerDataType.UInt32);
                            writer.Write((uint)pair.Value);
                        }
                        else if (pair.Value is ulong)
                        {
                            writer.Write((byte)ServerDataType.UInt64);
                            writer.Write((ulong)pair.Value);
                        }
                        else if (pair.Value is byte[] bytes)
                        {
                            writer.Write((byte)ServerDataType.Buffer);
                            writer.Write(bytes.Length);
                            writer.Write(bytes);
                        }
                        else if (pair.Value is Guid guid)
                        {
                            writer.Write((byte)ServerDataType.Guid);
                            writer.Write(guid.ToString());
                        }
                        else if (pair.Value is string)
                        {
                            writer.Write((byte)ServerDataType.String);
                            writer.Write((string)pair.Value);
                        }
                    }

                    stream.Position = 0;
                    for (byte i = 0; i < sizeof(ushort); i++) stream.WriteByte(((byte)(stream.Length >> (i * 8))));
                    client.Client.Send(stream.GetBuffer(), 0, (int)stream.Length, SocketFlags.None);
                }
            }

            isAdvertising = true;

            new Thread(() =>
            {
                DateTime lastRegisterTime = DateTime.Now;

                while (isAdvertising)
                {
                    Thread.Sleep(delay - (int)(DateTime.Now - lastRegisterTime).TotalMilliseconds);

                    if (registerGuid != null && isAdvertising)
                    {
                        lastRegisterTime = DateTime.Now;

                        using (MemoryStream stream = new MemoryStream())
                        {
                            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
                            {
                                stream.Position = 2;

                                writer.Write((byte)MessageType.ServerAlive);
                                writer.Write(registerGuid.Value.ToString());

                                stream.Position = 0;
                                for (byte i = 0; i < sizeof(ushort); i++) stream.WriteByte(((byte)(stream.Length >> (i * 8))));
                                client.Client.Send(stream.GetBuffer(), 0, (int)stream.Length, SocketFlags.None);
                            }
                        }
                    }
                }
            }).Start();
        }
        public void Dispose()
        {
            if (isAdvertising)
            {
                StopAdvertising();
                isAdvertising = false;
            }

            client.Close();
        }
    }
}
