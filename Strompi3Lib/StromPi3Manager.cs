using System;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.RotaryEncoder;
using Pi.Common;
using Strompi3Lib.Common;
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
public class StromPi3Manager
{

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

            Console.WriteLine(stromPi3.ToString()); ;
        }
    }


    public static async Task<StromPi3Configuration?> GetStatusAndMonitorPowerEventsAsync()
    {
        StromPi3Configuration? result = null;

        var cts = new CancellationTokenSource();

        using (var spManager = new SerialPortManager())
        {
            spManager.Open();
            var stromPi3 = new StromPi3(spManager);
            //result = stromPi3.ReceiveStatus();

            Console.WriteLine("Drücken Sie 'Q', um das Programm zu beenden.");

            Task<StromPi3Configuration> commandTask = null;
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

                // Starte den Task nur, wenn er noch nicht läuft oder bereits abgeschlossen ist.
                if (commandTask == null || commandTask.IsCompleted)
                {
                    commandTask = ReceiveStatusAsync(stromPi3, cts.Token, 3);
                    
                    Console.WriteLine($"ReceiveStatusAsync wurde gestartet: {DateTime.Now:T}.");
                    Console.WriteLine("Drücken Sie 'Q', um das Programm zu beenden.");                    
                    
                    result = commandTask.Result;
                    Console.WriteLine($"Status {Environment.NewLine}:{result}");
                }
            }

            if (commandTask != null)
            {
                result = commandTask.Result;// Warte, bis der laufende Task beendet ist.
            }
        }

        return result;
    }


    public static async Task<StromPi3Configuration?> GetStatusAndMonitorPowerEventsAsync(CancellationToken token)
    {
        StromPi3Configuration? result = null;

        var cts = new CancellationTokenSource();

        using (var spManager = new SerialPortManager())
        {
            spManager.Open();
            var stromPi3 = new StromPi3(spManager);
           
         Task<StromPi3Configuration> commandTask = null;
            while (token.IsCancellationRequested == false)
            {
                // Starte den Task nur, wenn er noch nicht läuft oder bereits abgeschlossen ist.
                if (commandTask == null || commandTask.IsCompleted)
                {
                    commandTask = ReceiveStatusAsync(stromPi3, cts.Token, 3);

                    result = commandTask.Result;

                    Console.WriteLine($"ReceiveStatusAsync wurde gestartet: {DateTime.Now:T}.");
                }
            }

            if (commandTask != null)
            {
                result = commandTask.Result;// Warte, bis der laufende Task beendet ist.
            }
        }

        return result;
    }

    /// <summary>
    /// Receives the status of the StromPi3 and prints it to the console.
    /// if forcedDelayInMinutes > 0, it waits for the specified time before returning.
    /// </summary>
    /// <param name="stromPi3"></param>
    /// <param name="token"></param>
    /// <param name="forcedDelayInMinutes"></param>
    /// <returns></returns>
    static async Task<StromPi3Configuration> ReceiveStatusAsync(StromPi3 stromPi3, CancellationToken token, int forcedDelayInMinutes = 0)
    {
        var status = stromPi3.ReceiveStatus();

        Console.WriteLine(stromPi3.ToString());

        if (forcedDelayInMinutes > 0)
        {
            try
            {
                // Warte <forcedDelayInMinutes> Minuten, aber breche den Delay ab, wenn cts.Cancel ausgelöst wird.
                await Task.Delay(TimeSpan.FromMinutes(forcedDelayInMinutes), token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Delay wurde abgebrochen.");
            }
        }

        //await Task.CompletedTask;
        return status;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void MonitorPowerChangeEvents()
    {
        using (var spManager = new SerialPortManager())
        {
            spManager.Open();
            var stromPi3 = new StromPi3(spManager);

            stromPi3.ReceiveStatus();
            Console.WriteLine(stromPi3.ToString()); ;

            // Keep-Alive-Thread starten
            Console.WriteLine("Monitoring started. Press any key, to end.");
            MonitorPowerChangeEventsAsync().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Keep-Alive-Thread, that allows monitors StromPi3 power changes until a key is pressed.
    /// </summary>
    /// <returns></returns>
    private static async Task MonitorPowerChangeEventsAsync()
    {
        Console.WriteLine("Monitoring gestartet. Drücken Sie eine beliebige Taste, um zu beenden.");

        CancellationTokenSource cts = new CancellationTokenSource();

        Task monitoringTask = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(2000, cts.Token);
            }
        }, cts.Token);

        await Task.Run(() => Console.ReadKey(true));
        cts.Cancel();

        await monitoringTask;

        Console.WriteLine("Monitoring beendet.");
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
        var isPortAvailable = Os.ShowAvailableSerialPorts("tty", true);

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
}
