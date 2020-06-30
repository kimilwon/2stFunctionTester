using System;
using System.Runtime.InteropServices;
using MES;
//using NiCAN;
//using iniControl; 


namespace PowerSeat통합검사기
{
    public enum MENU
    {
        NONE,
        TESTING,
        SETTING,
        OPTION,
        PASSWORD,
        DATAVIEW,
        CONFIG,
    }

    public enum OperOnOff
    {
        OFF,
        ON,
        ALLON,
        ALLOFF
    }



    public enum RESULT
    {
        READY = 0,
        PASS = 1,
        NG = 2,
        REJECT = 2,
        END = 3,
        STOP = 4,
        CLEAR = 5,
        TEST = 6,
        NOT_TEST = 7
    };

    public class IO_IN
    {
        //public const short PASS = 0;
        //public const short RESET = 1;
        //public const short LH_SELECT = 2;
        //public const short AUTO = 3;
        //public const short OPTION_HEATER = 4;
        //public const short OPTION_VENT = 5;
        //public const short OPTION_BUCKLE_WARNING = 6;
        //public const short OPTION_RETRACTOR = 7;
        //public const short OPTION_SBR = 9;
        //public const short OPTION_PSEAT = 8;
        //public const short MANUAL_SBR_FWD = 10;
        //public const short MANUAL_SBR_LEFT = 11;
        //public const short MANUAL_SBR_1ST_DOWN = 12;
        //public const short MANUAL_SBR_2ST_DOWN = 13;
        //public const short CAN = 14;
        //public const short HEATER_LAMP_2WIRE = 15;
        public const short SBR_LEFT_SENSOR = 16;
        public const short SBR_RIGHT_SENSOR = 17;
        public const short SBR_FWD_SENSOR = 18;
        public const short SBR_BWD_SENSOR = 19;
        public const short SBR_2ST_UP1_SENSOR = 20;
        public const short SBR_2ST_UP2_SENSOR = 21;
        public const short SBR_2ST_DOWN1_SENSOR = 22;
        public const short SBR_2ST_DOWN2_SENSOR = 23;
        public const short SBR_1ST_UP_SENSOR = 24;
        public const short SBR_1ST_DOWN_SENSOR = 25;

        public const short SEAT_BELT = 26;
        public const short PRODUCT = 27;
        public const short JIG_UP = 28;
        //public const short MODEL_11P = 29;
        //public const short MODEL_9P = 29;
        //public const short MODEL_78P = 30;
        //public const short MODEL_RELAX = 31;
        public const short MAX = 27;

        public const short SERIAL = 32;
        public const short PASS = SERIAL + 0;
        public const short RESET = SERIAL + 1;
        public const short LH_SELECT = SERIAL + 2;
        public const short AUTO = SERIAL + 3;
        public const short MODEL_9P = SERIAL + 4;
        public const short MODEL_11P = SERIAL + 5;
        public const short MODEL_78P = SERIAL + 6;
        public const short MODEL_RELAX = SERIAL + 7;
        public const short SPARE = SERIAL + 8;
        public const short OPTION_HEATER = SERIAL + 9;
        public const short OPTION_VENT = SERIAL + 10;
        public const short CAN = SERIAL + 11;
        public const short OPTION_BUCKLE_WARNING = SERIAL + 12;
        public const short OPTION_RETRACTOR = SERIAL + 13;
        public const short OPTION_PSEAT = SERIAL + 14;
        public const short OPTION_SBR = SERIAL + 15;
        public const short MANUAL_SBR_FWD = SERIAL + 16;
        public const short MANUAL_SBR_BWD = SERIAL + 17;
        public const short MANUAL_SBR_LEFT = SERIAL + 18;
        public const short MANUAL_SBR_RIGHT = SERIAL + 19;
        public const short MANUAL_SBR_1ST_UP = SERIAL + 21;
        public const short MANUAL_SBR_1ST_DOWN = SERIAL + 20;
        public const short MANUAL_SBR_2ST_UP = SERIAL + 23;
        public const short MANUAL_SBR_2ST_DOWN = SERIAL + 22;
        public const short HEATER_LAMP_2WIRE = 15;
    };

