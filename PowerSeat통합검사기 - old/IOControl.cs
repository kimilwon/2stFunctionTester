#define PROGRAM_RUNNING
#define THREAD_RUN

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.IO.Ports;
using MES;


namespace PowerSeat통합검사기
{
    public class IOControl
    {
        private MyInterface mControl = null;
        private IPEndPoint ep;
        private Socket server;
        private EndPoint remoteEP;
        private EndPoint remoteEP2;
        private byte[] rBuffer1 = new byte[1024];
        private __TCPIP__ Board;
        private __TCPIP__ PC;

        private SerialPort IOPortToSerial = null;
        private ulong[] InData = { 0x0000000000000000, 0x0000000000000000, 0x0000000000000000 };
        private ulong[] InData2 = { 0x0000000000000000, 0x0000000000000000, 0x0000000000000000 };
        private byte[,] OutData = { { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
        private ushort OutData2 = 0x0000;
        private ushort OutDataOld2 = 0x0000;
        private float CurrData = 0x00;
        private  System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();
        
#if !THREAD_RUN
        private Timer timer1 = new Timer();
#endif
        public IOControl()
        {
        }
        public IOControl(MyInterface mControl, __TCPIP__ Board, __TCPIP__ PC)
        {
            this.PC = PC;
            this.Board = Board;
#if !THREAD_RUN
            timer1.Interval = 10;
            timer1.Tick += timer1_tick;
#endif
            this.mControl = mControl;
            //timer1.Enabled = true;
        }

        public void Open()
        {
            if (UdpOpen() == false)
            {
                MessageBox.Show("마이컴 제어용 통신 포트를 오픈하지 못했습니다.");
            }
            else
            {
                UdpCanCommunicationInit();
                UdpRead();
#if !THREAD_RUN
                timer1.Enabled = true;
#else
                ThreadSetting();
#endif
            }
        }

        public void Open(string sPort = null, int Speed = 0)
        {
            if (sPort != null)
            {
                IOPortToSerial = new SerialPort()
                {
                    BaudRate = 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    PortName = sPort
                };
                IOPortToSerial.DataReceived += new SerialDataReceivedEventHandler(IOPortReceive);
                IOPortToSerial.Open();
            }
            

            timer2.Interval = 50;
            timer2.Tick += new EventHandler(time2_tick);
            timer2.Enabled = true;


            if (UdpOpen() == false)
            {
                MessageBox.Show("마이컴 제어용 통신 포트를 오픈하지 못했습니다.");
            }
            else
            {
                UdpCanCommunicationInit();
                UdpRead();
#if !THREAD_RUN
                timer1.Enabled = true;
#else
                ThreadSetting();
#endif
            }
        }


        private bool UdpOpen()
        {
            bool Flag;
            string strIP;
            int port1;

            Flag = false;
            if ((PC.IP != "") && (PC.IP != null))
            {
                //종점 생성
                strIP = PC.IP;
                IPAddress ip = IPAddress.Parse(strIP);
                port1 = PC.Port;
                ep = new IPEndPoint(ip, port1);

                server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                try
                {
                    server.Bind(ep);
                    isOpen = false;
                    isConnection = false;

                    remoteEP = (EndPoint)new IPEndPoint(ip, port1);

                    if ((Board.IP != "") && (PC.IP != null))
                    {
                        remoteEP2 = (EndPoint)new IPEndPoint(IPAddress.Parse(Board.IP), Board.Port);
                        isOpen = true;
                        isConnection = true;
                        Flag = isOpen;
                    }
                }
                catch// (Exception exp)                
                {
                    //MessageBox.Show("I/O Card 와 Bind 가 되지 않습니다. (이더넷 케이블 확인)");
                    isOpen = false;
                    isConnection = false;
                }
                finally
                {
                    Flag = isOpen;
                }
                server.ReceiveBufferSize = 4096;
                server.SendBufferSize = 4096;
            }

            //int port2 = Config.Board.Port;

            //UDP Socket 생성


            return Flag;
        }

        public ulong[] GetInData
        {
            get { return InData; }
        }

        public ulong[] GetInDataToSerial
        {
            get { return InData2; }
        }

        public bool isOpen { get; set; }
        //{
        //    get { return isOpen; }
        //}

        public bool isConnection { get; set; }
        //{
        //    get { return isConnection; }
        //}
        public void UdpClose()
        {
            //소켓 닫기
#if PROGRAM_RUNNING
            if (isOpen == true) server.Close();
            isOpen = false;
#endif

            return;
        }


        private void UdpWrite(int addr, byte[] Data, int Length)
        {
            //데이터 입력
#if PROGRAM_RUNNING
            int SendLength;
            //인코딩(byte[])
            byte[] sBuffer = new byte[100];
            //byte[] sBuffer = Encoding.UTF8.GetBytes(data); // data 가 string 이어야 한다.

            //보내기            

            //sBuffer = Encoding.UTF8.GetBytes(data);// data 가 string 이어야 한다.

            /*
            union_r r = new union_r();

            r.Addr = addr;

            sBuffer[0] = r.c1;
            sBuffer[1] = r.c2;
            sBuffer[2] = r.c3;
            sBuffer[3] = r.c4;
            */
            SendLength = 0;
            sBuffer[SendLength++] = (byte)((addr & 0xff000000) >> 24);
            sBuffer[SendLength++] = (byte)((addr & 0x00ff0000) >> 16);
            sBuffer[SendLength++] = (byte)((addr & 0x0000ff00) >> 8);
            sBuffer[SendLength++] = (byte)((addr & 0x000000ff) >> 0);
            sBuffer[SendLength++] = (byte)Length;

            for (int i = 0; i < Length; i++) sBuffer[SendLength++] = Data[i];

            //if ((isOpen[Ch] == true) && (isConnection[Ch] == true))
            if (isOpen == true)
            {
                try
                {
                    server.SendTo(sBuffer, SendLength, SocketFlags.DontRoute, remoteEP2);
                }
                catch
                {
                }
                finally
                {
                }
            }
#endif
            return;
        }
        private void UpdWrite2()
        {
            byte[] Data1 = { OutData[1, 0], OutData[1, 1], OutData[1, 2], OutData[1, 3], OutData[1, 4], OutData[1, 5], OutData[1, 6], OutData[1, 7] };
            byte[] Data2 = { OutData[0, 0], OutData[0, 1], OutData[0, 2], OutData[0, 3], OutData[0, 4], OutData[0, 5], OutData[0, 6], OutData[0, 7] };
            UdpWrite(0x151, Data1, 8);
            UdpWrite(0x101, Data2, 8);
            OutPos = 2;
            OutPosFirst = mControl.공용함수.timeGetTimems();
            OutPosLast = mControl.공용함수.timeGetTimems();
            return;
        }



        public void UdpCanCommunicationInit()
        {
#if PROGRAM_RUNNING
            byte[] sBuffer = { 0xfc, 0x03, 0xff };
            if (remoteEP2 != null) server.SendTo(sBuffer, 3, SocketFlags.DontRoute, remoteEP2);
            mControl.공용함수.timedelay(10);
            if (remoteEP2 != null) server.SendTo(sBuffer, 3, SocketFlags.DontRoute, remoteEP2);
            mControl.공용함수.timedelay(10);
            if (remoteEP2 != null) server.SendTo(sBuffer, 3, SocketFlags.DontRoute, remoteEP2);
#endif
            return;
        }

        private string UdpRead()
        {
            //데이터 받기
            string result = "";

            try
            {
#if PROGRAM_RUNNING
                server.BeginReceiveFrom(rBuffer1, 0, rBuffer1.Length, SocketFlags.None, ref remoteEP, new AsyncCallback(ReceiveUdp), remoteEP);
#endif
            }
            catch
            {
            }
            finally
            {
            }
            return result;
        }

        private void ReceiveUdp(IAsyncResult _AR)
        {
            try
            {
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                // 클라이언트로부터 메시지를 받는다.            
                isConnection = true;
                int ReceivedSize = server.EndReceiveFrom(_AR, ref remoteEP);

                if (0 < ReceivedSize)
                {
                    if (mControl.isExit == false)
                    {
                        //CheckUdpData(rBuffer1, 0, ReceivedSize);                                                
                        CheckUdpData(rBuffer1, ReceivedSize);
                    }
                }
                UdpComCheckFirst = mControl.공용함수.timeGetTimems();
                UdpComCheckLast = mControl.공용함수.timeGetTimems();
                //ReceivedSize[0] = 0;
                server.BeginReceiveFrom(rBuffer1, 0, rBuffer1.Length, SocketFlags.None, ref remoteEP, new AsyncCallback(ReceiveUdp), remoteEP);

            }
            catch// (Exception exp)
            {
                //throw exp;
                //MessageBox.Show(exp.Message);                    
            }

            return;
        }

        //private long UdpFirst = 0;
        //private long UdpLast = 0;
        //private bool isOpen = false;
        //private bool isConnection = false;

        private void CheckUdpData(byte[] data, int Length)
        {
            try
            {
                //this.Text = data;
                if (0 < Length)
                {
                    int CanID = 0;
                    float Ad;
                    ushort x = 0;
                    int DataLength;
                    ulong[] cData = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                    CanID = (int)(((data[0] & 0xff) << 24) & 0xff000000);
                    CanID |= (int)(((data[1] & 0xff) << 16) & 0x00ff0000);
                    CanID |= (int)(((data[2] & 0xff) << 8) & 0x0000ff00);
                    CanID |= (int)(((data[3] & 0xff) << 0) & 0x000000ff);
                    DataLength = data[4];

                    for (int i = 0; i < DataLength; i++)
                    {
                        if (i < 8)
                            cData[i] = data[i + 5];
                        else break;
                    }

                    switch (CanID)
                    {
                        case 0x150: // Product In
                            InData[0] = (cData[4] << 0) | (cData[5] << 8) | (cData[6] << 16) | (cData[7] << 24);
                            InData[2] = (cData[0] << 0) | (cData[1] << 8) | (cData[2] << 16) | (cData[3] << 24);

                            break;
                        case 0x100: // 3232 In                            
                            InData[1] = (cData[4] << 0) | (cData[5] << 8) | (cData[6] << 16) | (cData[7] << 24);

                            //string s = string.Format("{0:X} {1:X} {2:X} {3:X} ", cData[4], cData[5], cData[6], cData[7]);
                            //this.Text = s;
                            break;
                        case 0x160:
                            x = (ushort)((cData[1] << 0) | (cData[2] << 8));

                            Ad = (float)(x * (5.0 / 4096.0));
                            //0 V일때 2.6V 가 뜬다고 함

                            if (2.6 <= CurrData)
                            {
                                Ad = (float)((CurrData - 2.6) * (30.0 / 2.4)); //2.5V 30A
                            }
                            else
                            {
                                Ad = (float)((2.6 - CurrData) * (-30.0 / 2.4)); //2.5V 30A
                            }
                            CurrData = Ad;
                            break;
                    }

                    //Application.DoEvents() 를 사용하면 에러 발생 (이 함수고 Callback 으로 호출되어서 그런다.
                    //Application.DoEvents();
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message + "\n" + exp.StackTrace);
            }

            return;
        }



        public void time2_tick(object sender, EventArgs e)
        {
            try
            {
                timer2.Enabled = false;

                if (IOPortToSerial != null)
                {
                    if (IOPortToSerial.IsOpen == true)
                    {
                        //if (OutData2 != OutDataOld2)
                        //{
                        //    SerialPortOut();
                        //}
                        SerialPortOut();
                    }
                }
            }
            catch
            {
            }
            finally
            {
                timer2.Enabled = true;
            }
            return;
        }

        private void SerialPortOut()
        {
            Int64 temp_data;

            int temp_data3;
            int crc16;

            int temp_byte;

            byte[] temp_data2 = new byte[8];
            byte[] temp_data4 = new byte[35];

            temp_data = (long)OutData2;

            temp_data3 = (int)(temp_data) & 0xff;
            temp_data2[0] = (byte)temp_data3;

            temp_data3 = (int)(temp_data >> 8) & 0xff;
            temp_data2[1] = (byte)temp_data3;

            temp_data3 = (int)(temp_data >> 16) & 0xff;
            temp_data2[2] = (byte)temp_data3;

            temp_data3 = (int)(temp_data >> 32) & 0xff;
            temp_data2[3] = (byte)temp_data3;


            temp_byte = (byte)temp_data2[0] >> 4;
            temp_data4[0] = hex2ascii((byte)temp_byte);

            temp_byte = (byte)temp_data2[0] & 0xf;
            temp_data4[1] = hex2ascii((byte)temp_byte);

            temp_byte = (byte)temp_data2[1] >> 4;
            temp_data4[2] = hex2ascii((byte)temp_byte);

            temp_byte = (byte)temp_data2[1] & 0xf;
            temp_data4[3] = hex2ascii((byte)temp_byte);

            temp_byte = (byte)temp_data2[2] >> 4;
            temp_data4[4] = hex2ascii((byte)temp_byte);

            temp_byte = (byte)temp_data2[2] & 0xf;
            temp_data4[5] = hex2ascii((byte)temp_byte);

            temp_byte = (byte)temp_data2[3] >> 4;
            temp_data4[6] = hex2ascii((byte)temp_byte);

            temp_byte = (byte)temp_data2[3] & 0xf;
            temp_data4[7] = hex2ascii((byte)temp_byte);

            crc16 = CRC16(temp_data4, 7);

            temp_byte = (byte)(crc16 >> 12) & 0xf;
            temp_data4[8] = hex2ascii((byte)temp_byte);

            temp_byte = (byte)(crc16 >> 8) & 0xf;
            temp_data4[9] = hex2ascii((byte)temp_byte);

            temp_byte = (byte)(crc16 >> 4) & 0xf;
            temp_data4[10] = hex2ascii((byte)temp_byte);

            temp_byte = (byte)crc16 & 0xf;
            temp_data4[11] = hex2ascii((byte)temp_byte);

            if (IOPortToSerial.IsOpen)
            {
                IOPortToSerial.Write(temp_data4, 0, 12);
            }
            OutDataOld2 = OutData2;
            return;
        }


        private void IOPortReceive(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                mControl.공용함수.timedelay(30);
                int Length = IOPortToSerial.BytesToRead;
                byte[] buffer = new byte[Length + 10];

                IOPortToSerial.Read(buffer, 0, Length);


                int temp;
                int crc16;
                int crc16_c;
                byte[] buff2_buf = new byte[53];

                if (Length == 16)
                {

                    temp = Ascii2Hex(buffer[0]) << 4 | Ascii2Hex(buffer[1]);
                    buff2_buf[0] = (byte)temp;

                    temp = Ascii2Hex(buffer[2]) << 4 | Ascii2Hex(buffer[3]);
                    buff2_buf[1] = (byte)temp;

                    temp = Ascii2Hex(buffer[4]) << 4 | Ascii2Hex(buffer[5]);
                    buff2_buf[2] = (byte)temp;

                    temp = Ascii2Hex(buffer[6]) << 4 | Ascii2Hex(buffer[7]);
                    buff2_buf[3] = (byte)temp;

                    temp = Ascii2Hex(buffer[8]) << 4 | Ascii2Hex(buffer[9]);
                    buff2_buf[4] = (byte)temp;

                    temp = Ascii2Hex(buffer[10]) << 4 | Ascii2Hex(buffer[11]);
                    buff2_buf[5] = (byte)temp;

                    crc16 = Ascii2Hex(buffer[12]) << 12 | Ascii2Hex(buffer[13]) << 8 | Ascii2Hex(buffer[14]) << 4 | Ascii2Hex(buffer[15]);


                    crc16_c = CRC16(buffer, 12);
                    if (crc16 == crc16_c)
                    {
                        InData2[0] = ((ulong)buff2_buf[0] << 0) | ((ulong)buff2_buf[1] << 8) | ((ulong)buff2_buf[2] << 16) | ((ulong)buff2_buf[3] << 24);
                        InData2[1] = ((ulong)buff2_buf[3] << 0) | ((ulong)buff2_buf[4] << 8);

                        //In_data[0] = buff2_buf[0];
                        //In_data[1] = buff2_buf[1];
                        //In_data[2] = buff2_buf[2];
                        //In_data[3] = buff2_buf[3];
                        //In_data[4] = buff2_buf[4];
                        //In_data[5] = buff2_buf[5];
                    }

                }
                IOPortToSerial.DiscardInBuffer();
            }
            catch
            {

            }
            finally
            {
            }
            return;
        }
        public const UInt16 POLYNORMIAL = 0xA001;

        public ushort CRC16(byte[] bytes, int usDataLen)
        {
            ushort crc = 0xffff, flag, ct = 0;

            while (usDataLen != 0)
            {
                crc ^= bytes[ct];
                for (int i = 0; i < 8; i++)
                {
                    flag = 0;
                    if ((crc & 1) == 1) flag = 1;
                    crc >>= 1;
                    if (flag == 1) crc ^= POLYNORMIAL;
                }
                ct++;
                usDataLen--;
            }
            return crc;
        }

        byte hex2ascii(byte toconv)
        {
            if (toconv < 0x0A) toconv += 0x30;
            else toconv += 0x37;
            return (toconv);
        }

        int Ascii2Hex(byte asc)
        {
            int hex;

            if (asc >= '0' && asc <= '9')
            {
                hex = asc - 0x30;
            }
            else if (asc >= 'A' && asc <= 'Z')
            {
                hex = asc - 0x37;
            }
            else
            {
                hex = asc - 0x57;
            }
            return hex;
        }

        public static int OutPos = 0;
        private long OutPosFirst;
        private long OutPosLast;

        public float GetDarkCurrent
        {
            get { return CurrData; }
        }

        public bool RetractorOnOff
        {
            get
            {
                return OutputCheck(IO_OUT.RETRACTOR_RESI_SELECT);
            }
            set
            {
                outportb(IO_OUT.RETRACTOR_RESI_SELECT, value);
            }
        }

        public bool YellowOnOff
        {
            get
            {
                return OutputCheck(IO_OUT.YELLOW);
            }
            set
            {
                outportb(IO_OUT.YELLOW, value);
            }
        }
        public bool GreenOnOff
        {
            get
            {
                return OutputCheck(IO_OUT.GREEN);
            }
            set
            {
                outportb(IO_OUT.GREEN, value);
            }
        }
        public bool RedOnOff
        {
            get
            {
                return OutputCheck(IO_OUT.RED);
            }
            set
            {
                outportb(IO_OUT.RED, value);
            }
        }
        public bool SetProductIn
        {
            get
            {
                return OutputCheck(IO_OUT.PRODUCT);
            }
            set
            {
                outportb(IO_OUT.PRODUCT, value);
            }
        }
        public bool SetOriginOnOff
        {
            get
            {
                return OutputCheck(IO_OUT.ORG);
            }
            set
            {
                outportb(IO_OUT.ORG, value);
            }
        }
        public bool BuzzerOnOff
        {
            get
            {
                return OutputCheck(IO_OUT.BUZZER);
            }
            set
            {
                outportb(IO_OUT.BUZZER, value);
            }
        }
        public bool TestOKOnOff
        {
            get
            {
                return OutputCheck(IO_OUT.TEST_OK);
            }
            set
            {
                outportb(IO_OUT.TEST_OK, value);
            }
        }
        public bool TestINGOnOff
        {
            get
            {
                return OutputCheck(IO_OUT.TEST_ING);
            }
            set
            {
                outportb(IO_OUT.TEST_ING, value);
            }
        }
        public void outportb(int Out, bool OnOff)
        {
            if (Out < IO_OUT.SERIAL)
            {
                byte Data = 0x00;
                int Pos = Out / 8;
                int dPos = Out % 8;

                Data = (byte)(0x01 << dPos);

                if (OnOff == true)
                    OutData[0, Pos] |= Data;
                else OutData[0, Pos] &= (byte)(~Data);
                //OutPos = 1;
                UpdWrite2();
            }
            else
            {
                int Pos = Out - IO_OUT.SERIAL;
                ushort Data;

                Data = (ushort)(0x01 << Pos);

                if (OnOff == true)
                    OutData2 |= Data;
                else OutData2 &= (byte)(~Data);

                SerialPortOut();
            }
            return;
        }

        public void outportbToFunction(short Out, bool OnOff)
        {
            byte Data = 0x00;

            int Pos = (int)Out / 8;
            int dPos = (int)Out % 8;

            Data = (byte)(0x01 << dPos);

            if (OnOff == true)
                OutData[1, Pos] |= Data;
            else OutData[1, Pos] &= (byte)(~Data);
            //OutPos = 1;
            UpdWrite2();
            return;
        }

        public void IOInit()
        {
            for (int i = 0; i < 8; i++)
            {
                OutData[0, i] = 0x00;
                OutData[1, i] = 0x00;
            }
            return;
        }

        public void TestIOInit()
        {
            for (int i = 0; i < 8; i++)
            {
                OutData[0, i] = 0x00;
                OutData[1, i] = 0x00;
            }
            SetProductIn = GetProductIn;
            return;
        }

        public struct __IOData__
        {
            public short Card;
            public short Pos;
            public byte Data;
        }

        /// <summary>
        /// P32C32 지정된 포트의 I/O 위치를 알아낸다.
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public __IOData__ IOCheck(short Pos)
        {
            __IOData__ value = new __IOData__();

            if (Pos < IO_OUT.SERIAL)
            {
                int OPos = (int)Pos / 8;
                byte Data = (byte)(0x01 << ((int)Pos % 8));

                value.Card = (short)(OPos / 4);
                value.Pos = (short)(OPos % 4);
                value.Data = Data;
            }
            else
            {
                byte Data = (byte)(0x01 << ((int)Pos - IO_OUT.SERIAL));

                value.Card = 0;
                value.Pos = 0;
                value.Data = Data;
            }
            return value;
        }
        /// <summary>
        /// P32C32 지정된 포트가 동작하고 있는지 읽어 온다.
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public bool OutputCheck(short Pos)
        {
            if (Pos < IO_OUT.SERIAL)
            {
                __IOData__ Value = IOCheck(Pos);

                if ((OutData[0, Value.Pos] & Value.Data) == Value.Data)
                    return true;
                else return false;
            }
            else
            {
                __IOData__ Value = IOCheck(Pos);

                if ((OutData2 & Value.Data) == Value.Data)
                    return true;
                else return false;
            }
        }
        private long UdpComCheckFirst;
        private long UdpComCheckLast;
#if THREAD_RUN
        private BackgroundWorker backgroundWorker1 = null;
        private void ThreadSetting()
        {
            backgroundWorker1 = new BackgroundWorker();

            //ReportProgress메소드를 호출하기 위해서 반드시 true로 설정, false일 경우 ReportProgress메소드를 호출하면 exception 발생
            backgroundWorker1.WorkerReportsProgress = true;
            //스레드에서 취소 지원 여부
            backgroundWorker1.WorkerSupportsCancellation = true;
            //스레드가 run시에 호출되는 핸들러 등록
            backgroundWorker1.DoWork += new DoWorkEventHandler(BackgroundWorker1_DoWork);
            // ReportProgress메소드 호출시 호출되는 핸들러 등록
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker1_ProgressChanged);
            // 스레드 완료(종료)시 호출되는 핸들러 동록
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker1_RunWorkerCompleted);


