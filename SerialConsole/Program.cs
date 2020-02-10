using System;
using System.Diagnostics;
using System.IO.Ports;
using Strompi3Lib;

namespace SerialConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!StromPi3.ShowAvailableSerialPorts("tty")) return;

            using (var ups = new StromPi3())
            {
                var sw = new Stopwatch();
                sw.Start();
                ups.GetSettings();
                sw.Stop();
                Console.WriteLine($"Read Status in {sw.ElapsedMilliseconds/1000:F4} secs");
                Console.WriteLine(ups);

                sw.Restart();
                ups.SyncRTC();
                sw.Stop();
                Console.WriteLine($"Sync RTC in {sw.ElapsedMilliseconds/1000:F4} secs");

                ups.Configure();

                ups.PollAndWaitForPowerFailureToShutDown();

            }
        }
    }
}


