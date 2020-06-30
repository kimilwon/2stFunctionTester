//#define PROGRAM_TEST
#if !PROGRAM_TEST
#define PROGRAM_RUNNING
#endif

#define CAN_TEST

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Peak.Can.Basic;

namespace PowerSeat통합검사기
{
    public partial class SelfTest : Form
    {
        MyInterface mControl = null;
        private long First;
        private long Last;
        private bool PwrChangeFlag = false;
        private bool KeyChangeFlag = false;
        private long BuzzerFirst;
        private long BuzzerLast;
        private bool BuzerOnOff;
        private __CanMsg[] ReadMsg = new __CanMsg[5];
        private __CanMsg[] SendMsg = new __CanMsg[3];
        //private __CanControl CanCtrl = null;
        private long CanSendFirst;
        private long CanSendLast;
        public SelfTest()
        {
            InitializeComponent();            
        }
        public SelfTest(MyInterface mControl)
        {
            InitializeComponent();
            this.mControl = mControl;
        }

        private void SelfTest_Load(object sender, EventArgs e)
        {
            SetDoubleBuffered(fpSpread1);            
            timer1.Enabled = true;

            //CanCtrl = new __CanControl(mControl);
            if (mControl.GetPwrCtrl != null) mControl.GetPwrCtrl.POWER_CURRENTSetting(20);

            SendMsg[0].DATA = new byte[8];
            SendMsg[1].DATA = new byte[8];
            SendMsg[2].DATA = new byte[8];

            ReadMsg[0].DATA = new byte[8];
            ReadMsg[1].DATA = new byte[8];
            ReadMsg[2].DATA = new byte[8];
            ReadMsg[3].DATA = new byte[8];
            ReadMsg[4].DATA = new byte[8];

            SendMsg[0].ID = 0x100;
            SendMsg[0].DATA[0] = 0x00;
            SendMsg[0].DATA[1] = 0x00;
            SendMsg[0].DATA[2] = 0x00;
            SendMsg[0].DATA[3] = 0x0f; // Ign 관련
            SendMsg[0].DATA[4] = 0x00;
            SendMsg[0].DATA[5] = 0x20; //C_PPosition
            SendMsg[0].DATA[6] = 0x00;
            SendMsg[0].DATA[7] = 0x00;
            SendMsg[0].Length = 8;

            SendMsg[1].ID = 0x112;
            SendMsg[1].DATA[0] = 0x02;
            SendMsg[1].DATA[1] = 0x00;
            SendMsg[1].DATA[2] = 0x00;
            SendMsg[1].DATA[3] = 0x80;
            SendMsg[1].DATA[4] = 0x00;
            SendMsg[1].DATA[5] = 0x00;
            SendMsg[1].DATA[6] = 0x00;
            SendMsg[1].DATA[7] = 0x00;
            SendMsg[1].Length = 8;

            SendMsg[2].ID = 0x589;
            SendMsg[2].DATA[0] = 0x00;
            SendMsg[2].DATA[1] = 0x00;
            SendMsg[2].DATA[2] = 0x00;
            SendMsg[2].DATA[3] = 0x00;
            SendMsg[2].DATA[4] = 0x00;
            SendMsg[2].DATA[5] = 0x00;
            SendMsg[2].DATA[6] = 0x00;
            SendMsg[2].DATA[7] = 0x00;
            SendMsg[2].Length = 8;

            for (int i = 0; i < 5; i++)
            {
                ReadMsg[i].ID = 0x00;
                ReadMsg[i].DATA[0] = 0x00;
                ReadMsg[i].DATA[i] = 0x00;
                ReadMsg[i].DATA[2] = 0x00;
                ReadMsg[i].DATA[3] = 0x00; // Ign 관련
                ReadMsg[i].DATA[4] = 0x00;
                ReadMsg[i].DATA[5] = 0x00;
                ReadMsg[i].DATA[6] = 0x00;
                ReadMsg[i].DATA[7] = 0x00;
                ReadMsg[i].Length = 0;
            }
            CanSendFirst = mControl.공용함수.timeGetTimems();
            CanSendLast = mControl.공용함수.timeGetTimems();
            return;
        }

        private void SelfTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (led2.Value.AsBoolean == true)
            {
#if PROGRAM_RUNNING
                //mControl.GetCan.CanClose(0);                
#endif
            }
            e.Cancel = false;
            return;
        }

        //private void outportb(IO_OUT Out, bool OnOff)
        //{
        //    byte Data = 0x00;

        //    int Pos = (int)Out / 8;
        //    int dPos = (int)Out % 8;

