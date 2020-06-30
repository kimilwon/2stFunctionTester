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
using SLABHIDTOUART_DLL;
using System.Linq;

namespace PowerSeat통합검사기
{
    public class UsbMultiMeterControl
    {
        private const short HEX_EDIT_LIMIT_SIZE = 4096;
        private const short READ_TIMER_ID = 1;
        private const short READ_TIMER_ELAPSE = 50;
        private const uint READ_TIMEOUT = 0;
        private const uint WRITE_TIMEOUT = 2000;
        private const ushort READ_SIZE = 1000;

        Timer timer1 = new Timer();

        public UsbMultiMeterControl()
        {
            UpdateDeviceList();

            timer1.Interval = 10;
            timer1.Tick += timer1_Tick;
        }

        private bool OpenFlag = false;

        public bool Open()
        {
            Uartserial = HidDeviceString[0];
            bool Flag = Connect(HidDeviceString[0], 9600, SLABHIDTOUART.HID_UART_EIGHT_DATA_BITS, SLABHIDTOUART.HID_UART_NO_PARITY, SLABHIDTOUART.HID_UART_SHORT_STOP_BIT, SLABHIDTOUART.HID_UART_NO_FLOW_CONTROL);

            OpenFlag = Flag;

            if(OpenFlag == true) timer1.Enabled = true;
            return Flag;
        }
        private ushort VID = 0;
        private ushort PID = 0;
        private IntPtr m_hidUart = IntPtr.Zero;
        private string Uartserial = null;

        private bool Connect(string serial, uint baudRate, byte dataBits, byte parity, byte stopBits, byte flowControl)
        {
            int status = SLABHIDTOUART.HID_UART_DEVICE_NOT_FOUND;
            uint numDevices = 0;

            byte m_partNumber = 0x00;
            byte m_version = 0x00;

            if (SLABHIDTOUART.HidUart_GetNumDevices(ref numDevices, VID, PID) == SLABHIDTOUART.HID_UART_SUCCESS)
            {
                for (uint i = 0; i < numDevices; i++)
                {
                    //StringBuilder deviceString = null;
                    //string deviceString = null;

                    // Search through all HID devices for a matching serial string
                    StringBuilder deviceString = new StringBuilder(SLABHIDTOUART.HID_UART_DEVICE_STRLEN);

                    int result = SLABHIDTOUART.HidUart_GetString(i, VID, PID, deviceString, SLABHIDTOUART.HID_UART_GET_SERIAL_STR);
                    if (result == SLABHIDTOUART.HID_UART_SUCCESS)
                    {
                        // Found a matching device
                        if (serial == deviceString.ToString())
                        {
                            // Open the device
                            status = SLABHIDTOUART.HidUart_Open(ref m_hidUart, i, VID, PID);
                            break;
                        }
                    }
                }
            }

            // Found and opened the device
            if (status == SLABHIDTOUART.HID_UART_SUCCESS)
            {
                // Get part number and version

                status = SLABHIDTOUART.HidUart_GetPartNumber(m_hidUart, ref m_partNumber, ref m_version);
            }

            // Got part number
            if (status == SLABHIDTOUART.HID_UART_SUCCESS)
            {
                // Configure the UART
                status = SLABHIDTOUART.HidUart_SetUartConfig(m_hidUart, baudRate, dataBits, parity, stopBits, flowControl);
            }

            // Confirm UART settings
            if (status == SLABHIDTOUART.HID_UART_SUCCESS)
            {
                uint vBaudRate = 0;
                byte vDataBits = 0x00;
                byte vParity = 0x00;
                byte vStopBits = 0x00;
                byte vFlowControl = 0x00;

                status = SLABHIDTOUART.HidUart_GetUartConfig(m_hidUart, ref vBaudRate, ref vDataBits, ref vParity, ref vStopBits, ref vFlowControl);

                if (status == SLABHIDTOUART.HID_UART_SUCCESS)
                {
                    if (vBaudRate != baudRate ||
                        vDataBits != dataBits ||
                        vParity != parity ||
                        vStopBits != stopBits ||
                        vFlowControl != flowControl)
                    {
                        status = SLABHIDTOUART.HID_UART_INVALID_PARAMETER;
                    }
                }
            }

            // Configured the UART
            if (status == SLABHIDTOUART.HID_UART_SUCCESS)
            {
                // Set short read timeouts for periodic read timer
                // Set longer write timeouts for user transmits

                status = SLABHIDTOUART.HidUart_SetTimeouts(m_hidUart, READ_TIMEOUT, WRITE_TIMEOUT);
            }

            // Fully connected to the device
            if (status == SLABHIDTOUART.HID_UART_SUCCESS)
            {
                // Output the connection status to the status bar
                //string statusMsg;

                //statusMsg = string.Format("Connected to {0:%s}", serial);
                //MessageBox.Show(statusMsg);
            }
            // Connect failed

            // Return TRUE if the device was opened successfully
            return (status == SLABHIDTOUART.HID_UART_SUCCESS);
        }

