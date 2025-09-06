using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pi.Common;


namespace Strompi3Lib;

public interface IShutdownCoordinator
{
    void RegisterService(IStoppableService service);
    Task OnShutdownRequestedAsync(bool shutDown = true);
    CancellationToken ShutdownToken { get; }
    void SetToken(CancellationToken token);
}

/// <summary>
/// Coordinates the orderly shutdown of registered services and optionally the operating system.
/// Sammelt alle Services, die beim Shutdown gestoppt werden müssen.
/// Signaliert das Beenden aller registrierten Services durch Canceln seines eigenen Tokens.
/// Wird als zentrales Steuerungsinstrument für kontrolliertes Beenden/Shutdown genutzt.
/// </summary>
/// <remarks>This class manages the shutdown process by stopping all registered services and, if specified, 
/// shutting down the operating system. It provides a cancellation token to signal ongoing tasks  about the shutdown
/// process.</remarks>
public class ShutdownCoordinator : IShutdownCoordinator
{
    private readonly List<IStoppableService> _services = new();
    private CancellationToken _shutdownToken;
    public CancellationToken ShutdownToken => _shutdownToken;

    public ShutdownCoordinator() { }

    public void SetToken(CancellationToken token)
    {
        _shutdownToken = token;
    }



    public void RegisterService(IStoppableService service)
    {
        _services.Add(service);
    }


    public async Task OnShutdownRequestedAsync(bool shutDownRaspberry= true)
    {
        Console.WriteLine("Shutdown signal received. Cancelling tasks...");

        var tasks = _services.Select(svc => svc.StopAsync(_shutdownToken));
        await Task.WhenAll(tasks);

        Console.WriteLine("All services stopped.");
       
        await Task.Delay(2000).WaitAsync(_shutdownToken);


        if (shutDownRaspberry)
        {
            Console.WriteLine("Shutting down Raspberry Pi...");
            Os.ShutDown();
        }
        else
        {
            Console.WriteLine("Lurchicam app terminated (no OS shutdown).");
            Environment.Exit(0);  // beendet die App sofort
        }
    }
}