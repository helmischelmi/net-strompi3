using System;

namespace Strompi3Lib
{

    public class StartStopSettings
    {
        public bool PowerOnButtonEnable { get; private set; }
        public int PowerOnButtonSeconds { get; private set; }
        public bool PowersaveEnable { get; set; }
        public bool PoweroffMode { get; private set; }

        public bool PoweroffTimeEnableMode { get; private set; }
        //public string WakeupTimerMinutes { get; private set; }
        //public bool WakeupWeekendEnable { get; private set; }

        public void GetStartStopSettings(string sp3PowerOnButtonEnable, string sp3PowerOnButtonTime, string sp3PowersaveEnable, string sp3PoweroffMode,
            string poweroffTimeEnableMode)
        {
            PowerOnButtonEnable = ConverterHelper.EnabledDisabledConverter(sp3PowerOnButtonEnable, "sp3PowerOnButtonEnable");
            PowerOnButtonSeconds = Convert.ToInt32(sp3PowerOnButtonTime);
            PowersaveEnable = ConverterHelper.EnabledDisabledConverter(sp3PowersaveEnable, "sp3PowersaveEnable");
            PoweroffMode = ConverterHelper.EnabledDisabledConverter(sp3PoweroffMode, "sp3PoweroffMode");
            PoweroffTimeEnableMode = ConverterHelper.EnabledDisabledConverter(poweroffTimeEnableMode, "poweroffTimeEnableMode");
        }


        public void SetPowerOnButton(in bool powerOnButtonEnable, in int newPowerOnButtonSeconds)
        {
            PowerOnButtonEnable = powerOnButtonEnable;
            PowerOnButtonSeconds = newPowerOnButtonSeconds;
        }

        public void SetPowerOffMode(in bool powerOffEnable)
        {
            PoweroffMode = powerOffEnable;
        }
    }
}
