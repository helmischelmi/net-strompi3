using System;

namespace Strompi3Lib
{
    public class VoltageMeter
    {
        private const double WideRangeVoltMin = 4.8;
        private const double BatteryVoltMin = 0.5;
        private const double MUsbVoltMin = 4.1;

        public double WideRangeVolt { get; private set; }
        public double mUsbVolt { get; private set; }
        public double BatteryVolt { get; private set; }
        public double OutputVolt { get; private set; }

        public void GetVoltage(string sp3AdcWide, string sp3AdcBat, string sp3AdcUsb, string outputVolt)
        {
            try
            {
                WideRangeVolt = VoltageConverter(Convert.ToDouble(sp3AdcWide) / 1000, WideRangeVoltMin);
                mUsbVolt = VoltageConverter(Convert.ToDouble(sp3AdcUsb) / 1000, MUsbVoltMin);
                BatteryVolt = VoltageConverter(Convert.ToDouble(sp3AdcBat) / 1000, BatteryVoltMin);
                OutputVolt = Convert.ToDouble(outputVolt) / 1000;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static double VoltageConverter(double voltage, double minimumVoltage)
        {
            if (voltage > minimumVoltage)
            {
                return voltage;
            }
            return 0;
        }
    }
}
