using System;
using System.Data;
using System.Threading;
using Strompi3Lib;

namespace Strompi3Console
{
    public class Program
    {

        #region Menu-control
        /// <summary>
        /// Main menu
        /// </summary>
        static void Main(string[] args)
        {
            ConsoleKeyInfo objKeyInfo;
            bool bContinue = true;

            // endless loop for menu
            while (bContinue)
            {
                try
                {
                    // clear console
                    ShowTitleInteractive("Strompi3 - Management Console (Core 3.1)", "");

                    // show menu
                    Console.SetCursorPosition(10, 12);
                    Console.Write("Select:");
                    Console.SetCursorPosition(10, 14);
                    Console.Write("  1  -> Show available serial ports");
                    Console.SetCursorPosition(10, 15);
                    Console.Write("  2  -> Get Configuration");
                    Console.SetCursorPosition(10, 16);
                    Console.Write("  3  -> Set Configuration");
                    Console.SetCursorPosition(10, 17);
                    Console.Write("  4  -> Set Time");
                    Console.SetCursorPosition(10, 18);
                    Console.Write("  5  -> Wait Polling for PowerFailure");
                    Console.SetCursorPosition(10, 19);
                    Console.Write("  6  -> Wait IRQ for PowerFailure");
                    Console.SetCursorPosition(10, 20);
                    Console.Write("  7  -> ");
                    Console.SetCursorPosition(10, 21);
                    Console.Write("  8  ->");
                    Console.SetCursorPosition(10, 22);
                    Console.Write("  9  -> ");
                    Console.SetCursorPosition(10, 23);
                    Console.Write("  0  -> Shutdown Raspi");
                    Console.SetCursorPosition(10, 27);
                    Console.Write("Select (ESC to end): ");

                    // Lesen der betätigten Taste
                    objKeyInfo = Console.ReadKey();

                    // Auswertung der Taste
                    switch (objKeyInfo.Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            ReadPorts();
                            break;
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            GetConfiguration();
                            break;
                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            ShowConfigurationSubMenu();
                            break;
                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                            SetDateTime();
                            break;
                        case ConsoleKey.D5:
                        case ConsoleKey.NumPad5:
                            WaitPollingforPowerFailure();
                            break;
                        case ConsoleKey.D6:
                        case ConsoleKey.NumPad6:
                            WaitIRQforPowerFailure();
                            break;
                        case ConsoleKey.D7:
                        case ConsoleKey.NumPad7:
                        case ConsoleKey.D8:
                        case ConsoleKey.NumPad8:
                        case ConsoleKey.D9:
                        case ConsoleKey.NumPad9:
                        case ConsoleKey.D0:
                        case ConsoleKey.NumPad0:
                            ShutDownRaspi();
                            break;
                        case ConsoleKey.Escape:
                            bContinue = false;
                            continue;
                        default:
                            continue;
                    }

                    // wait for next key
                    ReadKey();
                }
                catch (Exception exc)
                {
                    // error in method
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine(exc.Message);

                    // read next key
                    Console.WriteLine();
                    ReadKey();
                }
            }

            // close application
            Console.ResetColor();
            Console.Clear();
        }

        /// <summary>
        /// wait for keyboard input
        /// </summary>
        private static void ReadKey()
        {
            ReadKey("Press key to get back to menu...");
        }

        /// <summary>
        /// wait for special keyboard input
        /// </summary>
        /// <param name="strText">text to show on console.</param>
        private static void ReadKey(string strText)
        {
            Console.WriteLine();
            Console.WriteLine(strText);
            Console.ReadKey();
        }

        /// <summary>
        /// writes the title
        /// </summary>
        /// <param name="strTitle">Title of menu</param>
        /// <param name="strSubtitle">Theme of menu</param>
        public static void ShowTitleInteractive(string strTitle, string strSubtitle)
        {
            // prepare console
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();

            // write title
            Console.SetCursorPosition(10, 6);
            Console.Write(strTitle);
            Console.SetCursorPosition(10, 7);
            Console.Write(new string('=', strTitle.Length));

            // write subtitle, if given
            if (strSubtitle.Length <= 0) return;

            Console.SetCursorPosition(10, 9);
            Console.Write("Theme: " + strSubtitle);
        }

