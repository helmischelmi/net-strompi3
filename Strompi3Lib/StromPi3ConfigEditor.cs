using System;
using System.Device.Gpio;
using System.Threading;

namespace Strompi3Lib
{
    public static class StromPi3ConfigEditor
    {
        public const int GPIOShutdownPinBoardNumber = 40;


        /// <summary>
        /// reads the given setting from user input and sends it to the Strompi3-port
        /// </summary>
        /// <param name="ups"></param>
        public static void InputPriorityMode(StromPi3 ups)
        {
            Console.WriteLine($"StromPi Input-Priority-Mode: ({(int)ups.Settings.PriorityMode}) = {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");

            foreach (EInputPriority priority in (EInputPriority[])Enum.GetValues(typeof(EInputPriority)))
            {
                Console.WriteLine($"Mode  {(int)priority}: {ConverterHelper.GetEnumDescription(priority)}");
            }
            int priorityMode = ConverterHelper.ReadInt(1, 6, "Mode: 1 - 6");

            ups.Settings.SetInputPriorityMode(priorityMode.ToString());

            Console.WriteLine($"Transfer Input-Priority Mode: {(int)ups.Settings.PriorityMode} =" +
                              $" {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");

            ups.Port.SendConfigElement(EConfigElement.InputPriority, (int)ups.Settings.PriorityMode);
            ups.Port.SendConfigElement(EConfigElement.ModusReset, 1);
            ups.Port.ReceiveConfiguration(ups.Settings);
        }

        /// <summary>
        /// reads the given setting from user input and sends it to the Strompi3-port
        /// </summary>
        public static void ShutdownMode(StromPi3 ups)
        {
            Console.WriteLine($"Raspberry Pi Shutdown: ({ups.Settings.ShutdownEnable}) ");
            int shutdownEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True");

            Console.WriteLine($"Shutdown-Timer (0..65535 secs): ({ups.Settings.ShutdownSeconds}) ");
            int newShutdownSeconds = ConverterHelper.ReadInt(0, 65535, "Timer (0..65535 secs)");

            Console.WriteLine($"Battery-Level Shutdown (0='Disabled', 1= '< 10%', 2='< 25%', 3='< 50%'): ({ups.Settings.BatteryHat.BatteryShutdownLevel}) ");
            int batteryLevelShutdown = ConverterHelper.ReadInt(0, 3, "Battery-Level Shutdown (0='Disabled', 1= '< 10%', 2='< 25%', 3='< 50%')");

            ups.Settings.SetShutDown(shutdownEnable.ToString(), newShutdownSeconds, batteryLevelShutdown);

            Console.WriteLine($"Transfer Shutdown {ups.Settings.ShutdownEnable.ToNumber()} in {ups.Settings.ShutdownSeconds} secs");
            ups.Port.SendConfigElement(EConfigElement.ShutdownEnable, ups.Settings.ShutdownEnable.ToNumber());
            ups.Port.SendConfigElement(EConfigElement.ShutdownTimer, ups.Settings.ShutdownSeconds);

            ups.Port.SendConfigElement(EConfigElement.ShutdownBatteryLevel, (int)ups.Settings.BatteryHat.BatteryShutdownLevel);
            ups.Port.ReceiveConfiguration(ups.Settings);
        }

        /// <summary>
        /// reads the given setting from user input and sends it to the Strompi3-port
        /// <para>USAGE: Enabling or disabling the Powerfail-Warning (instead of a shutdown) through the serial interface</para>
        /// </summary>
        /// <param name="ups"></param>
        public static void PowerFailWarning(StromPi3 ups)
        {
            Console.WriteLine($"StromPi Powerfail Warning Enable: {ups.Settings.PowerFailWarningEnable}");
            int powerFailEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True");

            ups.Settings.SetPowerFailWarningEnable(powerFailEnable.ToString());

            Console.WriteLine($"Transfer power-fail warning Enable: {ups.Settings.PowerFailWarningEnable}");
            ups.Port.SendConfigElement(EConfigElement.WarningEnable, ups.Settings.PowerFailWarningEnable.ToNumber());
            ups.Port.ReceiveConfiguration(ups.Settings);
        }

