using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

namespace UDPChat
{
    [RequireComponent(typeof(Serialization))]
    [RequireComponent(typeof(CarController))]
    [RequireComponent(typeof(Spawner))]
    public class UDPClient : MonoBehaviour
    {
        #region field && state
        private Serialization m_serialization;
        private CarController m_Car;
        private Spawner m_guest;
        public bool wired, wireless;
        public string wireId, wirelessId;
        private string ServerId;
        public int s_Inport, s_Outport, c_Inport, c_Outport;   // Port for ingoing and outgoing
        public static IPEndPoint ServerIpEndpointIn, ServerIpEndpointOut;
        public static UdpClient clientOut = null, clientIn = null;
        public static List<UInt32> guests = new List<UInt32>();
        private byte[] SendData = null, receiveBytes = null, data_serialized = null;
        private static Serialization[] data_deserialized = new Serialization[5];
        public char[] ClientName = new char[32];
        public UInt32 counter = 0;
        public GameObject vehicle;
        public double SimTime = 0;
        public static GameObject[] players = new GameObject[10]; // the max number of player
        private static Vector3[] CurrentPos = new Vector3[10];
        private static Vector3[] LatestPos = new Vector3[10];
        private static Vector3[] NewVel = new Vector3[10];
        private static Vector3[] OldVel = new Vector3[10];
        private static Vector3 Pos, Vel, localangularvelocity, targetPos;
        private static Vector3 diff = Vector3.zero;
        bool flag = false;
        private int j = 0; //the number of current other players
        UInt32[] LastFrame = new UInt32[10];
        private Rigidbody rb;
        public int type;
        public int smooth;
        public float rotAngle = 0.0f, radius;
        public UInt32 ID_UnityPlayer1, ID_UnityPlayer2,id;
        UInt32[] spare = new UInt32[3] { 0, 0, 0 };
        byte[] spare0 = new byte[2] { 0, 0 };
        UInt32[] spare1 = new UInt32[4] { 0, 0, 0, 0 };
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch(); // SimTime
        System.Diagnostics.Stopwatch stopWatch1 = new System.Diagnostics.Stopwatch(); // Calculate the time scape
        #endregion
        void Awake()
        {
            stopWatch.Start(); // time counter
            m_serialization = GetComponent<Serialization>();
            m_Car = GetComponent<CarController>();// get the car controller
            m_guest = GetComponent<Spawner>();
        }

        void Start()
        {
            id = ID_UnityPlayer1;
            if (wired)  ServerId = wireId;
            if (wireless)  ServerId = wirelessId;
            clientOut = new UdpClient(c_Outport); // client send 
            ServerIpEndpointIn = new IPEndPoint(IPAddress.Parse(ServerId), s_Inport);
            clientOut.Connect(ServerIpEndpointIn);

            clientIn = new UdpClient(c_Inport); // client receive
            ServerIpEndpointOut = new IPEndPoint(IPAddress.Parse(ServerId), s_Outport);
            clientIn.BeginReceive(new AsyncCallback(OnReceive), null); // begin receive data
        }

        void FixedUpdate()
        {
            stopWatch1.Reset();
            stopWatch1.Start();
            OnSend(SendData); // client send   
            if (flag)  Spawning();  // if recieved massage 
        }

        public void OnSend(byte[] SendData)
        {
            SendData = StateMassage(); // write data
            clientOut.Send(SendData, SendData.Length);
            Debug.Log("Sent to:" + ServerIpEndpointIn.ToString());
        }

        public void OnReceive(IAsyncResult res)
        {
            receiveBytes = clientIn.EndReceive(res, ref ServerIpEndpointOut);
            stopWatch1.Stop();
            Debug.Log("End received from:" + ServerIpEndpointOut +",Length :" + receiveBytes.Length);
            data_deserialized = m_serialization.Deserialize(receiveBytes); // deserialization
            flag = true;
            //DateTime sendtime = DateTime.FromOADate(data_deserialized.msg_hdr.simTime);
            //time = DateTime.Now - sendtime; // calculate the round time trip
            //Debug.Log("Ping: " + String.Format("{0}.{1}", time.Seconds, time.Milliseconds.ToString().PadLeft(3, '0')) + "s");
            clientIn.BeginReceive(new AsyncCallback(OnReceive), null);
        }