        /// <summary>
        /// Delete content of console and write title to console.
        /// </summary>
        /// <param name="strTitle">Title to show</param>
        public static void WriteTitle(string strTitle)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine(strTitle);
            Console.WriteLine(new string('-', strTitle.Length));
            Console.WriteLine();
        }


        public static void ShowConfigurationSubMenu()
        {
            ConsoleKeyInfo objKeyInfo;
            bool bContinue = true;

            // endless loop for menu
            while (bContinue)
            {
                try
                {
                    // clear console
                    ShowTitleInteractive("Strompi3 - Management Console  (Core 3.1)", "Set Configuration");

                    // show menu
                    Console.SetCursorPosition(10, 12);
                    Console.Write("Select:");
                    Console.SetCursorPosition(10, 14);
                    Console.Write("  1  -> Set Power Priority");
                    Console.SetCursorPosition(10, 15);
                    Console.Write("  2  -> Set Shutdown-Enabler, -Timer and Shutdown-battery-level");
                    Console.SetCursorPosition(10, 16);
                    Console.Write("  3  -> Set Serialless-Mode ON/OFF");
                    Console.SetCursorPosition(10, 17);
                    Console.Write("  4  -> Set Power Save Mode");
                    Console.SetCursorPosition(10, 18);
                    Console.Write("  5  -> Set Power-ON-Button-Enabler and -Timer");
                    Console.SetCursorPosition(10, 19);
                    Console.Write("  6  -> Set Power-OFF-Enabler");
                    Console.SetCursorPosition(10, 20);
                    Console.Write("  7  -> Alarm-Enabler");
                    Console.SetCursorPosition(10, 21);
                    Console.Write("  8  -> ");
                    Console.SetCursorPosition(10, 22);
                    Console.Write("  9  -> Complete Configuration (all-in-one)");
                    Console.SetCursorPosition(10, 23);
                    Console.Write("  0  -> i");
                    Console.SetCursorPosition(10, 27);
                    Console.Write("Select (ESC to end): ");

                    // Lesen der betätigten Taste
                    objKeyInfo = Console.ReadKey();

                    // Auswertung der Taste
                    switch (objKeyInfo.Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            SetPowerPriority();
                            break;
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            SetShutdownSettings();
                            break;
                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            SetSerialLessEnable();
                            break;
                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                            SetPowerSaveMode();
                            break;
                        case ConsoleKey.D5:
                        case ConsoleKey.NumPad5:
                            SetPowerONButtonEnablerAndTimer();
                            break;
                        case ConsoleKey.D6:
                        case ConsoleKey.NumPad6:
                            //WaitIRQforPowerFailure();
                            break;
                        case ConsoleKey.D7:
                        case ConsoleKey.NumPad7:
                        case ConsoleKey.D8:
                        case ConsoleKey.NumPad8:
                            //SetSerialLessMode();
                            break;
                        case ConsoleKey.D9:
                        case ConsoleKey.NumPad9:
                            //ReSetToSerialMode();
                            break;
                        case ConsoleKey.D0:
                        case ConsoleKey.NumPad0:
                            //TransferConfigurationToStrompi3();
                            break;
                        case ConsoleKey.Escape:
                            bContinue = false;
                            continue;
                        default:
                            continue;
                    }

                    // wait for next key
                    ReadKey();
                }
                catch (Exception exc)
                {
                    // error in method
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine(exc.Message);

                    // read next key
                    Console.WriteLine();
                    ReadKey();
                }
            }

        }

        private static void SetPowerONButtonEnablerAndTimer()
        { 
            ShowTitleInteractive("Strompi3", "Set Power-ON-Button Enabler & Timer:");
            Console.WriteLine();
            Console.WriteLine();

            using (var ups = new StromPi3(true))
            {
                ups.GetSettings();
                StromPi3ConfigEditor.EditPowerOnButtonEnabler(ups);
                Console.WriteLine($"Power-ON-Button Enable = {ups.Settings.StartStopSettings.PowerOnButtonEnable}, Timer = {ups.Settings.StartStopSettings.PowerOnButtonSeconds} secs");
            }
        }

