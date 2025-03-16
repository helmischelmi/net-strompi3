using System.ComponentModel;

namespace Strompi3Lib;

public enum EAlarmMode
{
    [Description("nothing")]
    nothing = 0,

    [Description("Time-Alarm")]
    TimeAlarm = 1,

    [Description("Date-Alarm")]
    DateAlarm = 2,

    [Description("Weekday-Alarm")]
    WeekdayAlarm = 3,

    [Description("Wakeup-Timer")]
    WakeupTimer = 4
}