        public void Spawning()
        {
            for (int n = 0; n < data_deserialized.GetLength(0); n++)
            {
                UInt32 id = data_deserialized[n].object_state.Base.id;
                if (m_serialization.flag_vires == true)
                {
                    if (id == ID_UnityPlayer1 || id == ID_UnityPlayer2) // ID for unity vechiles
                       continue;
                }
                if (id == 0) break;

                if (!guests.Contains(id))
                {
                    guests.Add(id); // add new player
                    Debug.Log("Add new player with Id:" + id);
                    RDB_COORD_t p = data_deserialized[n].object_state.Base.pos;
                    m_guest.SpawnLocation = new Vector3(Convert.ToSingle(-p.y), Convert.ToSingle(p.z), Convert.ToSingle(p.x));
                    Debug.Log("position:" + m_guest.SpawnLocation);
                    players[j] = m_guest.SpawnVechile(type);
                    float factor = 180 / Mathf.PI;
                    players[j].transform.rotation = Quaternion.Euler(new Vector3(p.p * factor, -p.h * factor, p.r * factor)); ;
                    j += 1;
                }
                else
                {
                    for (int i = 0; i < guests.Count; i++)
                    {
                        LatestPos[i] = players[i].transform.position;
                        if (guests[i].Equals(id))
                        {
                            if (LastFrame[i] < data_deserialized[n].msg_hdr.frameNo)
                            {
                                Debug.Log("The frame number is right in order");
                                UpdateState(i, n);
                            }
                            else if (LastFrame[i] == data_deserialized[n].msg_hdr.frameNo)
                            {
                                Debug.Log("There is no message recieved in this frame");
                                PredictPosition(i);
                            }
                            else if (LastFrame[i] > data_deserialized[n].msg_hdr.frameNo)
                            {
                                Debug.Log("The frame number is out of order");
                            }
                        }
                        break;
                    }
                }
            }
        }

        public void UpdateState(int i, int n)
        {
            RDB_COORD_t pos = data_deserialized[n].object_state.Base.pos;
            RDB_COORD_t v = data_deserialized[n].object_state.ext.speed;
            CurrentPos[i] = new Vector3(Convert.ToSingle(-pos.y), Convert.ToSingle(pos.z), Convert.ToSingle(pos.x));

            NewVel[i] = new Vector3(Convert.ToSingle(-v.y), Convert.ToSingle(v.z), Convert.ToSingle(v.x));
            diff = CurrentPos[i] - LatestPos[i];
            float factor = 180 / Mathf.PI;
            players[i].transform.rotation = Quaternion.Euler(new Vector3(pos.p * factor, -pos.h * factor, -pos.r * factor));
            players[i].transform.position = CurrentPos[i];
            //Debug.Log("Elapsed:" + (stopWatch1.Elapsed.Milliseconds)/1000f);
            //targetPos = CurrentPos[i] + NewVel[i] * (Time.fixedDeltaTime-(stopWatch1.Elapsed.Milliseconds)/1000);
            //players[i].transform.position = Vector3.Lerp(CurrentPos[i], targetPos, Time.fixedDeltaTime * 5f);
            //Debug.Log("True position:" + CurrentPos[i] + "frameNo:" + data_deserialized[n].msg_hdr.frameNo);
            LastFrame[i] = data_deserialized[n].msg_hdr.frameNo;
            // Update wheel steering angle
            GameObject FL_Hub = players[i].transform.GetChild(1).GetChild(0).gameObject;
            GameObject FR_Hub = players[i].transform.GetChild(1).GetChild(1).gameObject;
            GameObject FL = players[i].gameObject.transform.GetChild(5).GetChild(0).gameObject;
            GameObject FR = players[i].gameObject.transform.GetChild(5).GetChild(1).gameObject;

            float y = data_deserialized[n].wheel[0].Base.steeringAngle * factor;
            FL.transform.localRotation = Quaternion.Euler(new Vector3(0, -y, 0));
            FR.transform.localRotation = Quaternion.Euler(new Vector3(0, -y, 0));
            WheelCollider FL_collider = FL_Hub.GetComponent<WheelCollider>();
            WheelCollider FR_collider = FL_Hub.GetComponent<WheelCollider>();
            FL_collider.steerAngle = -y;
            FR_collider.steerAngle = -y;
        }

        public void PredictPosition(int i)
        {
            switch (smooth)
            {
                case 0:
                    players[i].transform.position = LatestPos[i] + diff;
                    break;
                case 1:
                    targetPos = players[i].transform.position + diff;
                    players[i].transform.position = Vector3.Lerp(LatestPos[i], targetPos, NewVel[i].magnitude * Time.fixedDeltaTime);
                    break;
                case 2:
                    players[i].transform.position = LatestPos[i] + NewVel[i] * Time.fixedDeltaTime;
                    break;
                case 3:
                    targetPos = LatestPos[i] + NewVel[i] * Time.fixedDeltaTime;
                    players[i].transform.position = Vector3.Lerp(LatestPos[i], targetPos, NewVel[i].magnitude * Time.fixedDeltaTime);
                    break;
                case 4:
                    targetPos = LatestPos[i] + NewVel[i] * Time.fixedDeltaTime;
                    players[i].transform.position = Vector3.SmoothDamp(targetPos, LatestPos[i], ref NewVel[i], Time.fixedDeltaTime * 5f);
                    break;
            }
            //Debug.Log("Predict position:" + players[i].transform.position + "frameNo:" + (LastFrame[i] + 1));
        }

