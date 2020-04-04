using System;
using System.Threading;
using Strompi3Lib;

namespace Strompi3Console
{
    public static class Strompi3API
    {
        #region main menu functions

        public static void SyncRTCTime()
        {
            Console.WriteLine();
            Console.WriteLine();
            using (var ups = new StromPi3(true))
            {
                ups.SyncRTC();
            }
        }

        public static bool ReadPorts()
        {
            Console.WriteLine();
            Console.WriteLine();
            return Os.ShowAvailableSerialPorts("tty");
        }

        public static void GetConfiguration()
        {

            Console.WriteLine();
            Console.WriteLine();
            using (var ups = new StromPi3(true))
            {
                Console.WriteLine(ups.Settings);
            }
        }

        public static void WaitPollingforPowerFailure()
        {

            Console.WriteLine();
            Console.WriteLine();
            using (var ups = new StromPi3(true))
            {
                ups.Monitor.Poll();
            }
        }

        public static void WaitIRQforPowerFailure()
        {
            Console.WriteLine();
            Console.WriteLine();
            using (var ups = new StromPi3())
            {
                Console.WriteLine("TODO: not implemented");
                //ups.Monitor.PowerFailureByIRQ();
            }
        }

        public static void ShutDownRaspi()
        {
            Console.WriteLine();
            Console.WriteLine();
            Thread.Sleep(2000);
            Os.ShutDown();
        }

        #endregion

        #region config menu functions

        public static void SetPowerONButtonEnablerAndTimer()
        {
            Console.WriteLine();
            Console.WriteLine();

            using (var ups = new StromPi3(true))
            {
                StromPi3ConfigEditor.PowerOnButton(ups);
                Console.WriteLine($"Power-ON-Button Enable = {ups.Settings.StartStopSettings.PowerOnButtonEnable}, Timer = {ups.Settings.StartStopSettings.PowerOnButtonSeconds} secs");
            }
        }

        public static void SetPowerSaveMode()
        {
            Console.WriteLine();
            Console.WriteLine();

            using (var ups = new StromPi3(true))
            {
                StromPi3ConfigEditor.PowerSaveMode(ups);
                Console.WriteLine($"Set Power-Save Mode to {ups.Settings.StartStopSettings.PowersaveEnable.ToNumber()}) = {ConverterHelper.EnabledDisabledConverter(ups.Settings.StartStopSettings.PowersaveEnable.ToNumber().ToString(), "PowerSaveMode")}");
            }
        }

        public static void SetPowerPriority()
        {
            Console.WriteLine();
            Console.WriteLine();

            using (var ups = new StromPi3(true))
            {
                StromPi3ConfigEditor.InputPriorityMode(ups);
                Console.WriteLine($"Set Power Priority to {(int)ups.Settings.PriorityMode}) = {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");
            }
        }

        /// <summary>
        /// Set Shutdown-Enabler, -Timer and Shutdown-battery-level
        /// </summary>
        public static void SetShutdownSettings()
        {
            Console.WriteLine();
            Console.WriteLine();

            using (var ups = new StromPi3(true))
            {
                StromPi3ConfigEditor.ShutdownMode(ups);
                Console.WriteLine($"Shutdown Timer Mode = {ups.Settings.ShutdownEnable}, Timer = {ups.Settings.ShutdownSeconds} secs");
            }
        }

        /// <summary>
        /// Set SerialLess-Mode (requires serial port)
        /// </summary>
        public static void SetSerialLess()
        {
            Console.WriteLine();
            Console.WriteLine();

            using (var ups = new StromPi3(true))
            {
                StromPi3ConfigEditor.Serialless(ups);
                Console.WriteLine($"Serial Less Enable = {ups.Settings.SerialLessEnable}");
            }
        }

        #endregion

