using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Strompi3Lib;
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
            var status = await StromPi3Manager.GetStatusAndMonitorPowerEventsAsync();

            Console.WriteLine($"Strompi3 has status = {Environment.NewLine} {status}");

            await Task.Delay(5000); // Überprüfung alle 5 Sekunden
        }
    }
}