//#define PROGRAM_TEST
//#define POP_MODE
//#define CAN_TEST


#if !PROGRAM_TEST
#define PROGRAM_RUNNING
#endif


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.IO;
using Peak.Can.Basic;
//using NiCAN;
using System.Runtime.InteropServices;
using System.Diagnostics;
using MES;

namespace PowerSeat통합검사기
{
    public interface MyInterface
    {
        COMMON_FUCTION 공용함수 { get; }
        LinControl GetLin { get; }
        __CanControl GetCan { get; }
        bool isExit { get; }
        IOControl GetIOPort { get; }

        CanMap GetCanReWrite { get; }
        __Config__ GetConfig { get; set; }
        PowerControl GetPwrCtrl { get; }
        //double[] GetADData { get; }
        //UsbMultiMeterControl GetMultiMeter { get; }
        GW_MultiMeter GetMultiMeter { get; }
        PanelMeter GetPanelMeter { get; }
        short GetLinChannel(short Ch);
        bool isRunning { get; }
    }
    public partial class MainForm : Form, MyInterface
    {
        //private Graphics g1;
        //private Graphics g2;
        private COMMON_FUCTION ComF = new COMMON_FUCTION();

        protected Bitmap backbuffer;    //Double Buffering에 사용할 버퍼
        //private bool CanOpenFlag = false;
        private IOControl IOPort = null;
        private __Spec__ TSpec;
        private MES_Control MESCtrl = null;

        private bool SelfTestMode = false;
        private int TestOKCount;
        public bool ExitFlag { get; set; }
        private bool JigUpFlag = false;
        private int JigUpCount = 0;
        private bool SaveDataFlag = false;
        //private short VentPos = 0;
        //private short HeaterPos = 0;
        //private int Angle = 0;
        //private long FanFirst;
        //private long FanLast;
        private Form NewForm = null;
        private long TotalFirst;
        private long TotalLast;
        private long BuzzerFirst;
        private long BuzzerLast;
        private bool BuzerOnOff;
        private bool BuzzerRunFlag;
        private short BuzzerOnCount;

        private delegate Color GetBackColor(int Row, int Column);
        private delegate bool GetCurrentPos(int Step);
        private delegate void ScreenDisplayToAdData();

        private GetBackColor GetCol = null;
        private GetCurrentPos GetCurrCheck = null;
        private ScreenDisplayToAdData ADDisplay = null;

        //private short ImageFirstOutCount = 0;
        private string Model;
        //private bool SWCheckFirstFlag;
        private long SWCheckOFFFirst;
        private long SWCheckOFFLast;
        //private long SWCheckFirst;
        //private long SWCheckLast;
        private __Config__ Config;
        private PowerControl PwrCtrl = null;
        //public bool MultiMesterRangeSetFlag = false;
        //private int CanReadCount;
        private __Infor__ TInfor;
        private bool ProductOutFlag = false;

        //public static ulong[] InData = { 0x0000000000000000, 0x0000000000000000, 0x0000000000000000 };
        //public static byte[,] OutData = { { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
        //public static float CurrData = 0x00;
        //public double PlotMax;
        //public double PlotMin;
        private __CheckItem__ CheckItem;
        private __Data__ TData;
        private bool DialogFlag = false;

        private bool RunningFlag { get; set; }
        private short Step = 0;
        private __CanControl CanCtrl = null;
        private LinControl LinCtrl = null;
        //public static __CanControl[] CanCtrl = new __CanControl[2];
        //private __CanControl CanCtrl = new __CanControl();
        //private int SwitchType = 0;
        //private int CarType = 0;

        private long CanSendFirst;
        private long CanSendLast;

        private PanelMeter pMeter = null;
        //private UsbMultiMeterControl MultiMeter;
        private GW_MultiMeter MultiMeter;
        //public double[] ADData = { 0, 0, 0, 0, 0 };

        private int RowCount;

        private Color SelectOnColor;
        private Color SelectOffColor;

        private CanMap CanReWrite = null;

        private __MODEL SelectModel = __MODEL.NONE;

        public MainForm()
        {
            InitializeComponent();
        }


        public __Config__ GetConfig
        {
            get { return Config; }
            set{  Config = value; }
        }

        public bool isRunning
        {
            get
            {
                return RunningFlag;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Config = new __Config__();
            TSpec = new __Spec__();

            CheckItem = new __CheckItem__();

            TData.HeaterLamp.Result = new RESULT[3];
            TData.VentLamp.Result = new RESULT[3];


            TInfor = new __Infor__();
            SetDoubleBuffered(fpSpread1);

            CheckForIllegalCrossThreadCalls = false;

            ConfigSetting Set = new ConfigSetting();
            Config = Set.ReadWriteConfig;

            //CreateFileName();
            OpenInfor();

            fpSpread2.Visible = false;
#if PROGRAM_RUNNING


            TCPOpen();

            if (SerialOpen() == false)
            {
            }

            CanCtrl = new __CanControl(this);
            CanCtrl.OpenCan(0, CanPosition(0), (short)Config.Can1.Speed, false);
            //CanCtrl.OpenCan(1, CanPosition(1), (short)Config.Can2.Speed, false);
            LinCtrl = new LinControl(false, this);
            LinCtrl.LinOpen(LinPosition(0, Config.Lin1.Device), LinControl.HW_MODE.SLAVE, 3);
            LinCtrl.LinOpen(LinPosition(1, Config.Lin2.Device), LinControl.HW_MODE.SLAVE, 3);
            CanReWrite = new CanMap(this);

            //Thread UdpThread = new Thread(new ThreadStart(UdpThreadRun));
            //UdpThread.Start();
            //UdpWrite("Are you alive?");
#endif
            LoadCarModel();

            //CreateDataFile();

            if (0 < comboBox1.Items.Count)
            {
                comboBox1.SelectedIndex = 0;
            }
            /*
            switchRotary3.Items[0].Text = "DE";
            switchRotary3.Items[1].Text = "BSD";
            switchRotary3.Items[2].Text = "SP2";
            */
            //if (UdpOpenFlag == true) UdpRead();
            IOPort.YellowOnOff = true;


            //if (TSpec.Offset.CanType == true)
            //    
            //else 
            //CanCtrl[0] = new __CanControl();
            //CanCtrl[1] = new __CanControl();

            //CanCtrl[0].OpenCan(Config.LowCan.Port, Config.LowCan.Speed);
            //CanCtrl[1].OpenCan(Config.HighCan.Port, Config.HighCan.Speed);


            SelectOffColor = fpSpread1.ActiveSheet.Cells[0, 0].BackColor;
            SelectOnColor = Color.FromArgb(172, 227, 175);

            //fpSpread1.ActiveSheet.Cells[9, 0].ForeColor = Color.Silver;
            //fpSpread1.ActiveSheet.Cells[9, 0].BackColor = SelectOffColor;
            //for (int i = 1; i < 7; i++)
            //{
            //    fpSpread1.ActiveSheet.Cells[9, i].ForeColor = Color.Silver;
            //    fpSpread1.ActiveSheet.Cells[10, i].ForeColor = Color.Silver;
            //    fpSpread1.ActiveSheet.Cells[11, i].ForeColor = Color.Silver;
            //    fpSpread1.ActiveSheet.Cells[12, i].ForeColor = Color.Silver;
            //    if (i <= 2)
            //    {
            //        fpSpread1.ActiveSheet.Cells[9, i].BackColor = SelectOffColor;
            //        fpSpread1.ActiveSheet.Cells[10, i].BackColor = SelectOffColor;
            //        fpSpread1.ActiveSheet.Cells[11, i].BackColor = SelectOffColor;
            //        fpSpread1.ActiveSheet.Cells[12, i].BackColor = SelectOffColor;
            //    }
            //}

            //fpSpread1.ActiveSheet.Cells[13, 0].ForeColor = Color.Silver;
            //fpSpread1.ActiveSheet.Cells[13, 0].BackColor = SelectOffColor;

            //for (int i = 1; i < 7; i++)
            //{
            //    fpSpread1.ActiveSheet.Cells[13, i].ForeColor = Color.Silver;
            //    fpSpread1.ActiveSheet.Cells[14, i].ForeColor = Color.Silver;
            //    fpSpread1.ActiveSheet.Cells[15, i].ForeColor = Color.Silver;
            //    fpSpread1.ActiveSheet.Cells[16, i].ForeColor = Color.Silver;
            //    if (i <= 2)
            //    {
            //        fpSpread1.ActiveSheet.Cells[13, i].BackColor = SelectOffColor;
            //        fpSpread1.ActiveSheet.Cells[14, i].BackColor = SelectOffColor;
            //        fpSpread1.ActiveSheet.Cells[15, i].BackColor = SelectOffColor;
            //        fpSpread1.ActiveSheet.Cells[16, i].BackColor = SelectOffColor;
            //    }
            //}

            return;
        }
        private void TCPOpen()
        {
            MESCtrl = new MES_Control(ClientIp: Config.Client, ServerIp: Config.Server, mControl: this);
            MESCtrl.Open();
            IOPort = new IOControl(Board: Config.Board, PC: Config.PC, mControl: this);
            IOPort.Open(sPort:Config.SerialtypeIOCard.Port, Speed:Config.SerialtypeIOCard.Speed);

            if (IOPort.isOpen == false) MessageBox.Show("I/O Card IP 를 확인해 주십시오.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (IOPort.isConnection == false) MessageBox.Show("I/O Card 와 접속되지 않습니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (MESCtrl.isClientConnection == false)
            {
                // MessageBox.Show("서버와 접속되지 않습니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return;
        }

        public bool isExit
        {
            get { return ExitFlag; }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (RunningFlag == true) return;
            DialogFlag = true;

            //if (MessageBox.Show("프로그램을 종료 하시겠습니까?", "확인", MessageBoxButtons.OKCancel) == DialogResult.OK)
            //{
            panel1.Visible = false;
            ExitFlag = true;
            timer1.Enabled = false;
            timer2.Enabled = false;
            timer3.Enabled = false;
            IOPort.IOInit();

            CloseForm CForm = new CloseForm();

            CForm.FormClosing += delegate (object Sender1, FormClosingEventArgs e1)
            {
#if PROGRAM_RUNNING
                IOPort.UdpClose();
                MESCtrl.Close();

                if (CanCtrl.isOpen(0) == true) CanCtrl.CanClose(0);
                if (CanCtrl.isOpen(1) == true) CanCtrl.CanClose(1);
                if ((LinCtrl.isOpen(LinChannel[0]) == true) || (LinCtrl.isOpen(LinChannel[1]) == true)) LinCtrl.LinClose();

                if (MultiMeter.isOpen == true) MultiMeter.Close();
                if (pMeter.isOpen == true) pMeter.Close();
                if (ScannerSerial.IsOpen == true) ScannerSerial.Close();
                if (PowerSerial.IsOpen == true) PowerSerial.Close();

#endif
                    e.Cancel = false;

                    //이게 없으면 실제 종료가 되지 않는다.
                    //반드시 this.Dispose(); 함수를 호출 해야만 한다.
                    this.Dispose();
            };
            CForm.StartPosition = FormStartPosition.CenterParent;
            CForm.Show();
            e.Cancel = true;
            //}
            //else
            //{
            //    e.Cancel = true;
            //    DialogFlag = false;
            //}
            return;
        }

        private int SelectMenu = 0;

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            int x = Cursor.Position.X;
            int y = Cursor.Position.Y;

            SelectMenu = 0;

            if (ActiveForm == null) return;

            if (panel1.Visible == true)
            {
                if (((ActiveForm.Left + panel1.Left) <= x) && (x <= (ActiveForm.Left + panel1.Left + panel1.Width)))
                {
                    if (((ActiveForm.Top + panel1.Top) <= y) && (y <= (ActiveForm.Top + panel1.Top + panel1.Height)))
                    {
                        x = x - (panel1.Left + ActiveForm.Left);
                        y = y - (panel1.Top + ActiveForm.Top);

                        //폼 캡션등이 크기 때문에 그 만큼 제한다.
                        x = x - 5;
                        y = y - 31;
                        /*
                        if ((linkLabel1.Left <= x) && (x <= (linkLabel1.Left + linkLabel1.Width)))
                        {
                            if ((linkLabel1.Top <= y) && (y <= (linkLabel1.Top + linkLabel1.Height)))
                            {
                                if (linkLabel1.BorderStyle == BorderStyle.None) linkLabel1.BorderStyle = BorderStyle.FixedSingle;
                                SelectMenu = 1;
                                
                            }
                            else
                            {
                                if (linkLabel1.BorderStyle == BorderStyle.FixedSingle) linkLabel1.BorderStyle = BorderStyle.None;
                            }
                        }
                        else
                        {
                            if (linkLabel1.BorderStyle == BorderStyle.FixedSingle) linkLabel1.BorderStyle = BorderStyle.None;
                        }

                        if ((linkLabel2.Left <= x) && (x <= (linkLabel2.Left + linkLabel2.Width)))
                        {
                            if ((linkLabel2.Top <= y) && (y <= (linkLabel2.Top + linkLabel2.Height)))
                            {
                                if (linkLabel2.BorderStyle == BorderStyle.None) linkLabel2.BorderStyle = BorderStyle.FixedSingle;
                                SelectMenu = 2;
                            }
                            else
                            {
                                if (linkLabel2.BorderStyle == BorderStyle.FixedSingle) linkLabel2.BorderStyle = BorderStyle.None;
                            }
                        }
                        else
                        {
                            if (linkLabel2.BorderStyle == BorderStyle.FixedSingle) linkLabel2.BorderStyle = BorderStyle.None;
                        }
                        if ((linkLabel3.Left <= x) && (x <= (linkLabel3.Left + linkLabel3.Width)))
                        {
                            if ((linkLabel3.Top <= y) && (y <= (linkLabel3.Top + linkLabel3.Height)))
                            {
                                if (linkLabel3.BorderStyle == BorderStyle.None) linkLabel3.BorderStyle = BorderStyle.FixedSingle;
                                SelectMenu = 3;
                            }
                            else
                            {
                                if (linkLabel3.BorderStyle == BorderStyle.FixedSingle) linkLabel3.BorderStyle = BorderStyle.None;
                            }
                        }
                        else
                        {
                            if (linkLabel3.BorderStyle == BorderStyle.FixedSingle) linkLabel3.BorderStyle = BorderStyle.None;
                        }

                        if ((linkLabel4.Left <= x) && (x <= (linkLabel4.Left + linkLabel4.Width)))
                        {
                            if ((linkLabel4.Top <= y) && (y <= (linkLabel4.Top + linkLabel4.Height)))
                            {
                                if (linkLabel4.BorderStyle == BorderStyle.None) linkLabel4.BorderStyle = BorderStyle.FixedSingle;
                                SelectMenu = 4;
                            }
                            else
                            {
                                if (linkLabel4.BorderStyle == BorderStyle.FixedSingle) linkLabel4.BorderStyle = BorderStyle.None;
                            }
                        }
                        else
                        {
                            if (linkLabel4.BorderStyle == BorderStyle.FixedSingle) linkLabel4.BorderStyle = BorderStyle.None;
                        }

                        if ((linkLabel5.Left <= x) && (x <= (linkLabel5.Left + linkLabel5.Width)))
                        {
                            if ((linkLabel5.Top <= y) && (y <= (linkLabel5.Top + linkLabel5.Height)))
                            {
                                if (linkLabel5.BorderStyle == BorderStyle.None) linkLabel5.BorderStyle = BorderStyle.FixedSingle;
                                SelectMenu = 5;
                            }
                            else
                            {
                                if (linkLabel5.BorderStyle == BorderStyle.FixedSingle) linkLabel5.BorderStyle = BorderStyle.None;
                            }
                        }
                        else
                        {
                            if (linkLabel5.BorderStyle == BorderStyle.FixedSingle) linkLabel5.BorderStyle = BorderStyle.None;
                        }

                        if ((linkLabel6.Left <= x) && (x <= (linkLabel6.Left + linkLabel6.Width)))
                        {
                            if ((linkLabel6.Top <= y) && (y <= (linkLabel6.Top + linkLabel6.Height)))
                            {
                                if (linkLabel6.BorderStyle == BorderStyle.None) linkLabel6.BorderStyle = BorderStyle.FixedSingle;
                                SelectMenu = 6;
                            }
                            else
                            {
                                if (linkLabel6.BorderStyle == BorderStyle.FixedSingle) linkLabel6.BorderStyle = BorderStyle.None;
                            }
                        }
                        else
                        {
                            if (linkLabel6.BorderStyle == BorderStyle.FixedSingle) linkLabel6.BorderStyle = BorderStyle.None;
                        }
                        */
                    }
                }
            }
            else
            {
                x = x - ActiveForm.Left;
                y = y - ActiveForm.Top;

                //폼 캡션등이 크기 때문에 그 만큼 제한다.
                x = x - 5;
                y = y - 31;

                if (((linkLabel7.Left + panel2.Left) <= x) && (x <= (linkLabel7.Left + linkLabel7.Width + panel2.Left)))
                {
                    if (((linkLabel7.Top + panel2.Top) <= y) && (y <= (linkLabel7.Top + linkLabel7.Height + panel2.Top)))
                    {
                        if (linkLabel7.BorderStyle == BorderStyle.None) linkLabel7.BorderStyle = BorderStyle.FixedSingle;
                        SelectMenu = 7;
                    }
                    else
                    {
                        if (linkLabel7.BorderStyle == BorderStyle.FixedSingle) linkLabel7.BorderStyle = BorderStyle.None;
                    }
                }
                else
                {
                    if (linkLabel7.BorderStyle == BorderStyle.FixedSingle) linkLabel7.BorderStyle = BorderStyle.None;
                }
            }
            return;
        }


        private void linkLabel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (RunningFlag == true) return;
            if ((IOPort.GetAuto == true) && (NewForm == null)) return;
            if (NewForm == null)
            {
                SelectMenu = 7;
                MenuClick();
            }
            else if (NewForm.Text == "")
            {
                SelectMenu = 7;
                MenuClick();
            }
            return;
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (RunningFlag == true) return;
            if ((IOPort.GetAuto == true) && (NewForm == null)) return;


            if (NewForm == null)
            {
                SelectMenu = 7;
                MenuClick();
            }
            else if (NewForm.Text == "")
            {
                SelectMenu = 7;
                MenuClick();
            }
            return;
        }
        //private bool SelfMOde = false;
        private void MenuClick()
        {
            DialogFlag = true;
            switch (SelectMenu)
            {
                case 1: //스팩 설정
                    if (imageButton2.ButtonText == "로그아웃")
                    {
                        panel1.Visible = false;
                        //SpecSet Set = new SpecSet(Model);
                        //Set.TopMost = true;
                        //Set.Show();

                        NewForm = new SpecSet(this, Model);
                        NewForm.TopMost = true;

                        //아래와 같이 해 주면 폼을 닫을때 Dialog로 오픈을 하지 않아도 {} 안을 실행하게 된다. 동시에 해당 폼의 FormClosing이 동시에 실행되므로 Dialog로 오픈한것 같은 효과를 얻는다.
                        NewForm.FormClosing += delegate (object sender, FormClosingEventArgs e)
                        {
                            SpecSet xForm = (NewForm as SpecSet);
                            Model = xForm.SpecName;
                            NewForm.Dispose();
                            NewForm = null;

                            LoadCarModel();

                            if (Model != null)
                            {
                                if (Model != "")
                                {
                                    if (0 < comboBox1.Items.Count)
                                    {
                                        if (comboBox1.SelectedItem != null)
                                        {
                                            if (comboBox1.SelectedItem.ToString() != Model) comboBox1.SelectedItem = Model;
                                        }
                                        else
                                        {
                                            comboBox1.SelectedItem = Model;
                                        }

                                        string xName = comboBox1.SelectedItem.ToString();

                                        if(xName == "KA4 2열 7/8P") xName = "KA4 2열 78P";

                                        string sName = Program.SPEC_PATH.ToString() + "\\" + xName + ".spc";
                                        if (File.Exists(sName) == true)
                                        {
                                            TSpec = ComF.ReadSpec(sName);                                            
                                        }
                                    }
                                }
                            }
                            DisplaySpec();
                        };

                        NewForm.Shown += delegate (object sender2, EventArgs e2)
                        {
                            NewForm.Location = new Point((this.Width / 2) - (NewForm.Width / 2), 20);
                        };
                        NewForm.Show();
                        /*
                        Model = Set.SpecName;

                        LoadCarModel();

                        if (Model != null)
                        {
                            if (Model != "")
                            {
                                if (0 < comboBox1.Items.Count)
                                {
                                    if (comboBox1.SelectedItem != null)
                                    {
                                        if (comboBox1.SelectedItem.ToString() != Model) comboBox1.SelectedItem = Model;
                                    }
                                    else
                                    {
                                        comboBox1.SelectedItem = Model;
                                    }
                                    Name = Program.SPEC_PATH.ToString() + "\\" + comboBox1.SelectedItem.ToString() + ".spc";
                                    if (File.Exists(Name) == true)
                                    {
                                        TSpec = ComF.ReadSpec(Name);
                                        DisplaySpec();
                                    }
                                }
                            }
                        }
                        */
                    }
                    else
                    {
                        //MessageBox.Show("로그인을 먼저 진행해 주십시오.");
                        label25.Text = "로그인을 먼저 진행해 주십시오.";
                        label25.Visible = true;
                        ComF.timedelay(1000);
                        label25.Visible = false;
                    }
                    break;
                case 2: //로그인
                    if (imageButton2.ButtonText == "로그인")
                    {
                        PasswordCheckForm pass = new PasswordCheckForm();
                        //아래와 같이 해 주면 폼을 닫을때 Dialog로 오픈을 하지 않아도 {} 안을 실행하게 된다. 동시에 해당 폼의 FormClosing이 동시에 실행되므로 Dialog로 오픈한것 같은 효과를 얻는다.
                        pass.FormClosing += delegate (object sender, FormClosingEventArgs e)
                        {
                            if (pass.result == true)
                            {
                                imageButton2.ButtonText = "로그아웃";
                            }
                        };
                        pass.Show();
                    }
                    else
                    {
                        imageButton2.ButtonText = "로그인";
                    }
                    break;
                case 3: //점검
                    if (imageButton2.ButtonText == "로그아웃")
                    {
                        //SelfMOde = true;           
                        panel1.Visible = false;

                        SelfTest Self = new SelfTest(this);
                        Self.TopMost = false;
                        SelfTestMode = true;
                        Self.FormClosing += delegate (object sender, FormClosingEventArgs e)
                        {
                            //SelfMOde = false;                        
                            SelfTestMode = false;
                        };
                        Self.Show();

                        Self.Shown += delegate (object sender2, EventArgs e2)
                        {
                            Self.Location = new Point((this.Width / 2) - (Self.Width / 2), 20);
                        };
                    }
                    else
                    {
                        //MessageBox.Show("로그인을 먼저 진행해 주십시오.");
                        label25.Text = "로그인을 먼저 진행해 주십시오.";
                        label25.Visible = true;
                        ComF.timedelay(1000);
                        label25.Visible = false;
                    }
                    break;
                case 4: //옵션
                    if (imageButton2.ButtonText == "로그아웃")
                    {
                        panel1.Visible = false;
                        OptionSet OptSet = new OptionSet(this);
                        OptSet.TopMost = true;
                        OptSet.Show();

                        OptSet.Shown += delegate (object sender2, EventArgs e2)
                        {
                            OptSet.Location = new Point((this.Width / 2) - (OptSet.Width / 2), 50);
                        };
                    }
                    else
                    {
                        //MessageBox.Show("로그인을 먼저 진행해 주십시오.");
                        label25.Visible = true;
                        ComF.timedelay(1000);
                        label25.Visible = false;
                    }
                    break;
                case 5://종료
                    this.Close();
                    break;
                case 6://데이타
                    panel1.Visible = false;
                    System.Diagnostics.Process.Start("explorer.exe", Program.DATA_PATH.ToString());
                    break;
                case 7:
                    panel1.Visible = true;
                    panel1.BringToFront();
                    break;
                default:
                    panel1.Visible = false;
                    break;
            }
            DialogFlag = false;
            //DisplayImage(Angle);
            return;
        }
        /*
        private void HeaterPictureDisplay(short Pos)
        {
            if (HeaterPos == Pos) return;

            string Path = Program.IMAGE_PATH.ToString();

            switch (Pos)
            {
                case 1:
                    Path = Path + "\\Heater1단.jpg";
                    break;
                case 2:
                    Path = Path + "\\Heater2단.jpg";
                    break;
                case 3:
                    Path = Path + "\\Heater3단.jpg";
                    break;
                default:
                    Path = Path + "\\HeaterOFF.jpg";
                    break;
            }
            pictureBox1.Image = Image.FromFile(Path);
            HeaterPos = Pos;
            return;
        }

        private void DisplayImage(int pang = 0)
        {
            int CenX = (pictureBox2.Width / 2) - 25;
            int CenY = (pictureBox2.Height / 2) - 25;

            int sr;
            int lr;
            int sang;
            int x1, y1, x2, y2, x3; //,y3;
            int tTop = 40;
            int ProfallaNo = 6;
                        
            g1.Clear(Color.White);
            

            sr = 1 * (tTop + 0) + 4;
            lr = 2 * (tTop + 0) + 4;

            
            x1 = 0;
            x2 = 0;
            y1 = 0;
            y2 = 0;
                        
            g1.DrawEllipse(new Pen(Color.Black, 2), new Rectangle(new Point(CenX - 80, CenY - 80), new Size(200, 200)));


            g1.FillEllipse(new SolidBrush(Color.Blue), new Rectangle(new Point(CenX,CenY), new Size(40 ,40)));
            g1.DrawEllipse(new Pen(Color.Black, 2), new Rectangle(new Point(CenX, CenY), new Size(41, 41)));

            for (sang = pang; sang < (360 + pang); sang += (360 / ProfallaNo))
            {
                x1 = (int)(CenX + sr * Math.Cos((double)-sang * (Math.PI / 180)));
                y1 = (int)(CenY + sr * Math.Sin((double)-sang * (Math.PI / 180)));
                x2 = (int)(CenX + lr * Math.Cos((double)-sang * (Math.PI / 180)));
                y2 = (int)(CenY + lr * Math.Sin((double)-sang * (Math.PI / 180)));
                
                if (sang == pang)
                {
                    g1.FillEllipse(new SolidBrush(Color.Yellow), new Rectangle(new Point((x1 + x2) / 2, ((y1 + y2) / 2)), new Size(40, 40)));
                    g1.DrawEllipse(new Pen(Color.Black, 2), new Rectangle(new Point((x1 + x2) / 2, ((y1 + y2) / 2)), new Size(41, 41)));                    
                }
                else
                {
                    g1.DrawEllipse(new Pen(Color.Black,2), new Rectangle(new Point((x1 + x2) / 2, ((y1 + y2) / 2)), new Size(41, 41)));
                }
            }            
            if(ExitFlag == false) g2.DrawImage(backbuffer, 0, 0);
            return;
        }
        */
        private void MainForm_Shown(object sender, EventArgs e)
        {
            label2.Text = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();

            ADDisplay += DisplayAdData1;
            ADDisplay += DisplayAdData2;
            ADDisplay += DisplayAdData3;
            ADDisplay += DisplayAdData4;

            timer1.Enabled = true;
            timer2.Enabled = true;
            timer3.Enabled = true;
            //HeaterPos = -1;
            //HeaterPictureDisplay(0);
            //VentPos = 0;
            return;
        }



        public static void SetDoubleBuffered(Control control)
        {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(Control).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, control, new object[] { true });
            return;
        }

        //public static int OutPos = 0;
        //private long OutPosFirst;
        //private long OutPosLast;


        private void timer1_Tick(object sender, EventArgs e)
        {
            //bool Flag = false;

            timer1.Enabled = false;

            
            if (IOPort != null)
            {
                if (IOPort.isOpen == true)
                {
                    if (led1.Value.AsBoolean != IOPort.isConnection) led1.Value.AsBoolean = IOPort.isConnection;
                }
                else
                {
                    if (led1.Value.AsBoolean != false) led1.Value.AsBoolean = false;
                }
            }
            else
            {
                if (led1.Value.AsBoolean != false) led1.Value.AsBoolean = false;
            }

            if(MESCtrl != null)
            {
                if (led10.Value.AsBoolean != MESCtrl.isClientConnection) led10.Value.AsBoolean = MESCtrl.isClientConnection;
            }
            else
            {
                if (led10.Value.AsBoolean != false) led10.Value.AsBoolean = false;
            }

            
            if (BuzzerRunFlag == true)
            {
                BuzzerLast = ComF.timeGetTimems();
                if (BuzerOnOff == true)
                {
                    if (700 <= (BuzzerLast - BuzzerFirst))
                    {
                        BuzerOnOff = false;
                        IOPort.BuzzerOnOff = BuzerOnOff;
                        //IOPort.outportb(IO_OUT.BUZZER, BuzerOnOff);
                        BuzzerFirst = ComF.timeGetTimems();
                        BuzzerOnCount++;
                    }
                }
                else
                {
                    if (500 <= (BuzzerLast - BuzzerFirst))
                    {
                        if (BuzzerOnCount < 4)
                        {
                            BuzerOnOff = true;
                            //IOPort.outportb(IO_OUT.BUZZER, BuzerOnOff);
                            IOPort.BuzzerOnOff = BuzerOnOff;
                            BuzzerFirst = ComF.timeGetTimems();
                        }
                        else
                        {
                            BuzerOnOff = false;
                            //IOPort.outportb(IO_OUT.BUZZER, BuzerOnOff);
                            IOPort.BuzzerOnOff = BuzerOnOff;
                            BuzzerOnCount = 0;
                            BuzzerRunFlag = false;
                            //SaveDataFlag = true;
                            //SaveData();
                        }
                    }
                }
            }


            if (MESCtrl.isClientConnection == true)
            {
                if (MESCtrl.isReading == true)
                {
                    if (RunningFlag == false)
                    {
                        PopData = MESCtrl.GetReadData;
                        CheckPopData();
                        SaveLogData = "Receive - " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
                        SaveLogData = MESCtrl.SourceData;
                    }
                    MESCtrl.isReading = false;
                }                
            }

            timer1.Enabled = !ExitFlag;
            return;
        }

        private bool PopDataReadFlag = false;
        private bool OldPopDataReadFlag = false;

