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

    public Task<string> StartAsync(string projectDirectory, string host, int port, string @base)
        => _environment.InvokeAsync(_bridge, "startVite",
            JsonSerializer.Serialize(new { projectDirectory, host, port, @base }));

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _environment.InvokeAsync(_bridge, "closeVite");
        }
        finally
        {
            _environment.Dispose();
        }
    }
}