    public enum __MODEL
    {
        NONE,
        MODEL_11P,
        MODEL_9P,
        MODEL_78P,
        MODEL_RELAX,
        MODEL_SPARE
    }

    public class IO_IN_FUNC
    {
        //public const short DRV_BATT = 0;
        //public const short PASS_BATT = 1;
        //public const short DRV_HEATER_BATT = 2;
        //public const short DRV_HEATER_GND = 3;
        //public const short DRV_VENT_BATT = 4;
        //public const short DRV_VENT_GND = 5;
        //public const short PASS_HEATER_BATT = 6;
        //public const short PASS_HEATER_GND = 7;
        //public const short PASS_VENT_BATT = 8;
        //public const short PASS_VENT_GND = 9;
        //public const short IGN1_BATT = 10;
        //public const short IGN1_GND = 11;
        //public const short IGN2_BATT = 12;
        //public const short IGN2_GND = 13;
        //public const short SP1_BATT = 14;
        //public const short SP1_GND = 15;
        //public const short SP2_BATT = 16;
        //public const short SP2_GND = 17;
        //public const short SP3_BATT = 18;
        //public const short SP3_GND = 19;
        //public const short DRV_LAMP_ACTIVE_LOW = 20;
        //public const short PASS_LAMP_ACTIVE_LOW = 21;
        //public const short SP4_BATT = 22;
        //public const short SP4_GND = 23;
        public const short DRV_HEATER_HIGH = 24;
        public const short DRV_HEATER_MID = 25;
        public const short DRV_HEATER_LOW = 26;
        public const short DRV_VENT_HIGH = 27;
        public const short DRV_VENT_MID = 28;
        public const short DRV_VENT_LOW = 29;
        public const short PASS_HEATER_HIGH = 30;
        public const short PASS_HEATER_MID = 31;
        public const short PASS_HEATER_LOW = 32;
        public const short PASS_VENT_HIGH = 33;
        public const short PASS_VENT_MID = 34;
        public const short PASS_VENT_LOW = 35;
        //public const short RESERVED = 36;
        public const short BUCKLE_WARNING = 37;        
        public const short MAX = 36;
    };

    public class LHRH
    {
        public const short LH = 1;
        public const short RH = 0;
    }


    public class IO_OUT
    {
        public const short RED = 0;
        public const short YELLOW = 1;
        public const short GREEN = 2;
        public const short BUZZER = 3;
        public const short PRODUCT = 4;
        public const short TEST_OK = 5;
        public const short ORG = 6;
        public const short TEST_ING = 7;        
        public const short SBR_LEFT = 8;
        public const short SBR_RIGHT = 9;
        public const short SBR_BWD = 10;

        //public const short PALLET_수평 = 11;
        public const short RETRACTOR_RESI_SELECT = 12;

        public const short SBR_1ST_DOWN = 16;
        public const short SBR_2ST_DOWN = 17;
        public const short RH_SELECT = 13;
        public const short MAX = 18;
        //----------------------- 시리얼 타입 카드에 출력할 경우
        public const short SERIAL = 32;
    };

    public class IO_OUT_FUNC
    {
        public const short DRV_BATT = 0;
        public const short PASS_BATT = 1;
        public const short DRV_HEATER_BATT = 2;
        public const short DRV_HEATER_GND = 3;
        public const short DRV_VENT_BATT = 4;
        public const short DRV_VENT_GND = 5;
        public const short PASS_HEATER_BATT = 6;
        public const short PASS_HEATER_GND = 7;
        public const short PASS_VENT_BATT = 8;
        public const short PASS_VENT_GND = 9;
        public const short IGN1_BATT = 10;
        public const short IGN1_GND = 11;
        public const short IGN2_BATT = 12;
        public const short IGN2_GND = 13;
        public const short SP1_BATT = 14;
        public const short SP1_GND = 15;
        public const short SP2_BATT = 16;
        public const short SP2_GND = 17;
        public const short SP3_BATT = 18;
        public const short SP3_GND = 19;
        public const short DRV_LAMP_ACTIVE_LOW = 20;
        public const short PASS_LAMP_ACTIVE_LOW = 21;
        public const short SP4_BATT = 22;
        public const short SP4_GND = 23;
        public const short PSEAT_BATT = 24;
        public const short MAX = 25;
    }

