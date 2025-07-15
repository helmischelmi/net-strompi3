using Strompi3Lib.batteryHat;
using Strompi3Lib.Common;
using System;
using System.Threading;
using Strompi3Lib.serialPort;
using System.Device.Gpio;
using Pi.Common;

namespace Strompi3Lib;



public class StromPi3 : IStromPi3
{
    public readonly SerialPortManager PortManager;

    public const int GPIOShutdownPinBoardNumber = 40;


    // Die Pausen in Millisekunden (0.1 s und 0.5 s)
    private const int BreakShort = 100;
    private const int BreakLong = 500;

    private const int ExpectedLinesReadStatus = 38;

    public StromPi3Configuration Cfg { get; private set; }
    public UpsMonitor UpsMonitor { get; }


    public StromPi3(SerialPortManager portManager, bool bSilent = false)
    {
        PortManager = portManager;

        UpsMonitor = new UpsMonitor(this);

        if (CheckSerialPortAvailability() == false)
        {
            throw new Exception("Serial port not available for Strompi3");
        }
    }



    /// <summary>
    /// Initializes a new instance (minimized) of the <see cref="StromPi3"/> class.
    /// This contains only the configuration and no serial port manager.
    /// </summary>

    private StromPi3()
    {
        Cfg = new StromPi3Configuration();
    }

    /// <summary>
    /// Checks if the serial port is available for Strompi3
    /// </summary>
    /// <returns></returns>
    private bool CheckSerialPortAvailability()
    {
        var isSerialPortConfiguredToUseStromPi3 = Os.IsSerialConsoleDeactivatedAndSerialPortActive();
        var isPortAvailable = Os.HasSerialPort("tty");

        return isSerialPortConfiguredToUseStromPi3 && isPortAvailable;
    }


    /// <summary>
    /// Sends initialize command to Strompi3, expects no answer
    /// </summary>
    /// <para>
    /// <remarks>Requires serial-mode</remarks>
    /// </para>
    public void InitializePort()
    {
        string[] cmds = new string[] { "quit", "\r" };
        int[]? delays = new int[] { BreakShort, BreakLong };

        PortManager.SendCommand(cmds, delays, false,0);
    }

    /// <summary>
    /// Reads the state and configuration of the StromPi3. 
    /// </summary>
    public StromPi3Configuration ReceiveStatus(bool bSilent = true)
    {
        InitializePort(); // send "quit" command to Strompi3

        var configuration = new StromPi3Configuration();

        // Befehlssequenz: "status-rpi" senden, 1000ms warten, dann "\r" senden.
        string[] cmds = new string[] { "status-rpi", "\r" };
        int[] delays = new int[] { 1000, 0 };

        string response = PortManager.SendCommand(cmds, delays, true, 38);

        // Zerlege die Antwort in Zeilen und verarbeite sie.
        string[] lines = response.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < ExpectedLinesReadStatus)
        {
            Console.WriteLine("Statusantwort unvollständig. Erwartet: " + ExpectedLinesReadStatus + " Zeilen, erhalten: " + lines.Length);
            return configuration;
        }

         // Zeilenweise Verarbeitung der Antwort:
        // Zeile 0: StromPi3-Zeit, Zeile 1: Datum
        configuration.GetRTCDateTime(lines[0], lines[1]);

        // Zeile 3: Input-Priority-Mode
        configuration.GetInputPriorityMode(lines[3]);

        // Zeile 4: Alarm Enabled, Zeile 5: Alarm Mode
        configuration.AlarmSettings.GetAlarmEnabled(lines[4]);
        configuration.AlarmSettings.GetAlarmMode(lines[5]);

        // Zeilen 6-10: Alarm-Zeit (Stunde, Minute, Tag, Monat, Wochentag)
        configuration.AlarmSettings.GetAlarmDateTime(lines[6], lines[7], lines[8], lines[9], lines[10]);

        // Zeile 11: Alarm Power-Off Enabled
        configuration.AlarmSettings.GetAlarmPowerOffEnabled(lines[11]);

        // Zeilen 12-13: Power-Off Alarm Zeit (Stunde, Minute)
        configuration.AlarmSettings.GetAlarmPowerOffTimePeriod(lines[12], lines[13]);

        // Zeile 14-15: Shutdown Enable und Shutdown Seconds
        configuration.GetShutDown(lines[14], Convert.ToInt32(lines[15]), (int)EBatteryLevel.nothing);

        // Zeile 16: PowerFail Warning Enable
        configuration.GetPowerFailWarningEnable(lines[16]);

        // Zeile 17: Serialless Enable
        configuration.GetSerialLessEnable(lines[17]);

        // Zeilen 18-20: Interval Alarm (Enable, On-Time, Off-Time)
        configuration.AlarmSettings.GetAlarmIntervall(lines[18], lines[19], lines[20]);

        // Zeilen 21-23: BatteryHat (Shutdown Level, Batteriestatus, Charging-Status)
        configuration.BatteryHat.GetBatteryHat(Convert.ToInt32(lines[21]), lines[22], lines[23]);

        // Zeilen 24-28: StartStopSettings (PowerOnButton Enable, Timer, Powersave Enable, Poweroff Mode, Enable Mode)
        configuration.StartStopSettings.GetStartStopSettings(lines[24], lines[25], lines[26], lines[27], lines[28]);

        // Zeilen 29-30: Wakeup Timer und Wakeup Weekend Enable
        configuration.AlarmSettings.GetAlarmWakeupTimerAndWeekend(lines[29], lines[30]);

        // Zeilen 31-34: VoltageMeter (ADC Wide, ADC Bat, ADC Usb, Output Voltage)
        configuration.VoltageMeter.GetVoltageMeter(lines[31], lines[32], lines[33], lines[34]);

