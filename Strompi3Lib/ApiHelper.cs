using System;
using System.Collections.Generic;
using System.Text;

namespace Strompi3Lib
{
    public class ApiHelper
    {
        public static StromPi3 Strompi3Client;

        public static void InitializeClient()
        {
            if (Strompi3Client == null)
            {
                Strompi3Client = new StromPi3();
            }
        }

        public static void EditInputPriorityMode()
        {
            InitializeClient();
            using (var ups = Strompi3Client)
            {
                StromPi3ConfigEditor.InputPriorityMode(ups);
                Console.WriteLine($"Set Power Priority to {(int)ups.Settings.PriorityMode}) = {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");
            }
        }

        public static void EditShutdownMode()
        {
            InitializeClient();
            using (var ups = Strompi3Client)
            {
                StromPi3ConfigEditor.ShutdownMode(ups);
                Console.WriteLine($"Shutdown Timer Mode = {ups.Settings.ShutdownEnable}, Timer = {ups.Settings.ShutdownSeconds} secs");
            }
        }

        public static void EditSerialless()
        {
            InitializeClient();
            using (var ups = Strompi3Client)
            {
                StromPi3ConfigEditor.Serialless(ups);
                Console.WriteLine($"Serial Less Enable = {ups.Settings.SerialLessEnable}");
            }
        }

        public static void EditPowerOnButton()
        {
            InitializeClient();
            using (var ups = Strompi3Client)
            {
                StromPi3ConfigEditor.PowerOnButton(ups);
                Console.WriteLine($"Power-ON-Button Enable = {ups.Settings.StartStopSettings.PowerOnButtonEnable}, Timer = {ups.Settings.StartStopSettings.PowerOnButtonSeconds} secs");
            }
        }

        public static void EditAlarmConfiguration()
        {
            InitializeClient();
            using (var ups = Strompi3Client)
            {
                StromPi3ConfigEditor.AlarmConfiguration(ups);
            }
        }

        public static void EditPowerSaveMode()
        {
            InitializeClient();
            using (var ups = Strompi3Client)
            {
                StromPi3ConfigEditor.PowerSaveMode(ups);
                Console.WriteLine($"Set Power-Save Mode to {ups.Settings.StartStopSettings.PowersaveEnable.ToNumber()}) = {ConverterHelper.EnabledDisabledConverter(ups.Settings.StartStopSettings.PowersaveEnable.ToNumber().ToString(), "PowerSaveMode")}");
            }
        }

        public static void SendConfiguration(StromPi3 ups)
        {
            if (ups != null)
            {
                StromPi3ConfigEditor.SendConfiguration(ups);
            }
        }

        public static StromPi3 EditCompleteConfiguration()
        {
            return StromPi3ConfigEditor.CompleteConfiguration();
        }

       
    }
}
