using System.Diagnostics;

namespace Strompi3Lib.Common;

public class ProcessRunner
{
    private ProcessStartInfo _processStartInfo;

    public bool IsBusy { get; private set; }

    public string ErrorMsg { get; private set; }

    public ProcessRunner(ProcessStartInfo processStartInfo)
    {
        _processStartInfo = processStartInfo;
    }


    public int Run()
    {
        IsBusy = true;

        var process = new Process();
        process.StartInfo = _processStartInfo;

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        ErrorMsg = process.StandardError.ReadToEnd();

        process.WaitForExit();

        IsBusy = false;

        return process.ExitCode;
    }
}