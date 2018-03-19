using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace MyApp
{
    public class MyNetwork
    {
        public int s_SendPort = 48199;
        public int s_ReceivePort = 600;
        public int c_SendPort = 9000;
        public int c_ReceivePort = 5000;

        public int ReceivePort;
        public int SendPort;      
        public IPEndPoint IPEP;
    
        public UdpClient Sender;
        public IPEndPoint Objective;
        public UdpClient Receiver;     
        public IPEndPoint RefPoint;

        public static string serverAddress = "10.246.141.115";
        public bool isServer = true;

        public byte[] buffer = new byte[1024];
        HandlePacket handlePacket = new HandlePacket();
        static  MyNetwork instance = new MyNetwork();
        public string pl;  // payload

        public List<IPEndPoint> ClientList = new List<IPEndPoint>();

        public enum ClientStatus
        {
            New, Old, Left
        }

        public byte [] ClientWritePacket(string pl, int len_msg)
        {
            HandlePacket.Packet pkt = new HandlePacket.Packet(pl, len_msg);
            buffer = handlePacket.Serialize(pkt);
            return buffer;
        }

        public IPEndPoint Conversion(IPEndPoint ipep)
        {
            IPEP = new IPEndPoint(ipep.Address, c_ReceivePort);
            return (IPEP);
        }

        public void AddClient(IPEndPoint IPEP)
        {
            ClientStatus cs;
            if (ClientList.Contains(IPEP) == false)
            {
                cs = ClientStatus.New;
                ClientList.Add(IPEP);
                Console.WriteLine("Newly registered");
            }
            else
            {
                cs = ClientStatus.Old;
            }
        }

        public void BroadcastMessage(byte[] msg, List<IPEndPoint> list, IPEndPoint specifiedIP)
        {           
            foreach (var ip in ClientList)
            {
                string _ip = ip.ToString();
                string _specifiedIP = specifiedIP.ToString();
                bool isEqual = _ip.Equals(_specifiedIP);
                if (!isEqual)
                {                  
                    Sender.Client.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, ip, OnSend, null);
                }               
            }
        }

        void Awake()
        {
            // Receiving Message
            if (isServer)
            {
                this.ReceivePort = s_ReceivePort;
                this.SendPort = s_SendPort; 
                Console.WriteLine("Server started, servicing on port {0}:", this.ReceivePort);
                this.Objective = new IPEndPoint(IPAddress.Any, c_ReceivePort);
                this.RefPoint = new IPEndPoint(IPAddress.Any, c_SendPort);
            }
            else
            {
                this.ReceivePort = c_ReceivePort;
                this.SendPort = c_SendPort;
                Console.WriteLine("This is a client");
                this.Objective = new IPEndPoint(IPAddress.Parse(serverAddress), s_ReceivePort);
                this.RefPoint = new IPEndPoint(IPAddress.Parse(serverAddress), s_SendPort);
                this.ClientList.Add(this.Objective);
            }
            this.Sender = new UdpClient(this.SendPort);
            this.Receiver = new UdpClient(this.ReceivePort);
        }

        //void Init()
        //{
        //    //DontDestroyOnLoad(gameObject);
        //}
 
        void Start()
        {
            //this.Init();
            this.Receiver.BeginReceive(OnReceive, null);
        }

        void Update()
        {
            if (!isServer)
            {
                string msg = Console.ReadLine();
                int len_msg = msg.Length;
                buffer = ClientWritePacket(msg, len_msg);
                handlePacket.PrintByteArray(buffer);
                BroadcastMessage(buffer, ClientList, RefPoint);
            }
        }

        public static void Main(string[] args)
        {
           
            instance.Awake();
            instance.Start();
            instance.Update();
            while (true)
            {
               
            }

        }

        void OnReceive(IAsyncResult ar)
        {
           try
           {
                buffer = Receiver.EndReceive(ar, ref RefPoint);
                Console.WriteLine("End Receiving");
                handlePacket.Parse(buffer, RefPoint, instance);
                Receiver.BeginReceive(OnReceive, null);
           }
           catch (ArgumentException)
           {

           }
        }

        public void OnSend(IAsyncResult ar)
        {
            try
            {
                Sender.EndSend(ar);
                Console.WriteLine("End Sending");
                Update();
            }
            catch (ArgumentException)
            {

            }
        }

        public void RemoveClient(IPEndPoint ipep)
        {
            ClientList.Remove(ipep);
        }

        void OnApplicationQuit()
        {
            Sender.Close();
            Receiver.Close();
        }

        //void OnDisable()
        //{
        //    Sender.Close();
        //    Receiver.Close();
        //}

    }

    public class HandlePacket
    {
        public byte[] m_data;
        public IPEndPoint IPEP;
        int len_time = DateTime.Now.ToString().Length;
        List<byte> mergedStream = new List<byte>(1024);
        int count = 0;

        public struct Vector3
        {
            public float x;
            public float y;
            public float z;
        }

        public class Packet
        {
            public string Sof; //Start of frame
            public DateTime timeStamp;  //Time stamp
            public string time; //Time stamp to string
            public string payload;
            public int len_msg;

            public UInt32 id;  //unique object ID
            public byte category = new byte();  //category               
            public byte type = new byte();  //type                                          
            public UInt16 visMask;        //mask                                           
            public char name;   //symbolic name                                             
            public Vector3 geo = new Vector3();     // info about object's geometry                            
            public Vector3 pos = new Vector3();     //position and orientation of object's reference point        
            public UInt32 parent;     //ID of parent object                                   
            public UInt16 cfgFlags;   //configuration flags                                          
            public UInt16 cfgModelId;   //visual model ID (configuration parameter)  

            // constructors
            public Packet(string pl, int len_msg)
            {
                this.id = 53;
                this.category = 0x01;
                this.type = 0x20;
                this.visMask = 55;
                this.name = 'c';
                this.geo.x = 1.65F;
                this.geo.y = 2.32F;
                this.geo.z = 3.78F;
                this.pos.x = 4.666F;
                this.pos.y = 5.9F;
                this.pos.z = 6.3F;
                this.parent = 33;
                this.cfgFlags = 9;
                this.cfgModelId = 20;
                this.Sof = "00001111";
                this.timeStamp = DateTime.Now;
                this.len_msg = len_msg;
                this.payload = pl;
            }

            public Packet(byte[] DataStream, int len_time)
            {
                this.id = BitConverter.ToUInt32(DataStream, 0);
                this.category = DataStream[4];
                this.type = DataStream[5];
                this.visMask = BitConverter.ToUInt16(DataStream, 6);
                this.name = BitConverter.ToChar(DataStream, 8);
                this.geo.x = BitConverter.ToSingle(DataStream, 10);
                this.geo.y = BitConverter.ToSingle(DataStream, 14);
                this.geo.z = BitConverter.ToSingle(DataStream, 18);
                this.pos.x = BitConverter.ToSingle(DataStream, 22);
                this.pos.y = BitConverter.ToSingle(DataStream, 26);
                this.pos.z = BitConverter.ToSingle(DataStream, 30);
                this.parent = BitConverter.ToUInt32(DataStream, 34);
                this.cfgFlags = BitConverter.ToUInt16(DataStream, 38);
                this.cfgModelId = BitConverter.ToUInt16(DataStream, 40);
                this.Sof = Encoding.UTF8.GetString(DataStream, 42, 8); //fixed length
                this.time = Encoding.UTF8.GetString(DataStream, 50, len_time);
                this.len_msg = BitConverter.ToInt16(DataStream, 50 + len_time); // weired to occupy 4 bytes
                this.payload = Encoding.UTF8.GetString(DataStream, 54 + len_time, this.len_msg);
            }
        }

        public byte[] Serialize(Packet pkt)
        {
            List<byte> DataStream = new List<byte>();
            DataStream.AddRange(BitConverter.GetBytes(pkt.id));
            DataStream.Add(pkt.category);
            DataStream.Add(pkt.type);
            DataStream.AddRange(BitConverter.GetBytes(pkt.visMask));
            DataStream.AddRange(BitConverter.GetBytes(pkt.name));
            DataStream.AddRange(BitConverter.GetBytes(pkt.geo.x));
            DataStream.AddRange(BitConverter.GetBytes(pkt.geo.y));
            DataStream.AddRange(BitConverter.GetBytes(pkt.geo.z));
            DataStream.AddRange(BitConverter.GetBytes(pkt.pos.x));
            DataStream.AddRange(BitConverter.GetBytes(pkt.pos.y));
            DataStream.AddRange(BitConverter.GetBytes(pkt.pos.z));
            DataStream.AddRange(BitConverter.GetBytes(pkt.parent));
            DataStream.AddRange(BitConverter.GetBytes(pkt.cfgFlags));
            DataStream.AddRange(BitConverter.GetBytes(pkt.cfgModelId));
            DataStream.AddRange(Encoding.UTF8.GetBytes(pkt.Sof));
            DataStream.AddRange(Encoding.UTF8.GetBytes(pkt.timeStamp.ToString()));
            DataStream.AddRange(BitConverter.GetBytes(pkt.len_msg));
            DataStream.AddRange(Encoding.UTF8.GetBytes(pkt.payload));
            return DataStream.ToArray();
        }

        public void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("Packet sent in array: ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            Console.WriteLine(sb.ToString());
        }

        public void DisplayPacket(Packet pkt)
        {
            Console.WriteLine("Object id: " + pkt.id);
            Console.WriteLine("Object category: " + pkt.category);
            Console.WriteLine("Object type: " + pkt.type);
            Console.WriteLine("Visibility mask: " + pkt.visMask);
            Console.WriteLine("Symbolic name: " + pkt.name);
            Console.Write("Object's geometry: (" + pkt.geo.x);
            Console.Write(", " + pkt.geo.y);
            Console.WriteLine(", " + pkt.geo.z + ")");
            Console.Write("Object's position: (" + pkt.pos.x);
            Console.Write(", " + pkt.pos.y);
            Console.WriteLine(", " + pkt.pos.z + ")");
            Console.WriteLine("Parent object id: " + pkt.parent);
            Console.WriteLine("Configuration flags: " + pkt.cfgFlags);
            Console.WriteLine("Visual model ID: " + pkt.cfgModelId);
            Console.WriteLine("Start of message: " + pkt.Sof);
            Console.WriteLine("Sent time: " + pkt.time);
            Console.WriteLine("Chat message: " + pkt.payload);
        }

        public void MergeMessage(byte [] stream)
        {
            mergedStream.AddRange(stream);
            count++;
            Console.WriteLine("count is " + count.ToString());
        }

        public void Parse(byte[] stream, IPEndPoint ipep, MyNetwork instance)
        {
            IPEP = instance.Conversion(ipep);
            Packet pkt = new Packet(stream, len_time);
            DisplayPacket(pkt);

            if (instance.isServer)
            {
                instance.AddClient(IPEP);
                if (pkt.payload == "J")
                {
                    Console.WriteLine("Generate GameObject"); //Broadcast this message instantly
                }
                if (pkt.payload == "Q")
                {
                    instance.RemoveClient(IPEP);
                    Console.WriteLine("Remove GameObject"); //Broadcast this message instantly
                }
                instance.BroadcastMessage(stream, instance.ClientList, IPEP);
                
            }
        }


    }
        
}