namespace Jazor.VueHost.Hosting;

public interface IVueHostService
{
    ValueTask StartAsync(CancellationToken cancellationToken);

    ValueTask StopAsync(CancellationToken cancellationToken);
}
