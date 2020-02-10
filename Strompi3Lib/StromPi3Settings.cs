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

            BatteryHat.SetBatteryShutdownLevel(batteryLevelShutdown);
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
    }
}