        //    Data = (byte)(0x01 << dPos);

        //    if (OnOff == true)
        //        mControl.GetIOPort.SetOutData[0, Pos] |= Data;
        //    else mControl.GetIOPort.SetOutData[0, Pos] &= (byte)(~Data);
        //    mControl.GetIOPort.SetOutPos = 1;
        //    return;
        //}

        //private void outportb(IO_OUT_FUNC Out, bool OnOff)
        //{
        //    byte Data = 0x00;

        //    int Pos = (int)Out / 8;
        //    int dPos = (int)Out % 8;

        //    Data = (byte)(0x01 << dPos);

        //    if (OnOff == true)
        //        mControl.GetIOPort.SetOutData[1, Pos] |= Data;
        //    else mControl.GetIOPort.SetOutData[1, Pos] &= (byte)(~Data);
        //    mControl.GetIOPort.SetOutPos = 1;
        //    return;
        //}


        //private short SendFlag = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                ulong Data;
                int Row;
                //int cPos = -1;

                timer1.Enabled = false;

                //if (switchLever2.Value.AsBoolean == true)
                {
                    //ulong xData;
                    Row = 17;
                    

                    if (PwrChangeFlag == true)
                    {
                        Last = mControl.공용함수.timeGetTimems();

                        if (500 <= (Last - First))
                        {
                            if (mControl.GetPwrCtrl != null) mControl.GetPwrCtrl.POWER_PWSetting((float)knob1.Value.AsDouble);
                            PwrChangeFlag = false;
                        }
                    }


                    if (imageButton11.ButtonColor == Color.DeepPink)
                    {
                        BuzzerLast = mControl.공용함수.timeGetTimems();
                        if (BuzerOnOff == true)
                        {
                            if (700 <= (BuzzerLast - BuzzerFirst))
                            {
                                BuzerOnOff = false;
                                mControl.GetIOPort.BuzzerOnOff = BuzerOnOff;
                                BuzzerFirst = mControl.공용함수.timeGetTimems();
                            }
                        }
                        else
                        {
                            if (500 <= (BuzzerLast - BuzzerFirst))
                            {
                                BuzerOnOff = true;
                                mControl.GetIOPort.BuzzerOnOff = BuzerOnOff;
                                BuzzerFirst = mControl.공용함수.timeGetTimems();
                            }
                        }
                    }

                    if (mControl.GetPanelMeter != null)
                    {
                        fpSpread1.ActiveSheet.Cells[33, 5].Text = string.Format("{0:0.00}", mControl.GetPanelMeter.GetBatt);
                        fpSpread1.ActiveSheet.Cells[34, 5].Text = string.Format("{0:0.00}", mControl.GetPanelMeter.GetCurr);
                        fpSpread1.ActiveSheet.Cells[35, 5].Text = string.Format("{0:0.00}", mControl.GetPanelMeter.GetBuckle);
                    }

                    if (mControl.GetMultiMeter != null)
                    {
                        if (999999 < mControl.GetMultiMeter.GetReadData)
                            fpSpread1.ActiveSheet.Cells[36, 5].Text = "HHHH";
                        else fpSpread1.ActiveSheet.Cells[36, 5].Text = string.Format("{0:0.00}", mControl.GetMultiMeter.GetReadData);
                    }
                    for (int i = 0; i < 24; i++)
                    {
                        Data = (ulong)(0x01 << i);

                        if ((mControl.GetIOPort.GetInDataToSerial[0] & Data) == Data)
                        {
                            if (fpSpread1.ActiveSheet.Cells[1 + i, 2].Text != "ON")
                            {
                                fpSpread1.ActiveSheet.Cells[1 + i, 2].Text = "ON";
                                fpSpread1.ActiveSheet.Cells[1 + i, 2].BackColor = Color.Lime;
                                fpSpread1.ActiveSheet.Cells[1 + i, 2].ForeColor = Color.Black;
                            }
                        }
                        else
                        {
                            if (fpSpread1.ActiveSheet.Cells[1 + i, 2].Text != "OFF")
                            {
                                fpSpread1.ActiveSheet.Cells[1 + i, 2].Text = "OFF";
                                fpSpread1.ActiveSheet.Cells[1 + i, 2].BackColor = Color.White;
                                fpSpread1.ActiveSheet.Cells[1 + i, 2].ForeColor = Color.Silver;
                            }
                        }
                    }

                    //if (mControl.GetIOPort.GetCanType == true)
                    //{
                    //    if (fpSpread1.ActiveSheet.Cells[24, 2].Text != "ON")
                    //    {
                    //        fpSpread1.ActiveSheet.Cells[24, 2].Text = "ON";
                    //        fpSpread1.ActiveSheet.Cells[24, 2].BackColor = Color.Lime;
                    //        fpSpread1.ActiveSheet.Cells[24, 2].ForeColor = Color.Black;
                    //    }
                    //}
                    //else
                    //{
                    //    if (fpSpread1.ActiveSheet.Cells[24, 2].Text != "OFF")
                    //    {
                    //        fpSpread1.ActiveSheet.Cells[24, 2].Text = "OFF";
                    //        fpSpread1.ActiveSheet.Cells[24, 2].BackColor = Color.White;
                    //        fpSpread1.ActiveSheet.Cells[24, 2].ForeColor = Color.Silver;
                    //    }
                    //}

                    if (mControl.GetIOPort.GetHeaterLampTo2Wire == true)
                    {
                        if (fpSpread1.ActiveSheet.Cells[25, 2].Text != "ON")
                        {
                            fpSpread1.ActiveSheet.Cells[25, 2].Text = "ON";
                            fpSpread1.ActiveSheet.Cells[25, 2].BackColor = Color.Lime;
                            fpSpread1.ActiveSheet.Cells[25, 2].ForeColor = Color.Black;
                        }
                    }
                    else
                    {
                        if (fpSpread1.ActiveSheet.Cells[25, 2].Text != "OFF")
                        {
                            fpSpread1.ActiveSheet.Cells[25, 2].Text = "OFF";
                            fpSpread1.ActiveSheet.Cells[25, 2].BackColor = Color.White;
                            fpSpread1.ActiveSheet.Cells[25, 2].ForeColor = Color.Silver;
                        }
                    }


                    for (int i = 16; i < (IO_IN.MAX - 1); i++)
                    {
                        Data = (ulong)(0x01 << i);

                        if ((mControl.GetIOPort.GetInData[1] & Data) == Data)
                        {
                            if (fpSpread1.ActiveSheet.Cells[i, 6].Text != "ON")
                            {
                                fpSpread1.ActiveSheet.Cells[i, 6].Text = "ON";
                                fpSpread1.ActiveSheet.Cells[i, 6].BackColor = Color.Lime;
                                fpSpread1.ActiveSheet.Cells[i, 6].ForeColor = Color.Black;
                            }
                        }
                        else
                        {
                            if (fpSpread1.ActiveSheet.Cells[i, 6].Text != "OFF")
                            {
                                fpSpread1.ActiveSheet.Cells[i, 6].Text = "OFF";
                                fpSpread1.ActiveSheet.Cells[i, 6].BackColor = Color.White;
                                fpSpread1.ActiveSheet.Cells[i, 6].ForeColor = Color.Silver;
                            }
                        }
                    }

                    bool[] LampFlag = 
                    {
                        mControl.GetCanReWrite.LH_HeaterHighLamp,
                        mControl.GetCanReWrite.LH_HeaterMidLamp,
                        mControl.GetCanReWrite.LH_HeaterLowLamp,
                        mControl.GetCanReWrite.LH_VentHighLamp,
                        mControl.GetCanReWrite.LH_VentMidLamp,
                        mControl.GetCanReWrite.LH_VentLowLamp,
                        mControl.GetCanReWrite.RH_HeaterHighLamp,
                        mControl.GetCanReWrite.RH_HeaterMidLamp,
                        mControl.GetCanReWrite.RH_HeaterLowLamp,
                        mControl.GetCanReWrite.RH_VentHighLamp,
                        mControl.GetCanReWrite.RH_VentMidLamp,
                        mControl.GetCanReWrite.RH_VentLowLamp
                    };

                    
                    Row = 0;
                    for (int i = 0; i < (IO_IN_FUNC.MAX - IO_IN_FUNC.DRV_HEATER_HIGH); i++)
                    {
                        Data = (ulong)(0x01 << i);

                        if (((mControl.GetIOPort.GetInData[0] & Data) == Data) || (LampFlag[i] == true))
                        {
                            if (fpSpread1.ActiveSheet.Cells[1 + i, 6].Text != "ON")
                            {
                                fpSpread1.ActiveSheet.Cells[1 + i, 6].Text = "ON";
                                fpSpread1.ActiveSheet.Cells[1 + i, 6].BackColor = Color.Lime;
                                fpSpread1.ActiveSheet.Cells[1 + i, 6].ForeColor = Color.Black;
                            }
                        }
                        else
                        {
                            if (fpSpread1.ActiveSheet.Cells[1 + i, 6].Text != "OFF")
                            {
                                fpSpread1.ActiveSheet.Cells[1 + i, 6].Text = "OFF";
                                fpSpread1.ActiveSheet.Cells[1 + i, 6].BackColor = Color.White;
                                fpSpread1.ActiveSheet.Cells[1 + i, 6].ForeColor = Color.Silver;
                            }
                        }
                        Row++;
                    }

                    Data = (ulong)(0x01 << IO_IN.SEAT_BELT) & 0xffffffff;
                    if ((mControl.GetIOPort.GetInData[1] & Data) == Data)
                    {
                        if (fpSpread1.ActiveSheet.Cells[1 + Row, 6].Text != "ON")
                        {
                            fpSpread1.ActiveSheet.Cells[1 + Row, 6].Text = "ON";
                            fpSpread1.ActiveSheet.Cells[1 + Row, 6].BackColor = Color.Lime;
                            fpSpread1.ActiveSheet.Cells[1 + Row, 6].ForeColor = Color.Black;
                        }
                    }
                    else
                    {
                        if (fpSpread1.ActiveSheet.Cells[1 + Row, 6].Text != "OFF")
                        {
                            fpSpread1.ActiveSheet.Cells[1 + Row, 6].Text = "OFF";
                            fpSpread1.ActiveSheet.Cells[1 + Row, 6].BackColor = Color.White;
                            fpSpread1.ActiveSheet.Cells[1 + Row, 6].ForeColor = Color.Silver;
                        }
                    }
                    
                    Data = (ulong)(0x01 << IO_IN.PRODUCT) & 0xffffffff;
                    if ((mControl.GetIOPort.GetInData[1] & Data) == Data)
                    {
                        if (fpSpread1.ActiveSheet.Cells[2 + Row, 6].Text != "ON")
                        {
                            fpSpread1.ActiveSheet.Cells[2 + Row, 6].Text = "ON";
                            fpSpread1.ActiveSheet.Cells[2 + Row, 6].BackColor = Color.Lime;
                            fpSpread1.ActiveSheet.Cells[2 + Row, 6].ForeColor = Color.Black;
                        }
                    }
                    else
                    {
                        if (fpSpread1.ActiveSheet.Cells[2 + Row, 6].Text != "OFF")
                        {
                            fpSpread1.ActiveSheet.Cells[2 + Row, 6].Text = "OFF";
                            fpSpread1.ActiveSheet.Cells[2 + Row, 6].BackColor = Color.White;
                            fpSpread1.ActiveSheet.Cells[2 + Row, 6].ForeColor = Color.Silver;
                        }
                    }

                    Data = (ulong)(0x01 << IO_IN.JIG_UP) & 0xffffffff;
                    if ((mControl.GetIOPort.GetInData[1] & Data) == Data)
                    {
                        if (fpSpread1.ActiveSheet.Cells[3 + Row, 6].Text != "ON")
                        {
                            fpSpread1.ActiveSheet.Cells[3 + Row, 6].Text = "ON";
                            fpSpread1.ActiveSheet.Cells[3 + Row, 6].BackColor = Color.Lime;
                            fpSpread1.ActiveSheet.Cells[3 + Row, 6].ForeColor = Color.Black;
                        }
                    }
                    else
                    {
                        if (fpSpread1.ActiveSheet.Cells[3 + Row, 6].Text != "OFF")
                        {
                            fpSpread1.ActiveSheet.Cells[3 + Row, 6].Text = "OFF";
                            fpSpread1.ActiveSheet.Cells[3 + Row, 6].BackColor = Color.White;
                            fpSpread1.ActiveSheet.Cells[3 + Row, 6].ForeColor = Color.Silver;
                        }
                    }
                }

