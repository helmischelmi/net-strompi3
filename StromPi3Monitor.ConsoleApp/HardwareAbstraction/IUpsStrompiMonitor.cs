using System;
using System.Threading;
using System.Threading.Tasks;

namespace StromPi3Monitor.ConsoleApp.HardwareAbstraction;

public interface IUpsStrompiMonitor
{
    public Task MonitorUpsAsync(CancellationToken token);
}