        public void useSynchronousOperation(string devicePath)
        {

            ////create a handle to the device by calling the constructor of the HID class
            ////This can be done using either the VID/PID/Serialnumber, or the device path (string) 
            ////all of these details are available from the HIDDevice.interfaceDetails[] struct array created above
            ////The "false" boolean in the constructor tells the class we only want synchronous operation
            //UsbPort = new HIDDevice(devicePath, false);

            ////OR, the normal usage when you know the VID and PID of the device
            ////HIDDevice device = new HIDDevice(VID, PID, (ushort)SN, false);

            ////Write some data to the device (the write method throws an exception if the data is longer than the report length
            ////specified by the device, this length can be found in the HIDDevice.interfaceDetails struct)
            //byte[] Open1 = { 0x07, 0xab, 0xcd, 0x04, 0x58, 0x00, 0x01, 0xd4 };
            //byte[] Open2 = { 0x07, 0xab, 0xcd, 0x04, 0x5a, 0x00, 0x01, 0xd6 };
            //UsbPort.write(Open1);    //Its that easy!!
            //UsbPort.write(Open2);    //Its that easy!!

            //////Read some data synchronously from the device. This method blocks the calling thread until the data
            //////is returned. This takes 1-20ms for most HID devices

            ////byte[] readData = device.read();    //again, that easy!

            //////close the device to release all handles etc

            ////device.close();
        }



        public void startAsyncOperation(string devicePath)
        {
            ////create a handle to the device by calling the constructor of the HID class
            ////This can be done using either the VID/PID/Serialnumber, or the device path (string) 
            ////all of these details are available from the HIDDevice.interfaceDetails[] struct array created above
            ////The "true" boolean in the constructor tells the class we want asynchronous operation this time
            //UsbPort = new HIDDevice(devicePath, true);

            ////OR, the normal usage when you know the VID and PID of the device
            ////HIDDevice device = new HIDDevice(VID, PID, (ushort)SN, true);

            ////next create the event handler for the incoming reports
            //UsbPort.dataReceived += new HIDDevice.dataReceivedEvent(device_dataReceived);

            //byte[] Open1 = { 0x07, 0xab, 0xcd, 0x04, 0x58, 0x00, 0x01, 0xd4 };
            //byte[] Open2 = { 0x07, 0xab, 0xcd, 0x04, 0x5a, 0x00, 0x01, 0xd6 };
            //UsbPort.write(Open1);    //Its that easy!!
            //Thread.Sleep(1000);
            //UsbPort.write(Open2);    //Its that easy!!

            return;
        }

        //Whenever a report comes in, this method will be called and the data will be available! Like magic...
        void device_dataReceived(byte[] message)
        {
            //Do something with the data here...
            //byte[] readData = UsbPort.read();    //again, that easy!
        }
        public bool isOpen
        {
            get { return OpenFlag; }
        }

        public void Close()
        {
            int status = SLABHIDTOUART.HID_UART_DEVICE_NOT_FOUND;

            status = SLABHIDTOUART.HidUart_Close(m_hidUart);

            // Disconnect failed
            if (status != SLABHIDTOUART.HID_UART_SUCCESS)
            {
                // Notify the user that an error occurred
                //string msg;
                //msg = string.Format("Failed to disconnect: {0:%s}", SLABHIDTOUART.GetHidUartStatusStr(status));
                //MessageBox.Show(msg);
            }


            // Return TRUE if the device was closed successfully
            bool Flag = (status == SLABHIDTOUART.HID_UART_SUCCESS);
            OpenFlag = Flag;
            timer1.Enabled = false;
            return;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                timer1.Enabled = false;
                ReceiveData();
            }
            catch { }
            finally { timer1.Enabled = OpenFlag; }
            return;
        }

        private double ReadData = 0;

        public double GetReadData
        {
            get
            {
                return ReadData;
            }
        }
        private List<string> HidDeviceString = new List<string>();

        private void UpdateDeviceList()
        {
            uint numDevices = 0;

            HidDeviceString.Clear();
            //bool Contect = false;

            StringBuilder deviceString = new StringBuilder(SLABHIDTOUART.HID_UART_DEVICE_STRLEN);
            if (SLABHIDTOUART.HidUart_GetNumDevices(ref numDevices, VID, PID) == SLABHIDTOUART.HID_UART_SUCCESS)
            {
                for (uint i = 0; i < numDevices; i++)
                {
                    int result = SLABHIDTOUART.HidUart_GetString(i, VID, PID, deviceString, SLABHIDTOUART.HID_UART_GET_SERIAL_STR);
                    if (result == SLABHIDTOUART.HID_UART_SUCCESS)
                    {
                        HidDeviceString.Add(deviceString.ToString());
                    }
                }
            }
            return;
        }

