using System;
using System.Device.Gpio;
using System.Text.Encodings.Web;
using System.Threading;

namespace Strompi3Lib
{
    public static class StromPi3ConfigEditor
    {
        public const int GPIOShutdownPinBoardNumber = 40;

        //public static void EditInputPriorityMode(StromPi3 ups)
        //{

        //    Console.WriteLine($"StromPi Input-Priority-Mode: ({(int)ups.Settings.PriorityMode}) = {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");

        //    foreach (EInputPriority priority in (EInputPriority[])Enum.GetValues(typeof(EInputPriority)))
        //    {
        //        Console.WriteLine($"Mode  {(int)priority}: {ConverterHelper.GetEnumDescription(priority)}");
        //    }
        //    Console.WriteLine($"Set Mode: 1 - 6 or ENTER to continue");

        //    bool bContinue = true;

        //    // endless loop for menu
        //    while (bContinue)
        //    {
        //        // Lesen der betätigten Taste
        //        var keyInfo = Console.ReadKey();

        //        // Auswertung der Taste
        //        switch (keyInfo.Key)
        //        {
        //            case ConsoleKey.D1:
        //            case ConsoleKey.NumPad1:
        //            case ConsoleKey.D2:
        //            case ConsoleKey.NumPad2:
        //            case ConsoleKey.D3:
        //            case ConsoleKey.NumPad3:
        //            case ConsoleKey.D4:
        //            case ConsoleKey.NumPad4:
        //            case ConsoleKey.D5:
        //            case ConsoleKey.NumPad5:
        //            case ConsoleKey.D6:
        //            case ConsoleKey.NumPad6:
        //                ups.Settings.SetInputPriorityMode(keyInfo.KeyChar.ToString());
        //                Console.WriteLine($"Transfer Input-Priority Mode: {(int)ups.Settings.PriorityMode} =" +
        //                                  $" {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");

        //                ups.TransferSetting(ESetConfig.InputPriority, (int)ups.Settings.PriorityMode, false);
        //                ups.TransferSetting(ESetConfig.ModusReset, 1);

        //                bContinue = false;
        //                continue;
        //            case ConsoleKey.Enter:
        //                bContinue = false;
        //                continue;
        //            default:
        //                continue;
        //        }
        //    }


        //}

        public static void EditInputPriorityMode(StromPi3 ups)
        {
            Console.WriteLine($"StromPi Input-Priority-Mode: ({(int)ups.Settings.PriorityMode}) = {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");

            foreach (EInputPriority priority in (EInputPriority[])Enum.GetValues(typeof(EInputPriority)))
            {
                Console.WriteLine($"Mode  {(int)priority}: {ConverterHelper.GetEnumDescription(priority)}");
            }
            Console.WriteLine($"Set Mode: 1 - 6 or ENTER to continue");

            int priorityMode = ReadInt(1, 6);

            ups.Settings.SetInputPriorityMode(priorityMode.ToString());

            Console.WriteLine($"Transfer Input-Priority Mode: {(int)ups.Settings.PriorityMode} =" +
                              $" {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");

            ups.TransferSetting(ESetConfig.InputPriority, (int)ups.Settings.PriorityMode, false);
            ups.TransferSetting(ESetConfig.ModusReset, 1);
        }

        //public static void EditShutdownEnabler(StromPi3 ups)
        //{
        //    Console.WriteLine($"Raspberry Pi Shutdown: ({ups.Settings.ShutdownEnable}) ");
        //    Console.WriteLine($"Set: 0 = OFF (False), 1 = ON (True) or ENTER to continue");

        //    bool bContinue = true;

        //    while (bContinue)
        //    {
        //        var keyInfo = Console.ReadKey();  // get user input

        //        switch (keyInfo.Key)
        //        {
        //            case ConsoleKey.D0:
        //            case ConsoleKey.NumPad0:
        //            case ConsoleKey.D1:
        //            case ConsoleKey.NumPad1:
        //                ups.Settings.SetShutDown(keyInfo.KeyChar.ToString(), ups.Settings.ShutdownSeconds);
        //                //ups.TransferSetting(ESetConfig.ShutdownEnable,ups.Settings.ShutdownEnable.ToNumber());
        //                bContinue = false;
        //                continue;
        //            case ConsoleKey.Enter:
        //                bContinue = false;
        //                continue;
        //            default:
        //                continue;
        //        }
        //    }

        //    Console.WriteLine($"Shutdown-Timer (0..65535 secs): ({ups.Settings.ShutdownSeconds}) ");
        //    Console.WriteLine($"Set timer seconds and press ENTER to continue");

        //    int timerInputSeconds = ups.Settings.ShutdownSeconds;
        //    bContinue = true;
        //    while (bContinue)
        //    {
        //        bContinue = !int.TryParse(Console.ReadLine(), out timerInputSeconds);
        //        Console.WriteLine($"INPUT seconds = {timerInputSeconds}, if OK, press ENTER to continue");
        //        var keyInfo = Console.ReadKey();
        //        if (keyInfo.Key == ConsoleKey.Enter) bContinue = false;
        //    }

