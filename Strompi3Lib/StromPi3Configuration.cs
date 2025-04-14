using Strompi3Lib.batteryHat;
using Strompi3Lib.Common;
using System;

namespace Strompi3Lib;

/// <summary>
/// represents the StromPi3 configuration
/// </summary>
public class StromPi3Configuration
{
    public DateTime CurrentDateTime { get; private set; }
    public EInputPriority PriorityMode { get; private set; }
    public AlarmSettings AlarmSettings { get; }
    public BatteryHat BatteryHat { get; }
    public StartStopSettings StartStopSettings { get; }
    public VoltageMeter VoltageMeter { get; }

    public bool ShutdownEnable { get; private set; }
    public int ShutdownSeconds { get; private set; }
    public bool PowerFailWarningEnable { get; private set; }
    public bool SerialLessEnable { get; private set; }

    public EPowerInputSource PowerInputSource { get; private set; }
    public int PowerFailureCounter { get; private set; }
    public string FirmwareVersion { get; private set; }


    public StromPi3Configuration()
    {
        AlarmSettings = new AlarmSettings();
        BatteryHat = new BatteryHat();
        StartStopSettings = new StartStopSettings();
        VoltageMeter = new VoltageMeter();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sp3Time">format=hhmmss</param>
    /// <param name="sp3Date">format=yymmdd</param>
    public void GetRTCDateTime(string sp3Time, string sp3Date)
    {
        try
        {
            int intSp3Time = Convert.ToInt32(sp3Time);
            var ts = new TimeSpan(intSp3Time / 10000, intSp3Time % 10000 / 100, intSp3Time % 100);
            int isp3Date = Convert.ToInt32(sp3Date);
            int year = (isp3Date / 10000) + 2000;
            int month = isp3Date % 10000 / 100;
            int day = isp3Date % 100;
            CurrentDateTime = new DateTime(year, month, day) + ts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"***error: Conversion of current DateTime failed: time = '{sp3Time}', date= '{sp3Date}'");
            CurrentDateTime = DateTime.MinValue;
        }
    }


    public void GetInputPriorityMode(string sp3Modus)
    {
        PriorityMode = EInputPriority.nothing;
        var modus = Convert.ToInt32(sp3Modus);

        if (modus >= 1 && modus <= 6)
        {
            PriorityMode = (EInputPriority)modus;
        }
    }


