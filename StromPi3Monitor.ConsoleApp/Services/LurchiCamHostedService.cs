using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace StromPi3Monitor.ConsoleApp.Services;

/// <summary>
/// implementation (service) of a generic .NET host
/// </summary>
public class LurchiCamHostedService: IHostedService
{
    private readonly LurchiCamService _lurchiCamService;
 
    /// <summary>
    /// linked cts, to cover external (from the host) and internal cancel-signals,
    /// to  synchronize the lifetime (and cancel) the connected tasks 
    /// </summary>
    private CancellationTokenSource? _cts;

    public LurchiCamHostedService(LurchiCamService lurchiCamService)
    {
        _lurchiCamService = lurchiCamService;
    }

    /// <summary>
    /// inits the inner LurchiCamservice
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Task.Run(() => _lurchiCamService.RunAsync(_cts.Token), cancellationToken);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }
}