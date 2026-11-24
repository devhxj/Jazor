using System.Reflection;
using System.Text.Json;

namespace Jazor.NodeHost;

internal static class NodeToolchain
{
    private static readonly EmbeddedNodeModule Bridge = new(typeof(NodeToolchain), "toolchain.cjs");
    private static readonly string LightningCssWasmVersion = typeof(NodeToolchain).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(attribute => attribute.Key == "LightningCssWasmVersion").Value!;

    public static async Task<string> RestoreAsync(string projectDirectory, string storeDirectory,
        CancellationToken cancellationToken, TextWriter? progress = null, string? engineCacheDirectory = null, bool standaloneTool = false)
    {
        var bootstrap = await PnpmBootstrap.EnsureAsync(engineCacheDirectory ?? storeDirectory, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        using var environment = new NodeEnvironment(projectDirectory, progress);
        var result = await environment.InvokeAsync(MaterializeBridge(projectDirectory), "restore",
            JsonSerializer.Serialize(new { projectDirectory, storeDirectory, bootstrap, standaloneTool,
                engineVersion = PnpmBootstrap.Version, lightningCssWasmVersion = LightningCssWasmVersion }));
        progress?.WriteLine("[Jazor NodeApi] " + result);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    public static string MaterializeBridge(string projectDirectory) => Bridge.Materialize(projectDirectory);

    public static async Task<string> BuildAsync(string projectDirectory, bool ssr,
        CancellationToken cancellationToken, TextWriter? progress = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var environment = new NodeEnvironment(projectDirectory, progress);
        var result = await environment.InvokeAsync(MaterializeBridge(projectDirectory), "build", cancellationToken,
            JsonSerializer.Serialize(new { projectDirectory, ssr }));
        progress?.WriteLine("[Jazor NodeApi] " + result);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    public static async Task<string> CheckAsync(string projectDirectory, IReadOnlyList<string> entryPaths,
        CancellationToken cancellationToken, TextWriter? progress = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var environment = new NodeEnvironment(projectDirectory, progress);
        var result = await environment.InvokeAsync(MaterializeBridge(projectDirectory), "check", cancellationToken,
            JsonSerializer.Serialize(new { projectDirectory, entryPaths }));
        progress?.WriteLine("[Jazor NodeApi] " + result);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }
}
