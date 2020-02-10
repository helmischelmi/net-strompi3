using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace Strompi3Lib
{
    /// <summary>
    /// parameters for serial communication
    /// </summary>
    public class SerialPortParameter
    {
        public string PortName { get; private set; }
        public int BaudRate { get; private set; }
        public Parity Parity { get; private set; }
        public int DataBits { get; private set; }
        public StopBits StopBits { get; private set; }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
        /// </summary>
        public int ReadTimeout { get; private set; }  // in milliseconds
        public int WriteTimeout { get; private set; }


        public SerialPortParameter(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, int readTimeout, int writeTimeout)
        {
            PortName = portName;
            BaudRate = baudRate;
            Parity = parity;
            DataBits = dataBits;
            StopBits = stopBits;

            ReadTimeout = readTimeout;
            WriteTimeout = writeTimeout;
        }

        public SerialPort InitializeSerialPort()
        {
            SerialPort result;
            try
            {
                result = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits)
                {
                    ReadTimeout = ReadTimeout,
                    WriteTimeout = WriteTimeout
                };

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Create Serial Interface failed: ", e);
                this.ToString();
                throw;
            }
        }

        public string ShowStatus()
        {
            return
                $"Port-Name '{PortName}', Baud: {BaudRate}, Data: {DataBits}, Stopbits: {StopBits}, Parity: {Parity} {Environment.NewLine}" +
                $"- Timeout, Read:{ReadTimeout} msec, Write: {WriteTimeout} msec, Buffer in Byte: Read (Default): 4096, Write (Default): 2048 {Environment.NewLine}";
        }


        public override string ToString()
        {
            return $"Serial Port: {Environment.NewLine}" +
                   $"- Portname: {PortName}{Environment.NewLine}" +
                   $"- Baudrate: {BaudRate}{Environment.NewLine}" +
                   $"- Parity: {Parity}{Environment.NewLine}" +
                   $"- Databits: {DataBits}{Environment.NewLine}" +
                   $"- Stopbits: {StopBits}{Environment.NewLine}" +
                   $"- Read timeout:{ReadTimeout} msec{Environment.NewLine}" +
                   $"- Write timeout: {WriteTimeout} msec{Environment.NewLine}";
        }
    }
}
