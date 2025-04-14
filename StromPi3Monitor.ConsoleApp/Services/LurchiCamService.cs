using System;
using System.Threading;
using System.Threading.Tasks;
using StromPi3Monitor.ConsoleApp.HardwareAbstraction;

namespace StromPi3Monitor.ConsoleApp.Services;

/// <summary>
/// class that manages a camera-system triggered by a light-barrier-sensor,
/// capturing photos when the sensor detects a signal.
/// Additionally, it monitors a UPS and communicates with a web-api.
/// </summary>
public class LurchiCamService
{
    private readonly IUpsStrompiMonitor _ups;


    private static readonly SemaphoreSlim _cameraSemaphore = new SemaphoreSlim(1, 1);

    public LurchiCamService( IUpsStrompiMonitor ups)
    {
        _ups = ups;
    }



    /// <summary>
    /// Main method to run the service.
    /// Coordinates multiple asynchronous tasks, that run parallel
    /// to monitor the sensor, the UPS and the communication with the web-api.
    /// all tasks use the same cancellation-token to synchronize their lifetime.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RunAsync(CancellationToken cancellationToken)
    {

        // parallel tasks to monitor the UPS and communicate with the web-api
        var upsTask = _ups.MonitorUpsAsync(cancellationToken);



        // error-messaging on "OnlyOnFaulted"

        upsTask.ContinueWith(task =>
        {
            var aggregate = task.Exception.Flatten();
            foreach (var ex in aggregate.InnerExceptions)
            {
                Console.WriteLine($"UPS-Task Error: {ex.Message}");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);


        // Keep-Alive-Loop of the service:
        // checks every second if the service should be cancelled
        while (!cancellationToken.IsCancellationRequested)
        {
            // Messaging is possible here, e.g. to log the service is still alive
            await Task.Delay(1000);
        }
    }
}