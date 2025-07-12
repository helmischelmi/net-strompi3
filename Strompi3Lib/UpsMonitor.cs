using System;
using System.Threading.Tasks;
using Pi.Common;
using Pi.Common.io.email;
using Strompi3Lib.serialPort;


namespace Strompi3Lib;


public class UpsMonitor
{
    private const string PowerFailureMessage = "StromPiPowerfail";
    private const string ShutDownMessage = "ShutdownRaspberryPi";
    private const string PowerBackMessage = "StromPiPowerBack";

    private StromPi3 _strompi3;
    private EUpsState State;
    private DateTime PowerFailureStart;


    public UpsMonitor(StromPi3 strompi3)
    {
        _strompi3 = strompi3;
        State = EUpsState.PowerOk;
        _strompi3.PortManager.PowerChangeDetected += OnPowerChanged;
        Console.WriteLine("OnPowerChanged registriert");
    }


    /// <summary>
    /// event handler, registered to the powerChangeDetected event of StromPi3.
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnPowerChanged(object sender, EventArgs e)
    {
        var msg = ((SerialPortEventArgs)e).Message;
        Console.WriteLine("\nOnPowerChanged Eventhandler ausgelöst in UPSMonitor\n");

        if (msg.Contains(PowerFailureMessage)) HandlePowerFailure();
        else if (msg.Contains(ShutDownMessage)) HandleShutDown();
        else if (msg.Contains(PowerBackMessage)) HandlePowerBack();

    }

    private void HandlePowerFailure()
    {
        Console.WriteLine(" - OnPowerFailure erkannt");
        SetState(EUpsState.PowerIsMissing);
        StartShutdownCountdown();
    }

    private void HandleShutDown()
    {
        Console.WriteLine(" - OnShutDown erkannt");
        SetState(EUpsState.ShutdownNow);
        PerformShutdown();
    }

    private void HandlePowerBack()
    {
        SetState(EUpsState.PowerBack);
        Console.WriteLine(" - OnPowerBack erkannt");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public void SetState(EUpsState state)
    {
        State = state;
        Console.WriteLine($"State changed to {State}");
    }


    private void StartShutdownCountdown()
    {
        Console.WriteLine($"STATE Power is Missing");
        Console.WriteLine($"Countdown is enabled  : {_strompi3.Cfg.ShutdownEnable}");
        Console.WriteLine($"Failure counter is    : {_strompi3.Cfg.PowerFailureCounter}");
        Console.WriteLine($"Countdown to shutdown : {_strompi3.Cfg.ShutdownSeconds} seconds");

        PowerFailureStart = DateTime.Now;
        int secondsToShutdown = _strompi3.Cfg.ShutdownSeconds;

        while ((DateTime.Now - PowerFailureStart).TotalSeconds < secondsToShutdown && State == EUpsState.PowerIsMissing)
        {
            int elapsed = (int)(DateTime.Now - PowerFailureStart).TotalSeconds;
            Console.Write($"\rCountdown {elapsed} of {secondsToShutdown} seconds");
            Task.Delay(900).Wait();
        }
        Console.WriteLine();

        if (State == EUpsState.PowerIsMissing)
        {
            // Countdown abgelaufen, Shutdown starten
            PerformShutdown();
        }
        else
        {
            Console.WriteLine("Countdown interrupted due to power recovery.");
        }
    }

    private void PerformShutdown()
    {
        SetState(EUpsState.ShutdownNow);

        _strompi3.ReceiveStatus();  // die serielle Verbindung ist bei Shutdown nicht mehr verfügbar
                                    // daher wird das hier nix. 
        Console.WriteLine($"Countdown is enabled  : {_strompi3.Cfg.ShutdownEnable}");
        Console.WriteLine($"Failure counter is    : {_strompi3.Cfg.PowerFailureCounter}");
        Console.WriteLine($"Countdown to shutdown : {_strompi3.Cfg.ShutdownSeconds} seconds");
        Console.WriteLine($"Battery Level : {_strompi3.Cfg.BatteryHat.Level} ");

        Console.WriteLine($"SET Shutdown at {DateTime.Now.ToLongTimeString()}");

        SmtpMailer.SendEmail(SmtpConfiguration.GetDefaultConfiguration(), "LurchCam shuts down", $"Got ShutDown-Signal from StromPi3 at {DateTime.Now.ToLongTimeString()}!");
        
        Task.Delay(20000).Wait();
        //Os.ShutDown();
    }


}