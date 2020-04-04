using System;

namespace Strompi3Lib
{

    public class AlarmSettings
    {
        public bool WakeupEnable { get; private set; }
        public bool PowerOffEnable { get; private set; }
        public bool IntervalAlarmEnable { get; private set; }
        public int IntervalAlarmOnMinutes { get; private set; }
        public int IntervalAlarmOffMinutes { get; private set; }
        public EAlarmMode Mode { get; private set; }
        public int PowerOffHour { get; private set; }
        public int PowerOffMinute { get; private set; }
        public int WakeUpHour { get; private set; }
        public int WakeUpMinute { get; private set; }
        public int WakeUpDay { get; private set; }
        public int WakeUpMonth { get; private set; }
        public EWeekday WakeUpWeekday { get; private set; }

        public int WakeupTimerMinutes { get; private set; }
        public bool WakeUpWeekendEnable { get; private set; }

        public void GetAlarmEnabled(string sp3AlarmEnable)
        {
            WakeupEnable = ConverterHelper.EnabledDisabledConverter(sp3AlarmEnable, "sp3AlarmEnable");
        }
        public void GetAlarmMode(string sp3AlarmMode)
        {
            Mode = EAlarmMode.nothing;
            var mode = Convert.ToInt32(sp3AlarmMode);

            if (mode >= 1 && mode <= 4)
            {
                Mode = (EAlarmMode)mode;
            }
        }

        public void GetAlarmPowerOffEnabled(string sp3AlarmPoweroff)
        {
            PowerOffEnable = ConverterHelper.EnabledDisabledConverter(sp3AlarmPoweroff, "sp3AlarmPoweroff");
        }

        public void GetAlarmIntervall(string sp3IntervalAlarm, string intervallAlarmOnTimeMinutes, string intervallAlarmOffTimeMinutes)
        {
            IntervalAlarmOnMinutes = Convert.ToInt32(intervallAlarmOnTimeMinutes);
            IntervalAlarmOffMinutes = Convert.ToInt32(intervallAlarmOffTimeMinutes);
            IntervalAlarmEnable = ConverterHelper.EnabledDisabledConverter(sp3IntervalAlarm, "sp3IntervalAlarm");
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
                WakeUpHour = Convert.ToInt32(sp3AlarmHour);
                WakeUpMinute = Convert.ToInt32(sp3AlarmMin);
                WakeUpDay = Convert.ToInt32(sp3AlarmDay);
                WakeUpMonth = Convert.ToInt32(sp3AlarmMonth);

                WakeUpWeekday = EWeekday.nothing;
                var weekday = Convert.ToInt32(sp3AlarmWeekday);

                if (weekday >= 1 && weekday <= 7)
                {
                    WakeUpWeekday = (EWeekday)weekday;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
        }

        public void GetAlarmWakeupTimerAndWeekend(string wakeupTimerMinutes, string sp3WakeupweekendEnable)
        {
            WakeupTimerMinutes = Convert.ToInt32(wakeupTimerMinutes);
            WakeUpWeekendEnable = ConverterHelper.EnabledDisabledConverter(sp3WakeupweekendEnable, "sp3WakeupweekendEnable");
        }

        public void GetAlarmPowerOffTimePeriod(string sp3PowerOffHours, string sp3PowerOffMinutes)
        {
            PowerOffHour = Convert.ToInt32(sp3PowerOffHours);
            PowerOffMinute = Convert.ToInt32(sp3PowerOffMinutes);
        }

    }
}