                if (mControl.GetIOPort.Get1StUpSensor == true)
                {
                    if (mControl.GetIOPort.GetLeftSelect == true)
                    {
                        if (IOLeftSelect == false)
                        {
                            IOLeftSelect = true;
                            mControl.GetIOPort.JigLeftRightMove = true;
                        }
                    }
                    else if (IOLeftSelect == true)
                    {
                        IOLeftSelect = false;
                        mControl.GetIOPort.JigLeftRightMove = false;
                    }

                    if (mControl.GetIOPort.GetFwdSelect == true)
                    {
                        if (IOFwdSelect == false)
                        {
                            IOFwdSelect = true;
                            mControl.GetIOPort.JigFwdMove = true;
                        }
                    }
                    else
                    {
                        if (IOFwdSelect == true)
                        {
                            IOFwdSelect = false;
                            mControl.GetIOPort.JigFwdMove = false;
                        }
                    }
                }
                if (mControl.GetIOPort.Get1StDnSelect == true)
                {
                    if (IO1StDownSelect == false)
                    {
                        IO1StDownSelect = true;
                        mControl.GetIOPort.Jig1StDown = true;
                    }
                }
                else
                {
                    if (IO1StDownSelect == true)
                    {
                        IO1StDownSelect = false;
                        mControl.GetIOPort.Jig1StDown = false;
                    }
                }
                //if (mControl.GetIOPort.Get1StDnSensor == true)
                {
                    if (mControl.GetIOPort.Get2StDnSelect == true)
                    {
                        if (IO2StDownSelect == false)
                        {
                            IO2StDownSelect = true;
                            mControl.GetIOPort.Jig2StDown = true;
                        }
                    }
                    else
                    {
                        if (IO2StDownSelect == true)
                        {
                            IO2StDownSelect = false;
                            mControl.GetIOPort.Jig2StDown = false;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message + "\n" + exp.StackTrace);
            }
            finally
            {
                timer1.Enabled = true;
            }
            return;
        }

