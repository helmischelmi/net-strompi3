using System;
using System.Threading;
using System.Threading.Tasks;
using Pi.Common;
using Strompi3Lib.serialPort;

namespace Strompi3Lib;


/// <summary>
/// Helper class that provides methods to edit StromPi3 configuration, such as
/// power priority,
/// shutdown mode,
/// serial less mode,
/// power-on button,
/// alarm configuration and
/// power-save mode.
/// 
/// </summary>
public static class StromPi3Manager
{


    public static EConnectionState ConnectionState
    {
        get
        {
            return Os.IsSerialConsoleDeactivatedAndSerialPortActive() && Os.HasSerialPort("tty")
                ? EConnectionState.UART_Connected
                : EConnectionState.UART_NotConnected;
        }
    }



    /// <summary>
    /// Gets the current status of the StromPi3.
    /// </summary>
    public static void GetStatus()
    {
        using (var spManager = new SerialPortManager())
        {
            spManager.Open();
            var stromPi3 = new StromPi3(spManager);

            stromPi3.ReceiveStatus();

            stromPi3.ToString();
        }
    }

    /// <summary>
    /// Gets the current status of the StromPi3.
    /// </summary>
    /// <param name="expectedConfig"></param>
    public static void GetStatusAndCompare(StromPi3 expectedConfig)
    {
        using (var spManager = new SerialPortManager())
        {
            spManager.Open();
            var stromPi3 = new StromPi3(spManager);
            stromPi3.ReceiveStatus();
            Console.WriteLine(stromPi3.ToString());

            // Compare the current configuration with the expected configuration

            Console.WriteLine(stromPi3.Cfg.MatchesExpectedConfiguration(expectedConfig.Cfg)
                ? "Current StromPi3 configuration matches the expected configuration."
                : "Current StromPi3 configuration does NOT match the expected configuration.");

            stromPi3.CheckSettings();
        }
    }

    public static async Task<StromPi3Configuration?> GetStatusAndMonitorPowerChangeEventsAsync()
    {
        StromPi3Configuration? result = null;

        using (var spManager = new SerialPortManager())
        {
            spManager.Open();
            var stromPi3 = new StromPi3(spManager);
            result = stromPi3.ReceiveStatus();

            Console.WriteLine("Drücken Sie 'Q', um das Programm zu beenden.");

            var cts = new CancellationTokenSource();

            // Receice status every 3 minutes in a background task
            var pollTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    // (Optional) Polling, falls wirklich gebraucht
                    await Task.Delay(TimeSpan.FromMinutes(3), cts.Token);
                    var status = stromPi3.ReceiveStatus();
                    status.ToString();
                    Console.WriteLine("Drücken Sie 'Q', um das Programm zu beenden.");
                }
            }, cts.Token);

            // Tastaturüberwachung im Hauptthread
            while (true)
            {
                // Überprüfe, ob der Benutzer 'Q' drückt, um das Programm zu beenden
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.KeyChar == 'Q' || key.KeyChar == 'q')
                    {
                        Console.WriteLine($" Q pressed at {DateTime.Now:T}");
                        cts.Cancel();
                        break;
                    }
                }
                await Task.Delay(200); // Kurze Pause, um CPU zu schonen
            }

            await pollTask; // Warten, bis Hintergrund-Task sauber beendet

            return result;
        }
    }


    public static void MonitorPowerChangeEvents()
    {
        using (var spManager = new SerialPortManager())
        {
            spManager.Open();
            var stromPi3 = new StromPi3(spManager);

            stromPi3.ReceiveStatus();
            stromPi3.ToString();

            Console.WriteLine("\"Monitoring started. Press Q or q any key, to end.");

            // Endlos-Schleife, die das Programm am Leben hält
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.KeyChar == 'Q' || key.KeyChar == 'q')
                    {
                        Console.WriteLine($"Q pressed at {DateTime.Now:T} – Monitoring ended.");
                        break;
                    }
                }
                Thread.Sleep(200); // CPU schonen, 200 ms Pause
            }
        }
    }


    /// <summary>
    /// Updates the StromPi3 configuration and sends it to the device.
    /// </summary>
    public static void UpdateAndSendConfiguration()
    {
        using (var spManager = new SerialPortManager())
        {
            spManager.Open();
            var stromPi3 = new StromPi3(spManager);

            stromPi3.ReceiveStatus();

            Console.WriteLine(stromPi3.ToString()); ;

            Console.Write("Do you want to update the configuration? (Y/N): ");

            string update = Console.ReadLine();

            if (update?.ToUpper() == "Y")
            {
                stromPi3.UpdateCompleteConfiguration();

                stromPi3.SendConfiguration();

                Console.WriteLine("Configuration transferred successfully.");
            }
            else
            {
                Console.WriteLine("No changes made.");
            }
        }
    }


    public static void SynchronizeAndSendRtc()
    {
        using (var spManager = new SerialPortManager())
        {
            spManager.Open();
            var stromPi3 = new StromPi3(spManager);

            stromPi3.ReceiveStatus();//  Statusabfrage

            Console.WriteLine(stromPi3.ToString()); ;

            Console.Write("Do you want to synchronize the RTC? (Y/N): ");
            string update = Console.ReadLine();
            if (update?.ToUpper() == "Y")
            {
                stromPi3.SyncRtc();

                Console.WriteLine("RTC transferred successfully.");
            }
            else
            {
                Console.WriteLine("No changes made.");
            }
        }
    }


    public static void SendConfiguration()
    {
        var isSerialPortConfiguredToUseStromPi3 = Os.IsSerialConsoleDeactivatedAndSerialPortActive(true);
        var isPortAvailable = Os.HasSerialPort("tty", true);

        if (isSerialPortConfiguredToUseStromPi3 == false || isPortAvailable == false)
        {
            return; // config error
        }

        using (var spManager = new SerialPortManager())
        {
            spManager.Open();

            var stromPi3 = new StromPi3(spManager);

            stromPi3.ReceiveStatus();

            Console.WriteLine(stromPi3.ToString()); ;

            stromPi3.SendConfiguration();

            Console.WriteLine("Configuration transferred successfully.");
        }
    }

    //public static EConnectionState CheckConnection()
    //{

    //    using (var spManager = new SerialPortManager())
    //    {
    //        spManager.Open();

    //        var stromPi3 = new StromPi3(spManager);

    //        var connectionState = stromPi3.ConnectionState;
            
    //        Console.WriteLine($"ConnectionState is {connectionState}.");

    //        return connectionState;
    //    }
    //}
}