        private byte[] DataBuffer = new byte[255];
        private int BufLength = 0;
        private void ReceiveData()
        {
            int status;
            uint numBytesToRead = READ_SIZE;
            uint numBytesRead = 0;
            byte[] buffer = new byte[numBytesToRead];

            // Receive UART data from the device (up to 1000 bytes)
            status = SLABHIDTOUART.HidUart_Read(m_hidUart, buffer, numBytesToRead, ref numBytesRead);

            // HidUart_Read returns HID_UART_SUCCESS if numBytesRead == numBytesToRead
            // and returns HID_UART_READ_TIMED_OUT if numBytesRead < numBytesToRead
            if (status == SLABHIDTOUART.HID_UART_SUCCESS || status == SLABHIDTOUART.HID_UART_READ_TIMED_OUT)
            {
                // Output received data to the receive window
                if (numBytesRead > 0)
                {
                    //m_hexEditReceive.AppendWindowHex(buffer, numBytesRead);
                    //string s = Encoding.Default.GetString(buffer, 0, (int)numBytesRead);

                    //Data += s;

                    if (0 < BufLength)
                        Array.Copy(buffer, 0, DataBuffer, BufLength - 1, (int)numBytesRead);
                    else Array.Copy(buffer, 0, DataBuffer, BufLength, (int)numBytesRead);

                    BufLength += (int)numBytesRead;

                    if (40 <= BufLength)
                    {
                        for (int i = 0; i < BufLength; i++)
                        {
                            if (DataBuffer[i] == 0xab)
                            {
                                if ((i + 1) < BufLength)
                                {
                                    if (DataBuffer[i + 1] == 0xcd)
                                    {
                                        if ((i + 21) <= BufLength)
                                        {
                                            byte[] Data = new byte[22];

                                            Array.Copy(DataBuffer, i, Data, 0, 21);

                                            if ((i + 21) < BufLength)
                                            {
                                                Array.Copy(DataBuffer, i + 21, DataBuffer, 0, BufLength - (i + 21));
                                                BufLength -= i + 21;
                                            }


                                            byte Mode = Data[5];
                                            int Pos = IndexOf(Data, 0, 0x08) + 2;
                                            int Length;
                                            string Pow2 = "";

                                            //< 부호가 있는지
                                            bool Flag = Data.Contains((byte)0x3c);

                                            if (Flag == true)
                                            {
                                                Length = IndexOf(Data, 0, 0x3c);

                                                int End = IndexOf(Data, Length + 1, 0x04);

                                                byte[] Pow = new byte[5];

                                                if (End != -1)
                                                {
                                                    Array.Copy(Data, Length + 1, Pow, 0, End - (Length + 1));
                                                    Pow2 = Encoding.Default.GetString(Pow);
                                                }
                                                else
                                                {
                                                    Pow2 = "00";
                                                }

                                                //if (int.TryParse(Pow2, out xPow) == false) xPow = 0;
                                            }
                                            else
                                            {
                                                Length = IndexOf(Data, 0, 0x04);
                                            }

                                            //for (int j = Pos + 1;j < (i + 21);j++)
                                            //{

                                            //}

                                            string s = "";

                                            if (Flag == true)
                                                s = Encoding.Default.GetString(DataBuffer, Pos, (Length - Pos) - 1);
                                            else s = Encoding.Default.GetString(DataBuffer, Pos, Length - Pos);
                                            string 승수 = "0";

                                            switch (Mode)
                                            {
                                                case 0x30: break;
                                                case 0x31: 승수 = "10" + Pow2; break;
                                                case 0x32: 승수 = "100" + Pow2; break;
                                                case 0x33: 승수 = "1000" + Pow2; break;
                                                case 0x34: 승수 = "10000" + Pow2; break;
                                                case 0x35: 승수 = "100000" + Pow2; break;
                                            }

                                            double Value = 0;
                                            double JisuValue = 0;

                                            if (double.TryParse(승수, out JisuValue) == false) JisuValue = 0;
                                            if (double.TryParse(s, out Value) == false) Value = 0;

                                            if (0 < JisuValue)
                                                ReadData = Value * JisuValue;
                                            else ReadData = Value;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    return;
                }
            }

            return;
        }

        public int IndexOf(byte[] Data, int StartIndex, byte Target)
        {
            int Pos = -1;

            for (int i = StartIndex; i < Data.Length; i++)
            {
                if (Data[i] == Target) { Pos = i; break; }
            }


            return Pos;
        }
        ~UsbMultiMeterControl()
        {

        }
    }
}