    public void GetShutDown(string sp3ShutdownEnable, int sp3ShutdownSeconds, int batteryLevelShutdown)
    {
        try
        {
            ShutdownSeconds = Convert.ToInt32(sp3ShutdownSeconds);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        ShutdownEnable = ConverterHelper.EnabledDisabledConverter(sp3ShutdownEnable, "sp3ShutdownEnable");

        BatteryHat.SetShutdownLevel(batteryLevelShutdown);
    }


    public void GetPowerFailWarningEnable(string sp3WarningEnable)
    {
        PowerFailWarningEnable = ConverterHelper.EnabledDisabledConverter(sp3WarningEnable, "sp3WarningEnable");
    }


    public void GetSerialLessEnable(string sp3SerialLessMode)
    {
        SerialLessEnable = ConverterHelper.EnabledDisabledConverter(sp3SerialLessMode, "sp3SerialLessMode");
    }


    public void GetPowerSaveEnable(string sp3PowerSaveMode)
    {
        StartStopSettings.PowersaveEnable = ConverterHelper.EnabledDisabledConverter(sp3PowerSaveMode, "sp3PowerSaveMode");
    }


    public void GetPowerInputSource(string sp3PowerInputSource)
    {
        PowerInputSource = EPowerInputSource.nothing;
        var modus = Convert.ToInt32(sp3PowerInputSource);

        if (modus >= 0 && modus <= 4)
        {
            PowerInputSource = (EPowerInputSource)modus;
        }
    }


    public void GetPowerFailureCounter(string sp3PowerFailureCounter)
    {
        try
        {
            PowerFailureCounter = Convert.ToInt32(sp3PowerFailureCounter);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }


    public void GetFirmwareVersion(string sp3FirmwareVersion)
    {
        FirmwareVersion = sp3FirmwareVersion;
    }


    public override string ToString()
    {
        string status = "------------------------- StromPi-Status ------------------------------------" + Environment.NewLine;
        status += $"Firmware:       {FirmwareVersion,-27} DateTime: {CurrentDateTime} " + Environment.NewLine;
        status += $"Serialless-Mode: {SerialLessEnable, -26} Power Save Mode: {StartStopSettings.PowersaveEnable}" + Environment.NewLine;
        status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        status += $"Power-Priority: {ConverterHelper.GetEnumDescription(PriorityMode),-27} Power-Source:   {PowerInputSource}" + Environment.NewLine;
        status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        status += $"Powerfail Warning: {PowerFailWarningEnable,-24} Battery-Level Shutdown: {ConverterHelper.GetEnumDescription(BatteryHat.BatteryShutdownLevel)}" + Environment.NewLine;
        status += $"Powerfail-Counter: {PowerFailureCounter}" + Environment.NewLine;
        status += $"Pi Shutdown Timer Mode: {ShutdownEnable,-19} Timer: {ShutdownSeconds} seconds" + Environment.NewLine;
        status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        status += $"PowerOff Mode: {StartStopSettings.PoweroffMode}" + Environment.NewLine;
        status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        status += $"PowerOn-Button: {StartStopSettings.PowerOnButtonEnable,-27} Timer: {StartStopSettings.PowerOnButtonSeconds} seconds" + Environment.NewLine;
        status += "-----------------------------------------------------------------------------" + Environment.NewLine;

        status += "----------------------------- Alarm-Modes -----------------------------------" + Environment.NewLine;
        if (StartStopSettings.PoweroffTimeEnableMode)
            status += $" Alarm-Mode: Minute WakeUp-Alarm" + Environment.NewLine;
        else
        {
            status += $"Alarm-Mode: {AlarmSettings.Mode}" + Environment.NewLine;
        }
        status += $"Alarm Weekday: {AlarmSettings.WakeUpWeekday}" + Environment.NewLine;


        status += $"WakeUp-Alarm: {AlarmSettings.WakeupEnable, -25} Alarm at: {AlarmSettings.WakeUpDay:00}:{AlarmSettings.WakeUpMonth:00} [DD:MM] {AlarmSettings.WakeUpHour:00}:{AlarmSettings.WakeUpMinute:00} [hh:mm]" + Environment.NewLine;

        status += $"Weekend Wakeup: {AlarmSettings.WakeUpWeekendEnable}" + Environment.NewLine;
        status += $"Minute Wakeup Timer: {AlarmSettings.WakeupTimerMinutes} minutes" + Environment.NewLine;
        status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        status += $"PowerOff-Alarm: {AlarmSettings.PowerOffEnable,-27} With Off-Time: {AlarmSettings.PowerOffHour:00}:{AlarmSettings.PowerOffMinute:00} [hh:mm]" + Environment.NewLine;
        status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        status += $"Interval-Alarm: {AlarmSettings.IntervalAlarmEnable,-27} On-minutes: {AlarmSettings.IntervalAlarmOnMinutes}, Off-minutes: {AlarmSettings.IntervalAlarmOffMinutes}" + Environment.NewLine;
        status += "-----------------------------------------------------------------------------" + Environment.NewLine;

        status += "------------------------------ Voltage-Levels -------------------------------" + Environment.NewLine;
        status += $"Wide-Range          = {VoltageMeter.WideRangeVolt:F2} Volt            microUSB = {VoltageMeter.mUsbVolt:F2} Volt" + Environment.NewLine;
        status += $"Output              =  {VoltageMeter.OutputVolt:F2} Volt" + Environment.NewLine;
        status += $"LifePo4-Battery-Hat =  {VoltageMeter.BatteryVolt:F2} Volt            Level: {ConverterHelper.GetEnumDescription(BatteryHat.Level)}, " +
                  $"Charging [{BatteryHat.IsCharging}]" + Environment.NewLine;
        status += "-----------------------------------------------------------------------------" + Environment.NewLine;

        return status;
    }
}