        private bool IOLeftSelect { get; set; }
        
        private bool IOFwdSelect { get; set; }
        
        private bool IO1StDownSelect { get; set; }
        private bool IO2StDownSelect { get; set; }
        
        public static void SetDoubleBuffered(Control control)
        {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(Control).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, control, new object[] { true });
            return;
        }

        private void switchLever1_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            led1.Value.AsBoolean = e.ValueNew;

            if (mControl.GetPwrCtrl != null)
            {
                if (e.ValueNew == true)
                    mControl.GetPwrCtrl.POWER_PWON();
                else mControl.GetPwrCtrl.POWER_PWOFF();
            }
            return;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                double Value;

                KeyChangeFlag = true;

                if (0 <= textBox1.Text.IndexOf("[V]"))
                {
                    string s;
                    if(0 <= textBox1.Text.IndexOf(" [V]"))
                        s = textBox1.Text.Substring(0, textBox1.Text.IndexOf(" [V]"));
                    else s = textBox1.Text.Substring(0, textBox1.Text.IndexOf("[V]"));
                    if (double.TryParse(s, out Value) == false) Value = 0;
                }
                else
                {
                    if (double.TryParse(textBox1.Text, out Value) == false) Value = 0;
                }
                knob1.Value.AsDouble = Value;
                if (mControl.GetPwrCtrl != null) mControl.GetPwrCtrl.POWER_PWSetting((float)Value);
                KeyChangeFlag = false;
                if (textBox1.Text.IndexOf("[V]") < 0)
                {
                    textBox1.Text = textBox1.Text + " [V]";
                }
                return;
            }
        }

        
        private void knob1_ValueChanged(object sender, Iocomp.Classes.ValueDoubleEventArgs e)
        {
            if (KeyChangeFlag == true) return;
            textBox1.Text = string.Format("{0:0.00} [V]", knob1.Value.AsDouble);
            First = mControl.공용함수.timeGetTimems();
            Last = mControl.공용함수.timeGetTimems();
            PwrChangeFlag = true;
            return;
        }

