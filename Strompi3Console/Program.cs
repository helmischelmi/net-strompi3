using System;
using System.Threading;
using Strompi3Lib;

namespace Strompi3Console
{
    public class Program
    {

        public static string MenuTitle = "Strompi3 - Management Console (.NET 8) V 0.1";
        public static string SubTitle = "Strompi3";

        public static StromPi3 configuredUps;

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
                    ShowTitleInteractive(MenuTitle, "Main Menu ");

                    // show menu
                    Console.SetCursorPosition(10, 12);
                    Console.Write("Select:");
                    Console.SetCursorPosition(10, 14);
                    Console.Write("  1  -> Show serial ports on Raspberry");
                    Console.SetCursorPosition(10, 15);
                    Console.Write("  2  -> Get Configuration");
                    Console.SetCursorPosition(10, 16);
                    Console.Write("  3  -> Set Configuration");
                    Console.SetCursorPosition(10, 17);
                    Console.Write("  4  -> Sync RTC with Raspberry");
                    Console.SetCursorPosition(10, 18);
                    Console.Write("  5  -> TEST: Wait Polling for PowerFailure (SERIAL)");
                    Console.SetCursorPosition(10, 19);
                    Console.Write("  6  -> TEST: Wait IRQ for PowerFailure (SERIALLESS)");
                    Console.SetCursorPosition(10, 20);

                    Console.SetCursorPosition(10, 22);
                    Console.Write("  0  -> Shutdown Raspberry PI");
                    Console.SetCursorPosition(10, 24);
                    Console.Write("Select (ESC to end): ");

                    // Lesen der betätigten Taste
                    objKeyInfo = Console.ReadKey();

                    // Auswertung der Taste
                    switch (objKeyInfo.Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            ShowTitleInteractive(SubTitle, "Read available ports:");
                            Console.WriteLine();
                            Console.WriteLine();
                            Os.ShowAvailableSerialPorts("tty");
                            break;

                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            ShowTitleInteractive(SubTitle, "Get Configuration:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.InitializeClient();
                            using (var ups = ApiHelper.Strompi3Client)
                            {
                                Console.WriteLine(ups.Settings);
                            }
                            break;

                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            ShowConfigurationSubMenu();
                            break;
                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                            ShowTitleInteractive(SubTitle, "Synchronize Date and Time:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.InitializeClient();
                            using (var ups = ApiHelper.Strompi3Client)
                            {
                                ups.SyncRTC();
                            }
                            break;

                        case ConsoleKey.D5:
                        case ConsoleKey.NumPad5:
                            ShowTitleInteractive(SubTitle, "Wait - Polling for Power Failure:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.InitializeClient();
                            using (var ups = ApiHelper.Strompi3Client)
                            {
                                ups.Monitor.Poll();
                            }
                            break;

                        case ConsoleKey.D6:
                        case ConsoleKey.NumPad6:
                            ShowTitleInteractive(SubTitle, "WaitIRQforPowerFailure:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.InitializeClient();
                            using (var ups = ApiHelper.Strompi3Client)
                            {
                                Console.WriteLine("TODO: not implemented");
                                //ups.Monitor.PowerFailureByIRQ();
                            }
                            break;

                        case ConsoleKey.D0:
                        case ConsoleKey.NumPad0:
                            ShowTitleInteractive(SubTitle, "Raspberry Pi: running shutdown");
                            Console.WriteLine();
                            Console.WriteLine();
                            Thread.Sleep(2000);
                            Os.ShutDown();
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
                    ShowTitleInteractive(MenuTitle, "Configuration Menu");

                    // show menu
                    Console.SetCursorPosition(10, 12);
                    Console.Write("Select:");
                    Console.SetCursorPosition(10, 14);
                    Console.Write("  1  -> Modify Configuration (all-in-one) ");
                    Console.SetCursorPosition(10, 15);
                    Console.Write("  2  -> Transfer Configuration to Strompi3");
                    Console.SetCursorPosition(10, 16);
                    Console.Write("  3  -> Set Power Priority ");
                    Console.SetCursorPosition(10, 17);
                    Console.Write("  4  -> Set Shutdown-Enable, -Timer and Shutdown-battery-level ");
                    Console.SetCursorPosition(10, 18);
                    Console.Write("  5  -> Set Power Save Mode");
                    Console.SetCursorPosition(10, 19);
                    Console.Write("  6  -> Alarm-Enable");
                    Console.SetCursorPosition(10, 20);
                    Console.Write("  7  -> ");
                    Console.SetCursorPosition(10, 21);
                    Console.Write("  8  -> ");
                    Console.SetCursorPosition(10, 22);
                    Console.Write("  9  -> MOD: Set Power-ON-Button-Enable and -Timer");
                    Console.SetCursorPosition(10, 23);
                    Console.Write("  0  -> MOD: Set Serialless-Mode ON/OFF");
                    Console.SetCursorPosition(10, 27);
                    Console.Write("Select (ESC to end): ");

                    // Lesen der betätigten Taste
                    objKeyInfo = Console.ReadKey();

                    // Auswertung der Taste
                    switch (objKeyInfo.Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            ShowTitleInteractive(SubTitle, "Modify Configuration (all-in-one):");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.EditCompleteConfiguration();
                            break;

                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            ShowTitleInteractive(SubTitle, "Transfer Configuration to Strompi3:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.SendConfiguration(configuredUps);
                            break;

                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            ShowTitleInteractive(SubTitle, "Set Power Priority:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.EditInputPriorityMode();
                            break;

                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                            ShowTitleInteractive(SubTitle, "Set Shutdown-Enable, -Timer and Shutdown-battery-level:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.EditShutdownMode();
                            break;

                        case ConsoleKey.D5:
                        case ConsoleKey.NumPad5:
                            ShowTitleInteractive(SubTitle, "Set Power Save Mode:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.EditPowerSaveMode();
                            break;

                        case ConsoleKey.D6:
                        case ConsoleKey.NumPad6:
                            ShowTitleInteractive(SubTitle, "Alarm-Enable:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.EditAlarmConfiguration();
                            break;

                        case ConsoleKey.D7:
                        case ConsoleKey.NumPad7:
                            ShowTitleInteractive(SubTitle, "TBD:");

                            break;

                        case ConsoleKey.D8:
                        case ConsoleKey.NumPad8:
                            ShowTitleInteractive(SubTitle, " TBD:");
                            break;
                        case ConsoleKey.D9:
                        case ConsoleKey.NumPad9:
                            ShowTitleInteractive(SubTitle, "MOD: Set Power-ON-Button-Enable and -Timer:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.EditPowerOnButton();
                            break;

                        case ConsoleKey.D0:
                        case ConsoleKey.NumPad0:
                            ShowTitleInteractive(SubTitle, "MOD: Set Serialless-Mode ON/OFF:");
                            Console.WriteLine();
                            Console.WriteLine();
                            ApiHelper.EditSerialless();
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

        #endregion
    }
}

