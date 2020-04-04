using System;
using System.IO.Ports;
using System.Threading;

namespace Strompi3Lib
{
    public class StromPi3 : IDisposable
    {
        public SerialPort Port { get; set; }
        public StromPi3Settings Settings { get; }
        public UpsMonitor Monitor { get; }


        public StromPi3(bool bSilent = false)
        {
            Settings = new StromPi3Settings();
            Connect(bSilent);
            Port.ReceiveConfiguration(Settings);
            Monitor = new UpsMonitor(this);
        }

        /// <summary>
        /// Configures and connects the serial interface of StromPi3
        /// </summary>
        public void Connect(bool bSilent = false)
        {
            int baudRate = 38400;
            string portName = @"/dev/serial0";
            int dataBits = 8;
            StopBits stopBits = StopBits.One;
            Parity parity = Parity.None;

            // Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
            int readTimeout = 200;
            int writeTimeout = 500;

            var portParameter = new SerialPortConfigurator(portName, baudRate, parity, dataBits, stopBits, readTimeout, writeTimeout);

            if (Port == null)
            {
                Port = Port.GetInstance(portParameter);
            }

            if (!bSilent) Console.WriteLine($"connected to {portParameter}");

            if (Port.IsOpen) Port.Close();

            //_serialPort.DataReceived += ReceivedData;

            Port.Open();

            if (!bSilent) Console.WriteLine($"serial port is open ");
        }


 
        /// <summary>
        /// command to shutdown the Strompi3, in case a second power-source is enabled.
        ///<para>
        /// <remarks>Requires serial-mode</remarks></para>
        /// </summary>
        public void Shutdown()
        {
            if (Port.IsOpen) Port.Close();
            Port.Open();
            Port.Write("quit");
            Port.Write("\r");
            Port.Write("poweroff");
            Port.Write("\r");
            Port.Close();
        }


        private void ReceivedData(object sender, SerialDataReceivedEventArgs e)
        {
            var s = e.EventType.ToString();

            string serialOutput = Port.ReadExisting();
            string serialOutline = Port.ReadLine();

            Console.WriteLine($"Received: Eventtype {s}, ReadExisting {serialOutput}, Readline {serialOutline}");
        }

       

        /// <summary>
        /// Compares the actual SystemTime of the Raspberry Pi with the RTC of StromPi3.
        /// In the case of a deviation, the more recent time is adopted.
        /// <para>Requires serial-mode</para>
        /// <remarks>The functionality of RTCSerial.py from joy-it is ported by this method.The original py-script
        /// uses commands 'Q', '\r', 'date-rpi' and 'time-rpi' to read the current datetime
        /// of Strompi3. This steps could not be implemented successfully and were replaced by calling 'ReadStatus'.
        /// </remarks>
        /// </summary>
        public void SyncRTC()
        {
            // workaround to get the current settings,
            // because commands '"date-rpi' and '"time-rpi' don't work (produce timeout..) until now
            Port.ReceiveConfiguration(Settings);

            if (Port.IsOpen) Port.Close();
            Port.Open();

            Console.WriteLine("TimeSync-Process | Please Wait");
            Console.WriteLine($"StromPi3: Current dateTime {Settings.CurrentDateTime} ");
            var rpiDateTime = DateTime.Now;
            Console.WriteLine($"Raspi: Current dateTime {rpiDateTime} ");

            if (rpiDateTime > Settings.CurrentDateTime) // sync the Strompi
            {
                Console.WriteLine("The date und time will be synced: Raspberry Pi -> StromPi'");

                int dayOfWeekPython = (int)rpiDateTime.DayOfWeek;

                // map value of sunday (0 in .net to 7 on Strompi3)
                if (dayOfWeekPython == 0) dayOfWeekPython = 7;

                string argumentsDate = $"{rpiDateTime.Day:D2} {rpiDateTime.Month:D2} {rpiDateTime.Year % 100:D2} {dayOfWeekPython}";

                Console.WriteLine($"serial write 'set-date {argumentsDate}'");

                Port.Write($"set-date {argumentsDate}");
                Thread.Sleep(500);
                Port.Write("\r");
                Thread.Sleep(1000);

                string argumentsTime = $"{rpiDateTime.Hour:D2} {rpiDateTime.Minute:D2} {rpiDateTime.Second:D2}";

                Console.WriteLine($"serial write 'set-clock {argumentsTime}'");
                Port.Write($"set-clock {argumentsTime}");

                Thread.Sleep(500);
                Port.Write("\r");

                Port.Close();

                Port.ReceiveConfiguration(Settings);  // re-read to get the updated datetime

                Console.WriteLine("-----------------------------------");
                Console.WriteLine("The date und time has been synced: Raspberry Pi -> StromPi'");
                Console.WriteLine($"Strompi3 is up-to-date:  {Settings.CurrentDateTime}");
                Console.WriteLine("-----------------------------------");
            }

            if (rpiDateTime < Settings.CurrentDateTime) // sync the Raspi 
            {
                //TODO: not tested so far..
                Console.WriteLine("The date und time will be synced: StromPi -> Raspberry Pi'");
                Os.SetDateTime(Settings.CurrentDateTime);

                Console.WriteLine("-----------------------------------");
                Console.WriteLine("The date und time has been synced: StromPi -> Raspberry Pi'");
                Console.WriteLine("-----------------------------------");
            }
        }


        public void Dispose()
        {
            Port.Dispose();
        }

        public override string ToString()
        {
            return Settings.ToString();
        }
    }
}
