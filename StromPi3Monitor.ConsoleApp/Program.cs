using System;
using System.Device.Gpio;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pi.Common.utils;
using Strompi3Lib.Common;
using StromPi3Monitor.ConsoleApp.HardwareAbstraction;
using StromPi3Monitor.ConsoleApp.Services;

namespace StromPi3Monitor.ConsoleApp;

internal class Program
{
    public static async Task Main(string[] args)
    {

        Console.WriteLine("Starting up Pi.LurchiCam");
        Console.WriteLine(
            "This console-app uses an ir-camera with an Ir-LED-ring, triggered by a signal of a lightbarrier.");

        bool waitForDebugger = false;

        if (waitForDebugger)
            AttachDebuggerHelper.WaitForAttachedDebugger(60);

        // Create the host and configure DI.
        // The Generic host monitors abort-signals from the operations system, from CTRL+C for console apps
        // and submits this as a CancellationToken to the inner services
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Register your hardware implementations here.
                services.AddSingleton<GpioController>(provider => new GpioController(PinNumberingScheme.Board));
                services.AddSingleton<IUpsStrompiMonitor, UpsStrompiMonitorService>();
                // Register the inner FotofallenService and the hosted service.
                services.AddSingleton<LurchiCamService>();
                services.AddHostedService<LurchiCamHostedService>();
            })
            .Build();

        await host.RunAsync();
    }
}