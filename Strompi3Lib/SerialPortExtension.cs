using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace Strompi3Lib
{
    public static class SerialPortExtension
    {

        public static SerialPort Create(this SerialPort port, SerialPortParameter param)
        {
            SerialPort result;
            try
            {
                result = new SerialPort(param.PortName, param.BaudRate, param.Parity, param.DataBits, param.StopBits)
                {
                    ReadTimeout = param.ReadTimeout,
                    WriteTimeout = param.WriteTimeout
                };

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Create Serial Interface failed: ", e);
                Console.WriteLine(param.ToString());
                throw;
            }
        }

        public static string ShowStatusExtended(this SerialPort port)
        {
            var strEol = String.Empty;
            foreach (byte b in Encoding.UTF8.GetBytes(port.NewLine.ToCharArray()))
                strEol += b.ToString();

            string result = $"---------------------------------------------------------------------------------------------------{Environment.NewLine}" +
                            $"Handshake:{port.Handshake}, Connection broken: {port.BreakState}, Carrier-Detect-line: {port.CDHolding},{Environment.NewLine}" +
                            $"Clear-to-Send-Line (CTS): {port.CtsHolding}, Data Set Ready-Signal (DSR) was sent: {port.DsrHolding}, {Environment.NewLine}" +
                            $" Data Terminal Ready (DTR): {port.DtrEnable},  Request to Transmit (RTS): {port.RtsEnable}, ASCII-Wert for eol (Default \\n in C#):'{strEol}', {Environment.NewLine}" +
                            $"Bytes im Empfangspuffer: {port.BytesToRead}, im Sendepuffer: {port.BytesToWrite}{Environment.NewLine}" +
                            $"---------------------------------------------------------------------------------------------------{Environment.NewLine}";
            return result;
        }
    }
}
