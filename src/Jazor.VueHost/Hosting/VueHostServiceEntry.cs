namespace Jazor.VueHost.Hosting;

public sealed class VueHostServiceEntry
{
    private readonly IVueHostService _hostService;

    public VueHostServiceEntry(IVueHostService hostService)
    {
        _hostService = hostService ?? throw new ArgumentNullException(nameof(hostService));
    }

    public ValueTask RunAsync(CancellationToken cancellationToken)
        => _hostService.StartAsync(cancellationToken);
}
