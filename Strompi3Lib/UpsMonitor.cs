using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Strompi3Lib
{

    public class UpsMonitor
    {
        private const string PowerFailureMessage = "StromPiPowerfail";
        private const string ShutDownMessage = "ShutdownRaspberryPi";
        private const string PowerBackMessage = "StromPiPowerBack";

        private StromPi3 _ups;
        private SerialPort Port;
        private EUpsState State;

        private int CountDownSeconds = 0;
        private DateTime PowerFailureStart;
        private int PollDelayMilliSec = 100;


        public UpsMonitor(StromPi3 ups)
        {
            _ups = ups;
            Port = _ups.Port;
            State = EUpsState.PowerOk;
        }

        private void CheckSettings()
        {
            _ups.Port.ReceiveConfiguration(_ups.Settings);
            CountDownSeconds = _ups.Settings.ShutdownSeconds;

            if (!_ups.Settings.PowerFailWarningEnable)
            {
                Console.WriteLine("***error: Polling PowerFailure will fail, because PowerFail Warning of Strompi3 is NOT enabled!");
                State = EUpsState.InvalidSettings;
            }

            if ((int)_ups.Settings.BatteryHat.Level <= (int)_ups.Settings.BatteryHat.BatteryShutdownLevel)
            {
                Console.WriteLine("***error: Polling PowerFailure will fail, because Battery Level is already too low!");
                State = EUpsState.BatteryLevelBelowMinimum;
            }

            if (_ups.Settings.ShutdownEnable) 
            {
                Console.WriteLine("***warning: Immediate Shutdown is enabled in Settings!");
            }

            if (_ups.Settings.ShutdownSeconds < 10)  // set min. 10 secs
            {
                _ups.Settings.SetShutDown(_ups.Settings.ShutdownEnable.ToNumber().ToString(),
                    10, (int)_ups.Settings.BatteryHat.BatteryShutdownLevel);
                Console.WriteLine("***warning: Set ShutdownSeconds to 10 secs!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void SwitchState(string data)
        {
            //Console.WriteLine($"Switch State ({State}): <{data}>");

            if (State == EUpsState.PowerOk)
            {
                if (data.Contains(PowerFailureMessage))
                {
                    State = EUpsState.PowerFailure; // Start countdown
                    PowerFailureStart = DateTime.Now.AddSeconds(-10);  // give 10 secs to shutdown by script before power is turned of
                    Console.WriteLine($"SET PowerFailure - Start Countdown at : <{PowerFailureStart.ToLongTimeString()}>");
                    CountDownSeconds = Convert.ToInt32((DateTime.Now - PowerFailureStart).TotalSeconds);
                }

                if (data.Contains(ShutDownMessage))
                {
                    State = EUpsState.ShutdownNow;
                    Console.WriteLine();
                    Console.WriteLine($"Strompi will Shutdown RPi now: {DateTime.Now.ToLongTimeString()}");
                }

            }

            if (State == EUpsState.PowerFailure)
            {
                if (data.Contains(PowerBackMessage))
                {
                    State = EUpsState.PowerBack;
                    Console.WriteLine($"SET PowerBack");
                    // log msg
                    State = EUpsState.PowerOk;
                    Console.WriteLine($"SET PowerOK");
                }

                // continue countdown
                int tmpSeconds = Convert.ToInt32((DateTime.Now - PowerFailureStart).TotalSeconds);
                if (tmpSeconds > CountDownSeconds)
                {
                    CountDownSeconds = tmpSeconds;
                    Console.Write($"\rCountdown {CountDownSeconds} secs (of {_ups.Settings.ShutdownSeconds})");
                }


                // end with shutdown
                if (CountDownSeconds >= _ups.Settings.ShutdownSeconds)
                {
                    State = EUpsState.ShutdownNow;
                    Console.WriteLine();
                    Console.WriteLine($"SET Shutdown at {DateTime.Now.ToLongTimeString()}");
                }
            }

            //Console.WriteLine($"leave State ({State})");
        }

        /// <summary>
        /// Polls the powerFail-warning- (if enabled in configuration) and powerBack-signal.
        /// <para>Requires serial-mode</para>
        /// </summary>
        public void Poll()
        {
            CheckSettings();

            if (State != EUpsState.PowerOk)
            {
                Console.WriteLine("***Error: Strompi3 can't start monitoring");
                return;
            }

            if (!Port.IsOpen) Port.Open();
            while (State != EUpsState.ShutdownNow)
            {
                Thread.Sleep(PollDelayMilliSec);
                string data = String.Empty;
                try
                {
                    data = Port.ReadLine();
                }
                catch (TimeoutException) { }// ignore timeouts

                SwitchState(data);
            }

            Console.WriteLine($"Start Pi shutdown in 3 seconds...");
            Thread.Sleep(3000);
            Os.ShutDown();
        }


        /// <summary>
        /// IRQ-based method to get the powerFail-warning- (if enabled in configuration) and powerBack-signal.
        /// <para>Requires serialless-mode</para>
        /// </summary>
        /// <param name="waitForPowerBackTimerSeconds">Seconds to wait for a powerBack-signal before shutting down the Raspberry pi.
        /// This must be set - or will be forced - lower than the configured shutdown-timer to make a safe shutdown.</param>
        //public void WaitForPowerFailureIrqToShutDown(int waitForPowerBackTimerSeconds = 10)
        //{
        //    bool runCountdown = false;
        //    DateTime powerFailureStartTime = default;

        //    CheckSettings();
        //    if (State != EUpsState.PowerOk)
        //    {
        //        Console.WriteLine("***Error: Strompi3 can't start monitoring");
        //        return;
        //    }

        //    string data = String.Empty;

        //    Console.WriteLine($"PollingShutDownOnPowerFailure (wait {waitForPowerBackTimerSeconds} secs)");


        //    while (true)
        //    {
        //        Thread.Sleep(100);
        //        try
        //        {
        //            // data = _serialPort.ReadLine();
        //        }
        //        catch (TimeoutException) { }  // ignore timeouts

        //        var powerFailureSignal = OnPowerFailureMessage(data);
        //        var powerBackSignal = OnPowerBackMessage(data);

        //        if (powerFailureSignal || runCountdown)
        //        {
        //            if (runCountdown == false)
        //            {
        //                runCountdown = true;
        //                powerFailureStartTime = DateTime.Now;
        //            }

        //            int countdownSeconds = Convert.ToInt32((DateTime.Now - powerFailureStartTime).TotalSeconds);
        //            Console.WriteLine($"PowerFail - run countdown to shutdown the Pi ({waitForPowerBackTimerSeconds - countdownSeconds} secs)");

        //            if (countdownSeconds >= waitForPowerBackTimerSeconds)
        //            {
        //                Console.WriteLine($"Raspberry Pi: running shutdown...");
        //                Thread.Sleep(1000);
        //                Os.ShutDown();
        //            }
        //        }

        //        if (powerBackSignal && runCountdown)
        //        {
        //            Console.WriteLine("PowerBack - aborting Raspberry Pi shutdown");
        //            runCountdown = false;
        //        }
        //    }
        //}

        private bool OnPowerFailureMessage(string data)
        {
            return data.Contains(PowerFailureMessage);
        }


        private bool OnPowerBackMessage(string data)
        {
            return data.Contains(PowerBackMessage);
        }
    }
}
