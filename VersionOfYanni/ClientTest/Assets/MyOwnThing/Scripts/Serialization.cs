using System;
using System.IO;
using UnityEngine;
using System.Text;
using System.Runtime.InteropServices;

namespace UDPChat
{
    #region header of a complete message
    public struct RDB_MSG_HDR_t
    {
        public UInt16 magicNo;      //(2-byte) must be RDB_MAGIC_NO (35712)                                               @unit @link GENERAL_DEFINITIONS @endlink   @version 0x0100 */
        public UInt16 version;      //(2-byte)upper byte = major, lower byte = minor                                     @unit _                                    @version 0x0100 */
        public UInt32 headerSize;   //(4-byte)size of this header structure when transmitted                             @unit byte                                 @version 0x0100 */
        public UInt32 dataSize;     //(4-byte) size of data following the header                                          @unit byte                                 @version 0x0100 */
        public UInt32 frameNo;      //(4-byte) number of the simulation frame                                             @unit _                                    @version 0x0100 */
        public double simTime;      //(8-byte) simulation time                                                            @unit s                                    @version 0x0100 */

        public RDB_MSG_HDR_t(UInt16 magicNo, UInt16 version, UInt32 headerSize, UInt32 dataSize, UInt32 frameNo, double simTime)
        {
            this.magicNo = magicNo;
            this.version = version;
            this.headerSize = headerSize;
            this.dataSize = dataSize;
            this.frameNo = frameNo;
            this.simTime = simTime;
        }
    }
    #endregion

    #region header of a package vector within a message
    public struct RDB_MSG_ENTRY_HDR_t
    {
        public UInt32 headerSize;   //(4-byte) size of this header structure when transmitted                              @unit byte                     @version 0x0100 */
        public UInt32 dataSize;     //(4-byte) size of data following the header                                           @unit byte                     @version 0x0100 */
        public UInt32 elementSize;  //(4-byte) if data following the header contains an array of elements of equal size:
                                    //size of one element in this data
                                    //(elementSize is equivalent to dataSize if only one element is transmitted)  @unit byte                         @version 0x0100 */
        public UInt16 pkgId;        //(2-byte) package identifier                                                          @unit _                            @version 0x0100 */
        public UInt16 flags;        //(2-byte) various flags concerning the package's contents (e.g. extension)            @unit @link RDB_PKG_FLAG @endlink  @version 0x0100 */

        public RDB_MSG_ENTRY_HDR_t(UInt32 headerSize, UInt32 dataSize, UInt32 elementSize, UInt16 pkgId, UInt16 flags)
        {
            this.headerSize = headerSize;
            this.dataSize = dataSize;
            this.elementSize = elementSize;
            this.pkgId = pkgId;
            this.flags = flags;
        }
    }
    #endregion

    #region complete object data (basic and extended info)
    public struct RDB_OBJECT_STATE_t
    {
        public RDB_OBJECT_STATE_BASE_t Base;  //(112-byte) state of an object     @unit RDB_OBJECT_STATE_BASE_t   @version 0x0100 */
        public RDB_OBJECT_STATE_EXT_t ext;   //(88-byte) extended object data   @unit RDB_OBJECT_STATE_EXT_t    @version 0x0100 */
        public RDB_OBJECT_STATE_t(RDB_OBJECT_STATE_BASE_t Base, RDB_OBJECT_STATE_EXT_t ext)
        {
            this.Base = Base;
            this.ext = ext;
        }
    }

    /** ------ state of an object (may be extended by the next structure) ------- */
    public struct RDB_OBJECT_STATE_BASE_t
    {
        public UInt32 id;       //(4-byte) unique object ID
        public Byte category;   //(1-byte) object category
        public Byte type;       //(1-byte) object type
        public UInt16 visMask;  //(2-byte) visibility mask
        public char[] name;      //(32-byte) symbolic name
        public RDB_GEOMETRY_t geo;  //(24-byte) info about object's geometry
        public RDB_COORD_t pos;     //(40-byte) position and orientation of object's reference point
        public UInt32 parent;       //(4-byte) unique ID of parent object
        public UInt16 cfgFlags;     //(2-byte) configuration flags
        public UInt16 cfgModelId;   //(2-byte) visual model ID