//        public short CanPosition(short Ch)
//        {
//#if PROGRAM_RUNNING
//            //short ID = 0;

//            string[] Device = mControl.GetCan.GetDevice;

//            //for (short i = 0; i < Device.Length; i++)
//            //{
//            //    string s = Device[i];
//            //    string s1 = "0x" + Pos.ToString("X2");

//            //    if (0 <= s.IndexOf(s1))
//            //    {
//            //        ID = i;
//            //        break;
//            //    }
//            //}
//            if (Ch == 0)
//            {
//                short Pos = -1;
//                string s1 = "Device=" + mControl.GetConfig.Can1.Device.ToString();
//                string s2 = "Channel=" + mControl.GetConfig.Can1.Channel.ToString() + "h";

//                foreach (string s in Device)
//                {

//                    if (0 <= s.IndexOf(s1))
//                    {
//                        if (0 <= s.IndexOf(s2))
//                        {
//                            string ss = s.Substring(s.IndexOf("ID=") + "ID=".Length);
//                            string[] ss1 = ss.Split(',');
//                            if (1 < ss1.Length)
//                            {
//                                string ss2 = ss1[0].Replace("(", null);

//                                ss2 = ss2.Replace(")", null);
//                                Pos = (short)mControl.공용함수.StringToHex(ss2);
//                            }
//                        }
//                    }
//                }

