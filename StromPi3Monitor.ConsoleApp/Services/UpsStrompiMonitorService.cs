using System;
using System.Threading;
using System.Threading.Tasks;
using StromPi3Monitor.ConsoleApp.HardwareAbstraction;

namespace StromPi3Monitor.ConsoleApp.Services;

/// <summary>
/// Service to monitor Strompi3 UPS
/// </summary>
public class UpsStrompiMonitorService:IUpsStrompiMonitor
{
    
    public event EventHandler? PowerFailure;


    public async Task MonitorUpsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var powerOk = await CheckPowerStatusAsync();
            if (!powerOk)
            {
                // Reagiere auf einen Stromausfall
                // (z.B. Loggen, Benachrichtigung, etc.)
            }
            await Task.Delay(5000); // Überprüfung alle 5 Sekunden
        }
    }


    public async Task<bool> CheckPowerStatusAsync()
    {
        return ReturnPowerDummy();
    }

    private bool ReturnPowerDummy()
    {
        Console.WriteLine("Strompi3 power is ok");
        return true;
    }
}