using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Runtime.InteropServices;

namespace NiCAN
{
    public delegate int CanDataRxEvent(uint ObjHandle, UInt32 State, int Status, int RefData);

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NCTYPE_CAN_FRAME      // size of structure is 14 bytes(4,2,8)
    {
        public uint ArbitrationId;  //NCTYPE_CAN_ARBID
        public byte IsRemote;       //NCTYPE_BOOL
        public byte DataLength;     //NCTYPE_UINT8, length of Data in bytes
		//[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte Data0, Data1, Data2, Data3, Data4, Data5, Data6, Data7;          //NCTYPE_UINT8
    }

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NCTYPE_CAN_STRUCT     // size of structure is 22 bytes (8,4,2,8)
    {
        public ulong TimeStamp;      //NCTYPE_ABS_TIME, which is NCTYPE_UINT64, 8 bytes
        public uint ArbitrationId;   //NCTYPE_CAN_ARBID, which is NCTYPE_UINT32, 4 bytes
        public byte FrameType;       //NCTYPE_UINT8,   RTR or DATA
        public byte DataLength;      //NCTYPE_UINT8
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Data;          //NCTYPE_UINT8
    }

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NCTYPE_CAN_FRAME_TIMED
    {
        public UInt64 Timestamp;
        public UInt32 ArbitrationId;
        public Byte IsRemote;
        public Byte DataLength;
	//	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte Data0, Data1, Data2, Data3, Data4, Data5, Data6, Data7;
	}
    public struct NC_Queue
    {
        public int status;
        public NCTYPE_CAN_FRAME_TIMED Frames;
    }

    partial class NiCanFunc
    {
        public const uint NC_ST_READ_AVAIL =        0x00000001;
        public const uint NC_ST_ERROR = 0x00000010;

        public const uint NC_ATTR_ABS_TIME = (uint)(0x80000008);
        public const uint NC_ATTR_BAUD_RATE = (uint)(0x80000007);
        public const uint NC_ATTR_BEHAV_FINAL_OUT = (uint)(0x80010018);

        public const uint NC_ATTR_BKD_CAN_RESPONSE = (uint)(0x80010006);
        public const uint NC_ATTR_BKD_CHANGES_ONLY = (uint)(0x80000015);
        public const uint NC_ATTR_BKD_PERIOD = (uint)(0x8000000F);
        public const uint NC_ATTR_BKD_READ_SIZE = (uint)(0x8000000B);
        public const uint NC_ATTR_BKD_TYPE = (uint)(0x8000000D);
        public const uint NC_ATTR_BKD_WHEN_USED = (uint)(0x8000000E);
        public const uint NC_ATTR_BKD_WRITE_SIZE = (uint)(0x8000000C);

        public const uint NC_ATTR_CAN_BIT_TIMINGS = (uint)(0x80010005);
        public const uint NC_ATTR_CAN_COMP_STD = (uint)(0x80010001);
        public const uint NC_ATTR_CAN_COMP_XTD = (uint)(0x80010003);
        public const uint NC_ATTR_CAN_DATA_LENGTH = (uint)(0x80010007);
        public const uint NC_ATTR_CAN_MASK_STD = (uint)(0x80010002);
        public const uint NC_ATTR_CAN_MASK_XTD = (uint)(0x80010004);
        public const uint NC_ATTR_CAN_TX_RESPONSE = (uint)(0x80010006);