            // 스레드가 Busy(즉, run)가 아니라면
            if (backgroundWorker1.IsBusy != true)
            {
                // 스레드 작동!! 아래 함수 호출 시 위에서 bw.DoWork += new DoWorkEventHandler(bw_DoWork); 에 등록한 핸들러가
                // 호출 됩니다.

                backgroundWorker1.RunWorkerAsync();
            }
            return;
        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //바로 위에서 worker.ReportProgress((i * 10));호출 시 
            // bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged); 등록한 핸들러가 호출 된다고
            // 하였는데요.. 이 부분에서는 기존 Thread에서 처럼 Dispatcher를 이용하지 않아도 됩니다. 
            // 즉 아래처럼!!사용이 가능합니다.
            //this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");

            // 기존의 Thread클래스에서 아래와 같이 UI 엘리먼트를 갱신하려면
            // Dispatcher.BeginInvoke(delegate() 
            // {
            //        this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
            // )};
            //처럼 처리해야 할 것입니다. 그러나 바로 UI 엘리먼트를 업데이트 하고 있죠??
        }


        //스레드의 run함수가 종료될 경우 해당 핸들러가 호출됩니다.
        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            //스레드가 종료한 이유(사용자 취소, 완료, 에러)에 맞쳐 처리하면 됩니다.
            if ((e.Cancelled == true))
            {
            }
            else if (!(e.Error == null))
            {

            }
            else
            {

            }
        }


        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            do
            {
                //CancellationPending 속성이 true로 set되었다면(위에서 CancelAsync 메소드 호출 시 true로 set된다고 하였죠?
                if ((worker.CancellationPending == true))
                {
                    //루프를 break한다.(즉 스레드 run 핸들러를 벗어나겠죠)
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // 이곳에는 스레드에서 처리할 연산을 넣으시면 됩니다.

                    Processing();

                    Thread.Sleep(5);
                    //await Task.Delay(1000);
                    // 스레드 진행상태 보고 - 이 메소드를 호출 시 위에서 
                    // bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged); 등록한 핸들러가 호출 됩니다.
                    worker.ReportProgress(10);
                }
                if (mControl.isExit == true)
                {
                    worker.CancelAsync();
                }
            } while (true);
            //while (ExitFlag == false);
        }


        private void Processing()
        {
            try
            {
                if (isOpen == true)
                {
                    UdpComCheckLast = mControl.공용함수.timeGetTimems();
                    if (isConnection == true)
                    {
                        // 컨넥션 후 1초 이상 통신이 없으면 컨넥션이 끊어진 것으로 간주 한다.
                        if (1500 <= (UdpComCheckLast - UdpComCheckFirst))
                        {
                            isConnection = false;
                            UdpComCheckFirst = mControl.공용함수.timeGetTimems();
                            UdpComCheckLast = mControl.공용함수.timeGetTimems();
                        }
                    }
                    else
                    {
                        if (500 <= (UdpComCheckLast - UdpComCheckFirst))
                        {
                            UdpCanCommunicationInit();
                            UdpComCheckFirst = mControl.공용함수.timeGetTimems();
                            UdpComCheckLast = mControl.공용함수.timeGetTimems();
                        }
                    }
                }
                if (OutPos == 1)
                {
                    byte[] Data1 = { OutData[1, 0], OutData[1, 1], OutData[1, 2], OutData[1, 3], OutData[1, 4], OutData[1, 5], OutData[1, 6], OutData[1, 7] };
                    byte[] Data2 = { OutData[0, 0], OutData[0, 1], OutData[0, 2], OutData[0, 3], OutData[0, 4], OutData[0, 5], OutData[0, 6], OutData[0, 7] };
                    UdpWrite(0x151, Data1, 8);
                    UdpWrite(0x101, Data2, 8);
                    OutPos = 2;
                    OutPosFirst = mControl.공용함수.timeGetTimems();
                    OutPosLast = mControl.공용함수.timeGetTimems();
                }
                else
                {
                    if (OutPos == 2)
                    {
                        OutPosFirst = mControl.공용함수.timeGetTimems();
                        OutPosLast = mControl.공용함수.timeGetTimems();
                        OutPos = 0;
                    }
                    else if (OutPos == 0)
                    {
                        OutPosLast = mControl.공용함수.timeGetTimems();
                        if (100 <= (OutPosLast - OutPosFirst))
                        {
                            OutPos = 1;
                        }
                    }
                    else
                    {
                        byte[] Data = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                        if (DarkCurrStart == true) Data[0] |= 0x01;
                        UdpWrite(0x140, Data, 8);
                        OutPos = 0;
                        OutPosFirst = mControl.공용함수.timeGetTimems();
                        OutPosLast = mControl.공용함수.timeGetTimems();
                    }
                }
            }
            catch { }
            finally
            {
            }
        }        
