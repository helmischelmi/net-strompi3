using Strompi3Lib.batteryHat;
using Strompi3Lib.Common;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Strompi3Lib.serialPort;

public static class SerialPortExtension
{
    private const int Shortbreak = 100; // millisecs
    private const int Longbreak = 200; // millisecs

    public static SerialPort GetInstance(this SerialPort port, SerialPortConfigurator param)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="port"></param>
    /// <param name="configElement"></param>
    /// <param name="value"></param>
    public static void SendConfigElement(this SerialPort port, EConfigElement configElement, int value)
    {
        bool isPortOpen = port.IsOpen;
        if (!isPortOpen) port.Open();

        port.Write($"set-config {(int)configElement} {value}");
        Thread.Sleep(Shortbreak);
        port.Write("\r");
        Thread.Sleep(Longbreak);

        if (!isPortOpen) port.Close();

        Console.WriteLine($"serial Write {(int)configElement} {value} transfer successfull..");
    }


    /// <summary>
    /// Reads all status-related characteristics of the Strompi3.
    /// <para>
    /// <remarks>Requires serial-mode</remarks></para>
    /// </summary>
    public static string ReceiveConfiguration(this SerialPort port, StromPi3Settings settings, bool bSilent = true)
    {
        if (!Os.ShowAvailableSerialPorts("tty", true)) return string.Empty;

        bool isPortOpen = port.IsOpen;
        if (!isPortOpen) port.Open();

        port.Write("quit");
        port.Write("\r"); // \x0d = {13} Carriage Return
        port.Write("status-rpi");
        port.Write("\r"); // \x0d = {13} Carriage Return

        string sp3Time = port.ReadLine();  // timeout
        string sp3Date = port.ReadLine();

        settings.SetRTCDateTime(sp3Time, sp3Date);

        string sp3_weekday = port.ReadLine();  // not used
        string sp3_modus = port.ReadLine();
        settings.SetInputPriorityMode(sp3_modus);

        settings.AlarmSettings.GetAlarmEnabled(port.ReadLine());
        settings.AlarmSettings.GetAlarmMode(port.ReadLine());

        string sp3AlarmHour = port.ReadLine();
        string sp3AlarmMin = port.ReadLine();
        string sp3AlarmDay = port.ReadLine();
        string sp3AlarmMonth = port.ReadLine();
        string sp3AlarmWeekday = port.ReadLine();
        settings.AlarmSettings.GetAlarmDateTime(sp3AlarmHour, sp3AlarmMin, sp3AlarmDay, sp3AlarmMonth, sp3AlarmWeekday);

        settings.AlarmSettings.GetAlarmPowerOffEnabled(port.ReadLine());

        string alarmPowerOffHours = port.ReadLine();
        string alarmPowerOffMinutes = port.ReadLine();
        settings.AlarmSettings.GetAlarmPowerOffTimePeriod(alarmPowerOffHours, alarmPowerOffMinutes);

        string sp3ShutdownEnable = port.ReadLine();
        string sp3ShutdownSeconds = port.ReadLine();
        settings.SetShutDown(sp3ShutdownEnable, Convert.ToInt32(sp3ShutdownSeconds), (int)EBatteryLevel.nothing);

        settings.SetPowerFailWarningEnable(port.ReadLine());
        settings.SetSerialLessEnable(port.ReadLine());

        string sp3IntervalAlarm = port.ReadLine();
        string sp3IntervallAlarmOnTimeMinutes = port.ReadLine();
        string sp3IntervallAlarmOffTimeMinutes = port.ReadLine();
        settings.AlarmSettings.GetAlarmIntervall(sp3IntervalAlarm, sp3IntervallAlarmOnTimeMinutes, sp3IntervallAlarmOffTimeMinutes);

        string sp3BatLevelShutdown = port.ReadLine();
        string sp3BatLevel = port.ReadLine();
        string sp3Charging = port.ReadLine();
        settings.BatteryHat.GetBatteryHat(Convert.ToInt32(sp3BatLevelShutdown), sp3BatLevel, sp3Charging);

        string sp3PowerOnButtonEnable = port.ReadLine();
        string sp3PowerOnButtonTime = port.ReadLine();
        string sp3PowersaveEnable = port.ReadLine();
        string sp3PoweroffMode = port.ReadLine();
        string poweroffTimeEnableMode = port.ReadLine();
        settings.StartStopSettings.GetStartStopSettings(sp3PowerOnButtonEnable, sp3PowerOnButtonTime, sp3PowersaveEnable, sp3PoweroffMode, poweroffTimeEnableMode);

        string wakeupTimerMinutes = port.ReadLine();
        string sp3WakeupweekendEnable = port.ReadLine();
        settings.AlarmSettings.GetAlarmWakeupTimerAndWeekend(wakeupTimerMinutes, sp3WakeupweekendEnable);

        string sp3AdcWide = port.ReadLine();
        string sp3AdcBat = port.ReadLine();
        string sp3AdcUsb = port.ReadLine();
        string outputVolt = port.ReadLine();
        settings.VoltageMeter.GetVoltageMeter(sp3AdcWide, sp3AdcBat, sp3AdcUsb, outputVolt);

        settings.SetOutputStatus(port.ReadLine());
        settings.SetPowerFailureCounter(port.ReadLine());
        settings.SetFirmwareVersion(port.ReadLine());

        if (!isPortOpen) port.Close();

        return settings.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="port"></param>
    /// <param name="settings"></param>
    public static void SendConfiguration(this SerialPort port, StromPi3Settings settings)
    {
        port.SendConfigElement(EConfigElement.InputPriority, (int)settings.PriorityMode);

        switch (settings.AlarmSettings.Mode)
        {
            case EAlarmMode.TimeAlarm:
                port.SendConfigElement(EConfigElement.AlarmModeA, 0);
                port.SendConfigElement(EConfigElement.AlarmModeB, 0);
                port.SendConfigElement(EConfigElement.AlarmModeC, 1);
                port.SendConfigElement(EConfigElement.AlarmModeD, 0);
                break;
            case EAlarmMode.DateAlarm:
                port.SendConfigElement(EConfigElement.AlarmModeA, 1);
                port.SendConfigElement(EConfigElement.AlarmModeB, 0);
                port.SendConfigElement(EConfigElement.AlarmModeC, 0);
                port.SendConfigElement(EConfigElement.AlarmModeD, 0);
                break;

            case EAlarmMode.WeekdayAlarm:
                port.SendConfigElement(EConfigElement.AlarmModeA, 0);
                port.SendConfigElement(EConfigElement.AlarmModeB, 1);
                port.SendConfigElement(EConfigElement.AlarmModeC, 0);
                port.SendConfigElement(EConfigElement.AlarmModeD, 0);
                break;
            case EAlarmMode.WakeupTimer:
                port.SendConfigElement(EConfigElement.AlarmModeA, 0);
                port.SendConfigElement(EConfigElement.AlarmModeB, 0);
                port.SendConfigElement(EConfigElement.AlarmModeC, 0);
                port.SendConfigElement(EConfigElement.AlarmModeD, 1);
                break;
            default:
                throw new NotImplementedException($"Unknown EAlarmMode {settings.AlarmSettings.Mode}");
        }

        port.SendConfigElement(EConfigElement.AlarmPowerOff, settings.AlarmSettings.PowerOffEnable.ToNumber());
        port.SendConfigElement(EConfigElement.AlarmMinutes, settings.AlarmSettings.WakeUpMinute);
        port.SendConfigElement(EConfigElement.AlarmHours, settings.AlarmSettings.WakeUpHour);
        port.SendConfigElement(EConfigElement.AlarmMinutesOff, settings.AlarmSettings.PowerOffMinute);
        port.SendConfigElement(EConfigElement.AlarmHoursOff, settings.AlarmSettings.PowerOffHour);
        port.SendConfigElement(EConfigElement.AlarmDay, settings.AlarmSettings.WakeUpDay);
        port.SendConfigElement(EConfigElement.AlarmMonth, settings.AlarmSettings.WakeUpMonth);
        port.SendConfigElement(EConfigElement.AlarmWeekday, (int)settings.AlarmSettings.WakeUpWeekday);
        port.SendConfigElement(EConfigElement.AlarmEnable, settings.AlarmSettings.WakeupEnable.ToNumber());

        port.SendConfigElement(EConfigElement.ShutdownEnable, settings.ShutdownEnable.ToNumber());
        port.SendConfigElement(EConfigElement.ShutdownTimer, settings.ShutdownSeconds);
        port.SendConfigElement(EConfigElement.WarningEnable, settings.PowerFailWarningEnable.ToNumber());
        port.SendConfigElement(EConfigElement.SerialLessMode, settings.SerialLessEnable.ToNumber());
        port.SendConfigElement(EConfigElement.ShutdownBatteryLevel, (int)settings.BatteryHat.BatteryShutdownLevel);

        port.SendConfigElement(EConfigElement.IntervalAlarmEnable, settings.AlarmSettings.IntervalAlarmEnable.ToNumber());
        port.SendConfigElement(EConfigElement.IntervalAlarmOnTime, settings.AlarmSettings.IntervalAlarmOnMinutes);
        port.SendConfigElement(EConfigElement.IntervalAlarmOffTime, settings.AlarmSettings.IntervalAlarmOffMinutes);


        port.SendConfigElement(EConfigElement.PowerOnButtonEnable, settings.StartStopSettings.PowerOnButtonEnable.ToNumber());
        port.SendConfigElement(EConfigElement.PowerOnButtonTime, settings.StartStopSettings.PowerOnButtonSeconds);
        port.SendConfigElement(EConfigElement.PowerSaveEnable, settings.StartStopSettings.PowersaveEnable.ToNumber());


        port.SendConfigElement(EConfigElement.PowerOffMode, settings.StartStopSettings.PoweroffMode.ToNumber());
        port.SendConfigElement(EConfigElement.PowerOffTimer, settings.AlarmSettings.WakeupTimerMinutes);
        port.SendConfigElement(EConfigElement.WakeupWeekendEnable, settings.AlarmSettings.WakeUpWeekendEnable.ToNumber());

        port.SendConfigElement(EConfigElement.ModusReset, 1);

        Console.WriteLine("Transfer Successful");

    }

    public static string ShowConfigExtended(this SerialPort port)
    {
        var strEol = string.Empty;
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