        public const uint NC_ATTR_COMM_TYPE = (uint)(0x80000016);
        public const uint NC_ATTR_COMP_STD = (uint)(0x80010001);
        public const uint NC_ATTR_COMP_XTD = (uint)(0x80010003);
        public const uint NC_ATTR_DATA_LEN = (uint)(0x80010007);
        public const uint NC_ATTR_HW_FORMFACTOR = (uint)(0x80020004);     // Formfactor of card - NC_HW_FORMFACTOR_???
        public const uint NC_ATTR_HW_SERIAL_NUM = (uint)(0x80020003);     // Serial Number of card
        public const uint NC_ATTR_HW_SERIES = (uint)(0x80020005);     // Series of Card - NC_HW_SERIES_???
        public const uint NC_ATTR_HW_TRANSCEIVER = NC_ATTR_TRANSCEIVER_TYPE; // NC_HW_TRANSCEIVER_???
        public const uint NC_ATTR_INTERFACE_NUM = (uint)(0x80020008);     // 0 for CAN0, 1 for CAN1, etc...
        public const uint NC_ATTR_IS_NET_SYNC = (uint)(0x8001000E);
        public const uint NC_ATTR_LIN_CHECKSUM_TYPE = (uint)(0x80020043);
        public const uint NC_ATTR_LIN_ENABLE_DLC_CHECK = (uint)(0x80020045);
        public const uint NC_ATTR_LIN_LOG_WAKEUP = (uint)(0x80020046);
        public const uint NC_ATTR_LIN_RESPONSE_TIMEOUT = (uint)(0x80020044);
        public const uint NC_ATTR_LIN_SLEEP = (uint)(0x80020042);
        public const uint NC_ATTR_LISTEN_ONLY = (uint)(0x80010010);
        public const uint NC_ATTR_LOG_BUS_ERROR = (uint)(0x80020037);
        public const uint NC_ATTR_LOG_COMM_ERRS = (uint)(0x8001000A);
        public const uint NC_ATTR_LOG_START_TRIGGER = (uint)(0x80020031);
        public const uint NC_ATTR_LOG_TRANSCEIVER_FAULT = (uint)(0x80020038);
        public const uint NC_ATTR_MASK_STD = (uint)(0x80010002);
        public const uint NC_ATTR_MASK_XTD = (uint)(0x80010004);
        public const uint NC_ATTR_MASTER_TIMEBASE_RATE = (uint)(0x80020033);
        public const uint NC_ATTR_NET_SYNC_COUNT = (uint)(0x8001000D);
        public const uint NC_ATTR_NOTIFY_MULT_LEN = (uint)(0x8001000B);
        public const uint NC_ATTR_NOTIFY_MULT_SIZE = (uint)(0x8001000B);
        public const uint NC_ATTR_NUM_CARDS = (uint)(0x80020002);     // Number of Cards present in system.
        public const uint NC_ATTR_NUM_PORTS = (uint)(0x80020006);     // Number of Ports present on card
        public const uint NC_ATTR_PERIOD = (uint)(0x8000000F);
        public const uint NC_ATTR_PROTOCOL = (uint)(0x80000001);
        public const uint NC_ATTR_PROTOCOL_VERSION = (uint)(0x80000002);
        public const uint NC_ATTR_READ_PENDING = (uint)(0x80000011);
        public const uint NC_ATTR_READ_Q_LEN = (uint)(0x80000013);
        public const uint NC_ATTR_RESET_ON_START = (uint)(0x80010008);
        public const uint NC_ATTR_RTSI_FRAME = (uint)(0x80000020);
        public const uint NC_ATTR_RTSI_MODE = (uint)(0x80000017);
        public const uint NC_ATTR_RTSI_SIG_BEHAV = (uint)(0x80000019);
        public const uint NC_ATTR_RTSI_SIGNAL = (uint)(0x80000018);
        public const uint NC_ATTR_RTSI_SKIP = (uint)(0x80000021);
        public const uint NC_ATTR_RX_CHANGES_ONLY = (uint)(0x80000015);
        public const uint NC_ATTR_RX_ERROR_COUNTER = (uint)(0x80010011);
        public const uint NC_ATTR_RX_Q_LEN = (uint)(0x8001000C);
        public const uint NC_ATTR_SELF_RECEPTION = (uint)(0x80010016);
        public const uint NC_ATTR_SERIAL_NUMBER = (uint)(0x800000A0);
        public const uint NC_ATTR_SERIES2_COMP = (uint)(0x80010013);
        public const uint NC_ATTR_SERIES2_ERR_ARB_CAPTURE = (uint)(0x8001001C);
        public const uint NC_ATTR_SERIES2_FILTER_MODE = (uint)(0x80010015);
        public const uint NC_ATTR_SERIES2_MASK = (uint)(0x80010014);
        public const uint NC_ATTR_SINGLE_SHOT_TX = (uint)(0x80010017);
        public const uint NC_ATTR_SOFTWARE_VERSION = (uint)(0x80000003);
        public const uint NC_ATTR_START_ON_OPEN = (uint)(0x80000006);
        public const uint NC_ATTR_START_TRIG_BEHAVIOR = (uint)(0x80010023);
        public const uint NC_ATTR_STATE = (uint)(0x80000009);
        public const uint NC_ATTR_STATUS = (uint)(0x8000000A);
        public const uint NC_ATTR_TERMINATION = (uint)(0x80020041);
        public const uint NC_ATTR_TIMELINE_RECOVERY = (uint)(0x80020035);
        public const uint NC_ATTR_TIMESTAMP_FORMAT = (uint)(0x80020032);
        public const uint NC_ATTR_TIMESTAMPING = (uint)(0x80000010);
        public const uint NC_ATTR_TRANSCEIVER_EXTERNAL_IN = (uint)(0x8001001B);
        public const uint NC_ATTR_TRANSCEIVER_EXTERNAL_OUT = (uint)(0x8001001A);
        public const uint NC_ATTR_TRANSCEIVER_MODE = (uint)(0x80010019);
        public const uint NC_ATTR_TRANSCEIVER_TYPE = (uint)(0x80020007);
        public const uint NC_ATTR_TRANSMIT_MODE = (uint)(0x80020029);
        public const uint NC_ATTR_TX_ERROR_COUNTER = (uint)(0x80010012);
        public const uint NC_ATTR_TX_RESPONSE = (uint)(0x80010006);
        public const uint NC_ATTR_VERSION_BUILD = (uint)(0x8002000D);     // U32 build (primarily useful for beta)
        public const uint NC_ATTR_VERSION_COMMENT = (uint)(0x8002000E);     // String comment on version (max 80 chars)
        public const uint NC_ATTR_VERSION_MAJOR = (uint)(0x80020009);     // U32 major version (X in X.Y.Z)
        public const uint NC_ATTR_VERSION_MINOR = (uint)(0x8002000A);     // U32 minor version (Y in X.Y.Z)
        public const uint NC_ATTR_VERSION_PHASE = (uint)(0x8002000C);     // U32 phase (1=alpha, 2=beta, 3=release)
        public const uint NC_ATTR_VERSION_UPDATE = (uint)(0x8002000B);     // U32 minor version (Z in X.Y.Z)
        public const uint NC_ATTR_VIRTUAL_BUS_TIMING = (uint)(0xA0000031);
        public const uint NC_ATTR_WRITE_ENTRIES_FREE = (uint)(0x80020034);
        public const uint NC_ATTR_WRITE_PENDING = (uint)(0x80000012);
        public const uint NC_ATTR_WRITE_Q_LEN = (uint)(0x80000014);

