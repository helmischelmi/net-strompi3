using System;
using System.Diagnostics;
using System.IO.Ports;

namespace SerialConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();
            Console.WriteLine("The following serial ports were found:");
            // Display each port name to the console.
            foreach (string port in ports)
            {
                Console.WriteLine($"Serial name: {port}");
                var isTTY = port.Contains("tty");
                if (isTTY) continue;

                Console.WriteLine("No tty.. serial port!");
                return;
            }

            Console.WriteLine("Yes, we have the embedded serial port available, connecting it");

            // Strompi3 - Serial UART - Interface
            using (StromPi3 ups = new StromPi3())
            {
                var sw = new Stopwatch();
                sw.Start();
                ups.ReadStatus();
                sw.Stop();
                Console.WriteLine($"Read Status in {sw.ElapsedMilliseconds/1000:F4} secs");
                Console.WriteLine(ups);

                sw.Restart();
                ups.SyncRTC();
                sw.Stop();
                Console.WriteLine($"Sync RTC in {sw.ElapsedMilliseconds/1000:F4} secs");

                ups.PollingShutDownOnPowerFailure();

            }
        }
    }
}