#else
        private void timer1_tick(object sender, EventArgs e)
        {
            try
            {
                timer1.Enabled = false;

                if (isOpen == true)
                {
                    UdpComCheckLast = mControl.공용함수.timeGetTimems();
                    if (isConnection == true)
                    {
                        if (1500 <= (UdpComCheckLast - UdpComCheckFirst))
                        {
                            isConnection = false;
                            UdpComCheckFirst = mControl.공용함수.timeGetTimems();
                            UdpComCheckLast = mControl.공용함수.timeGetTimems();
                        }
                    }
                    else
                    {
                        if (500 <= (UdpComCheckLast - UdpComCheckFirst))
                        {
                            UdpCanCommunicationInit();
                            UdpComCheckFirst = mControl.공용함수.timeGetTimems();
                            UdpComCheckLast = mControl.공용함수.timeGetTimems();
                        }
                    }
                }
                if (OutPos == 1)
                {
                    byte[] Data1 = { OutData[1, 0], OutData[1, 1], OutData[1, 2], OutData[1, 3], OutData[1, 4], OutData[1, 5], OutData[1, 6], OutData[1, 7] };
                    byte[] Data2 = { OutData[0, 0], OutData[0, 1], OutData[0, 2], OutData[0, 3], OutData[0, 4], OutData[0, 5], OutData[0, 6], OutData[0, 7] };
                    UdpWrite(0x151, Data1, 8);
                    UdpWrite(0x101, Data2, 8);
                    OutPos = 2;
                    OutPosFirst = mControl.공용함수.timeGetTimems();
                    OutPosLast = mControl.공용함수.timeGetTimems();
                }
                else
                {
                    if (OutPos == 2)
                    {
                        OutPosFirst = mControl.공용함수.timeGetTimems();
                        OutPosLast = mControl.공용함수.timeGetTimems();
                        OutPos = 0;
                    }
                    else if (OutPos == 0)
                    {
                        OutPosLast = mControl.공용함수.timeGetTimems();
                        if (100 <= (OutPosLast - OutPosFirst))
                        {
                            OutPos = 1;
                        }
                    }
                    else
                    {
                        byte[] Data = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                        if (DarkCurrStart == true) Data[0] |= 0x01;
                        UdpWrite(0x140, Data, 8);
                        OutPos = 0;
                        OutPosFirst = mControl.공용함수.timeGetTimems();
                        OutPosLast = mControl.공용함수.timeGetTimems();
                    }
                }
            }
            catch { }
            finally
            {
                timer1.Enabled = !mControl.isExit;
            }
        }
