//#define PROGRAM_TEST

#if !PROGRAM_TEST
#define PROGRAM_RUNNING
#endif


using System;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.Ports;
using System.Collections.Generic;


namespace PowerSeat통합검사기
{
    public class MiltiMeterControl
    {
        private SerialPort MultiPort;

        public MiltiMeterControl()
        {
        }

        public MiltiMeterControl(SerialPort Port)
        {
#if PROGRAM_RUNNING
            MultiPort = Port;
#endif
        }
        
        public void DCVoltModeSet(float Volt)
        {
            string s = ":CONFigure:VOLTage:DC";

            s = s + string.Format(" {0:0.00}\n\r", Volt);
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void ACVoltModeSet(float Volt)
        {
            string s = ":CONFigure:VOLTage:AC";

            s = s + string.Format(" {0:0.00}\n\r", Volt);
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void DCACVoltModeSet(float Volt)
        {
            string s = ":CONFigure:VOLTage:DCAC";

            s = s + string.Format(" {0:0.00}\n\r", Volt);
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }
               

        public void DCCurrentModeSet(float Curr)
        {
            string s = ":CONFigure:CURRent:DC";

            s = s + string.Format(" {0:0.00}\n\r", Curr);
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void ACCurrentModeSet(float Curr)
        {
            string s = ":CONFigure:CURRent:AC";

            s = s + string.Format(" {0:0.00}\n\r", Curr);
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void DCACCurrentModeSet(float Curr)
        {
            string s = ":CONFigure:CURRent:DCAC";

            s = s + string.Format(" {0:0.00}\n\r", Curr);
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void ResitanceModeSet(float Resi)
        {
            string s = ":CONFigure:RESistance";

            s = s + string.Format(" {0:0.00}\n\r", Resi);
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void FreqModeSet(int Freq)
        {
            string s = ":CONFigure:FREQuency";

            s = s + string.Format(" {0:0}\n\r", Freq);
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void DiodeModeSet()
        {
            string s = ":CONFigure:DIODe\n\r";
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
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
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void CapacitanceModeSet(int Cap)
        {
            //string s = ":CONFigure:CAPacitance \n\r";
            string s = ":CONFigure:CAP";

            s = s + string.Format(" {0}\n\r", Cap);
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void AutoModeSet(bool Flag)
        {
            string s = ":CONFigure:AUTO";

            if (Flag == true)
                s = s + " ON";
            else s = s + " OFF";
            s = s + "\n\r";
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void ReadDisplayValueToAll()
        {
            string s = "READ?";
            s = s + "\n\r";
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void Read1()
        {
            string s = "VALUE?";
            s = s + "\n\r";
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void Read2()
        {
            string s = "VALUE1?";
            s = s + "\n\r";
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public void Read3()
        {
            string s = "VALUE2?";
            s = s + "\n\r";
#if PROGRAM_RUNNING
            if (MultiPort != null)
            {
                if (MultiPort.IsOpen == true) MultiPort.WriteLine(s);
            }
#endif
            return;
        }

        public double Data;

        public void ReadValue()
        {
            try
            {
                unchecked //overflow check를 하지 않도록 설정
                {
#if PROGRAM_RUNNING
                int iRecSize = MultiPort.BytesToRead; // 수신된 데이터 갯수
                //string strRxData;
                //strRxData = "";

                byte[] buff = new byte[iRecSize + 10];
                MultiPort.Read(buff, 0, iRecSize);

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
                            Data = 0;
                        }
                        else
                        {
                            //double tester_data = Convert.ToDouble(strRxData[0]);
                            double tester_data;
                            if (double.TryParse(strRxData[0], out tester_data) == true) Data = tester_data;
                            if (99999 <= Data) Data = 0;
                        }
                    }
                }
                
                MultiPort.DiscardInBuffer();
#endif
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message + "\n" + exp.StackTrace);
                throw exp;
            }            
            finally
            {
#if PROGRAM_RUNNING
                MultiPort.DiscardInBuffer();
#endif
            }
            return;
        }

        public double ReadData
        {
            get
            {
                return Data;
            }
        }
        
        ~MiltiMeterControl()
        {
#if PROGRAM_RUNNING
            MultiPort.Dispose();
            MultiPort = null;
#endif
        }
    }
}