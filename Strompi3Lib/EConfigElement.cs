using System.ComponentModel;

namespace Strompi3Lib;

public enum EConfigElement
{
    /// <summary>
    /// Resets the StromPi3 to default settings.
    /// </summary>
    [Description("[ModusReset]")]
    ModusReset = 0,

    [Description("[InputPriority]")]
    InputPriority = 1,

    [Description("AlarmModeA")]
    AlarmModeA = 2,

    [Description("AlarmModeB")]
    AlarmModeB = 3,

    [Description("AlarmModeC")]
    AlarmModeC = 4,

    [Description("AlarmModeD")]
    AlarmModeD = 26,

        
    [Description("AlarmPowerOff")]
    AlarmPowerOff = 5,

    [Description("AlarmMinutes")]
    AlarmMinutes = 6,

    [Description("AlarmHours")]
    AlarmHours = 7,

    [Description("AlarmMinutesOff")]
    AlarmMinutesOff = 8,

    [Description("AlarmHoursOff")]
    AlarmHoursOff = 9,

    [Description("AlarmDay")]
    AlarmDay = 10,

    [Description("AlarmMonth")]
    AlarmMonth = 11,

    [Description("AlarmWeekday")]
    AlarmWeekday = 12,

    [Description("AlarmEnable")]
    AlarmEnable = 13,

    [Description("ShutdownEnable")]
    ShutdownEnable = 14,

    [Description("ShutdownTimer")]
    ShutdownTimer = 15,

    [Description("WarningEnable")]
    WarningEnable = 16,

    [Description("SerialLessMode")]
    SerialLessMode = 17,

    [Description("ShutdownBatteryLevel")]
    ShutdownBatteryLevel = 18,

    [Description("IntervalAlarmEnable")]
    IntervalAlarmEnable = 19,

    [Description("IntervalAlarmOnTime")]
    IntervalAlarmOnTime = 20,

    [Description("IntervalAlarmOffTime")]
    IntervalAlarmOffTime = 21,

    [Description("PowerOnButtonEnable")]
    PowerOnButtonEnable = 22,
        
    [Description("PowerOnButtonTime")]
    PowerOnButtonTime = 23,
        
    [Description("PowerSaveEnable")]
    PowerSaveEnable = 24,

    [Description("PowerOffMode")]
    PowerOffMode = 25,

    [Description("PowerOffTime")]
    PowerOffTimer = 27,

    [Description("WakeupWeekendEnable")]
    WakeupWeekendEnable = 28,
}