        //    ups.Settings.SetShutDown(ups.Settings.ShutdownEnable.ToNumber().ToString(), timerInputSeconds);
        //    Console.WriteLine($"Transfer Shutdown-Timer: {ups.Settings.ShutdownSeconds}");
        //    ups.TransferSetting(ESetConfig.ShutdownEnable, ups.Settings.ShutdownEnable.ToNumber());
        //    ups.TransferSetting(ESetConfig.ShutdownTimer, ups.Settings.ShutdownSeconds);
        //}

        public static void EditShutdownEnabler(StromPi3 ups)
        {
            Console.WriteLine($"Raspberry Pi Shutdown: ({ups.Settings.ShutdownEnable}) ");
            Console.WriteLine($"Set: 0 = False, 1 = True or ENTER to continue");
            int shutdownEnable = ReadInt(0, 1);

            Console.WriteLine($"Shutdown-Timer (0..65535 secs): ({ups.Settings.ShutdownSeconds}) ");
            Console.WriteLine($"Set timer seconds and press ENTER to continue");
            int newShutdownSeconds = ReadInt(0, 65535);

            Console.WriteLine($"Battery-Level Shutdown (0='Disabled', 1= '< 10%', 2='< 25%', 3='< 50%'): ({ups.Settings.BatteryHat.ShutdownLevel}) ");
            Console.WriteLine($"Set battery-level and press ENTER to continue");
            int batteryLevelShutdown = ReadInt(0, 3);

            ups.Settings.SetShutDown(shutdownEnable.ToString(), newShutdownSeconds, batteryLevelShutdown);

            Console.WriteLine($"Transfer Shutdown {ups.Settings.ShutdownEnable.ToNumber()} in {ups.Settings.ShutdownSeconds} secs");
            ups.TransferSetting(ESetConfig.ShutdownEnable, ups.Settings.ShutdownEnable.ToNumber(), false);
            ups.TransferSetting(ESetConfig.ShutdownTimer, ups.Settings.ShutdownSeconds, false);

            ups.TransferSetting(ESetConfig.ShutdownBatteryLevel, (int)ups.Settings.BatteryHat.ShutdownLevel);
        }

        public static void EditPowerOnButtonEnabler(StromPi3 ups)
        {
            Console.WriteLine($"Power-ON-Button Enabler: ({ups.Settings.StartStopSettings.PowerOnButtonEnable}) ");
            Console.WriteLine($"Set: 0 = False, 1 = True or ENTER to continue");
            int powerOnButtonEnable = ReadInt(0, 1);

            Console.WriteLine($"Power-ON-Button-Timer (0..65535 secs): ({ups.Settings.StartStopSettings.PowerOnButtonSeconds}) ");
            Console.WriteLine($"Set timer seconds and press ENTER to continue");
            int newPowerOnButtonSeconds = ReadInt(0, 65535);


        }


        public static void EditSeriallessEnable(StromPi3 ups)
        {
            Console.WriteLine($"StromPi Serialless-Enable: {ups.Settings.SerialLessEnable}");
            Console.WriteLine($"Set: 0 = False, 1 = True or ENTER to continue");
            int serialLessEnable = ReadInt(0, 1);

            ups.Settings.SetSerialLessEnable(serialLessEnable.ToString());

            if (ups.Settings.SerialLessEnable) // use serial port
            {
                Console.WriteLine($"Transfer Serial-Less Enable: {ups.Settings.SerialLessEnable}");
                ups.TransferSetting(ESetConfig.SerialLessMode, ups.Settings.SerialLessEnable.ToNumber());
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


        public static void EditPowerSaveMode(StromPi3 ups)
        {
            Console.WriteLine($"StromPi Power-Save-Mode Enable: {ups.Settings.StartStopSettings.PowersaveEnable}");
            Console.WriteLine($"Set: 0 = False, 1 = True or ENTER to continue");
            int powerSaveEnable = ReadInt(0, 1);

            ups.Settings.SetPowerSaveEnable(powerSaveEnable.ToString());

            Console.WriteLine($"Transfer power-save Enable: {ups.Settings.StartStopSettings.PowersaveEnable}");
            ups.TransferSetting(ESetConfig.PowerSaveEnable, ups.Settings.StartStopSettings.PowersaveEnable.ToNumber());
        }

        /// <summary>
        /// reads keys from console until ENTER is pressed
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int ReadInt(int min, int max)
        {
            bool bContinue = true;
            bool bError = false;
            string strResult = String.Empty;
            int result = -1;

            while (bContinue)
            {
                var keyInfo = Console.ReadKey();// Get user input

                if (keyInfo.Key == ConsoleKey.Enter && bError == false)
                {
                    bContinue = false;
                    continue;
                }

                if (!char.IsDigit(keyInfo.KeyChar)) continue;

                strResult += keyInfo.KeyChar.ToString();

                result = int.Parse(strResult);

                if (result >= min && result <= max)
                {
                    bError = false;
                    continue;
                }
                strResult = string.Empty;
                Console.CursorLeft = 0;
                bError = true;
            }
            Console.WriteLine();
            return result;
        }


    }
}