        MES_Control.__ReadMesData__ PopData = new MES_Control.__ReadMesData__()
        {
            Barcode = null,
            Check = null,
            Date = null,
            LineCode = null,
            MachineNo = null
        };
        private void CheckPopData()
        {
            if (PopData.Check == null) return;
            label18.Text = label17.Text;
            label17.Text = PopData.Barcode; 
            textBox1.Text = PopData.Barcode;
            label23.Text = PopData.Check;
            MESCtrl.isReading = false;
            
            if (label5.Text != "대기중 입니다.")
            {
                label5.Text = "대기중 입니다.";
                ScreenInit();
                sevenSegmentInteger4.Value.AsInteger = 0;
                IOPort.GreenOnOff = false;
                //outportb(IO_OUT.YELLOW, false);
                IOPort.YellowOnOff = true;
                IOPort.RedOnOff = false;
            }
            if (label16.Text != "")
            {
                label16.Text = "";
                label16.BackColor = Color.Black;
                label16.ForeColor = Color.Gray;
                RunningFlag = false;
            }

            label16.Text = "";

            if (IOPort.GetAuto == true)
            {
                PopDataReadFlag = true;
                if (PopData.Check.Substring(0, 1) == "1")
                {
                    //일반 히터
                    CheckItem.Heater.Lamp = true;
                    CheckItem.Heater.Curr = true;
                    CheckItem.SWHeater = true;
                }
                else
                {
                    CheckItem.Heater.Lamp = false;
                    CheckItem.Heater.Curr = false;
                    CheckItem.SWHeater = false;
                }

                //Aribag
                if (PopData.Check.Substring(2, 1) == "1")
                {
                    //CheckItem.AirBag = true;
                    //CheckItem.SWSAB = true;
                }
                else
                {
                    //CheckItem.AirBag = false;
                    //CheckItem.SWSAB = false;
                }

                //버클 센서
                if (PopData.Check.Substring(3, 1) == "1")
                {
                    //CheckItem.BuckleSensor = true;
                    //CheckItem.SWBuckleSensor = true;
                }
                else
                {
                    //CheckItem.BuckleSensor = false;
                    //CheckItem.SWBuckleSensor = false;
                }

                if (PopData.Check.Substring(5, 1) == "1")
                {
                    //통풍 
                    CheckItem.SWVent = true;
                    CheckItem.Vent.Lamp = true;
                    CheckItem.Vent.Curr = true;
                }
                else
                {
                    //검사 없음
                    CheckItem.SWVent = false;
                    CheckItem.Vent.Lamp = false;
                    CheckItem.Vent.Curr = false;
                }

                
                //버클 워닝/SBR 워닝

                if (PopData.Check.Substring(4, 1) == "1")
                {
                    CheckItem.BuckleWarning = true;
                    CheckItem.SWBuckleWar = true;

                }
                else
                {
                    CheckItem.BuckleWarning = false;
                    CheckItem.SWBuckleWar = false;
                }

                //파워 , IMS
                if (PopData.Check.Substring(6, 1) == "1")
                {
                    CheckItem.PowerSW = true;
                    //CheckItem.Can = false;
                    //CheckItem.Ims = false;
                }
                else
                {
                    CheckItem.PowerSW = false;
                    //CheckItem.Can = false;
                    //CheckItem.Ims = false;
                }

                //Lumber
                if (PopData.Check.Substring(7, 1) == "1")
                {
                }
                else
                {
                }

                //리트렉터
                if (PopData.Check.Substring(8, 1) == "1")
                {
                    CheckItem.Retractor = true;
                    CheckItem.SWRetractor = true;
                }
                else
                {
                    CheckItem.Retractor = false;
                    CheckItem.SWRetractor = false;
                }
                //SBR 메트
                if (PopData.Check.Substring(9, 1) == "1")
                {
                    CheckItem.SBR = true;
                    CheckItem.SWSBR = true;
                }
                else
                {
                    CheckItem.SBR = false;
                    CheckItem.SWSBR = false;
                }

                //Can
                if (PopData.Check.Substring(17, 1) == "1")
                {
                    CheckItem.CanCheck = true;
                }
                else
                {
                    CheckItem.CanCheck = false;
                }

                //if ((CheckItem.SWVent == true) && (CheckItem.SWHeater == true))
                if (CheckItem.SWVent == true)
                    CheckItem.Lin = true;
                else CheckItem.Lin = false;

                //Limber
                if (PopData.Check.Substring(6, 1) == "1")
                {
                    //CheckItem.Lumber.FW = true;
                    //CheckItem.Lumber.BW = true;
                    //CheckItem.Lumber.Up = true;
                    //CheckItem.Lumber.Dn = true;
                }
                else
                {
                    //CheckItem.Lumber.FW = false;
                    //CheckItem.Lumber.BW = false;
                    //CheckItem.Lumber.Up = false;
                    //CheckItem.Lumber.Dn = false;
                }

                if (PopData.Check.Substring(18, 1) == "1")
                {
                    SelectModel = __MODEL.MODEL_78P;
                }
                else if (PopData.Check.Substring(18, 1) == "1")
                {
                    SelectModel = __MODEL.MODEL_78P;
                }
                else
                {
                    SelectModel = __MODEL.MODEL_11P;
                }

                if (CheckItem.CanCheck == true)
                {
                    if (comboBox5.SelectedItem != null)
                    {
                        if (comboBox5.SelectedItem.ToString() != "CAN") comboBox5.SelectedItem = "CAN";
                    }
                    else
                    {
                        comboBox5.SelectedItem = "CAN";
                    }
                }
                else
                {
                    if (comboBox5.SelectedItem != null)
                    {
                        if (comboBox5.SelectedItem.ToString() != "NONE CAN") comboBox5.SelectedItem = "NONE CAN";
                    }
                    else
                    {
                        comboBox5.SelectedItem = "NONE CAN";
                    }
                }

                if (PopData.Check.Substring(16, 1) == "1")
                {
                    if (comboBox3.SelectedItem != null)
                    {
                        if (comboBox3.SelectedItem.ToString() != "RH") comboBox3.SelectedItem = "RH";
                    }
                    else
                    {
                        comboBox3.SelectedItem = "RH";
                    }
                }
                else
                {
                    if (comboBox3.SelectedItem != null)
                    {
                        if (comboBox3.SelectedItem.ToString() != "LH") comboBox3.SelectedItem = "LH";
                    }
                    else
                    {
                        comboBox3.SelectedItem = "LH";
                    }
                }

                if (((CheckItem.SWHeater == true) && (CheckItem.SWVent == true)) || 
                    ((CheckItem.SWHeater == false) && (CheckItem.SWVent == true)))
                {
                    CheckItem.LampTo2Wire = false;
                    if (comboBox6.SelectedItem != null)
                    {
                        if (comboBox6.SelectedItem.ToString() != "3WIRE") comboBox6.SelectedItem = "3WIRE";
                    }
                    else
                    {
                        comboBox6.SelectedItem = "3WIRE";
                    }
                }
                else
                {
                    if (comboBox6.SelectedItem != null)
                    {
                        if (comboBox6.SelectedItem.ToString() != "2WIRE") comboBox6.SelectedItem = "2WIRE";
                    }
                    else
                    {
                        comboBox6.SelectedItem = "2WIRE";
                    }
                    CheckItem.LampTo2Wire = true;
                }
                

                bool Flag = true;
                if (comboBox1.SelectedItem != null)
                {
                    if (SelectModel == __MODEL.MODEL_11P)
                    {
                        if (comboBox1.SelectedItem.ToString() != "KA4 2열 11P")
                        {
                            comboBox1.SelectedItem = "KA4 2열 11P";
                            Flag = false;
                        }
                    }
                    else if (SelectModel == __MODEL.MODEL_9P)
                    {
                        if (comboBox1.SelectedItem.ToString() != "KA4 2열 9P")
                        {
                            comboBox1.SelectedItem = "KA4 2열 9P";
                            Flag = false;
                        }
                    }
                    else if (SelectModel == __MODEL.MODEL_78P)
                    {
                        if (comboBox1.SelectedItem.ToString() != "KA4 2열 7/8P")
                        {
                            comboBox1.SelectedItem = "KA4 2열 7/8P";
                            Flag = false;
                        }
                    }
                    else
                    {
                        if (comboBox1.SelectedItem.ToString() != "KA4 2열 RELAX")
                        {
                            comboBox1.SelectedItem = "KA4 2열 RELAX";
                            Flag = false;
                        }
                    }
                    //if (SelectModel == __MODEL.MODEL_78P)
                    //{
                    //    if (comboBox1.SelectedItem.ToString() != "KA4 2열 78P")
                    //    {
                    //        comboBox1.SelectedItem = "KA4 2열 78P";
                    //        Flag = false;
                    //    }
                    //}
                    //else
                    //{
                    //    if (comboBox1.SelectedItem.ToString() != "KA4")
                    //    {
                    //        comboBox1.SelectedItem = "KA4";
                    //        Flag = false;
                    //    }
                    //}
                }
                else
                {
                    if (SelectModel == __MODEL.MODEL_11P)
                    {
                        comboBox1.SelectedItem = "KA4 2열 11P";
                        Flag = false;
                    }
                    else if (SelectModel == __MODEL.MODEL_9P)
                    {
                        comboBox1.SelectedItem = "KA4 2열 9P";
                        Flag = false;
                    }
                    else if (SelectModel == __MODEL.MODEL_78P)
                    {
                        comboBox1.SelectedItem = "KA4 2열 7/8P";
                        Flag = false;
                    }
                    else
                    {
                        comboBox1.SelectedItem = "KA4 2열 RELAX";
                        Flag = false;
                    }
                    //if (SelectModel == __MODEL.MODEL_78P)
                    //{
                    //    comboBox1.SelectedItem = "KA4 2열 78P";
                    //}
                    //else
                    //{
                    //    comboBox1.SelectedItem = "KA4";
                    //}                    
                }


                DisplaySpec();

                if (isCheckItem == false)
                {
                    label5.Text = "검사 사양이 아닙니다.";
                    PopDataReadFlag = false;
                }
            }
            else
            {
                OldPopDataReadFlag = true;
            }
            return;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            string s = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
            if (label2.Text != s) label2.Text = s;

            if (TInfor.Date != DateTime.Now.ToString("yyyyMMdd"))
            {
                string dPath = Program.DATA_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMM") + ".xls";
                TInfor.Date = DateTime.Now.ToString("yyyyMMdd");
                TInfor.DataName = dPath;
                TInfor.Count.Total = 0;
                TInfor.Count.OK = 0;
                TInfor.Count.NG = 0;
                TInfor.ReBootingFlag = false;
                TInfor.Model = comboBox1.SelectedItem.ToString();
                SaveInfor();
            }
            return;
        }


        private bool PassButtonCheckFlag = false;
        //private bool IOLeftSelect { get; set; }
        //private bool IOFwdSelect { get; set; }
        //private bool IO1StDownSelect { get; set; }
        //private bool IO2StDownSelect { get; set; }
        private void timer3_Tick(object sender, EventArgs e)
        {
            //테스트 타임머
            try
            {
                timer3.Enabled = false;

               //this.Text = IOPort.GetInData[1].ToString("X");

                if ((DateTime.Now.Hour == 6) && (DateTime.Now.Minute == 1))
                {
                    if (TInfor.ReBootingFlag == false)
                    {
                        TInfor.ReBootingFlag = true;
                        SaveInfor();
                        ComF.WindowRestartTo30Secconds();

                        System.Diagnostics.Process[] mProcess = System.Diagnostics.Process.GetProcessesByName(Application.ProductName);
                        foreach (System.Diagnostics.Process p in mProcess) p.Kill();
                    }
                }
                if (SbrTestEndToJigMoveFlag == true) JigOff();


                if ((IOPort.Get2StUp1Sensor == true) && (IOPort.Get2StUp2Sensor == true) && (IOPort.Get1StUpSensor == true) && (IOPort.GetRearSensor == true))
                {
                    if (IOPort.SetOriginOnOff == true) IOPort.SetOriginOnOff = false;
                }
                else
                {
                    if (IOPort.SetOriginOnOff == false) IOPort.SetOriginOnOff = true;
                }

                if (IOPort.SetOriginOnOff != led8.Value.AsBoolean) led8.Value.AsBoolean = IOPort.SetOriginOnOff;

                if (ExitFlag == false)
                {
                    if ((DialogFlag == false) && (SelfTestMode == false))
                    {
                        bool[] LampFlag =
                        {
                            CanReWrite.LH_HeaterHighLamp,//0
                            CanReWrite.LH_HeaterMidLamp,//1
                            CanReWrite.LH_HeaterLowLamp,//2
                            CanReWrite.LH_VentHighLamp,//3
                            CanReWrite.LH_VentMidLamp,//4
                            CanReWrite.LH_VentLowLamp,//5

                            CanReWrite.RH_HeaterHighLamp,//6
                            CanReWrite.RH_HeaterMidLamp,//7
                            CanReWrite.RH_HeaterLowLamp,//8
                            CanReWrite.RH_VentHighLamp,//9
                            CanReWrite.RH_VentMidLamp,//10
                            CanReWrite.RH_VentLowLamp//11
                        };

                        bool[] LampFlag2 = new bool[3];
                                                
                        if(((InCheck(IO_IN_FUNC.DRV_HEATER_LOW) == true) && (InCheck(IO_IN_FUNC.DRV_HEATER_HIGH) == true) && (InCheck(IO_IN_FUNC.DRV_HEATER_MID) == false)) ||
                            ((InCheck(IO_IN_FUNC.PASS_HEATER_LOW) == true) && (InCheck(IO_IN_FUNC.PASS_HEATER_HIGH) == true) && (InCheck(IO_IN_FUNC.PASS_HEATER_MID) == false)))
                        {
                            LampFlag2[0] = true; //High
                            LampFlag2[1] = true; //Mid
                            LampFlag2[2] = true; //Low
                        }
                        else if (((InCheck(IO_IN_FUNC.DRV_HEATER_LOW) == false) && (InCheck(IO_IN_FUNC.DRV_HEATER_HIGH) == true) && (InCheck(IO_IN_FUNC.DRV_HEATER_MID) == false)) ||
                            ((InCheck(IO_IN_FUNC.PASS_HEATER_LOW) == false) && (InCheck(IO_IN_FUNC.PASS_HEATER_HIGH) == true) && (InCheck(IO_IN_FUNC.PASS_HEATER_MID) == false)))
                        {
                            //Mid
                            //High
                            LampFlag2[0] = false; //High
                            LampFlag2[1] = true; //Mid
                            LampFlag2[2] = true; //Low
                        }
                        else if (((InCheck(IO_IN_FUNC.DRV_HEATER_LOW) == true) && (InCheck(IO_IN_FUNC.DRV_HEATER_HIGH) == false) && (InCheck(IO_IN_FUNC.DRV_HEATER_MID) == false)) ||
                            ((InCheck(IO_IN_FUNC.PASS_HEATER_LOW) == true) && (InCheck(IO_IN_FUNC.PASS_HEATER_HIGH) == false) && (InCheck(IO_IN_FUNC.PASS_HEATER_MID) == false)))
                        {
                            //Mid
                            //High
                            LampFlag2[0] = false; //High
                            LampFlag2[1] = false; //Mid
                            LampFlag2[2] = true; //Low
                        }
                        else
                        {
                            //Off
                            LampFlag2[0] = false; //High
                            LampFlag2[1] = false; //Mid
                            LampFlag2[2] = false; //Low
                        }

                        if (CheckItem.LampTo2Wire == true)
                        {
                            if ((LampFlag[2] == true) || (LampFlag[8] == true) || (LampFlag2[2] == true))
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 3].BackColor != Color.Lime) fpSpread1.ActiveSheet.Cells[3, 3].BackColor = Color.Lime;
                            }
                            else
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 3].BackColor != Color.White) fpSpread1.ActiveSheet.Cells[3, 3].BackColor = Color.White;
                            }

