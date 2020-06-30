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
using PowerSeat통합검사기.Properties;

using System.Collections;
using System.Linq;

namespace PowerSeat통합검사기
{
    public class InputBox
    {
        public static DialogResult Show(string title, string promptText, ref string value)
        {
            return Show(title, promptText, ref value, null);
        }

        public static DialogResult Show(string title, string promptText, ref string value,
                                        InputBoxValidation validation)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.TopMost = true;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            if (validation != null)
            {
                form.FormClosing += delegate (object sender, FormClosingEventArgs e)
                {
                    if (form.DialogResult == DialogResult.OK)
                    {
                        string errorText = validation(textBox.Text);
                        if (e.Cancel = (errorText != ""))
                        {
                            MessageBox.Show(form, errorText, "Validation Error",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                            textBox.Focus();
                        }
                    }
                };
            }
            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }
    public delegate string InputBoxValidation(string errorMessage);


    public class uMessageBox
    {
        public static void Show(string title = "경고", string promptText = null)
        {
            xShow(title, promptText);
            return;
        }

        public static void xShow(string title, string promptText)
        {
            MessageBox.Show(caption: title, text: promptText, buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Warning, defaultButton: MessageBoxDefaultButton.Button1, options: MessageBoxOptions.ServiceNotification);
        }
    }


    public class uxMessageBox : IDisposable
    {
        static Button[] Buttonx;
        static Form form = new Form();

        public static DialogResult Show(string title, string promptText, string[] sButton, DialogResult[] Result)
        {
            DialogResult sResult = Show(title, promptText, sButton, Result, null);
            return sResult;
        }

        public static DialogResult Show(string title = "경고", string promptText = null)
        {
            DialogResult sResult = Show(title, promptText, null);
            return sResult;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            return;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            return;
        }

        public static DialogResult Show(string title, string promptText, string[] sButton, DialogResult[] Result,
                                        uxMessageBoxValidation validation)
        {
            Label label = new Label();
            Buttonx = new Button[sButton.Length];
            PictureBox picture = new PictureBox()
            {
                Parent = form,
                Image = PowerSeat통합검사기.Properties.Resources.Danger_Shield,

            };
            picture.SetBounds(9, 10, 32, 32);


            int i = 0;
            int bSize;

            for (i = 0; i < Buttonx.Length; i++)
            {
                Buttonx[i] = new Button()
                {
                    AutoSize = false
                };
            }

            i = 0;
            foreach (string s in sButton)
            {
                Buttonx[i].Text = s;
                //Buttonx[i].AutoSize = true;
                i++;
            }
            int Max = 0;
            foreach (string s in sButton)
            {
                label.Text = s;
                if (Max < label.Width) Max = label.Right;
            }

            bSize = Max + 10;

            form.Text = title;
            label.Text = promptText;


            int Size = 0;
            for (i = 0; i < sButton.Length; i++)
            {
                Size += bSize + 8;
            }

            //label.SetBounds(9, 20, 372, 13);
            label.SetBounds(50, 20, 372, 13);
            label.Parent = form;
            label.AutoSize = true;

            Max = Math.Max(300, label.Right + 10);
            Max = Math.Max(Size, label.Right + 10);


            form.ClientSize = new Size(396 + 50, 107);
            form.ClientSize = new Size(Math.Max(300, Max) + 50, form.ClientSize.Height);

            int Widgth = (form.Width / 2) - (Size / 2);
            int X = 0;

            X = Widgth;
            for (i = 0; i < sButton.Length; i++)
            {
                Buttonx[i].SetBounds(X, 72, bSize, 23);
                X += bSize + 8;
                Buttonx[i].Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                Buttonx[i].Parent = form;
                Buttonx[i].Visible = true;
                Buttonx[i].DialogResult = Result[i];
                Buttonx[i].Click += new EventHandler(ButtonClick);
            }

            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ControlBox = false;
            form.TopMost = true;

            if (validation != null)
            {
                form.FormClosing += delegate (object sender, FormClosingEventArgs e)
                {
                    if (form.DialogResult == DialogResult.OK)
                    {
                    }
                    else if (form.DialogResult == DialogResult.No)
                    {
                    }
                    else if (form.DialogResult == DialogResult.Cancel)
                    {
                    }
                    form.Dispose();
                    form = null;
                };
            }
            DialogResult dialogResult = form.ShowDialog();
            return dialogResult;
        }

        public static DialogResult Show(string title, string promptText, uxMessageBoxValidation validation)
        {
            Label label = new Label();
            Buttonx = new Button[1];
            Buttonx[0] = new Button() { AutoSize = false };
            PictureBox picture = new PictureBox()
            {
                Parent = form,
                Image = PowerSeat통합검사기.Properties.Resources.Danger_Shield

            };
            picture.SetBounds(9, 10, 32, 32);

            System.Drawing.Graphics formGraphics = form.CreateGraphics();

            float FWidth = (form.Font.SizeInPoints / 72) * formGraphics.DpiX;
            int FHeight = form.Font.Height;

            Buttonx[0].Text = "확인";

            int bSize = (int)("확인".Length * FWidth);
            int Size = bSize + 50;

            form.Text = title;
            label.Text = promptText;

            //label.SetBounds(9, 20, 372, 13);
            label.SetBounds(50, 20, 372, 13);
            label.Parent = form;
            label.AutoSize = true;

            int Max = 0;
            Max = Math.Max(300, label.Right + 10);
            Max = Math.Max(Size, label.Right + 10);


            form.ClientSize = new Size(396 + 50, 107);
            form.ClientSize = new Size(Math.Max(300, Max) + 50, form.ClientSize.Height);

            int Widgth = (form.Width / 2) - (Size / 2);
            int X = 0;

            X = Widgth;
            //for (i = 0; i < sButton.Length; i++)
            //{
            //    Buttonx[i].SetBounds(X, 72, bSize, 23);
            //    X += bSize + 8;
            //    Buttonx[i].Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            //    Buttonx[i].Parent = form;
            //    Buttonx[i].Visible = true;
            //    Buttonx[i].DialogResult = Result[i];
            //    Buttonx[i].Click += new EventHandler(ButtonClick);
            //}

            Buttonx[0].SetBounds(X, 72, Size, 23);
            X += bSize + 8;
            Buttonx[0].Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Buttonx[0].Parent = form;
            Buttonx[0].Visible = true;
            Buttonx[0].DialogResult = DialogResult.OK;
            Buttonx[0].Click += new EventHandler(ButtonClick);

            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ControlBox = false;
            form.TopMost = true;

            if (validation != null)
            {
                form.FormClosing += delegate (object sender, FormClosingEventArgs e)
                {
                    if (form.DialogResult == DialogResult.OK)
                    {
                    }
                    else if (form.DialogResult == DialogResult.No)
                    {
                    }
                    else if (form.DialogResult == DialogResult.Cancel)
                    {
                    }
                    form.Dispose();
                };
            }
            DialogResult dialogResult = form.ShowDialog();
            return dialogResult;
        }

        ~uxMessageBox()
        {

        }

        private static void ButtonClick(object sender, EventArgs e)
        {
            form.Close();
            return;
        }

