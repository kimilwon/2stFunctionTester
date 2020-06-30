using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;
//using __COMMONE;

namespace PowerSeat통합검사기
{
    public partial class PasswordCheckForm : Form
    {
        bool Result;
        private Password Pass = new Password();

        public PasswordCheckForm()
        {
            InitializeComponent();            
            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Result = false;
            this.Close();
            return;
        }

        public bool result
        {
            get { return Result; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Pass.GetPassword == textBox1.Text)
            {
                Result = true;
            }
            else
            {
                if (Pass.GetMaster == textBox1.Text)
                {
                    Result = true;
                }
                else
                {
                    MessageBox.Show("입력한 암호가 일치하지 않습니다.");
                    Result = false;
                }
            }
            this.Close();
            return;
        }

        private void PasswordCheckForm_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            
            return;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {            
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //EventArgs x;

            if (e.KeyChar == (char)ConsoleKey.Enter)
            {
                if (Pass.GetPassword == textBox1.Text)
                {
                    Result = true;
                }
                else
                {
                    if (Pass.GetMaster == textBox1.Text)
                    {
                        Result = true;
                    }
                    else
                    {
                        MessageBox.Show("입력한 암호가 일치하지 않습니다.");
                        Result = false;
                    }
                }
                this.Close();
            }
            return;
        }

        private void PasswordCheckForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
            return;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Pass.GetPassword == textBox1.Text)
            {
                PasswordSetForm set = new PasswordSetForm();
                set.Show();               
            }
            else
            {
                if (Pass.GetMaster == textBox1.Text)
                {
                    PasswordSetForm set = new PasswordSetForm();
                    set.Show();
                }
                else
                {
                    MessageBox.Show("입력한 암호가 일치하지 않습니다.");                    
                }
            }
        }
    }
}
