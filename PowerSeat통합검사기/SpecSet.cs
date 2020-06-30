using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PowerSeat통합검사기
{
    public partial class SpecSet : Form
    {
        MyInterface mControl  = null;
        private string Model;
        private __Spec__ TSpec;

        public SpecSet()
        {
            InitializeComponent();
        }

        public SpecSet(MyInterface mControl, string s)
        {
            InitializeComponent();
            Model = s;
            this.mControl = mControl;
        }


        private void SpecSet_Load(object sender, EventArgs e)
        {
            TSpec = new __Spec__();
            

            CarModelListOpen();


            if (0 < comboBox1.Items.Count)
            {
                comboBox1.SelectedItem = Model;
            }
            else
            {
                Model = "";
                ClearSpec();
                DisplaySpec();
            }
            
            return;
        }

        public string SpecName
        {
            get
            {
                return Model;
            }
        }
                
        private void DisplaySpec()
        {
            comboBox1.SelectedItem = TSpec.CarName;

            fpSpread1.ActiveSheet.Cells[2, 1].Text = string.Format("{0:0.00}", TSpec.Heater.Min);
            fpSpread1.ActiveSheet.Cells[2, 2].Text = string.Format("{0:0.00}", TSpec.Heater.Max);

            fpSpread1.ActiveSheet.Cells[3, 1].Text = string.Format("{0:0.00}", TSpec.VentHeater.Min);
            fpSpread1.ActiveSheet.Cells[3, 2].Text = string.Format("{0:0.00}", TSpec.VentHeater.Max);

            fpSpread1.ActiveSheet.Cells[4, 1].Text = string.Format("{0:0.00}", TSpec.Vent.Min);
            fpSpread1.ActiveSheet.Cells[4, 2].Text = string.Format("{0:0.00}", TSpec.Vent.Max);

            //fpSpread1.ActiveSheet.Cells[4, 1].Text = string.Format("{0:0.00}", TSpec.LHHeater.Min);
            //fpSpread1.ActiveSheet.Cells[4, 2].Text = string.Format("{0:0.00}", TSpec.LHHeater.Max);

            fpSpread1.ActiveSheet.Cells[10, 1].Text = string.Format("{0:0.00}", TSpec.Retractor.Min);
            fpSpread1.ActiveSheet.Cells[10, 2].Text = string.Format("{0:0.00}", TSpec.Retractor.Max);

            
            //fpSpread1.ActiveSheet.Cells[5, 1].Text = string.Format("{0:0.00}", TSpec.BuckleWaring.Min);
            //fpSpread1.ActiveSheet.Cells[5, 2].Text = string.Format("{0:0.00}", TSpec.BuckleWaring.Max);

            fpSpread1.ActiveSheet.Cells[5, 1].Text = string.Format("{0:0.00}", TSpec.PWSwitch.Min);
            fpSpread1.ActiveSheet.Cells[5, 2].Text = string.Format("{0:0.00}", TSpec.PWSwitch.Max);

            fpSpread1.ActiveSheet.Cells[7, 1].Text = string.Format("{0:0.00}", TSpec.SBR.NotLoad);

            fpSpread1.ActiveSheet.Cells[8, 1].Text = string.Format("{0:0.00}", TSpec.SBR.Load15Kg);

            fpSpread1.ActiveSheet.Cells[9, 2].Text = string.Format("{0:0.00}", TSpec.SBR.Load30Kg);

            

            fpSpread1.ActiveSheet.Cells[10, 5].Text = string.Format("{0:0.00}", TSpec.Volt);
            fpSpread1.ActiveSheet.Cells[10, 7].Text = string.Format("{0:0.00}", TSpec.SWOffCurr);

            fpSpread1.ActiveSheet.Cells[0, 6].Text = string.Format("{0}", TSpec.SWCheckTime);

            fpSpread1.ActiveSheet.Cells[1, 6].Text = string.Format("{0}", TSpec.HeaterCheckPos.Time);
            if (TSpec.HeaterCheckPos.Pos <= 0) TSpec.HeaterCheckPos.Pos = 1;            
            //if (TSpec.HeaterCheckPos2.Pos <= 0) TSpec.HeaterCheckPos2.Pos = 1;

            fpSpread1.ActiveSheet.Cells[2, 6].Value = TSpec.HeaterCheckPos.Pos;
            //fpSpread1.ActiveSheet.Cells[3, 7].Value = TSpec.HeaterCheckPos2.Pos;
            fpSpread1.ActiveSheet.Cells[3, 6].Text = string.Format("{0}", TSpec.VentCheckPos.Time);
            if (TSpec.VentCheckPos.Pos <= 0) TSpec.VentCheckPos.Pos = 1;
            fpSpread1.ActiveSheet.Cells[4, 6].Value = TSpec.VentCheckPos.Pos;

            Offset_HeaterLH.Text = string.Format("{0:0.00}", TSpec.Offset.HeaterLH);
            Offset_HeaterRH.Text = string.Format("{0:0.00}", TSpec.Offset.HeaterRH);
            Offset_Vent.Text = string.Format("{0:0.00}", TSpec.Offset.Vent);
            Offset_Buckle.Text = string.Format("{0:0.00}", TSpec.Offset.BuckleSensor);
            Offset_Airbag.Text = string.Format("{0:0.00}", TSpec.Offset.Retractor);
            

            if (TSpec.Offset.IGN == false)
                radioButton4.Checked = true;
            else radioButton3.Checked = true;

            if (TSpec.Offset.Button == false)
                radioButton6.Checked = true;
            else radioButton5.Checked = true;

            checkBox2.Checked = TSpec.Offset.DrvLamp;
            checkBox3.Checked = TSpec.Offset.AssistLamp;
            return;
        }

        private void MoveSpec()
        {
            TSpec.CarName = comboBox1.SelectedItem.ToString();

            if (double.TryParse(fpSpread1.ActiveSheet.Cells[2, 1].Text, out TSpec.Heater.Min) == false) TSpec.Heater.Min = 0;
            if (double.TryParse(fpSpread1.ActiveSheet.Cells[2, 2].Text, out TSpec.Heater.Max) == false) TSpec.Heater.Max = 0;
            if (double.TryParse(fpSpread1.ActiveSheet.Cells[3, 1].Text, out TSpec.VentHeater.Min) == false) TSpec.VentHeater.Min = 0;
            if (double.TryParse(fpSpread1.ActiveSheet.Cells[3, 2].Text, out TSpec.VentHeater.Max) == false) TSpec.VentHeater.Max = 0;
            if (double.TryParse(fpSpread1.ActiveSheet.Cells[4, 1].Text, out TSpec.Vent.Min) == false) TSpec.Vent.Min = 0;
            if (double.TryParse(fpSpread1.ActiveSheet.Cells[4, 2].Text, out TSpec.Vent.Max) == false) TSpec.Vent.Max = 0;

            if (double.TryParse(fpSpread1.ActiveSheet.Cells[10, 1].Text, out TSpec.Retractor.Min) == false) TSpec.Retractor.Min = 0;
            if (double.TryParse(fpSpread1.ActiveSheet.Cells[10, 2].Text, out TSpec.Retractor.Max) == false) TSpec.Retractor.Max = 0;

            //if (double.TryParse(fpSpread1.ActiveSheet.Cells[5, 1].Text, out TSpec.Lumber.Min) == false) TSpec.Lumber.Min = 0;
            //if (double.TryParse(fpSpread1.ActiveSheet.Cells[5, 2].Text, out TSpec.Lumber.Max) == false) TSpec.Lumber.Max = 0;

            if (double.TryParse(fpSpread1.ActiveSheet.Cells[5, 1].Text, out TSpec.PWSwitch.Min) == false) TSpec.PWSwitch.Min = 0;
            if (double.TryParse(fpSpread1.ActiveSheet.Cells[5, 2].Text, out TSpec.PWSwitch.Max) == false) TSpec.PWSwitch.Max = 0;

            //if (double.TryParse(fpSpread1.ActiveSheet.Cells[5, 1].Text, out TSpec.BuckleWaring.Min) == false) TSpec.BuckleWaring.Min = 0;
            //if (double.TryParse(fpSpread1.ActiveSheet.Cells[5, 2].Text, out TSpec.BuckleWaring.Max) == false) TSpec.BuckleWaring.Max = 0;

            if (float.TryParse(fpSpread1.ActiveSheet.Cells[7, 1].Text, out TSpec.SBR.NotLoad) == false) TSpec.SBR.NotLoad = 0;

            if (float.TryParse(fpSpread1.ActiveSheet.Cells[8, 1].Text, out TSpec.SBR.Load15Kg) == false) TSpec.SBR.Load15Kg = 0;
            if (float.TryParse(fpSpread1.ActiveSheet.Cells[9, 2].Text, out TSpec.SBR.Load30Kg) == false) TSpec.SBR.Load30Kg = 0;

            //if (double.TryParse(fpSpread1.ActiveSheet.Cells[13, 1].Text, out TSpec.Heater2.Min) == false) TSpec.Heater2.Min = 0;
            //if (double.TryParse(fpSpread1.ActiveSheet.Cells[13, 2].Text, out TSpec.Heater2.Max) == false) TSpec.Heater2.Max = 0;


            
            //if (double.TryParse(fpSpread1.ActiveSheet.Cells[8, 6].Text, out TSpec.LHFan.Min) == false) TSpec.LHFan.Min = 0;
            //if (double.TryParse(fpSpread1.ActiveSheet.Cells[8, 7].Text, out TSpec.LHFan.Max) == false) TSpec.LHFan.Max = 0;

            if (float.TryParse(fpSpread1.ActiveSheet.Cells[10, 5].Text, out TSpec.Volt) == false) TSpec.Volt = 0;
            if (float.TryParse(fpSpread1.ActiveSheet.Cells[10, 7].Text, out TSpec.SWOffCurr) == false) TSpec.SWOffCurr = 0;

            if (int.TryParse(fpSpread1.ActiveSheet.Cells[0, 6].Text, out TSpec.SWCheckTime) == false) TSpec.SWCheckTime = 0;

            if (int.TryParse(fpSpread1.ActiveSheet.Cells[1, 6].Text, out TSpec.HeaterCheckPos.Time) == false) TSpec.HeaterCheckPos.Time = 0;
            //if (int.TryParse(fpSpread1.ActiveSheet.Cells[2, 6].Text, out TSpec.HeaterCheckPos2.Time) == false) TSpec.HeaterCheckPos2.Time = 0;
            if (int.TryParse(fpSpread1.ActiveSheet.Cells[2, 6].Value.ToString(), out TSpec.HeaterCheckPos.Pos) == false) TSpec.HeaterCheckPos.Pos = 0;
            //if (int.TryParse(fpSpread1.ActiveSheet.Cells[3, 7].Value.ToString(), out TSpec.HeaterCheckPos2.Pos) == false) TSpec.HeaterCheckPos2.Pos = 0;
            if (int.TryParse(fpSpread1.ActiveSheet.Cells[3, 6].Text, out TSpec.VentCheckPos.Time) == false) TSpec.VentCheckPos.Time = 0;
            if (int.TryParse(fpSpread1.ActiveSheet.Cells[4, 6].Text, out TSpec.VentCheckPos.Pos) == false) TSpec.VentCheckPos.Pos = 0;

            if (float.TryParse(Offset_HeaterLH.Text, out TSpec.Offset.HeaterLH) == false) TSpec.Offset.HeaterLH = 0;           
            if (float.TryParse(Offset_HeaterRH.Text, out TSpec.Offset.HeaterRH) == false) TSpec.Offset.HeaterRH = 0;
            if (float.TryParse(Offset_Vent.Text, out TSpec.Offset.Vent) == false) TSpec.Offset.Vent = 0;
            if (float.TryParse(Offset_Buckle.Text, out TSpec.Offset.BuckleSensor) == false) TSpec.Offset.BuckleSensor = 0;
            if (float.TryParse(Offset_Airbag.Text, out TSpec.Offset.Retractor) == false) TSpec.Offset.Retractor = 0;
            //if (float.TryParse(Offset_RearHeater.Text, out TSpec.Offset.HeaterRear) == false) TSpec.Offset.HeaterRear = 0;

            //TSpec.Offset.CanType = radioButton1.Checked;
            TSpec.Offset.IGN = radioButton3.Checked;
            TSpec.Offset.Button = radioButton5.Checked;

            TSpec.Offset.DrvLamp = checkBox2.Checked;
            TSpec.Offset.AssistLamp = checkBox3.Checked;
            return;
        }
        
        private void SpecSet_FormClosing(object sender, FormClosingEventArgs e)
        {
            CarModelListSave();
            e.Cancel = false;
            return;
        }

        private void ClearSpec()
        {
            TSpec.Retractor.Max = 0;
            TSpec.Retractor.Min = 0;
            //TSpec.BuckleSensor.Max = 0;
            //TSpec.BuckleSensor.Min = 0;
            TSpec.CarName = "";
            TSpec.Heater.Max = 0;
            TSpec.Heater.Min = 0;
            //TSpec.Heater2.Max = 0;
            //TSpec.Heater2.Min = 0;
            TSpec.HeaterCheckPos.Pos = 0;
            TSpec.HeaterCheckPos.Time = 0;
            //TSpec.HeaterCheckPos2.Pos = 0;
            //TSpec.HeaterCheckPos2.Time = 0;
            TSpec.SWCheckTime = 0;

            TSpec.SBR.Load15Kg = 0;
            TSpec.SBR.Load30Kg = 0;
            TSpec.SBR.NotLoad = 0;
            //TSpec.IMS.M1 = 0;
            //TSpec.IMS.M2 = 0;
            //TSpec.IMS.M3 = 0;
            //TSpec.IMS.Set = 0;
            //TSpec.Lumber.Max = 0;
            //TSpec.Lumber.Min = 0;
            TSpec.Offset.Retractor = 0;
            TSpec.Offset.BuckleSensor = 0;
            TSpec.Offset.Button = false;
            //TSpec.Offset.CanType = false;
            TSpec.Offset.HeaterLH = 0;
            TSpec.Offset.HeaterRH = 0;
            TSpec.Offset.IGN = false;
            TSpec.Offset.Vent = 0;
            //TSpec.Offset.HeaterRear = 0;
            TSpec.PWSwitch.Max = 0;
            TSpec.PWSwitch.Min = 0;
            TSpec.Vent.Max = 0;
            TSpec.Vent.Min = 0;
            TSpec.VentCheckPos.Pos = 0;
            TSpec.VentCheckPos.Time = 0;
            TSpec.VentHeater.Max = 0;
            TSpec.VentHeater.Min = 0;
            //TSpec.SBR.Max = 0;
            //TSpec.SBR.Min = 0;

            //TSpec.LHHeater.Max = 0;
            //TSpec.LHHeater.Min = 0;

            //TSpec.LHFan.Max = 0;
            //TSpec.LHFan.Min = 0;

            //TSpec.NTC.Max = 0;
            //TSpec.NTC.Min = 0;

            //TSpec.BwrSpeed.Max = 0;
            //TSpec.BwrSpeed.Min = 0;

            TSpec.Volt = 0;
            TSpec.SWOffCurr = 0;
            return;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string xName = comboBox1.SelectedItem.ToString();

            if (xName == "KA4 2열 7/8P") xName = "KA4 2열 78P";
            string dName = Program.SPEC_PATH.ToString() + "\\" + xName + ".spc";
            if (File.Exists(dName) == true)
            {
                TSpec = mControl.공용함수.ReadSpec(dName);
                DisplaySpec();
            }
            return;
        }

        private void CarModelListSave()
        {
            string fPath = Program.SYSTEM_PATH.ToString();

            fPath = fPath + "\\CarList.ini";

            if (File.Exists(fPath) == true) File.Delete(fPath);

            FileStream fp = new FileStream(fPath, FileMode.CreateNew);            
            StreamWriter write = new StreamWriter(fp);

            foreach (string s in comboBox1.Items)
            {
                if (s == "KA4 2열 7/8P")
                    write.Write("KA4 2열 78P" + "\n");
                else write.Write(s + "\n");
            }
            write.Close();
            return;
        }

        private void CarModelListOpen()
        {
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
                if(s == "KA4 2열 78P")
                    comboBox1.Items.Add("KA4 2열 7/8P");
                else comboBox1.Items.Add(s);
            }            
            read.Close();
            return;
        }

        private short ButtonInPos = 0;

        private void imageButton1_Click(object sender, EventArgs e)
        {
            /*
            string s = "";
            
            if (InputBox.Show("차종 입력", "차종명을 입력해 주십시오.", ref s) == DialogResult.OK)
            {
                Name = Program.SPEC_PATH + "\\" + s + ".spc";

                if (File.Exists(Name) == false)
                    comboBox1.Items.Add(s);
                else MessageBox.Show("차종이 이미 존재 합니다.");
                
                ClearSpec();
                DisplaySpec();
                TSpec.CarName = s;
                mControl.공용함수.WriteSpec(Name, TSpec);
                comboBox1.SelectedItem = s;
                Model = s;
                return;
            }
            */
            textBox1.Text = "";
            panel5.Visible = true;
            textBox1.Focus();
            ButtonInPos = 1;
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            MoveSpec();
            mControl.공용함수.WriteSpec(Name, TSpec);
            return;
        }

        private void imageButton3_Click(object sender, EventArgs e)
        {
            /*
            string s = "";

            if (InputBox.Show("차종 입력", "차종명을 입력해 주십시오.", ref s) == DialogResult.OK)
            {
                Name = Program.SPEC_PATH + "\\" + s + ".spc";
                MoveSpec();
                TSpec.CarName = s;
                if (File.Exists(Name) == true) comboBox1.Items.Add(s);
                mControl.공용함수.WriteSpec(Name, TSpec);
                comboBox1.SelectedItem = s;
                Model = s;
            }
            */

            textBox1.Text = "";
            panel5.Visible = true;
            textBox1.Focus();
            ButtonInPos = 2;
            return;
        }

        private void imageButton4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null) return;
            if (comboBox1.SelectedItem.ToString() == "") return;
            if (comboBox1.SelectedIndex < 0) return;

            string s = comboBox1.SelectedItem.ToString();

            if (s == "KA4 2열 7/8P") s = "KA4 2열 78P";

            string xName = Program.SPEC_PATH + "\\" + s + ".spc";
            if (File.Exists(xName) == true) File.Delete(xName);

            ClearSpec();
            DisplaySpec();
            return;
        }

        private void imageButton5_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }

        private void imageButton6_Click(object sender, EventArgs e)
        {
            string s = textBox1.Text;

            if (s == "KA4 7/8P") s = "KA4 78P";

            if (ButtonInPos == 2)
            {
                Name = Program.SPEC_PATH + "\\" + s + ".spc";
                MoveSpec();
                TSpec.CarName = textBox1.Text;
                if (File.Exists(Name) == true) comboBox1.Items.Add(s);
                mControl.공용함수.WriteSpec(Name, TSpec);
                comboBox1.SelectedItem = s;
                Model = s;
            }
            else if (ButtonInPos == 1)
            {
                Name = Program.SPEC_PATH + "\\" + s + ".spc";

                if (File.Exists(Name) == false)
                    comboBox1.Items.Add(s);
                else MessageBox.Show("차종이 이미 존재 합니다.");

                ClearSpec();
                DisplaySpec();
                TSpec.CarName = textBox1.Text;
                mControl.공용함수.WriteSpec(Name, TSpec);
                comboBox1.SelectedItem = s;
                Model = s;
            }
            panel5.Visible = false;
            return;
        }

        private void imageButton7_Click(object sender, EventArgs e)
        {
            panel5.Visible = false;
            return;
        }

        private Form OffsetForm = null;
        private void imageButton8_Click(object sender, EventArgs e)
        {
            OffsetForm = new Form()
            {
                ControlBox = false,
                MinimizeBox = false,
                MaximizeBox = false,
                FormBorderStyle = FormBorderStyle.FixedSingle,
                Owner = this,
                StartPosition = FormStartPosition.CenterParent,
                WindowState = FormWindowState.Normal,
                TopMost = true,
                Text = "OFFSET SETTING",
                Size = new Size(panel1.Width + 21, panel1.Height + 36),
                Location = new Point((this.Width / 2) - ((panel1.Width + 21) / 2), 300)
            };

            panel1.Visible = true;
            panel1.Parent = OffsetForm;
            panel1.Location = new Point(1, 1);
            imageButton8.Enabled = false;

            OffsetForm.FormClosing += delegate (object sender1, FormClosingEventArgs e1)
            {
                e1.Cancel = false;
                panel1.Parent = this;
                panel1.Visible = false;
                imageButton8.Enabled = true;
                OffsetForm.Dispose();
                OffsetForm = null;
            };
            imageButton9.Click += delegate (object sender2, EventArgs e2)
            {
                if (OffsetForm != null) OffsetForm.Close();
            };

            OffsetForm.Show();
            return;
        }
    }
}