//                if (Pos == -1)
//                {
//                    Pos = mControl.GetConfig.Can1.ID;
//                }
//                return Pos;
//            }
//            else
//            {
//                short Pos = -1;
//                string s1 = "Device=" + mControl.GetConfig.Can2.Device.ToString();
//                string s2 = "Channel=" + mControl.GetConfig.Can2.Channel.ToString() + "h";

//                foreach (string s in Device)
//                {

//                    if (0 <= s.IndexOf(s1))
//                    {
//                        if (0 <= s.IndexOf(s2))
//                        {
//                            string ss = s.Substring(s.IndexOf("ID=") + "ID=".Length);
//                            string[] ss1 = ss.Split(',');
//                            if (1 < ss1.Length)
//                            {
//                                string ss2 = ss1[0].Replace("(", null);

//                                ss2 = ss2.Replace(")", null);
//                                Pos = (short)mControl.공용함수.StringToHex(ss2);
//                            }
//                        }
//                    }
//                }

//                if (Pos == -1)
//                {
//                    Pos = mControl.GetConfig.Can2.ID;
//                }
//                return Pos;
//            }
           
//#else
//            return 0;
//#endif
//        }


        private void imageButton1_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (imageButton2.ButtonColor == Color.Red)
                    imageButton2.ButtonColor = Color.Black;
                else imageButton2.ButtonColor = Color.Red;

                imageButton18.Visible = imageButton2.ButtonColor == Color.Red ? true : false;
                imageButton42.Visible = imageButton2.ButtonColor == Color.Red ? true : false;
                imageButton43.Visible = imageButton2.ButtonColor == Color.Red ? true : false;
                imageButton44.Visible = imageButton2.ButtonColor == Color.Red ? true : false;
#if PROGRAM_RUNNING
                if (imageButton2.ButtonColor == Color.Red)
                {
                    //mControl.GetCan.OpenCan(0, CanPosition(), (short)mControl.GetConfig.Can.Speed, false);

                    if (mControl.GetCan != null) mControl.GetCanReWrite.CanLinDefaultSetting();

                    richTextBox1.Clear();
                    led2.Value.AsBoolean = imageButton2.ButtonColor == Color.Red ? true : false;
                }
                else
                {

                    //mControl.GetCan.CanClose(0);

                    led2.Value.AsBoolean = imageButton2.ButtonColor == Color.Red ? true : false;
                }
