using System;
using System.Diagnostics;
using System.IO;

namespace Strompi3Lib
{
    /// <summary>
    /// enables direct os calls
    /// </summary>
    public static class Os
    {
                /// <summary>
        /// prepares os call setting the system date, see https://linux.die.net/man/1/date
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static void SetDateTime(DateTime dateTime)
        {
            //os-call: sudo date -s '2014-12-25 12:34:56'
            string exe = $"date";
            string arguments = $" -s {dateTime.Year}-{dateTime.Month}-{dateTime.Day}" +
                               $" {dateTime.Hour}:{dateTime.Minute}:{dateTime.Second}";

            OsCall(exe,arguments);
        }


        /// <summary>
        /// wraps os call 'sudo shutdown -h now'
        /// </summary>
        public static void ShutDown()
        {
            string exe = $"shutdown";
            string arguments = $"-h now";
            OsCall(exe,arguments);
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

            if (String.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine($"exit-code: {result}");
            }
            else
            {
                Console.WriteLine($"exit-code: {result}, error: {error}");
            }

            
        }
    }
}