        public static void AlarmConfiguration(StromPi3 ups)
        {
            if (ups.Settings.StartStopSettings.PoweroffTimeEnableMode)
            {
                // $" Alarm-Mode: Minute WakeUp-Timer-Alarm" + Environment.NewLine;
            }

            else
            {
                // Time, Date, Weekday - Alarm
                string s = $"Alarm-Mode: {ups.Settings.AlarmSettings.Mode}" + Environment.NewLine;
            }

            StromPi3ConfigEditor.AlarmPowerOff(ups);

            StromPi3ConfigEditor.AlarmWakeUp(ups);

            StromPi3ConfigEditor.AlarmInterval(ups);
        }

        public static void AlarmWakeUp(StromPi3 ups)
        {
            int wakeUpAlarmHour = ups.Settings.AlarmSettings.WakeUpHour;
            int wakeUpAlarmMinute = ups.Settings.AlarmSettings.WakeUpMinute;
            int wakeUpAlarmDay = ups.Settings.AlarmSettings.WakeUpDay;
            int wakeUpAlarmMonth = ups.Settings.AlarmSettings.WakeUpMonth;
            int wakeUpWeekday = (int)ups.Settings.AlarmSettings.WakeUpWeekday;

            int wakeUpTimerMinutes = Convert.ToInt32(ups.Settings.AlarmSettings.WakeupTimerMinutes);
            bool wakeUpWeekendEnable = ups.Settings.AlarmSettings.WakeUpWeekendEnable;

            Console.WriteLine($"StromPi Wake-up Alarm Enable: {ups.Settings.AlarmSettings.WakeupEnable}");
            bool wakeUpAlarmEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();

            if (wakeUpAlarmEnable)
            {
                Console.WriteLine($"StromPi Wake-up Alarm Mode: {ups.Settings.AlarmSettings.Mode}");
                int wakeUpAlarmmode = ConverterHelper.ReadInt(1, 4, "alarm mode (1 = Time-Alarm , 2 = Date-Alarm, 3 = Weekday-Alarm, 4 = Wakeup Timer)");

                ups.Settings.AlarmSettings.GetAlarmMode(wakeUpAlarmmode.ToString());

                if (ups.Settings.AlarmSettings.Mode == EAlarmMode.WakeupTimer)
                {
                    Console.WriteLine(
                        $"Wake-up Alarm Timer (minutes: {ups.Settings.AlarmSettings.WakeupTimerMinutes}");
                    wakeUpTimerMinutes = ConverterHelper.ReadInt(1, 65535, "timer (1-65535 mins)");

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

                    Console.WriteLine($" Wakeup-Alarm Month (1..12): {ups.Settings.AlarmSettings.WakeUpMonth}");
                    wakeUpAlarmMonth = ConverterHelper.ReadInt(1, 12, "month (1..12)");
                }
                else
                {
                    Console.WriteLine($"Wakeup-Alarm Weekday (1..7): {ups.Settings.AlarmSettings.WakeUpWeekday}");
                    wakeUpWeekday = ConverterHelper.ReadInt(1, 7, "weekday (1 = Monday, 2 = Tuesday, 3 = Wednesday, 4 = Thursday, 5 = Friday, 6 = Saturday, 7 = Sunday)");
                }

                ups.Settings.AlarmSettings.GetAlarmDateTime(wakeUpAlarmHour.ToString(), wakeUpAlarmMinute.ToString(),
                    wakeUpAlarmDay.ToString(), wakeUpAlarmMonth.ToString(), wakeUpWeekday.ToString());

                ups.Settings.AlarmSettings.GetAlarmWakeupTimerAndWeekend(wakeUpTimerMinutes.ToString(), wakeUpWeekendEnable.ToNumber().ToString());

                Console.WriteLine($"Transfer  Wake-up Alarm Mode: {ups.Settings.AlarmSettings.Mode}");
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

                ups.Port.SendConfigElement(EConfigElement.AlarmMinutes, ups.Settings.AlarmSettings.WakeUpMinute);
                ups.Port.SendConfigElement(EConfigElement.AlarmHours, ups.Settings.AlarmSettings.WakeUpHour);
                ups.Port.SendConfigElement(EConfigElement.AlarmDay, ups.Settings.AlarmSettings.WakeUpDay);
                ups.Port.SendConfigElement(EConfigElement.AlarmMonth, ups.Settings.AlarmSettings.WakeUpMonth);
                ups.Port.SendConfigElement(EConfigElement.AlarmWeekday, (int)ups.Settings.AlarmSettings.WakeUpWeekday);
                ups.Port.SendConfigElement(EConfigElement.AlarmEnable, ups.Settings.AlarmSettings.WakeupEnable.ToNumber());

                // weekend enable and  wakeUpTimerMinutes
                Console.WriteLine($"Transfer Wake-up Weekend {ups.Settings.AlarmSettings.WakeUpWeekendEnable} " +
                                  $"and Wakeup Timer: {ups.Settings.AlarmSettings.WakeupTimerMinutes}");
                ups.Port.SendConfigElement(EConfigElement.WakeupWeekendEnable, ups.Settings.AlarmSettings.WakeUpWeekendEnable.ToNumber());
                ups.Port.SendConfigElement(EConfigElement.PowerOffTimer, ups.Settings.AlarmSettings.WakeupTimerMinutes);


                ups.Port.ReceiveConfiguration(ups.Settings);
            }
        }



