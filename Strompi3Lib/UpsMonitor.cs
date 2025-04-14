using Strompi3Lib.Common;
using System;
using System.IO.Ports;
using System.Numerics;
using System.Threading.Tasks;
using Strompi3Lib.serialPort;


namespace Strompi3Lib;


public class UpsMonitor
{
    private const string PowerFailureMessage = "StromPiPowerfail";
    private const string ShutDownMessage = "ShutdownRaspberryPi";
    private const string PowerBackMessage = "StromPiPowerBack";

    private StromPi3 _strompi3;
    private EUpsState State;

    private int CountDownSeconds = 0;
    private DateTime PowerFailureStart;


    public UpsMonitor(StromPi3 strompi3)
    {
        _strompi3 = strompi3;
        State = EUpsState.PowerOk;

        // Registrierung des Eventhandlers für das Power-Change-Signal

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
        Console.WriteLine("OnPowerChanged ausgelöst");
        Console.WriteLine("UPS Monitor: Power-Change signaled!");

        if (((SerialPortEventArgs)e).Message.Contains(PowerFailureMessage)) // "xxx--StromPiPowerfail--xxx"
        {
            OnPowerFailure(((SerialPortEventArgs)e).Message);
        }

        if (((SerialPortEventArgs)e).Message.Contains(ShutDownMessage)) // "xxx--StromPiPowerfail--xxx"
        {
            OnShutDown(((SerialPortEventArgs)e).Message);
        }

        if (((SerialPortEventArgs)e).Message.Contains(PowerBackMessage)) // "xxx--StromPiPowerfail--xxx"
        {
            OnPowerBack(((SerialPortEventArgs)e).Message);
        }
    }


    private bool OnPowerFailure(string msg)
    {
        Console.WriteLine("OnPowerFailure ausgelöst");
        SetState(EUpsState.PowerIsMissing);
        return msg.Contains(PowerFailureMessage);
    }

    private bool OnShutDown(string msg)
    {
        Console.WriteLine("OnShutDown ausgelöst");
        return msg.Contains(ShutDownMessage);
    }


    private bool OnPowerBack(string msg)
    {
        Console.WriteLine("OnPowerBack ausgelöst");
        SetState(EUpsState.PowerBack);
        return msg.Contains(PowerBackMessage);
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    private void SetState(EUpsState state)
    {
        State = state;

        if (State == EUpsState.PowerIsMissing)
        {
            Console.WriteLine($"STATE Power is Missing");
            Console.WriteLine($"Countdown is enabled  : {_strompi3.Cfg.ShutdownEnable}");
            Console.WriteLine($"Failure counter is    : {_strompi3.Cfg.PowerFailureCounter}");
            Console.WriteLine($"Countdown to shutdown : {_strompi3.Cfg.ShutdownSeconds} seconds");

            PowerFailureStart = DateTime.Now;
            CountDownSeconds = _strompi3.Cfg.ShutdownSeconds;
            int currentcountDownSeconds = 0;

 
            // Start des Countdowns mit Überprüfung des aktuellen Zustands in jedem Zyklus:
            while (currentcountDownSeconds < CountDownSeconds && State == EUpsState.PowerIsMissing)
            {
                currentcountDownSeconds = (int)(DateTime.Now - PowerFailureStart).TotalSeconds;
                Console.Write($"\rCountdown {currentcountDownSeconds} of {CountDownSeconds} seconds");
                Task.Delay(900).Wait();
            }
            Console.WriteLine();

            if (State == EUpsState.PowerIsMissing)
            {
                // Countdown wurde vollständig durchlaufen, ohne dass ein PowerBack erfolgte.
                State = EUpsState.ShutdownNow;
                Console.WriteLine($"SET Shutdown at {DateTime.Now.ToLongTimeString()}");
            }
            else
            {
                // Der Countdown wurde unterbrochen, weil sich der Zustand z. B. auf PowerBack geändert hat.
                Console.WriteLine("Countdown interrupted due to power recovery.");
            }
        }

        if (State == EUpsState.PowerBack)
        {
            Console.WriteLine($"STATE Power OK");
        }
    }


    private void CheckSettings()
    {
        _strompi3.ReceiveStatus();
        CountDownSeconds = _strompi3.Cfg.ShutdownSeconds;

        if (!_strompi3.Cfg.PowerFailWarningEnable)
        {
            Console.WriteLine("***error: Polling PowerIsMissing will fail, because PowerFail Warning of Strompi3 is NOT enabled!");
            State = EUpsState.InvalidSettings;
        }

        if ((int)_strompi3.Cfg.BatteryHat.Level <= (int)_strompi3.Cfg.BatteryHat.BatteryShutdownLevel)
        {
            Console.WriteLine("***error: Polling PowerIsMissing will fail, because Battery Level is already too low!");
            State = EUpsState.BatteryLevelBelowMinimum;
        }


        if (_strompi3.Cfg.ShutdownSeconds < 10)  // set min. 10 secs
        {
            _strompi3.Cfg.GetShutDown(_strompi3.Cfg.ShutdownEnable.ToNumber().ToString(),
                10, (int)_strompi3.Cfg.BatteryHat.BatteryShutdownLevel);
            Console.WriteLine("***warning: Set ShutdownSeconds to 10 secs!");
        }
    }
}