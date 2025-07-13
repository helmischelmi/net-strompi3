using Strompi3Lib.batteryHat;
using Strompi3Lib.Common;
using System;
using System.Collections.Generic;
using Spectre.Console;

namespace Strompi3Lib;

/// <summary>
/// represents the StromPi3 configuration
/// </summary>
public class StromPi3Configuration
{
    public DateTime CurrentDateTime { get; private set; }
    public EPriorityMode PriorityModeMode { get; internal set; }
    public AlarmSettings AlarmSettings { get; }
    public BatteryHat BatteryHat { get; }
    public StartStopSettings StartStopSettings { get; }
    public VoltageMeter VoltageMeter { get; }

    public bool ShutdownEnable { get; set; }
    public int ShutdownSeconds { get; internal set; }
    public bool PowerFailWarningEnable { get; internal set; }
    public bool SerialLessEnable { get; internal set; }

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
        PriorityModeMode = EPriorityMode.nothing;
        var modus = Convert.ToInt32(sp3Modus);

        if (modus >= 1 && modus <= 6)
        {
            PriorityModeMode = (EPriorityMode)modus;
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

    /// <summary>
    /// Determines whether the current configuration matches the expected one as parameter.
    /// </summary>
    /// <param name="expectedConfig">The expected configuration to compare against.</param>
    /// <returns><see langword="true"/> if the specified configuration matches the expected settings; otherwise, <see
    /// langword="false"/>. </returns>
    public bool MatchesExpectedConfiguration(StromPi3Configuration expectedConfig)
    {
        List<string> misMatches = new List<string>();

        if (SerialLessEnable != expectedConfig.SerialLessEnable)
        {
            misMatches.Add($"SerialLessEnable: {SerialLessEnable} != {expectedConfig.SerialLessEnable} (expected)");
        }

        if (PriorityModeMode != expectedConfig.PriorityModeMode)
        {
            misMatches.Add($"PriorityModeMode: {PriorityModeMode} != {expectedConfig.PriorityModeMode} (expected)");
        }

        if (ShutdownEnable != expectedConfig.ShutdownEnable)
        {
            misMatches.Add($"ShutdownEnable: {ShutdownEnable} != {expectedConfig.ShutdownEnable} (expected)");
        }
        if (ShutdownSeconds != expectedConfig.ShutdownSeconds)
        {
            misMatches.Add($"ShutdownSeconds: {ShutdownSeconds} != {expectedConfig.ShutdownSeconds} (expected)");
        }
        if (PowerFailWarningEnable != expectedConfig.PowerFailWarningEnable)
        {
            misMatches.Add($"PowerFailWarningEnable: {PowerFailWarningEnable} != {expectedConfig.PowerFailWarningEnable}  (expected)");
        }
        if (BatteryHat.BatteryShutdownLevel != expectedConfig.BatteryHat.BatteryShutdownLevel)
        {
            misMatches.Add($"BatteryHat.BatteryShutdownLevel: {BatteryHat.BatteryShutdownLevel} != {expectedConfig.BatteryHat.BatteryShutdownLevel} (expected)");
        }

        if (AlarmSettings.WakeupEnable != expectedConfig.AlarmSettings.WakeupEnable)
        {
            misMatches.Add($"AlarmSettings.WakeupEnable: {AlarmSettings.WakeupEnable} != {expectedConfig.AlarmSettings.WakeupEnable} (expected)");
        }
        if (AlarmSettings.WakeUpHour != expectedConfig.AlarmSettings.WakeUpHour)
        {
            misMatches.Add($"AlarmSettings.WakeUpHour: {AlarmSettings.WakeUpHour} != {expectedConfig.AlarmSettings.WakeUpHour} (expected)");
        }
        if (AlarmSettings.WakeUpMinute != expectedConfig.AlarmSettings.WakeUpMinute)
        {
            misMatches.Add($"AlarmSettings.WakeUpMinute: {AlarmSettings.WakeUpMinute} != {expectedConfig.AlarmSettings.WakeUpMinute} (expected)");
        }
        if (AlarmSettings.WakeUpDay != expectedConfig.AlarmSettings.WakeUpDay)
        {
            misMatches.Add($"AlarmSettings.WakeUpDay: {AlarmSettings.WakeUpDay} != {expectedConfig.AlarmSettings.WakeUpDay} (expected)");
        }
        if (AlarmSettings.WakeUpMonth != expectedConfig.AlarmSettings.WakeUpMonth)
        {
            misMatches.Add($"AlarmSettings.WakeUpMonth: {AlarmSettings.WakeUpMonth} != {expectedConfig.AlarmSettings.WakeUpMonth} (expected)");
        }
        if (AlarmSettings.WakeUpWeekday != expectedConfig.AlarmSettings.WakeUpWeekday)
        {
            misMatches.Add($"AlarmSettings.WakeUpWeekday: {AlarmSettings.WakeUpWeekday} != {expectedConfig.AlarmSettings.WakeUpWeekday} (expected)");
        }
        if (AlarmSettings.Mode != expectedConfig.AlarmSettings.Mode)
        {
            misMatches.Add($"AlarmSettings.Mode: {AlarmSettings.Mode} != {expectedConfig.AlarmSettings.Mode} (expected)");
        }
        if (AlarmSettings.IntervalAlarmEnable != expectedConfig.AlarmSettings.IntervalAlarmEnable)
        {
            misMatches.Add($"AlarmSettings.IntervalAlarmEnable: {AlarmSettings.IntervalAlarmEnable} != {expectedConfig.AlarmSettings.IntervalAlarmEnable} (expected)");
        }
        if (AlarmSettings.WakeupTimerMinutes != expectedConfig.AlarmSettings.WakeupTimerMinutes)
        {
            misMatches.Add($"AlarmSettings.WakeupTimerMinutes: {AlarmSettings.WakeupTimerMinutes} != {expectedConfig.AlarmSettings.WakeupTimerMinutes}  (expected)");
        }

        if (AlarmSettings.WakeUpWeekendEnable != expectedConfig.AlarmSettings.WakeUpWeekendEnable)
        {
            misMatches.Add($"AlarmSettings.WakeUpWeekendEnable: {AlarmSettings.WakeUpWeekendEnable} != {expectedConfig.AlarmSettings.WakeUpWeekendEnable}  (expected)");
        }

        if (StartStopSettings.PowerOnButtonEnable != expectedConfig.StartStopSettings.PowerOnButtonEnable)
        {
            misMatches.Add($"StartStopSettings.PowerOnButtonEnable: {StartStopSettings.PowerOnButtonEnable} != {expectedConfig.StartStopSettings.PowerOnButtonEnable} (expected)");
        }

        if (StartStopSettings.PowerOnButtonSeconds != expectedConfig.StartStopSettings.PowerOnButtonSeconds)
        {
            misMatches.Add($"StartStopSettings.PowerOnButtonSeconds: {StartStopSettings.PowerOnButtonSeconds} != {expectedConfig.StartStopSettings.PowerOnButtonSeconds}  (expected)");
        }

        if (StartStopSettings.PowerOffMode != expectedConfig.StartStopSettings.PowerOffMode)
        {
            misMatches.Add($"StartStopSettings.PowerOffMode: {StartStopSettings.PowerOffMode} != {expectedConfig.StartStopSettings.PowerOffMode} (expected)");
        }

        if (StartStopSettings.PowersaveEnable != expectedConfig.StartStopSettings.PowersaveEnable)
        {
            misMatches.Add($"StartStopSettings.PowersaveEnable: {StartStopSettings.PowersaveEnable} != {expectedConfig.StartStopSettings.PowersaveEnable} (expected)");
        }

        Console.WriteLine();
        foreach (var msg in misMatches)
        {
            Console.WriteLine(msg);
        }

        Console.WriteLine();
        return misMatches.Count == 0;
    }



    public override string ToString()
    {
        // Create a table
        AnsiConsole.Foreground = Color.Yellow;
        var table = new Table();
        table.Title = new TableTitle("[yellow]StromPi-Status[/]");
        table.LeftAligned();
        table.BorderStyle = new Style(foreground: Color.Yellow);


        // Add some columns
        table.AddColumn($"[yellow]Firmware: {FirmwareVersion}[/]");
        table.AddColumn($"[yellow]DateTime: {CurrentDateTime}[/]");

        table.Columns[0].Width(40);
        table.Columns[1].Width(40);

        table.AddRow($"[yellow]Serialless-Mode: {SerialLessEnable}[/]", $"[yellow]Power Save Mode: {StartStopSettings.PowersaveEnable}[/]");
        table.AddRow($"[yellow]{new string('─', 40)}[/]", "[yellow]" + new string('─', 40) + "[/]");
        table.AddRow($"[yellow]Power-Priority: {ConverterHelper.GetEnumDescription(PriorityModeMode)}[/]", $"[yellow]Power-Source:   {PowerInputSource}[/]");
        table.AddEmptyRow();
        table.AddRow($"[yellow]Powerfail Warning: {PowerFailWarningEnable}[/]", $"[yellow]Battery-Level Shutdown: {ConverterHelper.GetEnumDescription(BatteryHat.BatteryShutdownLevel).EscapeMarkup()}[/]");
        table.AddRow($"[yellow]Powerfail-Counter: {PowerFailureCounter}[/]", "");
        table.AddRow($"[yellow]Pi Shutdown Timer Mode: {ShutdownEnable}[/]", $"[yellow]Timer: {ShutdownSeconds} seconds[/]");
        table.AddRow(
            $"[yellow]{new string('─', 12)} Power ON / OFF {new string('─', 12)}[/]",
            "[yellow]" + new string('─', 40) + "[/]");
        table.AddRow($"[yellow]PowerOff Mode: {StartStopSettings.PowerOffMode}[/]", "");
        table.AddRow($"[yellow]PowerOn-Button: {StartStopSettings.PowerOnButtonEnable}[/]", $"[yellow]Timer: {StartStopSettings.PowerOnButtonSeconds} seconds[/]");
        table.AddRow(
            $"[yellow]{new string('─', 14)} Alarm Modes {new string('─', 13)}[/]",
            "[yellow]" + new string('─', 40) + "[/]");
        table.AddRow($"[yellow]Alarm-Mode: {(StartStopSettings.PoweroffTimeEnableMode ? "Minute WakeUp-Alarm" : AlarmSettings.Mode)}[/]", "");
        table.AddRow($"[yellow]Alarm Weekday:  {AlarmSettings.WakeUpWeekday}[/]", "");
        table.AddRow($"[yellow]WakeUp-Alarm:   {AlarmSettings.WakeupEnable}[/]", $"[yellow]Alarm at: {AlarmSettings.WakeUpDay:00}:{AlarmSettings.WakeUpMonth:00} [[DD:MM]] {AlarmSettings.WakeUpHour:00}:{AlarmSettings.WakeUpMinute:00} [[hh:mm]][/]");
        table.AddRow($"[yellow]Weekend Wakeup: {AlarmSettings.WakeUpWeekendEnable}[/]", "");
        table.AddRow($"[yellow]Minute Wakeup Timer: {AlarmSettings.WakeupTimerMinutes} minutes[/]", "");
        table.AddEmptyRow();
        table.AddRow($"[yellow]PowerOff-Alarm: {AlarmSettings.PowerOffEnable}[/]", $"[yellow]With Off-Time: {AlarmSettings.PowerOffHour:00}:{AlarmSettings.PowerOffMinute:00} [[hh:mm]][/]");
        table.AddRow($"[yellow]Interval-Alarm: {AlarmSettings.IntervalAlarmEnable}[/]", $"[yellow]On-minutes: {AlarmSettings.IntervalAlarmOnMinutes}, Off-minutes: {AlarmSettings.IntervalAlarmOffMinutes}[/]");
        table.AddRow(
            $"[yellow]{new string('─', 12)} Voltage Levels {new string('─', 12)}[/]",
            "[yellow]" + new string('─', 40) + "[/]");
        table.AddRow($"[yellow]Wide-Range          = {VoltageMeter.WideRangeVolt:F2} Volt[/]", $"[yellow]microUSB = {VoltageMeter.mUsbVolt:F2} Volt[/]");
        table.AddRow($"[yellow]Output              = {VoltageMeter.OutputVolt:F2} Volt[/]", "");
        table.AddRow($"[yellow]LifePo4-Battery-Hat = {VoltageMeter.BatteryVolt:F2} Volt[/]", $"[yellow]Level: {ConverterHelper.GetEnumDescription(BatteryHat.Level).EscapeMarkup()}, Charging [[{BatteryHat.IsCharging}]][/]");

        AnsiConsole.Write(table);// Render the table to the console

        // The rest of the method remains unchanged
        string status = string.Empty;
        //string status = "------------------------- StromPi-Status ------------------------------------" + Environment.NewLine;
        //status += $"Firmware:       {FirmwareVersion,-27} DateTime: {CurrentDateTime} " + Environment.NewLine;
        //status += $"Serialless-Mode: {SerialLessEnable,-26} Power Save Mode: {StartStopSettings.PowersaveEnable}" + Environment.NewLine;
        //status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        //status += $"Power-Priority: {ConverterHelper.GetEnumDescription(PriorityModeMode),-27} Power-Source:   {PowerInputSource}" + Environment.NewLine;
        //status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        //status += $"Powerfail Warning: {PowerFailWarningEnable,-24} Battery-Level Shutdown: {ConverterHelper.GetEnumDescription(BatteryHat.BatteryShutdownLevel)}" + Environment.NewLine;
        //status += $"Powerfail-Counter: {PowerFailureCounter}" + Environment.NewLine;
        //status += $"Pi Shutdown Timer Mode: {ShutdownEnable,-19} Timer: {ShutdownSeconds} seconds" + Environment.NewLine;
        //status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        //status += $"PowerOff Mode: {StartStopSettings.PowerOffMode}" + Environment.NewLine;
        //status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        //status += $"PowerOn-Button: {StartStopSettings.PowerOnButtonEnable,-27} Timer: {StartStopSettings.PowerOnButtonSeconds} seconds" + Environment.NewLine;
        //status += "-----------------------------------------------------------------------------" + Environment.NewLine;

        //status += "----------------------------- Alarm-Modes -----------------------------------" + Environment.NewLine;
        //if (StartStopSettings.PoweroffTimeEnableMode)
        //    status += $" Alarm-Mode: Minute WakeUp-Alarm" + Environment.NewLine;
        //else
        //{
        //    status += $"Alarm-Mode: {AlarmSettings.Mode}" + Environment.NewLine;
        //}
        //status += $"Alarm Weekday: {AlarmSettings.WakeUpWeekday}" + Environment.NewLine;

        //status += $"WakeUp-Alarm: {AlarmSettings.WakeupEnable,-25} Alarm at: {AlarmSettings.WakeUpDay:00}:{AlarmSettings.WakeUpMonth:00} [DD:MM] {AlarmSettings.WakeUpHour:00}:{AlarmSettings.WakeUpMinute:00} [hh:mm]" + Environment.NewLine;

        //status += $"Weekend Wakeup: {AlarmSettings.WakeUpWeekendEnable}" + Environment.NewLine;
        //status += $"Minute Wakeup Timer: {AlarmSettings.WakeupTimerMinutes} minutes" + Environment.NewLine;
        //status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        //status += $"PowerOff-Alarm: {AlarmSettings.PowerOffEnable,-27} With Off-Time: {AlarmSettings.PowerOffHour:00}:{AlarmSettings.PowerOffMinute:00} [hh:mm]" + Environment.NewLine;
        //status += "-----------------------------------------------------------------------------" + Environment.NewLine;
        //status += $"Interval-Alarm: {AlarmSettings.IntervalAlarmEnable,-27} On-minutes: {AlarmSettings.IntervalAlarmOnMinutes}, Off-minutes: {AlarmSettings.IntervalAlarmOffMinutes}" + Environment.NewLine;
        //status += "-----------------------------------------------------------------------------" + Environment.NewLine;

        //status += "------------------------------ Voltage-Levels -------------------------------" + Environment.NewLine;
        //status += $"Wide-Range          = {VoltageMeter.WideRangeVolt:F2} Volt            microUSB = {VoltageMeter.mUsbVolt:F2} Volt" + Environment.NewLine;
        //status += $"Output              =  {VoltageMeter.OutputVolt:F2} Volt" + Environment.NewLine;
        //status += $"LifePo4-Battery-Hat =  {VoltageMeter.BatteryVolt:F2} Volt            Level: {ConverterHelper.GetEnumDescription(BatteryHat.Level)}, " +
        //          $"Charging [{BatteryHat.IsCharging}]" + Environment.NewLine;
        //status += "-----------------------------------------------------------------------------" + Environment.NewLine;

        return status;
    }
}