        /// <summary>
        /// reads the given setting from user input and sends it to the Strompi3-port
        /// <para>USAGE: turns ON or OFF a daily shutdown of the raspberry at given hour:minute </para>
        /// </summary>
        /// <param name="ups"></param>
        public static void AlarmPowerOff(StromPi3 ups)
        {
            Console.WriteLine($"StromPi Power-Off Alarm Enable: {ups.Settings.AlarmSettings.PowerOffEnable}");
            Console.WriteLine($"Set: 0 = False, 1 = True or ENTER to continue");
            bool powerOffAlarmEnable = ConverterHelper.ReadInt(0, 1, "").ToBool();

            int powerOffAlarmHour = ups.Settings.AlarmSettings.PowerOffHour;
            int powerOffAlarmMinute = ups.Settings.AlarmSettings.PowerOffMinute;

            if (powerOffAlarmEnable)
            {
                Console.WriteLine($"StromPi poweroff time: hour (0..23): {powerOffAlarmHour} hour.");
                powerOffAlarmHour = ConverterHelper.ReadInt(0, 23, "hour (0..23)");

                Console.WriteLine($"StromPi poweroff time: minute (0..59): {powerOffAlarmMinute} minute.");
                powerOffAlarmMinute = ConverterHelper.ReadInt(0, 59, "minute (0..59)");
            }

            ups.Settings.AlarmSettings.GetAlarmPowerOffEnabled(powerOffAlarmEnable.ToNumber().ToString());
            ups.Settings.AlarmSettings.GetAlarmPowerOffTimePeriod(powerOffAlarmHour.ToString(), powerOffAlarmMinute.ToString());

            Console.WriteLine($"Transfer Power-Off Alarm Enable: {ups.Settings.AlarmSettings.PowerOffEnable}");
            ups.Port.SendConfigElement(EConfigElement.AlarmPowerOff, ups.Settings.AlarmSettings.PowerOffEnable.ToNumber());
            Console.WriteLine($"Transfer Power-Off Alarm Hour: {ups.Settings.AlarmSettings.PowerOffHour}");
            ups.Port.SendConfigElement(EConfigElement.AlarmHoursOff, ups.Settings.AlarmSettings.PowerOffHour);
            Console.WriteLine($"Transfer Power-Off Alarm Minute: {ups.Settings.AlarmSettings.PowerOffMinute}");
            ups.Port.SendConfigElement(EConfigElement.AlarmMinutesOff, ups.Settings.AlarmSettings.PowerOffMinute);
            ups.Port.ReceiveConfiguration(ups.Settings);
        }


