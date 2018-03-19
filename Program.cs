using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;


namespace UdpChat
{
    public class Client
    {
        #region field && state
        public static string ServerIp = "10.246.130.191"; // server Ip
        ////public static string ServerIp = "10.246.53.11"; // server Ip (Ethernet)
        public static string MyIp = "10.246.130.191"; // Client Ip
        //public static string ServerIp = "192.168.2.101"; // server Ip
        //public static string MyIp = "192.168.2.101"; // Client Ip
        public static int s_Inport = 48190; // Port for ingoing 
        public static int s_Outport = 48191; // Port for outgoing 
        public static int c_Inport = 48192; // Port for ingoing 
        public static int c_Outport = 48193; // Port for outgoing
        public static IPEndPoint ServerIpEndpoint = null;
        public static IPEndPoint ServerIpEndpointOut = null;
        public static UdpClient clientOut = null;
        public static UdpClient clientIn = null;
        public static Serialization SendData = null;
        public static byte[] data_serialized = null;
        public static byte[] receiveBytes = null;
        public static string MessageType = null;
        public static Serialization instance = new Serialization();// creat a instance    
        #endregion

        static void Main()
        {
            try
            {
                Console.WriteLine("Client ready");
                Client instance1 = new Client(); // creat a instance
                ////////////////////////////////Receive/////////////////////////////
                clientIn = new UdpClient(c_Inport);
                ServerIpEndpointOut = new IPEndPoint(IPAddress.Parse(ServerIp), s_Outport);
                clientIn.Connect(ServerIpEndpointOut);
                clientIn.BeginReceive(new AsyncCallback(instance1.OnReceive), null); // begin receive data

                ////////////////////////////////Send/////////////////////////////
                while (true)
                {
                    clientOut = new UdpClient(c_Outport);
                    ServerIpEndpoint = new IPEndPoint(IPAddress.Parse(ServerIp), s_Inport);
                    clientOut.Connect(ServerIpEndpoint);
                    SendData = instance1.GetMassage(); // write data
                    instance1.PrintMessage(SendData);
                    instance1.OnSend(); // client send
                    clientOut.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error:" + e.Message);
            }
        }

        private Serialization GetMassage()
        {
            MessageType = Console.ReadLine();
            switch (MessageType)
            {
                case "I":
                    instance.MessageType = "LogIn";
                    break;
                case "O":
                    instance.MessageType = "LogOut";
                    break;
                default:
                    instance.MessageType = MessageType;
                    break;
            }
            instance.id = BitConverter.ToUInt32(IPAddress.Parse(MyIp).GetAddressBytes(), 0);
            instance.name = 'C';
            instance.time = DateTime.Now.ToString();
            return instance;
        }

        public void PrintMessage(Serialization data)
        {
            Console.WriteLine("<ip> " + new IPAddress(BitConverter.GetBytes(data.id)).ToString());
            Console.WriteLine("<category> " + Convert.ToString(data.category));
            Console.WriteLine("<type> " + Convert.ToString(data.type));
            Console.WriteLine("<visMask> " + Convert.ToString(data.visMask));
            Console.WriteLine("<Name> " + Convert.ToString(data.name));
            Console.Write("<geo> " + "x:" + Convert.ToString(data.geo.x));
            Console.Write("  y:" + Convert.ToString(data.geo.y));
            Console.WriteLine("  z:" + Convert.ToString(data.geo.z));
            Console.Write("<pos> " + "x:" + Convert.ToString(data.pos.x));
            Console.Write("  y:" + Convert.ToString(data.pos.y));
            Console.WriteLine("  z:" + Convert.ToString(data.pos.z));
            Console.WriteLine("<parent> " + Convert.ToString(data.parent));
            Console.WriteLine("<cfgFlags> " + Convert.ToString(data.cfgFlags));
            Console.WriteLine("<cfgModelId> " + Convert.ToString(data.cfgModelId));
            Console.WriteLine("<Time> " + data.time);
            Console.WriteLine("<Message> " + data.MessageType);
        }
        //send message to the server
        public void OnSend()
        {
            data_serialized = instance.Serialize();
            //Console.Write("Packet:");
            //PrintByteArray(data_serialized);
            //Console.WriteLine(Encoding.Default.GetString(data_serialized));
            clientOut.Send(data_serialized, data_serialized.Length);
        }

        public void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            Console.WriteLine(sb.ToString());
        }

