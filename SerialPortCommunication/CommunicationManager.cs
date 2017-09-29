using System;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using System.Collections.Generic;
namespace PCComm
{
    class CommunicationManager
    {
        #region Manager Enums
        public enum MessageType { Incoming, Outgoing, Normal};
        #endregion

        #region Manager Variables
        //property variables
        private string _baudRate = string.Empty;
        private string _parity = string.Empty;
        private string _stopBits = string.Empty;
        private string _dataBits = string.Empty;
        private string _portName = string.Empty;
        private RichTextBox _displayWindow;
        //global manager variables
        private Color[] MessageColor = { Color.Blue, Color.Green, Color.Black };
        private SerialPort comPort = new SerialPort();
        #endregion

        #region Manager Properties
        public string BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        public string Parity
        {
            get { return _parity; }
            set { _parity = value; }
        }

        public string PortName
        {
            get { return _portName; }
            set { _portName = value; }
        }

        public RichTextBox DisplayWindow
        {
            get { return _displayWindow; }
            set { _displayWindow = value; }
        }
        #endregion

        #region Manager Constructors

        public CommunicationManager()
        {
            _baudRate = string.Empty;
            _parity = string.Empty;
            _stopBits = string.Empty;
            _dataBits = string.Empty;
            _portName = string.Empty;
            _displayWindow = null;
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
        }
        #endregion

        #region WriteData
        public void WriteData(string msg)
        {
            int temp = Convert.ToInt32(comPort.PortName[comPort.PortName.Length - 1]);
            byte destination = 0;
            byte source = 0;
            if (temp == 1){
                source = 1;
                destination = 2;
            }
            else
            {
                source = 2;
                destination = 1;
            }
            var sendPackage = new List<Byte>();
            sendPackage.Add(126);
            if (!(comPort.IsOpen == true)) comPort.Open();
            byte[] msgArray = Encoding.ASCII.GetBytes(msg);
            foreach(Byte item in msgArray){
                if(item == 126){
                    sendPackage.Add(125);
                    sendPackage.Add(94);
                    continue;
                }
                if(item == 125){
                    sendPackage.Add(125);
                    sendPackage.Add(125);
                    continue;
                }
                sendPackage.Add(item);
            }
            sendPackage.Add(destination);
            sendPackage.Add(source);
            sendPackage.Add(126);
            Console.WriteLine(msgArray);
            msgArray = sendPackage.ToArray();
            comPort.Write(msgArray, 0, msgArray.Length);
            DisplayData(MessageType.Outgoing, msg + "\n");
            Console.Read();
        }
        #endregion

        #region DisplayData
        [STAThread]
        private void DisplayData(MessageType type, string msg)
        {
            _displayWindow.Invoke(new EventHandler(delegate
            {
                _displayWindow.SelectedText = string.Empty;
                _displayWindow.SelectionFont = new Font(_displayWindow.SelectionFont, FontStyle.Bold);
                _displayWindow.SelectionColor = MessageColor[(int)type];
                _displayWindow.AppendText(msg);
                _displayWindow.ScrollToCaret();
            }));
        }
        #endregion

        #region OpenPort
        public bool OpenPort()
        {
            try
            {
                if (comPort.IsOpen == true) comPort.Close();
                comPort.PortName = _portName; 
                comPort.BaudRate = int.Parse(_baudRate); 
                comPort.DataBits = 8;
                comPort.StopBits = (StopBits)1;
                comPort.Parity = (Parity)Enum.Parse(typeof(Parity), _parity);
                comPort.Open();
                DisplayData(MessageType.Normal, "Port opened at " + DateTime.Now + "\n");
                return true;
            }
            catch (Exception ex)
            {
                DisplayData(MessageType.Normal, ex.Message);
                return false;
            }
        }
        #endregion

        #region SetParityValues
        public void SetParityValues(object obj)
        {
            foreach (string str in Enum.GetNames(typeof(Parity)))
            {
                ((ComboBox)obj).Items.Add(str);
            }
        }
        #endregion

        #region SetPortNameValues
        public void SetPortValues(object obj)
        {

            foreach (string str in SerialPort.GetPortNames())
            {
                ((ComboBox)obj).Items.Add(str);
            }
        }
        #endregion

        #region comPort_DataReceived
        void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] list = new byte[comPort.BytesToRead];
            comPort.Read(list, 0, comPort.BytesToRead);
            if (list[0] == 126 && list[list.Length - 1] == 126)
            {
                int temp = Convert.ToInt32(comPort.PortName[comPort.PortName.Length - 1]);
                byte source = 0;
                if (temp == 1)
                    source = 1;
                else
                    source = 2;
                byte tempSrc = list[list.Length - 2];
                if (source == tempSrc)
                {
                    var tempList = new List<Byte>();
                    for (int i = 1; i < list.Length - 3; i++)
                    {
                        if (list[i] == 125 && list[i + 1] == 94)
                        {
                            tempList.Add(126);
                            i += 1;
                            continue;
                        }
                        if (list[i] == 125 && list[i + 1] == 125)
                        {
                            tempList.Add(125);
                            i += 1;
                            continue;
                        }
                        tempList.Add(list[i]);
                    }
                    string message = Encoding.UTF8.GetString(tempList.ToArray());
                    DisplayData(MessageType.Incoming, message + "\n");
                }
                else
                {
                    DisplayData(MessageType.Normal, "Invalid access.\n");
                }
            }
            else
            {
                DisplayData(MessageType.Normal, "Error in package.\n");
            }
        }
        #endregion
    }
}
