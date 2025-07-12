using System;
using System.Threading.Tasks;
using Pi.Common;
using Pi.Common.utils;
using Strompi3Lib;
using Strompi3Lib.Common;

namespace Strompi3Console;

public class Program
{

    public static string MenuTitle = "Strompi3 - Management Console (.NET 8) V 0.5";
    public static string SubTitle = "Strompi3";


    #region Menu-control
    /// <summary>
    /// Main menu
    /// </summary>
    static async Task Main(string[] args)
    {

        bool waitForDebugger = true;

        if (waitForDebugger)
            AttachDebuggerHelper.WaitForAttachedDebugger(60);

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
                Console.Write("  2  -> Get Strompi3 Configuration");
                Console.SetCursorPosition(10, 16);
                Console.Write("  3  -> Set Strompi3 Configuration");
                Console.SetCursorPosition(10, 17);
                Console.Write("  4  -> Sync RTC of StromPi3 with Raspberry");
                Console.SetCursorPosition(10, 18);
                Console.Write("  5  -> Monitor PowerChangedEvent (SERIAL)");
                Console.SetCursorPosition(10, 19);
                Console.Write("  6  -> Do SelfCheck with expected Status");
                Console.SetCursorPosition(10, 20);
                Console.Write("  7  -> Get Status and Monitor Power Events (SERIAL)");
                Console.SetCursorPosition(10, 21);

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
                        Os.IsSerialConsoleDeactivatedAndSerialPortActive();
                        Os.ShowAvailableSerialPorts("tty");
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        ShowTitleInteractive(SubTitle, "Get Strompi3 Configuration:");
                        Console.WriteLine();
                        Console.WriteLine();
                        StromPi3Manager.GetStatus();

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
                        StromPi3Manager.SynchronizeAndSendRtc();

                        break;

                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        ShowTitleInteractive(SubTitle, "Monitor PowerChangedEvent (SERIAL)");
                        Console.WriteLine();
                        Console.WriteLine();
                        StromPi3Manager.MonitorPowerChangeEvents();

                        break;

                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        ShowTitleInteractive(SubTitle, "Do SelfCheck with expected Status");
                        Console.WriteLine();
                        Console.WriteLine();

                        StromPi3Manager.GetStatusAndCompare(StromPi3.CreateExpectedConfigForBe102Manually());
                        break;

                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                        ShowTitleInteractive(SubTitle, "Get Status and Monitor Power Events (SERIAL)");
                        Console.WriteLine();
                        Console.WriteLine();
                        var status = await StromPi3Manager.GetStatusAndMonitorPowerEventsAsync();
                        Console.WriteLine(status.ToString());
                        break;


                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        ShowTitleInteractive(SubTitle, "Raspberry Pi: running shutdown");
                        Console.WriteLine();
                        Console.WriteLine();
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
                ShowTitleInteractive(MenuTitle, "Strompi3 Configuration Menu");

                // show menu
                Console.SetCursorPosition(10, 12);
                Console.Write("Select:");
                Console.SetCursorPosition(10, 14);
                Console.Write("  1  -> Edit and Send complete Strompi3 Configuration)");
                Console.SetCursorPosition(10, 15);
                Console.Write("  2  -> Send Configuration to Strompi3");
                Console.SetCursorPosition(10, 16);
                Console.Write("  3  -> TODO: Edit Power Priority ");
                Console.SetCursorPosition(10, 17);
                Console.Write("  4  -> TODO: Edit Shutdown-Enable, -Timer and Shutdown-battery-level ");
                Console.SetCursorPosition(10, 18);
                Console.Write("  5  -> TODO: Edit Power Save Mode");
                Console.SetCursorPosition(10, 19);
                Console.Write("  6  -> TODO: Edit Alarm-Enable");
                Console.SetCursorPosition(10, 20);
                Console.Write("  7  -> ...");
                Console.SetCursorPosition(10, 21);
                Console.Write("  8  -> ...");
                Console.SetCursorPosition(10, 22);
                Console.Write("  9  -> TODO: Set Power-ON-Button-Enable and -Timer");
                Console.SetCursorPosition(10, 23);
                Console.Write("  0  -> TODO: Set Serialless-Mode ON/OFF");
                Console.SetCursorPosition(10, 27);
                Console.Write("Select (ESC to end): ");

                // Lesen der betätigten Taste
                objKeyInfo = Console.ReadKey();

                // Auswertung der Taste
                switch (objKeyInfo.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        ShowTitleInteractive(SubTitle, "Edit and send Strompi3 complete configuration:");
                        Console.WriteLine();
                        Console.WriteLine();
                        StromPi3Manager.UpdateAndSendConfiguration();

                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        ShowTitleInteractive(SubTitle, "Send Configuration to Strompi3:");
                        Console.WriteLine();
                        Console.WriteLine();
                        StromPi3Manager.SendConfiguration();
                        break;

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        ShowTitleInteractive(SubTitle, "TODO: Edit and send Power Priority:");
                        Console.WriteLine();
                        Console.WriteLine();
                        //StromPi3Manager.Strompi3Client.UpdateInputPriorityMode(true);
                        break;

                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        ShowTitleInteractive(SubTitle, "TODO: Edit and sendShutdown-Enable, -Timer and Shutdown-battery-level:");
                        Console.WriteLine();
                        Console.WriteLine();
                        //StromPi3Manager.Strompi3Client.UpdateShutdownModeAndTimer(true);
                        break;

                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        ShowTitleInteractive(SubTitle, "TODO: Edit and send Power Save Mode:");
                        Console.WriteLine();
                        Console.WriteLine();
                        // StromPi3Manager.Strompi3Client.UpdatePowerSaveMode(true);
                        break;

                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        ShowTitleInteractive(SubTitle, "TODO: Edit and send Alarm-Enable:");
                        Console.WriteLine();
                        Console.WriteLine();
                        // StromPi3Manager.Strompi3Client.UpdateAlarmConfiguration(true);
                        break;

                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                        ShowTitleInteractive(SubTitle, " TBD:");
                        Console.WriteLine();
                        Console.WriteLine();
                        //StromPi3Manager.
                        break;

                    case ConsoleKey.D8:
                    case ConsoleKey.NumPad8:
                        ShowTitleInteractive(SubTitle, " TBD:");
                        Console.WriteLine();
                        Console.WriteLine();
                        //StromPi3Manager.
                        break;
                    case ConsoleKey.D9:
                    case ConsoleKey.NumPad9:
                        ShowTitleInteractive(SubTitle, "TODO: Edit and send Power-ON-Button-Enable and -Timer:");
                        Console.WriteLine();
                        Console.WriteLine();
                        //StromPi3Manager.Strompi3Client.UpdatePowerOnButton(true);
                        break;

                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        ShowTitleInteractive(SubTitle, "TODO: Edit and send Serialless-Mode ON/OFF:");
                        Console.WriteLine();
                        Console.WriteLine();
                        //StromPi3Manager.Strompi3Client.UpdateSerialless(true);
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