                            if ((LampFlag[1] == true) || (LampFlag[7] == true) || (LampFlag2[1] == true))
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 4].BackColor != Color.Lime) fpSpread1.ActiveSheet.Cells[3, 4].BackColor = Color.Lime;
                            }
                            else
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 4].BackColor != Color.White) fpSpread1.ActiveSheet.Cells[3, 4].BackColor = Color.White;
                            }

                            if ((LampFlag[0] == true) || (LampFlag[6] == true) || (LampFlag2[0] == true))
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 5].BackColor != Color.Lime) fpSpread1.ActiveSheet.Cells[3, 5].BackColor = Color.Lime;
                            }
                            else
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 5].BackColor != Color.White) fpSpread1.ActiveSheet.Cells[3, 5].BackColor = Color.White;
                            }
                        }
                        else
                        {
                            if ((InCheck(IO_IN_FUNC.DRV_HEATER_LOW) == true) || (InCheck(IO_IN_FUNC.PASS_HEATER_LOW) == true) || (LampFlag[2] == true) || (LampFlag[8] == true))
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 3].BackColor != Color.Lime) fpSpread1.ActiveSheet.Cells[3, 3].BackColor = Color.Lime;
                            }
                            else
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 3].BackColor != Color.White) fpSpread1.ActiveSheet.Cells[3, 3].BackColor = Color.White;
                            }

                            if ((InCheck(IO_IN_FUNC.DRV_HEATER_MID) == true) || (InCheck(IO_IN_FUNC.PASS_HEATER_MID) == true) || (LampFlag[1] == true) || (LampFlag[7] == true))
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 4].BackColor != Color.Lime) fpSpread1.ActiveSheet.Cells[3, 4].BackColor = Color.Lime;
                            }
                            else
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 4].BackColor != Color.White) fpSpread1.ActiveSheet.Cells[3, 4].BackColor = Color.White;
                            }

                            if ((InCheck(IO_IN_FUNC.DRV_HEATER_HIGH) == true) || (InCheck(IO_IN_FUNC.PASS_HEATER_HIGH) == true) || (LampFlag[0] == true) || (LampFlag[6] == true))
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 5].BackColor != Color.Lime) fpSpread1.ActiveSheet.Cells[3, 5].BackColor = Color.Lime;
                            }
                            else
                            {
                                if (fpSpread1.ActiveSheet.Cells[3, 5].BackColor != Color.White) fpSpread1.ActiveSheet.Cells[3, 5].BackColor = Color.White;
                            }


                            if ((InCheck(IO_IN_FUNC.DRV_VENT_LOW) == true) || (InCheck(IO_IN_FUNC.PASS_VENT_LOW) == true) || (LampFlag[5] == true) || (LampFlag[11] == true))
                            {
                                if (fpSpread1.ActiveSheet.Cells[5, 3].BackColor != Color.Lime) fpSpread1.ActiveSheet.Cells[5, 3].BackColor = Color.Lime;
                            }
                            else
                            {
                                if (fpSpread1.ActiveSheet.Cells[5, 3].BackColor != Color.White) fpSpread1.ActiveSheet.Cells[5, 3].BackColor = Color.White;
                            }

                            if ((InCheck(IO_IN_FUNC.DRV_VENT_MID) == true) || (InCheck(IO_IN_FUNC.PASS_VENT_MID) == true) || (LampFlag[4] == true) || (LampFlag[10] == true))
                            {
                                if (fpSpread1.ActiveSheet.Cells[5, 4].BackColor != Color.Lime) fpSpread1.ActiveSheet.Cells[5, 4].BackColor = Color.Lime;
                            }
                            else
                            {
                                if (fpSpread1.ActiveSheet.Cells[5, 4].BackColor != Color.White) fpSpread1.ActiveSheet.Cells[5, 4].BackColor = Color.White;
                            }
                            if ((InCheck(IO_IN_FUNC.DRV_VENT_HIGH) == true) || (InCheck(IO_IN_FUNC.PASS_VENT_HIGH) == true) || (LampFlag[3] == true) || (LampFlag[9] == true))
                            {
                                if (fpSpread1.ActiveSheet.Cells[5, 5].BackColor != Color.Lime) fpSpread1.ActiveSheet.Cells[5, 5].BackColor = Color.Lime;
                            }
                            else
                            {
                                if (fpSpread1.ActiveSheet.Cells[5, 5].BackColor != Color.White) fpSpread1.ActiveSheet.Cells[5, 5].BackColor = Color.White;
                            }
                        }
                        ADDisplay();
                    }


                    
                    //if (MultiMeter != null)
                    //{
                    //    ADData[ADPos.MULTI_METER] = MultiMeter.GetReadData;
                    //}

                    if ((DialogFlag == false) && (SelfTestMode == false))
                    {
                        DisplayIOIn();

                        if(IOPort.GetPassButton == true)
                        {
                            if (PassButtonCheckFlag == false)
                            {
                                if (RunningFlag == false)
                                {
                                    bool xFlag = false;
                                    bool Flag = false;

                                    if (IOPort.GetAuto == true)
                                    {
                                        if(PopDataReadFlag == false) PopDataReadFlag = OldPopDataReadFlag;
                                        if ((label16.Text == "") && (PopDataReadFlag == true))
                                        {
                                            if (JigUpFlag == true) Flag = true;
                                            Flag = true;
                                        }
                                        else if ((label16.Text == "대기중") && (PopDataReadFlag == true))
                                        {
                                            if (JigUpFlag == true) Flag = true;
                                            Flag = true;
                                        }

                                        if(Flag == true)
                                        {
                                            if (isCheckItem == false)
                                            {
                                                xFlag = true;
                                                PopDataReadFlag = false;
                                                Flag = false;
                                                IOPort.TestINGOnOff = false;
                                                IOPort.TestOKOnOff = true;
                                            }                                            
                                        }                                      
                                    }
                                    else
                                    {
                                        if (label16.Text == "")
                                        {
                                            Flag = true;
                                        }
                                        else if (label16.Text == "대기중")
                                        {
                                            if (JigUpFlag == true) Flag = true;                                            
                                        }
                                    }

                                    if (led4.Value.AsBoolean == true)//제품이 있을 경우
                                    {
                                        if (Flag == true)
                                        {
                                            StartSetting();
                                        }
                                        else if ((label16.Text != "NG") && (xFlag == false))
                                        {
                                            if (SbrTestEndToJigMoveFlag == false)
                                            {
                                                PassButtonCheckFlag = false;
                                                BatteryOnOff(false);
                                                IOPort.TestOKOnOff = false;
                                                IOPort.TestINGOnOff = false;
                                                led5.Value.AsBoolean = false; // Test ING                                    
                                                label16.Text = "";
                                                RunningFlag = false;
                                                label5.Text = "제품 배출중 입니다.";
                                                led6.Value.AsBoolean = true; // Test OK
                                                IOPort.TestOKOnOff = true;
                                                IOPort.TestINGOnOff = false;
                                                label16.BackColor = Color.Black;
                                                ComF.timedelay(700);
                                                IOPort.TestOKOnOff = false;
                                                led6.Value.AsBoolean = false; // Test OK
                                                ProductOutFlag = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            PassButtonCheckFlag = false;
                        }

                        if(IOPort.GetResetButton == true)
                        {
                            if (led4.Value.AsBoolean == true)//제품이 있을 경우
                            {
                                if (RunningFlag == true)
                                {
                                    BatteryOnOff(false);
                                    label16.Text = "";
                                    label5.Text = "제품 검사 중지\n재 대기중";

                                    IOPort.TestOKOnOff = false;
                                    IOPort.TestINGOnOff = false;
                                    label16.BackColor = Color.Black;
                                    RunningFlag = false;
                                    BatteryOnOff(false);
                                    ScreenInit();

                                    if (CheckItem.SBR == true)
                                    {
                                        SbrTestEndToJigMoveFlag = true;
                                        JigOffStep = 0;
                                    }
                                }
                                else
                                {
                                    
                                    if (ProductOutFlag == false)
                                    {
                                        if (label16.Text != "")
                                        {
                                            IOPort.YellowOnOff = true;
                                            IOPort.RedOnOff = false;
                                            BatteryOnOff(false);
                                            label16.Text = "";
                                            label5.Text = "PASS 버튼을 눌러 검사를 진행하십시오.";
                                            IOPort.TestOKOnOff = false;
                                            IOPort.TestINGOnOff = false;
                                            label16.BackColor = Color.Black;
                                            label16.ForeColor = Color.Gray;
                                            ScreenInit();
                                            //if (CanCtrl != null)
                                            //{
                                            //if (CanCtrl.isOpen() == true) CanCtrl.CanClose();
                                            //}
                                            RunningFlag = false;
                                        }
                                    }
                                    else
                                    {
                                        if (label16.Text != "")
                                        {
                                            IOPort.YellowOnOff = true;
                                            IOPort.RedOnOff = false;
                                            BatteryOnOff(false);
                                            label16.Text = "";
                                            label5.Text = "PASS 버튼을 눌러 검사를 진행하십시오.";
                                            IOPort.TestOKOnOff = false;
                                            IOPort.TestINGOnOff = false;
                                            label16.BackColor = Color.Black;
                                            label16.ForeColor = Color.Gray;
                                            ScreenInit();
                                            //if (CanCtrl != null)
                                            //{
                                            //if (CanCtrl.isOpen() == true) CanCtrl.CanClose();
                                            // }
                                            RunningFlag = false;
                                        }
                                        else
                                        {
                                            if(IOPort.GetAuto == false)
                                            {
                                                if (label16.Text == "")
                                                {
                                                    IOPort.YellowOnOff = true;
                                                    IOPort.RedOnOff = false;
                                                    BatteryOnOff(false);
                                                    label16.Text = "";
                                                    label5.Text = "PASS 버튼을 눌러 검사를 진행하십시오.";
                                                    IOPort.TestOKOnOff = false;
                                                    IOPort.TestINGOnOff = false;
                                                    label16.BackColor = Color.Black;
                                                    label16.ForeColor = Color.Gray;
                                                    ScreenInit();
                                                    //if (CanCtrl != null)
                                                    //{
                                                    //if (CanCtrl.isOpen() == true) CanCtrl.CanClose();
                                                    // }
                                                    RunningFlag = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (label16.Text != "")
                                {
                                    BatteryOnOff(false);
                                    label16.Text = "";
                                    RunningFlag = false;
                                    label16.BackColor = Color.Black;
                                    label16.ForeColor = Color.Gray;
                                    ScreenInit();
                                }
                            }
                        }


                        if (IOPort.GetAuto == true)
                        {
                            if (led3.Value.AsBoolean == false) led3.Value.AsBoolean = true;
                            if (led2.Indicator.Text != "자동")
                            {
                                led2.Indicator.Text = "자동";
                                CheckPopData();
                            }
                            if (label32.Visible == false)
                            {
                                label32.Visible = true;
                                label32.BringToFront();
                            }
                            if (label17.Visible == false) label17.Visible = true;
                            if (textBox1.Visible == true) textBox1.Visible = false;
                        }
                        else
                        {
                            if (led3.Value.AsBoolean == true) led3.Value.AsBoolean = false;
                            if (led2.Indicator.Text != "수동") led2.Indicator.Text = "수동";
                            if(comboBox1.Enabled == false) comboBox1.Enabled = true;
                            if(label32.Visible == true) label32.Visible = false;

                            if (label17.Visible == true) label17.Visible = false;
                            if (textBox1.Visible == false) textBox1.Visible = true;

                            if (SelfTestMode == false)
                            {
                                if (RunningFlag == false) DisplayPanelSW();

                                //if (imageButton9.ButtonColor == Color.Red)
                                //{
                                if (IOPort.Get1StUpSensor == true)
                                {
                                    if (IOPort.GetLeftSelect == true)
                                    {
                                        //if (IOLeftSelect == false)
                                        //{
                                        //    IOLeftSelect = true;
                                        IOPort.JigLeftRightMove = true;
                                        //}
                                    }
                                    if (IOPort.GetRightSelect == true)
                                    {
                                        //if (IOLeftSelect == false)
                                        //{
                                        //    IOLeftSelect = true;
                                        IOPort.JigLeftRightMove = false;
                                        //}
                                    }
                                    //if (IOLeftSelect == true)
                                    // {
                                    //     IOLeftSelect = false;
                                    //     IOPort.JigLeftRightMove = false;
                                    // }

                                    if (IOPort.GetFwdSelect == true)
                                    {
                                        //if (IOFwdSelect == false)
                                        //{
                                        //    IOFwdSelect = true;
                                        IOPort.JigFwdMove = true;
                                        //}
                                    }
                                    if (IOPort.GetBwdSelect == true)
                                    {
                                        //if (IOFwdSelect == false)
                                        //{
                                        //    IOFwdSelect = true;
                                        IOPort.JigFwdMove = false;
                                        //}
                                    }
                                    //else
                                    //{
                                    //    if (IOFwdSelect == true)
                                    //    {
                                    //        IOFwdSelect = false;
                                    //        IOPort.JigFwdMove = false;
                                    //    }
                                    //}
                                }
                                if (IOPort.Get1StDnSelect == true)
                                {
                                    //if (IO1StDownSelect == false)
                                    //{
                                    //    IO1StDownSelect = true;
                                    IOPort.Jig1StDown = true;
                                    //}
                                }
                                if (IOPort.Get1StUpSelect == true)
                                {
                                    //if (IO1StDownSelect == false)
                                    //{
                                    //    IO1StDownSelect = true;
                                    IOPort.Jig1StDown = false;
                                    //}
                                }
                                //else
                                //{
                                //    if (IO1StDownSelect == true)
                                //    {
                                //        IO1StDownSelect = false;
                                //        IOPort.Jig1StDown = false;
                                //    }
                                //}
                                //if (IOPort.Get1StDnSensor == true)
                                //{
                                if (IOPort.Get2StDnSelect == true)
                                {
                                    //if (IO2StDownSelect == false)
                                    //{
                                    //    IO2StDownSelect = true;
                                    IOPort.Jig2StDown = true;
                                    //}
                                }
                                if (IOPort.Get2StUpSelect == true)
                                {
                                    //if (IO2StDownSelect == false)
                                    //{
                                    //    IO2StDownSelect = true;
                                    IOPort.Jig2StDown = false;
                                    //}
                                }
                                //else
                                //{
                                //    if (IO2StDownSelect == true)
                                //    {
                                //        IO2StDownSelect = false;
                                //        IOPort.Jig2StDown = false;
                                //    }
                                //}
                                //}
                                //}
                            }
                        }

                        if (ProductOutFlag == false)
                        {
                            if (led4.Value.AsBoolean == true) //제품 감지
                            {
                                if (led9.Value.AsBoolean == true) //지그 업
                                {
                                    if (label16.Text == "")
                                    {
                                        if (label5.Text != "컨넥터를 채결 하시고 PASS 버툰을 눌러 검사를 진행하십시오.") label5.Text = "컨넥터를 채결 하시고 PASS 버툰을 눌러 검사를 진행하십시오.";
                                        //outportb(IO_OUT.YELLOW, true);
                                        IOPort.YellowOnOff = true;
                                        label16.Text = "대기중";
                                        label16.BackColor = Color.Black;
                                        label16.ForeColor = Color.Silver;
                                    }
                                }
                            }
                        }

                        if (IOPort.SetProductIn != led7.Value.AsBoolean) led7.Value.AsBoolean = IOPort.SetProductIn;

                        if (IOPort.GetProductIn == true)
                        {
                            if (ProductOutFlag == false)
                            {
                                if (led4.Value.AsBoolean == false)
                                {
                                    led4.Value.AsBoolean = true;
                                    IOPort.SetProductIn = true;
                                }
                            }
                            JigUpCount = 0;
                        }
                        else if(RunningFlag == false)
                        {
                            bool Flag = false;
                            ulong Inx;

                            if (TSpec.Offset.IGN == false)
                            {
                                
                                Inx = 0x01 << (int)IO_OUT_FUNC.IGN1_BATT;
                                if ((IOPort.GetInData[0] & Inx) == Inx) Flag = true;
                                Inx = 0x01 << (int)IO_OUT_FUNC.IGN2_BATT;
                                if ((IOPort.GetInData[0] & Inx) == Inx) Flag = true;
                            }
                            else
                            {
                                Inx = 0x01 << (int)IO_OUT_FUNC.IGN1_GND;
                                if ((IOPort.GetInData[0] & Inx) == Inx) Flag = true;
                                Inx = 0x01 << (int)IO_OUT_FUNC.IGN2_GND;
                                if ((IOPort.GetInData[0] & Inx) == Inx) Flag = true;
                            }
                            if (Flag == true) BatteryOnOff(false);
                            if (ProductOutFlag == true) ProductOutFlag = false;

                            if (RunningFlag == false)
                            {
                                if (IOPort.GetAuto == false)
                                {
                                    if (label5.Text != "대기중 입니다.")
                                    {
                                        label5.Text = "대기중 입니다.";
                                        ScreenInit();
                                        sevenSegmentInteger4.Value.AsInteger = 0;
                                        IOPort.GreenOnOff = false;
                                        //outportb(IO_OUT.YELLOW, false);
                                        IOPort.YellowOnOff = false;
                                        IOPort.RedOnOff = false;
                                    }
                                    if (label16.Text != "")
                                    {
                                        label16.Text = "";
                                        label16.BackColor = Color.Black;
                                        label16.ForeColor = Color.Gray;
                                        RunningFlag = false;
                                    }
                                }
                            }
                            if (led4.Value.AsBoolean == true)
                            {
                                led4.Value.AsBoolean = false;
                                IOPort.SetProductIn = false;
                            }

                            if (JigUpFlag == true)
                            {
                                JigUpCount++;
                                if (10 <= JigUpCount) JigUpFlag = false;
                            }

                            if(JigUpFlag == false)
                            {
                                if (IOPort.TestINGOnOff == true) IOPort.TestINGOnOff = false;
                            }
                        }

                        /* POP연동이면 이 주석으로된 부분을 사용
                        if (led3.Value.AsBoolean == false) //수동이면
                        {
                            In = 0x01 << (int)IO_IN.LH_SELECT;
                            if ((InData[1] & In) == In)
                            {
                                if (switchRotary1.Value != 0) switchRotary1.Value = 0;
                                if (comboBox3.SelectedIndex != 0) comboBox3.SelectedIndex = 0;
                            }

                            In = 0x01 << (int)IO_IN.RH_SELECT;
                            if ((InData[1] & In) == In)
                            {
                                if (switchRotary1.Value != 1) switchRotary1.Value = 1;
                                if (comboBox3.SelectedIndex != 1) comboBox3.SelectedIndex = 1;
                            }

                            In = 0x01 << (int)IO_IN.LHRH_SELECT;
                            if ((InData[1] & In) == In)
                            {
                                if (switchRotary1.Value != 2) switchRotary1.Value = 2;
                                if (comboBox3.SelectedIndex != 2) comboBox3.SelectedIndex = 2;
                            }
                        }
                        */

                        //ulong In;

                        //In = 0x01 << (int)IO_OUT.TEST_OK;
                        //if ((IOPort.GetInData[0] & In) == In)
                        //{
                        //    if (led6.Value.AsBoolean == false) led6.Value.AsBoolean = true;
                        //}
                        //else
                        //{
                        //    if (led6.Value.AsBoolean == true) led6.Value.AsBoolean = false;
                        //}

                        if (led6.Value.AsBoolean != IOPort.TestOKOnOff) led6.Value.AsBoolean = IOPort.TestOKOnOff;

                        if (RunningFlag == false)
                        {
                            if (IOPort.OutputCheck(IO_OUT.TEST_ING) == true) IOPort.TestINGOnOff = false;
                        }


                        //In = 0x01 << (int)IO_OUT.TEST_ING;
                        //if ((IOPort.GetInData[0] & In) == In)
                        //{
                        //    if (led5.Value.AsBoolean == false) led5.Value.AsBoolean = true;
                        //}
                        //else
                        //{
                        //    if (led5.Value.AsBoolean == true) led5.Value.AsBoolean = false;
                        //}

                        if (led5.Value.AsBoolean != IOPort.TestINGOnOff) led5.Value.AsBoolean = IOPort.TestINGOnOff;

                        /*
                        In = 0x01 << (int)IO_OUT.PRODUCT;
                        if ((InData[2] & In) == In)
                        {
                            if (led8.Value.AsBoolean == false)
                            {
                                led8.Value.AsBoolean = true;
                                outportb(IO_OUT.PRODUCT, true);
                            }
                        }
                        else
                        {
                            if (led8.Value.AsBoolean == true)
                            {
                                outportb(IO_OUT.PRODUCT, false);
                                led8.Value.AsBoolean = false;
                            }
                        }
                        */
                        /*
                        In = 0x01 << (int)IO_OUT.PRODUCT_HORIZENTAL;
                        if ((InData[2] & In) == In)
                        {
                            if (led7.Value.AsBoolean == false) led7.Value.AsBoolean = true;
                        }
                        else
                        {
                            if (led7.Value.AsBoolean == true) led7.Value.AsBoolean = false;
                        }
                        */
                        if (RunningFlag == true)
                        {
                            Processing();
                            Processing2();
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
                timer3.Enabled = !ExitFlag;
            }
            return;
        }


        //public static ulong[] InData = { 0x0000000000000000, 0x0000000000000000 };
        //public static ulong[] OutData = { 0x0000000000000000, 0x0000000000000000 };

        //private void CheckUdpData(byte[] data,int Length)
        //{
        //    try
        //    {
        //        //this.Text = data;
        //        if (0 < Length)
        //        {
        //            int CanID = 0;
        //            float Ad;
        //            ushort x = 0;
        //            int DataLength;
        //            ulong[] cData = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        //            CanID = (int)(((data[0] & 0xff) << 24) & 0xff000000);
        //            CanID |= (int)(((data[1] & 0xff) << 16) & 0x00ff0000);
        //            CanID |= (int)(((data[2] & 0xff) << 8) & 0x0000ff00);
        //            CanID |= (int)(((data[3] & 0xff) << 0) & 0x000000ff);
        //            DataLength = data[4];

        //            for (int i = 0; i < DataLength; i++) cData[i] = data[i + 5];



        //            switch (CanID)
        //            {
        //                case 0x150: // Product In
        //                    InData[0] = (cData[4] << 0) | (cData[5] << 8) | (cData[6] << 16) | (cData[7] << 24);
        //                    InData[2] = (cData[0] << 0) | (cData[1] << 8) | (cData[2] << 16) | (cData[3] << 24);

        //                    break;
        //                case 0x130: // 3232 In                            
        //                    InData[1] = (cData[4] << 0) | (cData[5] << 8) | (cData[6] << 16) | (cData[7] << 24);

        //                    //string s = string.Format("{0:X} {1:X} {2:X} {3:X} ", cData[4], cData[5], cData[6], cData[7]);
        //                    //this.Text = s;
        //                    break;
        //                case 0x160:
        //                    x = (ushort)((cData[1] << 0) | (cData[2] << 8));

        //                    Ad = (float)(x * (5.0 / 4096.0));
        //                    //0 V일때 2.6V 가 뜬다고 함

        //                    if (2.6 <= CurrData)
        //                    {
        //                        Ad = (float)((CurrData - 2.6) * (30.0 / 2.4)); //2.5V 30A
        //                    }
        //                    else
        //                    {
        //                        Ad = (float)((2.6 - CurrData) * (-30.0 / 2.4)); //2.5V 30A
        //                    }
        //                    CurrData = Ad;
        //                    break;
        //            }

        //            //Application.DoEvents() 를 사용하면 에러 발생 (이 함수고 Callback 으로 호출되어서 그런다.
        //            //Application.DoEvents();
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        MessageBox.Show(exp.Message + "\n" + exp.StackTrace);
        //    }

        //    return;
        //}

        //private void outportb(IO_OUT Out, bool OnOff)
        //{
        //    byte Data = 0x00;

        //    int Pos = (int)Out / 8;
        //    int dPos = (int)Out % 8;

        //    Data = (byte)(0x01 << dPos);

        //    if(OnOff == true)
        //        OutData[0,Pos] |= Data; 
        //    else OutData[0,Pos] &= (byte)(~Data);
        //    //OutPos = 1;
        //    UpdWrite2();
        //    return;
        //}

        //private void outportb(IO_OUT_FUNC Out, bool OnOff)
        //{
        //    byte Data = 0x00;

        //    int Pos = (int)Out / 8;
        //    int dPos = (int)Out % 8;

        //    Data = (byte)(0x01 << dPos);

        //    if (OnOff == true)
        //        OutData[1, Pos] |= Data;
        //    else OutData[1, Pos] &= (byte)(~Data);
        //    //OutPos = 1;
        //    UpdWrite2();
        //    return;
        //}

        //private void IOInit()
        //{
        //    for (int i = 0; i < 8; i++)
        //    {
        //        OutData[0, i] = 0x00;
        //        OutData[1, i] = 0x00;
        //    }
        //    return;
        //}


        private bool SerialOpen()
        {
            bool Flag = false;


#if PROGRAM_RUNNING
            PwrCtrl = new PowerControl(Config.Power);
            if (PwrCtrl.IsOpen == false) MessageBox.Show("파워 통신 포트 오픈 실패", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            try
            {
                PowerSerial.Open();
            }
            catch
            {

            }
            finally
            {
                PwrCtrl.POWER_CURRENTSetting(20);
                                
                pMeter = new PanelMeter(this);
                
                pMeter.Open(Config.PanelMeter);
                if (pMeter.isOpen == false) MessageBox.Show("판넬메터 통신 포트 오픈 실패", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //MultiMeter = new UsbMultiMeterControl();
                MultiMeter = new GW_MultiMeter(this);
                MultiMeter.Open(Config.MultiMeter);
                if (MultiMeter.isOpen == false) MessageBox.Show("멀티메터 통신 포트 오픈 실패", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
            }
            return Flag;
        }



        private void ScannerSerial_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            ScannerSerial.DiscardOutBuffer();
            return;
        }


        public bool InCheck(short In)
        {
            bool Flag = false;

            ulong Data = 0x0000000000000000;


            short Pos = (short)(In - IO_IN_FUNC.DRV_HEATER_HIGH);

            Data = (ulong)(0x01 << Pos) & 0xffffffff;

            if ((IOPort.GetInData[0] & Data) == Data) Flag = true;

            return Flag;
        }

        public bool InCheck2(short In)
        {
            bool Flag = false;

            ulong Data = 0x0000000000000000;


            Data = (ulong)(0x01 << In) & 0xffffffff;

            if ((IOPort.GetInData[1] & Data) == Data) Flag = true;

            return Flag;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string sModel;

            string xName = comboBox1.SelectedItem.ToString();

            if (xName == "KA4 2열 7/8P") xName = "KA4 2열 78P";

            string sName = Program.SPEC_PATH.ToString() + "\\" + xName + ".spc";
            if (File.Exists(sName) == true)
            {
                if (Model != comboBox1.SelectedItem.ToString())
                {
                    Model = comboBox1.SelectedItem.ToString();
                    if(IOPort.GetAuto == true)
                        label32.Text = "KA4";
                    else label32.Text = Model;
                    TSpec = ComF.ReadSpec(sName);                    
                }                
            }
            //DisplaySpec();
            return;
        }

        private void DisplaySpec()
        {
            //if (CheckItem.Heater.Curr == true || CheckItem.Heater.Lamp == true || CheckItem.Heater.NTC == true)
            if ((CheckItem.Heater.Lamp == true) || (CheckItem.Heater.Curr == true))
            {
                fpSpread1.ActiveSheet.Cells[2, 0].ForeColor = Color.Black;
                fpSpread1.ActiveSheet.Cells[2, 0].BackColor = SelectOnColor;

                for (int i = 2; i < 7; i++)
                {
                    if (CheckItem.Heater.Curr == true)
                    {
                        fpSpread1.ActiveSheet.Cells[2, i].ForeColor = Color.Black;
                        if (i <= 2) fpSpread1.ActiveSheet.Cells[2, i].BackColor = SelectOnColor;
                    }
                    else
                    {
                        fpSpread1.ActiveSheet.Cells[3, i].ForeColor = Color.Silver;
                        if (i <= 2) fpSpread1.ActiveSheet.Cells[3, i].BackColor = SelectOffColor;
                    }
                    if (CheckItem.Heater.Lamp == true)
                    {
                        fpSpread1.ActiveSheet.Cells[3, i].ForeColor = Color.Black;
                        if (i <= 2) fpSpread1.ActiveSheet.Cells[3, i].BackColor = SelectOnColor;
                    }
                    else
                    {
                        fpSpread1.ActiveSheet.Cells[2, i].ForeColor = Color.Silver;
                        if (i <= 2) fpSpread1.ActiveSheet.Cells[2, i].BackColor = SelectOffColor;
                    }
                }
            }
            else
            {
                fpSpread1.ActiveSheet.Cells[2, 0].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[2, 0].BackColor = SelectOffColor;
                for (int i = 2; i < 7; i++)
                {
                    fpSpread1.ActiveSheet.Cells[2, i].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[3, i].ForeColor = Color.Silver;
                    if (i <= 2)
                    {
                        fpSpread1.ActiveSheet.Cells[2, i].BackColor = SelectOffColor;
                        fpSpread1.ActiveSheet.Cells[3, i].BackColor = SelectOffColor;
                    }
                }
            }


            //if (CheckItem.VentCurr == true || CheckItem.VentLamp == true || CheckItem.VentFan == true)
            if ((CheckItem.Vent.Curr == true) || (CheckItem.Vent.Lamp == true))
            {
                fpSpread1.ActiveSheet.Cells[4, 0].ForeColor = Color.Black;
                fpSpread1.ActiveSheet.Cells[4, 0].BackColor = SelectOnColor;

                for (int i = 2; i < 7; i++)
                {
                    if (CheckItem.Vent.Curr == true)
                    {
                        fpSpread1.ActiveSheet.Cells[4, i].ForeColor = Color.Black;
                        if (i <= 2) fpSpread1.ActiveSheet.Cells[4, i].BackColor = SelectOnColor;
                    }
                    else
                    {
                        fpSpread1.ActiveSheet.Cells[4, i].ForeColor = Color.Silver;
                        if (i <= 2) fpSpread1.ActiveSheet.Cells[4, i].BackColor = SelectOffColor;
                    }

                    if (CheckItem.Vent.Lamp == true)
                    {
                        fpSpread1.ActiveSheet.Cells[5, i].ForeColor = Color.Black;
                        if (i <= 2) fpSpread1.ActiveSheet.Cells[5, i].BackColor = SelectOnColor;
                    }
                    else
                    {
                        fpSpread1.ActiveSheet.Cells[5, i].ForeColor = Color.Silver;
                        if (i <= 2) fpSpread1.ActiveSheet.Cells[5, i].BackColor = SelectOffColor;
                    }
                }
            }
            else
            {
                fpSpread1.ActiveSheet.Cells[4, 0].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[4, 0].BackColor = SelectOffColor;
                for (int i = 2; i < 7; i++)
                {
                    fpSpread1.ActiveSheet.Cells[4, i].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[5, i].ForeColor = Color.Silver;
                    if (i <= 2)
                    {
                        fpSpread1.ActiveSheet.Cells[4, i].BackColor = SelectOffColor;
                        fpSpread1.ActiveSheet.Cells[5, i].BackColor = SelectOffColor;
                    }
                }
            }

            if (CheckItem.BuckleWarning == true)
            {
                fpSpread1.ActiveSheet.Cells[6, 0].ForeColor = Color.Black;
                fpSpread1.ActiveSheet.Cells[6, 0].BackColor = SelectOnColor;
            }
            else
            {
                fpSpread1.ActiveSheet.Cells[6, 0].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[6, 0].BackColor = SelectOffColor;
            }

            if (CheckItem.Retractor == true)
            {
                fpSpread1.ActiveSheet.Cells[7, 0].ForeColor = Color.Black;
                fpSpread1.ActiveSheet.Cells[7, 0].BackColor = SelectOnColor;
            }
            else
            {
                fpSpread1.ActiveSheet.Cells[7, 0].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[7, 0].BackColor = SelectOffColor;
            }


            for (int i = 2; i < 7; i++)
            {
                if (CheckItem.BuckleWarning == true)
                {
                    fpSpread1.ActiveSheet.Cells[6, i].ForeColor = Color.Black;
                    if (i <= 2) fpSpread1.ActiveSheet.Cells[6, i].BackColor = SelectOnColor;
                }
                else
                {
                    fpSpread1.ActiveSheet.Cells[6, i].ForeColor = Color.Silver;
                    if (i <= 2) fpSpread1.ActiveSheet.Cells[6, i].BackColor = SelectOffColor;
                }

                if (CheckItem.Retractor == true)
                {
                    fpSpread1.ActiveSheet.Cells[7, i].ForeColor = Color.Black;
                    if (i <= 2) fpSpread1.ActiveSheet.Cells[7, i].BackColor = SelectOnColor;
                }
                else
                {
                    fpSpread1.ActiveSheet.Cells[7, i].ForeColor = Color.Silver;
                    if (i <= 2) fpSpread1.ActiveSheet.Cells[7, i].BackColor = SelectOffColor;
                }
            }

            if (CheckItem.PowerSW == true)
            {
                fpSpread1.ActiveSheet.Cells[8, 0].ForeColor = Color.Black;
                fpSpread1.ActiveSheet.Cells[8, 0].BackColor = SelectOnColor;

                for (int i = 1; i < 7; i++)
                {
                    fpSpread1.ActiveSheet.Cells[8, i].ForeColor = Color.Black;
                    fpSpread1.ActiveSheet.Cells[9, i].ForeColor = Color.Black;
                    fpSpread1.ActiveSheet.Cells[10, i].ForeColor = Color.Black;
                    fpSpread1.ActiveSheet.Cells[11, i].ForeColor = Color.Black;
                    if (i <= 2)
                    {
                        fpSpread1.ActiveSheet.Cells[8, i].BackColor = SelectOnColor;
                        fpSpread1.ActiveSheet.Cells[9, i].BackColor = SelectOnColor;
                        fpSpread1.ActiveSheet.Cells[10, i].BackColor = SelectOnColor;
                        fpSpread1.ActiveSheet.Cells[11, i].BackColor = SelectOnColor;
                    }
                }
            }
            else
            {
                fpSpread1.ActiveSheet.Cells[8, 0].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[8, 0].BackColor = SelectOffColor;
                for (int i = 1; i < 7; i++)
                {
                    fpSpread1.ActiveSheet.Cells[8, i].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[9, i].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[10, i].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[11, i].ForeColor = Color.Silver;
                    if (i <= 2)
                    {
                        fpSpread1.ActiveSheet.Cells[8, i].BackColor = SelectOffColor;
                        fpSpread1.ActiveSheet.Cells[9, i].BackColor = SelectOffColor;
                        fpSpread1.ActiveSheet.Cells[10, i].BackColor = SelectOffColor;
                        fpSpread1.ActiveSheet.Cells[11, i].BackColor = SelectOffColor;
                    }
                }
            }


            if (CheckItem.SBR == true)
            {
                fpSpread1.ActiveSheet.Cells[12, 0].ForeColor = Color.Black;
                fpSpread1.ActiveSheet.Cells[12, 0].BackColor = SelectOnColor;

                for (int i = 1; i < 7; i++)
                {
                    fpSpread1.ActiveSheet.Cells[12, i].ForeColor = Color.Black;
                    fpSpread1.ActiveSheet.Cells[13, i].ForeColor = Color.Black;
                    fpSpread1.ActiveSheet.Cells[14, i].ForeColor = Color.Black;
                    if (i <= 2)
                    {
                        fpSpread1.ActiveSheet.Cells[12, i].BackColor = SelectOnColor;
                        fpSpread1.ActiveSheet.Cells[13, i].BackColor = SelectOnColor;
                        fpSpread1.ActiveSheet.Cells[14, i].BackColor = SelectOnColor;
                    }
                }
            }
            else
            {
                fpSpread1.ActiveSheet.Cells[12, 0].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[12, 0].BackColor = SelectOffColor;

                for (int i = 1; i < 7; i++)
                {
                    fpSpread1.ActiveSheet.Cells[12, i].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[13, i].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[14, i].ForeColor = Color.Silver;
                    if (i <= 2)
                    {
                        fpSpread1.ActiveSheet.Cells[12, i].BackColor = SelectOffColor;
                        fpSpread1.ActiveSheet.Cells[13, i].BackColor = SelectOffColor;
                        fpSpread1.ActiveSheet.Cells[14, i].BackColor = SelectOffColor;
                    }
                }
            }


            if ((CheckItem.Heater.Curr == true) && (CheckItem.Vent.Curr == false))
                fpSpread1.ActiveSheet.Cells[2, 3].Text = string.Format("{0:0.0} ~ {1:0.00}[A]", TSpec.Heater.Min, TSpec.Heater.Max);
            else fpSpread1.ActiveSheet.Cells[2, 3].Text = string.Format("{0:0.0} ~ {1:0.00}[A]", TSpec.VentHeater.Min, TSpec.VentHeater.Max);
            
            fpSpread1.ActiveSheet.Cells[4, 3].Text = string.Format("{0:0.0} ~ {1:0.00}[A]", TSpec.Vent.Min, TSpec.Vent.Max);
                        
            fpSpread1.ActiveSheet.Cells[7, 3].Text = string.Format("{0:0.0} ~ {1:0.00}[Ω]", TSpec.Retractor.Min, TSpec.Retractor.Max);
            //fpSpread1.ActiveSheet.Cells[11, 3].Text = string.Format("{0:0.0} ~ {1:0.00}[Ω]", TSpec.SBR.Min, TSpec.SBR.Max);

            fpSpread1.ActiveSheet.Cells[8, 3].Text = string.Format("{0:0.0} ~ {1:0.00}[A]", TSpec.PWSwitch.Min, TSpec.PWSwitch.Max);
            fpSpread1.ActiveSheet.Cells[9, 3].Text = string.Format("{0:0.0} ~ {1:0.00}[A]", TSpec.PWSwitch.Min, TSpec.PWSwitch.Max);
            fpSpread1.ActiveSheet.Cells[10, 3].Text = string.Format("{0:0.0} ~ {1:0.00}[A]", TSpec.PWSwitch.Min, TSpec.PWSwitch.Max);
            fpSpread1.ActiveSheet.Cells[11, 3].Text = string.Format("{0:0.0} ~ {1:0.00}[A]", TSpec.PWSwitch.Min, TSpec.PWSwitch.Max);

            fpSpread1.ActiveSheet.Cells[12, 3].Text = string.Format("{0:0}[Ω] 이상", (int)TSpec.SBR.NotLoad);
            fpSpread1.ActiveSheet.Cells[13, 3].Text = string.Format("{0:0}[Ω] 이상", (int)TSpec.SBR.Load15Kg);
            fpSpread1.ActiveSheet.Cells[14, 3].Text = string.Format("{0:0}[Ω] 미만", (int)TSpec.SBR.Load30Kg);
            return;
        }

        private void DisplayPanelSW()
        {
            //ulong In = 0x00;
            bool Flag = false;

            //#if POP_MODE
            if (led3.Value.AsBoolean == true) return;
            //#endif

            //In = 0x01 << IO_IN.LHD_RHD;
            //if ((IOPort.GetInData[1] & In) == In)
            //{
            //    if (comboBox2.SelectedIndex != 0) comboBox2.SelectedIndex = 0;
            //}
            //else
            //{
            //    if (comboBox2.SelectedIndex != 1) comboBox2.SelectedIndex = 1;
            //}

            if (IOPort.GetLhSelect == true)
            {
                //if (switchRotary1.Value.AsInteger != 0) switchRotary1.Value.AsInteger = 0;
                if (comboBox3.SelectedIndex != 0)
                {
                    if (0 < comboBox3.Items.Count) comboBox3.SelectedIndex = 0;
                    Flag = true;
                }
            }
            else
            {
                if (comboBox3.SelectedIndex != 1)
                {
                    if (1 < comboBox3.Items.Count) comboBox3.SelectedIndex = 1;
                    Flag = true;
                }
            }

            if (IOPort.GetHeaterSw == true)
            {
                if (CheckItem.SWHeater == false)
                {
                    Flag = true;
                    CheckItem.SWHeater = true;
                }
            }
            //else if (IOPort.GetHeaterLampTo2Wire == true)
            //{
            //    if (CheckItem.SWHeater == false)
            //    {
            //        Flag = true;
            //        CheckItem.SWHeater = true;
            //    }
            //    if (led11.Value.AsBoolean == false) led11.Value.AsBoolean = true;
            //    if (CheckItem.LampTo2Wire == false) CheckItem.LampTo2Wire = true;
            //}
            else
            {
                if (CheckItem.SWHeater == true)
                {
                    Flag = true;
                    CheckItem.SWHeater = false;
                }
                if (led11.Value.AsBoolean == true) led11.Value.AsBoolean = false;
                if (CheckItem.LampTo2Wire == true) CheckItem.LampTo2Wire = false;
            }

            //if(CheckItem.LampTo2Wire == true)
            //{
            //    if (comboBox6.SelectedIndex != 0) comboBox6.SelectedIndex = 0;
            //}
            //else
            //{
            //    if (comboBox6.SelectedIndex != 1) comboBox6.SelectedIndex = 1;
            //}



            if (IOPort.GetVentSw == true)
            {
                if (CheckItem.SWVent == false)
                {
                    Flag = true;
                    CheckItem.SWVent = true;
                }
            }
            else
            {
                if (CheckItem.SWVent == true)
                {
                    Flag = true;
                    CheckItem.SWVent = false;
                }
            }

            if (((CheckItem.SWHeater == true) && (CheckItem.SWVent == true)) ||
                ((CheckItem.SWHeater == false) && (CheckItem.SWVent == true)))
            {
                CheckItem.LampTo2Wire = false;
                if (comboBox6.SelectedItem != null)
                {
                    if (comboBox6.SelectedItem.ToString() != "3WIRE") comboBox6.SelectedItem = "3WIRE";
                }
                else
                {
                    comboBox6.SelectedItem = "3WIRE";
                }
            }
            else
            {
                if (comboBox6.SelectedItem != null)
                {
                    if (comboBox6.SelectedItem.ToString() != "2WIRE") comboBox6.SelectedItem = "2WIRE";
                }
                else
                {
                    comboBox6.SelectedItem = "2WIRE";
                }
                CheckItem.LampTo2Wire = true;
            }


            if (IOPort.GetBuckleSw == true)
            {
                if (CheckItem.SWBuckleWar == false)
                {
                    Flag = true;
                    CheckItem.SWBuckleWar = true;
                }
            }
            else
            {
                if (CheckItem.SWBuckleWar == true)
                {
                    Flag = true;
                    CheckItem.SWBuckleWar = false;
                }
            }

            if (IOPort.GetRetractorSw == true)
            {
                if (CheckItem.SWRetractor == false)
                {
                    Flag = true;
                    CheckItem.Retractor = true;
                }
            }
            else
            {
                if (CheckItem.SWRetractor == true)
                {
                    Flag = true;
                    CheckItem.Retractor = false;
                }
            }

            if (IOPort.GetSBRSw == true)
            {
                if (CheckItem.SWSBR == false)
                {
                    Flag = true;
                    CheckItem.SBR = true;
                    CheckItem.SWSBR = true;
                }
            }
            else
            {
                if (CheckItem.SWSBR == true)
                {
                    Flag = true;
                    CheckItem.SBR = false;
                    CheckItem.SWSBR = false;
                }
            }

            if (IOPort.GetPSeatSw == true)
            {
                if (CheckItem.PowerSW == false)
                {
                    Flag = true;
                    CheckItem.PowerSW = true;
                }
            }
            else
            {
                if (CheckItem.PowerSW == true)
                {
                    Flag = true;
                    CheckItem.PowerSW = false;
                }
            }


            if (IOPort.GetCanType == true)
            {
                if (CheckItem.CanCheck == false) CheckItem.CanCheck = true;
            }
            else
            {
                if (CheckItem.CanCheck == true) CheckItem.CanCheck = false;
            }

            //#if !POP_MODE
            CheckRunItem();

            if (/*(CheckItem.SWHeater == true) && */(CheckItem.SWVent == true))
                CheckItem.Lin = true;
            else CheckItem.Lin = false;

            if ((CheckItem.CanCheck == true) && (comboBox5.SelectedItem.ToString() != "CAN")) 
                comboBox5.SelectedItem = "CAN";
            else if ((CheckItem.CanCheck == false) && (comboBox5.SelectedItem.ToString() != "NONE CAN")) 
                comboBox5.SelectedItem = "NONE CAN";


            bool DisplayFlag = Flag;

            if (SelectModel != IOPort.GetModel)
            {
                SelectModel = IOPort.GetModel;
                if (comboBox1.SelectedItem != null)
                {
                    if (SelectModel == __MODEL.MODEL_11P)
                    {
                        if (comboBox1.SelectedItem.ToString() != "KA4 2열 11P")
                        {
                            comboBox1.SelectedItem = "KA4 2열 11P";
                            Flag = false;
                        }
                    }
                    else if (SelectModel == __MODEL.MODEL_9P)
                    {
                        if (comboBox1.SelectedItem.ToString() != "KA4 2열 9P")
                        {
                            comboBox1.SelectedItem = "KA4 2열 9P";
                            Flag = false;
                        }
                    }
                    else if (SelectModel == __MODEL.MODEL_78P)
                    {
                        if (comboBox1.SelectedItem.ToString() != "KA4 2열 7/8P")
                        {
                            comboBox1.SelectedItem = "KA4 2열 7/8P";
                            Flag = false;
                        }
                    }
                    else
                    {
                        if (comboBox1.SelectedItem.ToString() != "KA4 2열 RELAX")
                        {
                            comboBox1.SelectedItem = "KA4 2열 RELAX";
                            Flag = false;
                        }
                    }

                    //if (SelectModel == __MODEL.MODEL_78P)
                    //{
                    //    if (comboBox1.SelectedItem.ToString() != "KA4 78P")
                    //    {
                    //        comboBox1.SelectedItem = "KA4 78P";
                    //        Flag = false;
                    //    }
                    //}
                    //else
                    //{
                    //    if (comboBox1.SelectedItem.ToString() != "KA4")
                    //    {
                    //        comboBox1.SelectedItem = "KA4";
                    //        Flag = false;
                    //    }
                    //}
                }
                else
                {
                    if (SelectModel == __MODEL.MODEL_11P)
                    {
                        comboBox1.SelectedItem = "KA4 2열 11P";
                    }
                    else if (SelectModel == __MODEL.MODEL_9P)
                    {
                        comboBox1.SelectedItem = "KA4 2열 9P";
                    }
                    else if (SelectModel == __MODEL.MODEL_78P)
                    {
                        comboBox1.SelectedItem = "KA4 2열 7/8P";
                    }
                    else
                    {
                        comboBox1.SelectedItem = "KA4 2열 RELAX";
                    }

                    //if (SelectModel == __MODEL.MODEL_78P)
                    //{
                    //    comboBox1.SelectedItem = "KA4 78P";
                    //}
                    //else
                    //{
                    //    comboBox1.SelectedItem = "KA4";
                    //}
                    Flag = false;
                }
            }

            if (DisplayFlag == true)
            {
                DisplaySpec();
            }
            //#endif

        }

        private void CheckRunItem()
        {
            if (RunningFlag == true) return;

            if(IOPort.GetVentSw == true)
            {
                CheckItem.Vent.Curr = true;
                CheckItem.Vent.Lamp = true;
                CheckItem.SWVent = true;
            }
            else
            {
                CheckItem.Vent.Curr = false;
                CheckItem.Vent.Lamp = false;
                CheckItem.SWVent = false; ;
            }

            if (IOPort.GetHeaterSw == true)
            {
                CheckItem.Heater.Curr = true;
                CheckItem.Heater.Lamp = true;
                CheckItem.SWHeater = true;
            }
            else if (IOPort.GetHeaterLampTo2Wire == true)
            {
                CheckItem.Heater.Curr = true;
                CheckItem.Heater.Lamp = true;
                CheckItem.SWHeater = true;
                if (CheckItem.LampTo2Wire == false) CheckItem.LampTo2Wire = true;
            }
            else
            {
                CheckItem.Heater.Curr = false;
                CheckItem.Heater.Lamp = false;
                CheckItem.SWHeater = false;
                if (CheckItem.LampTo2Wire == true) CheckItem.LampTo2Wire = false;
            }

            if (CheckItem.LampTo2Wire == true)
            {
                if (comboBox6.SelectedIndex != 0) comboBox6.SelectedIndex = 0;
            }
            else
            {
                if (comboBox6.SelectedIndex != 1) comboBox6.SelectedIndex = 1;
            }

            if (IOPort.GetBuckleSw == true)
            {
                CheckItem.BuckleWarning = true;
                CheckItem.SWBuckleWar = true;
            }
            else
            {
                CheckItem.BuckleWarning = false;
                CheckItem.SWBuckleWar = false;
            }

            if (IOPort.GetRetractorSw == true)
            {
                CheckItem.Retractor = true;
                CheckItem.SWRetractor = true;
            }
            else
            {
                CheckItem.Retractor = false;
                CheckItem.SWRetractor = false;
            }

            if (IOPort.GetSBRSw == true)
            {
                CheckItem.SBR = true;
            }
            else
            {
                CheckItem.SBR = false;
            }

            return;
        }

        private void BatteryOnOff(bool OnOff)
        {
            if (OnOff == true)
            {
                IOPort.outportbToFunction(IO_OUT_FUNC.PSEAT_BATT, false);
                //PSeatPwrCount = 0;

                //if (((comboBox2.SelectedItem.ToString() == "LHD") && (comboBox3.SelectedItem.ToString() == "LH")) ||
                //        ((comboBox2.SelectedItem.ToString() == "RHD") && (comboBox3.SelectedItem.ToString() == "RH")))
                //{
                //    IOPort.outportb(IO_OUT_FUNC.DRV_BATT, true);
                //    CanReWrite.LhRhSelect = LHRH.LH;
                //}
                //else
                //{
                //    IOPort.outportb(IO_OUT_FUNC.PASS_BATT, true);
                //    CanReWrite.LhRhSelect = LHRH.RH;
                //}

                if(/*(CheckItem.SWHeater == true) && */(CheckItem.SWVent == true))
                {
                    IOPort.outportbToFunction(IO_OUT_FUNC.PASS_BATT, true);
                    CanReWrite.LhRhSelect = LHRH.RH;
                }
                else
                {
                    if(comboBox3.SelectedItem.ToString() == "LH")
                        IOPort.outportbToFunction(IO_OUT_FUNC.DRV_BATT, true);
                    else IOPort.outportbToFunction(IO_OUT_FUNC.PASS_BATT, true);
                    if (comboBox3.SelectedItem.ToString() == "LH")
                        CanReWrite.LhRhSelect = LHRH.RH;
                    else CanReWrite.LhRhSelect = LHRH.LH;
                }

                if (TSpec.Offset.IGN == false)
                {
                    IOPort.outportbToFunction(IO_OUT_FUNC.IGN1_BATT, true);
                    IOPort.outportbToFunction(IO_OUT_FUNC.IGN2_BATT, true);
                }
                else
                {
                    IOPort.outportbToFunction(IO_OUT_FUNC.IGN1_GND, true);
                    IOPort.outportbToFunction(IO_OUT_FUNC.IGN2_GND, true);
                }
                //IOPort.outportbToFunction(IO_OUT_FUNC.DRV_LAMP_ACTIVE_LOW, TSpec.Offset.DrvLamp);
                //IOPort.outportbToFunction(IO_OUT_FUNC.PASS_LAMP_ACTIVE_LOW, TSpec.Offset.AssistLamp);

                //ComF.timedelay(200);
                //PwrCtrl.POWER_PWSetting(TSpec.Volt);
                //ComF.timedelay(200);
                //PwrCtrl.POWER_PWON();

                CanReWrite.CanLinDefaultSetting();
            }
            else
            {
                if (TSpec.Offset.IGN == false)
                {
                    IOPort.outportbToFunction(IO_OUT_FUNC.IGN1_BATT, false);
                    IOPort.outportbToFunction(IO_OUT_FUNC.IGN2_BATT, false);
                }
                else
                {
                    IOPort.outportbToFunction(IO_OUT_FUNC.IGN1_GND, false);
                    IOPort.outportbToFunction(IO_OUT_FUNC.IGN2_GND, false);
                }
                ComF.timedelay(200);
                IOPort.outportbToFunction(IO_OUT_FUNC.DRV_BATT, true);
                IOPort.outportbToFunction(IO_OUT_FUNC.PASS_BATT, true);

                //IOPort.outportb(IO_OUT.PSEAT_IGN, false);
                IOPort.outportbToFunction(IO_OUT_FUNC.PSEAT_BATT, false);

                //ComF.timedelay(200);
                //IOPort.outportb(IO_OUT_FUNC.PSEAT_BATT, false);
                //ComF.timedelay(200);

                IOPort.outportb(IO_OUT.RH_SELECT, false);
                //IOPort.outportbToFunction(IO_OUT_FUNC.DRV_LAMP_ACTIVE_LOW, TSpec.Offset.DrvLamp);
                //IOPort.outportbToFunction(IO_OUT_FUNC.PASS_LAMP_ACTIVE_LOW, TSpec.Offset.AssistLamp);
                ComF.timedelay(200);
                PwrCtrl.POWER_PWSetting(0);
                //ComF.timedelay(200);
                PwrCtrl.POWER_PWOFF();
                //if(CanCtrl.isOpen(0) == true) CanCtrl.CanClose(0);                
            }

            CanReWrite.PowerOnOff = OnOff;
            return;
        }

        public PowerControl GetPwrCtrl
        {
            get { return PwrCtrl; }
        }


        private bool SpecOutputFlag = false;
        private bool OffSpecOutputFlag = false;
        private bool SWOffCheckFlag = false;
        //private bool MeterModeChangeFlag = false;
        private long pLast;
        private long pFirst;
        //private short PSeatPwrCount = 0;

        //private short SendMessageFlag = 0;

        private void Processing()
        {
            TotalLast = ComF.timeGetTimems();
            if (((TotalLast - TotalFirst) / 1000) != sevenSegmentInteger4.Value.AsInteger) sevenSegmentInteger4.Value.AsInteger = (int)((TotalLast - TotalFirst) / 1000);
            switch (Step)
            {
                case 0:                    
                    SpecOutputFlag = true;
                    //plot1.Channels[0].Clear();
                    //PlotMax = -9999;
                    //PlotMin = 9999;
                    try
                    {
                        ComF.timedelay(200);
                        PwrCtrl.POWER_PWSetting(TSpec.Volt);
                        PwrCtrl.POWER_CURRENTSetting(20);
                        //ComF.timedelay(200);
                        PwrCtrl.POWER_PWON();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        Step++;
                    }
                    break;
                case 1:                    
                    //CanOpenFlag = true;
                    //MeterModeChangeFlag = true;
                    //Meter.ResitanceModeSet(1000);
                    //led7.Value.AsBoolean = true;
                    BatteryOnOff(true);

                    //IOPort.outportb(IO_OUT.PALLET_수평, true);
                    IOPort.RetractorOnOff = true;
                    ComF.timedelay(700);
                    //Meter.AutoModeSet(false);
                    //IOPort.outportb(IO_OUT.PALLET_수평, false);
                    //led7.Value.AsBoolean = false;
                    SpecOutputFlag = false;
                    //MeterModeChangeFlag = false;
                    Step++;
                    break;
                case 2:
                    if (CheckItem.Retractor == true)
                    {
                        CheckRetractor();
                    }
                    else
                    {
                        IOPort.RetractorOnOff = false;
                        ComF.timedelay(500);
                        Step++;
                    }
                    break;
                case 3: //Heater 3                    
                case 4: //Heater 2                    
                case 5: //Heater 1                    
                case 6: //Heater Off                         
                    if (CheckItem.SWHeater == false)
                    {
                        SpecOutputFlag = false;
                        Step = 7;
                        return;
                    }

                    if ((SpecOutputFlag == false) && (Step == 2)) label5.Text = "히터 검사 중입니다.";
                    //if (((comboBox2.SelectedItem.ToString() == "LHD") && (comboBox3.SelectedItem.ToString() == "LH")) ||
                    //   ((comboBox2.SelectedItem.ToString() == "RHD") && (comboBox3.SelectedItem.ToString() == "RH")))
                    //{
                    //    //ECU가 없음
                    //    NoneECUHeaerCheck(Step - 3);
                    //}
                    //else
                    //{
                    if ((CheckItem.Heater.Lamp == true) || (CheckItem.Heater.Curr == true))
                    {
                        HeaterCheck(Step - 3);
                    }
                    //}

                    break;
                case 7://통풍 3
                case 8://통풍 2
                case 9://통풍 1
                case 10://통풍 OFF
                    if ((CheckItem.SWVent == false) || ((CheckItem.Vent.Curr == false) && (CheckItem.Vent.Lamp == false)))
                    {
                        SpecOutputFlag = false;
                        Step = 11;
                        return;
                    }
                    if ((SpecOutputFlag == false) && (Step == 6)) label5.Text = "통풍 검사 중입니다.";
                    //if (((comboBox2.SelectedItem.ToString() == "LHD") && (comboBox3.SelectedItem.ToString() == "LH")) ||
                    //   ((comboBox2.SelectedItem.ToString() == "RHD") && (comboBox3.SelectedItem.ToString() == "RH")))
                    //{
                    //    //ECU가 없음
                    //    NoneECUVentCheck(Step - 7);
                    //}
                    //else
                    //{
                    if ((CheckItem.Vent.Lamp == true) || (CheckItem.Vent.Curr == true))
                    {
                        VentCheck(Step - 7);
                    }
                    //}
                    break;                
                case 11: //버클 워닝                    
                    if (CheckItem.SWBuckleWar == false)
                    {
                        Step++;
                        SpecOutputFlag = false;
                        return;
                    }
                    if (SpecOutputFlag == false) label5.Text = "버클워닝 검사 중입니다.";
                    CheckBuckleWarning();
                    break;                                
                case 12://Power Seat - Recline
                case 13://Power Seat - Legrest
                case 14://Power Seat - Legrest Ext
                case 15://Power Seat - Relax
                    if (CheckItem.PowerSW == false)
                    {
                        Step = 16;
                        SpecOutputFlag = false;
                        return;
                    }
                    CheckPowerSeat(Step - 12);
                    break;
                default:
                    if (TestEnd[0] == false) TestEnd[0] = true;

                    if ((TestEnd[0] == true) && (TestEnd[1] == true))
                    {
                        SaveDataFlag = true;
                        CheckResult();
                        //BatteryOnOff(false);
                        SpecOutputFlag = false;
                        Step = 0;
                    }
                    break;
            }
            return;
        }

        private bool[] TestEnd = { false, false };

        private short SbrStep { get; set; }
        private bool SbrSpecOutputFlag { get; set; }

        private long SbrTestTimeToFirst { get; set; }
        private long SbrTestTimeToLast { get; set; }
        private bool SbrTestFlag { get; set; }

        private bool SbrTestEndToJigMoveFlag { get; set; } 
        private short JigOffStep { get; set; }

        private long OKFirst = 0;

        private void Processing2()
        {
            SbrTestTimeToLast = ComF.timeGetTimems();
            if (CheckItem.SBR == false)
            {
                if (TestEnd[1] == false) TestEnd[1] = true;
                return;
            }
            if (TestEnd[1] == true) return;
                       

            switch(SbrStep)
            {
                case 0:  
                    if(SbrSpecOutputFlag == false)
                    {
                        if (comboBox3.SelectedItem.ToString() == "RH")
                        {
                            if (IOPort.GetRightSensor == false)
                            {
                                if (CheckItem.Retractor == false) IOPort.RetractorOnOff = false;
                                IOPort.JigLeftRightMove = true;
                                SbrTestTimeToLast = ComF.timeGetTimems();
                                SbrTestTimeToFirst = ComF.timeGetTimems();
                                SbrSpecOutputFlag = true;
                            }
                            else
                            {
                                SbrStep++;
                                SbrSpecOutputFlag = false;
                            }
                        }
                        else
                        {
                            if (IOPort.GetLeftSensor == false)
                            {
                                IOPort.JigLeftRightMove = false;
                                SbrTestTimeToLast = ComF.timeGetTimems();
                                SbrTestTimeToFirst = ComF.timeGetTimems();
                                SbrSpecOutputFlag = true;
                            }
                            else
                            {
                                SbrStep++;
                                SbrSpecOutputFlag = false;
                            }
                        }
                    }
                    else
                    {
                        if (IOPort.GetRightSensor == true)
                        {
                            SbrStep++;
                            SbrSpecOutputFlag = false;
                        }
                        else
                        {
                            //10초
                            if(10000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                            {
                                TestEnd[1] = true;
                                MessageBox.Show(this, "SBR 지그 동작에 문제가 발생하였습니다.\n 점검을 진행해 주십시오", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }
                        }
                    }
                    break;
                case 1:
                    if (SbrSpecOutputFlag == false)
                    {
                        if (IOPort.GetFrontSensor == false)
                        {
                            IOPort.JigFwdMove = true;
                            SbrTestTimeToLast = ComF.timeGetTimems();
                            SbrTestTimeToFirst = ComF.timeGetTimems();
                            SbrSpecOutputFlag = true;
                        }
                        else
                        {
                            SbrStep++;
                            SbrSpecOutputFlag = false;
                        }
                    }
                    else
                    {
                        if (IOPort.GetFrontSensor == true)
                        {
                            SbrStep++;
                            SbrSpecOutputFlag = false;
                        }
                        else
                        {
                            //10초
                            if (10000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                            {
                                //IOPort.JigBwdMove = false;
                                SbrTestEndToJigMoveFlag = true;
                                JigOffStep = 0;
                                TestEnd[1] = true;
                                MessageBox.Show(this, "SBR 지그 동작에 문제가 발생하였습니다.\n 점검을 진행해 주십시오", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }                            
                        }
                    }
                    break;
                case 2:
                    //무부하시
                    if (2 < Step)
                    {
                        if (SbrSpecOutputFlag == false)
                        {
                            SbrSpecOutputFlag = true;
                            IOPort.RetractorOnOff = false;

                            SbrTestTimeToLast = ComF.timeGetTimems();
                            SbrTestTimeToFirst = ComF.timeGetTimems();
                        }
                        else
                        {
                            //int Time1;
                            //int Time2;

                            //if (CheckItem.Retractor == false)
                            //{
                            //    Time1 = 100;
                            //    Time2 = 2000;
                            //}
                            //else
                            //{
                            //    Time1 = 6000;
                            //    Time2 = 8000;
                            //}
                            //if (Time1 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                            //{
                            //    if (TData.SBR.Test == false) TData.SBR.Test = true;
                            //    float Value = (float)MultiMeter.GetReadData;
                            //    if (0.5 < Value) Value = Value + TSpec.Offset.Retractor;

                            //    string s;
                            //    if (Value < 9999)
                            //        s = string.Format("{0:0.00}", Value);
                            //    else s = "OL";

                            //    plot2.Channels[0].AddXY(plot2.Channels[0].Count, Value);
                            //    plot2.XAxes[0].Tracking.ZoomToFitAll();
                            //    plot2.YAxes[0].Tracking.ZoomToFitAll();

                            //    if (fpSpread1.ActiveSheet.Cells[12, 6].Text != s) fpSpread1.ActiveSheet.Cells[12, 6].Text = s;

                            //    //2초간 데이타를 읽는다.
                            //    if (Time2 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                            //    {
                            //        if (s != "OL")
                            //        {
                            //            if (float.TryParse(s, out TData.SBR.NotLoadData) == false) TData.SBR.NotLoadData = 0;
                            //        }
                            //        else
                            //        {
                            //            TData.SBR.NotLoadData = 99999;
                            //        }
                            //        if (TSpec.SBR.NotLoad < TData.SBR.NotLoadData)
                            //        {
                            //            TData.SBR.ResultNotLoad = RESULT.PASS;
                            //        }
                            //        else
                            //        {
                            //            TData.SBR.ResultNotLoad = RESULT.REJECT;
                            //            TData.SBR.Result = RESULT.REJECT;
                            //        }

                            //        fpSpread1.ActiveSheet.Cells[12, 7].Text = (TData.SBR.ResultNotLoad == RESULT.PASS) ? "OK" : "NG";
                            //        fpSpread1.ActiveSheet.Cells[12, 7].ForeColor = (TData.SBR.ResultNotLoad == RESULT.PASS) ? Color.Lime : Color.Red;

                            //        SbrSpecOutputFlag = false;
                            //        SbrStep++;
                            //    }
                            //}

                            if (1000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                            {
                                TData.SBR.Test = true;
                                TData.SBR.NotLoadData = 99999;
                                TData.SBR.ResultNotLoad = RESULT.PASS;
                                SbrSpecOutputFlag = false;
                                SbrStep++;
                            }
                        }
                    }
                    break;
                case 3:
                    //15K
                    if (SbrSpecOutputFlag == false)
                    {
                        SbrSpecOutputFlag = true;
                        IOPort.Jig1StDown = true;

                        SbrTestTimeToLast = ComF.timeGetTimems();
                        SbrTestTimeToFirst = ComF.timeGetTimems();
                        SbrTestFlag = false;
                    }
                    else
                    {
                        if (IOPort.Get1StDownSensor == true)
                        {
                            if (SbrTestFlag == false)
                            {
                                SbrTestTimeToLast = ComF.timeGetTimems();
                                SbrTestTimeToFirst = ComF.timeGetTimems();
                                SbrTestFlag = true;
                            }
                            else 
                            { 
                                float Value = (float)MultiMeter.GetData;
                                string s;

                                if (Value < 9999)
                                {
                                    s = string.Format("{0:0.00}", Value);
                                }
                                else
                                {
                                    TData.SBR.Load15KData = 999;
                                    s = "999";
                                }
                                if (TData.SBR.Test == false) TData.SBR.Test = true;
                                plot2.Channels[0].AddXY(plot2.Channels[0].Count, (Value < 9999) ? Value : 0);
                                plot2.XAxes[0].Tracking.ZoomToFitAll();
                                plot2.YAxes[0].Tracking.ZoomToFitAll();
                                
                                if (2000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                                {
                                    if (fpSpread1.ActiveSheet.Cells[13, 6].Text != s) fpSpread1.ActiveSheet.Cells[13, 6].Text = s;

                                    if (s != "OL")
                                    {
                                        if (float.TryParse(s, out TData.SBR.Load15KData) == false) TData.SBR.Load15KData = 0;
                                        if (TSpec.SBR.Load15Kg <= TData.SBR.Load15KData)
                                        {
                                            TData.SBR.Result15Kg = RESULT.PASS;
                                        }
                                        else
                                        {
                                            OKFirst = ComF.timeGetTimems();
                                            TData.SBR.Result15Kg = RESULT.REJECT;
                                            TData.SBR.Result = RESULT.REJECT;
                                        }
                                    }
                                    else
                                    {
                                        OKFirst = ComF.timeGetTimems();
                                        TData.SBR.Result15Kg = RESULT.REJECT;
                                        TData.SBR.Result = RESULT.REJECT;

                                        TData.SBR.Load15KData = 99999;
                                    }

                                    fpSpread1.ActiveSheet.Cells[13, 7].Text = (TData.SBR.Result15Kg == RESULT.PASS) ? "OK" : "NG";
                                    fpSpread1.ActiveSheet.Cells[13, 7].ForeColor = (TData.SBR.Result15Kg == RESULT.PASS) ? Color.Lime : Color.Red;

                                    SbrSpecOutputFlag = false;
                                    SbrTestFlag = false; 
                                    SbrStep++;                                    
                                }
                            }
                        }
                        else
                        {
                            //20초
                            if (20000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                            {
                                //IOPort.Jig1StDown = false;
                                SbrTestEndToJigMoveFlag = true;
                                JigOffStep = 0;
                                TestEnd[1] = true;
                                MessageBox.Show(this, "SBR 지그 동작에 문제가 발생하였습니다.\n 점검을 진행해 주십시오", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }            
                    break;
                case 4:
                    //30K
                    if (SbrSpecOutputFlag == false)
                    {
                        SbrSpecOutputFlag = true;
                        IOPort.Jig2StDown = true;

                        SbrTestTimeToLast = ComF.timeGetTimems();
                        SbrTestTimeToFirst = ComF.timeGetTimems();
                        SbrTestFlag = false;
                    }
                    else
                    {
                        bool Flag = false;

                        if (2000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                            Flag = true;
                        else if ((IOPort.Get2StDown1Sensor == true) && (IOPort.Get2StDown2Sensor == true))
                            Flag = true;

                        if(Flag == true)
                        {
                            if (SbrTestFlag == false)
                            {
                                SbrTestTimeToLast = ComF.timeGetTimems();
                                SbrTestTimeToFirst = ComF.timeGetTimems();
                                SbrTestFlag = true;
                            }
                            else
                            {
                                float Value = (float)MultiMeter.GetData;
                                string s;
                                if (Value < 9999)
                                    s = string.Format("{0:0.00}", Value);
                                else s = "999";

                                plot2.Channels[0].AddXY(plot2.Channels[0].Count, (Value < 9999) ? Value : 0);
                                plot2.XAxes[0].Tracking.ZoomToFitAll();
                                plot2.YAxes[0].Tracking.ZoomToFitAll();

                                
                                //3초간 데이타를 읽는다.
                                if (3000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                                {
                                    if (s != "999")
                                    {
                                        if (float.TryParse(s, out TData.SBR.Load30KData) == false) TData.SBR.Load30KData = 0;
                                        if (TData.SBR.Load30KData < TSpec.SBR.Load30Kg)
                                        {
                                            if (fpSpread1.ActiveSheet.Cells[14, 6].Text != s) fpSpread1.ActiveSheet.Cells[14, 6].Text = s;

                                            TData.SBR.Result30Kg = RESULT.PASS;
                                            fpSpread1.ActiveSheet.Cells[14, 7].Text = (TData.SBR.Result30Kg == RESULT.PASS) ? "OK" : "NG";
                                            fpSpread1.ActiveSheet.Cells[14, 7].ForeColor = (TData.SBR.Result30Kg == RESULT.PASS) ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            if (fpSpread1.ActiveSheet.Cells[14, 6].Text != s) fpSpread1.ActiveSheet.Cells[14, 6].Text = s;

                                            TData.SBR.Result30Kg = RESULT.REJECT;
                                            TData.SBR.Result = RESULT.REJECT;
                                            fpSpread1.ActiveSheet.Cells[14, 7].Text = (TData.SBR.Result30Kg == RESULT.PASS) ? "OK" : "NG";
                                            fpSpread1.ActiveSheet.Cells[14, 7].ForeColor = (TData.SBR.Result30Kg == RESULT.PASS) ? Color.Lime : Color.Red;
                                        }
                                    }
                                    else
                                    {
                                        if (fpSpread1.ActiveSheet.Cells[14, 6].Text != s) fpSpread1.ActiveSheet.Cells[14, 6].Text = s;

                                        TData.SBR.Result30Kg = RESULT.REJECT;
                                        TData.SBR.Result = RESULT.REJECT;
                                        fpSpread1.ActiveSheet.Cells[14, 7].Text = (TData.SBR.Result30Kg == RESULT.PASS) ? "OK" : "NG";
                                        fpSpread1.ActiveSheet.Cells[14, 7].ForeColor = (TData.SBR.Result30Kg == RESULT.PASS) ? Color.Lime : Color.Red;
                                        TData.SBR.Load30KData = 99999;
                                    }

                                    
                                    SbrSpecOutputFlag = false;
                                    SbrTestFlag = false;
                                    SbrStep++;
                                }                                
                            }
                        }
                        else
                        {
                            //20초
                            if (20000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                            {                                
                                SbrTestEndToJigMoveFlag = true;
                                JigOffStep = 0;
                                TestEnd[1] = true;
                                MessageBox.Show(this, "SBR 지그 동작에 문제가 발생하였습니다.\n 점검을 진행해 주십시오", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    break;
                case 5:
                    if ((TData.SBR.ResultNotLoad == RESULT.PASS) && (TData.SBR.Result15Kg == RESULT.PASS) && (TData.SBR.Result30Kg == RESULT.PASS))
                        TData.SBR.Result = RESULT.PASS;
                    else TData.SBR.Result = RESULT.REJECT;
                    SbrStep++;
                    break;
                default:
                    SbrTestEndToJigMoveFlag = true;
                    JigOffStep = 0;
                    TestEnd[1] = true;
                    break;
            }
            return;
        }

        private void JigOff()
        {
            SbrTestTimeToLast = ComF.timeGetTimems();
            switch (JigOffStep)
            {
                case 0:
                    IOPort.Jig1StDown = false;
                    IOPort.Jig2StDown = false;
                    SbrTestTimeToLast = ComF.timeGetTimems();
                    SbrTestTimeToFirst = ComF.timeGetTimems();
                    JigOffStep++;
                    break;
                case 1:
                    //20초
                    if (20000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                    {
                        SbrTestEndToJigMoveFlag = false;
                        JigOffStep = 0;                        
                        MessageBox.Show(this, "SBR 지그 동작에 문제가 발생하였습니다.\n 점검을 진행해 주십시오", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        //if ((IOPort.Get2StUp1Sensor == true) && (IOPort.Get2StUp2Sensor == true) && (IOPort.Get1StUpSensor == true))
                        //{
                        //    JigOffStep++;
                        //}
                        //else if (2000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                        //{
                        //    JigOffStep++;
                        //}
                        if (IOPort.Get1StUpSensor == true)
                        {
                            JigOffStep++;
                        }
                    }
                    break;
                case 2:
                    IOPort.JigFwdMove = false;
                    SbrTestTimeToLast = ComF.timeGetTimems();
                    SbrTestTimeToFirst = ComF.timeGetTimems();
                    JigOffStep++;
                    break;
                case 3:
                    if (20000 <= (SbrTestTimeToLast - SbrTestTimeToFirst))
                    {
                        SbrTestEndToJigMoveFlag = false;
                        JigOffStep = 0;
                        MessageBox.Show(this, "SBR 지그 동작에 문제가 발생하였습니다.\n 점검을 진행해 주십시오", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if ((IOPort.Get2StUp1Sensor == true) && (IOPort.Get2StUp2Sensor == true) && (IOPort.Get1StUpSensor == true) && (IOPort.GetRearSensor == true))
                        {
                            JigOffStep++;
                        }
                    }
                    break;
                case 4:
                    SbrTestEndToJigMoveFlag = false;
                    JigOffStep = 0;
                    break;
            }
        }

        private void HeaterDrvSW()
        {
            //if (comboBox3.SelectedItem.ToString() != "LH")
            if ((comboBox3.SelectedItem.ToString() == "RH") && (CheckItem.Lin == true))
            {
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSWRaw, (byte)LH_HeaterSWRaw.Data.On);
                ComF.timedelay(100);
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSW, (byte)LH_HeaterSW.Data.Short_Term_Push);
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSWRaw, (byte)LH_HeaterSWRaw.Data.Off);
                ComF.timedelay(300);
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSW, (byte)LH_HeaterSW.Data.No_Action);
            }
            else  if ((CheckItem.Heater.Curr == false) || (CheckItem.Vent.Curr == false))
            {
                if (TSpec.Offset.Button == false)
                    IOPort.outportbToFunction(IO_OUT_FUNC.DRV_HEATER_BATT, true);
                else IOPort.outportbToFunction(IO_OUT_FUNC.DRV_HEATER_GND, true);
                ComF.timedelay(500);
                if (TSpec.Offset.Button == false)
                    IOPort.outportbToFunction(IO_OUT_FUNC.DRV_HEATER_BATT, false);
                else IOPort.outportbToFunction(IO_OUT_FUNC.DRV_HEATER_GND, false);
            }            
            else
            {
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSWRaw, (byte)LH_HeaterSWRaw.Data.On);
                ComF.timedelay(100);
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSW, (byte)LH_HeaterSW.Data.Short_Term_Push);
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSWRaw, (byte)LH_HeaterSWRaw.Data.Off);
                ComF.timedelay(300);
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_HeaterSW, (byte)LH_HeaterSW.Data.No_Action);
            }
            return;
        }

        private void HeaterPassSW()
        {
            //if (comboBox3.SelectedItem.ToString() != "LH")
            if ((CheckItem.Heater.Curr == false) || (CheckItem.Vent.Curr == false))
            {                
                if (TSpec.Offset.Button == false)
                    IOPort.outportbToFunction(IO_OUT_FUNC.PASS_HEATER_BATT, true);
                else IOPort.outportbToFunction(IO_OUT_FUNC.PASS_HEATER_GND, true);
                ComF.timedelay(500);
                if (TSpec.Offset.Button == false)
                    IOPort.outportbToFunction(IO_OUT_FUNC.PASS_HEATER_BATT, false);
                else IOPort.outportbToFunction(IO_OUT_FUNC.PASS_HEATER_GND, false);
            }
            else
            {

                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.RH_HeaterSWRaw, (byte)RH_HeaterSWRaw.Data.On);
                ComF.timedelay(100);
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.RH_HeaterSW, (byte)RH_HeaterSW.Data.Short_Term_Push);
                ComF.timedelay(300);
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.RH_HeaterSWRaw, (byte)RH_HeaterSWRaw.Data.Off);
                CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.RH_HeaterSW, (byte)RH_HeaterSW.Data.No_Action);
            }
            return;
        }

        private void VentDrvSW()
        {
            CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_VentSWRaw, (byte)LH_VentSWRaw.Data.On);
            ComF.timedelay(100);
            CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_VentSW, (byte)LH_VentSW.Data.Short_Term_Push);
            ComF.timedelay(300);
            CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_VentSWRaw, (byte)LH_VentSWRaw.Data.Off);
            CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.LH_VentSW, (byte)LH_VentSW.Data.No_Action);
            return;
        }

        private void VentPassSW()
        {
            CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.RH_VentSWRaw, (byte)RH_VentSWRaw.Data.On);
            ComF.timedelay(100);
            CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.RH_VentSW, (byte)RH_VentSW.Data.Short_Term_Push);
            ComF.timedelay(300);
            CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.RH_VentSWRaw, (byte)RH_VentSWRaw.Data.Off);
            CanReWrite.LINDataOutput(CanReWrite.LhRhSelect, OUT_LIN_LIST.RH_VentSW, (byte)RH_VentSW.Data.No_Action);
            return;
        }
        private void HeaterCheck(int xStep)
        {
            bool Flag1 = false;
            bool Flag2 = false;


            if (SpecOutputFlag == false)
            {
                if (xStep == 0)
                {
                    //plot1.Channels[0].Clear();
                    //PlotMax = -9999;
                    //PlotMin = 9999;
                    GetCol = HeaterBackColorRead;
                    GetCurrCheck = HeaterCurrentStep;

                }
                //else if(xStep == 1)
                //{
                //    if (CheckItem.LampTo2Wire == true)
                //    {
                //        Step++;
                //        SpecOutputFlag = false;
                //        return;
                //    }
                //}
                
                TestOKCount = 0;
                               
                //if (((comboBox2.SelectedItem.ToString() == "LHD") && (comboBox3.SelectedItem.ToString() == "LH")) ||
                //    ((comboBox2.SelectedItem.ToString() == "RHD") && (comboBox3.SelectedItem.ToString() == "RH")))
                //{
                //    HeaterDrvSW();
                //}
                //else
                //{
                //    HeaterPassSW();
                //}

                if((CheckItem.Heater.Curr == true) && (CheckItem.Vent.Curr == true))
                {
                    HeaterDrvSW();
                }
                else
                {
                    if(comboBox3.SelectedItem.ToString() == "LH")
                        HeaterDrvSW();
                    else if ((comboBox3.SelectedItem.ToString() == "RH") && (CheckItem.Lin == true))
                        HeaterDrvSW();
                    else HeaterPassSW();
                }

                pLast = ComF.timeGetTimems();
                pFirst = ComF.timeGetTimems();
                SpecOutputFlag = true;
            }
            else
            {
                TData.HeaterLamp.Test = true;
                
                pLast = ComF.timeGetTimems();
                bool xFlag = false;

                switch (xStep)
                {
                    case 0:
                        //if (CheckItem.LampTo2Wire == false)
                        //{
                        if ((GetCol(3, 5) == Color.Lime) && (GetCol(3, 4) == Color.Lime) && (GetCol(3, 3) == Color.Lime)) xFlag = true;
                        if (xFlag == true)
                        {
                            TData.HeaterLamp.Result[2] = RESULT.PASS;
                            if (fpSpread1.ActiveSheet.Cells[3, 6].Text != "3단") fpSpread1.ActiveSheet.Cells[3, 6].Text = "3단";
                            Flag1 = true;
                        }
                        //}
                        //else
                        //{
                        //    if ((GetCol(3, 5) == Color.Lime) && (GetCol(3, 4) == Color.White) && (GetCol(3, 3) == Color.Lime)) xFlag = true;

                        //    if (xFlag == true)
                        //    {
                        //        TData.HeaterLamp.Result[1] = RESULT.PASS;
                        //        if (fpSpread1.ActiveSheet.Cells[3, 6].Text != "3단") fpSpread1.ActiveSheet.Cells[3, 6].Text = "3단";
                        //        Flag1 = true;
                        //    }
                        //}
                        break;
                    case 1:
                        //if (CheckItem.LampTo2Wire == false)
                        //{
                        if ((GetCol(3, 5) == Color.White) && (GetCol(3, 4) == Color.Lime) && (GetCol(3, 3) == Color.Lime)) xFlag = true;

                        if (xFlag == true)
                        {
                            TData.HeaterLamp.Result[1] = RESULT.PASS;
                            if (fpSpread1.ActiveSheet.Cells[3, 6].Text != "2단") fpSpread1.ActiveSheet.Cells[3, 6].Text = "2단";
                            Flag1 = true;
                        }
                        //}
                        //else
                        //{
                        //    if ((GetCol(3, 5) == Color.Lime) && (GetCol(3, 4) == Color.White) && (GetCol(3, 3) == Color.White)) xFlag = true;

                        //    if (xFlag == true)
                        //    {
                        //        TData.HeaterLamp.Result[1] = RESULT.PASS;
                        //        if (fpSpread1.ActiveSheet.Cells[3, 6].Text != "2단") fpSpread1.ActiveSheet.Cells[3, 6].Text = "2단";
                        //        Flag1 = true;
                        //    }
                        //}
                        break;
                    case 2:
                        if ((GetCol(3, 5) == Color.White) && (GetCol(3, 4) == Color.White) && (GetCol(3, 3) == Color.Lime)) xFlag = true;
                        {
                            if (xFlag == true)
                            {
                                TData.HeaterLamp.Result[0] = RESULT.PASS;
                                if (fpSpread1.ActiveSheet.Cells[3, 6].Text != "1단") fpSpread1.ActiveSheet.Cells[3, 6].Text = "1단";
                                Flag1 = true;
                            }
                        }
                        break;
                    case 3:
                        if ((GetCol(3, 5) == Color.White) && (GetCol(3, 4) == Color.White) && (GetCol(3, 3) == Color.White)) xFlag = true;
                        if (xFlag == true)
                        {
                            if (fpSpread1.ActiveSheet.Cells[3, 6].Text != "OFF") fpSpread1.ActiveSheet.Cells[3, 6].Text = "OFF";
                            Flag1 = true;
                        }
                        break;
                }

                if (xStep < 3)
                {
                    if (GetCurrCheck(xStep) == true)
                    {
                        float Value;

                        //if (comboBox3.SelectedItem.ToString() == "RH")
                        //    Value = (float)ADData[ADPos.HEATER] + TSpec.Offset.HeaterRH;
                        //else Value = (float)ADData[ADPos.HEATER] + TSpec.Offset.HeaterLH;

                        if (comboBox3.SelectedItem.ToString() == "RH")
                            Value = pMeter.GetCurr + TSpec.Offset.HeaterRH;
                        else Value = pMeter.GetCurr + TSpec.Offset.HeaterLH;

                        //plot1.Channels[0].AddXY(plot1.Channels[0].Count, Value);
                        //plot1.XAxes[0].Tracking.ZoomToFitAll();

                        //bool PlotFlag = false;
                        //if (Value < PlotMin)
                        //{
                        //    PlotMin = Value;
                        //    PlotFlag = true;
                        //}
                        //if (PlotMax < Value)
                        //{
                        //    PlotMax = Value;
                        //    PlotFlag = true;
                        //}
                        //if (PlotFlag == true)
                        //{
                        //    plot1.YAxes[0].Min = PlotMin - 5;
                        //    plot1.YAxes[0].Span = (PlotMax + 5) - (PlotMin - 5);
                        //}

                        TData.HeaterCurrent.Test = true;
                        TData.HeaterCurrent.Data = Value;

                        if ((CheckItem.Heater.Curr == true) && (CheckItem.Vent.Curr == false))
                        {
                            if (fpSpread1.ActiveSheet.Cells[2, 7].Text == "")
                            {
                                __MinMax__ MinMax = new __MinMax__();

                                //if (((comboBox2.SelectedItem.ToString() == "LHD") && (comboBox3.SelectedItem.ToString() == "LH")) ||
                                //    ((comboBox2.SelectedItem.ToString() == "RHD") && (comboBox3.SelectedItem.ToString() == "RH")))
                                //{
                                //    if ((CheckItem.Heater.Curr == true) && (CheckItem.Vent.Curr == false))
                                //        MinMax = TSpec.LHHeater;
                                //    else MinMax = TSpec.VentHeater;
                                //}
                                //else
                                //{
                                //    if ((CheckItem.Heater.Curr == true) && (CheckItem.Vent.Curr == false))
                                //        MinMax = TSpec.Heater;
                                //    else MinMax = TSpec.VentHeater;
                                //}

                                if ((CheckItem.Heater.Curr == true) && (CheckItem.Vent.Curr == false))
                                    MinMax = TSpec.Heater;
                                else MinMax = TSpec.VentHeater;

                                if ((MinMax.Min <= TData.HeaterCurrent.Data) && (TData.HeaterCurrent.Data <= MinMax.Max))
                                {
                                    TestOKCount++;
                                    if (10 <= TestOKCount)
                                    {
                                        fpSpread1.ActiveSheet.Cells[2, 6].Text = string.Format("{0:0.00}", TData.HeaterCurrent.Data);
                                        fpSpread1.ActiveSheet.Cells[2, 7].Text = "OK";
                                        fpSpread1.ActiveSheet.Cells[2, 7].BackColor = Color.Black;
                                        fpSpread1.ActiveSheet.Cells[2, 7].ForeColor = Color.Lime;
                                        Flag2 = true;
                                        TData.HeaterCurrent.Result = RESULT.PASS;
                                    }
                                }
                                else
                                {
                                    fpSpread1.ActiveSheet.Cells[2, 6].Text = string.Format("{0:0.00}", TData.HeaterCurrent.Data);
                                    TestOKCount = 0;
                                }
                            }
                            else
                            {
                                Flag2 = true;
                            }
                        }
                        else
                        {
                            if (fpSpread1.ActiveSheet.Cells[2, 7].Text == "")
                            {
                                __MinMax__ MinMax = new __MinMax__();

                                //if (((comboBox2.SelectedItem.ToString() == "LHD") && (comboBox3.SelectedItem.ToString() == "LH")) ||
                                //    ((comboBox2.SelectedItem.ToString() == "RHD") && (comboBox3.SelectedItem.ToString() == "RH")))
                                //{
                                //    MinMax = TSpec.LHHeater;
                                //}
                                //else
                                //{
                                //    if ((CheckItem.Heater.Curr == true) && (CheckItem.Vent.Curr == false))
                                //        MinMax = TSpec.Heater;
                                //    else MinMax = TSpec.VentHeater;
                                //}

                                if ((CheckItem.Heater.Curr == true) && (CheckItem.Vent.Curr == false))
                                    MinMax = TSpec.Heater;
                                else MinMax = TSpec.VentHeater;

                                if ((MinMax.Min <= TData.HeaterCurrent.Data) && (TData.HeaterCurrent.Data <= MinMax.Max))
                                {
                                    TestOKCount++;
                                    if (10 <= TestOKCount)
                                    {
                                        fpSpread1.ActiveSheet.Cells[2, 6].Text = string.Format("{0:0.00}", TData.HeaterCurrent.Data);
                                        fpSpread1.ActiveSheet.Cells[2, 7].Text = "OK";
                                        fpSpread1.ActiveSheet.Cells[2, 7].BackColor = Color.Black;
                                        fpSpread1.ActiveSheet.Cells[2, 7].ForeColor = Color.Lime;
                                        Flag2 = true;
                                        TData.HeaterCurrent.Result = RESULT.PASS;
                                    }
                                }
                                else
                                {
                                    fpSpread1.ActiveSheet.Cells[2, 6].Text = string.Format("{0:0.00}", TData.HeaterCurrent.Data);
                                    TestOKCount = 0;
                                }
                            }
                            else
                            {
                                Flag2 = true;
                            }
                        }
                    }
                    else
                    {
                        //검사 위치가 아니면                        
                        Flag2 = true;
                    }
                }
                else
                {
                    //오프 위치이면
                    Flag2 = true;
                }

                if ((Flag1 == false) || (Flag2 == false))
                {
                    //판정이 안된 항목이 있을 경우
                    if ((TSpec.HeaterCheckPos.Time * 1000) <= (pLast - pFirst))
                    {
                        //검사 시간 초과

                        if (Flag1 == false) TData.HeaterLamp.Result[2] = RESULT.REJECT;
                        if (Flag2 == false) TData.HeaterCurrent.Result = RESULT.REJECT;

                        if (Flag2 == false)
                        {
                            fpSpread1.ActiveSheet.Cells[2, 7].Text = "NG";
                            fpSpread1.ActiveSheet.Cells[2, 7].BackColor = Color.Black;
                            fpSpread1.ActiveSheet.Cells[2, 7].ForeColor = Color.Red;
                        }

                        if (Flag1 == false)
                        {
                            fpSpread1.ActiveSheet.Cells[3, 7].Text = "NG";
                            fpSpread1.ActiveSheet.Cells[3, 7].BackColor = Color.Black;
                            fpSpread1.ActiveSheet.Cells[3, 7].ForeColor = Color.Red;
                        }

                        Step++;
                        SpecOutputFlag = false;
                    }
                }
                else
                {
                    //검사가 완료 된 경우
                    Step++;
                    SpecOutputFlag = false;
                    if (GetCurrCheck(xStep) == false) ComF.timedelay(200);

                    if (xStep == 3)
                    {
                        if (fpSpread1.ActiveSheet.Cells[3, 7].Text != "NG")
                        {
                            fpSpread1.ActiveSheet.Cells[3, 7].Text = "OK";
                            fpSpread1.ActiveSheet.Cells[3, 7].BackColor = Color.Black;
                            fpSpread1.ActiveSheet.Cells[3, 7].ForeColor = Color.Lime;
                        }
                    }
                }
            }

            return;
        }

        //private void NoneECUHeaerCheck(int xStep)
        //{
        //    if (SpecOutputFlag == false)
        //    {
        //        TestOKCount = 0;
        //        if (xStep == 0)
        //        {
        //            plot1.Channels[0].Clear();
        //            PlotMax = -9999;
        //            PlotMin = 9999;

        //            if (CheckItem.Heater.Curr == false)
        //            {
        //                Step++;
        //                SpecOutputFlag = false;
        //                //IOPort.outportb(IO_OUT.LH_HEATER_CURRENT, false);
        //                return;
        //            }
        //            //IOPort.outportb(IO_OUT.LH_HEATER_CURRENT, true);
        //            //if (PanelMeterReadPos != (short)PANELMETER_MESSAGE.HEATER) PanelMeterReadPos = (short)PANELMETER_MESSAGE.HEATER;
        //        }
        //        else if (xStep == 1)
        //        {
        //            //if (CheckItem.Heater.NTC == false)
        //            //{
        //            Step = 6; //통풍(Vent)
        //            SpecOutputFlag = false;
        //            return;
        //            //}

        //            //outportb(IO_OUT.LH_NTC_RESI, true);

        //            //PanelMeterReadPos = (short)PANELMETER_MESSAGE.NORMAL;
        //            //Meter.ResitanceModeSet(2000); //2k ohom
        //            //ComF.timedelay(100);
        //            //Meter.AutoModeSet(false);
        //            //MultiMesterRangeSetFlag = true;
        //        }
        //        SpecOutputFlag = true;
        //        pFirst = ComF.timeGetTimems();
        //        pLast = ComF.timeGetTimems();
        //    }
        //    else
        //    {
        //        pLast = ComF.timeGetTimems();

        //        float Value;

        //        if (xStep == 0)
        //        {
        //            if (comboBox3.SelectedItem.ToString() == "RH")
        //                Value = pMeter.GetCurr + TSpec.Offset.HeaterRH;
        //            else Value = pMeter.GetCurr + TSpec.Offset.HeaterLH;
        //        }
        //        else Value = (float)MultiMeter.GetReadData;

        //        if (500 <= (pLast - pFirst))
        //        {

        //            plot1.Channels[0].AddXY(plot1.Channels[0].Count, Value);
        //            plot1.XAxes[0].Tracking.ZoomToFitAll();

        //            bool PlotFlag = false;
        //            if (Value < PlotMin)
        //            {
        //                PlotMin = Value;
        //                PlotFlag = true;
        //            }
        //            if (PlotMax < Value)
        //            {
        //                PlotMax = Value;
        //                PlotFlag = true;
        //            }
        //            if (PlotFlag == true)
        //            {
        //                plot1.YAxes[0].Min = PlotMin - 5;
        //                plot1.YAxes[0].Span = (PlotMax + 5) - (PlotMin - 5);
        //            }

        //            if ((TSpec.HeaterCheckPos.Time * 1000) <= (pLast - pFirst))
        //            {
        //                switch (xStep)
        //                {
        //                    case 0: //Current
        //                        fpSpread1.ActiveSheet.Cells[xStep + 2, 6].Text = string.Format("{0:0.00}", Value);
        //                        TData.LHHeater.Test = true;
        //                        TData.LHHeater.Result = RESULT.REJECT;
        //                        TData.LHHeater.Data = pMeter.GetCurr;
        //                        //IOPort.outportb(IO_OUT.LH_HEATER_CURRENT, false);
        //                        SpecOutputFlag = false;
        //                        Step++;
        //                        break;
        //                    case 1://NTC
        //                        //fpSpread1.ActiveSheet.Cells[xStep + 3, 6].Text = string.Format("{0:0.00}", Value);
        //                        //TData.HeaterNTC.Test = true;
        //                        //TData.HeaterNTC.Result = RESULT.REJECT;
        //                        //TData.HeaterNTC.Data = (float)ADData[ADPos.MULTI_METER];
        //                        Step = 6; //통풍(Vent)
        //                        SpecOutputFlag = false;
        //                        //IOPort.outportb(IO_OUT.LH_NTC_RESI, false);
        //                        break;
        //                    default:
        //                        Step++;
        //                        break;
        //                }

        //                if (xStep == 0)
        //                {
        //                    fpSpread1.ActiveSheet.Cells[xStep + 2, 7].Text = "NG";
        //                    fpSpread1.ActiveSheet.Cells[xStep + 2, 7].ForeColor = Color.White;
        //                    fpSpread1.ActiveSheet.Cells[xStep + 2, 7].BackColor = Color.Red;
        //                }
        //                else
        //                {
        //                    //fpSpread1.ActiveSheet.Cells[xStep + 3, 7].Text = "NG";
        //                    //fpSpread1.ActiveSheet.Cells[xStep + 3, 7].ForeColor = Color.White;
        //                    //fpSpread1.ActiveSheet.Cells[xStep + 3, 7].BackColor = Color.Red;
        //                }
        //            }
        //            else
        //            {
        //                switch (xStep)
        //                {
        //                    case 0: //Current
        //                        fpSpread1.ActiveSheet.Cells[xStep + 2, 6].Text = string.Format("{0:0.00}", Value);
        //                        if ((TSpec.LHHeater.Min <= Value) && (Value <= TSpec.LHHeater.Max))
        //                        {
        //                            TestOKCount++;
        //                            if (10 <= TestOKCount)
        //                            {
        //                                TData.LHHeater.Test = true;
        //                                TData.LHHeater.Result = RESULT.PASS;
        //                                TData.LHHeater.Data = Value;

        //                                fpSpread1.ActiveSheet.Cells[xStep + 2, 7].Text = "OK";
        //                                fpSpread1.ActiveSheet.Cells[xStep + 2, 7].ForeColor = Color.Black;
        //                                fpSpread1.ActiveSheet.Cells[xStep + 2, 7].BackColor = Color.Lime;
        //                                //IOPort.outportb(IO_OUT.LH_HEATER_CURRENT, false);
        //                                SpecOutputFlag = false;
        //                                Step++;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            TestOKCount = 0;
        //                        }
        //                        break;
        //                    case 1://NTC
        //                        /*
        //                        fpSpread1.ActiveSheet.Cells[xStep + 3, 6].Text = string.Format("{0:0.00}", Value);

        //                        if ((TSpec.NTC.Min <= Value) && (Value <= TSpec.NTC.Max))
        //                        {
        //                            TestOKCount++;
        //                            if (10 <= TestOKCount)
        //                            {
        //                                TData.HeaterNTC.Test = true;
        //                                TData.HeaterNTC.Result = RESULT.PASS;
        //                                TData.HeaterNTC.Data = Value;

        //                                fpSpread1.ActiveSheet.Cells[xStep + 3, 7].Text = "OK";
        //                                fpSpread1.ActiveSheet.Cells[xStep + 3, 7].ForeColor = Color.Black;
        //                                fpSpread1.ActiveSheet.Cells[xStep + 3, 7].BackColor = Color.Lime;
        //                                outportb(IO_OUT.LH_NTC_RESI, false);
        //                                Step = 6; //통풍(Vent)
        //                                SpecOutputFlag = false;
        //                            }                                    
        //                        }
        //                        else
        //                        {
        //                            TestOKCount = 0;
        //                        }
        //                        */
        //                        break;
        //                    default:
        //                        Step++;
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //    return;
        //}

        //private void NoneECUVentCheck(int xStep)
        //{
        //    if (SpecOutputFlag == false)
        //    {
        //        TestOKCount = 0;
        //        if (xStep == 0)
        //        {
        //            plot1.Channels[0].Clear();
        //            PlotMax = -9999;
        //            PlotMin = 9999;
        //            if (CheckItem.Vent.Curr == false)
        //            {
        //                Step++;
        //                SpecOutputFlag = false;
        //                //IOPort.outportb(IO_OUT.VENT_FAN_CURRENT, false);
        //                return;
        //            }
        //            //IOPort.outportb(IO_OUT.VENT_FAN_CURRENT, true);
        //            //if (PanelMeterReadPos != (short)PANELMETER_MESSAGE.HEATER) PanelMeterReadPos = (short)PANELMETER_MESSAGE.HEATER;
        //        }
        //        else if (xStep == 1)
        //        {
        //            //if (CheckItem.VentFan == false)
        //            //{
        //            Step = 10; //버클센서
        //            SpecOutputFlag = false;
        //            return;
        //            //}
        //            //PanelMeterReadPos = (short)PANELMETER_MESSAGE.NORMAL;
        //        }
        //        SpecOutputFlag = true;
        //        pFirst = ComF.timeGetTimems();
        //        pLast = ComF.timeGetTimems();
        //    }
        //    else
        //    {
        //        float Value;

        //        if (xStep == 0)
        //            Value = pMeter.GetCurr + TSpec.Offset.Vent;
        //        else Value = (float)MultiMeter.GetReadData;

        //        pLast = ComF.timeGetTimems();
        //        if (500 <= (pLast - pFirst))
        //        {

        //            plot1.Channels[0].AddXY(plot1.Channels[0].Count, Value);

        //            plot1.XAxes[0].Tracking.ZoomToFitAll();

        //            bool PlotFlag = false;
        //            if (Value < PlotMin)
        //            {
        //                PlotMin = Value;
        //                PlotFlag = true;
        //            }
        //            if (PlotMax < Value)
        //            {
        //                PlotMax = Value;
        //                PlotFlag = true;
        //            }
        //            if (PlotFlag == true)
        //            {
        //                plot1.YAxes[0].Min = PlotMin - 0.5;
        //                plot1.YAxes[0].Span = (PlotMax + 0.5) - (PlotMin - 0.5);
        //            }

        //            if ((TSpec.VentCheckPos.Time * 1000) <= (pLast - pFirst))
        //            {
        //                switch (xStep)
        //                {
        //                    case 0: //Fan Current
        //                        fpSpread1.ActiveSheet.Cells[xStep + 5, 6].Text = string.Format("{0:0.00}", Value);
        //                        TData.LHFan.Test = true;
        //                        TData.LHFan.Result = RESULT.REJECT;
        //                        TData.LHFan.Data = Value;
        //                        //IOPort.outportb(IO_OUT.VENT_FAN_CURRENT, false);
        //                        Step++;
        //                        break;
        //                    case 1://Blower
        //                        //fpSpread1.ActiveSheet.Cells[xStep + 6, 6].Text = "OFF";
        //                        //TData.BlowerSpeed.Test = true;
        //                        //TData.BlowerSpeed.Result = RESULT.REJECT;
        //                        //TData.BlowerSpeed.Data = Value;
        //                        Step = 10; //버클센서
        //                        SpecOutputFlag = false;
        //                        break;
        //                    default:
        //                        Step++;
        //                        break;
        //                }

        //                if (xStep == 0)
        //                {
        //                    fpSpread1.ActiveSheet.Cells[xStep + 4, 7].Text = "NG";
        //                    fpSpread1.ActiveSheet.Cells[xStep + 4, 7].ForeColor = Color.White;
        //                    fpSpread1.ActiveSheet.Cells[xStep + 4, 7].BackColor = Color.Red;
        //                }
        //                else
        //                {
        //                    //fpSpread1.ActiveSheet.Cells[xStep + 4, 7].Text = "NG";
        //                    //fpSpread1.ActiveSheet.Cells[xStep + 4, 7].ForeColor = Color.White;
        //                    //fpSpread1.ActiveSheet.Cells[xStep + 4, 7].BackColor = Color.Red;
        //                }
        //            }
        //            else
        //            {
        //                switch (xStep)
        //                {
        //                    case 0: //Fan Current
        //                        fpSpread1.ActiveSheet.Cells[xStep + 4, 6].Text = string.Format("{0:0.00}", Value);
        //                        if ((TSpec.LHFan.Min <= Value) && (Value <= TSpec.LHFan.Max))
        //                        {
        //                            TestOKCount++;
        //                            if (10 <= TestOKCount)
        //                            {
        //                                TData.LHFan.Test = true;
        //                                TData.LHFan.Result = RESULT.REJECT;
        //                                TData.LHFan.Data = Value;

        //                                fpSpread1.ActiveSheet.Cells[xStep + 4, 7].Text = "OK";
        //                                fpSpread1.ActiveSheet.Cells[xStep + 4, 7].ForeColor = Color.Black;
        //                                fpSpread1.ActiveSheet.Cells[xStep + 4, 7].BackColor = Color.Lime;
        //                                //IOPort.outportb(IO_OUT.VENT_FAN_CURRENT, false);
        //                                Step++;
        //                            }
        //                            else
        //                            {
        //                                TestOKCount = 0;
        //                            }
        //                        }
        //                        break;
        //                    case 1://Blower
        //                        /*
        //                        bool Flag = InCheck(IO_IN.BLW_SPEED_VOLT_IN);

        //                        if (Flag == true)
        //                        {
        //                            fpSpread1.ActiveSheet.Cells[xStep + 6, 6].Text = "ON";
        //                            TData.BlowerSpeed.Test = true;
        //                            TData.BlowerSpeed.Result = RESULT.PASS;

        //                            fpSpread1.ActiveSheet.Cells[xStep + 6, 7].Text = "OK";
        //                            fpSpread1.ActiveSheet.Cells[xStep + 6, 7].ForeColor = Color.Black;
        //                            fpSpread1.ActiveSheet.Cells[xStep + 6, 7].BackColor = Color.Lime;

        //                            Step = 10; //버클센서
        //                            SpecOutputFlag = false;
        //                        }
        //                        else
        //                        {
        //                            fpSpread1.ActiveSheet.Cells[xStep + 6, 6].Text = "OFF";
        //                        }
        //                        */
        //                        break;
        //                    default:
        //                        Step++;
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //    return;
        //}

        private void VentCheck(int xStep)
        {
            bool Flag1 = false;
            bool Flag2 = false;


            if (SpecOutputFlag == false)
            {
                if (xStep == 0)
                {
                    //plot1.Channels[0].Clear();
                    //PlotMax = -9999;
                    //PlotMin = 9999;
                    GetCol = VentBackColorRead;
                    GetCurrCheck = VentCurrentStep;
                }
                else if (xStep == 1)
                {
                    //if (comboBox1.SelectedIndex == 1)
                    //{
                    //    //BDC
                    //    TData.VentLamp.Result[1] = RESULT.PASS;
                    //    Step++;
                    //    return;
                    //}
                }
                //PanelMeterReadPos = (short)PANELMETER_MESSAGE.HEATER;


                //if (((comboBox2.SelectedItem.ToString() == "LHD") && (comboBox3.SelectedItem.ToString() == "LH")) ||
                //        ((comboBox2.SelectedItem.ToString() == "RHD") && (comboBox3.SelectedItem.ToString() == "RH")))
                //{
                //    VentDrvSW();
                //}
                //else
                //{
                //    VentPassSW();
                //}

                if ((CheckItem.Heater.Curr == true) && (CheckItem.Vent.Curr == true))
                {
                    VentDrvSW();
                }
                else
                {
                    if (comboBox3.SelectedItem.ToString() == "LH")
                        VentPassSW();
                    else VentPassSW();
                }

                pLast = ComF.timeGetTimems();
                pFirst = ComF.timeGetTimems();
                SpecOutputFlag = true;
            }
            else
            {
                TData.VentLamp.Test = true;
                pLast = ComF.timeGetTimems();


                switch (xStep)
                {
                    case 0:
                        //if (comboBox1.SelectedIndex == 1)
                        //{
                        //    if ((GetCol(5, 5) == Color.Lime) && (GetCol(5, 4) == Color.White) && (GetCol(5, 3) == Color.Lime))
                        //    {
                        //        TData.VentLamp.Result[2] = RESULT.PASS;
                        //        if (fpSpread1.ActiveSheet.Cells[5, 6].Text != "2단") fpSpread1.ActiveSheet.Cells[5, 6].Text = "2단";
                        //        Flag1 = true;
                        //    }
                        //}
                        //else
                        //{


                        if (CheckItem.LampTo2Wire == false)
                        {
                            if ((GetCol(5, 5) == Color.Lime) && (GetCol(5, 4) == Color.Lime) && (GetCol(5, 3) == Color.Lime))
                            {
                                TData.VentLamp.Result[2] = RESULT.PASS;
                                if (fpSpread1.ActiveSheet.Cells[5, 6].Text != "3단") fpSpread1.ActiveSheet.Cells[5, 6].Text = "3단";
                                Flag1 = true;
                            }
                        }
                        else
                        {
                            if ((GetCol(5, 5) == Color.Lime) && (GetCol(5, 4) == Color.White) && (GetCol(5, 3) == Color.Lime))
                            {
                                TData.VentLamp.Result[2] = RESULT.PASS;
                                if (fpSpread1.ActiveSheet.Cells[5, 6].Text != "3단") fpSpread1.ActiveSheet.Cells[5, 6].Text = "3단";
                                Flag1 = true;
                            }
                        }
                        //}
                        break;
                    case 1:
                        if (CheckItem.LampTo2Wire == false)
                        {
                            if ((GetCol(5, 5) == Color.White) && (GetCol(5, 4) == Color.Lime) && (GetCol(5, 3) == Color.Lime))
                            {
                                TData.VentLamp.Result[1] = RESULT.PASS;
                                if (fpSpread1.ActiveSheet.Cells[5, 6].Text != "2단") fpSpread1.ActiveSheet.Cells[5, 6].Text = "2단";
                                Flag1 = true;
                            }
                        }
                        else
                        {
                            if ((GetCol(5, 5) == Color.Lime) && (GetCol(5, 4) == Color.White) && (GetCol(5, 3) == Color.White))
                            {
                                TData.VentLamp.Result[1] = RESULT.PASS;
                                if (fpSpread1.ActiveSheet.Cells[5, 6].Text != "2단") fpSpread1.ActiveSheet.Cells[5, 6].Text = "2단";
                                Flag1 = true;
                            }
                        }
                        break;
                    case 2:
                        if ((GetCol(5, 5) == Color.White) && (GetCol(5, 4) == Color.White) && (GetCol(5, 3) == Color.Lime))
                        {
                            TData.VentLamp.Result[0] = RESULT.PASS;
                            if (fpSpread1.ActiveSheet.Cells[5, 6].Text != "1단") fpSpread1.ActiveSheet.Cells[5, 6].Text = "1단";
                            Flag1 = true;
                        }
                        break;
                    case 3:
                        if ((GetCol(5, 5) == Color.White) && (GetCol(5, 4) == Color.White) && (GetCol(5, 3) == Color.White))
                        {
                            Flag1 = true;
                            if (fpSpread1.ActiveSheet.Cells[5, 6].Text != "OFF") fpSpread1.ActiveSheet.Cells[5, 6].Text = "OFF";
                        }
                        break;
                }

                if (xStep < 3)
                {
                    if (GetCurrCheck(xStep) == true)
                    {
                        float Value = pMeter.GetCurr + TSpec.Offset.Vent;

                        //plot1.Channels[0].AddXY(plot1.Channels[0].Count, Value);
                        //plot1.XAxes[0].Tracking.ZoomToFitAll();

                        //bool PlotFlag = false;

                        //if (Value < PlotMin)
                        //{
                        //    PlotMin = Value;
                        //    PlotFlag = true;
                        //}
                        //if (PlotMax < Value)
                        //{
                        //    PlotMax = Value;
                        //    PlotFlag = true;
                        //}
                        //if (PlotFlag == true)
                        //{
                        //    plot1.YAxes[0].Min = PlotMin - 0.5;
                        //    plot1.YAxes[0].Span = (PlotMax + 0.5) - (PlotMin - 0.5);
                        //}

                        TData.VentCurr.Data = Value;
                        TData.VentCurr.Test = true;



                        if (CheckItem.Vent.Curr == true)
                        {
                            if (8000 <= (pLast - pFirst))
                            {
                                if ((TSpec.Vent.Min <= TData.VentCurr.Data) && (TData.VentCurr.Data <= TSpec.Vent.Max))
                                {
                                    fpSpread1.ActiveSheet.Cells[4, 6].Text = string.Format("{0:0.00}", TData.VentCurr.Data);
                                    fpSpread1.ActiveSheet.Cells[4, 7].Text = "OK";
                                    fpSpread1.ActiveSheet.Cells[4, 7].BackColor = Color.Black;
                                    fpSpread1.ActiveSheet.Cells[4, 7].ForeColor = Color.Lime;
                                    Flag2 = true;
                                    TData.VentCurr.Result = RESULT.PASS;
                                }
                                else
                                {
                                    fpSpread1.ActiveSheet.Cells[4, 6].Text = string.Format("{0:0.00}", TData.VentCurr.Data);
                                }
                            }
                            else
                            {
                                fpSpread1.ActiveSheet.Cells[4, 6].Text = string.Format("{0:0.00}", TData.VentCurr.Data);
                            }
                        }
                        else
                        {
                            Flag2 = true;
                        }
                    }
                    else
                    {
                        //검사 위치가 아니면
                        Flag2 = true;
                    }
                }
                else
                {
                    //오프 모드
                    Flag2 = true;
                }
            }

            if ((Flag1 == false) || (Flag2 == false))
            {
                if ((TSpec.VentCheckPos.Time * 1000) <= (pLast - pFirst))
                {                    
                    if (Flag1 == false) TData.VentLamp.Result[2] = RESULT.REJECT;
                    if (Flag2 == false) TData.VentCurr.Result = RESULT.REJECT;

                    if (Flag2 == false)
                    {
                        fpSpread1.ActiveSheet.Cells[4, 7].Text = "NG";
                        fpSpread1.ActiveSheet.Cells[4, 7].BackColor = Color.Black;
                        fpSpread1.ActiveSheet.Cells[4, 7].ForeColor = Color.Red;
                    }

                    if (Flag1 == false)
                    {
                        fpSpread1.ActiveSheet.Cells[5, 7].Text = "NG";
                        fpSpread1.ActiveSheet.Cells[5, 7].BackColor = Color.Black;
                        fpSpread1.ActiveSheet.Cells[5, 7].ForeColor = Color.Red;
                    }

                    Step++;
                    SpecOutputFlag = false;
                }
            }
            else
            {                
                Step++;
                SpecOutputFlag = false;
                if (GetCurrCheck(xStep) == false) ComF.timedelay(200);

                if (xStep == 3)
                {
                    if (fpSpread1.ActiveSheet.Cells[5, 7].Text != "NG")
                    {
                        fpSpread1.ActiveSheet.Cells[5, 7].Text = "OK";
                        fpSpread1.ActiveSheet.Cells[5, 7].BackColor = Color.Black;
                        fpSpread1.ActiveSheet.Cells[5, 7].ForeColor = Color.Lime;
                    }
                }
            }
            return;
        }

        //private void CheckBuckleSensor()
        //{
        //    if (SpecOutputFlag == false)
        //    {
        //        //PanelMeterReadPos = (short)PANELMETER_MESSAGE.BUCKLE;
        //        pLast = ComF.timeGetTimems();
        //        pFirst = ComF.timeGetTimems();
        //        SpecOutputFlag = true;
        //        plot1.Channels[0].Clear();
        //        PlotMax = -9999;
        //        PlotMin = 9999;
        //        TestOKCount = 0;
        //    }
        //    else
        //    {
        //        float Value = pMeter.GetBuckle + TSpec.Offset.BuckleSensor;

        //        plot1.Channels[0].AddXY(plot1.Channels[0].Count, Value);
        //        plot1.XAxes[0].Tracking.ZoomToFitAll();

        //        bool PlotFlag = false;
        //        if (Value < PlotMin)
        //        {
        //            PlotMin = Value;
        //            PlotFlag = true;
        //        }
        //        if (PlotMax < Value)
        //        {
        //            PlotMax = Value;
        //            PlotFlag = true;
        //        }
        //        if (PlotFlag == true)
        //        {
        //            plot1.YAxes[0].Min = PlotMin - 5;
        //            plot1.YAxes[0].Span = (PlotMax + 5) - (PlotMin - 5);
        //        }

        //        pLast = ComF.timeGetTimems();
        //        if (3000 <= (pLast - pFirst))
        //        {
        //            TData.BuckleSensor.Data = Value;
        //            TData.BuckleSensor.Test = true;
        //            TData.BuckleSensor.Result = RESULT.REJECT;
        //            fpSpread1.ActiveSheet.Cells[6, 6].Text = string.Format("{0:0.00}", Value);
        //            fpSpread1.ActiveSheet.Cells[6, 7].Text = "NG";
        //            fpSpread1.ActiveSheet.Cells[6, 7].BackColor = Color.Red;
        //            fpSpread1.ActiveSheet.Cells[6, 7].ForeColor = Color.White;
        //            Step++;
        //            SpecOutputFlag = false;
        //        }
        //        else
        //        {
        //            if ((TSpec.BuckleSensor.Min <= Value) && (Value <= TSpec.BuckleSensor.Max))
        //            {
        //                TestOKCount++;
        //                if (10 <= TestOKCount)
        //                {
        //                    TData.BuckleSensor.Data = Value;
        //                    TData.BuckleSensor.Test = true;
        //                    TData.BuckleSensor.Result = RESULT.PASS;
        //                    fpSpread1.ActiveSheet.Cells[6, 6].Text = string.Format("{0:0.00}", Value);
        //                    fpSpread1.ActiveSheet.Cells[6, 7].Text = "OK";
        //                    fpSpread1.ActiveSheet.Cells[6, 7].BackColor = Color.Lime;
        //                    fpSpread1.ActiveSheet.Cells[6, 7].ForeColor = Color.Black;
        //                    Step++;
        //                    SpecOutputFlag = false;
        //                }
        //            }
        //            else
        //            {
        //                TestOKCount = 0;
        //                fpSpread1.ActiveSheet.Cells[6, 6].Text = string.Format("{0:0.00}", Value);
        //            }
        //        }
        //    }
        //    return;
        //}

        private void CheckBuckleWarning()
        {
            if (SpecOutputFlag == false)
            {
                pLast = ComF.timeGetTimems();
                pFirst = ComF.timeGetTimems();
                SpecOutputFlag = true;
                TestOKCount = 0;
            }
            else
            {
                pLast = ComF.timeGetTimems();
                if (3000 <= (pLast - pFirst))
                {
                    TData.BuckleWarning.Test = true;
                    TData.BuckleWarning.Result = RESULT.REJECT;

                    fpSpread1.ActiveSheet.Cells[6, 6].Text = "OFF";
                    fpSpread1.ActiveSheet.Cells[6, 7].Text = "NG";
                    fpSpread1.ActiveSheet.Cells[6, 7].BackColor = Color.Black;
                    fpSpread1.ActiveSheet.Cells[6, 7].ForeColor = Color.Red;
                    Step++;
                    SpecOutputFlag = false;
                }
                else
                {
                    

                    if (IOPort.GetBuckleWarning == true)
                    {
                        TestOKCount++;
                        if (10 <= TestOKCount)
                        {
                            TData.BuckleWarning.Test = true;
                            TData.BuckleWarning.Result = RESULT.PASS;
                            fpSpread1.ActiveSheet.Cells[6, 6].Text = "ON";
                            fpSpread1.ActiveSheet.Cells[6, 7].Text = "OK";
                            fpSpread1.ActiveSheet.Cells[6, 7].BackColor = Color.Black;
                            fpSpread1.ActiveSheet.Cells[6, 7].ForeColor = Color.Lime;
                            Step++;
                            SpecOutputFlag = false;
                        }
                    }
                    else
                    {
                        TestOKCount = 0;
                        if (fpSpread1.ActiveSheet.Cells[6, 6].Text != "OFF") fpSpread1.ActiveSheet.Cells[6, 6].Text = "OFF";
                    }
                }
            }
            return;
        }

        private void CheckRetractor()
        {
            if (SpecOutputFlag == false)
            {
                pLast = ComF.timeGetTimems();
                pFirst = ComF.timeGetTimems();
                SpecOutputFlag = true;
                //plot1.Channels[0].Clear();
                //PlotMax = -9999;
                //PlotMin = 9999;
                TestOKCount = 0;
            }
            else
            {
                pLast = ComF.timeGetTimems();
                if (500 <= (pLast - pFirst))
                {
                    float Value = (float)MultiMeter.GetData + TSpec.Offset.Retractor;

                    //plot1.Channels[0].AddXY(plot1.Channels[0].Count, Value);
                    //plot1.XAxes[0].Tracking.ZoomToFitAll();

                    //bool PlotFlag = false;
                    //if (Value < PlotMin)
                    //{
                    //    PlotMin = Value;
                    //    PlotFlag = true;
                    //}
                    //if (PlotMax < Value)
                    //{
                    //    PlotMax = Value;
                    //    PlotFlag = true;
                    //}
                    //if (PlotFlag == true)
                    //{
                    //    plot1.YAxes[0].Min = PlotMin - 5;
                    //    plot1.YAxes[0].Span = (PlotMax + 5) - (PlotMin - 5);
                    //}

                    if (4000 <= (pLast - pFirst))
                    {
                        TData.Retractor.Data = Value;
                        TData.Retractor.Test = true;
                        TData.Retractor.Result = RESULT.REJECT;

                        string s;
                        if (Value < 9999)
                            s = string.Format("{0:0.00}", Value);
                        else s = "OL";

                        fpSpread1.ActiveSheet.Cells[7, 6].Text = s;
                        IOPort.RetractorOnOff = false;
                        fpSpread1.ActiveSheet.Cells[7, 7].Text = "NG";
                        fpSpread1.ActiveSheet.Cells[7, 7].BackColor = Color.Black;
                        fpSpread1.ActiveSheet.Cells[7, 7].ForeColor = Color.Red;
                        Step++;
                        SpecOutputFlag = false;
                    }
                    else
                    {
                        if (2000 <= (pLast - pFirst))
                        {
                            if ((TSpec.Retractor.Min <= Value) && (Value <= TSpec.Retractor.Max))
                            {
                                TestOKCount++;
                                if (3 <= TestOKCount)
                                {
                                    TData.Retractor.Data = Value;
                                    TData.Retractor.Test = true;
                                    TData.Retractor.Result = RESULT.PASS;

                                    string s;
                                    if (Value < 9999)
                                        s = string.Format("{0:0.00}", Value);
                                    else s = "OL";

                                    fpSpread1.ActiveSheet.Cells[7, 6].Text = s;
                                    IOPort.RetractorOnOff = false;
                                    fpSpread1.ActiveSheet.Cells[7, 7].Text = "OK";
                                    fpSpread1.ActiveSheet.Cells[7, 7].BackColor = Color.Black;
                                    fpSpread1.ActiveSheet.Cells[7, 7].ForeColor = Color.Lime;
                                    Step++;
                                    SpecOutputFlag = false;
                                }
                            }
                            else
                            {
                                TestOKCount = 0;

                                string s;
                                if (Value < 9999)
                                    s = string.Format("{0:0.00}", Value);
                                else s = "OL";
                                fpSpread1.ActiveSheet.Cells[7, 6].Text = s;
                            }
                        }
                    }
                }
            }
            return;
        }

        private bool CheckCurrOff(int xStep)
        {
            if (OffSpecOutputFlag == false)
            {
                //PanelMeterReadPos = (short)PANELMETER_MESSAGE.P_SEAT;
                SWCheckOFFFirst = ComF.timeGetTimems();
                SWCheckOFFLast = ComF.timeGetTimems();
                OffSpecOutputFlag = true;
            }

            SWCheckOFFLast = ComF.timeGetTimems();

            if ((TSpec.SWCheckTime * 1000) <= (SWCheckOFFLast - SWCheckOFFFirst))
            {
                switch (xStep)
                {
                    case 0: //Recline
                        TData.PW_Recline.Test = true;
                        TData.PW_Recline.Result = RESULT.REJECT;
                        break;
                    case 1: //Legrest
                        TData.PW_Legrest.Test = true;
                        TData.PW_Legrest.Result = RESULT.REJECT;
                        break;
                    case 2: //Legrest ext
                        TData.PW_LegrestExt.Test = true;
                        TData.PW_LegrestExt.Result = RESULT.REJECT;
                        break;
                }
                fpSpread1.ActiveSheet.Cells[8 + xStep, 6].Text = "0.0";
                fpSpread1.ActiveSheet.Cells[8 + xStep, 7].Text = "NG";
                fpSpread1.ActiveSheet.Cells[8 + xStep, 7].BackColor = Color.Black;
                fpSpread1.ActiveSheet.Cells[8 + xStep, 7].ForeColor = Color.Red;

                Step++;
                SpecOutputFlag = false;
                return true;
            }
            else
            {
                if (pMeter.GetCurr <= TSpec.SWOffCurr)
                {
                    //SWCheckFirstFlag = true;
                    return true;
                }
            }

            return false;
        }

        private void CheckPowerSeat(int xStep)
        {
            if (SpecOutputFlag == false)
            {
                IOPort.outportbToFunction(IO_OUT_FUNC.PSEAT_BATT, true);
                //IOPort.outportb(IO_OUT.PSEAT_IGN, true);
                //PanelMeterReadPos = (short)PANELMETER_MESSAGE.P_SEAT;
                TestOKCount = 0;
                if (xStep == 0)
                {
                    //if (0 < plot1.Channels[0].Count)
                    //{
                    //    plot1.Channels[0].Clear();
                    //    PlotMax = -9999;
                    //    PlotMin = 9999;
                    //}

                    IOPort.outportbToFunction(IO_OUT_FUNC.DRV_BATT, true);
                    IOPort.outportbToFunction(IO_OUT_FUNC.PSEAT_BATT, true);
                    //IOPort.outportb(IO_OUT.PSEAT_IGN, true);
                    label5.Text = "POWER 스위치를 동작시켜 주십시오.";
                    ComF.timedelay(500);
                }

                //switch (xStep)
                //{
                //    case 0: //slid
                //        label5.Text = "SLIDE S/W를 동작시켜 주십시오.";
                //        break;
                //    case 1: //Height
                //        label5.Text = "HEIGHT S/W를 동작시켜 주십시오.";
                //        break;
                //    case 2: //Tilt
                //        label5.Text = "TILT S/W를 동작시켜 주십시오.";
                //        break;
                //    case 3: //Recline
                //        label5.Text = "RECLINE S/W를 동작시켜 주십시오.";
                //        break;
                //}
                if (xStep == 0)
                    IOPort.SWOnBuzzer();
                else IOPort.SWCheckBuzzer();
                pLast = ComF.timeGetTimems();
                pFirst = ComF.timeGetTimems();
                SpecOutputFlag = true;
                //SWCheckFirstFlag = false;
                OffSpecOutputFlag = false;
                SWOffCheckFlag = false;
            }
            else
            {
                bool Flag;

                Flag = false;
                
                if (SWOffCheckFlag == false)
                {
                    Flag = CheckCurrOff(xStep);
                    pLast = ComF.timeGetTimems();
                    pFirst = ComF.timeGetTimems();
                    if (Flag == true) SWOffCheckFlag = true;
                }
                else
                {
                    Flag = true;
                }

                float Value = pMeter.GetCurr;

                //plot1.Channels[0].AddXY(plot1.Channels[0].Count, Value);
                //plot1.XAxes[0].Tracking.ZoomToFitAll();

                //bool PlotFlag = false;
                //if (Value < PlotMin)
                //{
                //    PlotMin = Value;
                //    PlotFlag = true;
                //}
                //if (PlotMax < Value)
                //{
                //    PlotMax = Value;
                //    PlotFlag = true;
                //}
                //if (PlotFlag == true)
                //{
                //    plot1.YAxes[0].Min = PlotMin - 5;
                //    plot1.YAxes[0].Span = (PlotMax + 5) - (PlotMin - 5);
                //}

                if (Flag == true)
                {
                    pLast = ComF.timeGetTimems();

                    if ((TSpec.SWCheckTime * 1000) <= (pLast - pFirst))
                    {
                        //switch (xStep)
                        //{
                        //case 0: //slid
                        //    TData.PW_Slide.Test = true;
                        //    TData.PW_Slide.Result = RESULT.REJECT;                                
                        //    TData.PW_Slide.Data = Value;
                        //    break;
                        //case 1: //Height
                        //    TData.PW_Height.Test = true;
                        //    TData.PW_Height.Result = RESULT.REJECT;
                        //    TData.PW_Height.Data = Value;
                        //    break;
                        //case 2: //Tilt
                        //    TData.PW_Tilt.Test = true;
                        //    TData.PW_Tilt.Result = RESULT.REJECT;
                        //    TData.PW_Tilt.Data = Value;
                        //    break;
                        //case 3: //Recline
                        //    TData.PW_Recline.Test = true;
                        //    TData.PW_Recline.Result = RESULT.REJECT;
                        //    TData.PW_Recline.Data = Value;
                        //    break;
                        //}

                        TData.PW_Recline.Test = true;
                        TData.PW_Recline.Result = RESULT.REJECT;
                        TData.PW_Recline.Data = Value;
                        TData.PW_Legrest.Test = true;
                        TData.PW_Legrest.Result = RESULT.REJECT;
                        TData.PW_Legrest.Data = Value;
                        TData.PW_LegrestExt.Test = true;
                        TData.PW_LegrestExt.Result = RESULT.REJECT;
                        TData.PW_LegrestExt.Data = Value;
                        TData.PW_Relax.Test = true;
                        TData.PW_Relax.Result = RESULT.REJECT;
                        TData.PW_Relax.Data = Value;

                        fpSpread1.ActiveSheet.Cells[8 + xStep, 6].Text = string.Format("{0:0.00}", Value);
                        fpSpread1.ActiveSheet.Cells[8 + xStep, 7].Text = "NG";
                        fpSpread1.ActiveSheet.Cells[8 + xStep, 7].BackColor = Color.Black;
                        fpSpread1.ActiveSheet.Cells[8 + xStep, 7].ForeColor = Color.Red;
                        Step += 4;
                        SpecOutputFlag = false;
                        OffSpecOutputFlag = false;
                    }
                    else
                    {
                        if ((TSpec.PWSwitch.Min <= Value) && (Value <= TSpec.PWSwitch.Max))
                        {
                            TestOKCount++;
                            if (10 <= TestOKCount)
                            {
                                //switch (xStep)
                                //{
                                //    case 0: //slid
                                //        TData.PW_Slide.Test = true;
                                //        TData.PW_Slide.Result = RESULT.PASS;
                                //        TData.PW_Slide.Data = Value;
                                //        break;
                                //    case 1: //Height
                                //        TData.PW_Height.Test = true;
                                //        TData.PW_Height.Result = RESULT.PASS;
                                //        TData.PW_Height.Data = Value;
                                //        break;
                                //    case 2: //Tilt
                                //        TData.PW_Tilt.Test = true;
                                //        TData.PW_Tilt.Result = RESULT.PASS;
                                //        TData.PW_Tilt.Data = Value;
                                //        break;
                                //    case 3: //Recline
                                //        TData.PW_Recline.Test = true;
                                //        TData.PW_Recline.Result = RESULT.PASS;
                                //        TData.PW_Recline.Data = Value;
                                //        break;
                                //}

                                TData.PW_Recline.Test = true;
                                TData.PW_Recline.Result = RESULT.PASS;
                                TData.PW_Recline.Data = Value;
                                TData.PW_Legrest.Test = true;
                                TData.PW_Legrest.Result = RESULT.PASS;
                                TData.PW_Legrest.Data = Value;
                                TData.PW_LegrestExt.Test = true;
                                TData.PW_LegrestExt.Result = RESULT.PASS;
                                TData.PW_LegrestExt.Data = Value;
                                TData.PW_Relax.Test = true;
                                TData.PW_Relax.Result = RESULT.PASS;
                                TData.PW_Relax.Data = Value;

                                fpSpread1.ActiveSheet.Cells[8 + xStep, 6].Text = string.Format("{0:0.00}", Value);
                                fpSpread1.ActiveSheet.Cells[8 + xStep, 7].Text = "OK";
                                fpSpread1.ActiveSheet.Cells[8 + xStep, 7].BackColor = Color.Black;
                                fpSpread1.ActiveSheet.Cells[8 + xStep, 7].ForeColor = Color.Lime;
                                Step += 4;
                                SpecOutputFlag = false;
                                OffSpecOutputFlag = false;
                            }
                        }
                        else
                        {
                            fpSpread1.ActiveSheet.Cells[8 + xStep, 6].Text = string.Format("{0:0.00}", Value);
                            TestOKCount = 0;
                        }
                    }
                }
            }
            return;
        }

        //private void CheckLumberSeat(int xStep)
        //{
        //    if (SpecOutputFlag == false)
        //    {
        //        TestOKCount = 0;
        //        //PanelMeterReadPos = (short)PANELMETER_MESSAGE.P_SEAT;
        //        if (xStep == 0)
        //        {

        //            if (0 < plot1.Channels[0].Count)
        //            {
        //                plot1.Channels[0].Clear();
        //                PlotMax = -9999;
        //                PlotMin = 9999;
        //            }

        //            IOPort.outportbToFunction(IO_OUT_FUNC.DRV_BATT, false);
        //            IOPort.outportbToFunction(IO_OUT_FUNC.PSEAT_BATT, true);
        //            //IOPort.outportb(IO_OUT.PSEAT_IGN, true);
        //            //SWCheckOFFFirst = ComF.timeGetTimems();
        //            //SWCheckOFFLast = ComF.timeGetTimems();

        //            //if (SWCheckFirstFlag == false)
        //            //{
        //            //    if (CheckCurrOff(xStep,false) == false) return;
        //            //}

        //            label5.Text = "LUMBER 스위치 동작시켜 주십시오.";
        //            ComF.timedelay(500);
        //        }

        //        if ((CheckItem.Lumber.FW == false) && (CheckItem.Lumber.BW == false) && (CheckItem.Lumber.Up == false) && (CheckItem.Lumber.Dn == false))
        //        {
        //            SpecOutputFlag = false;
        //            Step += 4;
        //            return;
        //        }

        //        //switch (xStep)
        //        //{
        //        //    case 0: //Fw
        //        //        if (CheckItem.Lumber.FW == false)
        //        //        {
        //        //            SpecOutputFlag = false;
        //        //            Step++;
        //        //            return;
        //        //        }
        //        //        break;
        //        //    case 1: //Bw
        //        //        if (CheckItem.Lumber.BW == false)
        //        //        {
        //        //            SpecOutputFlag = false;
        //        //            Step++;
        //        //            return;
        //        //        }
        //        //        break;
        //        //    case 2: //Up
        //        //        if (CheckItem.Lumber.Up == false)
        //        //        {
        //        //            SpecOutputFlag = false;
        //        //            Step++;
        //        //            return;
        //        //        }
        //        //        break;
        //        //    case 3: //Dn
        //        //        if (CheckItem.Lumber.Dn == false)
        //        //        {
        //        //            SpecOutputFlag = false;
        //        //            Step++;
        //        //            return;
        //        //        }
        //        //        break;
        //        //}

        //        //switch (xStep)
        //        //    {
        //        //        case 0: //Fw
        //        //            label5.Text = "LUMBER S/W를 FORWARD로 동작시켜 주십시오.";
        //        //            break;
        //        //        case 1: //Bw
        //        //            label5.Text = "LUMBER S/W를 BACKWARD로 동작시켜 주십시오.";
        //        //            break;
        //        //        case 2: //Up
        //        //            label5.Text = "LUMBER S/W를 UP으로 동작시켜 주십시오.";
        //        //            break;
        //        //        case 3: //Dn
        //        //            label5.Text = "LUMBER S/W를 DOWN 으로 동작시켜 주십시오.";
        //        //            break;
        //        //    }
        //        if (xStep == 0)
        //            IOPort.SWOnBuzzer();
        //        else IOPort.SWCheckBuzzer();
        //        pLast = ComF.timeGetTimems();
        //        pFirst = ComF.timeGetTimems();
        //        SpecOutputFlag = true;
        //        OffSpecOutputFlag = false;
        //        SWOffCheckFlag = false;
        //    }
        //    else
        //    {
        //        bool Flag;

        //        Flag = false;
                
        //        if (SWOffCheckFlag == false)
        //        {
        //            Flag = CheckCurrOff(xStep, false);
        //            pLast = ComF.timeGetTimems();
        //            pFirst = ComF.timeGetTimems();
        //            if (Flag == true) SWOffCheckFlag = true;
        //        }
        //        else
        //        {
        //            Flag = true;
        //        }


        //        float Value = pMeter.GetCurr;

        //        plot1.Channels[0].AddXY(plot1.Channels[0].Count, Value);
        //        plot1.XAxes[0].Tracking.ZoomToFitAll();

        //        bool PlotFlag = false;
        //        if (Value < PlotMin)
        //        {
        //            PlotMin = Value;
        //            PlotFlag = true;
        //        }
        //        if (PlotMax < Value)
        //        {
        //            PlotMax = Value;
        //            PlotFlag = true;
        //        }
        //        if (PlotFlag == true)
        //        {
        //            plot1.YAxes[0].Min = PlotMin - 5;
        //            plot1.YAxes[0].Span = (PlotMax + 5) - (PlotMin - 5);
        //        }

        //        if (Flag == true)
        //        {
        //            pLast = ComF.timeGetTimems();

        //            if ((TSpec.SWCheckTime * 1000) <= (pLast - pFirst))
        //            {
        //                //switch (xStep)
        //                //{
        //                //    case 0: //FW
        //                //        TData.LumberFW.Test = true;
        //                //        TData.LumberFW.Result = RESULT.REJECT;
        //                //        break;
        //                //    case 1: //BW
        //                //        TData.LumberBW.Test = true;
        //                //        TData.LumberBW.Result = RESULT.REJECT;
        //                //        break;
        //                //    case 2: //UP
        //                //        TData.LumberUP.Test = true;
        //                //        TData.LumberUP.Result = RESULT.REJECT;
        //                //        break;
        //                //    case 3: //DN
        //                //        TData.LumberDN.Test = true;
        //                //        TData.LumberDN.Result = RESULT.REJECT;
        //                //        break;
        //                //}                        

        //                TData.LumberFW.Test = true;
        //                TData.LumberFW.Result = RESULT.REJECT;
        //                TData.LumberBW.Test = true;
        //                TData.LumberBW.Result = RESULT.REJECT;
        //                TData.LumberUP.Test = true;
        //                TData.LumberUP.Result = RESULT.REJECT;
        //                TData.LumberDN.Test = true;
        //                TData.LumberDN.Result = RESULT.REJECT;

        //                fpSpread1.ActiveSheet.Cells[13 + xStep, 6].Text = string.Format("{0:0.00}", Value);
        //                fpSpread1.ActiveSheet.Cells[13 + xStep, 7].Text = "NG";
        //                fpSpread1.ActiveSheet.Cells[13 + xStep, 7].BackColor = Color.Red;
        //                fpSpread1.ActiveSheet.Cells[13 + xStep, 7].ForeColor = Color.White;
        //                Step += 4;
        //                SpecOutputFlag = false;
        //                OffSpecOutputFlag = false;
        //            }
        //        }
        //        else
        //        {
        //            if ((TSpec.Lumber.Min <= Value) && (Value <= TSpec.Lumber.Max))
        //            {
        //                TestOKCount++;
        //                if (10 <= TestOKCount)
        //                {
        //                    //switch (xStep)
        //                    //{
        //                    //    case 0: //FW
        //                    //        TData.LumberFW.Test = true;
        //                    //        TData.LumberFW.Result = RESULT.PASS;
        //                    //        break;
        //                    //    case 1: //BW
        //                    //        TData.LumberBW.Test = true;
        //                    //        TData.LumberBW.Result = RESULT.PASS;
        //                    //        break;
        //                    //    case 2: //UP
        //                    //        TData.LumberUP.Test = true;
        //                    //        TData.LumberUP.Result = RESULT.PASS;
        //                    //        break;
        //                    //    case 3: //DN
        //                    //        TData.LumberDN.Test = true;
        //                    //        TData.LumberDN.Result = RESULT.PASS;
        //                    //        break;
        //                    //}
        //                    TData.LumberFW.Test = true;
        //                    TData.LumberFW.Result = RESULT.PASS;
        //                    TData.LumberBW.Test = true;
        //                    TData.LumberBW.Result = RESULT.PASS;
        //                    TData.LumberUP.Test = true;
        //                    TData.LumberUP.Result = RESULT.PASS;
        //                    TData.LumberDN.Test = true;
        //                    TData.LumberDN.Result = RESULT.PASS;

        //                    fpSpread1.ActiveSheet.Cells[13 + xStep, 6].Text = string.Format("{0:0.00}", Value);
        //                    fpSpread1.ActiveSheet.Cells[13 + xStep, 7].Text = "OK";
        //                    fpSpread1.ActiveSheet.Cells[13 + xStep, 7].BackColor = Color.Lime;
        //                    fpSpread1.ActiveSheet.Cells[13 + xStep, 7].ForeColor = Color.Black;
        //                    Step += 4;
        //                    SpecOutputFlag = false;
        //                    OffSpecOutputFlag = false;

        //                }
        //            }
        //            else
        //            {
        //                fpSpread1.ActiveSheet.Cells[13 + xStep, 6].Text = string.Format("{0:0.00}", Value);
        //                TestOKCount = 0;
        //            }
        //        }                
        //    }
        //    return;
        //}
        //private void CheckSBR()
        //{
        //    if (CheckItem.SBR == false)
        //    {
        //        Step++;
        //        SpecOutputFlag = false;
        //        return;
        //    }
        //    if (SpecOutputFlag == false)
        //    {
        //        IOPort.outportb(IO_OUT.AIRBAG_RESI_SELECT, false);
        //        pLast = ComF.timeGetTimems();
        //        pFirst = ComF.timeGetTimems();
        //        SpecOutputFlag = true;

        //        //Meter.ResitanceModeSet(500);
        //        //ComF.timedelay(300);
        //        //Meter.AutoModeSet(false);
        //        //ComF.timedelay(300);
        //        //GDM_8243_check_Flag = false;
        //        //Send_req_gdm_8342();
        //        plot1.Channels[0].Clear();
        //        PlotMax = -9999;
        //        PlotMin = 9999;
        //        TestOKCount = 0;
        //    }
        //    else
        //    {
        //        pLast = ComF.timeGetTimems();
        //        if (500 <= (pLast - pFirst))
        //        {
        //            float Value = (float)ADData[(short)ADPos.MULTI_METER];
                    
        //            plot1.Channels[0].AddXY(plot1.Channels[0].Count, Value);
        //            plot1.XAxes[0].Tracking.ZoomToFitAll();

        //            bool PlotFlag = false;
        //            if (Value < PlotMin)
        //            {
        //                PlotMin = Value;
        //                PlotFlag = true;
        //            }
        //            if (PlotMax < Value)
        //            {
        //                PlotMax = Value;
        //                PlotFlag = true;
        //            }
        //            if (PlotFlag == true)
        //            {
        //                plot1.YAxes[0].Min = PlotMin - 5;
        //                plot1.YAxes[0].Span = (PlotMax + 5) - (PlotMin - 5);
        //            }
                    
        //            if (3000 <= (pLast - pFirst))
        //            {
        //                IOPort.outportb(IO_OUT.AIRBAG_RESI_SELECT, false);
        //                TData.SBR.Data = Value;
        //                TData.SBR.Test = true;
        //                TData.SBR.Result = RESULT.REJECT;
        //                fpSpread1.ActiveSheet.Cells[11, 6].Text = string.Format("{0:0.00}", Value);
        //                IOPort.outportb(IO_OUT.AIRBAG_RESI_SELECT, false);
        //                fpSpread1.ActiveSheet.Cells[11, 7].Text = "NG";
        //                fpSpread1.ActiveSheet.Cells[11, 7].BackColor = Color.Red;
        //                fpSpread1.ActiveSheet.Cells[11, 7].ForeColor = Color.White;
        //                Step++;
        //                SpecOutputFlag = false;
        //            }
        //            else
        //            {                        
        //                if ((TSpec.SBR.Min <= Value) && (Value <= TSpec.SBR.Max))
        //                {
        //                    TestOKCount++;
        //                    if (10 <= TestOKCount)
        //                    {
        //                        TData.SBR.Data = Value;
        //                        TData.SBR.Test = true;
        //                        TData.SBR.Result = RESULT.PASS;
        //                        fpSpread1.ActiveSheet.Cells[11, 6].Text = string.Format("{0:0.00}", Value);
        //                        IOPort.outportb(IO_OUT.AIRBAG_RESI_SELECT, false);
        //                        fpSpread1.ActiveSheet.Cells[11, 7].Text = "OK";
        //                        fpSpread1.ActiveSheet.Cells[11, 7].BackColor = Color.Lime;
        //                        fpSpread1.ActiveSheet.Cells[11, 7].ForeColor = Color.Black;
        //                        Step++;
        //                        SpecOutputFlag = false;
        //                    }
        //                }
        //                else
        //                {
        //                    TestOKCount = 0;
        //                    fpSpread1.ActiveSheet.Cells[11, 6].Text = string.Format("{0:0.00}", Value);
        //                }
        //            }
        //        }
        //    }
        //    return;
        //}

        //private void CheckILLVoltage()
        //{
            /*
            if (CheckItem.ILL == false)
            {
                Step++;
                SpecOutputFlag = false;
                return;
            }

            if (SpecOutputFlag == false)
            {
                pLast = ComF.timeGetTimems();
                pFirst = ComF.timeGetTimems();
                SpecOutputFlag = true;
            }
            else
            {
                bool In = InCheck(IO_IN.ILL_VOLT_IN);

                pLast = ComF.timeGetTimems();

                if (3000 <= (pLast - pFirst))
                {                    
                    TData.ILL.Test = true;
                    TData.ILL.Result = RESULT.REJECT;

                    fpSpread1.ActiveSheet.Cells[20, 6].Text = "OFF";
                    fpSpread1.ActiveSheet.Cells[20, 7].Text = "NG";
                    fpSpread1.ActiveSheet.Cells[20, 7].BackColor = Color.Red;
                    fpSpread1.ActiveSheet.Cells[20, 7].ForeColor = Color.White;
                    Step++;
                    SpecOutputFlag = false;
                }
                else
                {
                    if (In == true)
                    {
                        TData.ILL.Test = true;
                        TData.ILL.Result = RESULT.PASS;

                        fpSpread1.ActiveSheet.Cells[20, 6].Text = "ON";                                                                        
                        fpSpread1.ActiveSheet.Cells[20, 7].Text = "OK";
                        fpSpread1.ActiveSheet.Cells[20, 7].BackColor = Color.Lime;
                        fpSpread1.ActiveSheet.Cells[20, 7].ForeColor = Color.Black;
                        Step++;
                        SpecOutputFlag = false;
                    }
                    else
                    {
                        if(fpSpread1.ActiveSheet.Cells[20, 6].Text != "OFF") fpSpread1.ActiveSheet.Cells[22, 7].Text = "OFF";
                    }
                }
            }
            */
        //    return;
        //}

        private void CheckResult()
        {
            IOPort.outportb(IO_OUT.TEST_ING, false);

            TData.Result = RESULT.PASS;
            if ((TData.Retractor.Test == true) && (TData.Retractor.Result == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.BuckleWarning.Test == true) && (TData.BuckleWarning.Result == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.HeaterCurrent.Test == true) && (TData.HeaterCurrent.Result == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.HeaterLamp.Test == true) && (TData.HeaterLamp.Result[0] == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.HeaterLamp.Test == true) && (TData.HeaterLamp.Result[1] == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.HeaterLamp.Test == true) && (TData.HeaterLamp.Result[2] == RESULT.REJECT)) TData.Result = RESULT.REJECT;

            if ((TData.SBR.Test == true) && (TData.SBR.Result == RESULT.REJECT)) TData.Result = RESULT.REJECT;

            if ((TData.PW_Recline.Test == true) && (TData.PW_Recline.Result == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.PW_Legrest.Test == true) && (TData.PW_Legrest.Result == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.PW_LegrestExt.Test == true) && (TData.PW_LegrestExt.Result == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.PW_Relax.Test == true) && (TData.PW_Relax.Result == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if (TData.SBR.Test == true)
            {
                if (TData.SBR.Result == RESULT.REJECT) TData.Result = RESULT.REJECT;
                if (TData.SBR.Result15Kg == RESULT.REJECT) TData.Result = RESULT.REJECT;
                if (TData.SBR.Result30Kg == RESULT.REJECT) TData.Result = RESULT.REJECT;
                if (TData.SBR.ResultNotLoad == RESULT.REJECT) TData.Result = RESULT.REJECT;
            }
            if ((TData.VentCurr.Test == true) && (TData.VentCurr.Result == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.VentLamp.Test == true) && (TData.VentLamp.Result[0] == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.VentLamp.Test == true) && (TData.VentLamp.Result[1] == RESULT.REJECT)) TData.Result = RESULT.REJECT;
            if ((TData.VentLamp.Test == true) && (TData.VentLamp.Result[2] == RESULT.REJECT)) TData.Result = RESULT.REJECT;

            if (TData.Result == RESULT.PASS)
            {
                label16.Text = "OK";
                label16.BackColor = Color.Black;
                label16.ForeColor = Color.Lime;
                TInfor.Count.OK++;
                label5.Text = "PASS 버튼을 눌러 제품 배출을 진행해 주십시오.";
                IOPort.outportb(IO_OUT.GREEN, true);
                //IOPort.outportb(IO_OUT.YELLOW, false);
                IOPort.outportb(IO_OUT.YELLOW, false);
                IOPort.outportb(IO_OUT.RED, false);
                IOPort.outportb(IO_OUT.BUZZER, true);
                ComF.timedelay(500);
                IOPort.outportb(IO_OUT.BUZZER, false);
            }
            else
            {
                label16.Text = "NG";
                label16.BackColor = Color.Black;
                label16.ForeColor = Color.Red;
                TInfor.Count.NG++;
                label5.Text = "불량 입니다. 제검사를 하시려면 RESET 버튼을 눌러 초기화 한 후 PASS 버튼을 눌러 검사를 진행하십시오.";
                IOPort.outportb(IO_OUT.GREEN, false);
                //IOPort.outportb(IO_OUT.YELLOW, false);
                //IOPort.outportb(IO_OUT.YELLOW, true);
                IOPort.outportb(IO_OUT.RED, true);
                IOPort.outportb(IO_OUT.YELLOW, false);

                BuzzerFirst = ComF.timeGetTimems();
                BuzzerLast = ComF.timeGetTimems();                
                BuzerOnOff = true;
                BuzzerRunFlag = true;
                BuzzerOnCount = 1;
                IOPort.outportb(IO_OUT.BUZZER, true);
            }
            ComF.timedelay(100);
            //if (TData.Result == RESULT.PASS)
            //{
            SaveDataFlag = true;
            SaveData();
            SendData();
            //}
            RunningFlag = false;
            return;
        }

        
        
        //private void CheckCanPinOpen(int xStep)
        //{
        //    if (CheckItem.Can == false)
        //    {
        //        Step++;
        //        return;
        //    }
        //    if (SpecOutputFlag == false)
        //    {
        //        IOPort.outportb(IO_OUT_FUNC.HEATER_BATT, true);
        //        IOPort.outportb(IO_OUT_FUNC.PSEAT_BATT, true);
        //        IOPort.outportb(IO_OUT.PSEAT_IGN, true);
        //        switch (xStep)
        //        {
        //            case 0:
        //                IOPort.outportb(IO_OUT.CAN_HI, true);
        //                IOPort.outportb(IO_OUT.CAN_LO, false);
        //                if (SpecOutputFlag == false) label5.Text = "CAN PIN OPEN 검사 중입니다.";
        //                break;
        //            case 1:
        //                IOPort.outportb(IO_OUT.CAN_HI, false);
        //                IOPort.outportb(IO_OUT.CAN_LO, true);
        //                break;
        //        }

        //        CanReadCount = 0;

        //        short Ch;

        //        SpecOutputFlag = true;
        //        pLast = ComF.timeGetTimems();
        //        pFirst = ComF.timeGetTimems();
        //    }
        //    else
        //    {                               
        //        pLast = ComF.timeGetTimems();
        //        if (3000 <= (pLast - pFirst))
        //        {
        //            if (xStep == 0)
        //            {
        //                TData.CanHi.Test = true;
        //                TData.CanHi.Result = RESULT.REJECT;

        //                fpSpread1.ActiveSheet.Cells[24, 6].Text = "OFF";
        //                fpSpread1.ActiveSheet.Cells[24, 7].Text = "NG";
        //                fpSpread1.ActiveSheet.Cells[24, 7].BackColor = Color.Red;
        //                fpSpread1.ActiveSheet.Cells[24, 7].ForeColor = Color.White;
        //            }
        //            else
        //            {
        //                TData.CanLo.Test = true;
        //                TData.CanLo.Result = RESULT.REJECT;

        //                fpSpread1.ActiveSheet.Cells[25, 6].Text = "OFF";
        //                fpSpread1.ActiveSheet.Cells[25, 7].Text = "NG";
        //                fpSpread1.ActiveSheet.Cells[25, 7].BackColor = Color.Red;
        //                fpSpread1.ActiveSheet.Cells[25, 7].ForeColor = Color.White;
        //            }
        //            Step++;
        //            SpecOutputFlag = false;
        //        }
        //        else
        //        {
        //            if (3 <= CanReadCount)
        //            {
        //                if (xStep == 0)
        //                {
        //                    TData.CanHi.Test = true;
        //                    TData.CanHi.Result = RESULT.PASS;

        //                    fpSpread1.ActiveSheet.Cells[24, 7].Text = "ON";
        //                    fpSpread1.ActiveSheet.Cells[24, 7].Text = "OK";
        //                    fpSpread1.ActiveSheet.Cells[24, 7].BackColor = Color.Lime;
        //                    fpSpread1.ActiveSheet.Cells[24, 7].ForeColor = Color.Black;
        //                }
        //                else
        //                {
        //                    TData.CanLo.Test = true;
        //                    TData.CanLo.Result = RESULT.PASS;

        //                    fpSpread1.ActiveSheet.Cells[25, 7].Text = "ON";
        //                    fpSpread1.ActiveSheet.Cells[25, 7].Text = "OK";
        //                    fpSpread1.ActiveSheet.Cells[25, 7].BackColor = Color.Lime;
        //                    fpSpread1.ActiveSheet.Cells[25, 7].ForeColor = Color.Black;
        //                }
        //                Step++;
        //                SpecOutputFlag = false;
        //            }
        //        }
        //    }
        //    return;
        //}

        private void CreateFileName()
        {
            string Path = Program.DATA_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMM") + ".xls";

            //if ((TInfor.DataName != "") && (TInfor.DataName != null))
            //{
            //    if (File.Exists(TInfor.DataName))
            //    {
            //        if (TInfor.DataName != Path)
            //        {
            //            TInfor.Date = DateTime.Now.ToString("yyyyMMdd");
            //            TInfor.DataName = Path;
            //            TInfor.Count.Total = 0;
            //            TInfor.Count.OK = 0;
            //            TInfor.Count.NG = 0;
            //            TInfor.ReBootingFlag = false;
            //            TInfor.Model = comboBox1.SelectedItem.ToString();
            //            SaveInfor();                        
            //        }
            //    }
            //}
            //else
            //{
            //    TInfor.Date = DateTime.Now.ToString("yyyyMMdd");
            //    TInfor.DataName = Path;
            //    TInfor.DataName = Path;
            //    TInfor.Count.Total = 0;
            //    TInfor.Count.OK = 0;
            //    TInfor.Count.NG = 0;
            //    TInfor.ReBootingFlag = false;

            //    if (comboBox1.SelectedItem != null)
            //        TInfor.Model = comboBox1.SelectedItem.ToString();
            //    else TInfor.Model = "";
            //    SaveInfor();
            //}

            if (File.Exists(Path) == false)
            {
                CreateDataFile(Path);
            }
            else
            {
                fpSpread2.OpenExcel(Path);
                fpSpread2.ActiveSheet.Protect = false;
                RowCount = 6;
                for (int i = RowCount; i < fpSpread2.ActiveSheet.RowCount; i++)
                {
                    if (fpSpread2.ActiveSheet.Cells[RowCount, 0].Text == "") break;
                    if (fpSpread2.ActiveSheet.Cells[RowCount, 0].Text == null) break;
                    RowCount++;
                }

                int Col = 0;
                for (int i = 0; i < fpSpread2.ActiveSheet.ColumnCount; i++)
                {
                    if (fpSpread2.ActiveSheet.Cells[3, i].Text == "판정")
                        break;
                    else Col++;
                }
                fpSpread2.ActiveSheet.ColumnCount = Col + 1;
            }
            return;
        }

        private void SaveData()
        {
            //if (SaveDataFlag == false) return;

            SaveDataFlag = false;
            string Path = Program.DATA_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMM") + ".xls";

            //if ((TInfor.DataName != "") && (TInfor.DataName != null))
            //{
            //    if (File.Exists(TInfor.DataName))
            //    {
            //        if (TInfor.DataName != Path)
            //        {
            //            TInfor.DataName = Path;
            //            TInfor.Count.Total = 0;
            //            TInfor.Count.OK = 0;
            //            TInfor.Count.NG = 0;
            //            TInfor.ReBootingFlag = false;
            //            TInfor.Model = comboBox1.SelectedItem.ToString();
            //            SaveInfor();
            //        }
            //    }
            //}
            //else
            //{
            //    TInfor.DataName = Path;
            //    TInfor.Count.Total = 0;
            //    TInfor.Count.OK = 0;
            //    TInfor.Count.NG = 0;
            //    TInfor.ReBootingFlag = false;
            //    TInfor.Model = comboBox1.SelectedItem.ToString();
            //    SaveInfor();
            //}

            if (File.Exists(Path) == false) CreateDataFile(Path);

            int Col = 0;

            fpSpread2.SuspendLayout();
            fpSpread2.ActiveSheet.RowCount = RowCount + 1;

            fpSpread2.ActiveSheet.SetRowHeight(RowCount, 21);
            //fpSpread2.ActiveSheet.ColumnCount = 39;
            for (int i = 0; i < fpSpread2.ActiveSheet.ColumnCount; i++)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, i].CellType = new FarPoint.Win.Spread.CellType.TextCellType();
                fpSpread2.ActiveSheet.Cells[RowCount, i].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
                fpSpread2.ActiveSheet.Cells[RowCount, i].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;                                
                fpSpread2.ActiveSheet.Cells[RowCount, i].Border = LineBorderToData;
                fpSpread2.ActiveSheet.Cells[RowCount, i].Text = "";
            }
            //No.
            fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = ((RowCount - 6) + 1).ToString();
            Col++;

            //Time
            fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = DateTime.Now.ToString("yyyy-MM-dd") + " " + DateTime.Now.ToLongTimeString();
            Col++;

            //Barcode
            fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;

            if(IOPort.GetAuto == true)
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = label17.Text;
            else fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = textBox1.Text;
            Col++;

            //Heater Current
            if (TData.HeaterCurrent.Test == true)
            {

                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[2, 3].Text;
                Col++;

                if (TData.HeaterCurrent.Result != RESULT.REJECT)
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.HeaterCurrent.Data);
            }
            else
            {
                Col++;
            }
            Col++;

            //Heater Led
            if (TData.HeaterLamp.Test == true)
            {
                if ((TData.HeaterLamp.Result[0] != RESULT.REJECT) && (TData.HeaterLamp.Result[1] != RESULT.REJECT) && (TData.HeaterLamp.Result[2] != RESULT.REJECT))
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                if ((TData.HeaterLamp.Result[0] != RESULT.REJECT) && (TData.HeaterLamp.Result[1] != RESULT.REJECT) && (TData.HeaterLamp.Result[2] != RESULT.REJECT))
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "OK";
                else fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "NG";
            }
            Col++;

            //Heater NTC
            /*
            if (TData.HeaterNTC.Test == true)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[4, 3].Text;
                Col++;

                if (TData.HeaterNTC.Result != RESULT.REJECT)
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.HeaterNTC.Data);
            }
            else
            {
                Col++;
            }
            Col++;
            */


            //Vent Current
            if (TData.VentCurr.Test == true)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[4, 3].Text;
                Col++;

                if (TData.VentCurr.Result != RESULT.REJECT)
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.VentCurr.Data);
            }
            else
            {
                //fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                //fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                //fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[4, 3].Text;
                Col++;                
            }
            Col++;

            //Vent Led
            if (TData.VentLamp.Test == true)
            {
                if ((TData.VentLamp.Result[0] != RESULT.REJECT) && (TData.VentLamp.Result[1] != RESULT.REJECT) && (TData.VentLamp.Result[2] != RESULT.REJECT))
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                if ((TData.VentLamp.Result[0] != RESULT.REJECT) && (TData.VentLamp.Result[1] != RESULT.REJECT) && (TData.VentLamp.Result[2] != RESULT.REJECT))
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "OK";
                else fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "NG";
            }
            Col++;
            //버클워닝
            if (TData.BuckleWarning.Test == true)
            {
                if (TData.BuckleWarning.Result != RESULT.REJECT)
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                if(TData.BuckleWarning.Result != RESULT.REJECT)
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "OK";
                else fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "NG";
            }
            Col++;

            //리트렉터
            if (TData.Retractor.Test == true)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[7, 3].Text;
                Col++;
                if (TData.Retractor.Result != RESULT.REJECT)
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                if (TData.Retractor.Data < 9999)
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.Retractor.Data);
                else fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "OL";
            }
            else
            {
                Col++;
            }
            Col++;

            //Power
            if (TData.PW_Recline.Test == true)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[8, 3].Text;
                Col++;
                if (TData.PW_Recline.Result != RESULT.REJECT)
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.PW_Recline.Data);
            }
            else
            {
                Col++;
            }
            Col++;

            ////Power Legrest
            //if (TData.PW_Legrest.Test == true)
            //{
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[10, 3].Text;
            //    Col++;
            //    if (TData.PW_Legrest.Result != RESULT.REJECT)
            //    {
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            //    }
            //    else
            //    {
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
            //    }
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.PW_Legrest.Data);
            //}
            //else
            //{
            //    Col++;
            //}
            //Col++;

            ////Power Legrest Ext
            //if (TData.PW_LegrestExt.Test == true)
            //{
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[11, 3].Text;
            //    Col++;
            //    if (TData.PW_LegrestExt.Result != RESULT.REJECT)
            //    {
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            //    }
            //    else
            //    {
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
            //    }
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.PW_LegrestExt.Data);
            //}
            //else
            //{
            //    Col++;
            //}
            //Col++;

            ////Power Relax
            //if (TData.PW_Relax.Test == true)
            //{
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[12, 3].Text;
            //    Col++;
            //    if (TData.PW_Relax.Result != RESULT.REJECT)
            //    {
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            //    }
            //    else
            //    {
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
            //    }
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.PW_Relax.Data);
            //}
            //else
            //{
            //    Col++;
            //}
            //Col++;

            //SBR 무부하
            if (TData.SBR.Test == true)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[12, 3].Text;
                Col++;
                if (TData.SBR.ResultNotLoad != RESULT.REJECT)
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }

                if (TData.SBR.NotLoadData < 9999)
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.SBR.NotLoadData);
                else fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "OL";
            }
            else
            {
                Col++;
            }
            Col++;
            //SBR 15Kg
            if (TData.SBR.Test == true)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[13, 3].Text;
                Col++;
                if (TData.SBR.Result15Kg!= RESULT.REJECT)
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;                    
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;                    
                }
                if (TData.SBR.Load15KData < 9999)
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.SBR.Load15KData);
                else fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "OL";
            }
            else
            {
                Col++;
            }
            Col++;

            //SBR 30Kg
            if (TData.SBR.Test == true)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[14, 3].Text;
                Col++;
                if (TData.SBR.Result30Kg != RESULT.REJECT)
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;                    
                }

                if (TData.SBR.Load30KData < 9999)
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = string.Format("{0:0.00}", TData.SBR.Load30KData);
                else fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "OL";
            }
            else
            {
                Col++;
            }
            Col++;


            if (TData.Result == RESULT.PASS)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            }
            else if (TData.Result == RESULT.REJECT)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
            }
            if (TData.Result == RESULT.PASS)
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "OK";
            else if (TData.Result == RESULT.REJECT)
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "NG";
            RowCount++;
            fpSpread2.ResumeLayout();
            fpSpread2.SaveExcel(Path);
            return;
        }


        private FarPoint.Win.LineBorder LineBorderToHeader = new FarPoint.Win.LineBorder(Color.Black, 1/*RowHeight*/, true, true, true, true);//line color,line style,left,top,right,buttom                       
        private FarPoint.Win.LineBorder LineBorderToData = new FarPoint.Win.LineBorder(Color.Black, 1/*RowHeight*/, true, false, true, true);//line color,line style,left,top,right,buttom                       
        private void CreateDataFile(string dPaht)
        {
            fpSpread2.ActiveSheet.Reset();
            fpSpread2.ActiveSheet.Protect = false;
            fpSpread2.SuspendLayout();
            fpSpread2.ActiveSheet.RowCount = 6;            

            //용지 방향
            fpSpread2.ActiveSheet.PrintInfo.Orientation = FarPoint.Win.Spread.PrintOrientation.Landscape;
            //프린트 할 때 가로,세로 중앙에 프린트 할 수 있도록 설정
            fpSpread2.ActiveSheet.PrintInfo.Centering = FarPoint.Win.Spread.Centering.Horizontal; //좌/우 중앙                        
            //fpSpread2.ActiveSheet.PrintInfo.PrintCenterOnPageV = false; //Top 쪽으로간다. 만약 true로 설정할 경우 상,하 중간에 프린트가 된다.

            //여백
            fpSpread2.ActiveSheet.PrintInfo.Margin.Bottom = 1;
            fpSpread2.ActiveSheet.PrintInfo.Margin.Left = 1;
            fpSpread2.ActiveSheet.PrintInfo.Margin.Right = 1;
            fpSpread2.ActiveSheet.PrintInfo.Margin.Top = 2;
            
            //프린트에서 컬러 표시
            fpSpread2.ActiveSheet.PrintInfo.ShowColor = true;
            //프린트에서 셀 라인 표시여부 (true일경우 내가 그린 라인 말고 셀에 사각 표시 라인도 같이 프린트가 된다.
            fpSpread2.ActiveSheet.PrintInfo.ShowGrid = false;

            fpSpread2.ActiveSheet.VerticalGridLine = new FarPoint.Win.Spread.GridLine(FarPoint.Win.Spread.GridLineType.None);
            fpSpread2.ActiveSheet.HorizontalGridLine = new FarPoint.Win.Spread.GridLine(FarPoint.Win.Spread.GridLineType.None);
            //용지 넓이에 페이지 맞춤
            fpSpread2.ActiveSheet.PrintInfo.UseSmartPrint = true;

            //그리드를 표시할 경우 저장할 때나 프린트 할 때 화면에 같이 표시,그리드가 프린트 되기 때문에 지저분해 보인다.
            fpSpread2.ActiveSheet.PrintInfo.ShowColumnFooter = FarPoint.Win.Spread.PrintHeader.Hide;
            fpSpread2.ActiveSheet.PrintInfo.ShowColumnFooterEachPage = false;


            //헤더와 밖같 라인이 같이 프린트 되지 않도록 한다.
            fpSpread2.ActiveSheet.PrintInfo.ShowBorder = false;
            fpSpread2.ActiveSheet.PrintInfo.ShowColumnHeader = FarPoint.Win.Spread.PrintHeader.Hide;
            fpSpread2.ActiveSheet.PrintInfo.ShowRowHeader = FarPoint.Win.Spread.PrintHeader.Hide;
            fpSpread2.ActiveSheet.PrintInfo.ShowShadows = false;
            fpSpread2.ActiveSheet.PrintInfo.ShowTitle = FarPoint.Win.Spread.PrintTitle.Hide;
            fpSpread2.ActiveSheet.PrintInfo.ShowSubtitle = FarPoint.Win.Spread.PrintTitle.Hide;

            //시트 보호를 해지 한다.
            //fpSpread2.ActiveSheet.PrintInfo.PrintType = FarPoint.Win.Spread.PrintType.All;
            //fpSpread2.ActiveSheet.PrintInfo.SmartPrintRules.Add(new ReadOnlyAttribute(false));
            //axfpSpread1.Protect = false;

            
            //for (int i = 0; i < 24; i++) fpSpread2.ActiveSheet.SetColumnWidth(i, 80);

            //틀 고정
            fpSpread2.ActiveSheet.FrozenColumnCount = 3;
            fpSpread2.ActiveSheet.FrozenRowCount = 6;
            fpSpread2.ActiveSheet.Cells[1, 0].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[1, 0].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Right;
            fpSpread2.ActiveSheet.Cells[1, 0].Text = "날짜 :";
            fpSpread2.ActiveSheet.AddSpanCell(1, 1, 1, 22);            
            
            fpSpread2.ActiveSheet.Cells[1, 1].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();                
            fpSpread2.ActiveSheet.Cells[1, 1].Text = DateTime.Now.ToLongDateString();
            fpSpread2.ActiveSheet.Cells[1, 1].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[1, 1].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Left;

            int Col;

            Col = 0;
            //No
            fpSpread2.ActiveSheet.SetRowHeight(3, 31);
            fpSpread2.ActiveSheet.SetRowHeight(4, 31);
            fpSpread2.ActiveSheet.SetRowHeight(5, 31);
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 3, 1);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "NO.";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;
            Col++;

            //Time
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 3, 1);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 300);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "생산 시간";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;
            Col++;

            //바코드
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 3, 1);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 300);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "바코드";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;
            Col++;

            //희터
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 1, 3);            
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "히터";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;            

            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);            
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 1, 2);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "전류 [A]";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;

            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "스팩";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;


            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "램프";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            Col++;
            
            //통풍(VENT)
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 1, 3);            
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "통풍(VENT)";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;

            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 1, 2);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "전류 [A]";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;

            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "스팩";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "램프";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);            
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 3, 1);            
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "버클워닝";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToData;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;


            Col++;

            //리트렉터
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 2, 2);            
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "리트렉터 [Ω]";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;

            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            //fpSpread2.ActiveSheet.AddSpanCell(4, Col, 1, 2);
            //fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].Text = "센서 [Ω]";
            //fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "스팩";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;

           

            ////sbr
            //fpSpread2.ActiveSheet.AddSpanCell(3, Col, 2, 2);            
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            //fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[3, Col].Text = "SBR";
            //fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;

            //fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].Text = "스팩";
            //fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            //Col++;
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            //fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            //fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            //Col++;

            //POWER
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 2, 2);            
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "POWER SWITCH [A]";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;

            //fpSpread2.ActiveSheet.AddSpanCell(4, Col, 1, 2);
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);            
            //fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].Text = "RECLINER [A]";
            //fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;

            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "스팩";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;

            //fpSpread2.ActiveSheet.AddSpanCell(4, Col, 1, 2);
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 80);            
            //fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].Text = "LEGREST [A]";
            //fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;

            //fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].Text = "스팩";
            //fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            //Col++;
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            //fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            //fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            //Col++;

            //fpSpread2.ActiveSheet.AddSpanCell(4, Col, 1, 2);
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);            
            //fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].Text = "LEGREST EXT [A]";
            //fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;

            //fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].Text = "스팩";
            //fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            //Col++;
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            //fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            //fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            //Col++;

            //fpSpread2.ActiveSheet.AddSpanCell(4, Col, 1, 2);
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            //fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].Text = "RELAX [A]";
            //fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;

            //fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].Text = "스팩";
            //fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            //Col++;
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            //fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            //fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            //Col++;

            //SBR
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 1, 6);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "SBR";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;
            
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 1, 2);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "무부하 [Ω]";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;

            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "SPEC";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;

            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 1, 2);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "15 Kg [Ω]";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;

            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "스팩";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;            
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;

            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 1, 2);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "30 Kg [Ω]";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;

            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "스팩";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[5, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[5, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[5, Col].Text = "데이타";
            fpSpread2.ActiveSheet.Cells[5, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[5, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[5, Col].Border = LineBorderToHeader;
            Col++;

            ////CAN
            //fpSpread2.ActiveSheet.AddSpanCell(3, Col, 1, 2);
            //fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[3, Col].Text = "CAN PIN OPEN CHECK";
            //fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;

            //fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            //fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].Text = "HIGH";
            //fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            //Col++;

            //fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            //fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].Text = "LOW";
            //fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            //Col++;

            //판정
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 3, 1);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "판정";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;
            Col++;
            fpSpread2.ActiveSheet.ColumnCount = Col;

            //Header
            fpSpread2.ActiveSheet.AddSpanCell(0, 0, 1, Col);
            fpSpread2.ActiveSheet.SetRowHeight(0, 100);
            fpSpread2.ActiveSheet.Cells[0, 0].Font = new Font("맑은 고딕", 26);
            fpSpread2.ActiveSheet.SetText(0, 0, "레포트");
            fpSpread2.ActiveSheet.Cells[0, 0].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[0, 0].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ResumeLayout();
            RowCount = 6;
            fpSpread2.SaveExcel(dPaht);
            return;
        }

        private void SaveInfor()
        {
            string Path = Program.INFOR_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".inf";

            
            TIniFile Ini = new TIniFile(Path);

            Ini.WriteInteger("COUNT", "TOTAL", TInfor.Count.Total);
            Ini.WriteInteger("COUNT", "OK", TInfor.Count.OK);
            Ini.WriteInteger("COUNT", "NG", TInfor.Count.NG);

            Ini.WriteString("NAME", "DATA", TInfor.DataName);
            Ini.WriteString("NAME", "MODEL", TInfor.Model);
            Ini.WriteString("DATE", "VALUE", TInfor.Date);
            Ini.WriteBool("OPTION", "VALUE", TInfor.ReBootingFlag);
            return;
        }

        private void OpenInfor()
        {
            string Path = Program.INFOR_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".inf";
            string dPath = Program.DATA_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMM") + ".xls";

            if (File.Exists(Path) == false)
            {
                TInfor.Date = DateTime.Now.ToString("yyyyMMdd");
                TInfor.DataName = dPath;
                TInfor.Count.Total = 0;
                TInfor.Count.OK = 0;
                TInfor.Count.NG = 0;
                TInfor.ReBootingFlag = false;
                if (comboBox1.SelectedItem != null)
                    TInfor.Model = comboBox1.SelectedItem.ToString();
                else TInfor.Model = null;
                SaveInfor();
            }
            else
            {
                TIniFile Ini = new TIniFile(Path);

                if (Ini.ReadInteger("COUNT", "TOTAL", ref TInfor.Count.Total) == false) TInfor.Count.Total = 0;
                if (Ini.ReadInteger("COUNT", "OK", ref TInfor.Count.OK) == false) TInfor.Count.OK = 0;
                if (Ini.ReadInteger("COUNT", "NG", ref TInfor.Count.NG) == false) TInfor.Count.NG = 0;

                if (Ini.ReadString("NAME", "DATA", ref TInfor.DataName) == false) TInfor.DataName = dPath;
                if (Ini.ReadString("NAME", "MODEL", ref TInfor.Model) == false) TInfor.Model = "";
                if (Ini.ReadString("DATE", "VALUE", ref TInfor.Date) == false) TInfor.Date = DateTime.Now.ToString("yyyyMMdd");
                if (Ini.ReadBool("OPTION", "VALUE", ref TInfor.ReBootingFlag) == false) TInfor.ReBootingFlag = true;
            }

            if (File.Exists(dPath) == false)
            {   
                CreateFileName();
            }
            else
            {
                fpSpread2.OpenExcel(dPath);
                fpSpread2.ActiveSheet.Protect = false;
                RowCount = 6;
                for (int i = RowCount; i < fpSpread2.ActiveSheet.RowCount; i++)
                {
                    if (fpSpread2.ActiveSheet.Cells[RowCount, 0].Text == "") break;
                    if (fpSpread2.ActiveSheet.Cells[RowCount, 0].Text == null) break;
                    RowCount++;
                }

                int Col = 0;
                for (int i = 0; i < fpSpread2.ActiveSheet.ColumnCount; i++)
                {
                    if (fpSpread2.ActiveSheet.Cells[3, i].Text == "판정")
                        break;
                    else Col++;
                }
                fpSpread2.ActiveSheet.ColumnCount = Col + 1;
            }
        }
        
        
        private Color HeaterBackColorRead(int Row, int Col)
        {            
            Color bCol = fpSpread1.ActiveSheet.Cells[Row, Col].BackColor;
            return bCol;
        }

        private Color VentBackColorRead(int Row, int Col)
        {
            Color bCol = fpSpread1.ActiveSheet.Cells[Row, Col].BackColor;
            return bCol;
        }
               

        private bool HeaterCurrentStep(int xStep)
        {
            //if (comboBox5.SelectedIndex == 1)
            //{
            //    if ((TSpec.HeaterCheckPos2.Pos == 2) && (xStep == 0)) return true;
            //    //else if ((TSpec.HeaterCheckPos2.Pos == 1) && (xStep == 1)) return true;
            //    else if ((TSpec.HeaterCheckPos2.Pos == 1) && (xStep == 2)) return true;
            //    else return false;
            //}
            //else
            //{
            if ((TSpec.HeaterCheckPos.Pos == 3) && (xStep == 0)) return true;
            else if ((TSpec.HeaterCheckPos.Pos == 2) && (xStep == 1)) return true;
            else if ((TSpec.HeaterCheckPos.Pos == 1) && (xStep == 2)) return true;
            else return false;
            //}
        }

        private bool VentCurrentStep(int xStep)
        {
            if ((TSpec.VentCheckPos.Pos == 3) && (xStep == 0)) return true;
            else if ((TSpec.VentCheckPos.Pos == 2) && (xStep == 1)) return true;
            else if ((TSpec.VentCheckPos.Pos == 1) && (xStep == 2)) return true;
            else return false;
        }
                

        private void DisplayAdData1()
        {
            if (sevenSegmentAnalog1.Value.AsDouble != pMeter.GetBatt) sevenSegmentAnalog1.Value.AsDouble = pMeter.GetBatt;
            return;
        }
        private void DisplayAdData2()
        {
            if (sevenSegmentAnalog2.Value.AsDouble != pMeter.GetBuckle) sevenSegmentAnalog2.Value.AsDouble = pMeter.GetBuckle;
            return;
        }
        private void DisplayAdData3()
        {
            if (sevenSegmentAnalog3.Value.AsDouble != pMeter.GetCurr) sevenSegmentAnalog3.Value.AsDouble = pMeter.GetCurr;
            return;
        }
        private void DisplayAdData4()
        {
            if (MultiMeter.GetData < 9999)
            {
                if(label27.Visible == true) label27.Visible = false;
                if (sevenSegmentAnalog4.Value.AsDouble != MultiMeter.GetData) sevenSegmentAnalog4.Value.AsDouble = MultiMeter.GetData;
            }
            else
            {
                if(label27.Visible == false) label27.Visible = true;
            }
                        
            return;
        }
        

        private void LoadCarModel()
        {
            /*
            ComF.ReadFileListNotExt(Program.SPEC_PATH.ToString(), "*.spc");

            comboBox1.Items.Clear();
            foreach (string s in ComF.GetFileList)
            {
                comboBox1.Items.Add(s);
            }
            */
            string fPath = Program.SYSTEM_PATH.ToString();
            string s;

            fPath = fPath + "\\CarList.ini";

            comboBox1.Items.Clear();
            if (File.Exists(fPath) == false) return;

            FileStream fp = new FileStream(fPath, FileMode.Open);
            StreamReader read = new StreamReader(fp);
            
            while (read.EndOfStream == false)
            {
                s = read.ReadLine();
                if (s == "KA4 2열 78P") s = "KA4 2열 7/8P";
                comboBox1.Items.Add(s);
            }
            read.Close();
            return;
        }

        private void ScreenInit()
        {
            for (int i = 2; i < fpSpread1.ActiveSheet.RowCount; i++)
            {
                fpSpread1.ActiveSheet.Cells[i, 6].Text = "";
                fpSpread1.ActiveSheet.Cells[i, 7].Text = "";
                fpSpread1.ActiveSheet.Cells[i, 6].BackColor = Color.Black;
                fpSpread1.ActiveSheet.Cells[i, 7].BackColor = Color.Black;
                fpSpread1.ActiveSheet.Cells[i, 6].ForeColor = Color.White;
                fpSpread1.ActiveSheet.Cells[i, 7].ForeColor = Color.White;
            }

            TData.Result = RESULT.PASS;
            TData.Retractor.Test = false;
            TData.Retractor.Result = RESULT.CLEAR;
            TData.BuckleWarning.Test = false;
            TData.BuckleWarning.Result = RESULT.CLEAR;
            TData.HeaterCurrent.Test = false;
            TData.HeaterCurrent.Result = RESULT.CLEAR;
            TData.HeaterLamp.Test = false;
            TData.HeaterLamp.Result[0] = RESULT.CLEAR;
            TData.HeaterLamp.Test = false;
            TData.HeaterLamp.Result[1] = RESULT.CLEAR;
            TData.HeaterLamp.Test = false; 
            TData.HeaterLamp.Result[2] = RESULT.CLEAR;

            TData.SBR.Test = false;
            TData.SBR.Result = RESULT.CLEAR;

            TData.PW_Recline.Test = false;
            TData.PW_Recline.Result = RESULT.CLEAR;
            TData.PW_Legrest.Test = false;
            TData.PW_Legrest.Result = RESULT.CLEAR;
            TData.PW_LegrestExt.Test = false;
            TData.PW_LegrestExt.Result = RESULT.CLEAR;
            TData.PW_Relax.Test = false; 
            TData.PW_Relax.Result = RESULT.CLEAR;
            TData.SBR.Test = false;
            TData.SBR.Result = RESULT.CLEAR;
            TData.VentCurr.Test = false;
            TData.VentCurr.Result = RESULT.CLEAR;
            TData.VentLamp.Test = false;
            TData.VentLamp.Result[0] = RESULT.CLEAR;
            TData.VentLamp.Test = false;
            TData.VentLamp.Result[1] = RESULT.CLEAR;
            TData.VentLamp.Test = false;
            TData.VentLamp.Result[2] = RESULT.CLEAR;


            TData.Retractor.Data = 0;
            TData.Retractor.Result = RESULT.CLEAR;
            TData.Retractor.Test = false;

            //TData.BlowerSpeed.Data = 0;
            //TData.BlowerSpeed.Result = RESULT.CLEAR;
            //TData.BlowerSpeed.Test = false;

            //TData.BuckleSensor.Data = 0;
            //TData.BuckleSensor.Result = RESULT.CLEAR;
            //TData.BuckleSensor.Test = false;

            TData.BuckleWarning.Data = 0;
            TData.BuckleWarning.Result = RESULT.CLEAR;
            TData.BuckleWarning.Test = false;

            //TData.CanHi.Data = 0;
            //TData.CanHi.Result = RESULT.CLEAR;
            //TData.CanHi.Test = false;

            //TData.CanLo.Data = 0;
            //TData.CanLo.Result = RESULT.CLEAR;
            //TData.CanLo.Test = false;

            TData.HeaterCurrent.Data = 0;
            TData.HeaterCurrent.Result = RESULT.CLEAR;
            TData.HeaterCurrent.Test = false;

            TData.HeaterLamp.Data = 0;
            TData.HeaterLamp.Result[0] = RESULT.CLEAR;
            TData.HeaterLamp.Result[1] = RESULT.CLEAR;
            TData.HeaterLamp.Result[1] = RESULT.CLEAR;
            TData.HeaterLamp.Test = false;

            //TData.HeaterNTC.Data = 0;
            //TData.HeaterNTC.Result = RESULT.CLEAR;
            //TData.HeaterNTC.Test = false;

            //TData.IMS_M1.Data = 0;
            //TData.IMS_M1.Result = RESULT.CLEAR;
            //TData.IMS_M1.Test = false;

            //TData.IMS_M2.Data = 0;
            //TData.IMS_M2.Result = RESULT.CLEAR;
            //TData.IMS_M2.Test = false;

            //TData.IMS_M3.Data = 0;
            //TData.IMS_M3.Result = RESULT.CLEAR;
            //TData.IMS_M3.Test = false;

            //TData.IMS_SET.Data = 0;
            //TData.IMS_SET.Result = RESULT.CLEAR;
            //TData.IMS_SET.Test = false;

            //TData.LHFan.Data = 0;
            //TData.LHFan.Result = RESULT.CLEAR;
            //TData.LHFan.Test = false;

            //TData.LHHeater.Data = 0;
            //TData.LHHeater.Result = RESULT.CLEAR;
            //TData.LHHeater.Test = false;

            TData.SBR.NotLoadData = 0;
            TData.SBR.ResultNotLoad = RESULT.CLEAR;
            TData.SBR.Test = false;

            TData.SBR.Load15KData = 0;
            TData.SBR.Result15Kg = RESULT.CLEAR;

            TData.SBR.Load30KData = 0;
            TData.SBR.Result30Kg = RESULT.CLEAR;

            TData.PW_Legrest.Data = 0;
            TData.PW_Legrest.Result = RESULT.CLEAR;
            TData.PW_Legrest.Test = false;

            TData.PW_Recline.Data = 0;
            TData.PW_Recline.Result = RESULT.CLEAR;
            TData.PW_Recline.Test = false;

            TData.PW_LegrestExt.Data = 0;
            TData.PW_LegrestExt.Result = RESULT.CLEAR;
            TData.PW_LegrestExt.Test = false;

            TData.PW_Relax.Data = 0;
            TData.PW_Relax.Result = RESULT.CLEAR;
            TData.PW_Relax.Test = false;

            TData.Result = RESULT.CLEAR;

            //TData.SBR.Data = 0;
            //TData.SBR.Result = RESULT.CLEAR;
            //TData.SBR.Test = false;

            TData.VentCurr.Data = 0;
            TData.VentCurr.Result = RESULT.CLEAR;
            TData.VentCurr.Test = false;

            TData.VentLamp.Data = 0;
            TData.VentLamp.Result[0] = RESULT.CLEAR;
            TData.VentLamp.Result[1] = RESULT.CLEAR;
            TData.VentLamp.Result[2] = RESULT.CLEAR;
            TData.VentLamp.Test = false;

            label16.Text = "";
            label5.Text = "";
            return;
        }

        //private void UpdWrite2()
        //{
        //    byte[] Data1 = { OutData[1, 0], OutData[1, 1], OutData[1, 2], OutData[1, 3], OutData[1, 4], OutData[1, 5], OutData[1, 6], OutData[1, 7] };
        //    byte[] Data2 = { OutData[0, 0], OutData[0, 1], OutData[0, 2], OutData[0, 3], OutData[0, 4], OutData[0, 5], OutData[0, 6], OutData[0, 7] };
        //    UdpWrite(0x151, Data1, 8);
        //    UdpWrite(0x131, Data2, 8);
        //    OutPos = 2;
        //    OutPosFirst = ComF.timeGetTimems();
        //    OutPosLast = ComF.timeGetTimems();
        //    return;
        //}

        //private void SWOnBuzzer()
        //{
        //    IOPort.outportb(IO_OUT.BUZZER,true);
        //    ComF.timedelay(100);
        //    IOPort.outportb(IO_OUT.BUZZER, false);
        //    ComF.timedelay(100);
        //    IOPort.outportb(IO_OUT.BUZZER, true);
        //    ComF.timedelay(100);
        //    IOPort.outportb(IO_OUT.BUZZER, false);
        //    return;
        //}
        //private void SWCheckBuzzer()
        //{
        //    IOPort.outportb(IO_OUT.BUZZER, true);
        //    ComF.timedelay(100);
        //    IOPort.outportb(IO_OUT.BUZZER, false);
        //    return;
        //}

        //private void UdpCanCommunicationInit()
        //{
        //    byte[] sBuffer = { 0xfc, 0x03, 0xff };

        //    if (remoteEP2 != null)
        //    {
        //        server.SendTo(sBuffer, 3, SocketFlags.DontRoute, remoteEP2);
        //        server.SendTo(sBuffer, 3, SocketFlags.DontRoute, remoteEP2);
        //        server.SendTo(sBuffer, 3, SocketFlags.DontRoute, remoteEP2);
        //    }
        //    return;
        //}

        //private bool ImsSWOutFlag = false;
        private void CheckImsSW(int xStep)
        {
            //if (SpecOutputFlag == false)
            //{
            //    SWCheckFirst = ComF.timeGetTimems();
            //    SWCheckLast = ComF.timeGetTimems();
                                
            //    switch (xStep)
            //    {
            //        case 0: if (label5.Text != "SET 버튼을 검사 중 입니다.") label5.Text = "SET 버튼을 검사 중 입니다."; break;
            //        case 1: if (label5.Text != "M1 버튼을 검사 중 입니다.") label5.Text = "M1 버튼을 검사 중 입니다."; break;
            //        case 2: if (label5.Text != "M2 버튼을 검사 중 입니다.") label5.Text = "M2 버튼을 검사 중 입니다."; break;
            //        case 3: if (label5.Text != "M3 버튼을 검사 중 입니다.") label5.Text = "M3 버튼을 검사 중 입니다."; break;
            //    }
            //    ImsSWOutFlag = false;
            //    SpecOutputFlag = true;
            //    SWCheckFirst = ComF.timeGetTimems();
            //    SWCheckLast = ComF.timeGetTimems();
                
            //}
            //else
            //{
            //    if (ImsSWOutFlag == false)
            //    {
            //        if (xStep == 0)
            //        {                  
            //            //Set 버튼 인식이 안됨... ?????     
            //            IOPort.outportb(IO_OUT.IMS_M1, true);
            //            IOPort.outportb(IO_OUT.IMS_M2, false);
            //            IOPort.outportb(IO_OUT.IMS_M3, false);
            //            IOPort.outportb(IO_OUT.IMS_SET, false);
            //        }
            //        if (xStep == 1)
            //        {
            //            IOPort.outportb(IO_OUT.IMS_SET, false);                        
            //            IOPort.outportb(IO_OUT.IMS_M2, false);
            //            IOPort.outportb(IO_OUT.IMS_M3, false);
            //            IOPort.outportb(IO_OUT.IMS_M1, true);
            //        }
            //        if (xStep == 2)
            //        {
            //            IOPort.outportb(IO_OUT.IMS_SET, false);
            //            IOPort.outportb(IO_OUT.IMS_M1, false);                        
            //            IOPort.outportb(IO_OUT.IMS_M3, false);
            //            IOPort.outportb(IO_OUT.IMS_M2, true);
            //        }
            //        if (xStep == 3)
            //        {
            //            IOPort.outportb(IO_OUT.IMS_SET, false);
            //            IOPort.outportb(IO_OUT.IMS_M1, false);
            //            IOPort.outportb(IO_OUT.IMS_M2, false);
            //            IOPort.outportb(IO_OUT.IMS_M3, true);
            //        }
            //    }
            //    ImsSWOutFlag = true;
            //    SWCheckLast = ComF.timeGetTimems();
            //    if (2000 <= (SWCheckLast - SWCheckFirst))
            //    {
            //        fpSpread1.ActiveSheet.Cells[20 + xStep, 6].Text = "OFF";
            //        fpSpread1.ActiveSheet.Cells[20 + xStep, 7].Text = "NG";
            //        fpSpread1.ActiveSheet.Cells[20 + xStep, 7].BackColor = Color.Red;
            //        fpSpread1.ActiveSheet.Cells[20 + xStep, 7].ForeColor = Color.White;
            //        Step++;
            //        SpecOutputFlag = false;
            //        SWCheckFirst = ComF.timeGetTimems();
            //        SWCheckLast = ComF.timeGetTimems();
            //        switch (xStep)
            //        {
            //            case 0:
            //                TData.IMS_SET.Test = true;
            //                TData.IMS_SET.Result = RESULT.REJECT;
            //                break;
            //            case 1:
            //                TData.IMS_M1.Test = true;
            //                TData.IMS_M1.Result = RESULT.REJECT;
            //                break;
            //            case 2:
            //                TData.IMS_M2.Test = true;
            //                TData.IMS_M2.Result = RESULT.REJECT;
            //                break;
            //            case 3:
            //                TData.IMS_M3.Test = true;
            //                TData.IMS_M3.Result = RESULT.REJECT;
            //                break;
            //        }
            //        IOPort.outportb(IO_OUT.IMS_SET, false);
            //        IOPort.outportb(IO_OUT.IMS_M1, false);
            //        IOPort.outportb(IO_OUT.IMS_M2, false);
            //        IOPort.outportb(IO_OUT.IMS_M3, false);
            //    }
            //    else
            //    {
            //        RESULT Res = CheckCan(xStep);

            //        switch (Res)
            //        {
            //            case RESULT.REJECT:
            //                if (xStep < 3)
            //                {
            //                    fpSpread1.ActiveSheet.Cells[20 + xStep, 6].Text = "OFF";
            //                    fpSpread1.ActiveSheet.Cells[20 + xStep, 7].Text = "NG";
            //                    fpSpread1.ActiveSheet.Cells[20 + xStep, 7].BackColor = Color.Red;
            //                    fpSpread1.ActiveSheet.Cells[20 + xStep, 7].ForeColor = Color.White;
            //                }
            //                switch (xStep)
            //                {
            //                    case 0:
            //                        TData.IMS_SET.Test = true;
            //                        TData.IMS_SET.Result = RESULT.REJECT;
            //                        break;
            //                    case 1:
            //                        TData.IMS_M1.Test = true;
            //                        TData.IMS_M1.Result = RESULT.REJECT;
            //                        break;
            //                    case 2:
            //                        TData.IMS_M2.Test = true;
            //                        TData.IMS_M2.Result = RESULT.REJECT;
            //                        break;
            //                    case 3:
            //                        TData.IMS_M3.Test = false;
            //                        TData.IMS_M3.Result = RESULT.PASS;
            //                        fpSpread1.ActiveSheet.Cells[20 + xStep, 6].Text = "";
            //                        fpSpread1.ActiveSheet.Cells[20 + xStep, 7].Text = "";
            //                        //fpSpread1.ActiveSheet.Cells[20 + xStep, 7].BackColor = Color.Lime;
            //                        //fpSpread1.ActiveSheet.Cells[20 + xStep, 7].ForeColor = Color.Black;                                    

            //                        break;
            //                }
            //                IOPort.outportb(IO_OUT.IMS_SET, false);
            //                IOPort.outportb(IO_OUT.IMS_M1, false);
            //                IOPort.outportb(IO_OUT.IMS_M2, false);
            //                IOPort.outportb(IO_OUT.IMS_M3, false);
            //                ComF.timedelay(500);
            //                Step++;
            //                SpecOutputFlag = false;
            //                SWCheckFirst = ComF.timeGetTimems();
            //                SWCheckLast = ComF.timeGetTimems();
            //                break;
            //            case RESULT.PASS:
            //                if (xStep < 3)
            //                {
            //                    fpSpread1.ActiveSheet.Cells[20 + xStep, 6].Text = "ON";
            //                    fpSpread1.ActiveSheet.Cells[20 + xStep, 7].Text = "OK";
            //                    fpSpread1.ActiveSheet.Cells[20 + xStep, 7].BackColor = Color.Lime;
            //                    fpSpread1.ActiveSheet.Cells[20 + xStep, 7].ForeColor = Color.Black;
            //                }
            //                switch (xStep)
            //                {
            //                    case 0:
            //                        TData.IMS_SET.Test = true;
            //                        TData.IMS_SET.Result = RESULT.PASS;
            //                        break;
            //                    case 1:
            //                        TData.IMS_M1.Test = true;
            //                        TData.IMS_M1.Result = RESULT.PASS;
            //                        break;
            //                    case 2:
            //                        TData.IMS_M2.Test = true;
            //                        TData.IMS_M2.Result = RESULT.PASS;
            //                        break;
            //                    case 3:
            //                        TData.IMS_M3.Test = false;
            //                        TData.IMS_M3.Result = RESULT.PASS;
            //                        fpSpread1.ActiveSheet.Cells[20 + xStep, 6].Text = "";
            //                        fpSpread1.ActiveSheet.Cells[20 + xStep, 7].Text = "";
            //                        //fpSpread1.ActiveSheet.Cells[20 + xStep, 7].BackColor = Color.Lime;
            //                        //fpSpread1.ActiveSheet.Cells[20 + xStep, 7].ForeColor = Color.Black;                                    
            //                        break;
            //                }
            //                IOPort.outportb(IO_OUT.IMS_SET, false);
            //                IOPort.outportb(IO_OUT.IMS_M1, false);
            //                IOPort.outportb(IO_OUT.IMS_M2, false);
            //                IOPort.outportb(IO_OUT.IMS_M3, false);
            //                ComF.timedelay(500);
            //                Step++;
            //                SpecOutputFlag = false;
            //                SWCheckFirst = ComF.timeGetTimems();
            //                SWCheckLast = ComF.timeGetTimems();
            //                break;
            //            default: break;
            //        }
            //    }
            //}
            return;
        }

        //private RESULT CheckCan(int xStep)
        //{
        //    RESULT xResult = RESULT.CLEAR;

        //    for (int i = 0; i < 5; i++)
        //    {
        //        if (ReadMsg[i].ID == 0x40)
        //        {
        //            //this.Text = string.Format("{0:X}", ReadMsg[i].DATA[1]);
        //            switch (xStep)
        //            {
        //                case 0: //Set                            
        //                    //if ((ReadMsg[i].DATA[1] & 0x80) == 0x80)
        //                    if (ReadMsg[i].DATA[0] != 0x00)
        //                        xResult = RESULT.PASS;
        //                    else  xResult = RESULT.CLEAR;                            
        //                    break;
        //                case 1: //M1
        //                    //if (((ReadMsg[i].DATA[1] & 0x08) == 0x08) || ((ReadMsg[i].DATA[1] & 0x20) == 0x20))
        //                    if (ReadMsg[i].DATA[1] != 0x00)
        //                        xResult = RESULT.PASS;                            
        //                    else xResult = RESULT.CLEAR;
        //                    break;
        //                case 2: //M2
        //                    //if (((ReadMsg[i].DATA[1] & 0x10) == 0x10) || ((ReadMsg[i].DATA[1] & 0x40) == 0x40))
        //                    if (ReadMsg[i].DATA[1] != 0x00)
        //                        xResult = RESULT.PASS;                            
        //                    else xResult = RESULT.CLEAR;
        //                    break;
        //                case 3: //M3
        //                    xResult = RESULT.PASS;
        //                    break;
        //            }
        //            break;
        //        }
        //    }

        //    return xResult;
        //}

        private void fpSpread1_CellClick(object sender, FarPoint.Win.Spread.CellClickEventArgs e)
        {
            //int Row = e.Row;

            //if ((e.Column == 0) || (e.Column == 1) || (e.Column == 2))
            //{               
            //    if (Row == 6)
            //    {
            //        if (fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor == SelectOnColor)
            //        {

            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].ForeColor = Color.Silver;
            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor = SelectOffColor;
            //            CheckItem.BuckleWarning = false;
            //            CheckItem.SWBuckleWar = false;

            //            for (int i = 1; i < 7; i++)
            //            {
            //                if (i <= 2) fpSpread1.ActiveSheet.Cells[7, i].BackColor = SelectOffColor;

            //                fpSpread1.ActiveSheet.Cells[7, i].ForeColor = Color.Silver;
            //            }
            //        }
            //        else
            //        {
            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].ForeColor = Color.Black;
            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor = SelectOnColor;
            //            CheckItem.BuckleWarning = true;
            //            CheckItem.SWBuckleWar = true;

            //            for (int i = 1; i < 7; i++)
            //            {
            //                if (i <= 2) fpSpread1.ActiveSheet.Cells[7, i].BackColor = SelectOnColor;

            //                fpSpread1.ActiveSheet.Cells[7, i].ForeColor = Color.Black;
            //            }
            //        }
            //    }

            //    if (Row == 7)
            //    {
            //        if (fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor == SelectOnColor)
            //        {

            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].ForeColor = Color.Silver;
            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor = SelectOffColor;
            //            CheckItem.Retractor = false;
            //            CheckItem.SWRetractor = false;

            //            for (int i = 1; i < 7; i++)
            //            {
            //                if (i <= 2) fpSpread1.ActiveSheet.Cells[8, i].BackColor = SelectOffColor;

            //                fpSpread1.ActiveSheet.Cells[8, i].ForeColor = Color.Silver;
            //            }
            //        }
            //        else
            //        {
            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].ForeColor = Color.Black;
            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor = SelectOnColor;
            //            CheckItem.SWRetractor = true;
            //            CheckItem.Retractor = true;

            //            for (int i = 1; i < 7; i++)
            //            {
            //                if (i <= 2) fpSpread1.ActiveSheet.Cells[8, i].BackColor = SelectOnColor;

            //                fpSpread1.ActiveSheet.Cells[8, i].ForeColor = Color.Black;
            //            }
            //        }
            //    }
            //}
            //if ((8 <= Row) && (Row <= 11))
            //{
            //    if ((e.Column == 0) || (e.Column == 1) || (e.Column == 2))
            //    {
            //        if (fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor == SelectOnColor)
            //        {

            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].ForeColor = Color.Silver;
            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor = SelectOffColor;
            //            CheckItem.PowerSW = false;
                        
            //            for (int i = 1; i < 7; i++)
            //            {
            //                if (i <= 2)
            //                {
            //                    fpSpread1.ActiveSheet.Cells[9, i].BackColor = SelectOffColor;
            //                    fpSpread1.ActiveSheet.Cells[10, i].BackColor = SelectOffColor;
            //                    fpSpread1.ActiveSheet.Cells[11, i].BackColor = SelectOffColor;
            //                    fpSpread1.ActiveSheet.Cells[12, i].BackColor = SelectOffColor;
            //                }

            //                fpSpread1.ActiveSheet.Cells[9, i].ForeColor = Color.Silver;
            //                fpSpread1.ActiveSheet.Cells[10, i].ForeColor = Color.Silver;
            //                fpSpread1.ActiveSheet.Cells[11, i].ForeColor = Color.Silver;
            //                fpSpread1.ActiveSheet.Cells[12, i].ForeColor = Color.Silver;
            //            }
            //        }
            //        else
            //        {
            //            CheckItem.PowerSW = true;
            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].ForeColor = Color.Black;
            //            fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor = SelectOnColor;
            //            for (int i = 1; i < 7; i++)
            //            {
            //                if (i <= 2)
            //                {
            //                    fpSpread1.ActiveSheet.Cells[9, i].BackColor = SelectOnColor;
            //                    fpSpread1.ActiveSheet.Cells[10, i].BackColor = SelectOnColor;
            //                    fpSpread1.ActiveSheet.Cells[11, i].BackColor = SelectOnColor;
            //                    fpSpread1.ActiveSheet.Cells[12, i].BackColor = SelectOnColor;
            //                }

            //                fpSpread1.ActiveSheet.Cells[9, i].ForeColor = Color.Black;
            //                fpSpread1.ActiveSheet.Cells[10, i].ForeColor = Color.Black;
            //                fpSpread1.ActiveSheet.Cells[11, i].ForeColor = Color.Black;
            //                fpSpread1.ActiveSheet.Cells[12, i].ForeColor = Color.Black;
            //            }
            //        }
            //    }
            //}

            //if ((12 <= Row) && (Row <= 14))
            //{
            //    if (fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor == SelectOnColor)
            //    {

            //        fpSpread1.ActiveSheet.Cells[Row, e.Column].ForeColor = Color.Silver;
            //        fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor = SelectOffColor;
            //        CheckItem.SBR = false;
            //        CheckItem.SWSBR = false;

            //        for (int i = 1; i < 7; i++)
            //        {
            //            if (i <= 2)
            //            {
            //                fpSpread1.ActiveSheet.Cells[13, i].BackColor = SelectOffColor;
            //                fpSpread1.ActiveSheet.Cells[14, i].BackColor = SelectOffColor;
            //            }

            //            fpSpread1.ActiveSheet.Cells[13, i].ForeColor = Color.Silver;
            //            fpSpread1.ActiveSheet.Cells[14, i].ForeColor = Color.Silver;
            //        }
            //    }
            //    else
            //    {
            //        CheckItem.SBR = true;
            //        CheckItem.SWSBR = true;
            //        fpSpread1.ActiveSheet.Cells[Row, e.Column].ForeColor = Color.Black;
            //        fpSpread1.ActiveSheet.Cells[Row, e.Column].BackColor = SelectOnColor;
            //        for (int i = 1; i < 7; i++)
            //        {
            //            if (i <= 2)
            //            {
            //                fpSpread1.ActiveSheet.Cells[13, i].BackColor = SelectOnColor;
            //                fpSpread1.ActiveSheet.Cells[14, i].BackColor = SelectOnColor;
            //            }

            //            fpSpread1.ActiveSheet.Cells[13, i].ForeColor = Color.Black;
            //            fpSpread1.ActiveSheet.Cells[14, i].ForeColor = Color.Black;
            //        }
            //    }
            //}
            return;           
        }

        private void imageButton1_Click(object sender, EventArgs e)
        {
            //스팩설정
            SelectMenu = 1;
            MenuClick();
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            //로그인
            SelectMenu = 2;
            MenuClick();
            return;
        }

        private void imageButton4_Click(object sender, EventArgs e)
        {
            //옵션
            SelectMenu = 4;
            MenuClick();
            return;
        }

        private void imageButton3_Click(object sender, EventArgs e)
        {
            //점검
            SelectMenu = 3;
            MenuClick();
            return;
        }

        private void imageButton6_Click(object sender, EventArgs e)
        {
            //데이타
            SelectMenu = 6;
            MenuClick();
            return;
        }

        private void imageButton5_Click(object sender, EventArgs e)
        {
            SelectMenu = 5;
            MenuClick();
            return;
        }

        private void imageButton7_Click(object sender, EventArgs e)
        {
            SelectMenu = 0;
            MenuClick();
            return;
        }

        //public double[] GetADData
        //{
        //    get { return ADData; }
        //}

        public COMMON_FUCTION 공용함수
        {
            get { return ComF; }
        }
        
        public __CanControl GetCan
        {
            get { return CanCtrl; }
        }
        public LinControl GetLin
        {
            get { return LinCtrl; }
        }
        public IOControl GetIOPort { get { return IOPort; } }
        public CanMap GetCanReWrite { get { return CanReWrite; } }

        private short[] LinChannel = { 0, 0 };
        //private short CanChannel = 0;

        public short GetLinChannel(short Ch)
        {
            return LinChannel[Ch];
        }

        public short LinPosition(short Ch, short Pos)
        {
#if PROGRAM_RUNNING
            short ID = 0;

            string[] Device = LinCtrl.GetDevice;

            for (short i = 0; i < Device.Length; i++)
            {
                string s = Device[i];
                string s1 = Pos.ToString() + " - ID";

                if (0 <= s.IndexOf(s1))
                {
                    ID = i;
                    break;
                }
            }

            LinChannel[Ch] = ID;
            return ID;
#else
            return 0;
#endif
        }

        public short CanPosition(short Ch)
        {
#if PROGRAM_RUNNING
            //short ID = 0;

            string[] Device = CanCtrl.GetDevice;

            //for (short i = 0; i < Device.Length; i++)
            //{
            //    string s = Device[i];
            //    string s1 = "0x" + Pos.ToString("X2");

            //    if (0 <= s.IndexOf(s1))
            //    {
            //        ID = i;
            //        break;
            //    }
            //}
            if (Ch == 0)
            {
                short Pos = -1;
                string s1 = "Device=" + Config.Can1.Device.ToString();
                string s2 = "Channel=" + Config.Can1.Channel.ToString() + "h";

                foreach (string s in Device)
                {

                    if (0 <= s.IndexOf(s1))
                    {
                        if (0 <= s.IndexOf(s2))
                        {
                            string ss = s.Substring(s.IndexOf("ID=") + "ID=".Length);
                            string[] ss1 = ss.Split(',');
                            if (1 < ss1.Length)
                            {
                                string ss2 = ss1[0].Replace("(", null);

                                ss2 = ss2.Replace(")", null);
                                Pos = (short)ComF.StringToHex(ss2);
                            }
                        }
                    }
                }

                if (Pos == -1)
                {
                    Pos = Config.Can1.ID;
                }
                return Pos;
            }
            else
            {
                return 0;
            }
            //else
            //{
            //    short Pos = -1;
            //    string s1 = "Device=" + Config.Can2.Device.ToString();
            //    string s2 = "Channel=" + Config.Can2.Channel.ToString() + "h";

            //    foreach (string s in Device)
            //    {

            //        if (0 <= s.IndexOf(s1))
            //        {
            //            if (0 <= s.IndexOf(s2))
            //            {
            //                string ss = s.Substring(s.IndexOf("ID=") + "ID=".Length);
            //                string[] ss1 = ss.Split(',');
            //                if (1 < ss1.Length)
            //                {
            //                    string ss2 = ss1[0].Replace("(", null);

            //                    ss2 = ss2.Replace(")", null);
            //                    Pos = (short)ComF.StringToHex(ss2);
            //                }
            //            }
            //        }
            //    }

            //    if (Pos == -1)
            //    {
            //        Pos = Config.Can2.ID;
            //    }
            //    return Pos;
            //}

#else
                    return 0;
#endif
        }

        public PanelMeter GetPanelMeter
        {
            get { return pMeter; }
        }

        
        //public UsbMultiMeterControl GetMultiMeter
        //{
        //    get { return MultiMeter; }
        //}
        public GW_MultiMeter GetMultiMeter
        {
            get { return MultiMeter; }
        }
        
        //private string NAutoBarcode = "";
        public void StartSetting()
        {
            //bool Flag = false;

            //if (IOPort.GetAuto == false)
            //{
            //    //잠조에서 Micosoft.WisualBasic 을 먼저 등록해야 한다.
            //    string input = Microsoft.VisualBasic.Interaction.InputBox("바코드 정보", "바코드 정보를 입력해 주십시오.", NAutoBarcode);
            //    if (input == "")
            //    {
            //        if (MessageBox.Show(this, "바코드 정보가 입력되지 않았습니다. 검사를 진행하시겠습니까?", "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            //            Flag = true;
            //        else Flag = false;

            //        label17.Text = input;
            //    }
            //    else
            //    {
            //        Flag = true;
            //    }
            //    NAutoBarcode = input;
            //}
            //else
            //{
            //    Flag = true;
            //}

            //if (Flag == true)
            //{
            ScreenInit();
            CanReWrite.ModelTypeToRelax = CanMap.MODELTYPE.NOT_RELAX;

            OldPopDataReadFlag = PopDataReadFlag;
            IOPort.TestIOInit();
            TestEnd[0] = false;
            TestEnd[1] = false;

            //if (imageButton9.ButtonColor == Color.Red) imageButton9.ButtonColor = Color.Black;

            if (CheckItem.SBR == false) TestEnd[1] = true;
            PopDataReadFlag = false;
            PassButtonCheckFlag = true;
            BatteryOnOff(false);
            ComF.timedelay(300);
            IOPort.outportb(IO_OUT.TEST_OK, false);
            IOPort.outportb(IO_OUT.TEST_ING, true);
            IOPort.outportb(IO_OUT.GREEN, false);
            //outportb(IO_OUT.YELLOW, false);
            IOPort.outportb(IO_OUT.YELLOW, true);
            IOPort.outportb(IO_OUT.RED, false);
            led5.Value.AsBoolean = false; // Test ING
            led6.Value.AsBoolean = false; // Test OK
            label16.Text = "검사중";
            label16.ForeColor = Color.Yellow;
            label5.Text = "제품 검사중 입니다.";
            label16.BackColor = Color.Black;
            Step = 0;
            SbrStep = 0;
            GetCol = null;
            GetCurrCheck = null;            
            RunningFlag = true;

            OffSpecOutputFlag = false;
            //outportb(IO_OUT.YELLOW, true);
            IOPort.outportb(IO_OUT.YELLOW, true);
            IOPort.outportb(IO_OUT.TEST_ING, true);
            TInfor.Count.Total++;

            //SWCheckFirstFlag = false;
            TotalFirst = ComF.timeGetTimems();
            TotalLast = ComF.timeGetTimems();
            CanSendFirst = ComF.timeGetTimems();
            CanSendLast = ComF.timeGetTimems();

            //plot1.Channels[0].Clear();
            plot2.Channels[0].Clear();
            SbrSpecOutputFlag = false;
            //}
            return;
        }

        private void imageButton8_Click(object sender, EventArgs e)
        {
            if(RunningFlag == false) StartSetting();
            return;
        }

        private void SendData()
        {
            float Min;
            float Max;
            RESULT Vent = RESULT.PASS;
            RESULT Heater = RESULT.PASS;

            float Sbr15KData;
            float Sbr30KData;

            Sbr15KData = TData.SBR.Load15KData;
            Sbr30KData = TData.SBR.Load30KData;

            if (999 < Math.Abs(Sbr15KData)) Sbr15KData = 999;
            if (999 < Math.Abs(Sbr30KData)) Sbr30KData = 999;

            if (TData.HeaterLamp.Result[0] == RESULT.REJECT) Heater = RESULT.REJECT;
            if (TData.HeaterLamp.Result[1] == RESULT.REJECT) Heater = RESULT.REJECT;
            if (TData.HeaterLamp.Result[2] == RESULT.REJECT) Heater = RESULT.REJECT;

            if (TData.VentLamp.Result[0] == RESULT.REJECT) Vent = RESULT.REJECT;
            if (TData.VentLamp.Result[1] == RESULT.REJECT) Vent = RESULT.REJECT;
            if (TData.VentLamp.Result[2] == RESULT.REJECT) Vent = RESULT.REJECT;

            if ((CheckItem.SWHeater == true) && (CheckItem.SWVent == true))
            {
                Min = (float)TSpec.VentHeater.Min;
                Max = (float)TSpec.VentHeater.Max;
            }
            else 
            {
                Min = (float)TSpec.Heater.Min;
                Max = (float)TSpec.Heater.Max;
            }
            string s = MESCtrl.CrateData(
                   Serial: TInfor.Count.OK,
                   TestCount: TInfor.Count.Total,
                   Min_1: TData.HeaterCurrent.Test == true ? Min : -9999,
                   Max_1: TData.HeaterCurrent.Test == true ? Max : -9999,
                   Min_2: -9999,
                   Max_2: -9999,
                   Min_3: -9999,
                   Max_3: -9999,
                   Min_4: -9999,
                   Max_4: -9999,
                   Min_5: 1,
                   Max_5: 1,
                   Min_6: TData.VentCurr.Test == true ? (float)TSpec.Vent.Min : -9999,
                   Max_6: TData.VentCurr.Test == true ? (float)TSpec.Vent.Max : -9999,
                   Min_7: TData.PW_Recline.Test == true ? (float)TSpec.PWSwitch.Min : -9999,
                   Max_7: TData.PW_Recline.Test == true ? (float)TSpec.PWSwitch.Max : -9999,
                   Min_8: -9999,
                   Max_8: -9999,
                   Min_9: TData.Retractor.Test == true ? (float)TSpec.Retractor.Min : -9999,
                   Max_9: TData.Retractor.Test == true ? (float)TSpec.Retractor.Max : -9999,
                   Min_10: TData.SBR.Test == true ? TSpec.SBR.Load15Kg : -9999,
                   Max_10: TData.SBR.Test == true ? 999 : -9999,
                   Min_11: TData.SBR.Test == true ? 0 : -9999,
                   Max_11: TData.SBR.Test == true ? TSpec.SBR.Load30Kg : -9999,
                   Min_12: -9999,
                   Max_12: -9999,
                   Min_13: -9999,
                   Max_13: -9999,
                   Min_14: TData.HeaterLamp.Test == true ? 1 : -9999,
                   Max_14: TData.HeaterLamp.Test == true ? 1 : -9999,
                   Min_15: TData.VentLamp.Test == true ? 1 : -9999,
                   Max_15: TData.VentLamp.Test == true ? 1 : -9999,
                   Value1: (CheckItem.SWHeater == true) ? TData.HeaterCurrent.Data : -9999,
                   Value2: -9999,
                   Value3: -9999,
                   Value4: -9999,
                   Value5: (TData.BuckleWarning.Test == false) ? -9999 : (TData.BuckleWarning.Result == RESULT.REJECT ? 9 : 1),
                   Value6: (CheckItem.SWVent == true) ? TData.VentCurr.Data : -9999,
                   Value7: (TData.PW_Recline.Test == false) ? -9999 : TData.PW_Recline.Data,
                   Value8: -9999,
                   Value9: (TData.Retractor.Test == false) ? -9999 : TData.Retractor.Data,
                   Value10: (TData.SBR.Test == false) ? -9999 : Sbr15KData,
                   Value11: (TData.SBR.Test == false) ? -9999 : Sbr30KData,
                   Value12: -9999,
                   Value13: -9999,
                   Value14: TData.HeaterLamp.Test == true ? Heater == RESULT.PASS ? 1 : 9 : -9999,
                   Value15: TData.VentLamp.Test == true ? Vent == RESULT.PASS ? 1 : 9 : -9999,
                   Result: (short)TData.Result
                   );
            if (IOPort.GetAuto == true)
            {
                //if (MESCtrl.isServerConnection == true) MESCtrl.ServerSend = s;
                if (MESCtrl.isClientConnection == true) MESCtrl.Write = s + ",1";
            }
            else
            {
                if (MESCtrl.isClientConnection == true) MESCtrl.Write = s + ",2";
            }

            SaveLogData = "Sending - " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
            SaveLogData = s;
            return;
        }

        private void label2_Click(object sender, EventArgs e)
        {
            //TInfor.ReBootingFlag = true;
            //SaveInfor();
            //ComF.WindowRestartTo30Secconds();

            //System.Diagnostics.Process[] mProcess = System.Diagnostics.Process.GetProcessesByName(Application.ProductName);
            //foreach (System.Diagnostics.Process p in mProcess) p.Kill();
        }

        private void imageButton9_Click(object sender, EventArgs e)
        {
            //UserImageButton.ImageButton Btn = sender as UserImageButton.ImageButton;

            //Btn.ButtonColor = Btn.ButtonColor == Color.Red ? Color.Black : Color.Red;            
            //return;
        }

        private void DisplayIOIn()
        {
            if (IOPort.GetJigUp == true)
            {
                if (led9.Value.AsBoolean == false) led9.Value.AsBoolean = true;
                JigUpFlag = true;
            }
            else
            {
                if (led9.Value.AsBoolean == true) led9.Value.AsBoolean = false;
            }

            if (led13.Value.AsBoolean != IOPort.GetFrontSensor) led13.Value.AsBoolean = IOPort.GetFrontSensor;
            if (led14.Value.AsBoolean != IOPort.GetRearSensor) led14.Value.AsBoolean = IOPort.GetRearSensor;
            if (led12.Value.AsBoolean != IOPort.GetLeftSensor) led12.Value.AsBoolean = IOPort.GetLeftSensor;
            if (led16.Value.AsBoolean != IOPort.GetRightSensor) led16.Value.AsBoolean = IOPort.GetRightSensor;
            if (led17.Value.AsBoolean != IOPort.Get1StUpSensor) led17.Value.AsBoolean = IOPort.Get1StUpSensor;
            if (led15.Value.AsBoolean != IOPort.Get1StDownSensor) led15.Value.AsBoolean = IOPort.Get1StDownSensor;
            if (led18.Value.AsBoolean != IOPort.Get2StUp1Sensor) led19.Value.AsBoolean = IOPort.Get2StUp1Sensor;
            if (led18.Value.AsBoolean != IOPort.Get2StUp2Sensor) led18.Value.AsBoolean = IOPort.Get2StUp2Sensor;
            if (led21.Value.AsBoolean != IOPort.Get2StDown1Sensor) led21.Value.AsBoolean = IOPort.Get2StDown1Sensor;
            if (led20.Value.AsBoolean != IOPort.Get2StDown2Sensor) led20.Value.AsBoolean = IOPort.Get2StDown2Sensor;
            return;
        }

        private bool isCheckItem
        {
            get
            {
                if ((CheckItem.BuckleWarning == true) || (CheckItem.Heater.Curr == true) || (CheckItem.Vent.Curr == true) || (CheckItem.PowerSW == true) || (CheckItem.Retractor == true) || (CheckItem.SBR == true))
                    return true;
                else return false;
            }
        }

        private string SaveLogData
        {
            set
            {
                string xPath = Program.LOG_PATH + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                using (FileStream fp = File.Open(xPath, FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(fp);
                    writer.Write(value + "\n");
                    writer.Close();
                    fp.Close();
                }

                //using (File.Open(xPath, FileMode.Open))
                //{
                //    File.AppendAllText(value + "\n", xPath);
                //}
            }
        }
    }
}

