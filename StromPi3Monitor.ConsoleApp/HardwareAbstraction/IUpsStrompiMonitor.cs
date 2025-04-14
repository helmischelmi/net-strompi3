using System;
using System.Threading;
using System.Threading.Tasks;

namespace StromPi3Monitor.ConsoleApp.HardwareAbstraction;

public interface IUpsStrompiMonitor
{
    event EventHandler PowerFailure;

    Task MonitorUpsAsync(CancellationToken token);

    Task<bool> CheckPowerStatusAsync();
}