        private void FormClosing(object sender, FormClosingEventArgs e)
        {
            form.Dispose();
            form = null;
        }
    }
    public delegate string uxMessageBoxValidation(string errorMessage);




    public class COMMON_FUCTION
    {
        public Stopwatch STOP_WATCH = new Stopwatch();
        private List<string> FileList = new List<string>();
        private List<string> DirList = new List<string>();

//        private IntPtr hWnd;
        //uint vncstyles;


        public COMMON_FUCTION()
        {
            STOP_WATCH.Start();
        }

        ~COMMON_FUCTION()
        {
            STOP_WATCH.Stop();
        }



        /*
         컴퓨터를 종료/재부팅/로그오프를 하는방법은 다음과 같습니다.

        기본적으로 Using System.Diagnostics을 사용하며,

        예제코드는 다음과 같습니다.

        종료

        Process.Start("shutdown.exe", "-s"); // 기본적으로 30초 후 종료

        Process.Start("shutdown.exe", "-s -t xx") // xx 초 후 종료



        재부팅

        Process.Start("shutdown.exe","-r"); // 종료과 유사하며 커멘드만 "-r"을 사용

        Process.Start("shutdown.exe","-r -t xx");



        로그오프

        Process.Start("shutdown.exe","-l"); // 위 코드와 유사하며 커멘드만 "-l"(숫자 1이 아닌 소문자'l')을 사용

        원하는 시간에 종료하길 원하신다면, 현재시간을 비교해 종료하는 방법이 있음.


        */

        public void ExternalProgramRun(string name) //외부 프로그램 실행
        {
            Process.Start(name);
            return;
        }

        public void ExternalProgramExit(string name) //외부 프로그램 종료
        {
            string exitname;

            if (name.IndexOf(".exe") < 0)
            {
                exitname = name;
            }
            else
            {
                exitname = name.Substring(0, name.IndexOf(".exe"));
            }
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.StartsWith(exitname))
                {
                    process.Kill();
                }
            }
        }

        public void LogOff()
        {
            Process.Start("shutdown.exe", "-l"); // 위 코드와 유사하며 커멘드만 "-l"(숫자 1이 아닌 소문자'l')을 사용
            return;
        }

        public void WindowRestartToDelay(int Sec)
        {
            string s;

            //s = "shutdown.exe" + "," + "-r -t " + Sec.ToString();
            s = "shutdown.exe" + "-r -t " + Sec.ToString();
            Process.Start(s);
            return;
        }

        public void WindowRestartTo30Secconds()
        {
            Process.Start("shutdown.exe", "-r");
            return;
        }

        public void WindowExit()
        {
            Process.Start("shutdown.exe", "-s -t 0"); // xx 초 후 종료
            return;
        }

        public void WindowExitToDelay(int Sec)
        {
            string s;

            s = "shutdown.exe" + "," + "-s -t " + Sec.ToString();
            Process.Start(s);
            return;
        }

        public void WindowExitTo30Secconds()
        {
            Process.Start("shutdown.exe", "-s");
            return;
        }

        public List<string> GetFileList
        {
            get
            {
                return FileList;
            }
        }

        public List<string> GetDirList
        {
            get
            {
                return DirList;
            }
        }