        public static StromPi3 CompleteConfiguration()
        {
            var ups = new StromPi3(true);
            Console.WriteLine("Main Configuration");

            // strompi-mode
            Console.WriteLine("1. ----------------------------");
            Console.WriteLine(
                $"Input-Priority-Mode: ({(int)ups.Settings.PriorityMode}) = {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");

            foreach (EInputPriority priority in (EInputPriority[])Enum.GetValues(typeof(EInputPriority)))
            {
                Console.WriteLine($"Mode  {(int)priority}: {ConverterHelper.GetEnumDescription(priority)}");
            }

            ups.Settings.SetInputPriorityMode(ConverterHelper.ReadInt(1, 6, "Mode: 1 - 6").ToString());
            Console.WriteLine(
                $"Set Input Priority to {(int)ups.Settings.PriorityMode}) = {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");
            Console.WriteLine("-------------------------------");

            //#Shutdown-enable & set-timer
            Console.WriteLine("2. ----------------------------");
            Console.WriteLine($"Raspberry Pi Shutdown Timer Mode: ({ups.Settings.ShutdownEnable}) ");
            bool shutdownEnable = ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True").ToBool();
            int newShutdownSeconds = ups.Settings.ShutdownSeconds;
            if (shutdownEnable)
            {
                Console.WriteLine($"Shutdown-Timer (0..65535 secs): ({ups.Settings.ShutdownSeconds}) ");
                newShutdownSeconds = ConverterHelper.ReadInt(0, 65535, "timer (0..65535 secs)");
            }

            Console.WriteLine(
                $"Battery-Level Shutdown (0='Disabled', 1= '< 10%', 2='< 25%', 3='< 50%'): ({ups.Settings.BatteryHat.BatteryShutdownLevel}) ");
            int batteryLevelShutdown = ConverterHelper.ReadInt(0, 3, "Set Battery-Level Shutdown (0-3)");
            ups.Settings.SetShutDown(shutdownEnable.ToNumber().ToString(), newShutdownSeconds, batteryLevelShutdown);
            Console.WriteLine(
                $"Set Shutdown Timer Mode to ({ups.Settings.ShutdownEnable}), Timer  = {ups.Settings.ShutdownSeconds} secs.");
            Console.WriteLine(
                $"Set Battery-Level Shutdown to {ConverterHelper.GetEnumDescription(ups.Settings.BatteryHat.BatteryShutdownLevel)}");
            Console.WriteLine("-------------------------------");
            Console.WriteLine();
            // # serialless-mode
            Console.WriteLine("3. ----------------------------");
            Console.WriteLine($"Serialless-Mode: {ups.Settings.SerialLessEnable}");
            ups.Settings.SetSerialLessEnable(ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True").ToString());
            Console.WriteLine($"Set Serialless-Mode to ({ups.Settings.SerialLessEnable})");
            Console.WriteLine("-------------------------------");
            Console.WriteLine();
            // # powersave
            Console.WriteLine("4. ----------------------------");
            Console.WriteLine($"Power-Save-Mode Enable: {ups.Settings.StartStopSettings.PowersaveEnable}");
            ups.Settings.SetPowerSaveEnable(ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True").ToString());
            Console.WriteLine($"Set Power-Save-Mode to ({ups.Settings.StartStopSettings.PowersaveEnable})");
            Console.WriteLine("-------------------------------");
            Console.WriteLine();
            //#warning-enable
            Console.WriteLine("5. ----------------------------");
            Console.WriteLine($"Powerfail Warning Enable: {ups.Settings.PowerFailWarningEnable}");
            ups.Settings.SetPowerFailWarningEnable(ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True").ToString());
            Console.WriteLine($"Set Powerfail Warning to ({ups.Settings.PowerFailWarningEnable})");
            Console.WriteLine("-------------------------------");
            Console.WriteLine();

            //  # PowerOn-Button Enable & PowerOn-Button Timer
            Console.WriteLine("6. ----------------------------");
            Console.WriteLine($"Power-ON-Button Enable: ({ups.Settings.StartStopSettings.PowerOnButtonEnable}) ");
            bool powerOnButtonEnable = ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True").ToBool();
            int powerOnButtonSeconds = ups.Settings.StartStopSettings.PowerOnButtonSeconds;
            if (powerOnButtonEnable && ups.Settings.ShutdownEnable)
            {
                Console.WriteLine($"Poweroff-Mode Enable: ({ups.Settings.StartStopSettings.PoweroffMode}) ");
                ups.Settings.StartStopSettings.SetPowerOffMode(ConverterHelper.ReadInt(0, 1, "Set: 0 = False, 1 = True").ToBool());
            }
            if (powerOnButtonEnable)
            {
                Console.WriteLine($"Power-ON-Button-Timer (0..65535 secs): ({ups.Settings.StartStopSettings.PowerOnButtonSeconds}) ");
                powerOnButtonSeconds = ConverterHelper.ReadInt(0, 65535, "timer (0..65535 secs)");
            }
            ups.Settings.StartStopSettings.SetPowerOnButton(powerOnButtonEnable, powerOnButtonSeconds);

            Console.WriteLine($"Set Power-ON-Button to ({ups.Settings.StartStopSettings.PowerOnButtonEnable})");
            Console.WriteLine($"Set Power-ON-Button-Timer to ({ups.Settings.StartStopSettings.PowerOnButtonSeconds})");
            Console.WriteLine($"Set Poweroff-Mode to ({ups.Settings.StartStopSettings.PoweroffMode})");
            Console.WriteLine("-------------------------------");
            Console.WriteLine();

            // Time&Date-Configuration
            // #set-clock (skipped)

            //Alarm-Configuration
            //
            if (ups.Settings.StartStopSettings.PoweroffTimeEnableMode)
            {
                // $" Alarm-Mode: Minute WakeUp-Timer-Alarm" + Environment.NewLine;
                Console.WriteLine($"TODO: Alarm-Mode: Wakeup Timer");
                Console.WriteLine($"Alarm-Mode: {ups.Settings.AlarmSettings.Mode}");
            }

            else
            {
                // Time, Date, Weekday - Alarm
                Console.WriteLine("7. ----------------------------");
                Console.WriteLine($"Alarm-Mode: {ups.Settings.AlarmSettings.Mode}");
            }

            Console.WriteLine(
                $"Alarm-Time: {ups.Settings.AlarmSettings.WakeUpHour}:{ups.Settings.AlarmSettings.WakeUpMinute} hh:mm");
            Console.WriteLine($"Alarm-Date: {ups.Settings.AlarmSettings.WakeUpDay}.{ups.Settings.AlarmSettings.WakeUpMonth} dd:mm");
            Console.WriteLine(
                $"Alarm-Weekday: {ConverterHelper.GetEnumDescription(ups.Settings.AlarmSettings.WakeUpWeekday)}");

            Console.WriteLine($"PowerOff-Alarm: {ups.Settings.AlarmSettings.PowerOffEnable}");
            Console.WriteLine(
                $"PowerOff-Alarm-Time: {ups.Settings.AlarmSettings.PowerOffHour}:{ups.Settings.AlarmSettings.PowerOffMinute} hh:mm");

            Console.WriteLine($"Interval-Alarm: {ups.Settings.AlarmSettings.IntervalAlarmEnable}");
            Console.WriteLine($"Interval-On-Time: {ups.Settings.AlarmSettings.IntervalAlarmOnMinutes} minutes");
            Console.WriteLine($"Interval-Off-Time: {ups.Settings.AlarmSettings.IntervalAlarmOffMinutes} minutes");
            Console.WriteLine("-------------------------------");
            Console.WriteLine();
            // Power-Off Alarm Configuration
            Console.WriteLine("8. ----------------------------");
            Console.WriteLine($" Power-Off Alarm Enable: {ups.Settings.AlarmSettings.PowerOffEnable}");
            bool powerOffAlarmEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();

            int powerOffAlarmHour = ups.Settings.AlarmSettings.PowerOffHour;
            int powerOffAlarmMinute = ups.Settings.AlarmSettings.PowerOffMinute;

            if (powerOffAlarmEnable)
            {
                Console.WriteLine($"Poweroff time: hour (0..23): {powerOffAlarmHour} hour.");
                powerOffAlarmHour = ConverterHelper.ReadInt(0, 23, "hour (0..23)");

                Console.WriteLine($"Poweroff time: minute (0..59): {powerOffAlarmMinute} minute.");
                powerOffAlarmMinute = ConverterHelper.ReadInt(0, 59, "minute (0..59)");
            }

            ups.Settings.AlarmSettings.GetAlarmPowerOffEnabled(powerOffAlarmEnable.ToNumber().ToString());
            ups.Settings.AlarmSettings.GetAlarmPowerOffTimePeriod(powerOffAlarmHour.ToString(),
                powerOffAlarmMinute.ToString());
            Console.WriteLine($"PowerOff-Alarm: {ups.Settings.AlarmSettings.PowerOffEnable}");
            Console.WriteLine(
                $"PowerOff-Alarm-Time: {ups.Settings.AlarmSettings.PowerOffHour}:{ups.Settings.AlarmSettings.PowerOffMinute} hh:mm");
            Console.WriteLine("-------------------------------");
            Console.WriteLine();

            // Wake-Up Alarm-Configuration
            Console.WriteLine("9. ----------------------------");
            int wakeUpAlarmHour = ups.Settings.AlarmSettings.WakeUpHour;
            int wakeUpAlarmMinute = ups.Settings.AlarmSettings.WakeUpMinute;
            int wakeUpAlarmDay = ups.Settings.AlarmSettings.WakeUpDay;
            int wakeUpAlarmMonth = ups.Settings.AlarmSettings.WakeUpMonth;
            int wakeUpWeekday = (int)ups.Settings.AlarmSettings.WakeUpWeekday;

            int wakeUpTimerMinutes = Convert.ToInt32(ups.Settings.AlarmSettings.WakeupTimerMinutes);
            bool wakeUpWeekendEnable = ups.Settings.AlarmSettings.WakeUpWeekendEnable;

            Console.WriteLine($"Wake-up Alarm Enable: {ups.Settings.AlarmSettings.WakeupEnable}");
            bool wakeUpAlarmEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();

            if (wakeUpAlarmEnable)
            {
                Console.WriteLine($"Wake-up Alarm Mode: {ups.Settings.AlarmSettings.Mode}");
                int wakeUpAlarmmode = ConverterHelper.ReadInt(1, 4, "alarm mode (1 = Time-Alarm , 2 = Date-Alarm, 3 = Weekday-Alarm, 4 = Wakeup Timer)");

                ups.Settings.AlarmSettings.GetAlarmMode(wakeUpAlarmmode.ToString());

                if (ups.Settings.AlarmSettings.Mode == EAlarmMode.WakeupTimer)
                {
                    Console.WriteLine($"Wake-up Alarm Timer (minutes: {ups.Settings.AlarmSettings.WakeupTimerMinutes}");
                    wakeUpTimerMinutes = ConverterHelper.ReadInt(1, 65535, "wake up timer (1-65535 mins)");

                }

                if (ups.Settings.AlarmSettings.Mode == EAlarmMode.TimeAlarm)
                {
                    Console.WriteLine($"Wakeup-Alarm Hour (0..23): {ups.Settings.AlarmSettings.WakeUpHour}");
                    wakeUpAlarmHour = ConverterHelper.ReadInt(0, 23, "hour (0..23)");

                    Console.WriteLine($"Wakeup-Alarm Minute (0..59): {ups.Settings.AlarmSettings.WakeUpMinute}");
                    wakeUpAlarmMinute = ConverterHelper.ReadInt(0, 59, "minute (0..59)");

                    Console.WriteLine(
                        $"Wake-up Weekend Enable: {ups.Settings.AlarmSettings.WakeUpWeekendEnable}");
                    wakeUpWeekendEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();

                }
                else if (ups.Settings.AlarmSettings.Mode == EAlarmMode.DateAlarm)
                {
                    Console.WriteLine($"Wakeup-Alarm Day (1..31): {ups.Settings.AlarmSettings.WakeUpDay}");
                    wakeUpAlarmDay = ConverterHelper.ReadInt(1, 31, "day (1..31)");

                    Console.WriteLine($"Wakeup-Alarm Month (1..12): {ups.Settings.AlarmSettings.WakeUpMonth}");
                    wakeUpAlarmMonth = ConverterHelper.ReadInt(1, 12, "month (1..12)");
                }
                else
                {
                    Console.WriteLine($"StromPi Wakeup-Alarm Weekday (1..7): {ups.Settings.AlarmSettings.WakeUpWeekday}");
                    wakeUpWeekday = ConverterHelper.ReadInt(1, 7, " weekday (1 = Monday, 2 = Tuesday, 3 = Wednesday, 4 = Thursday, 5 = Friday, 6 = Saturday, 7 = Sunday)");
                }

                ups.Settings.AlarmSettings.GetAlarmDateTime(wakeUpAlarmHour.ToString(), wakeUpAlarmMinute.ToString(),
                    wakeUpAlarmDay.ToString(), wakeUpAlarmMonth.ToString(), wakeUpWeekday.ToString());
                ups.Settings.AlarmSettings.GetAlarmWakeupTimerAndWeekend(wakeUpTimerMinutes.ToString(),
                    wakeUpWeekendEnable.ToNumber().ToString());

                Console.WriteLine(
                    $"Alarm-Time: {ups.Settings.AlarmSettings.WakeUpHour}:{ups.Settings.AlarmSettings.WakeUpMinute} hh:mm");
                Console.WriteLine(
                    $"Alarm-Date: {ups.Settings.AlarmSettings.WakeUpDay}.{ups.Settings.AlarmSettings.WakeUpMonth} dd:mm");
                Console.WriteLine(
                    $"Alarm-Weekday: {ConverterHelper.GetEnumDescription(ups.Settings.AlarmSettings.WakeUpWeekday)}");

                Console.WriteLine($"Alarm-Weekend: {ups.Settings.AlarmSettings.WakeUpWeekendEnable}");
                Console.WriteLine($"Alarm-Timer (minutes): {ups.Settings.AlarmSettings.WakeupTimerMinutes}");
                Console.WriteLine($"Alarm-Mode: {ups.Settings.AlarmSettings.Mode}");


                // Interval-Alarm Configuration

                Console.WriteLine($"StromPi Interval-Alarm Enable: {ups.Settings.AlarmSettings.IntervalAlarmEnable}");
                Console.WriteLine($"Set: 0 = False, 1 = True or ENTER to continue");
                bool intervalAlarmEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();

                int intervalAlarmOnMinutes = ups.Settings.AlarmSettings.IntervalAlarmOnMinutes;
                int intervalAlarmOffMinutes = ups.Settings.AlarmSettings.IntervalAlarmOffMinutes;

                if (intervalAlarmEnable)
                {
                    Console.WriteLine($"StromPi Interval-On-Time (0..65535 mins): {intervalAlarmOnMinutes} minutes.");
                    intervalAlarmOnMinutes = ConverterHelper.ReadInt(0, 65535, "on-time (0..65535 mins)");

                    Console.WriteLine($"StromPi Interval-Off-Time (0..65535 mins): {intervalAlarmOffMinutes} minutes.");
                    intervalAlarmOffMinutes = ConverterHelper.ReadInt(0, 65535, "off-time (0..65535 mins)");
                }

                ups.Settings.AlarmSettings.GetAlarmIntervall(intervalAlarmEnable.ToNumber().ToString(),
                    intervalAlarmOnMinutes.ToString(), intervalAlarmOffMinutes.ToString());

                Console.WriteLine($"Set Interval-Alarm Enable to: {ups.Settings.AlarmSettings.IntervalAlarmEnable}");
                Console.WriteLine(
                    $"Set Interval-Alarm On / Off Minutes to: {ups.Settings.AlarmSettings.IntervalAlarmOnMinutes} / {ups.Settings.AlarmSettings.IntervalAlarmOffMinutes}");
            }

            return ups;
        }


        public static void TransferConfiguration(StromPi3 ups)
        {
            Console.WriteLine("Configuration Successful");
            Console.WriteLine("Transfer new Configuration to the StromPi 3");
            Console.WriteLine("###Please Wait###");

            ups.Port.SendConfigElement(EConfigElement.InputPriority, (int)ups.Settings.PriorityMode);

            switch (ups.Settings.AlarmSettings.Mode)
            {
                case EAlarmMode.TimeAlarm:
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeA, 0);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeB, 0);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeC, 1);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeD, 0);
                    break;
                case EAlarmMode.DateAlarm:
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeA, 1);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeB, 0);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeC, 0);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeD, 0);
                    break;

                case EAlarmMode.WeekdayAlarm:
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeA, 0);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeB, 1);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeC, 0);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeD, 0);
                    break;
                case EAlarmMode.WakeupTimer:
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeA, 0);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeB, 0);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeC, 0);
                    ups.Port.SendConfigElement(EConfigElement.AlarmModeD, 1);
                    break;
                default:
                    throw new NotImplementedException($"Unknown EAlarmMode {ups.Settings.AlarmSettings.Mode}");
            }

            ups.Port.SendConfigElement(EConfigElement.AlarmPowerOff, ups.Settings.AlarmSettings.PowerOffEnable.ToNumber());
            ups.Port.SendConfigElement(EConfigElement.AlarmMinutes, ups.Settings.AlarmSettings.WakeUpMinute);
            ups.Port.SendConfigElement(EConfigElement.AlarmHours, ups.Settings.AlarmSettings.WakeUpHour);
            ups.Port.SendConfigElement(EConfigElement.AlarmMinutesOff, ups.Settings.AlarmSettings.PowerOffMinute);
            ups.Port.SendConfigElement(EConfigElement.AlarmHoursOff, ups.Settings.AlarmSettings.PowerOffHour);
            ups.Port.SendConfigElement(EConfigElement.AlarmDay, ups.Settings.AlarmSettings.WakeUpDay);
            ups.Port.SendConfigElement(EConfigElement.AlarmMonth, ups.Settings.AlarmSettings.WakeUpMonth);
            ups.Port.SendConfigElement(EConfigElement.AlarmWeekday, (int)ups.Settings.AlarmSettings.WakeUpWeekday);
            ups.Port.SendConfigElement(EConfigElement.AlarmEnable, ups.Settings.AlarmSettings.WakeupEnable.ToNumber());

            ups.Port.SendConfigElement(EConfigElement.ShutdownEnable, ups.Settings.ShutdownEnable.ToNumber());
            ups.Port.SendConfigElement(EConfigElement.ShutdownTimer, ups.Settings.ShutdownSeconds);
            ups.Port.SendConfigElement(EConfigElement.WarningEnable, ups.Settings.PowerFailWarningEnable.ToNumber());
            ups.Port.SendConfigElement(EConfigElement.SerialLessMode, ups.Settings.SerialLessEnable.ToNumber());
            ups.Port.SendConfigElement(EConfigElement.ShutdownBatteryLevel, (int)ups.Settings.BatteryHat.BatteryShutdownLevel);

            ups.Port.SendConfigElement(EConfigElement.IntervalAlarmEnable, ups.Settings.AlarmSettings.IntervalAlarmEnable.ToNumber());
            ups.Port.SendConfigElement(EConfigElement.IntervalAlarmOnTime, ups.Settings.AlarmSettings.IntervalAlarmOnMinutes);
            ups.Port.SendConfigElement(EConfigElement.IntervalAlarmOffTime, ups.Settings.AlarmSettings.IntervalAlarmOffMinutes);


            ups.Port.SendConfigElement(EConfigElement.PowerOnButtonEnable, ups.Settings.StartStopSettings.PowerOnButtonEnable.ToNumber());
            ups.Port.SendConfigElement(EConfigElement.PowerOnButtonTime, ups.Settings.StartStopSettings.PowerOnButtonSeconds);
            ups.Port.SendConfigElement(EConfigElement.PowerSaveEnable, ups.Settings.StartStopSettings.PowersaveEnable.ToNumber());


            ups.Port.SendConfigElement(EConfigElement.PowerOffMode, ups.Settings.StartStopSettings.PoweroffMode.ToNumber());
            ups.Port.SendConfigElement(EConfigElement.PowerOffTimer, ups.Settings.AlarmSettings.WakeupTimerMinutes);
            ups.Port.SendConfigElement(EConfigElement.WakeupWeekendEnable, ups.Settings.AlarmSettings.WakeUpWeekendEnable.ToNumber());

            ups.Port.SendConfigElement(EConfigElement.ModusReset, 1);

            Console.WriteLine("Transfer Successful");

        }

        public static void SetAlarmMode()
        {
            using (var ups = new StromPi3(true))
            {
                StromPi3ConfigEditor.AlarmConfiguration(ups);
            }
        }
    }
}
