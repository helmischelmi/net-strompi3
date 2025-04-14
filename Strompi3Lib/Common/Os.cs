using System;
using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;

namespace Strompi3Lib.Common;

/// <summary>
/// enables direct os calls
/// </summary>
public static class Os
{
    /// <summary>
    /// prepares os call setting the system date, see https://linux.die.net/man/1/date
    /// needs admin rights (sudo), e.g. 'sudo date -s 2014-12-25 +12:34:56'
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static void SetSystemDateTime(DateTime dateTime)
    {
        string command = "date"; 
        string arguments = $"-s {dateTime:yyyy-MM-dd +HH:mm:ss}";

        OsCall(command, arguments);
    }

    /// <summary>
    /// To use the Strompi3 as a UPS, the serial console must be deactivated and the serial port must be activated (use raspi-config).
    /// dtoverlay=miniuart-bt ensures that primary UART (ttyAMA0 or serial0) is available for Strompi3 and not used by for bluetooth (moved to mini-UART (ttyS0)).
    /// The methods checks if the UART is enabled and the serial console is deactivated by reading configuration files
    /// on the system.
    /// </summary>
    /// <returns></returns>
    public static bool IsSerialConsoleDeactivatedAndSerialPortActive(bool bSilent = false)
    {
        try
        {
            // Checks, if enable_uart=1 in /boot/config.txt is set
            string configTxtPath = "/boot/firmware/config.txt";  // rasbian bookworm: changed the path
            bool uartEnabled = File.Exists(configTxtPath) &&
                               File.ReadAllLines(configTxtPath)
                                   .Any(line => line.Trim().StartsWith("enable_uart=1"));

            if (!bSilent) Console.WriteLine($" - Enabling UART is: {uartEnabled}");

            bool uartOverlayEnabled = File.Exists(configTxtPath) &&
                               File.ReadAllLines(configTxtPath)
                                   .Any(line => line.Trim().StartsWith("dtoverlay=miniuart-bt"));

            if (!bSilent) Console.WriteLine($" - Enabling dtoverlay= miniuart-bt: {uartOverlayEnabled}");

            // Checks, if serial console in /boot/cmdline.txt is active
            string cmdlineTxtPath = "/boot/firmware/cmdline.txt";   // rasbian bookworm: changed the path
            bool serialConsoleActive = File.Exists(cmdlineTxtPath) &&
                                       File.ReadAllText(cmdlineTxtPath).Contains("console=serial");

            if (!bSilent) Console.WriteLine($" - Serial console is active: {serialConsoleActive}");

            var result = uartEnabled && uartOverlayEnabled && !serialConsoleActive;

            if (result == false)
            {
                if (!bSilent)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        "For Strompi3-configuration, the UART must be enabled and the serial console deactivated.");
                    Console.WriteLine(
                        "Use 'sudo raspi-config' interfacing-options to adopt serial-configuration and reboot");
                    Console.ForegroundColor = ConsoleColor.Green;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// Shows list of serial port names.
    /// </summary>
    /// <returns>TRUE, if serial port is available</returns>
    public static bool ShowAvailableSerialPorts(string strRequiredPorts, bool bSilent = false)
    {
        string[] ports = SerialPort.GetPortNames();
        if (!bSilent)
        {
            Console.WriteLine();
            Console.WriteLine("The following serial ports were found:");
        }

        // Display each port name to the console.
        foreach (string port in ports)
        {
            if (!bSilent) Console.WriteLine($"Serial name: {port}");
            var isTTY = port.Contains(strRequiredPorts);
            if (isTTY) continue;

            if (!bSilent) Console.WriteLine($"No {strRequiredPorts}.. serial port found!{Environment.NewLine}");
            return false;
        }

        if (!bSilent) Console.WriteLine($"Yes, we have the serial port(s) available.{Environment.NewLine}");

        return true;
    }


    /// <summary>
    /// wraps os call 'sudo shutdown -h now'
    /// needs admin rights (sudo)
    /// </summary>
    public static void ShutDown()
    {
        string command = $"shutdown";
        string arguments = $"-h now";
        OsCall(command, arguments);
    }


    /// <summary>
    /// wraps os call as external process call
    /// </summary>
    private static void OsCall(string exe, string arguments)
    {
        Console.WriteLine($"Run os-call: {exe} {arguments}");

        var sw = new Stopwatch();
        var process = new Process();

        sw.Start();
        var start = new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Arguments = arguments,
        };

        process = Process.Start(start);
        process.WaitForExit();
        sw.Stop();

        string error, result;

        // read std output and std-error 
        using (StreamReader sr = process.StandardOutput)
        {
            error = process.StandardError.ReadToEnd();
            result = sr.ReadToEnd();
        }

        if (string.IsNullOrWhiteSpace(error))
        {
            Console.WriteLine($"exit-code: {result}");
        }
        else
        {
            Console.WriteLine($"exit-code: {result}, error: {error}");
        }


    }
}