#endif
            }
            catch (Exception exp)
            {
                MessageBox.Show("CAN 보드가 오픈되지 않습니다.");

                led2.Value.AsBoolean = false;                
                throw exp;
            }
            return;
        }

        private void imageButton3_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;


            mControl.GetIOPort.outportb(IO_OUT.RH_SELECT, Flag);
            return;
        }

       
        private void imageButton5_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;
            
            Btn.ButtonColor = Color.DeepPink;
            mControl.GetCanReWrite.CanDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_CAN_LIST.IMS_DrvrImsSwSetSta, (byte)IMS_DrvrImsSwSetSta.Data.On);
            mControl.공용함수.timedelay(400);
            Btn.ButtonColor = Color.Black;
            mControl.GetCanReWrite.CanDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_CAN_LIST.IMS_DrvrImsSwSetSta, (byte)IMS_DrvrImsSwSetSta.Data.Off);
            return;
        }

        private void imageButton6_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;
            
            mControl.GetCanReWrite.CanDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_CAN_LIST.IMS_DrvrImsSw1Sta, (byte)IMS_DrvrImsSw1Sta.Data.On);
            Btn.ButtonColor = Color.DeepPink;
            mControl.공용함수.timedelay(400);
            mControl.GetCanReWrite.CanDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_CAN_LIST.IMS_DrvrImsSw1Sta, (byte)IMS_DrvrImsSw1Sta.Data.Off);
            Btn.ButtonColor = Color.Black;
            return;
        }

        private void imageButton7_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;
            
            Btn.ButtonColor = Color.DeepPink;
            mControl.GetCanReWrite.CanDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_CAN_LIST.IMS_DrvrImsSw2Sta, (byte)IMS_DrvrImsSw2Sta.Data.On);
            mControl.공용함수.timedelay(400);
            Btn.ButtonColor = Color.Black;
            mControl.GetCanReWrite.CanDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_CAN_LIST.IMS_DrvrImsSw2Sta, (byte)IMS_DrvrImsSw2Sta.Data.Off);
            return;
        }

        private void imageButton10_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.Yellow)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.Yellow;
            bool Flag = Btn.ButtonColor == Color.Yellow ? true : false;
            mControl.GetIOPort.YellowOnOff = Flag;
            return;
        }

        private void imageButton9_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.Lime)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.Lime;
            bool Flag = Btn.ButtonColor == Color.Lime ? true : false;
            mControl.GetIOPort.GreenOnOff = Flag;
            return;
        }

        private void imageButton8_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.Red)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.Red;
            bool Flag = Btn.ButtonColor == Color.Red ? true : false;
            mControl.GetIOPort.RedOnOff = Flag;
            return;
        }

        private void imageButton13_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            mControl.GetIOPort.TestOKOnOff = Flag;

            return;
        }

        private void imageButton12_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;
            mControl.GetIOPort.TestINGOnOff = Flag;
            return;
        }

        private void imageButton11_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            mControl.GetIOPort.BuzzerOnOff = Flag;
            BuzzerFirst = mControl.공용함수.timeGetTimems();
            BuzzerLast = mControl.공용함수.timeGetTimems();
            BuzerOnOff = true;
            return;
        }

        private void imageButton15_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;
            mControl.GetIOPort.SetProductIn = Flag;
            return;
        }

        private void imageButton17_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.DRV_BATT, Flag);
            
            mControl.GetCanReWrite.LhRhSelect = LHRH.LH;

            return;
        }

        private void imageButton16_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PASS_BATT, Flag);

            mControl.GetCanReWrite.LhRhSelect = LHRH.RH;
            return;
        }

        private void imageButton19_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PSEAT_BATT, Flag);
        }

       
        private void imageButton21_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton20.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.DRV_HEATER_GND, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.DRV_HEATER_BATT, Flag);
            return;            
        }

        private void imageButton20_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton21.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.DRV_HEATER_BATT, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.DRV_HEATER_GND, Flag);
            
            return;
        }

        private void imageButton23_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton22.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.DRV_VENT_GND, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.DRV_VENT_BATT, Flag);

            return;
        }

        private void imageButton22_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton23.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.DRV_VENT_BATT, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.DRV_VENT_BATT, Flag);
            return;
        }

        private void imageButton27_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton26.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PASS_HEATER_GND, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PASS_HEATER_BATT, Flag);
            return;
        }

        private void imageButton26_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton27.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PASS_HEATER_BATT, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PASS_HEATER_GND, Flag);

            return;
        }

        private void imageButton25_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton24.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PASS_VENT_GND, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PASS_VENT_BATT, Flag);

            return;
        }

        private void imageButton24_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton25.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PASS_VENT_BATT, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PASS_VENT_BATT, Flag);
            return;
        }

        private void imageButton31_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton30.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.IGN1_GND, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.IGN1_BATT, Flag);
            
            return;
        }

        private void imageButton30_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton31.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.IGN1_BATT, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.IGN1_GND, Flag);
            return;
        }

        private void imageButton29_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton28.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.IGN2_GND, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.IGN2_BATT, Flag);
            return;
        }

        private void imageButton28_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton29.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.IGN2_BATT, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.IGN2_GND, Flag);
            return;
        }

        private void imageButton32_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton34.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP1_GND, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP1_BATT, Flag);
            return;
        }

        private void imageButton34_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton32.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP1_BATT, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP1_GND, Flag);
            return;
        }

        private void imageButton33_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton35.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP2_GND, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP2_BATT, Flag);
            return;
        }

        private void imageButton35_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton33.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP2_BATT, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP2_GND, Flag);
            return;
        }

        private void imageButton36_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton37.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP3_GND, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP3_BATT, Flag);
            return;
        }

        private void imageButton37_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton36.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP3_BATT, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP3_GND, Flag);
            return;
        }

        private void imageButton40_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.DRV_LAMP_ACTIVE_LOW, Flag);
            return;
        }

        private void imageButton39_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.PASS_LAMP_ACTIVE_LOW, Flag);
            return;
        }

        private void imageButton41_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton38.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP4_GND, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP4_BATT, Flag);
            return;
        }

        private void imageButton38_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            if (Flag == true)
            {
                imageButton41.ButtonColor = Color.Black;
                mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP4_BATT, false);
                mControl.공용함수.timedelay(20);
            }
            mControl.GetIOPort.outportbToFunction(IO_OUT_FUNC.SP4_GND, Flag);
            return;
        }

        private void imageButton18_Click(object sender, EventArgs e)
        {
            //히터 (LIN)
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            Btn.ButtonColor = Color.DeepPink;
            //mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWEvenLEDDimRes, (byte)SWEvenLEDDimRes.Data.Fixed_100Pro_Dimming);
            //mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWOddLEDDimRes, (byte)SWOddLEDDimRes.Data.Fixed_100Pro_Dimming);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWLEDStatus, (byte)SWLEDStatus.Data.AllLEDChannelTurnOffStatus);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSWRaw, (byte)LH_HeaterSWRaw.Data.On);
            mControl.공용함수.timedelay(100);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSW, (byte)LH_HeaterSW.Data.Short_Term_Push);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSWRaw, (byte)LH_HeaterSWRaw.Data.Off);
            mControl.공용함수.timedelay(300);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSW, (byte)LH_HeaterSW.Data.No_Action);
            Btn.ButtonColor = Color.Black;
            return;
        }

        private void imageButton42_Click(object sender, EventArgs e)
        {
            //통풍 (LIN)
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;
            //mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWEvenLEDDimRes, (byte)SWEvenLEDDimRes.Data.Fixed_100Pro_Dimming);
            //mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWOddLEDDimRes, (byte)SWOddLEDDimRes.Data.Fixed_100Pro_Dimming);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWLEDStatus, (byte)SWLEDStatus.Data.AllLEDChannelTurnOffStatus);
            Btn.ButtonColor = Color.DeepPink;
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.LH_VentSWRaw, (byte)LH_VentSWRaw.Data.On);
            mControl.공용함수.timedelay(100);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.LH_VentSW, (byte)LH_VentSW.Data.Short_Term_Push);
            mControl.공용함수.timedelay(300);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.LH_VentSWRaw, (byte)LH_VentSWRaw.Data.Off);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.LH_VentSW, (byte)LH_VentSW.Data.No_Action);
            Btn.ButtonColor = Color.Black;
            return;
        }

        private void imageButton43_Click(object sender, EventArgs e)
        {
            //히터 (LIN)
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            Btn.ButtonColor = Color.DeepPink;
            //mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWEvenLEDDimRes, (byte)SWEvenLEDDimRes.Data.Fixed_100Pro_Dimming);
            //mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWOddLEDDimRes, (byte)SWOddLEDDimRes.Data.Fixed_100Pro_Dimming);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.RH_HeaterSWRaw, (byte)RH_HeaterSWRaw.Data.On);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWLEDStatus, (byte)SWLEDStatus.Data.AllLEDChannelTurnOffStatus);
            mControl.공용함수.timedelay(100);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.RH_HeaterSW, (byte)RH_HeaterSW.Data.Short_Term_Push);
            mControl.공용함수.timedelay(300);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.RH_HeaterSWRaw, (byte)RH_HeaterSWRaw.Data.Off);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.RH_HeaterSW, (byte)RH_HeaterSW.Data.No_Action);
            Btn.ButtonColor = Color.Black;
            return;
        }

        private void imageButton44_Click(object sender, EventArgs e)
        {
            //통풍 (LIN)
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            Btn.ButtonColor = Color.DeepPink;
            //mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWEvenLEDDimRes, (byte)SWEvenLEDDimRes.Data.Fixed_100Pro_Dimming);
            //mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWOddLEDDimRes, (byte)SWOddLEDDimRes.Data.Fixed_100Pro_Dimming);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.RH_VentSWRaw, (byte)RH_VentSWRaw.Data.On);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.SWLEDStatus, (byte)SWLEDStatus.Data.AllLEDChannelTurnOffStatus);
            mControl.공용함수.timedelay(100);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.RH_VentSW, (byte)RH_VentSW.Data.Short_Term_Push);
            mControl.공용함수.timedelay(300);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.RH_VentSWRaw, (byte)RH_VentSWRaw.Data.Off);
            mControl.GetCanReWrite.LINDataOutput(mControl.GetCanReWrite.LhRhSelect, OUT_LIN_LIST.RH_VentSW, (byte)RH_VentSW.Data.No_Action);
            Btn.ButtonColor = Color.Black;
            return;
        }

        private void imageButton4_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;

            mControl.GetIOPort.SetOriginOnOff = Flag;
            return;
        }

        private void imageButton14_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;

            if(Btn.ButtonColor == Color.DeepPink)
            {
                knob1.Value.AsDouble = 13.6F;
                switchLever1.Value.AsBoolean = true;
            }
            else
            {
                knob1.Value.AsDouble = 0F;
                switchLever1.Value.AsBoolean = false;
            }
            return;
        }

        private void imageButton45_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            if (Btn.ButtonColor == Color.DeepPink)
                Btn.ButtonColor = Color.Black;
            else Btn.ButtonColor = Color.DeepPink;
            bool Flag = Btn.ButtonColor == Color.DeepPink ? true : false;


            mControl.GetIOPort.RetractorOnOff = Flag;
            return;
        }
    }
}
