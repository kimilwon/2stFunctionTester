using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PowerSeat통합검사기
{
    public partial class OptionSet : Form
    {
        MyInterface mControl = null;
        public OptionSet()
        {
            InitializeComponent();
        }
        public OptionSet(MyInterface mControl)
        {
            InitializeComponent();
            this.mControl = mControl;
        }

        private void OptionSet_Load(object sender, EventArgs e)
        {
            comboBox5.Items.Clear();
            comboBox7.Items.Clear();
            comboBox8.Items.Clear();

            comboBox10.Items.Clear();
            comboBox3.Items.Clear();

            comboBox9.Items.Clear();
            comboBox1.Items.Clear();

            comboBox8.Items.Add("2400");
            comboBox8.Items.Add("9600");
            comboBox8.Items.Add("10400");
            comboBox8.Items.Add("19200");

            comboBox1.Items.Add("2400");
            comboBox1.Items.Add("9600");
            comboBox1.Items.Add("10400");
            comboBox1.Items.Add("19200");

            if (mControl.GetLin != null)
            {
                string[] Device = mControl.GetLin.GetDevice;

                foreach (string sx in Device)
                {
                    comboBox7.Items.Add(sx);
                    comboBox3.Items.Add(sx);
                }
            }

            if (mControl.GetCan != null)
            {
                string[] Device = mControl.GetCan.GetDevice;

                foreach (string sx in Device)
                {
                    comboBox5.Items.Add(sx);
                    comboBox10.Items.Add(sx);
                }
            }
            DisplaySpec();
            return;
        }

        private void OptionSet_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
            return;
        }

        private string BuadRate(int Pos)
        {
            string s = "";

            switch (Pos)
            {
                case 0: s = "2400"; break;
                case 1: s = "4800"; break;
                case 2: s = "9600"; break;
                case 3: s = "11400"; break;
                case 4: s = "19200"; break;
                case 5: s = "38400"; break;
                case 6: s = "59200"; break;
                default: s = "115200"; break;
            }
            return s;
        }

        private void DisplaySpec()
        {
            comboBox6.Items.Clear();
            comboBox6.Items.Add("5K");
            comboBox6.Items.Add("10K");
            comboBox6.Items.Add("20K");
            comboBox6.Items.Add("33K");
            comboBox6.Items.Add("47K");
            comboBox6.Items.Add("50K");
            comboBox6.Items.Add("83K");
            comboBox6.Items.Add("95K");
            comboBox6.Items.Add("100K");
            comboBox6.Items.Add("125K");
            comboBox6.Items.Add("250K");
            comboBox6.Items.Add("500K");
            comboBox6.Items.Add("800K");
            comboBox6.Items.Add("1M");

            comboBox9.Items.Clear();
            comboBox9.Items.Add("5K");
            comboBox9.Items.Add("10K");
            comboBox9.Items.Add("20K");
            comboBox9.Items.Add("33K");
            comboBox9.Items.Add("47K");
            comboBox9.Items.Add("50K");
            comboBox9.Items.Add("83K");
            comboBox9.Items.Add("95K");
            comboBox9.Items.Add("100K");
            comboBox9.Items.Add("125K");
            comboBox9.Items.Add("250K");
            comboBox9.Items.Add("500K");
            comboBox9.Items.Add("800K");
            comboBox9.Items.Add("1M");

            //textBox13.Text = mControl.Config.Scaner.Port.ToString();
            //comboBox3.SelectedItem = BuadRate(mControl.Config.Scaner.Speed);

            //textBox1.Text = mControl.GetConfig.MultiMeter.Port;
            //comboBox1.SelectedItem = BuadRate(mControl.GetConfig.MultiMeter.Speed);


            textBox16.Text = mControl.GetConfig.Power.Port;
            comboBox4.SelectedItem = BuadRate(mControl.GetConfig.Power.Speed);

            textBox2.Text = mControl.GetConfig.PanelMeter.Port;
            comboBox2.SelectedItem = BuadRate(mControl.GetConfig.PanelMeter.Speed);

            textBox4.Text = mControl.GetConfig.Power.Port;
            comboBox4.SelectedItem = BuadRate(mControl.GetConfig.Power.Speed);

            textBox11.Text = mControl.GetConfig.SerialtypeIOCard.Port;
            comboBox11.SelectedItem = BuadRate(mControl.GetConfig.SerialtypeIOCard.Speed);

            textBox13.Text = mControl.GetConfig.MultiMeter.Port;
            comboBox12.SelectedItem = BuadRate(mControl.GetConfig.MultiMeter.Speed);

            textBox3.Text = mControl.GetConfig.BattID.ToString();
            //textBox4.Text = mControl.GetConfig.PSwitchID.ToString();
            textBox5.Text = mControl.GetConfig.HeaterID.ToString();
            textBox6.Text = mControl.GetConfig.BuckleID.ToString();

            textBox1.Text = mControl.GetConfig.Board.IP;
            textBox7.Text = mControl.GetConfig.Board.Port.ToString();

            textBox4.Text = mControl.GetConfig.PC.IP;
            textBox9.Text = mControl.GetConfig.PC.Port.ToString();

            textBox8.Text = mControl.GetConfig.Server.IP;
            textBox12.Text = mControl.GetConfig.Server.Port.ToString();

            textBox10.Text = mControl.GetConfig.Client.IP;
            textBox15.Text = mControl.GetConfig.Client.Port.ToString();

            string s;
            string s1;
            string s2;
            
            
            //s1 = "(0x" + mControl.GetConfig.Can.Device.ToString("X2") + ")";
            s1 = "Device=" + mControl.GetConfig.Can1.Device.ToString();
            s2 = "Channel=" + mControl.GetConfig.Can1.Channel.ToString() + "h";

            for (int i = 0; i < comboBox5.Items.Count; i++)
            {
                s = comboBox5.Items[i].ToString();

                if (0 <= s.IndexOf(s1))
                {
                    if (0 <= s.IndexOf(s2))
                    {
                        comboBox5.SelectedIndex = i;
                        break;
                    }
                }
            }

            if ((0 <= mControl.GetConfig.Can1.Speed) && (0 < comboBox6.Items.Count) && (mControl.GetConfig.Can1.Speed < comboBox6.Items.Count)) comboBox6.SelectedIndex = mControl.GetConfig.Can1.Speed;


            //s1 = "Device=" + mControl.GetConfig.Can2.Device.ToString();
            //s2 = "Channel=" + mControl.GetConfig.Can2.Channel.ToString() + "h";

            //for (int i = 0; i < comboBox10.Items.Count; i++)
            //{
            //    s = comboBox10.Items[i].ToString();

            //    if (0 <= s.IndexOf(s1))
            //    {
            //        if (0 <= s.IndexOf(s2))
            //        {
            //            comboBox10.SelectedIndex = i;
            //            break;
            //        }
            //    }
            //}

            //if ((0 <= mControl.GetConfig.Can2.Speed) && (0 < comboBox9.Items.Count) && (mControl.GetConfig.Can2.Speed < comboBox9.Items.Count)) comboBox9.SelectedIndex = mControl.GetConfig.Can2.Speed;


            //if ((0 <= mControl.GetConfig.Lin[0].Device) && (0 < axComboBox8.ListCount) && (mControl.GetConfig.Lin[0].Device < axComboBox8.ListCount)) axComboBox8.ListIndex = mControl.GetConfig.Lin[0].Device;
            s1 = mControl.GetConfig.Lin1.Device.ToString() + " - ID";
            for (int i = 0; i < comboBox7.Items.Count; i++)
            {
                s = comboBox7.Items[i].ToString();

                if (0 <= s.IndexOf(s1))
                {
                    comboBox7.SelectedIndex = i;
                    break;
                }
            }
            if ((0 <= mControl.GetConfig.Lin1.Speed) && (0 < comboBox8.Items.Count) && (mControl.GetConfig.Lin1.Speed < comboBox8.Items.Count)) comboBox8.SelectedIndex = mControl.GetConfig.Lin1.Speed;


            s1 = mControl.GetConfig.Lin2.Device.ToString() + " - ID";
            for (int i = 0; i < comboBox7.Items.Count; i++)
            {
                s = comboBox3.Items[i].ToString();

                if (0 <= s.IndexOf(s1))
                {
                    comboBox3.SelectedIndex = i;
                    break;
                }
            }
            if ((0 <= mControl.GetConfig.Lin2.Speed) && (0 < comboBox1.Items.Count) && (mControl.GetConfig.Lin2.Speed < comboBox1.Items.Count)) comboBox1.SelectedIndex = mControl.GetConfig.Lin2.Speed;
            //comboBox5.SelectedIndex = mControl.GetConfig.Can.Port;
            //comboBox6.SelectedIndex = mControl.GetConfig.Can.Speed;

            //comboBox7.SelectedIndex = mControl.GetConfig.Lin.Port;
            //comboBox8.SelectedIndex = 3;//mControl.Config.Lin.Speed;
            return;
        }

        private void MoveSpec()
        {
            //if (int.TryParse(textBox13.Text, out mControl.Config.Scaner.Port) == false) mControl.Config.Scaner.Port = 0;
            //mControl.Config.Scaner.Speed = comboBox3.SelectedIndex;

            //if (int.TryParse(textBox1.Text, out mControl.Config.MultiMeter.Port) == false) mControl.Config.MultiMeter.Port = 0;
            //mControl.Config.MultiMeter.Speed = comboBox1.SelectedIndex;

            __Config__ Cfg = mControl.GetConfig;

            //Cfg.Power.Port = textBox16.Text;
            //Cfg.Power.Speed = comboBox4.SelectedIndex;

            Cfg.PanelMeter.Port = textBox2.Text;
            Cfg.PanelMeter.Speed = comboBox2.SelectedIndex;

            Cfg.Power.Port = textBox16.Text;
            Cfg.Power.Speed = comboBox4.SelectedIndex;

            Cfg.SerialtypeIOCard.Port = textBox11.Text;
            Cfg.SerialtypeIOCard.Speed = comboBox11.SelectedIndex;

            Cfg.MultiMeter.Port = textBox13.Text;
            Cfg.MultiMeter.Speed = comboBox12.SelectedIndex;

            if (int.TryParse(textBox3.Text, out Cfg.BattID) == false) Cfg.BattID = 0;
            //if (int.TryParse(textBox4.Text, out Cfg.PSwitchID) == false) Cfg.PSwitchID = 0;
            if (int.TryParse(textBox5.Text, out Cfg.HeaterID) == false) Cfg.HeaterID = 0;
            if (int.TryParse(textBox6.Text, out Cfg.BuckleID) == false) Cfg.BuckleID = 0;

            Cfg.Board.IP = textBox1.Text;
            if (int.TryParse(textBox7.Text, out Cfg.Board.Port) == false) Cfg.Board.Port = 0;

            Cfg.PC.IP = textBox4.Text;
            if (int.TryParse(textBox9.Text, out Cfg.PC.Port) == false) Cfg.PC.Port = 0;

            Cfg.Server.IP = textBox8.Text;
            if (int.TryParse(textBox12.Text, out Cfg.Server.Port) == false) Cfg.Server.Port = 0;

            Cfg.Client.IP = textBox10.Text;
            if (int.TryParse(textBox15.Text, out Cfg.Client.Port) == false) Cfg.Client.Port = 0;

            //Cfg.Can.Port = comboBox5.SelectedIndex;
            //Cfg.Can.Speed = comboBox6.SelectedIndex;

            //Cfg.Lin.Port = comboBox7.SelectedIndex;
            //Cfg.Lin.Speed = 3;// comboBox8.SelectedIndex;

            if (1 <= comboBox5.Items.Count)
            {
                string[] s = comboBox5.Text.Split(':');
                string[] s2 = s[1].Split(',');

                string ss1 = s2[0].Substring(s2[0].IndexOf("Device=") + "Device=".Length);
                string ss2 = s2[2].Substring(s2[2].IndexOf("Channel=") + "Channel=".Length);

                ss2 = ss2.Replace("h", null);

                if (short.TryParse(ss1, out Cfg.Can1.Device) == false) Cfg.Can1.Device = -1;
                if (short.TryParse(ss2, out Cfg.Can1.Channel) == false) Cfg.Can1.Channel = -1;

                ss1 = s2[1].Substring(ss1.IndexOf("ID=") + "ID=".Length);

                ss2 = ss1.Replace("(", null);
                ss2 = ss2.Replace(")", null);
                Cfg.Can1.ID = (short)mControl.공용함수.StringToHex(ss2);
            }
            if (0 <= comboBox6.SelectedIndex) Cfg.Can1.Speed = comboBox6.SelectedIndex;

            //if (0 <= comboBox10.Items.Count)
            //{
            //    string[] s = comboBox10.Text.Split(':');
            //    string[] s2 = s[1].Split(',');

            //    string ss1 = s2[0].Substring(s2[0].IndexOf("Device=") + "Device=".Length);
            //    string ss2 = s2[2].Substring(s2[2].IndexOf("Channel=") + "Channel=".Length);

            //    ss2 = ss2.Replace("h", null);

            //    if (short.TryParse(ss1, out Cfg.Can2.Device) == false) Cfg.Can2.Device = -1;
            //    if (short.TryParse(ss2, out Cfg.Can2.Channel) == false) Cfg.Can2.Channel = -1;

            //    ss1 = s2[1].Substring(ss1.IndexOf("ID=") + "ID=".Length);

            //    ss2 = ss1.Replace("(", null);
            //    ss2 = ss2.Replace(")", null);
            //    Cfg.Can2.ID = (short)mControl.공용함수.StringToHex(ss2);
            //}
            //if (0 <= comboBox9.SelectedIndex) Cfg.Can2.Speed = comboBox6.SelectedIndex;

            if (1 <= comboBox7.Items.Count)
            {
                string[] s = comboBox7.Text.Split(',');

                if (2 <= s.Length)
                {
                    string[] s2 = s[0].Trim().Split('.');

                    if (2 <= s2.Length)
                    {
                        string[] ID = s2[1].Trim().Split('-');
                        ID[0] = ID[0].Trim();
                        if (short.TryParse(ID[0], out Cfg.Lin1.Device) == false) Cfg.Lin1.Device = 0;
                    }
                    //Config.Can.Device = (short)axComboBox4.ListIndex;
                }
            }
            if (0 <= comboBox8.SelectedIndex) Cfg.Lin1.Speed = comboBox8.SelectedIndex;

            if (1 <= comboBox3.Items.Count)
            {
                string[] s = comboBox3.Text.Split(',');

                if (2 <= s.Length)
                {
                    string[] s2 = s[0].Trim().Split('.');

                    if (2 <= s2.Length)
                    {
                        string[] ID = s2[1].Trim().Split('-');
                        ID[0] = ID[0].Trim();
                        if (short.TryParse(ID[0], out Cfg.Lin2.Device) == false) Cfg.Lin2.Device = 0;
                    }
                    //Config.Can.Device = (short)axComboBox4.ListIndex;
                }
            }
            if (0 <= comboBox1.SelectedIndex) Cfg.Lin2.Speed = comboBox1.SelectedIndex;

            mControl.GetConfig = Cfg;
            return;
        }

        

        private void imageButton1_Click(object sender, EventArgs e)
        {
            MoveSpec();
            ConfigSetting Set = new ConfigSetting();
            Set.ReadWriteConfig = mControl.GetConfig;
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }
    }
}