        /// <summary>
        /// reads the given setting from user input and sends it to the Strompi3-port
        /// <para>USAGE: intervals (in minutes) to turn the raspberry ON or OFF</para>
        /// <para>On-Time: minutes, the raspberry is ON</para>
        /// <para>Off-Time: minutes, the raspberry is OFF</para>
        /// </summary>
        /// <param name="ups"></param>
        public static void AlarmInterval(StromPi3 ups)
        {
            Console.WriteLine($"Interval-Alarm Enable: {ups.Settings.AlarmSettings.IntervalAlarmEnable}");
            bool intervalAlarmEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();

            int intervalAlarmOnMinutes = ups.Settings.AlarmSettings.IntervalAlarmOnMinutes;
            int intervalAlarmOffMinutes = ups.Settings.AlarmSettings.IntervalAlarmOffMinutes;

            if (intervalAlarmEnable)
            {
                Console.WriteLine($"Interval-On-Time (0..65535 mins): {intervalAlarmOnMinutes} minutes.");
                intervalAlarmOnMinutes = ConverterHelper.ReadInt(0, 65535, "on-time (0..65535 mins)");

                Console.WriteLine($"Interval-Off-Time (0..65535 mins): {intervalAlarmOffMinutes} minutes.");
                intervalAlarmOffMinutes = ConverterHelper.ReadInt(0, 65535, "off-time (0..65535 mins)");
            }

            ups.Settings.AlarmSettings.GetAlarmIntervall(intervalAlarmEnable.ToNumber().ToString(), intervalAlarmOnMinutes.ToString(), intervalAlarmOffMinutes.ToString());

            Console.WriteLine($"Transfer Interval-Alarm Enable: {ups.Settings.AlarmSettings.IntervalAlarmEnable}");
            ups.Port.SendConfigElement(EConfigElement.IntervalAlarmEnable, ups.Settings.AlarmSettings.IntervalAlarmEnable.ToNumber());
            Console.WriteLine($"Transfer Interval-On-Time: {ups.Settings.AlarmSettings.IntervalAlarmOnMinutes}");
            ups.Port.SendConfigElement(EConfigElement.IntervalAlarmOnTime, ups.Settings.AlarmSettings.IntervalAlarmOnMinutes);
            Console.WriteLine($"Transfer Interval-Off-Time: {ups.Settings.AlarmSettings.IntervalAlarmOffMinutes}");
            ups.Port.SendConfigElement(EConfigElement.IntervalAlarmOffTime, ups.Settings.AlarmSettings.IntervalAlarmOffMinutes);
            ups.Port.ReceiveConfiguration(ups.Settings);
        }

        /// <summary>
        /// reads the given setting from user input and sends it to the Strompi3-port
        /// <para>MOD (Chapter 8.2): additional cable soldering required</para>
        /// <para>USAGE: start system by brigding the reset-pins, after the system was shutdown by "poweroff"command </para>
        /// <para>Between poweroff and poweron !WAIT! min. 30 secs</para>
        /// </summary>
        /// <param name="ups"></param>
        public static void PowerOnButton(StromPi3 ups)
        {
            int powerOnButtonSeconds = ups.Settings.StartStopSettings.PowerOnButtonSeconds;

            Console.WriteLine($"Power-ON-Button Enabler: ({ups.Settings.StartStopSettings.PowerOnButtonEnable}) ");
            bool powerOnButtonEnable = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();

            if (powerOnButtonEnable)
            {
                Console.WriteLine($"Power-ON-Button-Timer (0..65535 secs): ({ups.Settings.StartStopSettings.PowerOnButtonSeconds}) ");
                powerOnButtonSeconds = ConverterHelper.ReadInt(0, 65535, "timer (0..65535 secs)");
            }

            ups.Settings.StartStopSettings.SetPowerOnButton(powerOnButtonEnable, powerOnButtonSeconds);

            if (powerOnButtonEnable && ups.Settings.ShutdownEnable)
            {
                Console.WriteLine($"Poweroff-Mode Enable: ({ups.Settings.StartStopSettings.PoweroffMode}) ");
                bool powerOffMode = ConverterHelper.ReadInt(0, 1, "0 = False, 1 = True").ToBool();
            }

            Console.WriteLine($"Transfer PowerOnButton: {ups.Settings.StartStopSettings.PowerOnButtonEnable}");
            ups.Port.SendConfigElement(EConfigElement.PowerOnButtonEnable, ups.Settings.StartStopSettings.PowerOnButtonEnable.ToNumber());
            Console.WriteLine($"Transfer PowerOnButton-Time: {ups.Settings.StartStopSettings.PowerOnButtonSeconds}");
            ups.Port.SendConfigElement(EConfigElement.PowerOnButtonTime, ups.Settings.StartStopSettings.PowerOnButtonSeconds);
            Console.WriteLine($"Transfer PowerOff Mode: {ups.Settings.StartStopSettings.PoweroffMode}");
            ups.Port.SendConfigElement(EConfigElement.PowerOffMode, ups.Settings.StartStopSettings.PoweroffMode.ToNumber());
            ups.Port.ReceiveConfiguration(ups.Settings);
        }

