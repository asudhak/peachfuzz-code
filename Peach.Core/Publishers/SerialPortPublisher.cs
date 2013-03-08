using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.IO.Ports;
using NLog;

namespace Peach.Core.Publishers
{
    [Publisher("SerialPort", true)]
    [Parameter("PortName", typeof(string), "Com interface for the device to connect to/")]
    [Parameter("Baudrate", typeof(int), "The serial baud rate.")]
    [Parameter("Parity", typeof(Parity), "The parity-checking protocol.")]
    [Parameter("DataBits", typeof(int), "Standard length of data bits per byte.")]
    [Parameter("StopBits", typeof(StopBits), "The standard number of stopbits per byte.")]
    [Parameter("Handshake", typeof(Handshake), "The handshaking protocol for serial port transmission of data.", "None")]
    [Parameter("Timeout", typeof(int), "How many milliseconds to wait for data (default 3000)", "3000")]
    public class SerialPortPublisher : BufferedStreamPublisher
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        protected override NLog.Logger Logger { get { return logger; } }

        public string PortName { get; protected set; }
        public int Baudrate { get; protected set; }
        public Parity Parity { get; protected set; }
        public int DataBits { get; protected set; }
        public StopBits StopBits { get; protected set; }
        public Handshake Handshake { get; protected set; }

        protected SerialPort _serial;

        public SerialPortPublisher(Dictionary<string, Variant> args)
            : base(args)
        {
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            try
            {
                _serial = new SerialPort(PortName, Baudrate, Parity, DataBits, StopBits);
                _serial.Handshake = Handshake;
                _serial.Open();
                _clientName = _serial.PortName;
                _client = _serial.BaseStream;
            }
            catch (Exception ex)
            {
                string msg = "Unable to open Serial Port {0}. {1}.".Fmt(PortName, ex.Message);
                Logger.Error(msg);
                throw new PeachException(msg, ex);
            }
           
            StartClient();
        }
    }
}
