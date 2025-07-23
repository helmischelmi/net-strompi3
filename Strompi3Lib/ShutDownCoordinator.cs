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
    Task OnShutdownRequestedAsync();
    CancellationToken ShutdownToken { get; }
}

public class ShutdownCoordinator : IShutdownCoordinator
{
    private readonly List<IStoppableService> _services = new();
    private readonly CancellationTokenSource _shutdownTokenSource = new();

    public CancellationToken ShutdownToken => _shutdownTokenSource.Token;

    public void RegisterService(IStoppableService service)
    {
        _services.Add(service);
    }

    public async Task OnShutdownRequestedAsync()
    {
        Console.WriteLine("Shutdown signal received. Cancelling tasks...");

        var tasks = _services.Select(svc => svc.StopAsync(_shutdownTokenSource.Token));
        await Task.WhenAll(tasks);

        Console.WriteLine("All services stopped. Shutting down Raspberry Pi...");
       
        await Task.Delay(2000).WaitAsync(ShutdownToken);

        Os.ShutDown();
    }


}