        public RDB_OBJECT_STATE_BASE_t(UInt32 id, Byte category, Byte type, UInt16 visMask, char[] name, RDB_GEOMETRY_t geo, RDB_COORD_t pos, UInt32 parent, UInt16 cfgFlags, UInt16 cfgModelId)
        {
            this.id = id;
            this.category = category;
            this.type = type;
            this.visMask = visMask;
            this.name = name;
            this.geo = geo;
            this.pos = pos;
            this.parent = parent;
            this.cfgFlags = cfgFlags;
            this.cfgModelId = cfgModelId;
        }
    }

    public struct RDB_COORD_t
    {
        public double x, y, z;  // x,y,z position
        public float h, p, r;   // heading, pitch, roll angle
        public Byte flags, type;  // co-ordinate flags and co-ordinate system type identifier  
        public UInt16 system; // unique ID of the corresponding (user) co-ordinate system
        public RDB_COORD_t(double x, double y, double z, float h, float p, float r, byte flags, byte type, UInt16 system)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.h = h;
            this.p = p;
            this.r = r;
            this.flags = flags;
            this.type = type;
            this.system = system;
        }
    }

    public struct RDB_GEOMETRY_t
    {
        public float dimX, dimY, dimZ, offX, offY, offZ;
        public RDB_GEOMETRY_t(float dimX, float dimY, float dimZ, float offX, float offY, float offZ)
        {
            this.dimX = dimX; // dimx dimension in object co-ordinates (length)
            this.dimY = dimY; // dimy dimension in object co-ordinates (width)  
            this.dimZ = dimZ; // dimz dimension in object co-ordinates (height) 
            this.offX = offX; // offx distance from ref. point to center of geometry, object co-ordinate system
            this.offY = offY; // offy distance from ref. point to center of geometry, object co-ordinate system 
            this.offZ = offZ; // offz distance from ref. point to center of geometry, object co-ordinate system
        }
    }

    /** ------ extended object data (e.g. for dynamic objects) ------- */
    public struct RDB_OBJECT_STATE_EXT_t
    {
        public RDB_COORD_t speed;              //(40-byte)< speed and rates                                               @unit m/s,m/s,m/s,rad/s,rad/s,rad/s          @version 0x0100 */
        public RDB_COORD_t accel;              //(40-byte)< acceleration                                                  @unit m/s2,m/s2,m/s2,rad/s2,rad/s2/rad/s2    @version 0x0100 */
        public float traveledDist;             //(4-byte) traveled distance                                             @unit m                                      @version 0x011a */
        public UInt32[] spare;                   //(4*3=12-byte) reserved for future use                                       @unit _                                      @version 0x0100 */
        public RDB_OBJECT_STATE_EXT_t(RDB_COORD_t speed, RDB_COORD_t accel, float traveledDist, UInt32[] spare)
        {
            this.speed = speed;
            this.accel = accel;
            this.traveledDist = traveledDist;
            this.spare = spare;
        }
    }
    #endregion

    #region standard wheel information
    public struct RDB_WHEEL_BASE_t
    {
        public UInt32 playerId;            //(4-byte) ID of the player to which the wheel belongs @unit _ @version 0x0100 */
        public Byte id;                    //(1-byte) ID of the wheel within the player @unit @link RDB_WHEEL_ID @endlink @version 0x0100 */
        public Byte flags;                 //(1-byte) wheel status flags (e.g. for sound ) @unit @link RDB_WHEEL_FLAG @endlink @version 0x0114 */
        public byte[] spare0;              //(1*2-byte) reserved for future use @unit _ @version 0x0100 */
        public float radiusStatic;         //(4-byte) static tire radius @unit m @version 0x0100 */
        public float springCompression;    //(4-byte) compression of spring @unit m @version 0x0100 */
        public float rotAngle;             //(4-byte) angle of rotation @unit rad @version 0x0100 */
        public float slip;                 //(4-byte) slip factor [0.0..1.0] @unit _ @version 0x0100 */
        public float steeringAngle;        //(4-byte) steering angle @unit rad @version 0x0100 */
        public UInt32[] spare1;              //(4*4=16-byte) reserved for future use @unit _ @version 0x0100 */
        public RDB_WHEEL_BASE_t(UInt32 playerId, byte id, byte flags, byte[] spare0, float radiusStatic, float springCompression, float rotAngle, float slip, float steeringAngle, UInt32[] spare1)
        {
            this.playerId = playerId;
            this.id = id;
            this.flags = flags;
            this.spare0 = spare0;
            this.radiusStatic = radiusStatic;
            this.springCompression = springCompression;
            this.rotAngle = rotAngle;
            this.slip = slip;
            this.steeringAngle = steeringAngle;
            this.spare1 = spare1;
        }
    }

    /** ------ complete wheel data (basic and extended info) ------- */
    public struct RDB_WHEEL_t
    {

        public RDB_WHEEL_BASE_t Base;        //(44-byte)standard wheel information @unit RDB_WHEEL_BASE_t @version 0x0100 */
        //public RDB_WHEEL_EXT_t ext;          //(60-byte) extension of standard wheel information @unit RDB_WHEEL_EXT_t @version 0x0100 */
        public RDB_WHEEL_t(RDB_WHEEL_BASE_t Base)
        {
            this.Base = Base;
        }
    }
    #endregion

    public class Serialization : MonoBehaviour
    {
        #region variables
        public UInt32 NoOfPlayers;
        public RDB_MSG_HDR_t msg_hdr;
        public RDB_MSG_ENTRY_HDR_t start_of_frame;
        public RDB_MSG_ENTRY_HDR_t msg_entry_hdr_object_state;
        public RDB_OBJECT_STATE_t object_state;
        public RDB_MSG_ENTRY_HDR_t msg_entry_hdr_wheel;
        public RDB_WHEEL_t[] wheel = new RDB_WHEEL_t[4];
        public RDB_MSG_ENTRY_HDR_t end_of_frame;
        public RDB_MSG_ENTRY_HDR_t msg_entry_hdr;
        private UInt32 l = 0;
        public bool flag_vires = false;
        #endregion
        void Start() { }

        public byte[] Serialize()
        {
            using (MemoryStream result = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(result))
                {
                    writer.Write(GetBytes(msg_hdr));  // header of a complete message
                    writer.Write(GetBytes(start_of_frame));  // start of a frame
                    writer.Write(GetBytes(msg_entry_hdr_object_state)); // object state
                    writer.Write(WriteObjectStateBase());
                    writer.Write(WriteObjectStateExt());
                    writer.Write(GetBytes(msg_entry_hdr_wheel)); // wheel
                    writer.Write(WriteWheelState(wheel[0]));
                    writer.Write(WriteWheelState(wheel[1]));
                    writer.Write(WriteWheelState(wheel[2]));
                    writer.Write(WriteWheelState(wheel[3]));
                    writer.Write(GetBytes(end_of_frame)); // end of a frame
                }
                return result.ToArray();
            }
        }

        public Serialization[] Deserialize(byte[] dataStream)
        {
            Serialization[] result_matrix = new Serialization[NoOfPlayers];
            Serialization result = new Serialization();
            using (MemoryStream m = new MemoryStream(dataStream))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result = GetStruct(reader.ReadBytes(24), result);
                    for (int n = 0; n < NoOfPlayers; n++)
                    {
                        result_matrix[n] = new Serialization();
                        result_matrix[n].msg_hdr = result.msg_hdr;
                    }

                    while (l < result.msg_hdr.dataSize)
                    {
                        result = GetStruct(reader.ReadBytes(16), result);
                        l += 16;
                        switch (result.msg_entry_hdr.pkgId)
                        {
                            case 1:
                                result.start_of_frame = result.msg_entry_hdr;
                                break;
                            case 9:
                                result.msg_entry_hdr_object_state = result.msg_entry_hdr;
                                UInt32 k1 = result.msg_entry_hdr_object_state.dataSize / 208;
                                if (k1 > 2) { flag_vires = true; }
                                for (int i = 0; i < k1; i++)
                                {
                                    result_matrix[i].object_state = GetObjectState(reader.ReadBytes(208));
                                }
                                break;
                            case 14:
                                result.msg_entry_hdr_wheel = result.msg_entry_hdr;
                                UInt32 k2 = result.msg_entry_hdr_wheel.dataSize / 176;
                                for (int j = 0; j < k2; j++)
                                {
                                    result = GetWheelState(reader.ReadBytes(44), result, 0);
                                    result = GetWheelState(reader.ReadBytes(44), result, 1);
                                    result = GetWheelState(reader.ReadBytes(44), result, 2);
                                    result = GetWheelState(reader.ReadBytes(44), result, 3);
                                    result_matrix = CheckWheelId(result.wheel, result_matrix);
                                }              
                                break;
                            case 2:
                                result.end_of_frame = result.msg_entry_hdr;
                                break;
                            default:
                                Debug.Log("Unwanted Package with pkgId:" + result.msg_entry_hdr.pkgId);
                                byte[] UnwantedMassege = reader.ReadBytes((int)result.msg_entry_hdr.dataSize);
                                break;
                        }
                        l += result.msg_entry_hdr.dataSize;
                    }
                    l = 0;
                }
            }
            return result_matrix;
        }

        public Serialization[] CheckWheelId(RDB_WHEEL_t[] wheelInfo, Serialization[] result_matrix)
        {
            for (int i = 0; i < NoOfPlayers; i++)
            {
                result_matrix[i].wheel = new RDB_WHEEL_t[4];
                if (wheelInfo[1].Base.playerId == result_matrix[i].object_state.Base.id)
                {
                    result_matrix[i].wheel = wheelInfo;
                    break;
                }
            }
            return result_matrix;
        }

        public RDB_OBJECT_STATE_t GetObjectState(byte[] data)
        {
            RDB_OBJECT_STATE_t ObjectState = new RDB_OBJECT_STATE_t();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    ObjectState.Base.id = reader.ReadUInt32();
                    ObjectState.Base.category = reader.ReadByte();
                    ObjectState.Base.type = reader.ReadByte();
                    ObjectState.Base.visMask = reader.ReadUInt16();
                    ObjectState.Base.name = reader.ReadChars(32);
                    ObjectState.Base.geo.dimX = reader.ReadSingle();
                    ObjectState.Base.geo.dimY = reader.ReadSingle();
                    ObjectState.Base.geo.dimZ = reader.ReadSingle();
                    ObjectState.Base.geo.offX = reader.ReadSingle();
                    ObjectState.Base.geo.offY = reader.ReadSingle();
                    ObjectState.Base.geo.offZ = reader.ReadSingle();
                    ObjectState.Base.pos.x = reader.ReadDouble();
                    ObjectState.Base.pos.y = reader.ReadDouble();
                    ObjectState.Base.pos.z = reader.ReadDouble();
                    ObjectState.Base.pos.h = reader.ReadSingle();
                    ObjectState.Base.pos.p = reader.ReadSingle();
                    ObjectState.Base.pos.r = reader.ReadSingle();
                    ObjectState.Base.pos.flags = reader.ReadByte();
                    ObjectState.Base.pos.type = reader.ReadByte();
                    ObjectState.Base.pos.system = reader.ReadUInt16();
                    ObjectState.Base.parent = reader.ReadUInt32();
                    ObjectState.Base.cfgFlags = reader.ReadUInt16();
                    ObjectState.Base.cfgModelId = reader.ReadUInt16();
                    ObjectState.ext.speed.x = reader.ReadDouble();
                    ObjectState.ext.speed.y = reader.ReadDouble();
                    ObjectState.ext.speed.z = reader.ReadDouble();
                    ObjectState.ext.speed.h = reader.ReadSingle();
                    ObjectState.ext.speed.p = reader.ReadSingle();
                    ObjectState.ext.speed.r = reader.ReadSingle();
                    ObjectState.ext.speed.flags = reader.ReadByte();
                    ObjectState.ext.speed.type = reader.ReadByte();
                    ObjectState.ext.speed.system = reader.ReadUInt16();
                    ObjectState.ext.accel.x = reader.ReadDouble();
                    ObjectState.ext.accel.y = reader.ReadDouble();
                    ObjectState.ext.accel.z = reader.ReadDouble();
                    ObjectState.ext.accel.h = reader.ReadSingle();
                    ObjectState.ext.accel.p = reader.ReadSingle();
                    ObjectState.ext.accel.r = reader.ReadSingle();
                    ObjectState.ext.accel.flags = reader.ReadByte();
                    ObjectState.ext.accel.type = reader.ReadByte();
                    ObjectState.ext.accel.system = reader.ReadUInt16();
                }
            }
            return ObjectState;
        }

        public byte[] WriteObjectStateBase()
        {
            using (MemoryStream result = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(result))
                {
                    writer.Write(object_state.Base.id);
                    writer.Write(object_state.Base.category);
                    writer.Write(object_state.Base.type);
                    writer.Write(object_state.Base.visMask);
                    writer.Write(object_state.Base.name);
                    writer.Write(GetBytes(object_state.Base.geo));
                    writer.Write(GetBytes(object_state.Base.pos));
                    writer.Write(object_state.Base.parent);
                    writer.Write(object_state.Base.cfgFlags);
                    writer.Write(object_state.Base.cfgModelId);
                }
                return result.ToArray();
            }
        }

        public byte[] WriteObjectStateExt()
        {
            using (MemoryStream result = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(result))
                {
                    writer.Write(GetBytes(object_state.ext.speed));
                    writer.Write(GetBytes(object_state.ext.accel));
                    writer.Write(object_state.ext.traveledDist);
                    writer.Write(object_state.ext.spare[0]);
                    writer.Write(object_state.ext.spare[1]);
                    writer.Write(object_state.ext.spare[2]);
                }
                return result.ToArray();
            }
        }

        public byte[] WriteWheelState(RDB_WHEEL_t wheel)
        {
            using (MemoryStream result = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(result))
                {
                    writer.Write(wheel.Base.playerId);
                    writer.Write(wheel.Base.id);
                    writer.Write(wheel.Base.flags);
                    writer.Write(wheel.Base.spare0);
                    writer.Write(wheel.Base.radiusStatic);
                    writer.Write(wheel.Base.springCompression);
                    writer.Write(wheel.Base.rotAngle);
                    writer.Write(wheel.Base.slip);
                    writer.Write(wheel.Base.steeringAngle);
                    writer.Write(wheel.Base.spare1[0]);
                    writer.Write(wheel.Base.spare1[1]);
                    writer.Write(wheel.Base.spare1[2]);
                    writer.Write(wheel.Base.spare1[3]);
                }
                return result.ToArray();
            }
        }

        public Serialization GetWheelState(byte[] data, Serialization result, int i)
        {
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.wheel[i].Base.playerId = reader.ReadUInt32();
                    result.wheel[i].Base.id = reader.ReadByte();
                    result.wheel[i].Base.flags = reader.ReadByte();
                    result.wheel[i].Base.spare0 = new byte[2];
                    result.wheel[i].Base.spare0[0] = reader.ReadByte();
                    result.wheel[i].Base.spare0[1] = reader.ReadByte();
                    result.wheel[i].Base.radiusStatic = reader.ReadSingle();
                    result.wheel[i].Base.springCompression = reader.ReadSingle();
                    result.wheel[i].Base.rotAngle = reader.ReadSingle();
                    result.wheel[i].Base.slip = reader.ReadSingle();
                    result.wheel[i].Base.steeringAngle = reader.ReadSingle();
                    result.wheel[i].Base.spare1 = new UInt32[4];
                    result.wheel[i].Base.spare1[0] = reader.ReadUInt32();
                    result.wheel[i].Base.spare1[1] = reader.ReadUInt32();
                    result.wheel[i].Base.spare1[2] = reader.ReadUInt32();
                    result.wheel[i].Base.spare1[3] = reader.ReadUInt32();
                }
                return result;
            }
        }

        byte[] GetBytes(object str) // get bytes from structure
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public Serialization GetStruct(byte[] arr, Serialization s) // get structure from bytes
        {
            int size = arr.Length;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(arr, 0, ptr, size);
            switch (arr.Length)
            {
                case 24:
                    s.msg_hdr = (RDB_MSG_HDR_t)Marshal.PtrToStructure(ptr, s.msg_hdr.GetType());
                    break;
                case 16:
                    s.msg_entry_hdr = (RDB_MSG_ENTRY_HDR_t)Marshal.PtrToStructure(ptr, s.msg_entry_hdr.GetType());
                    break;
            }
            Marshal.FreeHGlobal(ptr);
            return s;
        }

        public void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("{ ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            Debug.Log(sb.ToString());
        }
    }
}