using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
//using UnityEngine;
using System.Text;

namespace UdpChat
{
    public class Server //: MonoBehaviour
    {
        #region field && state
        //public static string ServerIp = "192.168.2.101"; // server Ip
        //public static string MyIp = "192.168.2.101"; // Client Ip
        public static string ServerIp = "10.246.135.204"; // server Ip
        public static string MyIp = "10.246.135.204"; // Client Ip
        public static int Inport = 48190; // Port for ingoing 
        public static int Outport = 48191; // Port for outgoing 
        public static int c_Inport = 48192; // Port for ingoing 
        public static int c_Outport = 48193; // Port for outgoing

        public static List<IPEndPoint> clients = new List<IPEndPoint>(); // one element for each client.

        public static IPEndPoint ClientIpEndpointIn = null;
        public static IPEndPoint ClientIpEndpointOut = null;
        public static UdpClient server = null;
        public static byte[] receiveData = null;
        public static byte[] data_serialized = null;
        public static byte[] dataInBytes = null;
        public static string BroadcastData = null;

        public static Serialization data_deserialized;
        #endregion
        
        public static void Main()
        {
            try
            {
                Console.WriteLine("Server ready");
                Serialization instance = new Serialization();
                while (true)
                {
                    server = new UdpClient(Inport); //Creates a UdpClient as server for reading incoming data.                     
                    ClientIpEndpointOut = new IPEndPoint(IPAddress.Any, 0);//read datagrams sent from any source.
                    receiveData = server.Receive(ref ClientIpEndpointOut);
                    server.Connect(ClientIpEndpointOut);
                    data_deserialized = instance.Deserialize(receiveData);
                    BroadcastData = DataHandle(data_deserialized);
                    server.Close();
                    if (BroadcastData != "") Broadcast(BroadcastData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void AddClient(IPEndPoint c)
        {
            clients.Add(c); // add a new client.
            Console.WriteLine("<" + c.ToString() + "> is connected");
        }

        private static void RemoveClient(IPEndPoint c)
        {
            clients.Remove(c); // remove a client.
            Console.WriteLine("<" + c.ToString() + "> is disconnected");
        }

        private static String DataHandle(Serialization data_deserialized)
        { // data processing
            Server instance = new Server();
            try
            {
                if (data_deserialized != null)
                {
                    switch (data_deserialized.ChatDataType)
                    {
                        case "I":
                            instance.WriteMessage(data_deserialized);
                            Console.WriteLine("It is a new client");
                            AddClient(ClientIpEndpointOut);
                            //clients.ForEach(Console.WriteLine);
                            BroadcastData = "The client <" + ClientIpEndpointOut.ToString() + "> is connected";
                            break;
                        case "O":
                            instance.WriteMessage(data_deserialized);
                            RemoveClient(ClientIpEndpointOut);
                            BroadcastData = "The client <" + ClientIpEndpointOut.ToString() + "> is disconnected";
                            break;
                        case "M":
                            if (clients.Contains(ClientIpEndpointOut) == true)
                            {
                                if (data_deserialized.ChatMessage != null)
                                {
                                    instance.WriteMessage(data_deserialized);
                                    BroadcastData = "The client<" + ClientIpEndpointOut.ToString() + "> sent the message:" + data_deserialized.ChatMessage;
                                }
                            }
                            else
                                BroadcastData = "";
                            break;
                        case "N":
                            BroadcastData = "";
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return BroadcastData;
        }

        public void WriteMessage(Serialization data_deserialized)
        {
            Console.WriteLine("The reveived packet:" + Encoding.Default.GetString(receiveData));
            Console.WriteLine("DataType:" + data_deserialized.ChatDataType);
            Console.WriteLine("Massage:" + data_deserialized.ChatMessage);
            Console.WriteLine("Time:" + data_deserialized.ChatTime);
            Console.WriteLine("Name:" + data_deserialized.ChatName);
        }

        public static void Broadcast(string data)
        {
            Serialization instance = new Serialization();
            foreach (IPEndPoint cl in clients) // send to each client
            {
                try
                {
                    server = new UdpClient(Outport); //Creates a UdpClient as server for reading outcoming data.
                    ClientIpEndpointIn = new IPEndPoint(cl.Address, c_Inport);

                    if (ClientIpEndpointIn.Address.ToString() == ClientIpEndpointOut.Address.ToString())
                    {
                        server.Connect(ClientIpEndpointIn); 
                        instance.ChatName = "Server";
                        instance.ChatDataType = "Message";

                        data_serialized = instance.Serialize(data); // serialize
                        server.Send(data_serialized, data_serialized.Length); // send data
                        Console.WriteLine("The message was sent to " + ClientIpEndpointIn.ToString());
                    }
                    server.Close();
                }
                catch (Exception e) // this UdpClient is closed
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
    /// <summary>
    /// /////////////////packetize && depacketize
    /// </summary>
    public class Serialization
    {
        private string Type;
        private string Time;
        private string Name;
        private string Message;
        public string ChatDataType
        {
            get { return Type; }
            set { Type = value; }
        }
        public string ChatTime
        {
            get { return Time; }
            set { Time = value; }
        }
        public string ChatName
        {
            get { return Name; }
            set { Name = value; }
        }
        public string ChatMessage
        {
            get { return Message; }
            set { Message = value; }
        }
        
        public Serialization()
        {
            this.Type = null;
            this.Time = null;
            this.Name = null;
            this.Message = null;
        }

        public byte[] Serialize(string data)
        {
            using (MemoryStream result = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(result))
                {
                    writer.Write(Type);
                    writer.Write(DateTime.Now.ToString());
                    writer.Write(Name);
                    writer.Write(data);
                }
                return result.ToArray();
            }
        }

        public Serialization Deserialize(byte[] dataStream)
        {
            Serialization result = new Serialization();
            using (MemoryStream m = new MemoryStream(dataStream))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.Type = reader.ReadString();
                    result.Time = reader.ReadString();
                    result.Name = reader.ReadString();
                    result.Message = reader.ReadString();
                }
            }
            return result;
        }
    }
}