        // Zeile 35: Output Status
        configuration.GetPowerInputSource(lines[35]);

        // Zeile 36: Power Failure Counter
        configuration.GetPowerFailureCounter(lines[36]);

        // Zeile 37: Firmware Version
        configuration.GetFirmwareVersion(lines[37]);

        Cfg = configuration;

        return configuration;
    }

    public void CheckSettings(bool receiveNewStatus = false)
    {
        if (receiveNewStatus) ReceiveStatus();

        if (!Cfg.PowerFailWarningEnable)
        {
            Console.WriteLine("***error: Polling PowerIsMissing will fail, because PowerFail Warning of Strompi3 is NOT enabled!");
            UpsMonitor.SetState(EUpsState.InvalidSettings);
        }

        if ((int)Cfg.BatteryHat.Level <= (int)Cfg.BatteryHat.BatteryShutdownLevel)
        {
            Console.WriteLine("***error: Polling PowerIsMissing will fail, because Battery Level is already too low!");
            UpsMonitor.SetState(EUpsState.BatteryLevelBelowMinimum);
        }


        if (Cfg.ShutdownSeconds < 10)  // set min. 10 secs
        {
            Cfg.GetShutDown(Cfg.ShutdownEnable.ToNumber().ToString(),
                10, (int)Cfg.BatteryHat.BatteryShutdownLevel);
            Console.WriteLine("***warning: Set ShutdownSeconds to 10 secs!");
        }

        if (Cfg.FirmwareVersion != "v1.8")
        {
            Console.WriteLine("***error: firmware should be updated to v1.8!");
        }
    }

    /// <summary>
    /// Updates the configuration of the Strompi3.
    /// </summary>
    public void UpdateCompleteConfiguration()
    {
        InitializePort(); // send "quit" command to Strompi3

        Console.WriteLine("Update complete Configuration");

        UpdateInputPriorityMode();

        UpdateShutdownModeAndTimer();

        //UpdateSerialless();

        //UpdatePowerSaveMode();

        UpdatePowerFailWarning();

        UpdatePowerOnButton();
    }

    /// <summary>
    /// reads the given setting from user input and sends it to the Strompi3-port
    /// </summary>
    private void UpdateInputPriorityMode(bool sendToStromPi3 = false)
    {
        Console.WriteLine($"1. Current Input-Priority-Mode: ({(int)Cfg.PriorityModeMode}) = {ConverterHelper.GetEnumDescription(Cfg.PriorityModeMode)}");

        foreach (EPriorityMode priority in (EPriorityMode[])Enum.GetValues(typeof(EPriorityMode)))
        {
            Console.WriteLine($"Mode  {(int)priority}: {ConverterHelper.GetEnumDescription(priority)}");
        }

        Cfg.GetInputPriorityMode(ConverterHelper.ReadInt(1, 6, "Mode: 1 - 6").ToString());

        Console.WriteLine(
            $"Set Input Priority to {(int)Cfg.PriorityModeMode}) = {ConverterHelper.GetEnumDescription(Cfg.PriorityModeMode)}");
        Console.WriteLine("-------------------------------");
        Console.WriteLine();

        if (sendToStromPi3)
        {
            SendConfigElement(EConfigElement.InputPriority, (int)Cfg.PriorityModeMode);
            SendConfigElement(EConfigElement.ModusReset, 1);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sendToStromPi3"></param>
    private void UpdateAlarmConfiguration(bool sendToStromPi3 = false)
    {

        if (Cfg.StartStopSettings.PoweroffTimeEnableMode)
        {
            Console.WriteLine($"TODO: Alarm-Mode: Wakeup Timer");
            Console.WriteLine($"Alarm-Mode: {Cfg.AlarmSettings.Mode}");
        }

        else
        {
            // Time, Date, Weekday - Alarm
            Console.WriteLine("7. ----------------------------");
            Console.WriteLine($"Alarm-Mode: {Cfg.AlarmSettings.Mode}");
        }

        Console.WriteLine($"Alarm-Time: {Cfg.AlarmSettings.WakeUpHour}:{Cfg.AlarmSettings.WakeUpMinute} hh:mm");
        Console.WriteLine($"Alarm-Date: {Cfg.AlarmSettings.WakeUpDay}.{Cfg.AlarmSettings.WakeUpMonth} dd:mm");
        Console.WriteLine($"Alarm-Weekday: {ConverterHelper.GetEnumDescription(Cfg.AlarmSettings.WakeUpWeekday)}");

        Console.WriteLine($"PowerOff-Alarm: {Cfg.AlarmSettings.PowerOffEnable}");
        Console.WriteLine($"PowerOff-Alarm-Time: {Cfg.AlarmSettings.PowerOffHour}:{Cfg.AlarmSettings.PowerOffMinute} hh:mm");




        UpdateAlarmPowerOff(sendToStromPi3);  //# Power-Off Alarm Cfg

        UpdateAlarmWakeUp(sendToStromPi3);    //# Wake-Up Alarm-Cfg

        UpdateAlarmInterval(sendToStromPi3);  //# Interval-Alarm Cfg
    }


    /// <summary>
    /// reads the given setting from user input and sends it to the Strompi3-port
    /// <para>USAGE: Enabling or disabling the Powerfail-Warning (instead of a shutdown) through the serial interface</para>
    /// </summary>
    /// <param name="sendToStromPi3"></param>
    private void UpdatePowerFailWarning(bool sendToStromPi3 = false)
    {
        Console.WriteLine($"5. Current Powerfail Warning Enable: {Cfg.PowerFailWarningEnable}");
        Cfg.GetPowerFailWarningEnable(ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True").ToString());

        Console.WriteLine($"Set Powerfail Warning to ({Cfg.PowerFailWarningEnable})");
        Console.WriteLine("-------------------------------");
        Console.WriteLine();

        if (sendToStromPi3)
        {
            SendConfigElement(EConfigElement.WarningEnable, Cfg.PowerFailWarningEnable.ToNumber());
        }
    }


    /// <summary>
    ///  # serialless-mode
    /// reads the given setting from user input and sends it to the Strompi3-port
    /// <para>MOD (Chapter 8.1): additional cable soldering required between Reset-Pin (Jumper ON) and chosen GPIO-Pin (40)</para>
    /// <para>SERIALLESS  ON: turns off the serial comm-port and sets a GPIO-pin (40) as alternative to read</para>
    /// <para>SERIALLESS OFF: turns on the serial comm-port and resets a GPIO-pin (40)</para>
    /// </summary>
    /// <param name="sendToStromPi3"></param>
    private void UpdateSerialless(bool sendToStromPi3 = false)
    {
        Console.WriteLine($"3. Current Serialless-Mode: {Cfg.SerialLessEnable}");
        Cfg.GetSerialLessEnable(ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True").ToString());

        Console.WriteLine($"Set Serialless-Mode to ({Cfg.SerialLessEnable})");
        Console.WriteLine("-------------------------------");
        Console.WriteLine();

        if (sendToStromPi3)
        {
            if (Cfg.SerialLessEnable == false) // use serial port
            {
                SendConfigElement(EConfigElement.SerialLessMode, Cfg.SerialLessEnable.ToNumber());
            }
            else // reset to serial port
            {
                using (var gpioController = new GpioController(PinNumberingScheme.Board))
                {
                    gpioController.OpenPin(GPIOShutdownPinBoardNumber);
                    gpioController.SetPinMode(GPIOShutdownPinBoardNumber, PinMode.Output);
                    gpioController.Write(GPIOShutdownPinBoardNumber, PinValue.High);
                    Console.WriteLine($"set pin {GPIOShutdownPinBoardNumber} to HIGH (3 secs)");
                    Thread.Sleep(3000);
                    gpioController.Write(GPIOShutdownPinBoardNumber, PinValue.Low);
                    Console.WriteLine($"set pin {GPIOShutdownPinBoardNumber} to LOW to Disable Serialless Mode.");
                    Console.WriteLine($"This will take approx. 10 seconds..");
                    Thread.Sleep(10000);
                    Console.WriteLine($"Serialless Mode is Disabled!");
                }
            }
        }
    }



    /// <summary>
    /// reads the given setting from user input and sends it to the Strompi3-port
    /// </summary>
    /// <param name="sendToStromPi3"></param>
    private void UpdateShutdownModeAndTimer(bool sendToStromPi3 = false)
    {
        Console.WriteLine($"2. Current Shutdown Mode (disconnect power supply to raspi): ({Cfg.ShutdownEnable}) ");
        Console.WriteLine($"Current Shutdown-Timer (0..65535 secs): ({Cfg.ShutdownSeconds}) ");

        int shutdownEnable = ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True");

        int shutdownSeconds = Cfg.ShutdownSeconds;

        if (shutdownEnable == 1)
        {
            Console.WriteLine(
                "Values above 255 won't work with firmware 1.72, see FAQ https://strompi.joy-it.net/questions/question/rpi4-strompi3-shutdown-funktioniert-nicht-korrekt-strompi-schaltet-zu-frueh-ab/page/3/");
            // Der Bug war, dass der Wert des Shutdowntimers in einer 16 bit Variable abgelegt wurde, aber in der weiteren Verarbeitung noch eine 8 bit Variable war.
            // Dies führte dazu, dass Werte über 255 Sekunden nicht korrekt funktionierten.
            Console.WriteLine($"Shutdown-Timer (0..65535 secs): ({Cfg.ShutdownSeconds}) ");
            shutdownSeconds = ConverterHelper.ReadInt(0, 65535, "timer (0..65535 secs)");
        }

        Console.WriteLine($"Current Battery-Level Shutdown (0='Disabled', 1= '< 10%', 2='< 25%', 3='< 50%'): ({Cfg.BatteryHat.BatteryShutdownLevel}) ");

        int batteryLevelShutdown = ConverterHelper.ReadInt(0, 3, "Battery-Level Shutdown (0='Disabled', 1= '< 10%', 2='< 25%', 3='< 50%')");

        Cfg.GetShutDown(shutdownEnable.ToString(), shutdownSeconds, batteryLevelShutdown);

        Console.WriteLine($"Set Shutdown Timer Mode to ({Cfg.ShutdownEnable}), Timer  = {Cfg.ShutdownSeconds} secs.");
        Console.WriteLine($"Set Battery-Level Shutdown to {ConverterHelper.GetEnumDescription(Cfg.BatteryHat.BatteryShutdownLevel)}");
        Console.WriteLine("-------------------------------");
        Console.WriteLine();


        if (sendToStromPi3)
        {
            Console.WriteLine($"Transfer Shutdown {Cfg.ShutdownEnable.ToNumber()} in {Cfg.ShutdownSeconds} secs");
            SendConfigElement(EConfigElement.ShutdownEnable, Cfg.ShutdownEnable.ToNumber());
            SendConfigElement(EConfigElement.ShutdownTimer, Cfg.ShutdownSeconds);
            SendConfigElement(EConfigElement.ShutdownBatteryLevel, (int)Cfg.BatteryHat.BatteryShutdownLevel);
        }
    }


    /// <summary>
    /// reads the given setting from user input and sends it to the Strompi3-port
    /// <para>USAGE: turns ON or OFF a daily shutdown of the raspberry at given hour:minute </para>
    /// </summary>
    /// <param name="sendToStromPi3"></param>
    private void UpdateAlarmPowerOff(bool sendToStromPi3 = false)
    {
        Console.WriteLine($"8. Current Power-Off Alarm Enable: {Cfg.AlarmSettings.PowerOffEnable}");

        Console.WriteLine($"Set: 0 = False, 1 = True or ENTER to continue");
        bool powerOffAlarmEnable = ConverterHelper.ReadInt(0, 1, "").ToBool();

        int powerOffAlarmHour = Cfg.AlarmSettings.PowerOffHour;
        int powerOffAlarmMinute = Cfg.AlarmSettings.PowerOffMinute;

        if (powerOffAlarmEnable)
        {
            Console.WriteLine($"StromPi poweroff time: hour (0..23): {powerOffAlarmHour} hour.");
            powerOffAlarmHour = ConverterHelper.ReadInt(0, 23, "hour (0..23)");

            Console.WriteLine($"StromPi poweroff time: minute (0..59): {powerOffAlarmMinute} minute.");
            powerOffAlarmMinute = ConverterHelper.ReadInt(0, 59, "minute (0..59)");
        }

        Cfg.AlarmSettings.GetAlarmPowerOffEnabled(powerOffAlarmEnable.ToNumber().ToString());
        Cfg.AlarmSettings.GetAlarmPowerOffTimePeriod(powerOffAlarmHour.ToString(),
            powerOffAlarmMinute.ToString());


        Console.WriteLine($"PowerOff-Alarm: {Cfg.AlarmSettings.PowerOffEnable}");
        Console.WriteLine($"PowerOff-Alarm-Time: {Cfg.AlarmSettings.PowerOffHour}:{Cfg.AlarmSettings.PowerOffMinute} hh:mm");
        Console.WriteLine("-------------------------------");
        Console.WriteLine();



        if (sendToStromPi3)
        {
            Console.WriteLine($"Transfer Power-Off Alarm Enable: {Cfg.AlarmSettings.PowerOffEnable}");
            SendConfigElement(EConfigElement.AlarmPowerOff, Cfg.AlarmSettings.PowerOffEnable.ToNumber());
            Console.WriteLine($"Transfer Power-Off Alarm Hour: {Cfg.AlarmSettings.PowerOffHour}");
            SendConfigElement(EConfigElement.AlarmHoursOff, Cfg.AlarmSettings.PowerOffHour);
            Console.WriteLine($"Transfer Power-Off Alarm Minute: {Cfg.AlarmSettings.PowerOffMinute}");
            SendConfigElement(EConfigElement.AlarmMinutesOff, Cfg.AlarmSettings.PowerOffMinute);
        }
    }


    /// <param name="sendToStromPi3"></param>
    private void UpdateAlarmWakeUp(bool sendToStromPi3 = false)
    {
        Console.WriteLine($"9. Current Wake-up Alarm Enable: {Cfg.AlarmSettings.WakeupEnable}");
        bool wakeUpAlarmEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();

        int wakeUpAlarmHour = Cfg.AlarmSettings.WakeUpHour;
        int wakeUpAlarmMinute = Cfg.AlarmSettings.WakeUpMinute;
        int wakeUpAlarmDay = Cfg.AlarmSettings.WakeUpDay;
        int wakeUpAlarmMonth = Cfg.AlarmSettings.WakeUpMonth;
        int wakeUpWeekday = (int)Cfg.AlarmSettings.WakeUpWeekday;

        int wakeUpTimerMinutes = Convert.ToInt32(Cfg.AlarmSettings.WakeupTimerMinutes);
        bool wakeUpWeekendEnable = Cfg.AlarmSettings.WakeUpWeekendEnable;

        if (wakeUpAlarmEnable)
        {
            Console.WriteLine($"Current Wake-up Alarm Mode: {Cfg.AlarmSettings.Mode}");
            int wakeUpAlarmmode = ConverterHelper.ReadInt(1, 4, "alarm mode (1 = Time-Alarm , 2 = Date-Alarm, 3 = Weekday-Alarm, 4 = Wakeup Timer)");

            Cfg.AlarmSettings.GetAlarmMode(wakeUpAlarmmode.ToString());

            if (Cfg.AlarmSettings.Mode == EAlarmMode.WakeupTimer)
            {
                Console.WriteLine($"Current Wake-up Alarm Timer (minutes: {Cfg.AlarmSettings.WakeupTimerMinutes}");
                wakeUpTimerMinutes = ConverterHelper.ReadInt(1, 65535, "wake up timer (1-65535 mins)");
            }


            if (Cfg.AlarmSettings.Mode == EAlarmMode.TimeAlarm)
            {
                Console.WriteLine($"Current Wakeup-Alarm Hour (0..23): {Cfg.AlarmSettings.WakeUpHour}");
                wakeUpAlarmHour = ConverterHelper.ReadInt(0, 23, "hour (0..23)");

                Console.WriteLine($"Current Wakeup-Alarm Minute (0..59): {Cfg.AlarmSettings.WakeUpMinute}");
                wakeUpAlarmMinute = ConverterHelper.ReadInt(0, 59, "minute (0..59)");

                Console.WriteLine(
                    $"Current Wake-up Weekend Enable: {Cfg.AlarmSettings.WakeUpWeekendEnable}");
                wakeUpWeekendEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();

            }
            else if (Cfg.AlarmSettings.Mode == EAlarmMode.DateAlarm)
            {
                Console.WriteLine($"Current Wakeup-Alarm Day (1..31): {Cfg.AlarmSettings.WakeUpDay}");
                wakeUpAlarmDay = ConverterHelper.ReadInt(1, 31, "day (1..31)");

                Console.WriteLine($"Current Wakeup-Alarm Month (1..12): {Cfg.AlarmSettings.WakeUpMonth}");
                wakeUpAlarmMonth = ConverterHelper.ReadInt(1, 12, "month (1..12)");
            }
            else
            {
                Console.WriteLine($"Current Wakeup-Alarm Weekday (1..7): {Cfg.AlarmSettings.WakeUpWeekday}");
                wakeUpWeekday = ConverterHelper.ReadInt(1, 7, "weekday (1 = Monday, 2 = Tuesday, 3 = Wednesday, 4 = Thursday, 5 = Friday, 6 = Saturday, 7 = Sunday)");
            }

            Cfg.AlarmSettings.GetAlarmDateTime(wakeUpAlarmHour.ToString(), wakeUpAlarmMinute.ToString(),
                wakeUpAlarmDay.ToString(), wakeUpAlarmMonth.ToString(), wakeUpWeekday.ToString());

            Cfg.AlarmSettings.GetAlarmWakeupTimerAndWeekend(wakeUpTimerMinutes.ToString(), wakeUpWeekendEnable.ToNumber().ToString());

            Console.WriteLine($"Alarm-Time: {Cfg.AlarmSettings.WakeUpHour}:{Cfg.AlarmSettings.WakeUpMinute} hh:mm");
            Console.WriteLine($"Alarm-Date: {Cfg.AlarmSettings.WakeUpDay}.{Cfg.AlarmSettings.WakeUpMonth} dd:mm");
            Console.WriteLine($"Alarm-Weekday: {ConverterHelper.GetEnumDescription(Cfg.AlarmSettings.WakeUpWeekday)}");

            Console.WriteLine($"Alarm-Weekend: {Cfg.AlarmSettings.WakeUpWeekendEnable}");
            Console.WriteLine($"Alarm-Timer (minutes): {Cfg.AlarmSettings.WakeupTimerMinutes}");
            Console.WriteLine($"Alarm-Mode: {Cfg.AlarmSettings.Mode}");
            Console.WriteLine();
        }


        if (sendToStromPi3)
        {
            Console.WriteLine($"Transfer  Wake-up Alarm Mode: {Cfg.AlarmSettings.Mode}");
            switch (Cfg.AlarmSettings.Mode)
            {
                case EAlarmMode.TimeAlarm:
                    SendConfigElement(EConfigElement.AlarmModeA, 0);
                    SendConfigElement(EConfigElement.AlarmModeB, 0);
                    SendConfigElement(EConfigElement.AlarmModeC, 1);
                    SendConfigElement(EConfigElement.AlarmModeD, 0);
                    break;
                case EAlarmMode.DateAlarm:
                    SendConfigElement(EConfigElement.AlarmModeA, 1);
                    SendConfigElement(EConfigElement.AlarmModeB, 0);
                    SendConfigElement(EConfigElement.AlarmModeC, 0);
                    SendConfigElement(EConfigElement.AlarmModeD, 0);
                    break;

                case EAlarmMode.WeekdayAlarm:
                    SendConfigElement(EConfigElement.AlarmModeA, 0);
                    SendConfigElement(EConfigElement.AlarmModeB, 1);
                    SendConfigElement(EConfigElement.AlarmModeC, 0);
                    SendConfigElement(EConfigElement.AlarmModeD, 0);
                    break;
                case EAlarmMode.WakeupTimer:
                    SendConfigElement(EConfigElement.AlarmModeA, 0);
                    SendConfigElement(EConfigElement.AlarmModeB, 0);
                    SendConfigElement(EConfigElement.AlarmModeC, 0);
                    SendConfigElement(EConfigElement.AlarmModeD, 1);
                    break;
                default:
                    throw new NotImplementedException($"Unknown EAlarmMode {Cfg.AlarmSettings.Mode}");
            }

            SendConfigElement(EConfigElement.AlarmMinutes, Cfg.AlarmSettings.WakeUpMinute);
            SendConfigElement(EConfigElement.AlarmHours, Cfg.AlarmSettings.WakeUpHour);
            SendConfigElement(EConfigElement.AlarmDay, Cfg.AlarmSettings.WakeUpDay);
            SendConfigElement(EConfigElement.AlarmMonth, Cfg.AlarmSettings.WakeUpMonth);
            SendConfigElement(EConfigElement.AlarmWeekday, (int)Cfg.AlarmSettings.WakeUpWeekday);
            SendConfigElement(EConfigElement.AlarmEnable, Cfg.AlarmSettings.WakeupEnable.ToNumber());

            // weekend enable and  wakeUpTimerMinutes
            Console.WriteLine($"Transfer Wake-up Weekend {Cfg.AlarmSettings.WakeUpWeekendEnable} " +
                              $"and Wakeup Timer: {Cfg.AlarmSettings.WakeupTimerMinutes}");
            SendConfigElement(EConfigElement.WakeupWeekendEnable, Cfg.AlarmSettings.WakeUpWeekendEnable.ToNumber());
            SendConfigElement(EConfigElement.PowerOffTimer, Cfg.AlarmSettings.WakeupTimerMinutes);
        }
    }

    /// <summary>
    /// Interval-Alarm Cfg
    /// Reads the given setting from user input and sends it to the Strompi3-port
    /// <para>USAGE: intervals (in minutes) to turn the raspberry ON or OFF</para>
    /// <para>On-Time: minutes, the raspberry is ON</para>
    /// <para>Off-Time: minutes, the raspberry is OFF</para>
    /// </summary>
    /// <param name="sendToStromPi3"></param>
    private void UpdateAlarmInterval(bool sendToStromPi3 = false)
    {
        Console.WriteLine($"Current Interval-Alarm Enable: {Cfg.AlarmSettings.IntervalAlarmEnable}");
        Console.WriteLine($"Current Interval-On-Time: {Cfg.AlarmSettings.IntervalAlarmOnMinutes} minutes");
        Console.WriteLine($"Current Interval-Off-Time: {Cfg.AlarmSettings.IntervalAlarmOffMinutes} minutes");
        Console.WriteLine("-------------------------------");

        Console.WriteLine($"Set Interval-Alarm Enable: 0 = False, 1 = True or ENTER to continue");
        bool intervalAlarmEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();

        int intervalAlarmOnMinutes = Cfg.AlarmSettings.IntervalAlarmOnMinutes;
        int intervalAlarmOffMinutes = Cfg.AlarmSettings.IntervalAlarmOffMinutes;

        if (intervalAlarmEnable)
        {
            Console.WriteLine($"Set Interval-On-Time (0..65535 mins): {intervalAlarmOnMinutes} minutes.");
            intervalAlarmOnMinutes = ConverterHelper.ReadInt(0, 65535, "on-time (0..65535 mins)");

            Console.WriteLine($"Set Interval-Off-Time (0..65535 mins): {intervalAlarmOffMinutes} minutes.");
            intervalAlarmOffMinutes = ConverterHelper.ReadInt(0, 65535, "off-time (0..65535 mins)");
        }

        Cfg.AlarmSettings.GetAlarmIntervall(intervalAlarmEnable.ToNumber().ToString(),
            intervalAlarmOnMinutes.ToString(), intervalAlarmOffMinutes.ToString());

        Console.WriteLine($"Interval-Alarm Enable is: {Cfg.AlarmSettings.IntervalAlarmEnable}");
        Console.WriteLine($"Interval-Alarm On / Off Minutes is: {Cfg.AlarmSettings.IntervalAlarmOnMinutes} / " +
                          $"{Cfg.AlarmSettings.IntervalAlarmOffMinutes}");

        Console.WriteLine();


        if (sendToStromPi3)
        {
            SendConfigElement(EConfigElement.IntervalAlarmEnable, Cfg.AlarmSettings.IntervalAlarmEnable.ToNumber());
            SendConfigElement(EConfigElement.IntervalAlarmOnTime, Cfg.AlarmSettings.IntervalAlarmOnMinutes);
            SendConfigElement(EConfigElement.IntervalAlarmOffTime, Cfg.AlarmSettings.IntervalAlarmOffMinutes);
        }
    }



    /// <summary>
    /// Reads the given setting from user input and sends it to the Strompi3-port
    /// <para>MOD (Chapter 8.2): additional cable soldering required</para>
    /// <para>USAGE: start system by brigding the reset-pins, after the system was shutdown by "poweroff"command </para>
    /// <para>Between poweroff and poweron !WAIT! min. 30 secs</para>
    /// </summary>
    /// <param name="sendToStromPi3"></param>
    private void UpdatePowerOnButton(bool sendToStromPi3 = false)
    {
        Console.WriteLine(
            $"6. Current Power-ON-Button Enabler: ({Cfg.StartStopSettings.PowerOnButtonEnable}) ");
        int powerOnButtonSeconds = Cfg.StartStopSettings.PowerOnButtonSeconds;

        bool powerOnButtonEnable = ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True").ToBool();

        if (powerOnButtonEnable)
        {
            Console.WriteLine($"Current Power-ON-Button-Timer (0..65535 secs): ({Cfg.StartStopSettings.PowerOnButtonSeconds}) ");
            powerOnButtonSeconds = ConverterHelper.ReadInt(0, 65535, "timer (0..65535 secs)");
        }

        Cfg.StartStopSettings.SetPowerOnButton(powerOnButtonEnable, powerOnButtonSeconds);

        if (powerOnButtonEnable && Cfg.ShutdownEnable)
        {
            Console.WriteLine($"Current Poweroff-Mode Enable: ({Cfg.StartStopSettings.PowerOffMode}) ");
            Cfg.StartStopSettings.SetPowerOffMode(ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True")
                .ToBool());
        }

        Console.WriteLine($"Set Power-ON-Button to ({Cfg.StartStopSettings.PowerOnButtonEnable})");
        Console.WriteLine($"Set Power-ON-Button-Timer to ({Cfg.StartStopSettings.PowerOnButtonSeconds})");
        Console.WriteLine($"Set Poweroff-Mode to ({Cfg.StartStopSettings.PowerOffMode})");
        Console.WriteLine("-------------------------------");
        Console.WriteLine();

        if (sendToStromPi3)
        {
            SendConfigElement(EConfigElement.PowerOnButtonEnable, Cfg.StartStopSettings.PowerOnButtonEnable.ToNumber());
            SendConfigElement(EConfigElement.PowerOnButtonTime, Cfg.StartStopSettings.PowerOnButtonSeconds);
            SendConfigElement(EConfigElement.PowerOffMode, Cfg.StartStopSettings.PowerOffMode.ToNumber());
        }
    }



    /// <summary>
    /// Reads the given setting from user input and sends it to the Strompi3-port
    /// <para>when not-used WIDE power-input: turns off voltage-converter of WIDE-input to save power</para>
    /// <para>JUMPER OFF required: s. Chapter 5.5 Power save mode</para>
    /// </summary>
    private void UpdatePowerSaveMode(bool sendToStromPi3 = false)
    {
        Console.WriteLine($"4. Current Power-Save-Mode Enable: {Cfg.StartStopSettings.PowersaveEnable}");
        Cfg.GetPowerSaveEnable(ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True").ToString());
        Console.WriteLine($"Set Power-Save-Mode to ({Cfg.StartStopSettings.PowersaveEnable})");
        Console.WriteLine("-------------------------------");
        Console.WriteLine();

        if (sendToStromPi3)
        {
            SendConfigElement(EConfigElement.PowerSaveEnable, Cfg.StartStopSettings.PowersaveEnable.ToNumber());
        }
    }

   
    /// <summary>
    /// Transfers the configuration of the Strompi3 to the device.
    /// </summary>
    public void SendConfiguration()
    {
        SendConfigElement(EConfigElement.InputPriority, (int)Cfg.PriorityModeMode);

        switch (Cfg.AlarmSettings.Mode)
        {
            case EAlarmMode.TimeAlarm:
                SendConfigElement(EConfigElement.AlarmModeA, 0);
                SendConfigElement(EConfigElement.AlarmModeB, 0);
                SendConfigElement(EConfigElement.AlarmModeC, 1);
                SendConfigElement(EConfigElement.AlarmModeD, 0);
                break;
            case EAlarmMode.DateAlarm:
                SendConfigElement(EConfigElement.AlarmModeA, 1);
                SendConfigElement(EConfigElement.AlarmModeB, 0);
                SendConfigElement(EConfigElement.AlarmModeC, 0);
                SendConfigElement(EConfigElement.AlarmModeD, 0);
                break;

            case EAlarmMode.WeekdayAlarm:
                SendConfigElement(EConfigElement.AlarmModeA, 0);
                SendConfigElement(EConfigElement.AlarmModeB, 1);
                SendConfigElement(EConfigElement.AlarmModeC, 0);
                SendConfigElement(EConfigElement.AlarmModeD, 0);
                break;
            case EAlarmMode.WakeupTimer:
                SendConfigElement(EConfigElement.AlarmModeA, 0);
                SendConfigElement(EConfigElement.AlarmModeB, 0);
                SendConfigElement(EConfigElement.AlarmModeC, 0);
                SendConfigElement(EConfigElement.AlarmModeD, 1);
                break;
            default:
                throw new NotImplementedException($"Unknown EAlarmMode {Cfg.AlarmSettings.Mode}");
        }

        SendConfigElement(EConfigElement.AlarmPowerOff, Cfg.AlarmSettings.PowerOffEnable.ToNumber());
        SendConfigElement(EConfigElement.AlarmMinutes, Cfg.AlarmSettings.WakeUpMinute);
        SendConfigElement(EConfigElement.AlarmHours, Cfg.AlarmSettings.WakeUpHour);
        SendConfigElement(EConfigElement.AlarmMinutesOff, Cfg.AlarmSettings.PowerOffMinute);
        SendConfigElement(EConfigElement.AlarmHoursOff, Cfg.AlarmSettings.PowerOffHour);
        SendConfigElement(EConfigElement.AlarmDay, Cfg.AlarmSettings.WakeUpDay);
        SendConfigElement(EConfigElement.AlarmMonth, Cfg.AlarmSettings.WakeUpMonth);
        SendConfigElement(EConfigElement.AlarmWeekday, (int)Cfg.AlarmSettings.WakeUpWeekday);
        SendConfigElement(EConfigElement.AlarmEnable, Cfg.AlarmSettings.WakeupEnable.ToNumber());

        SendConfigElement(EConfigElement.ShutdownEnable, Cfg.ShutdownEnable.ToNumber());
        SendConfigElement(EConfigElement.ShutdownTimer, Cfg.ShutdownSeconds);
        SendConfigElement(EConfigElement.WarningEnable, Cfg.PowerFailWarningEnable.ToNumber());
        SendConfigElement(EConfigElement.SerialLessMode, Cfg.SerialLessEnable.ToNumber());
        SendConfigElement(EConfigElement.ShutdownBatteryLevel, (int)Cfg.BatteryHat.BatteryShutdownLevel);

        SendConfigElement(EConfigElement.IntervalAlarmEnable, Cfg.AlarmSettings.IntervalAlarmEnable.ToNumber());
        SendConfigElement(EConfigElement.IntervalAlarmOnTime, Cfg.AlarmSettings.IntervalAlarmOnMinutes);
        SendConfigElement(EConfigElement.IntervalAlarmOffTime, Cfg.AlarmSettings.IntervalAlarmOffMinutes);


        SendConfigElement(EConfigElement.PowerOnButtonEnable, Cfg.StartStopSettings.PowerOnButtonEnable.ToNumber());
        SendConfigElement(EConfigElement.PowerOnButtonTime, Cfg.StartStopSettings.PowerOnButtonSeconds);
        SendConfigElement(EConfigElement.PowerSaveEnable, Cfg.StartStopSettings.PowersaveEnable.ToNumber());


        SendConfigElement(EConfigElement.PowerOffMode, Cfg.StartStopSettings.PowerOffMode.ToNumber());
        SendConfigElement(EConfigElement.PowerOffTimer, Cfg.AlarmSettings.WakeupTimerMinutes);
        SendConfigElement(EConfigElement.WakeupWeekendEnable, Cfg.AlarmSettings.WakeUpWeekendEnable.ToNumber());


        SendConfigElement(EConfigElement.ModusReset, 1);  // resets the Strompi3 to default settings

        Console.WriteLine("Transfer Successful");
    }


    /// <summary>
    /// transmits a configuration element and its associated value to a port ensuring proper timing between commands
    /// ensuring successful communication to the Strompi3.
    /// </summary>
    /// <param name="configElement"></param>
    /// <param name="value"></param>
    private void SendConfigElement(EConfigElement configElement, int value)
    {
        //"set-config {(int)configElement} {value}", BreakShort, "\r",BreakLong
        string[] cmds = new string[] { $"set-config {(int)configElement} {value}", "\r" };
        int[]? delays = new int[] { BreakShort, BreakLong };


        string response = PortManager.SendCommand(cmds, delays, false,0);


        Console.WriteLine($"serial Write {(int)configElement} {value} transfer successfull..");
    }



    /// <summary>
    /// command to shutdown the Strompi3, in case a second power-source is enabled.
    ///<para>
    /// <remarks>Requires serial-mode</remarks></para>
    /// </summary>
    public void PowerOffStromPi3()
    {
        //"quit", BreakShort, "\r","poweroff", Sleep(1000), "\r"
        string[] cmds = new string[] { "quit", "\r", "poweroff", "\r"};
        int[]? delays = new int[] { BreakShort,0, 1000 };

        PortManager.SendCommand(cmds, delays, false, 0);

        Console.WriteLine($"Send poweroff (shutdown) to Strompi3 successfull..");
    }



    /// <summary>
    /// The method replicates the essential process of the Python script by comparing the current Raspberry
    /// timestamp (DateTime.Now) with the most recently obtained StromPi3 timestamp via ReceiveStatus() to
    /// decide which time to adopt. Essentially, as in the Python script, it checks whether the Raspberry
    /// is "newer" (rpiDateTime > Cfg.CurrentDateTime) and, in that case, updates the StromPi3's RTC using serial commands;
    /// otherwise, the Raspberry is synchronized.
    /// <para>Requires serial-mode</para>
    /// <remarks>The functionality of RTCSerial.py from joy-it is ported by this method.The original py-script
    /// uses commands 'Q', '\r', 'date-rpi' and 'time-rpi' to read the current datetime
    /// of Strompi3. This steps could not be implemented successfully and were replaced by calling 'ReceiveStatus'.
    /// </remarks>
    /// </summary>
    public void SyncRtc()
    {
        InitializePort(); // send "quit" command to Strompi3

        Console.WriteLine("TimeSync-Process | Please Wait");
        Console.WriteLine($"StromPi3: Current dateTime {Cfg.CurrentDateTime} ");
        var rpiDateTime = DateTime.Now;
        Console.WriteLine($"Raspi: Current dateTime {rpiDateTime} ");

        if (rpiDateTime > Cfg.CurrentDateTime) // sync the Strompi
        {
            Console.WriteLine("The date und time will be synced: Raspberry Pi -> StromPi'");

            int dayOfWeekPython = (int)rpiDateTime.DayOfWeek;

            // map value of sunday (0 in .net to 7 on Strompi3)
            if (dayOfWeekPython == 0) dayOfWeekPython = 7;


            string argumentsDate = $"{rpiDateTime.Day:D2} {rpiDateTime.Month:D2} {rpiDateTime.Year % 100:D2} {dayOfWeekPython}";

            Console.WriteLine($"serial write 'set-date {argumentsDate}'");

            // "Q", Sleep(1000), "\r", Sleep(1000),"set-date {argumentsDate}", BreakLong, "\r", Sleep(1000)
            string[] cmdsDate = new string[] { "Q", "\r", $"set-date {argumentsDate}", "\r"};
            int[]? delaysDate = new int[] { 1000, 1000, BreakLong, 1000 };
            
            PortManager.SendCommand(cmdsDate, delaysDate, false, 0);


            string argumentsTime = $"{rpiDateTime.Hour:D2} {rpiDateTime.Minute:D2} {rpiDateTime.Second:D2}";

            Console.WriteLine($"serial write 'set-clock {argumentsTime}'");

            // Sende "set-clock" Befehl, um die Uhrzeit zu setzen
            Thread.Sleep(100);
            //"set-clock {argumentsTime}", BreakLong, "\r"
            string[] cmds = new string[] { $"set-clock {argumentsTime}", "\r"};
            int[]? delays = new int[] { BreakLong, 0 };

            PortManager.SendCommand(cmds, delays, false, 0);
            
            Cfg = ReceiveStatus();  // re-read to get the updated datetime

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("The date und time has been synced: Raspberry Pi -> StromPi'");
            Console.WriteLine($"Strompi3 is up-to-date:  {Cfg.CurrentDateTime}");
            Console.WriteLine("-----------------------------------");
        }

        if (rpiDateTime < Cfg.CurrentDateTime) // sync the Raspi 
        {
            //TODO: not tested so far..
            Console.WriteLine("The date und time will be synced: StromPi -> Raspberry Pi'");
            Os.SetSystemDateTime(Cfg.CurrentDateTime);

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("The date und time has been synced: StromPi -> Raspberry Pi'");
            Console.WriteLine("-----------------------------------");
        }
    }


    public override string ToString()
    {
        return Cfg.ToString();
    }



    /// <summary>
    /// Creates a predefined <see cref="StromPi3"/> configuration with expected values for the BE102-config.
    /// </summary>
    /// <returns>A <see cref="StromPi3"/> instance representing the expected configuration for the BE102-config.</returns>
    public static StromPi3 CreateExpectedConfigForBe102Manually()
    {
        var result = new StromPi3
        {
            Cfg =
            {
                SerialLessEnable = false,
                PriorityModeMode = EPriorityMode.Wide_mUSB_Battery,
                ShutdownEnable = true,
                ShutdownSeconds = 600,
                PowerFailWarningEnable = true,
                BatteryHat =
                {
                    BatteryShutdownLevel = EBatteryShutdownLevel.QuarterLeft
                },
                AlarmSettings =
                {
                    WakeupEnable = false,
                    WakeUpHour = 0,
                    WakeUpMinute = 0,
                    WakeUpDay = 1,
                    WakeUpMonth = 1,
                    WakeUpWeekday = EWeekday.Monday,
                    Mode = EAlarmMode.TimeAlarm,
                    IntervalAlarmEnable = false,
                    WakeUpWeekendEnable =true,
                    WakeupTimerMinutes = 30,
                },
                StartStopSettings =
                {
                    PowerOnButtonEnable = true,
                    PowerOnButtonSeconds = 30,
                    PowerOffMode = true,
                    PowersaveEnable = false
                }
            }
        };


        return result;
    }
}