        public void ReadFileList(string Path, string Ext)
        {
            try
            {
                DirectoryInfo DirInf = new DirectoryInfo(Path);
                if (DirInf.Exists == true) //폴더가 존제하면
                {
                    //FileInfo[] FileInf = DirInf.GetFiles("*.*");
                    FileInfo[] FileInf = DirInf.GetFiles(Ext);

                    FileList.Clear();
                    if (FileInf.Length == 0)
                    {
                        MessageBox.Show("파일이 존재하지 않습니다.");
                    }
                    else
                    {
                        string s = "";
                        for (int i = 0; i < FileInf.Length; i++)
                        {
                            s += FileInf[i].Name.ToString() + Environment.NewLine;

                            FileList.Add(s);
                            s = "";
                        }
                    }
                }
                else
                {
                    FileList.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        public void ReadFileListNotExt(string Path, string Ext)
        {
            try
            {
                DirectoryInfo DirInf = new DirectoryInfo(Path + "\\");
                if (DirInf.Exists == true) //폴더가 존제하면
                {                 
                    FileInfo[] FileInf = DirInf.GetFiles(Ext);

                    FileList.Clear();
                    if (FileInf.Length == 0)
                    {
                        MessageBox.Show("파일이 존재하지 않습니다.");
                    }
                    else
                    {
                        string s = "";
                        for (int i = 0; i < FileInf.Length; i++)
                        {
                            s += FileInf[i].Name.ToString() + Environment.NewLine;
                            string[] t = s.Split('.');
                            FileList.Add(t[0]);
                            s = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public void ReadDirList(string Path)
        {
            try
            {
                DirectoryInfo DirInf = new DirectoryInfo(Path + "\\");
                if (DirInf.Exists == true) //폴더가 존제하면
                {
                    DirectoryInfo[] DirListData = DirInf.GetDirectories();

                    DirList.Clear();
                    if (DirListData.Length == 0)
                    {
                        MessageBox.Show("폴더가 존재하지 않습니다.");
                    }
                    else
                    {
                        string s = "";
                        for (int i = 0; i < DirListData.Length; i++)
                        {
                            s += DirListData[i].Name.ToString();
                            //string[] t = s.Split('.');
                            //DirList.Add(t[0]);
                            string[] t = s.Split('\r');
                            DirList.Add(t[0]);
                            s = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public __Infor__ ReadInfor()
        {
            __Infor__ TInfor = new __Infor__();   

            return TInfor;
        }

        public __Infor__ SaveInfor(__Infor__ TInfor)
        {
            
            return TInfor;
        }

        public int GetToNumInt(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            int value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (int.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0;
                else return value;
            }
            else
            {
                if (int.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0;
                else return value;
            }
        }

        public string GetString(string data = "0", bool ReturnToChar = true, bool ToHexString = true)
        {
            int i;
            StringBuilder sb = new StringBuilder();

            i = 0;
            foreach (char c in data)
            {
                i++;
                // Check for numeric characters (hex in this case).  Add "." and "e" if float,
                // and remove letters.  Include initial space because it is harmless.
                if (ToHexString == true)
                {
                    if (((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'F')) || ((c >= 'a') && (c <= 'f')) || (c == ' ') || (c == '+') || (c == '-'))
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        if (ReturnToChar == true)
                        {
                            if ((c == 'x') || (c == 'X'))
                            {
                                if (2 < i) break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (((c >= '0') && (c <= '9')) || (c == '.') || (c == ' ') || (c == '+') || (c == '-'))
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return sb.ToString();
        }

        public void strcpy(out string target,char[] source)
        {
            target = "";

            foreach (char c in source)
            {
                target = target + c;
            }
            //target = target1;
            return;
        }

        public void strcpy(out char[] target,char[] source)
        {            
            int i;

            target = new char[source.Length];
            i = 0;
            foreach (char c in source)
            {
                target[i++] = c;                
            }
            return;
        }

        public void strcpy(out char[] target, string source)
        {
            int i;

            target = new char[source.Length];
            i = 0;
            foreach (char c in source)
            {
                target[i++] = c;
            }
            return;
        }

        public void strcpy(out byte[] target, string source)
        {
            int i;

            target = new byte[source.Length];
            i = 0;
            foreach (char c in source)
            {
                target[i++] = (byte)c;
            }
            return;
        }

        public float GetToNumFloat(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            float value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (float.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0;
                else return value;
            }
            else
            {
                if (float.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0;
                else return value;
            }
        }

        public short GetToNumShort(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            short value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (short.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0;
                else return value;
            }
            else
            {
                if (short.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0;
                else return value;
            }
        }

        public long GetToNumLong(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            long value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (long.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0;
                else return value;
            }
            else
            {
                if (long.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0;
                else return value;
            }
        }

        public double GetToNumDouble(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            double value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (double.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0;
                else return value;
            }
            else
            {
                if (double.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0;
                else return value;
            }
        }

        public byte GetToNumByte(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            byte value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (byte.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0x00;
                else return value;
            }
            else
            {
                if (byte.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0x00;
                else return value;
            }
        }

        public void GetSystemDate(out string outTime)
        {
            outTime = string.Format(DateTime.Now.ToString("yyyy.mm.dd"));

            return;
        }

        //---------------------------------------------------------------------------


        public void GetSystemTime(out string outTime)
        {
            outTime = string.Format("[" + DateTime.Now.ToString("yyyy.mm.dd") + "-" + DateTime.Now.ToString("HH:mm:ss") + "]");
            return;
        }

        //---------------------------------------------------------------------------
        public bool isnumeric(char s)
        {
            switch (s)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': return true;
            }
            return false;
        }
        //---------------------------------------------------------------------------
        public int StringToHex(char[] Data)
        {
            int Value;
            short i;
            short Length;

            Value = 0;

            Length = (short)Data.Length;

            for (i = 0; i < Length; i++)
            {
                Value |= CharToHex(Data[i]) << (4 * ((Length - 1) - i));
            }
            return Value;
        }
        //---------------------------------------------------------------------------
        public int StringToHex(string Data)
        {
            int Value;
            //short i;
            //short Length;

            //if(0 <= Data.IndexOf("0x"))
            //{
            //    Data = Data.Substring(2);
            //}
            //else if (0 <= Data.IndexOf("0X"))
            //{
            //    Data = Data.Substring(2);
            //}

            //Value = 0;

            //Length = (short)Data.Length;

            //i = 0;
            //foreach (char c in Data)
            //{
            //    Value |= CharToHex(c) << (4 * ((Length - 1) - i));
            //    i++;
            //}

            string s2 = Data.ToUpper();
            var sx = new String(s2.Where(Char.IsLetterOrDigit).ToArray());
            s2 = sx;
            //if (0 <= s2.IndexOf("0x")) s2 = s2.Remove(0, 2);
            if (0 <= s2.IndexOf("0X")) s2 = s2.Remove(0, 2);

            if (int.TryParse(s2, System.Globalization.NumberStyles.HexNumber, null, out Value) == false) Value = 0;
            return Value;
        }
        public short CharToHex(char c)
        {
            //    switch(c)
            //    {
            //        case '0' : return 0x00;
            //        case '1' : return 0x01;
            //        case '2' : return 0x02;
            //        case '3' : return 0x03;
            //        case '4' : return 0x04;
            //        case '5' : return 0x05;
            //        case '6' : return 0x06;
            //        case '7' : return 0x07;
            //        case '8' : return 0x08;
            //        case '9' : return 0x09;
            //        case 'A' : return 0x0a;
            //        case 'B' : return 0x0b;
            //        case 'C' : return 0x0c;
            //        case 'D' : return 0x0d;
            //        case 'E' : return 0x0e;
            //        case 'F' : return 0x0f;
            //        default  : return 0x00;
            //    }
            short Value = 0x00;

            if (('0' <= c) && (c <= '9'))
            {
                Value = (short)(c - '0');
            }
            else if (('A' <= c) && (c <= 'F'))
            {
                Value = (short)(c - 'A');
                Value += 10;
            }

            else if (('a' <= c) && (c <= 'f'))
            {
                Value = (short)(c - 'a');
                Value += 10;
            }
            return Value;
        }
        //------------------------------------------------------------------------------
        public string ToString(char[] Data)
        {
            string s;
            //short i;

            if (Data[0] == 0x00) return "0";

            /*
            for (i = 0; i < Data.Length; i++)
            {
                if (Data[i] == 0x0000) break;
            }

            s = new string(Data).Substring(0, i);
            */
            s = "";
            foreach (char c in Data)
            {
                s = s + c;
            }
            return s;
        }
        //------------------------------------------------------------------------------
        public string ReturnNumData(string Data)
        {
            string s;

            if (Data.Length == 0) return "0";
            s = "";
            foreach (char c in Data)
            {
                if (char.IsNumber(c))
                {
                    s = s + c;
                }
                else {
                    s = "0";
                    return s;
                }
            }
            //s = new string(Data).Substring(0, i);
            //s = Data.ToString().Substring(0,i);

            return s;
        }

        public string[] GetComName(string[] List)
        {
            string[] Name = new string[List.Length];
            int x;

            for (int i = 0; i < Name.Length; i++) Name[i] = "";
            x = 0;
            foreach (string s in List)
            {
                Name[x] = "COM";
                Name[x] = Name[x] + GetNumber(s);
                x++;
            }

            return Name;
        }

        public string GetNumber(string Data)
        {
            string s = "";

            foreach (char c in Data)
            {
                if (isnumeric(c) == true) s = s + c;
            }
            return s;
        }
        //------------------------------------------------------------------------------
        /*
        public void SaveSpec<__Spec__>(string Name, __Spec__ Data)
        {
            try
            {
                FileStream DataStream = new FileStream(Name, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[Marshal.SizeOf(typeof(__Spec__))];
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                //Marshal.StructureToPtr((object)Data, handle.AddrOfPinnedObject(), false);
                Marshal.StructureToPtr(Data, handle.AddrOfPinnedObject(), false);
                DataStream.Write(buffer, 0, Marshal.SizeOf(typeof(__Spec__)));
                handle.Free();
                DataStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        //------------------------------------------------------------------------------
        public __Spec__ ReadSpec<__Spec__>(string Name, __Spec__ Data)
        {
            int Length;
            FileStream DataStream;


            byte[] buffer = new byte[Marshal.SizeOf(typeof(__Spec__))];

            if (File.Exists(Name))
            {
                DataStream = new FileStream(Name, FileMode.Open, FileAccess.Read);
                Length = DataStream.Read(buffer, 0, Marshal.SizeOf(typeof(__Spec__)));
                DataStream.Close();

                if (Marshal.SizeOf(typeof(__Spec__)) == Length)
                {
                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    Data = (__Spec__)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(__Spec__));
                    handle.Free();
                }
            }
            return Data;
        }
        */

        public __Spec__ ReadSpec(string Name)
        {
            __Spec__ TSpec;

            TSpec = new __Spec__();
            //TSpec.Airbag = new __MinMax__();
            //TSpec.BuckleSensor = new __MinMax__();
            //TSpec.Heater = new __MinMax__();
            //TSpec.Heater2 = new __MinMax__();
            //TSpec.Lumber = new __MinMax__();
            //TSpec.PWSwitch = new __MinMax__();
            //TSpec.Vent = new __MinMax__();
            //TSpec.VentHeater = new __MinMax__();
            //TSpec.LHFan = new __MinMax__();
            //TSpec.LHFan = new __MinMax__();
            ////TSpec.NTC = new __MinMax__();
            ////TSpec.BwrSpeed = new __MinMax__();
            //TSpec.SBR = new __MinMax__();

            //TSpec.HeaterCheckPos = new __CurrentCheckPos__();
            //TSpec.HeaterCheckPos2 = new __CurrentCheckPos__();
            //TSpec.Offset = new __Offset__();            
            //TSpec.VentCheckPos = new __CurrentCheckPos__();
            //TSpec.IMS = new __IMS__ ();
            
            TIniFile Ini = new TIniFile(Name);


            //TSpec.LHHeater.Max = Ini.ReadFloat("Cushion", "MAX");
            //TSpec.LHHeater.Min = Ini.ReadFloat("Cushion", "MIN");

            //TSpec.LHFan.Max = Ini.ReadFloat("Back", "MAX");
            //TSpec.LHFan.Min = Ini.ReadFloat("Back", "MIN");

            //TSpec.NTC.Max = Ini.ReadFloat("NTC", "MAX");
            //TSpec.NTC.Min = Ini.ReadFloat("NTC", "MIN");

            //TSpec.BwrSpeed.Max = Ini.ReadFloat("BwrSpeed", "MAX");
            //TSpec.BwrSpeed.Min = Ini.ReadFloat("BwrSpeed", "MIN");


            TSpec.Retractor.Max = Ini.ReadFloat("RETRACTOR", "MAX");
            TSpec.Retractor.Min = Ini.ReadFloat("RETRACTOR", "MIN");

            //TSpec.SBR.Max = Ini.ReadFloat("SBR", "MAX");
            //TSpec.SBR.Min = Ini.ReadFloat("SBR", "MIN");

            //TSpec.BuckleWaring.Max = Ini.ReadFloat("BuckleWaring", "MAX");
            //TSpec.BuckleWaring.Min = Ini.ReadFloat("BuckleWaring", "MIN");

            TSpec.Heater.Max = Ini.ReadFloat("Heater", "MAX");
            TSpec.Heater.Min = Ini.ReadFloat("Heater", "MIN");

            TSpec.SBR.NotLoad = Ini.ReadFloat("SBR", "NOT LOAD");
            TSpec.SBR.Load15Kg = Ini.ReadFloat("SBR", "LAOD15KG");
            TSpec.SBR.Load30Kg = Ini.ReadFloat("SBR", "LAOD30KG");

            //TSpec.Lumber.Max = Ini.ReadFloat("Lumber", "MAX");
            //TSpec.Lumber.Min = Ini.ReadFloat("Lumber", "MIN");

            TSpec.PWSwitch.Max = Ini.ReadFloat("PWSwitchr", "MAX");
            TSpec.PWSwitch.Min = Ini.ReadFloat("PWSwitchr", "MIN");

            TSpec.Vent.Max = Ini.ReadFloat("Vent", "MAX");
            TSpec.Vent.Min = Ini.ReadFloat("Vent", "MIN");

            TSpec.VentHeater.Max = Ini.ReadFloat("VentHeater", "MAX");
            TSpec.VentHeater.Min = Ini.ReadFloat("VentHeater", "MIN");

            TSpec.HeaterCheckPos.Pos = Ini.ReadInteger("HeaterCheckPos", "Pos");
            TSpec.HeaterCheckPos.Time = Ini.ReadInteger("HeaterCheckPos", "Time");

            //TSpec.HeaterCheckPos2.Pos = Ini.ReadInteger("HeaterCheckPos2", "Pos");
            //TSpec.HeaterCheckPos2.Time = Ini.ReadInteger("HeaterCheckPos2", "Time");

            TSpec.VentCheckPos.Pos = Ini.ReadInteger("VentCheckPos", "Pos");
            TSpec.VentCheckPos.Time = Ini.ReadInteger("VentCheckPos", "Time");

            TSpec.Offset.Retractor = Ini.ReadFloat("Offset", "RETRACTOR");
            TSpec.Offset.BuckleSensor = Ini.ReadFloat("Offset", "BuckleSensor");
            TSpec.Offset.Button = Ini.ReadBool("Offset", "Button");
            //TSpec.Offset.CanType = Ini.ReadBool("Offset", "CanType");
            TSpec.Offset.HeaterLH = Ini.ReadFloat("Offset", "HeaterLH");
            TSpec.Offset.HeaterRH = Ini.ReadFloat("Offset", "HeaterRH");
            TSpec.Offset.IGN = Ini.ReadBool("Offset", "IGN");
            TSpec.Offset.Vent = Ini.ReadFloat("Offset", "Vent");
            TSpec.Offset.DrvLamp = Ini.ReadBool("Offset", "DRVLamp");
            TSpec.Offset.AssistLamp = Ini.ReadBool("Offset", "ASSISTLamp");
            //TSpec.Offset.HeaterRear = Ini.ReadFloat("Offset", "REARHEATER");

            //TSpec.IMS.Set = Ini.ReadInteger("IMS", "SET");
            //TSpec.IMS.M1 = Ini.ReadInteger("IMS", "M1");
            //TSpec.IMS.M2 = Ini.ReadInteger("IMS", "M2");
            //TSpec.IMS.M3 = Ini.ReadInteger("IMS", "M3");

            TSpec.SWCheckTime = Ini.ReadInteger("SWCheckTime", "VALUE");
            
            TSpec.CarName = Ini.ReadString("CARNAME", "VALUE");

            TSpec.Volt = Ini.ReadFloat("VOLTATE", "VALUE");
            TSpec.SWOffCurr = Ini.ReadFloat("SWOFF", "VALUE");
            
            return TSpec;
        }

        public void WriteSpec(string Name, __Spec__ TSpec)
        {        
            TIniFile Ini = new TIniFile(Name);

            Ini.WriteFloat("RETRACTOR", "MAX", TSpec.Retractor.Max);
            Ini.WriteFloat("RETRACTOR", "MIN", TSpec.Retractor.Min);

            //Ini.WriteFloat("BuckleWaring", "MAX", TSpec.BuckleWaring.Max);
            //Ini.WriteFloat("BuckleWaring", "MIN", TSpec.BuckleWaring.Min);

            Ini.WriteFloat("Heater", "MAX", TSpec.Heater.Max);
            Ini.WriteFloat("Heater", "MIN", TSpec.Heater.Min);

            //Ini.WriteFloat("Heater2", "MAX", TSpec.Heater2.Max);
            //Ini.WriteFloat("Heater2", "MIN", TSpec.Heater2.Min);

            //Ini.WriteFloat("SBR", "MAX", TSpec.SBR.Max);
            //Ini.WriteFloat("SBR", "MIN", TSpec.SBR.Min);

            //Ini.WriteFloat("Lumber", "MAX", TSpec.Lumber.Max);
            //Ini.WriteFloat("Lumber", "MIN", TSpec.Lumber.Min);

            Ini.WriteFloat("SBR", "NOT LOAD", TSpec.SBR.NotLoad);
            Ini.WriteFloat("SBR", "LAOD15KG", TSpec.SBR.Load15Kg);
            Ini.WriteFloat("SBR", "LAOD30KG", TSpec.SBR.Load30Kg);

            Ini.WriteFloat("PWSwitchr", "MAX", TSpec.PWSwitch.Max);
            Ini.WriteFloat("PWSwitchr", "MIN", TSpec.PWSwitch.Min);

            Ini.WriteFloat("Vent", "MAX", TSpec.Vent.Max);
            Ini.WriteFloat("Vent", "MIN", TSpec.Vent.Min);

            Ini.WriteFloat("VentHeater", "MAX", TSpec.VentHeater.Max);
            Ini.WriteFloat("VentHeater", "MIN", TSpec.VentHeater.Min);

            Ini.WriteInteger("HeaterCheckPos", "Pos", TSpec.HeaterCheckPos.Pos);
            Ini.WriteInteger("HeaterCheckPos", "Time", TSpec.HeaterCheckPos.Time);

            //Ini.WriteInteger("HeaterCheckPos2", "Pos", TSpec.HeaterCheckPos2.Pos);
            //Ini.WriteInteger("HeaterCheckPos2", "Time", TSpec.HeaterCheckPos2.Time);

            Ini.WriteInteger("VentCheckPos", "Pos", TSpec.VentCheckPos.Pos);
            Ini.WriteInteger("VentCheckPos", "Time", TSpec.VentCheckPos.Time);

            Ini.WriteFloat("Offset", "RETRACTOR", TSpec.Offset.Retractor);
            Ini.WriteFloat("Offset", "BuckleSensor", TSpec.Offset.BuckleSensor);
            Ini.WriteBool("Offset", "Button", TSpec.Offset.Button);
            //Ini.WriteBool("Offset", "CanType", TSpec.Offset.CanType);
            Ini.WriteFloat("Offset", "HeaterLH", TSpec.Offset.HeaterLH);
            Ini.WriteFloat("Offset", "HeaterRH", TSpec.Offset.HeaterRH);
            Ini.WriteBool("Offset", "IGN", TSpec.Offset.IGN);
            Ini.WriteFloat("Offset", "Vent", TSpec.Offset.Vent);
            Ini.WriteBool("Offset", "ASSISTLamp", TSpec.Offset.AssistLamp);
            //Ini.WriteFloat("Offset", "REARHEATER", TSpec.Offset.HeaterRear);

            //Ini.WriteInteger("IMS", "SET", TSpec.IMS.Set);
            //Ini.WriteInteger("IMS", "M1", TSpec.IMS.M1);
            //Ini.WriteInteger("IMS", "M2", TSpec.IMS.M2);
            //Ini.WriteInteger("IMS", "M3", TSpec.IMS.M3);

            Ini.WriteInteger("SWCheckTime", "VALUE", TSpec.SWCheckTime);
            
            Ini.WriteString("CARNAME", "VALUE", TSpec.CarName);

            //Ini.WriteFloat("Cushion", "MAX", TSpec.LHHeater.Max);
            //Ini.WriteFloat("Cushion", "MIN", TSpec.LHHeater.Min);

            //Ini.WriteFloat("Back", "MAX", TSpec.LHFan.Max);
            //Ini.WriteFloat("Back", "MIN", TSpec.LHFan.Min);

            //Ini.WriteFloat("NTC", "MAX", TSpec.NTC.Max);
            //Ini.WriteFloat("NTC", "MIN", TSpec.NTC.Min);

            //Ini.WriteFloat("BwrSpeed", "MAX", TSpec.BwrSpeed.Max);
            //Ini.WriteFloat("BwrSpeed", "MIN", TSpec.BwrSpeed.Min);

            Ini.WriteFloat("VOLTATE", "VALUE", TSpec.Volt);
            Ini.WriteFloat("SWOFF", "VALUE", TSpec.SWOffCurr);
            return;
        }


        public void SaveData<__TestData__>(string Name, __TestData__ Data)
        {
            try
            {
                FileStream DataStream;

                if (File.Exists(Name) == false)
                    DataStream = new FileStream(Name, FileMode.Create, FileAccess.Write);
                else DataStream = new FileStream(Name, FileMode.Append, FileAccess.Write);
                byte[] buffer = new byte[Marshal.SizeOf(typeof(__TestData__))];
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                //Marshal.StructureToPtr((object)Data, handle.AddrOfPinnedObject(), false);
                Marshal.StructureToPtr(Data, handle.AddrOfPinnedObject(), false);
                DataStream.Write(buffer, 0, Marshal.SizeOf(typeof(__TestData__)));
                handle.Free();
                DataStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        //------------------------------------------------------------------------------
        public __TestData__ ReadData<__TestData__>(string Name, __TestData__ Data)
        {
            int Length;
            FileStream DataStream;


            byte[] buffer = new byte[Marshal.SizeOf(typeof(__TestData__))];

            if (File.Exists(Name))
            {
                DataStream = new FileStream(Name, FileMode.Open, FileAccess.Read);
                Length = DataStream.Read(buffer, 0, Marshal.SizeOf(typeof(__TestData__)));
                DataStream.Close();

                if (Marshal.SizeOf(typeof(__TestData__)) == Length)
                {
                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    Data = (__TestData__)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(__TestData__));
                    handle.Free();
                }
            }
            return Data;
        }
        //------------------------------------------------------------------------------

        
        public long timeGetTimems()
        {
            return STOP_WATCH.ElapsedMilliseconds;
        }


        public void delay(long time)
        {
            long first;
            long last;

            first = timeGetTimems();
            last = timeGetTimems();
            do
            {
                last = timeGetTimems();
            } while ((last - first) < time);
            return;
        }

        public void timedelay(long time)
        {
            long first;
            long last;

            first = timeGetTimems();
            last = timeGetTimems();
            do
            {
                Application.DoEvents();
                last = timeGetTimems();
            } while ((last - first) < time);
            return;
        }
    }

    //------------------------------------------------------------------------------
    public class Password
    {
        private string Master;
        private string pass;
        private string Path;

        TIniFile Ini;// = new TIniFile(Name);                

        public Password()
        {
            DirectoryInfo Dir = new DirectoryInfo("C:\\Windows\\System32");
            if (Dir.Exists == false) Dir.Create();

            //Path = Main.SYSTEM_PATH.ToString() + "\\pass.ini";            
            Path = Dir.ToString() + "\\Pass.ini";
            Ini = new TIniFile(Path);
            OpenPassword();
        }

        public string SetPassword
        {
            //get { return Item; }
            set { pass = value; }
        }
        public string GetPassword
        {
            get { return pass; }
            //set { Item = value; }
        }

        public string GetMaster
        {
            get { return Master; }
            //set { Item = value; }
        }

        private void OpenPassword()
        {
            pass = Ini.ReadString("PASS", "PASS");
            Master = Ini.ReadString("PASS", "MASTER");
            if (Master == "") Master = "joeun";
        }

        public void SavePassword()
        {
            Ini.WriteString("PASS", "PASS", pass);
            Ini.WriteString("PASS", "MASTER", "joeun");
        }

        ~Password()
        {
        }
        //------------------------------------------------------------------------------
    }

    public class ConfigSetting
    {
        __Config__ xConfig = new __Config__();
        TIniFile Ini;// = new TIniFile(Name);                

        public ConfigSetting()
        {
            //xConfig.Client = new __TCPIP__();
            //xConfig.Server = new __TCPIP__();
            //xConfig.Board = new __TCPIP__();
            //xConfig.PC = new __TCPIP__();

            //xConfig.MultiMeter = new __Port__();
            //xConfig.PanelMeter = new __Port__();
            //xConfig.Scaner = new __Port__();
            //xConfig.Power = new __Port__();

            //xConfig.Can = new __CanPort__();
            //xConfig.Lin = new __CanPort__();

            return;
        }

        public __Config__ ReadWriteConfig
        {
            set { xConfig = value; SaveConfig(); }

            get { OpenConfig(); return xConfig; }
        }


        private void OpenConfig()
        {
            string Path;

            Path = Program.SYSTEM_PATH.ToString() + "\\Config.ini";

            if (File.Exists(Path) == false) return;
            Ini = new TIniFile(Path);

            //if (Ini.ReadString("CLIENT", "IP", ref xConfig.Client.IP) == false) xConfig.Client.IP = "";
            //if (Ini.ReadInteger("CLIENT", "PORT", ref xConfig.Client.Port) == false) xConfig.Client.Port = 0;

            //if (Ini.ReadString("SERVER", "IP", ref xConfig.Server.IP) == false) xConfig.Server.IP = "";
            //if (Ini.ReadInteger("SERVER", "PORT", ref xConfig.Server.Port) == false) xConfig.Server.Port = 0;

            //if (Ini.ReadString("BOARD", "IP", ref xConfig.Board.IP) == false) xConfig.Board.IP = "";
            //if (Ini.ReadInteger("BOARD", "PORT", ref xConfig.Board.Port) == false) xConfig.Board.Port = 0;

            //if (Ini.ReadString("PC", "IP", ref xConfig.PC.IP) == false) xConfig.PC.IP = "";
            //if (Ini.ReadInteger("PC", "PORT", ref xConfig.PC.Port) == false) xConfig.PC.Port = 0;

            ////if (Ini.ReadInteger("MultiMeter", "PORT", ref xConfig.MultiMeter.Port) == false) xConfig.MultiMeter.Port = 0;
            ////if (Ini.ReadInteger("MultiMeter", "BUADRATE", ref xConfig.MultiMeter.Speed) == false) xConfig.MultiMeter.Speed = 0;

            //if (Ini.ReadString("PanelMeter", "PORT", ref xConfig.PanelMeter.Port) == false) xConfig.PanelMeter.Port = string.Empty;
            //if (Ini.ReadInteger("PanelMeter", "BUADRATE", ref xConfig.PanelMeter.Speed) == false) xConfig.PanelMeter.Speed = 0;

            //if (Ini.ReadInteger("PanelMeter", "BattID", ref xConfig.BattID) == false) xConfig.BattID = 0;
            ////if (Ini.ReadInteger("PanelMeter", "PSwitchID", ref xConfig.PSwitchID) == false) xConfig.PSwitchID = 0;
            //if (Ini.ReadInteger("PanelMeter", "HeaterID", ref xConfig.HeaterID) == false) xConfig.HeaterID = 0;
            //if (Ini.ReadInteger("PanelMeter", "BuckleID", ref xConfig.BuckleID) == false) xConfig.BuckleID = 0;

            ////if (Ini.ReadInteger("Scaner", "PORT", ref xConfig.Scaner.Port) == false) xConfig.Scaner.Port = 0;
            ////if (Ini.ReadInteger("Scaner", "BUADRATE", ref xConfig.Scaner.Speed) == false) xConfig.Scaner.Speed = 0;

            //if (Ini.ReadString("Power", "PORT", ref xConfig.Power.Port) == false) xConfig.Power.Port = string.Empty;
            //if (Ini.ReadInteger("Power", "BUADRATE", ref xConfig.Power.Speed) == false) xConfig.Power.Speed = 0;

            ////if (Ini.ReadInteger("CAN", "PORT", ref xConfig.Can.Port) == false) xConfig.Can.Port = 0;
            ////if (Ini.ReadInteger("CAN", "SPEED", ref xConfig.Can.Speed) == false) xConfig.Can.Speed = 0;

            ////if (Ini.ReadInteger("LIN", "PORT", ref xConfig.Lin.Port) == false) xConfig.Lin.Port = 0;
            ////if (Ini.ReadInteger("LIN", "SPEED", ref xConfig.Lin.Speed) == false) xConfig.Lin.Speed = 0;
            
            xConfig.Lin1.Device = Settings.Default.LIN_DEVICE;
            xConfig.Lin1.Speed = Settings.Default.LIN_SPEED;

            xConfig.Lin2.Device = Settings.Default.LIN_DEVICE2;
            xConfig.Lin2.Speed = Settings.Default.LIN_SPEED2;

            xConfig.Can1.Channel = Settings.Default.CAN_CHANNEL;
            xConfig.Can1.Device = Settings.Default.CAN_DEVICE;
            xConfig.Can1.ID = Settings.Default.CAN_ID;
            xConfig.Can1.Speed = Settings.Default.CAN_SPEED;

            //xConfig.Can2.Channel = Settings.Default.CAN_CHANNEL2;
            //xConfig.Can2.Device = Settings.Default.CAN_DEVICE2;
            //xConfig.Can2.ID = Settings.Default.CAN_ID2;
            //xConfig.Can2.Speed = Settings.Default.CAN_SPEED2;

            xConfig.Client.IP = Settings.Default.MES_IP;
            xConfig.Client.Port = Settings.Default.MES_PORT;
            xConfig.Server.IP = Settings.Default.MES_SERVER_IP;
            xConfig.Server.Port = Settings.Default.MES_SERVER_PORT;

            xConfig.PanelMeter.Port = Settings.Default.P_METER_PORT;
            xConfig.PanelMeter.Speed = Settings.Default.P_METER_SPEED;

            xConfig.Board.IP = Settings.Default.IO_BOARD_IP;
            xConfig.Board.Port = Settings.Default.IO_BOARD_PORT;

            xConfig.PC.IP = Settings.Default.PC_IP;
            xConfig.PC.Port = Settings.Default.PC_PORT;


            xConfig.BattID = Settings.Default.BATTID;
            xConfig.BuckleID = Settings.Default.BUCKLEID;
            xConfig.HeaterID = Settings.Default.HEATERID;

            xConfig.Power.Port = Settings.Default.POWER_PORT;
            xConfig.Power.Speed = Settings.Default.POWER_SPEED;

            xConfig.SerialtypeIOCard.Port = Settings.Default.IO_CARD_PORT;
            xConfig.SerialtypeIOCard.Speed = Settings.Default.IO_CARD_SPEED;
            return;
        }

        private void SaveConfig()
        {
            //string Path = Program.SYSTEM_PATH.ToString() + "\\Config.ini";
            //TIniFile Ini = new TIniFile(Path);


            Settings.Default.LIN_DEVICE = xConfig.Lin1.Device;
            Settings.Default.LIN_SPEED = xConfig.Lin1.Speed;

            Settings.Default.LIN_DEVICE2 = xConfig.Lin2.Device;
            Settings.Default.LIN_SPEED2 = xConfig.Lin2.Speed;

            Settings.Default.CAN_CHANNEL = xConfig.Can1.Channel;
            Settings.Default.CAN_DEVICE = xConfig.Can1.Device;
            Settings.Default.CAN_ID = xConfig.Can1.ID;
            Settings.Default.CAN_SPEED = xConfig.Can1.Speed;

            //Settings.Default.CAN_CHANNEL2 = xConfig.Can2.Channel;
            //Settings.Default.CAN_DEVICE2 = xConfig.Can2.Device;
            //Settings.Default.CAN_ID2 = xConfig.Can2.ID;
            //Settings.Default.CAN_SPEED2 = xConfig.Can2.Speed;

            Settings.Default.MES_IP = xConfig.Client.IP;
            Settings.Default.MES_PORT = xConfig.Client.Port;
            Settings.Default.MES_SERVER_IP = xConfig.Server.IP;
            Settings.Default.MES_SERVER_PORT = xConfig.Server.Port;

            Settings.Default.P_METER_PORT = xConfig.PanelMeter.Port;
            Settings.Default.P_METER_SPEED = xConfig.PanelMeter.Speed;

            Settings.Default.IO_BOARD_IP = xConfig.Board.IP;
            Settings.Default.IO_BOARD_PORT = xConfig.Board.Port;

            Settings.Default.PC_IP = xConfig.PC.IP;
            Settings.Default.PC_PORT = xConfig.PC.Port;


            Settings.Default.BATTID = xConfig.BattID;
            Settings.Default.BUCKLEID = xConfig.BuckleID;
            Settings.Default.HEATERID = xConfig.HeaterID;

            Settings.Default.POWER_PORT = xConfig.Power.Port;
            Settings.Default.POWER_SPEED = xConfig.Power.Speed;
            Settings.Default.IO_CARD_PORT = xConfig.SerialtypeIOCard.Port;
            Settings.Default.IO_CARD_SPEED = (short)xConfig.SerialtypeIOCard.Speed;


            //Ini.WriteString("CLIENT", "IP", xConfig.Client.IP);
            //Ini.WriteInteger("CLIENT", "PORT", xConfig.Client.Port);

            //Ini.WriteString("SERVER", "IP", xConfig.Server.IP);
            //Ini.WriteInteger("SERVER", "PORT", xConfig.Server.Port);

            //Ini.WriteString("BOARD", "IP", xConfig.Board.IP);
            //Ini.WriteInteger("BOARD", "PORT", xConfig.Board.Port);

            //Ini.WriteString("PC", "IP", xConfig.PC.IP);
            //Ini.WriteInteger("PC", "PORT", xConfig.PC.Port);

            //Ini.WriteInteger("MultiMeter", "PORT", xConfig.MultiMeter.Port);
            //Ini.WriteInteger("MultiMeter", "BUADRATE", xConfig.MultiMeter.Speed);

            //Ini.WriteInteger("PanelMeter", "BattID", );
            //Ini.WriteInteger("PanelMeter", "PSwitchID", );
            //Ini.WriteInteger("PanelMeter", "HeaterID", );
            //Ini.WriteInteger("PanelMeter", "BuckleID", xConfig.BuckleID);

            //Ini.WriteString("PanelMeter", "PORT", xConfig.PanelMeter.Port);
            //Ini.WriteInteger("PanelMeter", "BUADRATE", xConfig.PanelMeter.Speed);

            //Ini.WriteInteger("Scaner", "PORT", xConfig.Scaner.Port);
            //Ini.WriteInteger("Scaner", "BUADRATE", xConfig.Scaner.Speed);

            //Ini.WriteString("Power", "PORT", xConfig.Power.Port);
            //Ini.WriteInteger("Power", "BUADRATE", xConfig.Power.Speed);

            //Ini.WriteInteger("CAN", "PORT", xConfig.Can.Port);
            //Ini.WriteInteger("CAN", "SPEED", xConfig.Can.Speed);

            //Ini.WriteInteger("LIN", "PORT", xConfig.Lin.Port);
            //Ini.WriteInteger("LIN", "SPEED", xConfig.Lin.Speed);

            Settings.Default.Save();
            return;
        }

        ~ConfigSetting()
        {
        }
    }


    public class PowerControl
    {
        //public string SendString;
        //public char[] SendBuf = new char[1000];
        public bool DCPowerCheckFlag; //extern 

        public char[] POWER_R_Data = new char[200];//extern 
        //private string rxData;
        public short Power_R_Length;//extern 
        private bool Power_R_Flag;//extern 
        private bool ExitFlag = false;
        private Timer pwTimer = new Timer()
        {
            Interval = 50
        };

        //long DCPowerCheckFirst = 0;
        //long DCPowerCheckLast = 0;


        public Stopwatch STOP_WATCH = new Stopwatch();

        private SerialPort PowerPort;

        public PowerControl(__Port__ Ps)
        {
            int Speed;

            try
            {
                Speed = 9600;
                //rxData = "";
                if ((Ps.Port != "") && (Ps.Port != null) && (Ps.Port != string.Empty))
                    PowerPort = new SerialPort(Ps.Port, Speed, Parity.None, 8, StopBits.One);
                else PowerPort = new SerialPort();

                PowerPort.ReadTimeout = 500;
                PowerPort.WriteTimeout = 500;
                PowerPort.ReadBufferSize = 4096;
                PowerPort.WriteBufferSize = 2048;
                PowerPort.DataReceived += new SerialDataReceivedEventHandler(PowerDataCatch);

                if ((Ps.Port != "") && (Ps.Port != null) && (Ps.Port != string.Empty)) PowerPort.Open();
                STOP_WATCH.Start();
                pwTimer.Tick += new EventHandler(pwTimer_Tick);
                pwTimer.Enabled = true;
            }
            catch { }
            return;
        }

        public SerialPort PowerPortOpen
        {
            set
            {
                PowerPort = value;
                pwTimer.Tick += new EventHandler(pwTimer_Tick);
                pwTimer.Enabled = true;
                //rxData = "";
                STOP_WATCH.Start();
            }
        }

        ~PowerControl()
        {
            ExitFlag = true;
            STOP_WATCH.Stop();
        }

        public void Close()
        {
            if (PowerPort != null)
            {
                if (PowerPort.IsOpen == true) PowerPort.Close();
            }
            return;
        }

        private List<string> MsgList = new List<string>();

        private bool Question = false;
        private long QuestionFirst = 0;
        private long QuestionLast = 0;
        private bool MsgSendFlag = false;

        private void pwTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                pwTimer.Enabled = false;

                if (Question == false)
                {
                    if (0 < MsgList.Count)
                    {
                        string SendString = MsgList[0] + "\n";
                        char[] SendBuf = new char[1000];

                        if (0 <= SendString.IndexOf("?"))
                        {
                            Question = true;
                            QuestionFirst = timeGetTimems();
                            QuestionLast = timeGetTimems();
                        }
                        Array.Clear(SendBuf, 0, 1000);
                        Array.Copy(SendString.ToCharArray(), SendBuf, SendString.Length);
                        PowerPort.Write(SendBuf, 0, SendString.Length);
                        MsgList.RemoveAt(0);
                        QuestionFirst = timeGetTimems();
                        QuestionLast = timeGetTimems();
                    }
                    else
                    {
                        QuestionLast = timeGetTimems();
                        if (500 <= (QuestionLast - QuestionFirst))
                        {
                            if (MsgSendFlag == false)
                                PowerReadMessage();
                            else PowerOnOffReadMessage();
                            MsgSendFlag = !MsgSendFlag;
                        }
                    }
                }
                else
                {
                    QuestionLast = timeGetTimems();
                    if (1500 <= (QuestionLast - QuestionFirst))
                    {
                        //1.5초 동안 응답이 없으면 타임 종료로 다시 통신을 제게한다.
                        Question = false;
                    }
                }
            }
            catch
            {

            }
            finally
            {
                pwTimer.Enabled = !ExitFlag;
            }
            return;
        }

        public bool IsOpen
        {
            get
            {
                return PowerPort.IsOpen;
            }
        }

        public long timeGetTimems()
        {
            return STOP_WATCH.ElapsedMilliseconds;
        }

        public void POWER_PWSetting(float Volt)   // Power Setting
        {
            if (PowerPort.IsOpen == false) return;

#if PROGRAM_RUNNING
            if (PowerPort.IsOpen == true) MsgList.Add(string.Format("VOLT {0:00.00}", Volt));
#endif
            return;
        }
        //---------------------------------------------------------------------------
        public void POWER_CURRENTSetting(float Current)   // Current Setting
        {
            if (PowerPort.IsOpen == false) return;
#if PROGRAM_RUNNING
            if (PowerPort.IsOpen == true) MsgList.Add(string.Format("CURR {0:00.00}", Current));
#endif
            return;
        }
        //---------------------------------------------------------------------------

        public void POWER_PWON()   // Power Output ON
        {
            if (PowerPort.IsOpen == false) return;

#if PROGRAM_RUNNING
            if (PowerPort.IsOpen == true) MsgList.Add("Outp ON");
#endif
            return;
        }
        //---------------------------------------------------------------------------
        public void POWER_PWOFF()  // Power Output OFF
        {
            if (PowerPort.IsOpen == false) return;

#if PROGRAM_RUNNING
            if (PowerPort.IsOpen == true) MsgList.Add("Outp OFF");
#endif
            return;
        }
        //---------------------------------------------------------------------------
        public void CHANNELSELECT()   // Power Output ON
        {
            if (PowerPort.IsOpen == false) return;

#if PROGRAM_RUNNING
            if (PowerPort.IsOpen == true) MsgList.Add("INST OUT1");
#endif
            return;
        }
        //---------------------------------------------------------------------------
        public void timedelay(long time)
        {
            long first;
            long last;

            first = timeGetTimems();
            last = timeGetTimems();
            do
            {
                Application.DoEvents();
                last = timeGetTimems();
            } while ((last - first) < time);
            return;
        }
        //---------------------------------------------------------------------------

        public void DCPowerReset()
        {
#if PROGRAM_RUNNING
            DCPowerCheckFlag = false;
            if (PowerPort.IsOpen == true) MsgList.Add("*RST");
#endif
            return;
        }
        //---------------------------------------------------------------------------

        public string CheckPowerSerialNo()
        {
#if PROGRAM_RUNNING
            string s;

            DCPowerCheckFlag = false;
            if (PowerPort.IsOpen == true) MsgList.Add("*SN?");
            s = ReadMessage();
            return s;
#else
            return "";
#endif
        }

        private string ToString(char[] Data)
        {
            string s;
            short i;

            for (i = 0; i < 50; i++)
            {
                if (Data[i] == 0x0000) break;
            }

            s = new string(Data).Substring(0, i);

            return s;
        }
        //---------------------------------------------------------------------------

        public enum SendMsgMode
        {
            VOLT,
            ONOFF
        }

        private bool SendFlag = false;
        private SendMsgMode SendMessageMode = SendMsgMode.VOLT;

        public SendMsgMode GetSendMsg
        {
            get
            {
                return SendMessageMode;
            }
        }

        private float SetVolt = 0;
        private bool PowerOnOff = false;

        public float ReadVolt
        {
            get
            {
                if (PowerOnOff == true)
                    return SetVolt;
                else return 0;
            }
        }

        public bool isPowerOn
        {
            get { return PowerOnOff; }
        }

        public bool GetReadEnd
        {
            get { return !SendFlag; }
            set { SendFlag = !value; }
        }

        public void PowerReadMessage()
        {

#if PROGRAM_RUNNING
            if (PowerPort.IsOpen == true) MsgList.Add("VOLT?");
#endif
            SendFlag = true;
            SendMessageMode = SendMsgMode.VOLT;
            return;
        }

        public void PowerOnOffReadMessage()
        {


#if PROGRAM_RUNNING
            if (PowerPort.IsOpen == true) MsgList.Add("OUTP?");
#endif
            SendFlag = true;
            SendMessageMode = SendMsgMode.ONOFF;
            return;
        }



        void POWER_Read()
        {
            /*
                        try
                        {
#if PROGRAM_RUNNING
                        try
                        {
                            s = ReadMessage();                    
                            return;
                        }
                        else{
                            Data->Flag = false;
                            Data->ReadFlag = false;
                        }

                        catch(...)
                        {
                        }
#endif
                        }
                        finally
                        {
                        }
            */
            return;
        }

        public string ReadMessage()
        {
            //#if PROGRAM_RUNNING	        
            long First;
            long Last;
            bool Flag;
            string s;

            if (PowerPort.IsOpen == false) return "";
            s = "";
            try
            {
                try
                {
                    First = timeGetTimems();
                    Flag = PowerPort.IsOpen;

                    do
                    {
                        if (Flag == true)
                        {
                            if (Power_R_Flag == true)
                            {
                                s = POWER_R_Data.ToString();
                                Power_R_Flag = false;
                                Power_R_Length = 0;
                                Array.Clear(POWER_R_Data, 0, 200);
                                return s;
                            }
                        }
                        else break;
                        Application.DoEvents();
                        Last = timeGetTimems();
                    } while ((Last - First) < 1000);
                }
                catch (Exception Msg)
                {
                    MessageBox.Show(Msg.Message + "\n" + Msg.StackTrace);
                    //uMessageBox.Show(promptText: ex.Message + "\n" + ex.StackTrace);
                }
            }
            finally
            {

            }
            return s;
            //#else
            //            return "";
            //#endif
        }
        //---------------------------------------------------------------------------

        private void PowerDataCatch(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                timedelay(30);
                int Length = PowerPort.BytesToRead;
                byte[] buffer = new byte[Length + 10];

                PowerPort.Read(buffer, 0, Length);

                string[] s = Encoding.Default.GetString(buffer).Split('\n');

                if (Question == true) Question = false;
                if (SendMessageMode == SendMsgMode.VOLT)
                {
                    if (float.TryParse(s[0], out SetVolt) == false) { }
                }
                else
                {
                    PowerOnOff = s[0] == "1" ? true : false;
                }
            }
            catch
            {

            }
            finally
            {
                SendFlag = false;
            }
            return;
        }
    }
}

