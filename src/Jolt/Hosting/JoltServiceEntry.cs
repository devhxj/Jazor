namespace Jolt.Hosting;

public sealed class JoltServiceEntry
{
    private readonly IJoltService _hostService;

    public JoltServiceEntry(IJoltService hostService)
    {
        _hostService = hostService ?? throw new ArgumentNullException(nameof(hostService));
    }

    public ValueTask RunAsync(CancellationToken cancellationToken)
        => _hostService.StartAsync(cancellationToken);
}
