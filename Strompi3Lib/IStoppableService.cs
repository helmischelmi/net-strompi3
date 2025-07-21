using System.Threading;
using System.Threading.Tasks;

namespace Strompi3Lib;

public interface IStoppableService
{
    Task StopAsync(CancellationToken token);
}