        /// <summary>
        /// reads the given setting from user input and sends it to the Strompi3-port
        /// <para>MOD (Chapter 8.1): additional cable soldering required between Reset-Pin (Jumper ON) and chosen GPIO-Pin (40)</para>
        /// <para>SERIALLESS  ON: turns off the serial comm-port and sets a GPIO-pin (40) as alternative to read</para>
        /// <para>SERIALLESS OFF: turns on the serial comm-port and resets a GPIO-pin (40)</para>
        /// </summary>
        /// <param name="ups"></param>
        public static void Serialless(StromPi3 ups)
        {
            Console.WriteLine($"StromPi Serialless-Enable: {ups.Settings.SerialLessEnable}");
            int serialLessEnable = ConverterHelper.ReadInt(0, 1,"0 = False, 1 = True");

            ups.Settings.SetSerialLessEnable(serialLessEnable.ToString());

            if (ups.Settings.SerialLessEnable) // use serial port
            {
                Console.WriteLine($"Transfer Serial-Less Enable: {ups.Settings.SerialLessEnable}");
                ups.Port.SendConfigElement(EConfigElement.SerialLessMode, ups.Settings.SerialLessEnable.ToNumber());
                ups.Port.ReceiveConfiguration(ups.Settings);
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

        /// <summary>
        /// reads the given setting from user input and sends it to the Strompi3-port
        /// <para>when not-used WIDE power-input: turns off voltage-converter of WIDE-input to save power</para>
        /// <para>JUMPER OFF required: s. Chapter 5.5 Power save mode</para>
        /// </summary>
        /// <param name="ups"></param>
        public static void PowerSaveMode(StromPi3 ups)
        {
            Console.WriteLine($"StromPi Power-Save-Mode Enable: {ups.Settings.StartStopSettings.PowersaveEnable}");
            int powerSaveEnable = ConverterHelper.ReadInt(0, 1,"0 = False, 1 = True");

            ups.Settings.SetPowerSaveEnable(powerSaveEnable.ToString());

            Console.WriteLine($"Transfer power-save Enable: {ups.Settings.StartStopSettings.PowersaveEnable}");
            ups.Port.SendConfigElement(EConfigElement.PowerSaveEnable, ups.Settings.StartStopSettings.PowersaveEnable.ToNumber());
            ups.Port.ReceiveConfiguration(ups.Settings);
        }


        public static void SendConfiguration(StromPi3 ups)
        {
            Console.WriteLine("Transfer new Configuration to the StromPi 3");
            Console.WriteLine("###Please Wait###");

            ups.Port.SendConfiguration(ups.Settings);
            ups.Port.ReceiveConfiguration(ups.Settings);
        }

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
                Console.WriteLine("Values above 255 won't work with firmware 1.72, see FAQ https://strompi.joy-it.net/questions/question/rpi4-strompi3-shutdown-funktioniert-nicht-korrekt-strompi-schaltet-zu-frueh-ab/page/3/");
                // Der Bug war, dass der Wert des Shutdowntimers in einer 16 bit Variable abgelegt wurde, aber in der weiteren Verarbeitung noch eine 8 bit Variable war.
                // Dies führte dazu, dass Werte über 255 Sekunden nicht korrekt funktionierten.
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
    }
}
