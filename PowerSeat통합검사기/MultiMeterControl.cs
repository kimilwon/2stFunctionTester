using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerSeat통합검사기
{
    public class GW_MultiMeter
    {
        SerialPort MultiMeterPort = new SerialPort()
        {
            DataBits = 8,
            StopBits = StopBits.One,
            Parity = Parity.None
        };
        MyInterface mControl = null;

        private Timer timer1 = new Timer();

        public GW_MultiMeter()
        {

        }
        public GW_MultiMeter(MyInterface mControl)
        {
            this.mControl = mControl;
            MultiMeterPort.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);
        }

        private bool SettingFlag = false;
        private bool StepSettingFlag = false;

        public bool Open(__Port__ Port)
        {
            bool Flag = false;

            if ((Port.Port != "") && (Port.Port != null) && (Port.Port != string.Empty))
            {
                MultiMeterPort.PortName = Port.Port;
                switch (Port.Speed)
                {
                    case 0: MultiMeterPort.BaudRate = 2400; break;
                    case 1: MultiMeterPort.BaudRate = 4800; break;
                    case 2: MultiMeterPort.BaudRate = 9600; break;
                    case 3: MultiMeterPort.BaudRate = 11400; break;
                    case 4: MultiMeterPort.BaudRate = 19200; break;
                    case 5: MultiMeterPort.BaudRate = 38400; break;
                    case 6: MultiMeterPort.BaudRate = 57600; break;
                    default: MultiMeterPort.BaudRate = 115200; break;
                }

                MultiMeterPort.Open();

                Flag = MultiMeterPort.IsOpen;

                Init();

                timer1.Interval = 10;
                
                timer1.Tick += new EventHandler(timer1_tick);
                timer1.Enabled = true;
            }
            return Flag;
        }
        private void timer1_tick(object sernder, EventArgs e)
        {
            try
            {
                timer1.Enabled = false;

                if ((SettingFlag == false) && (StepSettingFlag == false))
                {
                    SendingReadData();
                }
            }
            catch
            {

            }
            finally
            {
                timer1.Enabled = !mControl.isExit;
            }
        }
        public void Close()
        {
            if (MultiMeterPort.IsOpen == true)
            {
                MultiMeterPort.Close();
            }
        }

        public bool isOpen
        {
            get
            {
                return MultiMeterPort.IsOpen;
            }
        }


        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //this.Invoke(new EventHandler(MultiMeterDataRead));
            MultiMeterDataRead();
            return;
        }

        //private void MultiMeterDataRead(object s, EventArgs e)
        private void MultiMeterDataRead()
        {
            try
            {
                unchecked //overflow check를 하지 않도록 설정
                {
                int iRecSize = MultiMeterPort.BytesToRead; // 수신된 데이터 갯수
                //string strRxData;
                //strRxData = "";

                byte[] buff = new byte[iRecSize + 10];
                    MultiMeterPort.Read(buff, 0, iRecSize);

                string sx = Encoding.Default.GetString(buff);

                if (0 <= sx.IndexOf(","))
                {
                    string[] strRxData = sx.Split(',');

                    //if (1 < strRxData.Length)
                    //    MainForm.MultiData = strRxData[0];
                    //else MainForm.MultiData = Encoding.Default.GetString(buff);

                    if (1 < strRxData.Length)
                    {
                        if (strRxData[0] == "+ -OL- \n")
                        {
                            Value = 99999;
                        }
                        else
                        {
                            //double tester_data = Convert.ToDouble(strRxData[0]);
                            double tester_data;
                            if (double.TryParse(strRxData[0], out tester_data) == true) Value = tester_data;
                            //if (99999 <= Value) Value = 0;
                        }
                    }
                }

                    MultiMeterPort.DiscardInBuffer();
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message + "\n" + exp.StackTrace);
                throw exp;
            }
            finally
            {
                MultiMeterPort.DiscardInBuffer();
            }
            return;
        }

        public double GetData
        {
            get
            {
                return Value;
            }
        }

        private bool DataSendFlag = false;
        private double Value = 0;

        public void DCVoltModeSet(float Volt)
        {
            string s = ":CONFigure:VOLTage:DC";

            s = s + string.Format(" {0:0.00}\n\r", Volt);

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }
            return;
        }

        public void ACVoltModeSet(float Volt)
        {
            string s = ":CONFigure:VOLTage:AC";

            s = s + string.Format(" {0:0.00}\n\r", Volt);

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void DCACVoltModeSet(float Volt)
        {
            string s = ":CONFigure:VOLTage:DCAC";

            s = s + string.Format(" {0:0.00}\n\r", Volt);

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }


        public void DCCurrentModeSet(float Curr)
        {
            string s = ":CONFigure:CURRent:DC";

            s = s + string.Format(" {0:0.00}\n\r", Curr);

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void ACCurrentModeSet(float Curr)
        {
            string s = ":CONFigure:CURRent:AC";

            s = s + string.Format(" {0:0.00}\n\r", Curr);

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void DCACCurrentModeSet(float Curr)
        {
            string s = ":CONFigure:CURRent:DCAC";

            s = s + string.Format(" {0:0.00}\n\r", Curr);

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void ResitanceModeSet(float Resi)
        {
            string s = ":CONFigure:RESistance";

            s = s + string.Format(" {0:0.00}\n\r", Resi);

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void FreqModeSet(int Freq)
        {
            string s = ":CONFigure:FREQuency";

            s = s + string.Format(" {0:0}\n\r", Freq);

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void DiodeModeSet()
        {
            string s = ":CONFigure:DIODe\n\r";

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void TempModeSet(int Type)
        {
            string s = ":CONFigure:TEMP:TCO ";

            switch (Type)
            {
                case 0:
                    s = s + "J";
                    break;
                case 1:
                    s = s + "K";
                    break;
                default:
                    s = s + "T";
                    break;
            }
            s = s + "\n\r";

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void CapacitanceModeSet(int Cap)
        {
            //string s = ":CONFigure:CAPacitance \n\r";
            string s = ":CONFigure:CAP";

            s = s + string.Format(" {0}\n\r", Cap);

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void AutoModeSet(bool Flag)
        {
            string s = ":CONFigure:AUTO";

            if (Flag == true)
                s = s + " ON";
            else s = s + " OFF";
            s = s + "\n\r";

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void ReadDisplayValueToAll()
        {
            string s = "READ?";
            s = s + "\n\r";

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void Read1()
        {
            string s = "VALUE?";
            s = s + "\n\r";

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void Read2()
        {
            string s = "VALUE1?";
            s = s + "\n\r";

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void Read3()
        {
            string s = "VALUE2?";
            s = s + "\n\r";

            if (MultiMeterPort != null)
            {
                if (MultiMeterPort.IsOpen == true) MultiMeterPort.WriteLine(s);
            }

            return;
        }

        public void Init()
        {
            SettingFlag = true;

            ResitanceModeSet(500);
            //mControl.공용함수.timedelay(300);
            //AutoModeSet(false);
            SettingFlag = false;
            return;
        }

        private long First = 0;
        private long Last = 0;
        private void SendingReadData()
        {
            if (MultiMeterPort.IsOpen == true)
            {
                if (DataSendFlag == false)
                {
                    MultiMeterPort.DiscardInBuffer();
                    MultiMeterPort.ReceivedBytesThreshold = 8;

                    ReadDisplayValueToAll();
                    DataSendFlag = true;
                    First = mControl.공용함수.timeGetTimems();
                    Last = mControl.공용함수.timeGetTimems();
                }
                else
                {
                    Last = mControl.공용함수.timeGetTimems();
                    if(1000 <= (Last - First))
                    {
                        DataSendFlag = false;
                    }
                }
            }
            return;
        }

        ~GW_MultiMeter()
        {

        }    
    }
}