#endif

        private bool DarkCurrStart = false;
        public bool DarkCurrentReadStart
        {
            get
            {
                return DarkCurrStart;
            }
            set
            {
                if (DarkCurrStart != value)
                {
                    DarkCurrStart = value;
                    OutPos = 3;
                }
            }
        }

        public int SetOutPos
        {
            set { OutPos = value; }
        }

        public byte[,] SetOutData
        {
            get
            {
                return OutData;
            }
        }

        public void SWOnBuzzer()
        {
            outportb(IO_OUT.BUZZER, true);
            mControl.공용함수.timedelay(300);
            outportb(IO_OUT.BUZZER, false);
            return;
        }

        public void SWCheckBuzzer()
        {
            outportb(IO_OUT.BUZZER, true);
            mControl.공용함수.timedelay(300);
            outportb(IO_OUT.BUZZER, false);

            mControl.공용함수.timedelay(300);
            outportb(IO_OUT.BUZZER, true);
            mControl.공용함수.timedelay(300);
            outportb(IO_OUT.BUZZER, false);
            return;
        }

        public bool GetAuto
        {
            get
            {
                return !InportCheck(IO_IN.AUTO);
                //return false;
            }
        }

        public __MODEL GetModel
        {
            get
            {
                unchecked
                {
                    if (InportCheck(IO_IN.MODEL_9P) == true)
                        return __MODEL.MODEL_9P;
                    else if (InportCheck(IO_IN.MODEL_78P) == true)
                        return __MODEL.MODEL_78P;
                    else if (InportCheck(IO_IN.MODEL_RELAX) == true)
                        return __MODEL.MODEL_RELAX;
                    else if (InportCheck(IO_IN.MODEL_11P) == true)
                        return __MODEL.MODEL_11P;
                    else return __MODEL.MODEL_SPARE;
                }
            }
        }

        public bool GetLhSelect
        {
            get
            {
                return !InportCheck(IO_IN.LH_SELECT);
            }
        }

        public bool GetLeftSelect
        {
            get
            {
                return InportCheck(IO_IN.MANUAL_SBR_LEFT);
            }
        }

        public bool GetRightSelect
        {
            get
            {
                return InportCheck(IO_IN.MANUAL_SBR_RIGHT);
            }
        }

        public bool GetFwdSelect
        {
            get
            {
                return InportCheck(IO_IN.MANUAL_SBR_FWD);
            }
        }

        public bool GetBwdSelect
        {
            get
            {
                return InportCheck(IO_IN.MANUAL_SBR_BWD);
            }
        }

        public bool Get1StUpSelect
        {
            get
            {
                return InportCheck(IO_IN.MANUAL_SBR_1ST_UP);
            }
        }
        public bool Get1StDnSelect
        {
            get
            {
                return InportCheck(IO_IN.MANUAL_SBR_1ST_DOWN);
            }
        }
        public bool Get2StUpSelect
        {
            get
            {
                return InportCheck(IO_IN.MANUAL_SBR_2ST_UP);
            }
        }
        public bool Get2StDnSelect
        {
            get
            {
                return InportCheck(IO_IN.MANUAL_SBR_2ST_DOWN);
            }
        }

        public bool GetLeftSensor
        {
            get
            {
                return InportCheck(IO_IN.SBR_LEFT_SENSOR);
            }
        }

        public bool GetRightSensor
        {
            get
            {
                return InportCheck(IO_IN.SBR_RIGHT_SENSOR);
            }
        }

        public bool GetFrontSensor
        {
            get
            {
                return InportCheck(IO_IN.SBR_FWD_SENSOR);
            }
        }

        public bool GetRearSensor
        {
            get
            {
                return InportCheck(IO_IN.SBR_BWD_SENSOR);
            }
        }

        public bool Get2StDown1Sensor
        {
            get
            {
                return InportCheck(IO_IN.SBR_2ST_DOWN1_SENSOR);
            }
        }

        public bool Get2StUp1Sensor
        {
            get
            {
                return InportCheck(IO_IN.SBR_2ST_UP1_SENSOR);
            }
        }

        public bool Get2StDown2Sensor
        {
            get
            {
                return InportCheck(IO_IN.SBR_2ST_DOWN2_SENSOR);
            }
        }

        public bool Get2StUp2Sensor
        {
            get
            {
                return InportCheck(IO_IN.SBR_2ST_UP2_SENSOR);
            }
        }

        public bool Get1StDownSensor
        {
            get
            {
                return InportCheck(IO_IN.SBR_1ST_DOWN_SENSOR);
            }
        }

        public bool Get1StUpSensor
        {
            get
            {
                return InportCheck(IO_IN.SBR_1ST_UP_SENSOR);
            }
        }

        public bool GetPassButton
        {
            get
            {
               return InportCheck(IO_IN.PASS);
            }
        }
        public bool GetResetButton
        {
            get
            {
               return InportCheck(IO_IN.RESET);
            }
        }

        public bool GetHeaterSw
        {
            get
            {
                return InportCheck(IO_IN.OPTION_HEATER);
            }
        }
        public bool GetVentSw
        {
            get
            {
                return InportCheck(IO_IN.OPTION_VENT);
            }
        }

        public bool GetBuckleSw
        {
            get
            {
                return InportCheck(IO_IN.OPTION_BUCKLE_WARNING);
            }
        }


        public bool GetRetractorSw
        {
            get
            {
                return InportCheck(IO_IN.OPTION_RETRACTOR);
            }
        }

        public bool GetSBRSw
        {
            get
            {
                return InportCheck(IO_IN.OPTION_SBR);
            }
        }

        public bool GetPSeatSw
        {
            get
            {
                return InportCheck(IO_IN.OPTION_PSEAT);
            }
        }

        public bool GetHeaterLampTo2Wire
        {
            get
            {
                return InportCheck(IO_IN.HEATER_LAMP_2WIRE);
            }
        }
        

        public bool GetCanType
        {
            get
            {
                return InportCheck(IO_IN.CAN);
            }
        }
        public bool GetBuckleWarning
        {
            get
            {
                return InportCheck(IO_IN.SEAT_BELT);
            }
        }

        public bool GetJigUp
        {
            get
            {
                return InportCheck(IO_IN.JIG_UP);
            }
        }

        public bool GetProductIn
        {
            get
            {
                return InportCheck(IO_IN.PRODUCT);
            }
        }

        private bool SetLeftMoveOnOff
        {
            set
            {
                byte Data = 0x00;

                int Pos = IO_OUT.SBR_LEFT / 8;
                int dPos = IO_OUT.SBR_LEFT % 8;

                Data = (byte)(0x01 << dPos);

                if (value == true)
                    OutData[0, Pos] |= Data;
                else OutData[0, Pos] &= (byte)(~Data);
                //OutPos = 1;
                UpdWrite2();
            }
        }
        private bool SetRightMove
        {
            set
            {
                byte Data = 0x00;

                int Pos = IO_OUT.SBR_RIGHT / 8;
                int dPos = IO_OUT.SBR_RIGHT % 8;

                Data = (byte)(0x01 << dPos);

                if (value == true)
                    OutData[0, Pos] |= Data;
                else OutData[0, Pos] &= (byte)(~Data);
                //OutPos = 1;
                UpdWrite2();
            }
        }

        private bool InportCheck(short Port)
        {
            if(Port < 32)
            {
                ulong In = (ulong)(0x01 << (Port - 0));
                if ((InData[1] & In) == In)
                    return true;
                else return false;
            }
            else
            {
                ulong In = (ulong)(0x01 << (Port - IO_IN.SERIAL));
                if ((InData2[0] & In) == In)
                    return true;
                else return false;
            }
        }

        /// <summary>
        /// true - Left, false - Right
        /// </summary>
        public bool JigLeftRightMove
        {
            set
            {
                if(value == true)
                {
                    SetRightMove = false;
                    mControl.공용함수.timedelay(100);
                    SetLeftMoveOnOff = true;
                    mControl.공용함수.timedelay(600);
                    SetLeftMoveOnOff = false;
                }
                else
                {
                    SetLeftMoveOnOff = false;
                    mControl.공용함수.timedelay(100);
                    SetRightMove = true;
                    mControl.공용함수.timedelay(600);
                    SetRightMove = false;
                }
            }
        }

        public bool JigFwdMove
        {
            set
            {
                byte Data = 0x00;

                int Pos = IO_OUT.SBR_BWD / 8;
                int dPos = IO_OUT.SBR_BWD % 8;

                Data = (byte)(0x01 << dPos);

                if (value == true)
                    OutData[0, Pos] |= Data;
                else OutData[0, Pos] &= (byte)(~Data);
                
                UpdWrite2();
            }
        }

        public bool Jig1StDown
        {
            set
            {
                byte Data = 0x00;

                int Pos = IO_OUT.SBR_1ST_DOWN / 8;
                int dPos = IO_OUT.SBR_1ST_DOWN % 8;

                Data = (byte)(0x01 << dPos);

                if (value == true)
                    OutData[0, Pos] |= Data;
                else OutData[0, Pos] &= (byte)(~Data);

                UpdWrite2();
            }
        }

        public bool Jig2StDown
        {
            set
            {
                byte Data = 0x00;

                int Pos = IO_OUT.SBR_2ST_DOWN / 8;
                int dPos = IO_OUT.SBR_2ST_DOWN % 8;

                Data = (byte)(0x01 << dPos);

                if (value == true)
                    OutData[0, Pos] |= Data;
                else OutData[0, Pos] &= (byte)(~Data);

                UpdWrite2();
            }
        }

        ~IOControl()
        {

        }
    }
}
