using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using myApp;
using UnityStandardAssets.Vehicles.Car;


namespace myApp
{
    [RequireComponent(typeof(HandlePacket))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CarController))]
    public class Networking : MonoBehaviour
    {
        #region Parameters

        //parameters for networking
        public bool isServer = true;
        public static string serverAddress = "169.254.145.12";  // change it here
        private List<IPEndPoint> ClientList = new List<IPEndPoint>();

        public int s_SendPort = 48190;
        public int s_ReceivePort = 48191;
        public int c_SendPort = 48191;
        public int c_ReceivePort = 48190;       // check it here

        private int ReceivePort;
        private int SendPort;
        private IPEndPoint IPEP;

        private UdpClient Sender;
        private IPEndPoint Objective;
        private UdpClient Receiver;
        private IPEndPoint RefPoint;

        // parameters for writing packets
        private byte[] buffer = new byte[50];
        public UInt32 counter = 1;
        private UInt32 maxNo = 0;
        private int[] lens = { 24, 16, 16, 208, 16, 104, 104, 104, 104, 16 };
        private RingBuffer[] MsgQueue = new RingBuffer[10];
        private HandlePacket hp;
        private UInt32 m_Id;

        // parameters for vehicle behaviour
        public GameObject EgoVehicle;
        public GameObject[] PrefabList = new GameObject[1];
        public GameObject[] PlayerList;
        private List<UInt32> IdList;
        private Vector3 CurrentVelocity = new Vector3(0, 0, 0);
        private Vector3 CurrentPosition = new Vector3(0, 0, 0);
        private Quaternion CurrentRotation = new Quaternion();
        public int smoothFlag;

        // parameters for statistical analysis based on tests
        private DateTime testTime1;
        private DateTime testTime2;
        private TimeSpan timeSpan;
        public int m0 = 0;   // used for calculating frame average time
        public int m1 = 0;
        public int m2 = 0;
        public int n = 0;    // count for packets which are out of order

        #endregion

        #region DetailFunctions

        // related to networking
        public void StartToReceive()
        {
            this.Receiver.BeginReceive(OnReceive, null);
        }

        public IPEndPoint Conversion(IPEndPoint ipep)
        {
            if (isServer)
            {
                IPEP = new IPEndPoint(ipep.Address, c_ReceivePort);
            }
            else
            {
                IPEP = new IPEndPoint(ipep.Address, s_ReceivePort);
            }
            return (IPEP);
        }

        public void AddClient(IPEndPoint IPEP)
        {
            if (ClientList.Contains(IPEP) == false)
            {
                ClientList.Add(IPEP);
                Debug.Log("Newly registered" + IPEP.ToString());
            }
        }

        public void RemoveClient(IPEndPoint ipep)
        {
            ClientList.Remove(ipep);
        }

        public void BroadcastMessage(byte[] msg, List<IPEndPoint> list)
        {
            foreach (var ip in ClientList)
            {
                Sender.Client.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, ip, OnSend, null);
                Debug.Log("Message has been sent to: " + ip.ToString());
            }
        }

        // related to communication packets

        public HandlePacket.coord GetMySpeed(GameObject gameObject)
        {
            Vector3 speed_c = new Vector3();
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            speed_c.x = Convert.ToSingle(rigidbody.velocity.z * 2.23693629f * Math.PI / 180f);
            speed_c.y = -Convert.ToSingle(rigidbody.velocity.x * 2.23693629f * Math.PI / 180f);
            speed_c.z = Convert.ToSingle(rigidbody.velocity.y * 2.23693629f * Math.PI / 180f);
            Vector3 speed_a = rigidbody.rotation.eulerAngles;
            HandlePacket.coord speed = new HandlePacket.coord(speed_c.x, speed_c.y, speed_c.z, speed_a.x, speed_a.y, speed_a.z, 0x01 | 0x02, 0, 0);
            return speed;
        }

        public byte[] ClientWritePacket(GameObject go, UInt32 count)
        {
            byte[] spare0 = new byte[2];
            UInt32[] spare1 = new UInt32[4];
            UInt32[] spare2 = new UInt32[4];
            float[] xyz = new float[3];

            Vector3 pos_c = go.transform.position;
            Vector3 pos_a = go.transform.localEulerAngles;
            HandlePacket.coord pos = new HandlePacket.coord(pos_c.z, -pos_c.x, pos_c.y, Convert.ToSingle(pos_a.z*Math.PI/180f), -Convert.ToSingle(pos_a.x * Math.PI / 180f), Convert.ToSingle(pos_a.y * Math.PI / 180f), 0x01 | 0x02, 0, 0);
            HandlePacket.coord speed = GetMySpeed(go);
            HandlePacket.coord accel = new HandlePacket.coord(0, 0, 0, 0, 0, 0, 0x01, 0, 0);
            HandlePacket.geo geo = new HandlePacket.geo(4.6f, 1.86f, 1.6f, 0.8f, 0, 0.3f);


            HandlePacket.wheel_o[] wheel_O = new HandlePacket.wheel_o[4];
            wheel_O[0] = new HandlePacket.wheel_o(1, 0, 0, spare0, 1, -0.025f, 0, 0, 0.5f, spare1);
            wheel_O[1] = new HandlePacket.wheel_o(1, 1, 0, spare0, 1, -0.025f, 0, 0, 0, spare1);
            wheel_O[2] = new HandlePacket.wheel_o(1, 2, 0, spare0, 1, -0.025f, 0, 0, 0, spare1);
            wheel_O[3] = new HandlePacket.wheel_o(1, 3, 0, spare0, 1, -0.025f, 0, 0, 0, spare1);
            HandlePacket.wheel_e[] wheel_E = new HandlePacket.wheel_e[4];
            HandlePacket.wheel[] wheel = new HandlePacket.wheel[4];
            for (int i = 0; i < 4; i++)
            {
                wheel_E[i] = new HandlePacket.wheel_e(55, 2, 3, 4, xyz, 5, 6, 7, 8, spare2);
                wheel[i] = new HandlePacket.wheel(wheel_O[i], wheel_E[i]);
            }

            buffer = HandlePacket.Catch.CatchPacket(m_Id, geo, pos, speed, accel, count, wheel);
            return buffer;
        }

        public HandlePacket.Packet ParsePacket(byte[] stream, int[] lens)
        {
            HandlePacket.Packet pkt = new HandlePacket.Packet();
            HandlePacket.Packet pkt_n = pkt.Parse(stream, lens);
            return pkt_n;
        }

        public DateTime GetTime(HandlePacket.Packet pkt)
        {
            double theTime = pkt.C.simTime;
            DateTime myTime = DateTime.FromOADate(theTime);
            return myTime;
        }

        public UInt32 GetID(HandlePacket.Packet pkt)
        {
            UInt32 theId = pkt.State.state_base.id;
            return theId;
        }

        public Vector3 GetCoord(HandlePacket.Packet pkt, int flag1, bool flag2)
        {
            // flag1 is the option for "pos"(1) and "speed"(2), flag2 stands for "angular"(0) and "coordinates"(1)
            HandlePacket.coord pos = pkt.State.state_base.pos;
            Vector3 pos_c = new Vector3(-Convert.ToSingle(pos.y), Convert.ToSingle(pos.z), Convert.ToSingle(pos.x));
            Vector3 pos_a = new Vector3(-Convert.ToSingle(pos.p), Convert.ToSingle(pos.r), Convert.ToSingle(pos.h));
            HandlePacket.coord speed = pkt.State.state_ext.speed;
            Vector3 speed_c = new Vector3(-Convert.ToSingle(speed.y), Convert.ToSingle(speed.z), Convert.ToSingle(speed.x));
            Vector3 speed_a = new Vector3(-Convert.ToSingle(speed.p), Convert.ToSingle(speed.r), Convert.ToSingle(speed.h));
            if (flag1 == 1)
            {
                if (flag2)
                {
                    return pos_c;
                }
                else
                {
                    return pos_a;
                }
            }
            else if (flag1 == 2)
            {
                if (flag2)
                {
                    return speed_c;
                }
                else
                {
                    return speed_a;
                }
            }
            else
            {
                Debug.Log("No such a flag");
                return new Vector3(0, 0, 0);
            }
        }

        // related to vehicle behaviour

        public void Extrapolation(UInt32 i)
        {
            Vector3 target = CurrentPosition + Time.deltaTime * CurrentVelocity;
            switch(smoothFlag)
            {
                case 0:
                    PlayerList[i].transform.position = target;
                    break;
                case 1:
                    PlayerList[i].transform.position = Vector3.SmoothDamp(CurrentPosition, target, ref CurrentVelocity, Time.deltaTime);
                    break;
                case 2:
                    PlayerList[i].transform.position = Vector3.Lerp(CurrentPosition, target, Time.deltaTime * CurrentVelocity.magnitude);
                    break;
            }
            CurrentPosition = PlayerList[i].transform.position;
            Debug.Log("Predicted Position: " + PlayerList[i].transform.position);
            PlayerList[i].transform.localRotation = CurrentRotation;
        }

        public void GenerateObject(HandlePacket.Packet pkt)
        {
            UInt32 id = GetID(pkt);
            Vector3 SpawnLocation = GetCoord(pkt, 1, true);
            Vector3 SpawnRotation = GetCoord(pkt, 1, false);
            PlayerList[id] = Instantiate(PrefabList[0], SpawnLocation, Quaternion.Euler(SpawnRotation.x, SpawnRotation.y, SpawnRotation.z)) as GameObject;
            CurrentPosition = SpawnLocation;
        }

        #endregion

        #region ImportantFunctions

        void Init()
        {
            hp = GetComponent<HandlePacket>();
            PlayerList = new GameObject[10];
            IdList = new List<UInt32>();
            m_Id = hp.m_ID;
            IdList.Add(m_Id);
            for (int i = 0; i < 10; i++)
            {
                MsgQueue[i] = new RingBuffer(100);
            }
        }

        void Awake()
        {
            Init();
            if (isServer)
            {
                this.ReceivePort = s_ReceivePort;
                this.SendPort = s_SendPort;
                Debug.Log("Server started, servicing on port {0}:" + serverAddress + this.ReceivePort);
                this.Objective = new IPEndPoint(IPAddress.Any, c_ReceivePort);
                this.RefPoint = new IPEndPoint(IPAddress.Any, c_SendPort);
            }
            else
            {
                this.ReceivePort = c_ReceivePort;
                this.SendPort = c_SendPort;
                Debug.Log("This is a client");
                this.Objective = new IPEndPoint(IPAddress.Parse(serverAddress), s_ReceivePort);
                this.RefPoint = new IPEndPoint(IPAddress.Parse(serverAddress), s_SendPort);
                this.ClientList.Add(this.Objective);
            }
            this.Sender = new UdpClient(this.SendPort);
            this.Receiver = new UdpClient(this.ReceivePort);
            // Receiving Message
            this.StartToReceive();
        }

        void Update()
        {

            if (Time.deltaTime < 0.014)
            {
                m0++;
            }
            else if (Time.deltaTime < 0.022)
            {
                m1++;
            }
            else if (Time.deltaTime > 0.03)
            {
                m2++;
            }

            for (UInt32 i = 1; i < 10; i++)
            {
                if ((!IdList.Contains(i)) || i == m_Id)
                {
                    continue;
                }
                else
                {
                    if (MsgQueue[i].queue.Count == 0)
                    {
                        Debug.Log("Using Extrapolation");
                        if (PlayerList[i] == null)
                        {
                            Debug.Log("null");
                        }
                        else
                        {
                            Extrapolation(i);
                        }
                    }
                    else
                    {
                        Debug.Log("Using Received Packets");
                        while (MsgQueue[i].queue.Count != 0)
                        {
                            HandlePacket.Packet reader = MsgQueue[i].Read();
                            if (reader != null)
                            {
                                MySyncVar(reader);
                            }
                        }
                    }
                }
            }

            if (!isServer)
            {
                buffer = ClientWritePacket(EgoVehicle, counter);
                BroadcastMessage(buffer, ClientList);
                counter++;
            }

        }

        public void OnApplicationQuit()
        {
            Sender.Close();
            Receiver.Close();
        }

        public void OnDisable()
        {
            Sender.Close();
            Receiver.Close();
        }

        public void OnReceive(IAsyncResult ar)
        {
            try
            {
                buffer = Receiver.EndReceive(ar, ref RefPoint);
                Debug.Log("End Receiving from: " +RefPoint.ToString());
                testTime1 = DateTime.Now;

                
                if (isServer)
                {
                    // Add clients
                    IPEP = Conversion(RefPoint);
                    AddClient(IPEP);
                    BroadcastMessage(buffer, ClientList);
                }

                HandlePacket.Packet pkt_n = ParsePacket(buffer, lens);
                UInt32 ID = GetID(pkt_n);
                if (!IdList.Contains(ID))
                {
                    IdList.Add(ID);
                    Debug.Log("New Object");
                }
                MsgQueue[ID].Add(pkt_n);

                // Test Latency on the same laptop
                //if(id == m_Id)
                //{
                //    DateTime time = GetTime(pkt_n);
                //    TimeSpan interval = testTime - time;
                //    Debug.Log("Latency is " + String.Format("{0}.{1}", interval.Seconds, interval.Milliseconds.ToString().PadLeft(3, '0')) + "s");
                //}

                // Start to receive again
                StartToReceive();
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
                //Debug.Log("End Sending");
            }
            catch (ArgumentException)
            {

            }
        }

        public void MySyncVar(HandlePacket.Packet pkt)
        {
            UInt32 id = GetID(pkt);
            if (PlayerList[id] == null)
            {
                GenerateObject(pkt);
            }
            UInt32 _no = pkt.C.frameNo;
           
            if(_no<maxNo)
            {
                n++;
                Extrapolation(id);
            }
            else
            {
                maxNo = _no;
                Vector3 pos_c = GetCoord(pkt, 1, true);
                Vector3 vel_c = GetCoord(pkt, 2, true);
                switch (smoothFlag)
                {
                    case 0:
                        PlayerList[id].transform.position = pos_c;
                        break;
                    case 1:
                        PlayerList[id].transform.position = Vector3.SmoothDamp(CurrentPosition, pos_c, ref vel_c, Time.deltaTime);
                        break;
                    case 2:
                        PlayerList[id].transform.position = Vector3.Lerp(CurrentPosition, pos_c, Time.deltaTime * vel_c.magnitude);
                        break;
                } 
                CurrentPosition = PlayerList[id].transform.position;
                CurrentVelocity = vel_c;
                Debug.Log(PlayerList[id].transform.position);
                Vector3 pos_a = GetCoord(pkt, 1, false);
                PlayerList[id].transform.localRotation = Quaternion.Euler(pos_a.x, pos_a.y, pos_a.z);
                CurrentRotation = PlayerList[id].transform.localRotation;
            }

        }

        #endregion

    }

    public class RingBuffer
    {
        public Queue<HandlePacket.Packet> queue;
        public int size;

        #region Constructor
        public RingBuffer(int size)
        {
            this.queue = new Queue<HandlePacket.Packet>(size);
            this.size = size;
        }
        #endregion

        #region API

        public void Add(HandlePacket.Packet newItem)
        {
            if (queue.Count == this.size)
            {
                queue.Dequeue();
            }
            queue.Enqueue(newItem);
        }

        public HandlePacket.Packet Read()
        {
            if (queue.Count != 0)
            {
                return queue.Dequeue();
            }
            else
            {
                return null;
            }
        }

        public HandlePacket.Packet Peek()
        {
            return queue.Peek();
        }

        #endregion
    }
}