        public byte[] StateMassage()
        {
            Pos = transform.position;
            Vel = m_Car.Speed;
            Transform wheelsinfo = vehicle.gameObject.transform.GetChild(1).GetChild(0);
            radius = wheelsinfo.GetComponent<WheelCollider>().radius;
            rotAngle = (rotAngle + Time.fixedDeltaTime * m_Car.CurrentSpeed / 2.23693629f / radius) % (2 * Mathf.PI);
            SimTime = stopWatch.Elapsed.TotalSeconds;
            data_serialized = PacketizeMessage();
            return data_serialized;
        }

        public byte[] PacketizeMessage()
        {
            m_serialization.msg_hdr = new RDB_MSG_HDR_t(35712, 0x011e, 24, 448, ++counter, SimTime);
            m_serialization.start_of_frame = new RDB_MSG_ENTRY_HDR_t(16, 0, 0, 1, 0);
            m_serialization.msg_entry_hdr_object_state = new RDB_MSG_ENTRY_HDR_t(16, 208, 208, 9, 0x01);
            m_serialization.msg_entry_hdr_wheel = new RDB_MSG_ENTRY_HDR_t(16, 176, 44, 14, 0);
            m_serialization.end_of_frame = new RDB_MSG_ENTRY_HDR_t(16, 0, 0, 2, 0);

            RDB_GEOMETRY_t geo = new RDB_GEOMETRY_t(4.6f, 1.86f, 1.6f, 0.8f, 0.0f, 0.3f);
            RDB_COORD_t pos = new RDB_COORD_t(Pos.z, -Pos.x, Pos.y, -transform.eulerAngles.y / 180f * Mathf.PI, transform.eulerAngles.x / 180f * Mathf.PI, -transform.eulerAngles.z / 180f * Mathf.PI, 0x01 | 0x02, 0, 0);
            RDB_COORD_t speed = new RDB_COORD_t(Vel.z, -Vel.x, Vel.y, -localangularvelocity.y, localangularvelocity.x, -localangularvelocity.z, 0x01 | 0x02, 0, 0);

            RDB_OBJECT_STATE_BASE_t object_base = new RDB_OBJECT_STATE_BASE_t(id, 1, 1, 0x06, ClientName, geo, pos, 0, 0, 0);
            RDB_COORD_t accel = new RDB_COORD_t(0.0D, 0.0D, 0.0D, 0.0f, 0.0f, 0.0f, 0x01, 0, 0);
            RDB_OBJECT_STATE_EXT_t object_ext = new RDB_OBJECT_STATE_EXT_t(speed, accel, 1.0f, spare);
            m_serialization.object_state = new RDB_OBJECT_STATE_t(object_base, object_ext);

            RDB_WHEEL_BASE_t wheel_FL_base = new RDB_WHEEL_BASE_t(id, 0, 0, spare0, 1.0f, -0.025f, rotAngle / 180 * Mathf.PI, 0.0f, -m_Car.CurrentSteerAngle / 180 * Mathf.PI, spare1);
            m_serialization.wheel[0] = new RDB_WHEEL_t(wheel_FL_base);

            RDB_WHEEL_BASE_t wheel_FR_base = new RDB_WHEEL_BASE_t(id, 1, 0, spare0, 1.0f, -0.025f, rotAngle / 180 * Mathf.PI, 0.0f, -m_Car.CurrentSteerAngle / 180 * Mathf.PI, spare1);
            m_serialization.wheel[1] = new RDB_WHEEL_t(wheel_FR_base);

            RDB_WHEEL_BASE_t wheel_RR_base = new RDB_WHEEL_BASE_t(id, 2, 0, spare0, 1.0f, -0.025f, rotAngle / 180 * Mathf.PI, 0.0f, 0.0f, spare1);
            m_serialization.wheel[2] = new RDB_WHEEL_t(wheel_RR_base);

            RDB_WHEEL_BASE_t wheel_RL_base = new RDB_WHEEL_BASE_t(id, 3, 0, spare0, 1.0f, -0.025f, rotAngle / 180 * Mathf.PI, 0.0f, 0.0f, spare1);
            m_serialization.wheel[3] = new RDB_WHEEL_t(wheel_RL_base);

            data_serialized = m_serialization.Serialize();
            return data_serialized;
        }

        public void OnApplicationQuit()
        {
            clientIn.Close();
            clientOut.Close();
            stopWatch.Stop();
        }

        public void OnDisable()
        {
            clientIn.Close();
            clientOut.Close();
        }
    }
}