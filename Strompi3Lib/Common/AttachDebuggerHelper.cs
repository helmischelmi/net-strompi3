using System;
using System.Diagnostics;

namespace Strompi3Lib.Common;

/// <summary>
/// enables remote debugging on raspi
/// </summary>
public static class AttachDebuggerHelper
{
    /// <summary>
    /// waits until an external debugger is attached
    /// <para>usage: insert this at very beginning of code application</para>
    /// <para>so the app waits until a debugger is attached</para>
    /// </summary>
    public static void WaitForAttachedDebugger(int maxWaitTimeSeconds = 30)
    {
        int waitSeconds = maxWaitTimeSeconds;
        int sleepSeconds = 3;

        int cycles = maxWaitTimeSeconds / sleepSeconds;
        int i = 0;

        while(!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape) && i++ < cycles)
        {
            Console.WriteLine("waiting for debugger attach");
            Console.WriteLine("Press ESC to continue without debugger");
            if (Debugger.IsAttached) break;
            System.Threading.Thread.Sleep(sleepSeconds * 1000);
        }
        Console.WriteLine("Yay! debugger attached! ʕ•ᴥ•ʔ");

    }
}