        private static void SetPowerSaveMode()
        {
            ShowTitleInteractive("Strompi3", "Set Power-Save Mode:");
            Console.WriteLine();
            Console.WriteLine();

            using (var ups = new StromPi3(true))
            {
                ups.GetSettings();
                StromPi3ConfigEditor.EditPowerSaveMode(ups);
                Console.WriteLine($"Set Power-Save Mode to {ups.Settings.StartStopSettings.PowersaveEnable.ToNumber()}) = {ConverterHelper.EnabledDisabledConverter(ups.Settings.StartStopSettings.PowersaveEnable.ToNumber().ToString(), "PowerSaveMode")}");
            }
        }

        #endregion


        #region Settings Configuration

        private static void SetPowerPriority()
        {
            ShowTitleInteractive("Strompi3", "Set Power Priority:");
            Console.WriteLine();
            Console.WriteLine();

            using (var ups = new StromPi3(true))
            {
                ups.GetSettings();
                StromPi3ConfigEditor.EditInputPriorityMode(ups);
                Console.WriteLine($"Set Power Priority to {(int)ups.Settings.PriorityMode}) = {ConverterHelper.GetEnumDescription(ups.Settings.PriorityMode)}");
            }
        }

        /// <summary>
        /// Set Shutdown-Enabler, -Timer and Shutdown-battery-level
        /// </summary>
        private static void SetShutdownSettings()
        {
            ShowTitleInteractive("Strompi3", "Set Shutdown Settings:");
            Console.WriteLine();
            Console.WriteLine();

            using (var ups = new StromPi3(true))
            {
                ups.GetSettings();
                StromPi3ConfigEditor.EditShutdownEnabler(ups);
                Console.WriteLine($"Shutdown Enable = {ups.Settings.ShutdownEnable}, Timer = {ups.Settings.ShutdownSeconds} secs");
            }
        }

        /// <summary>
        /// Set SerialLess-Mode (requires serial port
        /// </summary>
        private static void SetSerialLessEnable()
        {
            ShowTitleInteractive("Strompi3", "Set Serial-Less Mode:");
            Console.WriteLine();
            Console.WriteLine();

            using (var ups = new StromPi3(true))
            {
                ups.GetSettings();
                StromPi3ConfigEditor.EditSeriallessEnable(ups);
                Console.WriteLine($"Serial Less Enable = {ups.Settings.SerialLessEnable}");
            }
        }

        private static void SetDateTime()
        {
            ShowTitleInteractive("Strompi3", "Syncronize Date and time:");
            Console.WriteLine();
            Console.WriteLine();
            using (var ups = new StromPi3(true))
            {
                ups.SyncRTC();
            }
        }

        #endregion





        public static bool ReadPorts()
        {
            ShowTitleInteractive("Strompi3", "Read available ports:");
            Console.WriteLine();
            Console.WriteLine();

            return StromPi3.ShowAvailableSerialPorts("tty");
        }


        private static void GetConfiguration()
        {
            ShowTitleInteractive("Strompi3", "Get Configuration:");
            Console.WriteLine();
            Console.WriteLine();
            using (var ups = new StromPi3(true))
            {
                Console.WriteLine(ups.GetSettings());
            }
        }


        private static void WaitPollingforPowerFailure()
        {
            ShowTitleInteractive("Strompi3", "Wait Polling for Power Failure:");
            Console.WriteLine();
            Console.WriteLine();
            using (var ups = new StromPi3(true))
            {
                ups.PollAndWaitForPowerFailureToShutDown(60);
            }
        }

        private static void WaitIRQforPowerFailure()
        {
            ShowTitleInteractive("Strompi3", "WaitIRQforPowerFailure:");
            Console.WriteLine();
            Console.WriteLine();
            using (var ups = new StromPi3())
            {
                ups.WaitForPowerFailureIrqToShutDown();
            }
        }

        private static void ShutDownRaspi()
        {
            Console.WriteLine($"Raspberry Pi: running shutdown...");
            ShowTitleInteractive("Strompi3", "Raspberry Pi: running shutdown");
            Console.WriteLine();
            Console.WriteLine();
            Thread.Sleep(2000);
            Os.ShutDown();
        }

    }
}

