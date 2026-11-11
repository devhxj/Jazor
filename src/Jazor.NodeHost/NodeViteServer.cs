using System.Text.Json;

namespace Jazor.NodeHost;

internal sealed class NodeViteServer : IAsyncDisposable
{
    private readonly NodeEnvironment _environment;
    private readonly string _bridge;

    public NodeViteServer(string projectDirectory)
    {
        _environment = new NodeEnvironment(projectDirectory);
        _bridge = NodeToolchain.MaterializeBridge(projectDirectory);
    }

    public Task<string> StartAsync(string projectDirectory, string host, int port, string @base,
        CancellationToken cancellationToken = default)
        => _environment.InvokeAsync(_bridge, "startVite", cancellationToken,
            JsonSerializer.Serialize(new { projectDirectory, host, port, @base }));

    public ValueTask DisposeAsync() => StopAsync(CancellationToken.None);

    public async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!_environment.IsStopped)
                await _environment.InvokeAsync(_bridge, "closeVite", cancellationToken);
        }
        finally
        {
            _environment.Dispose();
        }
    }
}
