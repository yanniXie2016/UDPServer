using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace myApp
{
    public class HandlePacket : MonoBehaviour
    {

        private char[] myname = new char[32];
        private UInt32[] spare = new UInt32[3];
        public static HandlePacket hp = new HandlePacket();
        public UInt32 m_ID;

        /** ------ header of a complete message ------ */
        public class header_c
        {
            public UInt16 magicNo;      /**< must be RDB_MAGIC_NO (35712)                                               @unit @link GENERAL_DEFINITIONS @endlink   @version 0x0100 */
            public UInt16 version;      /**< upper byte = major, lower byte = minor                                     @unit _                                    @version 0x0100 */
            public UInt32 headerSize;   /**< size of this header structure when transmitted                             @unit byte                                 @version 0x0100 */
            public UInt32 dataSize;     /**< size of data following the header                                          @unit byte                                 @version 0x0100 */
            public UInt32 frameNo;      /**< number of the simulation frame                                             @unit _                                    @version 0x0100 */
            public double simTime;      /**< simulation time                                                            @unit s                                    @version 0x0100 */

            public header_c(UInt32 dataSize, UInt32 frameNo, double simTime)
            {
                this.magicNo = 35712;
                this.version = 0x011e;   // no idea
                this.headerSize = 24;
                this.dataSize = dataSize;
                this.frameNo = frameNo;
                this.simTime = simTime;
            }

            public header_c(byte[] DataStream)
            {
                this.magicNo = BitConverter.ToUInt16(DataStream, 0);
                this.version = BitConverter.ToUInt16(DataStream, 2);
                this.headerSize = BitConverter.ToUInt32(DataStream, 4);
                this.dataSize = BitConverter.ToUInt32(DataStream, 8);
                this.frameNo = BitConverter.ToUInt32(DataStream, 12);
                this.simTime = BitConverter.ToDouble(DataStream, 16);
            }
        }

        /** ------ header of a package vector within a message ------ */
        public class header_m
        {
            public UInt32 headerSize;   /**< size of this header structure when transmitted                              @unit byte                     @version 0x0100 */
            public UInt32 dataSize;     /**< size of data following the header                                           @unit byte                     @version 0x0100 */
            public UInt32 elementSize;  /**< if data following the header contains an array of elements of equal size:
							size of one element in this data
							(elementSize is equivalent to dataSize if only one element is transmitted)  @unit byte                         @version 0x0100 */
            public UInt16 pkgId;        /**< package identifier                                                          @unit _                            @version 0x0100 */
            public UInt16 flags;        /**< various flags concerning the package's contents (e.g. extension)            @unit @link RDB_PKG_FLAG @endlink  @version 0x0100 */

            public header_m(UInt32 dataSize, UInt32 elementSize, UInt16 pkgId, UInt16 flags)
            {
                this.headerSize = 16;
                this.dataSize = dataSize;
                this.elementSize = elementSize;
                this.pkgId = pkgId;
                this.flags = flags;
            }

            public header_m(byte[] DataStream)
            {
                this.headerSize = BitConverter.ToUInt32(DataStream, 0);
                this.dataSize = BitConverter.ToUInt32(DataStream, 4);
                this.elementSize = BitConverter.ToUInt32(DataStream, 8);
                this.pkgId = BitConverter.ToUInt16(DataStream, 12);
                this.flags = BitConverter.ToUInt16(DataStream, 14);
            }
        }

        /** ------ geometry information for an object --- */
        public class geo
        {
            public float dimX;        /**< x dimension in object co-ordinates (length)                                               @unit m                                  @version 0x0100 */
            public float dimY;        /**< y dimension in object co-ordinates (width)                                                @unit m                                  @version 0x0100 */
            public float dimZ;        /**< z dimension in object co-ordinates (height)                                               @unit m                                  @version 0x0100 */
            public float offX;        /**< x distance from ref. point to center of geometry, object co-ordinate system               @unit m                                  @version 0x0100 */
            public float offY;        /**< y distance from ref. point to center of geometry, object co-ordinate system               @unit m                                  @version 0x0100 */
            public float offZ;        /**< z distance from ref. point to center of geometry, object co-ordinate system               @unit m                                  @version 0x0100 */

            public geo(float dimX, float dimY, float dimZ, float offX, float offY, float offZ)
            {
                this.dimX = dimX;
                this.dimY = dimY;
                this.dimZ = dimZ;
                this.offX = offX;
                this.offY = offY;
                this.offZ = offZ;
            }

            public geo(byte[] DataStream)
            {
                this.dimX = BitConverter.ToSingle(DataStream, 0);
                this.dimY = BitConverter.ToSingle(DataStream, 4);
                this.dimZ = BitConverter.ToSingle(DataStream, 8);
                this.offX = BitConverter.ToSingle(DataStream, 12);
                this.offY = BitConverter.ToSingle(DataStream, 16);
                this.offZ = BitConverter.ToSingle(DataStream, 20);
            }

            public geo ReturnGeo(geo mygeo)
            {
                return mygeo;
            }
        }

        /** ------ generic co-ordinate structure --- */
        public class coord
        {
            public double x;       /**< x position                                                @unit m                                @version 0x0100 */
            public double y;       /**< y position                                                @unit m                                @version 0x0100 */
            public double z;       /**< z position                                                @unit m                                @version 0x0100 */
            public float h;       /**< heading angle                                             @unit rad                              @version 0x0100 */
            public float p;       /**< pitch angle                                               @unit rad                              @version 0x0100 */
            public float r;       /**< roll angle                                                @unit rad                              @version 0x0100 */
            public byte flags;   /**< co-ordinate flags                                         @unit @link RDB_COORD_FLAG @endlink    @version 0x0100 */
            public byte type;    /**< co-ordinate system type identifier                        @unit @link RDB_COORD_TYPE @endlink    @version 0x0100 */
            public UInt16 system;  /**< unique ID of the corresponding (user) co-ordinate system  @unit _                                @version 0x0100 */

            public coord(double x, double y, double z, float h, float p, float r, byte flags, byte type, UInt16 system)
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

            public coord(byte[] DataStream)
            {
                this.x = BitConverter.ToDouble(DataStream, 0);
                this.y = BitConverter.ToDouble(DataStream, 8);
                this.z = BitConverter.ToDouble(DataStream, 16);
                this.h = BitConverter.ToSingle(DataStream, 24);
                this.p = BitConverter.ToSingle(DataStream, 28);
                this.r = BitConverter.ToSingle(DataStream, 32);
                this.flags = DataStream[36];
                this.type = DataStream[37];
                this.system = BitConverter.ToUInt16(DataStream, 38);
            }

            public coord ReturnCoord(coord mycoord)
            {
                return mycoord;
            }
        }

        /** ------ state of an object (may be extended by the next structure) ------- */
        public class state_o
        {
            
            public UInt32 id;                         /**< unique object ID                                              @unit _                                  @version 0x0100 */
            public byte category;                   /**< object category                                               @unit @link RDB_OBJECT_CATEGORY @endlink @version 0x0100 */
            public byte type;                       /**< object type                                                   @unit @link RDB_OBJECT_TYPE     @endlink @version 0x0100 */
            public UInt16 visMask;                    /**< visibility mask                                               @unit @link RDB_OBJECT_VIS_FLAG @endlink @version 0x0100 */
            public char[] name = new char[32];                       /**< symbolic name                                                 @unit _                                  @version 0x0100 */
            public geo geo;                        /**< info about object's geometry                                  @unit m,m,m,m,m,m                        @version 0x0100 */
            public coord pos;                        /**< position and orientation of object's reference point          @unit m,m,m,rad,rad,rad                  @version 0x0100 */
            public UInt32 parent;                     /**< unique ID of parent object                                    @unit _                                  @version 0x0100 */
            public UInt16 cfgFlags;                   /**< configuration flags                                           @unit @link RDB_OBJECT_CFG_FLAG @endlink @version 0x0100 */
            public UInt16 cfgModelId;                 /**< visual model ID (configuration parameter)                     @unit _                                  @version 0x0100 */
            public byte[] geo_b = new byte[24];
            public byte[] coord_b = new byte[40];
            public byte[] name_b = new byte[64];

            public state_o(UInt32 id, byte category, byte type, UInt16 visMask, char[] name, geo geo, coord pos, UInt32 parent, UInt16 cfgFlags, UInt16 cfgModelId)
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

            public state_o(byte[] DataStream)
            {
                this.id = BitConverter.ToUInt32(DataStream, 0);
                this.category = DataStream[4];
                this.type = DataStream[5];
                this.visMask = BitConverter.ToUInt16(DataStream, 6);
                Buffer.BlockCopy(DataStream, 8, name_b, 0, 32);
                Buffer.BlockCopy(DataStream, 40, geo_b, 0, 24);
                Buffer.BlockCopy(DataStream, 64, coord_b, 0, 40);
                this.name = Encoding.ASCII.GetString(name_b).ToCharArray();
                this.geo = new geo(geo_b);
                this.pos = new coord(coord_b);
                this.parent = BitConverter.ToUInt32(DataStream, 104);
                this.cfgFlags = BitConverter.ToUInt16(DataStream, 108);
                this.cfgModelId = BitConverter.ToUInt16(DataStream, 110);
            }
        }

        /** ------ extended object data (e.g. for dynamic objects) ------- */
        public class state_e
        {
            public coord speed;                      /**< speed and rates                                               @unit m/s,m/s,m/s,rad/s,rad/s,rad/s          @version 0x0100 */
            public coord accel;                      /**< acceleration                                                  @unit m/s2,m/s2,m/s2,rad/s2,rad/s2/rad/s2    @version 0x0100 */
            public float traveledDist;               /**< traveled distance                                             @unit m                                      @version 0x011a */
            public UInt32[] spare = new UInt32[3];                   /**< reserved for future use                                       @unit _                                      @version 0x0100 */
            public byte[] speed_b = new byte[40];
            public byte[] accel_b = new byte[40];

            public state_e(coord speed, coord accel, float traveledDist, UInt32[] spare)
            {
                this.speed = speed;
                this.accel = accel;
                this.traveledDist = traveledDist;
                this.spare = spare;
            }

            public state_e(byte[] DataStream)
            {
                Buffer.BlockCopy(DataStream, 0, speed_b, 0, 40);
                Buffer.BlockCopy(DataStream, 40, accel_b, 0, 40);
                this.speed = new coord(speed_b);
                this.accel = new coord(accel_b);
                this.traveledDist = BitConverter.ToSingle(DataStream, 80);
                this.spare[0] = BitConverter.ToUInt32(DataStream, 84);
                this.spare[1] = BitConverter.ToUInt32(DataStream, 88);
                this.spare[2] = BitConverter.ToUInt32(DataStream, 92);
            }
        }

        /** ------ complete object data (basic and extended info) ------- */
        public class state
        {
            public state_o state_base;           /**< state of an object     @unit RDB_OBJECT_STATE_BASE_t   @version 0x0100 */
            public state_e state_ext;            /**< extended object data   @unit RDB_OBJECT_STATE_EXT_t    @version 0x0100 */
            public byte[] state_base_b = new byte[112];
            public byte[] state_ext_b = new byte[96];

            public state(state_o state_base, state_e state_ext)
            {
                this.state_base = state_base;
                this.state_ext = state_ext;
            }

            public state(byte[] DataStream)
            {
                Buffer.BlockCopy(DataStream, 0, state_base_b, 0, 112);
                Buffer.BlockCopy(DataStream, 112, state_ext_b, 0, 96);
                this.state_base = new state_o(state_base_b);
                this.state_ext = new state_e(state_ext_b);
            }
        }

        /** ------ standard wheel information ------ */
        public class wheel_o
        {
            public UInt32 playerId; /**< ID of the player to which the wheel belongs @unit _ @version 0x0100 */
            public byte id; /**< ID of the wheel within the player @unit @link RDB_WHEEL_ID @endlink @version 0x0100 */
            public byte flags; /**< wheel status flags (e.g. for sound ) @unit @link RDB_WHEEL_FLAG @endlink @version 0x0114 */
            public byte[] spare0 = new byte[2]; /**< reserved for future use @unit _ @version 0x0100 */
            public float radiusStatic; /**< static tire radius @unit m @version 0x0100 */
            public float springCompression; /**< compression of spring @unit m @version 0x0100 */
            public float rotAngle; /**< angle of rotation @unit rad @version 0x0100 */
            public float slip; /**< slip factor [0.0..1.0] @unit _ @version 0x0100 */
            public float steeringAngle; /**< steering angle @unit rad @version 0x0100 */
            public UInt32[] spare1 = new UInt32[4]; /**< reserved for future use @unit _ @version 0x0100 */

            public wheel_o(UInt32 playerId, byte id, byte flags, byte[] spare0, float radiusStatic, float springCompression, float rotAngle, float slip, float steeringAngle, UInt32[] spare1)
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
        

            public wheel_o(byte[] DataStream)
            {
                this.playerId = BitConverter.ToUInt32(DataStream, 0);
                this.id = DataStream[4];
                this.flags = DataStream[5];
                this.spare0[0] = DataStream[6];
                this.spare0[1] = DataStream[7];
                this.radiusStatic = BitConverter.ToSingle(DataStream, 8);
                this.springCompression = BitConverter.ToSingle(DataStream, 12);
                this.rotAngle = BitConverter.ToSingle(DataStream, 16);
                this.slip = BitConverter.ToSingle(DataStream, 20);
                this.steeringAngle = BitConverter.ToSingle(DataStream, 24);
                for (int i = 0; i < 4; i++)
                {
                    this.spare1[i] = BitConverter.ToUInt32(DataStream, 28 + 4 * i);
                }

            }
        }

        /** ------ extension of standard wheel information ------ */
        public class wheel_e
        {
            public float vAngular; /**< angular velocity @unit rad/s @version 0x0100 */
            public float forceZ; /**< wheel contact force @unit N @version 0x0100 */
            public float forceLat; /**< lateral force @unit N @version 0x0100 */
            public float forceLong; /**< longitudinal force @unit N @version 0x0100 */
            public float[] forceTireWheelXYZ = new float[3]; /**< force of tire on wheel @unit N @version 0x0100 */
            public float radiusDynamic; /**< dynamic tire radius @unit m @version 0x0100 */
            public float brakePressure; /**< brake pressure at wheel @unit Pa @version 0x0100 */
            public float torqueDriveShaft; /**< torque at drive shaft @unit Nm @version 0x0100 */
            public float damperSpeed; /**< speed of damper @unit m/s @version 0x0100 */
            public UInt32[] spare2 = new UInt32[4]; /**< reserved for future use @unit _ @version 0x0100 */

            public wheel_e(float vAngular, float forceZ, float forceLat, float forceLong, float[] forceTireWheelXYZ, float radiusDynamic, float brakePressure, float torqueDriveShaft, float damperSpeed, UInt32[] spare2)
            {
                this.vAngular = vAngular;
                this.forceZ = forceZ;
                this.forceLat = forceLat;
                this.forceLong = forceLong;
                this.forceTireWheelXYZ = forceTireWheelXYZ;
                this.radiusDynamic = radiusDynamic;
                this.brakePressure = brakePressure;
                this.torqueDriveShaft = torqueDriveShaft;
                this.damperSpeed = damperSpeed;
                this.spare2 = spare2;
            }

            public wheel_e()
            {

            }
            public wheel_e(byte[] DataStream)
            {
                this.vAngular = BitConverter.ToSingle(DataStream, 0);
                this.forceZ = BitConverter.ToSingle(DataStream, 4);
                this.forceLat = BitConverter.ToSingle(DataStream, 8);
                this.forceLong = BitConverter.ToSingle(DataStream, 12);
                for (int i = 0; i < 3; i++)
                {
                    this.forceTireWheelXYZ[i] = BitConverter.ToSingle(DataStream, 16 + 4 * i);
                }
                this.radiusDynamic = BitConverter.ToSingle(DataStream, 20);
                this.brakePressure = BitConverter.ToSingle(DataStream, 0);
                this.torqueDriveShaft = BitConverter.ToSingle(DataStream, 0);
                this.damperSpeed = BitConverter.ToSingle(DataStream, 0);
                for (int i = 0; i < 4; i++)
                {
                    this.spare2[i] = BitConverter.ToUInt32(DataStream, 44 + 4 * i);
                }
            }
        }

        /** ------ complete wheel data (basic and extended info) ------- */
        public class wheel
        {
            public wheel_o wheel_O; /**< standard wheel information @unit RDB_WHEEL_BASE_t @version 0x0100 */
            public wheel_e wheel_E; /**< extension of standard wheel information @unit RDB_WHEEL_EXT_t @version 0x0100 */
            public byte[] wheel_base_O = new byte[44];
            public byte[] wheel_ext_E = new byte[60];

            public wheel(wheel_o wheel_O, wheel_e wheel_E)
            {
                this.wheel_O = wheel_O;
                this.wheel_E = wheel_E;
            }

            public wheel(byte[] DataStream)
            {
                Buffer.BlockCopy(DataStream, 0, wheel_base_O, 0, 44);
                Buffer.BlockCopy(DataStream, 44, wheel_ext_E, 0, 60);
                this.wheel_O = new wheel_o(wheel_base_O);
                this.wheel_E = new wheel_e(wheel_ext_E);
            }
        }

        public class Serialization
        {
            public byte[] Serialize(header_c header_C)
            {
                List<byte> DataStream = new List<byte>();
                DataStream.AddRange(BitConverter.GetBytes(header_C.magicNo));
                DataStream.AddRange(BitConverter.GetBytes(header_C.version));
                DataStream.AddRange(BitConverter.GetBytes(header_C.headerSize));
                DataStream.AddRange(BitConverter.GetBytes(header_C.dataSize));
                DataStream.AddRange(BitConverter.GetBytes(header_C.frameNo));
                DataStream.AddRange(BitConverter.GetBytes(header_C.simTime));
                return DataStream.ToArray();
            }

            public byte[] Serialize(header_m header_M)
            {
                List<byte> DataStream = new List<byte>();
                DataStream.AddRange(BitConverter.GetBytes(header_M.headerSize));
                DataStream.AddRange(BitConverter.GetBytes(header_M.dataSize));
                DataStream.AddRange(BitConverter.GetBytes(header_M.elementSize));
                DataStream.AddRange(BitConverter.GetBytes(header_M.pkgId));
                DataStream.AddRange(BitConverter.GetBytes(header_M.flags));
                return DataStream.ToArray();
            }

            public byte[] Serialize(geo geo)
            {
                List<byte> DataStream = new List<byte>();
                DataStream.AddRange(BitConverter.GetBytes(geo.dimX));
                DataStream.AddRange(BitConverter.GetBytes(geo.dimY));
                DataStream.AddRange(BitConverter.GetBytes(geo.dimZ));
                DataStream.AddRange(BitConverter.GetBytes(geo.offX));
                DataStream.AddRange(BitConverter.GetBytes(geo.offY));
                DataStream.AddRange(BitConverter.GetBytes(geo.offZ));
                return DataStream.ToArray();
            }

            public byte[] Serialize(coord coord)
            {
                List<byte> DataStream = new List<byte>();
                DataStream.AddRange(BitConverter.GetBytes(coord.x));
                DataStream.AddRange(BitConverter.GetBytes(coord.y));
                DataStream.AddRange(BitConverter.GetBytes(coord.z));
                DataStream.AddRange(BitConverter.GetBytes(coord.h));
                DataStream.AddRange(BitConverter.GetBytes(coord.p));
                DataStream.AddRange(BitConverter.GetBytes(coord.r));
                DataStream.AddRange(BitConverter.GetBytes(coord.flags));
                DataStream.AddRange(BitConverter.GetBytes(coord.type));
                DataStream.AddRange(BitConverter.GetBytes(coord.system));
                DataStream.RemoveAt(37);
                DataStream.RemoveAt(38);
                return DataStream.ToArray();
            }

            public byte[] Serialize(state_o state_O)
            {
                List<byte> DataStream = new List<byte>();
                DataStream.AddRange(BitConverter.GetBytes(state_O.id));
                DataStream.AddRange(BitConverter.GetBytes(state_O.category));
                DataStream.AddRange(BitConverter.GetBytes(state_O.type));
                DataStream.AddRange(BitConverter.GetBytes(state_O.visMask));
                byte[] realname = Encoding.ASCII.GetBytes(state_O.name);
                DataStream.AddRange(realname);
                byte[] realgeo = Serialize(state_O.geo);
                DataStream.AddRange(realgeo);
                byte[] realpos = Serialize(state_O.pos);
                DataStream.AddRange(realpos);
                DataStream.AddRange(BitConverter.GetBytes(state_O.parent));
                DataStream.AddRange(BitConverter.GetBytes(state_O.cfgFlags));
                DataStream.AddRange(BitConverter.GetBytes(state_O.cfgModelId));
                DataStream.RemoveAt(5);
                DataStream.RemoveAt(6);
                return DataStream.ToArray();
            }

            public byte[] Serialize(state_e state_E)
            {
                List<byte> DataStream = new List<byte>();
                byte[] realspeed = Serialize(state_E.speed);
                DataStream.AddRange(realspeed);
                byte[] realaccel = Serialize(state_E.accel);
                DataStream.AddRange(realaccel);
                DataStream.AddRange(BitConverter.GetBytes(state_E.traveledDist));
                DataStream.AddRange(BitConverter.GetBytes(state_E.spare[0]));
                DataStream.AddRange(BitConverter.GetBytes(state_E.spare[1]));
                DataStream.AddRange(BitConverter.GetBytes(state_E.spare[2]));
                return DataStream.ToArray();
            }

            public byte[] Serialize(state state)
            {
                List<byte> DataStream = new List<byte>();
                byte[] realstate_base = Serialize(state.state_base);
                DataStream.AddRange(realstate_base);
                byte[] realstate_ext = Serialize(state.state_ext);
                DataStream.AddRange(realstate_ext);
                return DataStream.ToArray();
            }

            public byte[] Serialize(wheel_o wheel_O)
            {
                List<byte> DataStream = new List<byte>();
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.playerId));
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.id));
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.flags));
                DataStream.AddRange(wheel_O.spare0);
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.radiusStatic));
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.springCompression));
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.rotAngle));
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.slip));
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.steeringAngle));
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.spare1[0]));
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.spare1[1]));
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.spare1[2]));
                DataStream.AddRange(BitConverter.GetBytes(wheel_O.spare1[3]));
                DataStream.RemoveAt(5);
                DataStream.RemoveAt(6);
                return DataStream.ToArray();
            }

            public byte[] Serialize(wheel_e wheel_E)
            {
                List<byte> DataStream = new List<byte>();
                DataStream.AddRange(BitConverter.GetBytes(wheel_E.vAngular));
                DataStream.AddRange(BitConverter.GetBytes(wheel_E.forceZ));
                DataStream.AddRange(BitConverter.GetBytes(wheel_E.forceLat));
                DataStream.AddRange(BitConverter.GetBytes(wheel_E.forceLong));
                for (int i = 0; i < 3; i++)
                {
                    DataStream.AddRange(BitConverter.GetBytes(wheel_E.forceTireWheelXYZ[i]));
                }
                DataStream.AddRange(BitConverter.GetBytes(wheel_E.radiusDynamic));
                DataStream.AddRange(BitConverter.GetBytes(wheel_E.brakePressure));
                DataStream.AddRange(BitConverter.GetBytes(wheel_E.torqueDriveShaft));
                DataStream.AddRange(BitConverter.GetBytes(wheel_E.damperSpeed));
                for (int i = 0; i < 4; i++)
                {
                    DataStream.AddRange(BitConverter.GetBytes(wheel_E.spare2[i]));
                }
                return DataStream.ToArray();
            }

            public byte[] Serialize(wheel wheel)
            {
                List<byte> DataStream = new List<byte>();
                byte[] realwheel_base = Serialize(wheel.wheel_O);
                DataStream.AddRange(realwheel_base);
                byte[] realwheel_ext = Serialize(wheel.wheel_E);
                DataStream.AddRange(realwheel_ext);
                return DataStream.ToArray();
            }

        }

        public class Packet
        {
            //public byte[] myArray = new byte[712];
            //public byte[][] b = new byte[712][];
            public byte[] myArray = new byte[472];
            public byte[][] b = new byte[472][];
            public Serialization serialization = new Serialization();
            public header_c C;
            public header_m[] _M = new header_m[4];
            public state State;
            public wheel_o[] Wheel_O = new wheel_o[4];

            public Packet(header_c C, header_m M0, header_m M1, state State, header_m M2, wheel_o[] wheel_O, header_m M3)
            {
                this.C = C;
                this._M[0] = M0;
                this._M[1] = M1;
                this._M[2] = M2;
                this.State = State;
                this.Wheel_O = wheel_O;
                this._M[3] = M3;
             }

            public Packet()
            {

            }

            public byte[] Combine(params byte[][] arrays)
            {
                int offset = 0;
                foreach (byte[] array in arrays)
                {
                    Buffer.BlockCopy(array, 0, myArray, offset, array.Length);
                    offset = offset + array.Length;
                }
                return myArray;
            }

            public byte[][] Split(byte[] DataStream, int[] lens)
            {
                int offset = 0;
                for (int i = 0; i < lens.Length; i++)
                {
                    b[i] = new byte[lens[i]];
                    Buffer.BlockCopy(DataStream, offset, b[i], 0, lens[i]);
                    offset = offset + lens[i];
                }
                return b;
            }

            public byte[] FormPacketArray(header_c header_C, header_m header_M0, header_m header_M1, state state, header_m header_M2, wheel_o[] wheel_O, header_m header_M3)
            {
                byte[] a1 = serialization.Serialize(header_C);
                byte[] a2 = serialization.Serialize(header_M0);
                byte[] a3 = serialization.Serialize(header_M1);
                byte[] a4 = serialization.Serialize(state);
                byte[] a5 = serialization.Serialize(header_M2);
                byte[] a6 = serialization.Serialize(wheel_O[0]);
                byte[] a7 = serialization.Serialize(wheel_O[1]);
                byte[] a8 = serialization.Serialize(wheel_O[2]);
                byte[] a9 = serialization.Serialize(wheel_O[3]);
                byte[] a10 = serialization.Serialize(header_M3);
                myArray = Combine(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);
                return myArray;
            }

            public Packet Parse(byte[] DataStream, int[] lens)
            {
                Packet pkt;
                wheel_o[] myWheel_O = new wheel_o[4];
                b = Split(DataStream, lens);
                header_c _header_C = new header_c(b[0]);
                header_m _header_M0 = new header_m(b[1]);
                header_m _header_M1 = new header_m(b[2]);
                state _state = new state(b[3]);
                header_m _header_M2 = new header_m(b[4]);
                myWheel_O[0] = new wheel_o(b[5]);
                myWheel_O[1] = new wheel_o(b[6]);
                myWheel_O[2] = new wheel_o(b[7]);
                myWheel_O[3] = new wheel_o(b[8]);
                header_m _header_M3 = new header_m(b[9]);
                pkt = new Packet(_header_C, _header_M0, _header_M1, _state, _header_M2, myWheel_O, _header_M3);
                return pkt;
            }

            public Packet Parse(byte[] DataStream)
            {
                Packet pkt;
                int offset = 0;
                int entry_hdr_size = 16;
                byte[] b = new byte[4096];
                byte[] filtered_packet = new byte[4096];
                byte[] wheel_pkg = new byte[512];
                byte[] state_pkg = new byte[512];
                wheel_o[] myWheel = new wheel_o[4];
                state _state = new state(new byte[1024]);
                header_m _header_M0 = new header_m(0, 0, 1, 0); // RDB_PKG_ID_START_OF_FRAME
                header_m _header_M1 = new header_m(0, 0, 0, 0); // Object state
                header_m _header_M2 = new header_m(0, 0, 0, 0); // Wheel frame
                header_m _header_M3 = new header_m(0, 0, 2, 0); // RDB_PKG_ID_END_OF_FRAME

                // Read main header
                header_c _header_C = new header_c(DataStream);

                // Move past header. Data segment is the first entry header.
                offset += (int)_header_C.headerSize;

                Console.WriteLine("Frame no: " + _header_C.frameNo);

                // Then check reamining sub packages
                while (offset < (int)_header_C.headerSize + (int)_header_C.dataSize)
                {
                    // Read entry header
                    Buffer.BlockCopy(DataStream, offset, b, 0, entry_hdr_size);
                    header_m entry_header = new header_m(b);


                    // Move ahead passed entry header, read data
                    offset += (int)entry_header.headerSize;
                    if (entry_header.dataSize > 0)
                    {
                        Buffer.BlockCopy(DataStream, offset, b, 0, (int)entry_header.dataSize);

                        if (entry_header.pkgId == 9)  // RDB_PKG_ID_OBJECT_STATE
                        {
                            _header_M1 = entry_header;
                            for (int i = 0; i < entry_header.dataSize / entry_header.elementSize; i++)
                            {
                                Buffer.BlockCopy(b, i * (int)entry_header.elementSize, state_pkg, 0, (int)entry_header.elementSize);
                                state state_tmp = new state(state_pkg);
                                Console.WriteLine("state :" + state_tmp.state_base.id);
                                if (state_tmp.state_base.id == 1)
                                {
                                    _state = state_tmp;
                                    Console.WriteLine("------Storing state pos (" +
                                        _state.state_base.pos.x + ", " +
                                        _state.state_base.pos.y + ", " +
                                        _state.state_base.pos.z + ")");
                                }
                            }
                        }
                        else if (entry_header.pkgId == 14)  // RDB_PKG_ID_WHEEL
                        {
                            _header_M2 = entry_header;
                            for (int i = 0; i < entry_header.dataSize / entry_header.elementSize; i++)
                            {
                                Buffer.BlockCopy(b, i * (int)entry_header.elementSize, wheel_pkg, 0, (int)entry_header.elementSize);
                                wheel_o tmp_wheel = new wheel_o(wheel_pkg);
                                Console.WriteLine("wheel: " + tmp_wheel.playerId);
                                if (tmp_wheel.playerId == 1)
                                {
                                    Console.WriteLine("------Storing wheel no " + i + " steering: " + tmp_wheel.steeringAngle);
                                    myWheel[tmp_wheel.id] = new wheel_o(wheel_pkg);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Discarded package id: " + entry_header.pkgId);
                        }
                        offset += (int)entry_header.dataSize;
                    }
                }

                if (_header_M1.pkgId == 0 | _header_M2.pkgId == 0)
                {
                    Console.WriteLine("Did not get start and/or end frame!\n");
                }
                pkt = new Packet(_header_C, _header_M0, _header_M1, _state, _header_M2, myWheel, _header_M3);
                return pkt;
            }
        }


        public class Catch
        {
            public static byte[] CatchPacket(double simTime, UInt32 id, geo geo, coord pos, coord speed, coord accel, UInt32 counter, wheel_o[] wheel_O)
            {
                Packet pkt = new Packet();
                header_m[] header_M = new header_m[4];
                header_c header_C;
                state_o state_O = new state_o(id, 1, 1, 0x6, hp.myname, geo, pos, 0, 0, 0);
                state_e state_E = new state_e(speed, accel, 498.55f, hp.spare);
                state state = new state(state_O, state_E);

                header_M[3] = new header_m(0, 0, 2, 0x0000);
                header_M[2] = new header_m(176, 44, 14, 0x0000);
                header_M[1] = new header_m(208, 208, 9, 0x1);
                header_M[0] = new header_m(0, 0, 1, 0x0000);
                header_C = new header_c(448, counter, simTime);
                byte[] stream = pkt.FormPacketArray(header_C, header_M[0], header_M[1], state, header_M[2], wheel_O, header_M[3]);
                return stream;
            }


        }


    }
}

