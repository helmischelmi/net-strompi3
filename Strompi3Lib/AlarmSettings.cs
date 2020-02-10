using System;

namespace Strompi3Lib
{

    public class AlarmSettings
    {
        public bool Enabled { get; private set; }
        public bool PowerOffEnabled { get; private set; }
        public bool IntervalAlarmEnabled { get; private set; }
        public string IntervalAlarmOnMinutes { get; private set; }
        public string IntervalAlarmOffMinutes { get; private set; }
        public EAlarmMode Mode { get; private set; }
        public int PowerOffHours { get; private set; }
        public int PowerOffMinutes { get; private set; }
        public int Hour { get; private set; }
        public int Minute { get; private set; }
        public int Day { get; private set; }
        public int Month { get; private set; }
        public EWeekday Weekday { get; private set; }

        public void GetAlarmEnabled(string sp3AlarmEnable)
        {
            Enabled = ConverterHelper.EnabledDisabledConverter(sp3AlarmEnable, "sp3AlarmEnable");
        }
        public void GetAlarmMode(string sp3AlarmMode)
        {
            Mode = EAlarmMode.nothing;
            var mode = Convert.ToInt32(sp3AlarmMode);

            if (mode >= 1 && mode <= 3)
            {
                Mode = (EAlarmMode)mode;
            }
        }

        public void GetAlarmPowerOffEnabled(string sp3AlarmPoweroff)
        {
            PowerOffEnabled = ConverterHelper.EnabledDisabledConverter(sp3AlarmPoweroff, "sp3AlarmPoweroff");
        }

        public void GetAlarmIntervall(string sp3IntervalAlarm, string intervallAlarmOnTimeMinutes, string intervallAlarmOffTimeMinutes)
        {
            IntervalAlarmOnMinutes = intervallAlarmOnTimeMinutes;
            IntervalAlarmOffMinutes = intervallAlarmOffTimeMinutes;
            IntervalAlarmEnabled = ConverterHelper.EnabledDisabledConverter(sp3IntervalAlarm, "sp3IntervalAlarm");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sp3AlarmHour"></param>
        /// <param name="sp3AlarmMin"></param>
        /// <param name="sp3AlarmDay"></param>
        /// <param name="sp3AlarmMonth"></param>
        /// <param name="sp3AlarmWeekday"></param>
        public void GetAlarmDateTime(string sp3AlarmHour, string sp3AlarmMin, string sp3AlarmDay, string sp3AlarmMonth, string sp3AlarmWeekday)
        {
            try
            {
                Hour = Convert.ToInt32(sp3AlarmHour);
                Minute = Convert.ToInt32(sp3AlarmMin);
                Day = Convert.ToInt32(sp3AlarmDay);
                Month = Convert.ToInt32(sp3AlarmMonth);

                Weekday = EWeekday.nothing;
                var weekday = Convert.ToInt32(sp3AlarmWeekday);

                if (weekday >= 1 && weekday <= 7)
                {
                    Weekday = (EWeekday)weekday;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
        }

        public void GetAlarmPowerOffTimePeriod(string sp3PowerOffHours, string sp3PowerOffMinutes)
        {
            PowerOffHours = Convert.ToInt32(sp3PowerOffHours);
            PowerOffMinutes = Convert.ToInt32(sp3PowerOffMinutes);
        }

    }
}
