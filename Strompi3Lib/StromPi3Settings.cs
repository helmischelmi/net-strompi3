using System;

namespace Strompi3Lib
{

    public class StromPi3Settings
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

        public EOutputStatus OutputStatus { get; private set; }
        public int PowerFailureCounter { get; private set; }
        public string FirmwareVersion { get; private set; }

        public StromPi3Settings()
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
        public void SetRTCDateTime(string sp3Time, string sp3Date)
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


        public void SetInputPriorityMode(string sp3Modus)
        {
            PriorityMode = EInputPriority.nothing;
            var modus = Convert.ToInt32(sp3Modus);

            if (modus >= 1 && modus <= 6)
            {
                PriorityMode = (EInputPriority)modus;
            }
        }


        public void SetShutDown(string sp3ShutdownEnable, int sp3ShutdownSeconds, int batteryLevelShutdown)
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


        public void SetPowerFailWarningEnable(string sp3WarningEnable)
        {
            PowerFailWarningEnable = ConverterHelper.EnabledDisabledConverter(sp3WarningEnable, "sp3WarningEnable");
        }

        public void SetSerialLessEnable(string sp3SerialLessMode)
        {
            SerialLessEnable = ConverterHelper.EnabledDisabledConverter(sp3SerialLessMode, "sp3SerialLessMode");
        }

        public void SetPowerSaveEnable(string sp3PowerSaveMode)
        {
            StartStopSettings.PowersaveEnable = ConverterHelper.EnabledDisabledConverter(sp3PowerSaveMode, "sp3PowerSaveMode");
        }

        public void SetOutputStatus(string sp3OutputState)
        {
            OutputStatus = EOutputStatus.nothing;
            var modus = Convert.ToInt32(sp3OutputState);

            if (modus >= 0 && modus <= 4)
            {
                OutputStatus = (EOutputStatus)modus;
            }
        }

        public void SetPowerFailureCounter(string sp3PowerFailureCounter)
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

        public void SetFirmwareVersion(string sp3FirmwareVersion)
        {
            FirmwareVersion = sp3FirmwareVersion;
        }


        public override string ToString()
        {
            string status = "--------------------------------------------------------------------" + Environment.NewLine;
            status += "StromPi-Status:" + Environment.NewLine;
            status += "---------------------------------------------------------------------" + Environment.NewLine;
            status += $"Firmware:       {FirmwareVersion,-27} DateTime: {CurrentDateTime} " + Environment.NewLine;
            status += $"Power-Priority: {ConverterHelper.GetEnumDescription(PriorityMode),-27} Serialless-Mode: {SerialLessEnable} " + Environment.NewLine;
            status += $"Power-Source:   {OutputStatus,-27} Power Save Mode: {StartStopSettings.PowersaveEnable}" + Environment.NewLine;
            status += "---------------------------------------------------------------------" + Environment.NewLine;
            status += $"Powerfail Warning: {PowerFailWarningEnable,-24} Battery-Level Shutdown: {ConverterHelper.GetEnumDescription(BatteryHat.BatteryShutdownLevel)}" + Environment.NewLine;
            status += $"Powerfail-Counter: {PowerFailureCounter}" + Environment.NewLine;
            status += $"Pi Shutdown Timer Mode: {ShutdownEnable,-19} Timer: {ShutdownSeconds} seconds" + Environment.NewLine;
            status += "---------------------------------------------------------------------" + Environment.NewLine;
            status += $"PowerOff Mode: {StartStopSettings.PoweroffMode}" + Environment.NewLine;
            status += $"PowerOn-Button: {StartStopSettings.PowerOnButtonEnable,-27} Timer: {StartStopSettings.PowerOnButtonSeconds} seconds" + Environment.NewLine;
            status += "---------------------------------------------------------------------" + Environment.NewLine;

            status += "---------------------------------" + Environment.NewLine;
            status += "Alarm-Modes" + Environment.NewLine;
            status += "---------------------------------" + Environment.NewLine;
            status += $"WakeUp-Alarm: {AlarmSettings.WakeupEnable}" + Environment.NewLine;

            if (StartStopSettings.PoweroffTimeEnableMode)
                status += $" Alarm-Mode: Minute WakeUp-Alarm" + Environment.NewLine;
            else
            {
                status += $"Alarm-Mode: {AlarmSettings.Mode}" + Environment.NewLine;
            }

            status += $"Alarm-DateTime: {AlarmSettings.WakeUpDay:00}.{AlarmSettings.WakeUpMonth:00} [dd:mm] at {AlarmSettings.WakeUpHour:00}:{AlarmSettings.WakeUpMinute:00} [hh:mm]" + Environment.NewLine;
            status += $"WakeUp-Alarm: {AlarmSettings.WakeUpWeekday}" + Environment.NewLine;
            status += $"Weekend Wakeup {AlarmSettings.WakeUpWeekendEnable}" + Environment.NewLine;
            status += $" Minute Wakeup Timer: {AlarmSettings.WakeupTimerMinutes} minutes" + Environment.NewLine;
            status += $"PowerOff-Alarm: {AlarmSettings.PowerOffEnable,-27} Time: {AlarmSettings.PowerOffHour:00}:{AlarmSettings.PowerOffMinute:00} [hh:mm]" + Environment.NewLine;
            status += $"Interval-Alarm: {AlarmSettings.IntervalAlarmEnable,-27} On/Off-Time: {AlarmSettings.IntervalAlarmOnMinutes}/{AlarmSettings.IntervalAlarmOffMinutes} minutes" + Environment.NewLine;

            status += "---------------------------------" + Environment.NewLine;
            status += "Voltage-Levels:" + Environment.NewLine;
            status += "---------------------------------" + Environment.NewLine;
            status += $"microUSB-Input Voltage: {VoltageMeter.mUsbVolt:F2}    Wide-Range-Input Voltage: {VoltageMeter.WideRangeVolt:F2}" + Environment.NewLine;
            status += $"LifePo4-Battery Voltage: {VoltageMeter.BatteryVolt:F2}  (Level: {ConverterHelper.GetEnumDescription(BatteryHat.Level)}, " +
                      $"Charging [{BatteryHat.IsCharging}])" + Environment.NewLine;

            status += $"Output-Voltage: {VoltageMeter.OutputVolt:F2}" + Environment.NewLine;

            return status;
        }
    }
}
