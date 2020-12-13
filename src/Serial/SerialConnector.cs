using System.IO.Ports;
using System.Collections.Generic;
using System;
using System.Threading;

namespace DSController.Serial
{
    class SerialConnector
    {
        protected SerialPort Port;

        public const int BAUD_RATE = 9600;
        public const string VERSION = "1.0";
        public const int MODE_NORMAL = 1;
        public const int MODE_PROBE = 2;

        public string PortName { get; protected set; }
        public string LastError { get; protected set; }

        public SerialConnector()
        {
            this.LastError = "";
        }

        public bool Open(string portName, string controllerCode = null)
        {
            this.PortName = portName;
            this.Port = new SerialPort(this.PortName);
            this.Port.BaudRate = BAUD_RATE;
            this.Port.ReadTimeout = 5000;
            this.Port.WriteTimeout = 2000;
            this.Port.NewLine = "\n";
            this.Port.RtsEnable = true;

            try {
                this.Port.Open();
            } catch(UnauthorizedAccessException) {
                this.LastError = "Unable to open serial port.";
                return false;
            }

            bool handshakeSuccess = this.PerformHandshake();
            if(handshakeSuccess) {
                this.SendCommand("MOD " + SerialConnector.MODE_NORMAL);
            }

            if(controllerCode != null) {
                this.SendCommand("CCD " + controllerCode);
            }

            return handshakeSuccess;
        }

        public void Clean()
        {
            this.Port.ReadExisting();
        }

        protected bool PerformHandshake()
        {
            string response = null;

            this.Clean();
            this.SendCommand("VER");
            response = this.WaitReply();

            if(response == null) {
                this.LastError = "Handshake timeout";
                return false;
            }

            if(response != VERSION) {
                this.LastError = "Protocol version mismatch ({0} != {1}). Please update the hardware.";
                this.LastError = string.Format(this.LastError, response, VERSION);
                return false;
            }

            return true;
        }

        public void ChangeMode(int mode)
        {
            if(mode == SerialConnector.MODE_NORMAL) {
                this.Port.ReadTimeout = 5000;
            } else if(mode == SerialConnector.MODE_PROBE) {
                this.Port.ReadTimeout = -1;
            }

            this.SendCommand("MOD " + mode.ToString());
        }

        public string WaitReply()
        {
            try {
                return this.Port.ReadLine();
            } catch(TimeoutException) {
                return null;
            }
        }

        public int WaitByte()
        {
            try {
                return this.Port.ReadByte();
            } catch(TimeoutException) {
                return -1;
            }
        }

        public void SendCommand(string command)
        {
            this.Port.Write(command + "\n");
        }

        public void SendKey(string key, bool pressed)
        {
            int pressedInt = pressed ? 1 : 0;
            this.SendCommand(string.Format("KEY {0} {1}", this.KeyCharCode(key), pressedInt));
        }

        public List<string> GetAvailablePorts()
        {
            List<string> ports = new List<string>();

            foreach(string portName in SerialPort.GetPortNames()) {
                ports.Add(portName);
            }

            return ports;
        }

        protected string KeyCharCode(string key)
        {
            // TODO: Find a better way to do that ? constant structure ?
            switch(key) {
                case "Up": key = "U"; break;
                case "Down": key = "D"; break;
                case "Left": key = "E"; break;
                case "Right": key = "I"; break;
                case "Start": key = "T"; break;
                case "Select": key = "C"; break;
            }

            return key;
        }
    }
}