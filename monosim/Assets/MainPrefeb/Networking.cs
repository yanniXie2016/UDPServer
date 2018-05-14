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
    public class Networking : MonoBehaviour
    {
        #region Parameters

        //parameters for networking
        public bool isServer;
        public bool isHost;
        public static string viresServer = "169.254.145.12";  // change it here
        public static string unityServer = "10.246.139.61";  // change it here
        private List<IPEndPoint> ClientList = new List<IPEndPoint>();

        public int s_SendPort;
        public int s_ReceivePort;
        public int c_SendPort;
        public int c_ReceivePort;  // check it here

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
        //private int[] lens = { 24, 16, 16, 208, 16, 104, 104, 104, 104, 16 };
        private int[] lens = { 24, 16, 16, 208, 16, 44, 44, 44, 44, 16 };
        private RingBuffer[] MsgQueue = new RingBuffer[10];
        private HandlePacket hp;
        private UInt32 m_Id;
        private System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
        public double simTime;
        private float travelDistance = 0f;
        private float radius;

        // parameters for vehicle behaviour
        public GameObject EgoVehicle;
        public GameObject Geometry;
        public GameObject[] PrefabList = new GameObject[2];
        public GameObject[] PlayerList;
        private GameObject[] GeoList;
        private Rigidbody[] RigidList;
        private List<UInt32> IdList;
        private Vector3[] CurrentVelocity;
        private Vector3[] CurrentPosition;
        private Vector3[] CurrentRotation;
        public int smoothFlag;
        private CarController carController;
        public bool dummyRigid;
        private GameObject myDummy;

        // parameters for statistical analysis based on tests
        private DateTime testTime1;
        private DateTime testTime2;
        private TimeSpan timeSpan;
        public int m0 = 0;   // used for calculating frame average time
        public int m1 = 0;
        public int m2 = 0;
        public int n = 0;    // count for packets which are out of order
        private System.Diagnostics.Stopwatch stopwatch2 = new System.Diagnostics.Stopwatch();

        #endregion

        #region DetailFunctions

        // related to networking

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
            Vector3 speed_a = new Vector3();
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            speed_c = rigidbody.velocity * 2.23693629f;
            speed_a = rigidbody.angularVelocity;  // This is in radians per second
            HandlePacket.coord speed = new HandlePacket.coord(speed_c.z, -speed_c.x, speed_c.y, -speed_a.y, speed_a.x, speed_a.z, 0x01 | 0x02, 0, 0);   // change it here
            return speed;
        }

        public float GetMyRotAngle()
        {
            travelDistance = carController.CurrentSpeed/ 2.23693629f * Time.deltaTime+travelDistance;
            float angle = travelDistance / radius  %(2 * Mathf.PI);
            return angle; 
        }

        public HandlePacket.wheel[] WheelExtensions(HandlePacket.wheel_o[] wheel_O)
        {
            HandlePacket.wheel_e[] wheel_E = new HandlePacket.wheel_e[4];
            HandlePacket.wheel[] wheel = new HandlePacket.wheel[4];
            for (int i = 0; i < 4; i++)
            {
                wheel_E[i] = new HandlePacket.wheel_e();
                wheel[i] = new HandlePacket.wheel(wheel_O[i], wheel_E[i]);
            }
            return wheel;
        }

        public byte[] ClientWritePacket(GameObject go, UInt32 count)
        {
            byte[] spare0 = new byte[2];
            UInt32[] spare1 = new UInt32[4];

            Vector3 pos_c = Geometry.transform.position;
            Vector3 pos_a = Geometry.transform.eulerAngles;
            HandlePacket.coord pos = new HandlePacket.coord(pos_c.z, -pos_c.x, pos_c.y, Convert.ToSingle(-pos_a.y*Math.PI/180f), Convert.ToSingle(pos_a.x * Math.PI / 180f), Convert.ToSingle(pos_a.z * Math.PI / 180f), 0x01 | 0x02, 0, 0);  // change it here
            HandlePacket.coord speed = GetMySpeed(go);
            HandlePacket.coord accel = new HandlePacket.coord(0, 0, 0, 0, 0, 0, 0x01, 0, 0);
            HandlePacket.geo geo = new HandlePacket.geo(4.6f, 1.86f, 1.6f, 0.8f, 0, 0.3f);
            
            HandlePacket.wheel_o[] wheel_O = new HandlePacket.wheel_o[4];
            float rotAngle = GetMyRotAngle();
            wheel_O[0] = new HandlePacket.wheel_o(1, 0, 0, spare0, 1, -0.025f, rotAngle / 180 * Mathf.PI, 0, -carController.CurrentSteerAngle / 180 * Mathf.PI, spare1);
            wheel_O[1] = new HandlePacket.wheel_o(1, 1, 0, spare0, 1, -0.025f, rotAngle / 180 * Mathf.PI, 0, -carController.CurrentSteerAngle / 180 * Mathf.PI, spare1);
            wheel_O[2] = new HandlePacket.wheel_o(1, 2, 0, spare0, 1, -0.025f, rotAngle / 180 * Mathf.PI, 0, 0, spare1);
            wheel_O[3] = new HandlePacket.wheel_o(1, 3, 0, spare0, 1, -0.025f, rotAngle / 180 * Mathf.PI, 0, 0, spare1);
     
            simTime = stopwatch1.Elapsed.TotalSeconds;
            buffer = HandlePacket.Catch.CatchPacket(simTime, m_Id, geo, pos, speed, accel, count, wheel_O);
            return buffer;
        }

        public HandlePacket.Packet ParsePacket(byte[] stream, int[] lens)
        {
            HandlePacket.Packet pkt = new HandlePacket.Packet();
            HandlePacket.Packet pkt_n = pkt.Parse(stream, lens);
            return pkt_n;
        }

        public UInt32 ParseID(HandlePacket.Packet pkt)
        {
            UInt32 theId = pkt.State.state_base.id;
            return theId;
        }

        public Vector3 ParseCoord(HandlePacket.Packet pkt, int flag1, bool flag2)
        {
            // flag1 is the option for "pos"(1) and "speed"(2), flag2 stands for "angular"(0) and "coordinates"(1)
            HandlePacket.coord pos = pkt.State.state_base.pos;
            HandlePacket.coord speed = pkt.State.state_ext.speed;
            Vector3 pos_c, pos_a, speed_c, speed_a = new Vector3();
            pos_c = new Vector3(-Convert.ToSingle(pos.y), Convert.ToSingle(pos.z), Convert.ToSingle(pos.x));
            pos_a = new Vector3(Convert.ToSingle(pos.p * 180f / Mathf.PI), Convert.ToSingle(-pos.h * 180f / Mathf.PI), Convert.ToSingle(pos.r * 180f / Mathf.PI));
            speed_c = new Vector3(-Convert.ToSingle(speed.y), Convert.ToSingle(speed.z), Convert.ToSingle(speed.x));
            speed_a = new Vector3(Convert.ToSingle(speed.p), Convert.ToSingle(-speed.h), Convert.ToSingle(speed.r));
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
            Vector3 target = CurrentPosition[i] + Time.deltaTime * CurrentVelocity[i];
            switch (smoothFlag)
            {
                case 0:
                    GeoList[i].transform.position = target;
                    break;
                case 1:
                    GeoList[i].transform.position = Vector3.SmoothDamp(CurrentPosition[i], target, ref CurrentVelocity[i], Time.deltaTime);
                    break;
                case 2:
                    GeoList[i].transform.position = Vector3.Lerp(CurrentPosition[i], target, Time.deltaTime * CurrentVelocity[i].magnitude);
                    break;
            }
            CurrentPosition[i] = GeoList[i].transform.position;
            Debug.Log("Predicted Position: " + CurrentPosition[i]);
            GeoList[i].transform.eulerAngles = CurrentRotation[i];
        }

        public void GenerateObject(HandlePacket.Packet pkt)
        {
            UInt32 id = ParseID(pkt);
            Vector3 SpawnLocation = ParseCoord(pkt, 1, true);
            Vector3 SpawnRotation = ParseCoord(pkt, 1, false);
            PlayerList[id] = Instantiate(myDummy, SpawnLocation, Quaternion.Euler(SpawnRotation.x, SpawnRotation.y, SpawnRotation.z)) as GameObject;
            CurrentPosition[id] = SpawnLocation;
            GeoList[id] = PlayerList[id].transform.GetChild(4).gameObject;
            if(dummyRigid)
            {
                RigidList[id] = PlayerList[id].GetComponent<Rigidbody>();
            }
        }

        #endregion

        #region ImportantFunctions

        void Init()
        {
            stopwatch1.Start();           
            hp = GetComponent<HandlePacket>();
            carController = EgoVehicle.GetComponent<CarController>();
            PlayerList = new GameObject[10];
            GeoList = new GameObject[10];
            if(dummyRigid)
            {
                RigidList = new Rigidbody[10];
                myDummy = PrefabList[0];
            }
            else
            {
                myDummy = PrefabList[1];
            }
            CurrentPosition = new Vector3[10];
            CurrentRotation = new Vector3[10];
            CurrentVelocity = new Vector3[10];
            IdList = new List<UInt32>();
            m_Id = hp.m_ID;
            IdList.Add(m_Id);
            for (int i = 0; i < 10; i++)
            {
                MsgQueue[i] = new RingBuffer(100);
                CurrentPosition[i] = new Vector3(0, 0, 0);
                CurrentRotation[i] = new Vector3(0, 0, 0);
                CurrentVelocity[i] = new Vector3(0, 0, 0);
            }
            GameObject wheelhub = EgoVehicle.transform.GetChild(2).gameObject;
            GameObject wheel1 = wheelhub.transform.GetChild(1).gameObject;
            radius = wheel1.GetComponent<WheelCollider>().radius;
        }

        void Awake()
        {
            Init();
            if(isHost)
            {
                this.SendPort = s_SendPort;
                this.ReceivePort = s_ReceivePort;
                Debug.Log("This is a host");
                this.Objective = new IPEndPoint(IPAddress.Any, c_ReceivePort);
                this.RefPoint = new IPEndPoint(IPAddress.Any, c_SendPort);
            }

            if (isServer)
            {
                this.ReceivePort = s_ReceivePort;
                this.SendPort = s_SendPort;
                Debug.Log("Server started, servicing on: " + unityServer + ": "+this.ReceivePort);
                this.Objective = new IPEndPoint(IPAddress.Any, c_ReceivePort);
                this.RefPoint = new IPEndPoint(IPAddress.Any, c_SendPort);
                //IPEndPoint vires = new IPEndPoint(IPAddress.Parse(viresServer), c_ReceivePort);
                //this.ClientList.Add(vires);
            }
            else
            {
                this.ReceivePort = c_ReceivePort;
                this.SendPort = c_SendPort;
                Debug.Log("This is a client");
                this.Objective = new IPEndPoint(IPAddress.Parse(unityServer), s_ReceivePort);
                this.RefPoint = new IPEndPoint(IPAddress.Parse(unityServer), s_SendPort);
                this.ClientList.Add(this.Objective);
            }
            this.Sender = new UdpClient(this.SendPort);
            this.Receiver = new UdpClient(this.ReceivePort);
            // Receiving Message
            this.StartToReceive();
            Debug.Log("Start");
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

            if ((!isServer) || (isHost))
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

        public void StartToReceive()
        {
            this.Receiver.BeginReceive(OnReceive, null);
            Debug.Log("In Start");
        }

        public void OnReceive(IAsyncResult ar)
        {
            try
            {
                Debug.Log("In");
                buffer = Receiver.EndReceive(ar, ref RefPoint);
                Debug.Log("End Receiving from: " +RefPoint.ToString());
                
                if (isServer|| isHost)
                {
                    // Add clients
                    IPEP = Conversion(RefPoint);
                    AddClient(IPEP);
                    BroadcastMessage(buffer, ClientList);
                }

                HandlePacket.Packet pkt_n = ParsePacket(buffer, lens);
                UInt32 ID = ParseID(pkt_n);
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
            UInt32 id = ParseID(pkt);
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
                Vector3 vel_c = ParseCoord(pkt, 2, true);               
                CurrentVelocity[id] = vel_c/ 2.23693629f;

                Vector3 vel_a = ParseCoord(pkt, 2, false);
                
                if(dummyRigid)
                {
                    RigidList[id].velocity = vel_c/ 2.23693629f;
                    RigidList[id].angularVelocity = vel_a;
                }

                Vector3 pos_c = ParseCoord(pkt, 1, true);
                switch (smoothFlag)
                {
                    case 0:
                        GeoList[id].transform.position = pos_c;
                        break;
                    case 1:
                        GeoList[id].transform.position = Vector3.SmoothDamp(CurrentPosition[id], pos_c, ref vel_c, Time.deltaTime);
                        break;
                    case 2:
                        GeoList[id].transform.position = Vector3.Lerp(CurrentPosition[id], pos_c, Time.deltaTime * vel_c.magnitude);
                        break;
                }
                CurrentPosition[id] = GeoList[id].transform.position;
                Debug.Log("Get Position: " + CurrentPosition[id]);

                Vector3 pos_a = ParseCoord(pkt, 1, false);
                GeoList[id].transform.eulerAngles = pos_a;
                CurrentRotation[id] = GeoList[id].transform.eulerAngles;

               
                
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