        public void OnReceive(IAsyncResult res)
        {
            receiveBytes = clientIn.EndReceive(res, ref ServerIpEndpointOut);
            Console.WriteLine("Recieved Time:" + DateTime.Now.ToString());
            Serialization data_deserialized = instance.Deserialize(receiveBytes);
            if (data_deserialized != null)
            {
                //DataInString = DataHandle(data_deserialized);
                Console.WriteLine("Recieved data:");
                PrintMessage(data_deserialized);
            }
            clientIn.BeginReceive(new AsyncCallback(OnReceive), null);
        }
        // data processing
        //public string DataHandle(Serialization data)
        //{
        //    if (data != null)
        //    {
        //    }
        //    return data;
        //}
    }
    /// <summary>
    /// /////////////////packetize && depacketize
    /// </summary>
    public class Serialization
    {
        /** ------ state of an object (may be extended by the next structure) ------- */
        //    typedef struct
        //    {
        //        uint32_t id;                         /**< unique object ID                                              @unit _                                  @version 0x0100 */
        //        uint8_t category;                    /**< object category                                               @unit @link RDB_OBJECT_CATEGORY @endlink @version 0x0100 */
        //        uint8_t type;                        /**< object type                                                   @unit @link RDB_OBJECT_TYPE     @endlink @version 0x0100 */
        //        uint16_t visMask;                    /**< visibility mask                                               @unit @link RDB_OBJECT_VIS_FLAG @endlink @version 0x0100 */
        //        char name[RDB_SIZE_OBJECT_NAME];     /**< symbolic name                                                 @unit _                                  @version 0x0100 */
        //        RDB_GEOMETRY_t geo;                  /**< info about object's geometry                                  @unit m,m,m,m,m,m                        @version 0x0100 */
        //        RDB_COORD_t pos;                     /**< position and orientation of object's reference point          @unit m,m,m,rad,rad,rad                  @version 0x0100 */
        //        uint32_t parent;                     /**< unique ID of parent object                                    @unit _                                  @version 0x0100 */
        //        uint16_t cfgFlags;                   /**< configuration flags                                           @unit @link RDB_OBJECT_CFG_FLAG @endlink @version 0x0100 */
        //        int16_t cfgModelId;                  /**< visual model ID (configuration parameter)                     @unit _                                  @version 0x0100 */
        //}
        //RDB_OBJECT_STATE_BASE_t;
        public UInt32 id;
        public Byte category;
        public Byte type;
        public UInt16 visMask;
        public char name;
        public Vector3 geo;
        public Vector3 pos;
        public UInt32 parent;
        public UInt16 cfgFlags;
        public UInt16 cfgModelId;
        public string time;
        public string MessageType;

        public struct Vector3
        {
            public float x;
            public float y;
            public float z;
        }

        public static Serialization GetValue(UInt32 id, Byte category, Byte type, UInt16 visMask, char name, Vector3 geo, Vector3 pos, UInt32 parent, UInt16 cfgFlags, UInt16 cfgModelId)
        {
            Serialization s = new Serialization();
            s.id = id;
            s.category = category;
            s.type = type;
            s.visMask = visMask;
            s.name = name;
            s.geo = geo;
            s.pos = pos;
            s.parent = parent;
            s.cfgFlags = cfgFlags;
            s.cfgModelId = cfgModelId;
            return s;
        }
        // Default Constructor
        public Serialization()
        {
            this.id = 0;
            this.category = 0;
            this.type = 0;
            this.visMask = 0;
            this.name = ' ';
            this.geo.x = 1.1f;
            this.geo.y = 0.5f;
            this.geo.z = 1.5f;
            this.pos.x = 1.2f;
            this.pos.y = 0.6f;
            this.pos.z = 1.3f;
            this.parent = 0;
            this.cfgFlags = 0;
            this.cfgModelId = 0;
            //this.time = Convert.ToString(DateTime.Now);
            //this.MessageType = null;
        }

        public byte[] Serialize()
        {
            using (MemoryStream result = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(result))
                {
                    writer.Write(id);
                    writer.Write(category);
                    writer.Write(type);
                    writer.Write(visMask);
                    writer.Write(name);
                    writer.Write(geo.x);
                    writer.Write(geo.y);
                    writer.Write(geo.z);
                    writer.Write(pos.x);
                    writer.Write(pos.y);
                    writer.Write(pos.z);
                    writer.Write(parent);
                    writer.Write(cfgFlags);
                    writer.Write(cfgModelId);
                    writer.Write(time);
                    writer.Write(MessageType);
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
                    result.id = reader.ReadUInt32();
                    result.category = reader.ReadByte();
                    result.type = reader.ReadByte();
                    result.visMask = reader.ReadUInt16();
                    result.name = reader.ReadChar();
                    result.geo.x = reader.ReadSingle();
                    result.geo.y = reader.ReadSingle();
                    result.geo.z = reader.ReadSingle();
                    result.pos.x = reader.ReadSingle();
                    result.pos.y = reader.ReadSingle();
                    result.pos.z = reader.ReadSingle();
                    result.parent = reader.ReadUInt32();
                    result.cfgFlags = reader.ReadUInt16();
                    result.cfgModelId = reader.ReadUInt16();
                    result.time = reader.ReadString();
                    result.MessageType = reader.ReadString();
                }
            }
            return result;
        }

    }
}