        public const uint NC_BKD_TYPE_PEER2PEER = (uint)(0x00000001);
        public const uint NC_BKD_TYPE_REQUEST = (uint)(0x00000002);
        public const uint NC_BKD_TYPE_RESPONSE = (uint)(0x00000003);
        public const uint NC_BKD_WHEN_PERIODIC = (uint)(0x00000001);
        public const uint NC_BKD_WHEN_UNSOLICITED = (uint)(0x00000002);
        public const uint NC_BKD_CAN_ZERO_SIZE = (uint)(0x00008000);

        public const uint NC_MASK_STD_DONTCARE = (uint)(0x00000000);     // recommended for Series 2
        public const uint NC_MASK_XTD_DONTCARE = (uint)(0x00000000);     // recommended for Series 2

        public const uint NC_CAN_MASK_STD_DONTCARE = NC_MASK_STD_DONTCARE;
        public const uint NC_CAN_MASK_XTD_DONTCARE = NC_MASK_XTD_DONTCARE;

        [DllImport("Nican.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int ncCloseObject(uint ObjHandle);
        [DllImport("Nican.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int ncConfig(
                                string ObjName,      //NCTYPE_STRING
                                uint NumAttrs,              //NCTYPE_UINT32
                                uint[] AttrIdList,      //NCTYPE_ATTRID_P
                                uint[] AttrValueList    //NCTYPE_UINT32_P
                                );
        [DllImport("Nican.dll" /*, CallingConvention = CallingConvention.Cdecl*/)]
        public static extern int ncOpenObject(
                                string ObjName,  //NCTYPE_STRING
                                ref uint ObjHandle      //NCTYPE_OBJH_P
                                );
        [DllImport("Nican.dll" /*, CallingConvention = CallingConvention.Cdecl*/)]
        public static extern int ncRead(
                                uint ObjHandle, //NCTYPE_OBJH
                                uint SizeofData,  //NCTYPE_UINT32
                                ref NCTYPE_CAN_FRAME_TIMED Data    // IntPtr Data    //NCTYPE_ANY_P
                                );
        [DllImport("Nican.dll" /*, CallingConvention = CallingConvention.Cdecl*/)]
        public static extern int ncReadMult(
                                uint ObjHandle, //NCTYPE_OBJH
                                uint SizeofData,  //NCTYPE_UINT32
                                IntPtr Data,   //NCTYPE_ANY_P,  IntPtr? ref NCTYPE_CAN_STRUCT[] Data?-conflict bw managed an unmanaged operations
                                ref uint ActualDataSize //NCTYPE_UINT32_P
                                );
        [DllImport("Nican.dll" /*, CallingConvention = CallingConvention.Cdecl*/)]
        public static extern int ncWrite(
                                    uint ObjHandle,     //NCTYPE_OBJH
                                    uint SizeofData,    //NCTYPE_UINT32
                                    ref NCTYPE_CAN_FRAME Data       //NCTYPE_ANY_P 
                                    );
        [DllImport("Nican.dll" /*, CallingConvention = CallingConvention.Cdecl*/)]
        public static extern int ncWriteMult(
                                    uint ObjHandle, //NCTYPE_OBJH
                                    uint SizeofData,  //NCTYPE_UINT32
                                    IntPtr FrameArray    //NCTYPE_ANY_P or NCTYPE_CAN_STRUCT_P
                                    );

        [DllImport("Nican.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int ncCreateNotification(
                           uint ObjHandle,
                           UInt32 DesiredState,
                           UInt32 Timeout,
                           UInt32 RefData,
                           CanDataRxEvent Callback);

    }
}