    public class ADPos
    {
        public const short MULTI_METER = 0;
        public const short BATT = 1;
        //public const short P_SEAT = 2;
        public const short HEATER = 2;
        public const short BUCKLE = 3;
    }

    public class PANELMETER_MESSAGE
    {
        public const short BATT = 0;
        //public const short P_SEAT = 1;
        public const short HEATER = 1;
        public const short BUCKLE = 2;
        public const short NORMAL = 3;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __LumberCheck__
    {
        public bool FW;
        public bool BW;
        public bool Up;
        public bool Dn;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __HeaterCheck__
    {
        public bool Curr;
        public bool Lamp;
        //public bool Cush;
        //public bool Back;
        //public bool NTC;
    }



    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __CheckItem__
    {
        public __HeaterCheck__ Heater;
        public __HeaterCheck__ Vent;
        public bool Retractor;
        public bool SBR;
        public bool BuckleWarning;
        public bool PowerSW;
        public bool CanCheck;
        public bool Lin;

        public bool SWHeater;
        public bool SWVent;
        public bool SWRetractor;
        public bool SWSBR;
        //public bool SWBuckleSensor;
        public bool SWBuckleWar;
        public bool LampTo2Wire;        
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __MinMax__
    {
        public double Min;
        public double Max;
    }

    public struct __Port__
    {
        public string Port;
        public int Speed;
    }

    //public struct __TCPIP__
    //{
    //    public string IP;
    //    public int Port;
    //}

    public struct __LinDevice__
    {
        public short Device;
        public int Speed;
    }
    public struct __CanDevice__
    {
        public short Device;
        public short Channel;
        public short ID;
        public int Speed;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __Config__
    {
        //public __Port__ MultiMeter;
        public __Port__ PanelMeter;
        public __Port__ SerialtypeIOCard;
        public __Port__ Power;

        public __TCPIP__ Board;
        public __TCPIP__ PC;
        public __TCPIP__ Server;
        public __TCPIP__ Client;

        public __LinDevice__ Lin1;
        public __LinDevice__ Lin2;
        public __CanDevice__ Can1;
        //public __CanDevice__ Can2;

        public int BattID;
        public int BuckleID;
        public int HeaterID;
    }

    public struct __Counter__
    {
        public int Total;
        public int OK;
        public int NG;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct __Infor__
    {
        public string DataName;
        public string Model;
        public string Date;
        public __Counter__ Count;
        public bool ReBootingFlag;
    }
    /*
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CanBuffer
    {
        public int status;
        public NCTYPE_CAN_FRAME_TIMED Frames;
    }
    */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct __IMS__
    {
        public int Set;
        public int M1;
        public int M2;
        public int M3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct __CurrentCheckPos__
    {
        public int Pos;
        public int Time;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct __Offset__
    {
        //public bool CanType; //false Low , true High
        public bool IGN; //false - active high, true - active low
        public bool Button; //false - active high, true - active low
        public bool DrvLamp;//false - active high, true - active low
        public bool AssistLamp;//false - active high, true - active low

        public float HeaterLH;
        public float HeaterRH;
        public float Vent;
        //public float Airbag;
        public float BuckleSensor;
        public float Retractor;
        //public float HeaterRear;
    }

    public struct __SbrSpec__
    {
        public float NotLoad;
        public float Load15Kg;
        public float Load30Kg;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct __Spec__
    {
        public string CarName;
        public __MinMax__ Heater; //희터        
        public __MinMax__ Vent; //통풍
        public __MinMax__ VentHeater; //통풍희터
        //public __MinMax__ LHHeater; //통풍희터

        //public __MinMax__ BuckleWaring; //버클 센서
        public __MinMax__ Retractor;
        //public __MinMax__ Lumber;
        public __MinMax__ PWSwitch;
        public __SbrSpec__ SBR;


        //public __MinMax__ LHFan;
        //public __MinMax__ NTC;
        //public __MinMax__ BwrSpeed;

        //public __IMS__ IMS;
        public int SWCheckTime;

        public __CurrentCheckPos__ HeaterCheckPos;
        //public __CurrentCheckPos__ HeaterCheckPos2;
        public __CurrentCheckPos__ VentCheckPos;

        public __Offset__ Offset;
        public float Volt;
        public float SWOffCurr;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct __DataItem__
    {
        public bool Test;
        public float Data;
        public RESULT Result;
    }

    public struct __SBRDataItem__
    {
        public bool Test;
        public float NotLoadData;
        public float Load15KData;
        public float Load30KData;

        public RESULT ResultNotLoad;
        public RESULT Result15Kg;
        public RESULT Result30Kg;

        public RESULT Result;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct __DataItem2__
    {
        public bool Test;
        public float Data;
        public RESULT[] Result;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct __Data__
    {
        public __DataItem__ HeaterCurrent;
        public __DataItem2__ HeaterLamp;
        public __DataItem__ Retractor;

        public __DataItem__ VentCurr;
        public __DataItem2__ VentLamp;
        public __SBRDataItem__ SBR;
        public __DataItem__ BuckleWarning;


        public __DataItem__ PW_Recline;
        public __DataItem__ PW_Relax;
        public __DataItem__ PW_Legrest;
        public __DataItem__ PW_LegrestExt;

        public RESULT Result;
    }


    public struct __CanOutPos
    {
        /// <summary>
        /// 데이타
        /// </summary>
        public byte Data;
        /// <summary>
        /// 초기화 데이타 
        /// </summary>
        public byte Mask;
        //시작 위치
        public short Byte;
        /// <summary>
        /// 비트 위치
        /// </summary>
        public short Pos;
        /// <summary>
        /// Lin FID/PID
        /// </summary>
        public int ID;
    }

    public struct __CanOutMessage
    {
        /// <summary>
        /// 데이타
        /// </summary>
        public byte[] Data;
        /// <summary>
        /// Lin FID/PID
        /// </summary>
        public int ID;
    }

    public struct __CanInPos
    {
        /// <summary>
        /// 데이타
        /// </summary>
        public byte Length;
        /// <summary>
        /// 초기화 데이타 
        /// </summary>
        public byte Mask;
        //시작 위치
        public short Byte;
        /// <summary>
        /// 비트 위치
        /// </summary>
        public short Pos;
        /// <summary>
        /// Lin FID/PID
        /// </summary>
        public int ID;
    }


    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]    
    public struct __CanData__
    {
        public short Data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] Title; //[50];
    };

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __ItemCan__
    {
        public short StartBit;
        public short Size;
        public short Mode; // 0이면 일반데이타가 1 이면 숫치 데이타가 들어가는 항목임 2 이면 아스키 데이타가 들어간다.
        public short DataCounter;
        public short Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] Title; //[50];
        public int CanID;
        public short S_ID;
        public short ReceiveTime; // 전송 간격을 갖는다.
        public bool CanLin; // true can, false Lin
        public bool InOut; // true 이면 output mode

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 40)]
        public __CanData__[] Data;//[40];
    }

    public struct __Can__
    {
        public short ItemCounter;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 700)]
        public __ItemCan__[] Item; // [700]
    };
    public struct __sCan__
    {
        public bool Run;
        public short sID;
        public short dBit;
    }

    public struct __CanMsg
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] DATA;// [8];
        public int Length;
        public int ID;
    }


    public struct __SendCan__
    {
        public int ID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Data; //[8]
        public int Length;
        public long first;
        public long last;
        public long sendtime;
        //public byte AliveCnt;
    }

    public struct __InOutCanMsg__
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public __SendCan__[] Send; // [20]
        public int Max;
    }

    public struct CanInOutStruct
    {
        public __InOutCanMsg__ Can;
    }
    public struct LinInOutStruct
    {
        public __InOutCanMsg__ Lin;
    }

    public struct __InOutCan__
    {
        public CanInOutStruct In;
        public CanInOutStruct Out;
    }
    public struct __InOutLin__
    {
        public LinInOutStruct In;
        public LinInOutStruct Out;
    }

    public struct __CanLin__
    {
        public __InOutCan__ Can1;
        //public __InOutCan__ Can2;
        public __InOutLin__ Lin1;
        public __InOutLin__ Lin2;
    }

}